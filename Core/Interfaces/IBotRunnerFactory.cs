using Core.Domain;

namespace Core.Interfaces;

public interface IBotRunnerFactory
{
	IBotRunner Create(BotConfiguration bot);
}
