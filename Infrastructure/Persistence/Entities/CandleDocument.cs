using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Entities;

public record CandleDocument
(
	DateTime OpenTime,
	decimal Open,
	decimal High,
	decimal Low,
	decimal Close,
	decimal Volume
);