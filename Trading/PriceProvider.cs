using System.ComponentModel;

namespace Trading;

public interface IPriceProvider
{
	Task<decimal> GetPriceAsync(string asset, string baseline, CancellationToken ct = default);
}

public class TestPriceProvider : IPriceProvider
{
	private decimal _currentPrice = 0;


	public Task<decimal> GetPriceAsync(string asset, string baseline, CancellationToken ct = default)
	{
		return Task.FromResult(_currentPrice);
	}

	public void SetPrice(decimal price)
	{
		_currentPrice = price;
	}
}