using System.Security.Cryptography;
using System.Text;
using Core.Interfaces;
using Core.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

/// <summary>
/// Provides envelope encryption for sensitive data using two-layer AES-GCM encryption.
/// Each value is encrypted with a unique Data Encryption Key (DEK), which is itself
/// encrypted with a master key.<br/> The resulting binary blob is safe to store in the database.
/// </summary>
public class EncryptionService : IEncryptionService
{
	private readonly byte[] _masterKey;
	private readonly EncryptionOptions _encryptionOptions;

	public EncryptionService(string masterKeyBase64, IOptions<EncryptionOptions> encryptionOptions)
	{
		try
		{
			_masterKey = Convert.FromBase64String(masterKeyBase64);
			_encryptionOptions = encryptionOptions.Value;

		}
		catch (FormatException ex)
		{
			throw new InvalidOperationException("MASTER_ENCRYPTION_KEY is not valid Base64", ex);
		}
	}

	/// <summary>
	/// Total size in bytes of an encrypted DEK blob (IV + ciphertext + tag).
	/// </summary>
	private int EncryptedDekSize => _encryptionOptions.IvSizeBytes + _encryptionOptions.DekSizeBytes + _encryptionOptions.TagSizeBytes;

	/// <summary>
	/// Minimum valid size of a full encrypted blob (encrypted DEK + IV + tag, no payload).
	/// </summary>
	private int MinBlobSize => EncryptedDekSize + _encryptionOptions.IvSizeBytes + _encryptionOptions.TagSizeBytes;

	/// <summary>
	/// Encrypts a plaintext string using envelope encryption.<br/>
	/// Blob layout: [EncryptedDEK (IV + ciphertext + tag)] [EncryptedData (IV + ciphertext + tag)]
	/// </summary>
	/// <param name="connection"></param>
	/// <returns>Encrypted binary blob ready to be stored.</returns>
	/// <exception cref="ArgumentException"></exception>
	public byte[] Encrypt(string connection)
	{
		if (string.IsNullOrEmpty(connection))
			throw new ArgumentException("Plaintext cannot be null or empty", nameof(connection));

		byte[]? dek = null;

		try
		{
			dek = GenerateDek();

			var encryptedDek = AesGcmEncrypt(_masterKey, dek);
			var encryptedConnection = AesGcmEncrypt(dek, Encoding.UTF8.GetBytes(connection));

			return CombineToBlob(encryptedDek, encryptedConnection);
		}
		finally
		{
			if (dek != null)
				Array.Clear(dek, 0, dek.Length);
		}
	}

	/// <summary>
	/// Decrypts a binary blob produced by <see cref="Encrypt"/>.
	/// </summary>
	/// <param name="encryptedConnection"></param>
	/// <returns>Original plaintext string.</returns>
	/// <exception cref="ArgumentException"></exception>
	/// <exception cref="InvalidOperationException">Thrown if the blob is corrupted or tampered with.</exception>
	public string Decrypt(byte[] encryptedConnection)
	{
		if (encryptedConnection == null || encryptedConnection.Length < MinBlobSize)
			throw new ArgumentException("Invalid encrypted data", nameof(encryptedConnection));

		byte[]? dek = null;

		try
		{
			var (encryptedDek, encryptedData) = ParseBlob(encryptedConnection);

			dek = AesGcmDecrypt(_masterKey, encryptedDek);

			var connectionBytes = AesGcmDecrypt(dek, encryptedData);

			return Encoding.UTF8.GetString(connectionBytes);
		}
		catch (CryptographicException ex)
		{
			throw new InvalidOperationException("Failed to decrypt - data may be corrupted or tampered", ex);
		}
		finally
		{
			if (dek != null)
				Array.Clear(dek, 0, dek.Length);
		}
	}

	/// <summary>
	/// Encrypts plaintext with AES-GCM using the provided key.
	/// Output format: [IV] [ciphertext] [tag]
	/// </summary>
	private byte[] AesGcmEncrypt(byte[] key, byte[] plaintext)
	{
		var iv = new byte[_encryptionOptions.IvSizeBytes];
		RandomNumberGenerator.Fill(iv);

		var ciphertext = new byte[plaintext.Length];
		var tag = new byte[_encryptionOptions.TagSizeBytes];

		using var aes = new AesGcm(key, _encryptionOptions.TagSizeBytes);
		aes.Encrypt(iv, plaintext, ciphertext, tag);

		var result = new byte[iv.Length + ciphertext.Length + tag.Length];
		var offset = 0;

		CopyAndAdvance(iv, result, ref offset);
		CopyAndAdvance(ciphertext, result, ref offset);
		CopyAndAdvance(tag, result, ref offset);

		return result;
	}

	/// <summary>
	/// Decrypts AES-GCM encrypted data produced by <see cref="AesGcmEncrypt"/>.<br/>
	/// Expected format: [IV] [ciphertext] [tag]
	/// </summary>
	private byte[] AesGcmDecrypt(byte[] key, byte[] data)
	{
		var offset = 0;

		var iv = SliceAndAdvance(data, _encryptionOptions.IvSizeBytes, ref offset);
		var ciphertext = SliceAndAdvance(data, data.Length - offset - _encryptionOptions.TagSizeBytes, ref offset);
		var tag = SliceAndAdvance(data, _encryptionOptions.TagSizeBytes, ref offset);

		var plaintext = new byte[ciphertext.Length];

		using var aes = new AesGcm(key, _encryptionOptions.TagSizeBytes);
		aes.Decrypt(iv, ciphertext, tag, plaintext);

		return plaintext;
	}

	/// <summary>
	/// Generates a random Data Encryption Key (DEK).
	/// </summary>
	private byte[] GenerateDek()
	{
		var dek = new byte[_encryptionOptions.DekSizeBytes];
		RandomNumberGenerator.Fill(dek);
		return dek;
	}

	/// <summary>
	/// Combines encrypted DEK and encrypted payload into a single binary blob.
	/// </summary>
	private byte[] CombineToBlob(byte[] encryptedDek, byte[] encryptedData)
	{
		var blob = new byte[encryptedDek.Length + encryptedData.Length];
		var offset = 0;

		CopyAndAdvance(encryptedDek, blob, ref offset);
		CopyAndAdvance(encryptedData, blob, ref offset);

		return blob;
	}

	/// <summary>
	/// Parses a binary blob into its encrypted DEK and encrypted payload parts.
	/// </summary>
	private (byte[] encryptedDek, byte[] encryptedData) ParseBlob(byte[] blob)
	{
		var offset = 0;

		var encryptedDek = SliceAndAdvance(blob, EncryptedDekSize, ref offset);
		var encryptedData = SliceAndAdvance(blob, blob.Length - offset, ref offset);

		return (encryptedDek, encryptedData);
	}

	/// <summary>
	/// Copies bytes from <paramref name="src"/> into <paramref name="dst"/> at the current offset and advances it.
	/// </summary>
	private static void CopyAndAdvance(byte[] src, byte[] dst, ref int offset)
	{
		Buffer.BlockCopy(src, 0, dst, offset, src.Length);
		offset += src.Length;
	}

	/// <summary>
	/// Extracts a slice of <paramref name="length"/> bytes from <paramref name="src"/> at the current offset and advances it.
	/// </summary>
	private static byte[] SliceAndAdvance(byte[] src, int length, ref int offset)
	{
		var result = new byte[length];
		Buffer.BlockCopy(src, offset, result, 0, length);
		offset += length;
		return result;
	}
}
