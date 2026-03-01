using Core.Domain;

namespace Core.Interfaces;

public interface IEncryptionService
{
	byte[] Encrypt(string connection);

	string Decrypt(byte[] encryptedConnection);
}
