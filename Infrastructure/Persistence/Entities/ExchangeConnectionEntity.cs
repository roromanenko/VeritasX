namespace Infrastructure.Persistence.Entities;

public class ExchangeConnectionEntity
{
	public required byte[] EncryptedApiKey { get; set; }

	public required byte[] EncryptedSecretKey { get; set; }

	public required byte[] EncryptedDek { get; set; }

	public bool IsTestnet { get; set; }
	public DateTimeOffset CreatedAt { get; set; }
	public DateTimeOffset? LastUsedAt { get; set; }
}
