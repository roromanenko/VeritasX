namespace VeritasX.Core.Options;

public class CacheOptions
{
    public int SizeLimit { get; set; } = 1024;
    public double CompactionPercentage { get; set; } = 0.2;
    public int DefaultTtlMinutes { get; set; } = 30;
    public IntervalBasedTtlOptions IntervalBasedTtl { get; set; } = new();
}

public class IntervalBasedTtlOptions
{
    public int ShortTerm { get; set; } = 2;
    public int MediumTerm { get; set; } = 10;
    public int LongTerm { get; set; } = 30;
    public int VeryLongTerm { get; set; } = 60;
} 