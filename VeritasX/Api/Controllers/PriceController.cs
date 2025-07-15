using Microsoft.AspNetCore.Mvc;
using VeritasX.Core.Interfaces;
using VeritasX.Core.Helpers;
using Microsoft.AspNetCore.Authorization;
using VeritasX.Core.Domain;

namespace VeritasX.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PriceController : BaseController
{
	private readonly ICachedPriceProvider _priceProvider;

	public PriceController(ICachedPriceProvider priceProvider)
	{
		_priceProvider = priceProvider;
	}

	[HttpGet("{symbol}")]
	public async Task<ActionResult<IEnumerable<Candle>>> Get(string symbol, DateTime from, DateTime to, string interval, CancellationToken ct = default)
	{
		TimeSpan intv = IntervalHelper.Parse(interval);

		var candles = await _priceProvider.GetHistoryAsync(symbol, from.ToUniversalTime(), to.ToUniversalTime(), intv, ct);

		return Ok(candles);
	}
}