using GradeGenie.Application.DTOs;
using GradeGenie.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GradeGenie.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/students/{studentId:guid}")]
public sealed class StudentsController(IStudentAcademicService academics) : ControllerBase
{
    [HttpGet("cgpa")]
    [ProducesResponseType<StudentCgpaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentCgpaDto>> GetCgpa(Guid studentId, CancellationToken cancellationToken)
    {
        var result = await academics.CalculateCgpaAsync(studentId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("semesters/{semesterId:guid}/insight")]
    [ProducesResponseType<SemesterInsightDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SemesterInsightDto>> GenerateInsight(Guid studentId, Guid semesterId, CancellationToken cancellationToken)
    {
        var result = await academics.GenerateSemesterInsightAsync(studentId, semesterId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
