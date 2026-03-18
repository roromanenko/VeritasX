using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain;

public class TradingSolution
{
	public required string Asset { get; set; }
	public decimal Quantity { get; set; }
	public decimal Price { get; set; }
	public SolutionType Type { get; set; }
	public string Reason { get; set; } = string.Empty;
}

public enum SolutionType
{
	Hold = 0,
	Buy = 1,
	Sell = 2,
}
