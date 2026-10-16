using Microsoft.AspNetCore.Mvc;

namespace VehicleRegistry.Api.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult ValidationErrors(IReadOnlyList<string> errors) =>
        ValidationProblem(new ValidationProblemDetails(
            new Dictionary<string, string[]> { ["validation"] = errors.ToArray() }));
}