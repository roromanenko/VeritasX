namespace Api.DTO;

public record AddExchangeConnectionRequest(
	string ApiKey,
	string SecretKey,
	bool IsTestnet
);

public record UpdateExchangeConnectionRequest(
	string ApiKey,
	string SecretKey,
	bool IsTestnet
);
