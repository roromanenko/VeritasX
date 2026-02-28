using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using Core.Interfaces;
using Core.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

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

	public byte[] Encrypt(string connection)
	{
		if (string.IsNullOrEmpty(connection))
			throw new ArgumentException("Plaintext cannot be null or empty", nameof(connection));

		byte[]? dek = null;

		try
		{
			dek = GenerateDek();

			var encryptedDek = EncryptDek(dek);

			var connectionBytes = Encoding.UTF8.GetBytes(connection);
			var (ciphertext, iv, tag) = EncryptWithDek(connectionBytes, dek);

			return CombineToBlob(encryptedDek, iv, tag, ciphertext);
		}
		finally
		{
			if (dek != null)
				Array.Clear(dek, 0, dek.Length);
		}
	}

	public string Decrypt(byte[] encryptedConnection)
	{
		if (encryptedConnection == null || encryptedConnection.Length < 89)
			throw new ArgumentException("Invalid encrypted data", nameof(encryptedConnection));

		byte[]? dek = null;

		try
		{
			var (encryptedDek, iv, tag, ciphertext) = ParseBlob(encryptedConnection);

			dek = DecryptDek(encryptedDek);

			var connectionBytes = DecryptWithDek(ciphertext, dek, iv, tag);

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

	private byte[] GenerateDek()
	{
		var dek = new byte[_encryptionOptions.DekSizeBytes];
		RandomNumberGenerator.Fill(dek);
		return dek;
	}

	private byte[] EncryptDek(byte[] dek)
	{
		var iv = new byte[_encryptionOptions.IvSizeBytes];
		RandomNumberGenerator.Fill(iv);

		var ciphertext = new byte[dek.Length];
		var tag = new byte[_encryptionOptions.TagSizeBytes];

		using var aes = new AesGcm(_masterKey, _encryptionOptions.TagSizeBytes);
		aes.Encrypt(iv, dek, ciphertext, tag);

		var result = new byte[iv.Length + ciphertext.Length + tag.Length];
		Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
		Buffer.BlockCopy(ciphertext, 0, result, iv.Length, ciphertext.Length);
		Buffer.BlockCopy(tag, 0, result, iv.Length + ciphertext.Length, tag.Length);

		return result;
	}

	private (byte[] ciphertext, byte[] iv, byte[] tag) EncryptWithDek(byte[] plaintext, byte[] dek)
	{
		var iv = new byte[_encryptionOptions.IvSizeBytes];
		RandomNumberGenerator.Fill(iv);

		var ciphertext = new byte[plaintext.Length];
		var tag = new byte[_encryptionOptions.TagSizeBytes];

		using var aes = new AesGcm(dek, _encryptionOptions.TagSizeBytes);
		aes.Encrypt(iv, plaintext, ciphertext, tag);

		return (ciphertext, iv, tag);
	}

	private static byte[] CombineToBlob(byte[] encryptedDek, byte[] iv, byte[] tag, byte[] ciphertext)
	{
		var blob = new byte[encryptedDek.Length + iv.Length + tag.Length + ciphertext.Length];
		var offset = 0;

		Buffer.BlockCopy(encryptedDek, 0, blob, offset, encryptedDek.Length);
		offset += encryptedDek.Length;

		Buffer.BlockCopy(iv, 0, blob, offset, iv.Length);
		offset += iv.Length;

		Buffer.BlockCopy(tag, 0, blob, offset, tag.Length);
		offset += tag.Length;

		Buffer.BlockCopy(ciphertext, 0, blob, offset, ciphertext.Length);

		return blob;
	}

	private (byte[] encryptedDek, byte[] iv, byte[] tag, byte[] ciphertext) ParseBlob(byte[] blob)
	{
		int encryptedDekSize = _encryptionOptions.IvSizeBytes + _encryptionOptions.DekSizeBytes + _encryptionOptions.TagSizeBytes;
		int minBlobSize = encryptedDekSize + _encryptionOptions.IvSizeBytes + _encryptionOptions.TagSizeBytes;

		if (blob.Length < minBlobSize)
			throw new ArgumentException($"Blob too small. Expected at least {minBlobSize} bytes, got {blob.Length}");

		var offset = 0;

		var encryptedDek = new byte[encryptedDekSize];
		Buffer.BlockCopy(blob, offset, encryptedDek, 0, encryptedDekSize);
		offset += encryptedDekSize;

		var iv = new byte[_encryptionOptions.IvSizeBytes];
		Buffer.BlockCopy(blob, offset, iv, 0, _encryptionOptions.IvSizeBytes);
		offset += _encryptionOptions.IvSizeBytes;

		var tag = new byte[_encryptionOptions.TagSizeBytes];
		Buffer.BlockCopy(blob, offset, tag, 0, _encryptionOptions.TagSizeBytes);
		offset += _encryptionOptions.TagSizeBytes;

		var ciphertext = new byte[blob.Length - offset];
		Buffer.BlockCopy(blob, offset, ciphertext, 0, ciphertext.Length);

		return (encryptedDek, iv, tag, ciphertext);
	}

	private byte[] DecryptDek(byte[] encryptedDek)
	{
		if (encryptedDek.Length != 60)
			throw new ArgumentException("Invalid encrypted DEK size");

		var iv = new byte[_encryptionOptions.IvSizeBytes];
		var ciphertext = new byte[_encryptionOptions.DekSizeBytes];
		var tag = new byte[_encryptionOptions.TagSizeBytes];

		Buffer.BlockCopy(encryptedDek, 0, iv, 0, _encryptionOptions.IvSizeBytes);
		Buffer.BlockCopy(encryptedDek, _encryptionOptions.IvSizeBytes, ciphertext, 0, _encryptionOptions.DekSizeBytes);
		Buffer.BlockCopy(encryptedDek, _encryptionOptions.IvSizeBytes + _encryptionOptions.DekSizeBytes, tag, 0, _encryptionOptions.TagSizeBytes);

		var plaintext = new byte[_encryptionOptions.DekSizeBytes];

		using var aes = new AesGcm(_masterKey, _encryptionOptions.TagSizeBytes);
		aes.Decrypt(iv, ciphertext, tag, plaintext);

		return plaintext;
	}

	private byte[] DecryptWithDek(byte[] ciphertext, byte[] dek, byte[] iv, byte[] tag)
	{
		var plaintext = new byte[ciphertext.Length];

		using var aes = new AesGcm(dek, _encryptionOptions.TagSizeBytes);
		aes.Decrypt(iv, ciphertext, tag, plaintext);

		return plaintext;
	}
}
