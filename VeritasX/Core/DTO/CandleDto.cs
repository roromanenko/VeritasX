namespace VeritasX.Core.DTO;

public record CandleDto(
    DateTime OpenTime,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume
); 