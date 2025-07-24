using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace VeritasX.Api.Controllers;

public class BaseController : ControllerBase
{
	protected string? Username => User?.Identity?.Name;
	protected string? UserId => User?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
}