namespace Core.Options;

public class EncryptionOptions
{
	public int DekSizeBytes { get; set; } = 32;
	public int IvSizeBytes { get; set; } = 12;
	public int TagSizeBytes { get; set; } = 16;
}
