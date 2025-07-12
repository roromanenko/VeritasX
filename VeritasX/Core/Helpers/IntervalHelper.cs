namespace VeritasX.Core.Helpers;

public static class IntervalHelper
{
	public static TimeSpan Parse(string interval)
	{
		if (string.IsNullOrEmpty(interval))
			throw new ArgumentException("Interval cannot be null or empty");

		var match = System.Text.RegularExpressions.Regex.Match(interval, @"^(\d+)([smhdwM])$");
		
		if (!match.Success)
			throw new ArgumentException($"Invalid interval format: {interval}. Use format like '1m', '5h', '1d'");

		var value = int.Parse(match.Groups[1].Value);
		var unit = match.Groups[2].Value;

		return unit switch
		{
			"s" => TimeSpan.FromSeconds(value),
			"m" => TimeSpan.FromMinutes(value),
			"h" => TimeSpan.FromHours(value),
			"d" => TimeSpan.FromDays(value),
			"w" => TimeSpan.FromDays(value * 7),
			"M" => TimeSpan.FromDays(value * 30),
			_ => throw new ArgumentException($"Unsupported interval unit: {unit}")
		};
	}
}