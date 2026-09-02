using GradeGenie.Application.DTOs;
using GradeGenie.Application.Validators;
using GradeGenie.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GradeGenie.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/conversion")]
public sealed class ConversionController : ControllerBase
{
    [HttpPost("convert")]
    [ProducesResponseType<ScaleConversionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<ScaleConversionResponse> Convert([FromBody] ScaleConversionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validator = new ScaleConversionValidator();
        var validation = validator.Validate(request);
        if (!validation.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(
                validation.Errors.GroupBy(error => error.PropertyName)
                    .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray())));
        }

        var convertedValue = ScaleConverter.Convert(request.Value, request.SourceScale, request.TargetScale);
        var response = new ScaleConversionResponse(convertedValue, request.SourceScale, request.TargetScale);
        return Ok(response);
    }
}
