using System.Security.Cryptography;
using Core.Options;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace Infrastructure.Tests.ServicesTests;

public class EncryptionServiceTests
{
	private static string DefaultMasterKey()
	{
		var key = new byte[32];
		RandomNumberGenerator.Fill(key);
		return Convert.ToBase64String(key);
	}

	private static EncryptionOptions DefaultOptions() => new()
	{
		DekSizeBytes = 32,
		IvSizeBytes = 12,
		TagSizeBytes = 16
	};

	private static EncryptionService CreateSut(string? masterKey = null, EncryptionOptions? options = null)
	{
		return new EncryptionService(
			masterKey ?? DefaultMasterKey(),
			Options.Create(options ?? DefaultOptions()));
	}

	#region Positive tests

	[Theory]
	[InlineData("aBcDeFgHiJkLmNoPqRsTuVwXyZ123456")]
	[InlineData("a1b2c3d4e5f6g7h8")]
	[InlineData("aBcDeFgHiJkLmNoPqRsTuVwXyZ123456aBcDeFgHiJkLmNoPqRsTuVwXyZ123456")]
	[InlineData("api-key_with-dashes_and_underscores-1234")]
	public void Encrypt_ThenDecrypt_ReturnsOriginalApiKey(string apiKey)
	{
		//Arrange
		var sut = CreateSut();

		//Act
		var blob = sut.Encrypt(apiKey);
		var result = sut.Decrypt(blob);

		//Assert
		result.Should().Be(apiKey);
	}

	[Fact]
	public void Encrypt_ValidApiKey_ReturnNonEmptyBlob()
	{
		//Arrange
		var sut = CreateSut();

		//Act
		var blob = sut.Encrypt("aBcDeFgHiJkLmNoPqRsTuVwXyZ123456");

		//Assert
		blob.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public void Decrypt_SameBlob_DecryptedTwice_ReturnsSameResult()
	{
		//Arrange
		var sut = CreateSut();
		var blob = sut.Encrypt("aBcDeFgHiJkLmNoPqRsTuVwXyZ123456");

		//Act
		var first = sut.Decrypt(blob);
		var second = sut.Decrypt(blob);

		//Assert
		first.Should().Be(second);
	}

	[Fact]
	public void Encrypt_ThenDecrypt_WithDifferentInstancesSameMasterKey_ReturnsOriginalApiKey()
	{
		//Arrange
		var masterKey = DefaultMasterKey();
		var options = DefaultOptions();
		var apiKey = "aBcDeFgHiJkLmNoPqRsTuVwXyZ123456";

		var encryptor = CreateSut(masterKey, options);
		var decryptor = CreateSut(masterKey, options);

		//Act
		var blob = encryptor.Encrypt(apiKey);
		var result = decryptor.Decrypt(blob);

		//Assert
		result.Should().Be(apiKey);
	}

	#endregion

	#region Cryptographic properties tests

	[Fact]
	public void Encrypt_SameApiKey_CalledTwice_ProducesDifferentBlobs()
	{
		//Arrange
		var sut = CreateSut();
		var apiKey = "aBcDeFgHiJkLmNoPqRsTuVwXyZ123456";

		//Act
		var first = sut.Encrypt(apiKey);
		var second = sut.Encrypt(apiKey);

		//Assert
		first.Should().NotBeEquivalentTo(second);
	}

	[Fact]
	public void Encrypt_ValidApiKey_BlobDoesNotContainPlaintext()
	{
		//Arrange
		var sut = CreateSut();
		var apiKey = "aBcDeFgHiJkLmNoPqRsTuVwXyZ123456";

		//Act
		var blob = sut.Encrypt(apiKey);
		var apiKeyBytes = System.Text.Encoding.UTF8.GetBytes(apiKey);

		//Assert
		blob.Should().NotContainInOrder(apiKeyBytes);
	}

	#endregion

	#region Negative tests

	[Fact]
	public void Encrypt_NullApiKey_ThrowsArgumentException()
	{
		//Arrange
		var sut = CreateSut();

		//Act
		var act = () => sut.Encrypt(null!);

		//Assert
		act.Should().Throw<ArgumentException>();
	}

	[Fact]
	public void Encrypt_EmptyApiKey_ThrowsArgumentException()
	{
		//Arrange
		var sut = CreateSut();

		//Act
		var act = () => sut.Encrypt(string.Empty);

		//Assert
		act.Should().Throw<ArgumentException>();
	}

	[Fact]
	public void Decrypt_NullBlob_ThrowsArgumentException()
	{
		//Arrange
		var sut = CreateSut();

		//Act
		var act = () => sut.Decrypt(null!);

		//Assert
		act.Should().Throw<ArgumentException>();
	}

	[Fact]
	public void Decrypt_TooShortBlob_ThrowsArgumentException()
	{
		//Arrange
		var sut = CreateSut();

		//Act
		var act = () => sut.Decrypt(new byte[10]);

		//Assert
		act.Should().Throw<ArgumentException>();
	}

	[Fact]
	public void Decrypt_WrongMasterKey_ThrowsInvalidOperationException()
	{
		//Arrange
		var encryptor = CreateSut();
		var decryptor = CreateSut();

		//Act
		var blob = encryptor.Encrypt("aBcDeFgHiJkLmNoPqRsTuVwXyZ123456");
		var act = () => decryptor.Decrypt(blob);

		//Assert
		act.Should().Throw<InvalidOperationException>();
	}

	#endregion

	#region Boundary tests
	
	[Fact]
	public void Encrypt_WhitespaceString_DoesNotThrow()
	{
		//Arrange
		var sut = CreateSut();

		//Act
		var act = () => sut.Encrypt("   ");

		//Assert
		act.Should().NotThrow();
	}

	#endregion


	#region Tamp

	[Theory]
	[InlineData(0)]   // encrypted DEK zone
	[InlineData(30)]  // IV inside DEK zone
	[InlineData(61)]  // IV data zone
	[InlineData(75)]  // ciphertext zone
	[InlineData(73)]  // tag zone
	public void Decrypt_TamperedBlob_ThrowsInvalidOperationException(int byteIndex)
	{
		//Arrange
		var sut = CreateSut();

		//Act
		var blob = sut.Encrypt("aBcDeFgHiJkLmNoPqRsTuVwXyZ123456");

		blob[byteIndex] ^= 0xFF;
		var act = () => sut.Decrypt(blob);

		//Assert
		act.Should().Throw<InvalidOperationException>();
	}

	#endregion
}
