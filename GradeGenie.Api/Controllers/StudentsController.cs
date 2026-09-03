using GradeGenie.Application.DTOs;
using GradeGenie.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GradeGenie.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/students")]
public sealed class StudentsController(IStudentAcademicService academics) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType<StudentDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<StudentDto>> CreateStudent(CreateStudentRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        // For local development allow anonymous creation by generating a transient user id
        if (userId is null) userId = Guid.NewGuid().ToString();
        var student = await academics.CreateStudentAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetCgpa), new { studentId = student.Id }, student);
    }

    [HttpPost("{studentId:guid}/semesters")]
    [ProducesResponseType<SemesterDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SemesterDto>> AddSemester(Guid studentId, CreateSemesterRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var semester = await academics.AddSemesterAsync(userId, studentId, request, cancellationToken);
        return semester is null ? NotFound() : CreatedAtAction(nameof(GetCgpa), new { studentId }, semester);
    }

    [HttpPost("{studentId:guid}/semesters/{semesterId:guid}/courses")]
    [ProducesResponseType<CourseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseDto>> AddCourse(Guid studentId, Guid semesterId, CreateCourseRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var course = await academics.AddCourseAsync(userId, studentId, semesterId, request, cancellationToken);
        return course is null ? NotFound() : StatusCode(StatusCodes.Status201Created, course);
    }

    [HttpGet("{studentId:guid}/cgpa")]
    [ProducesResponseType<StudentCgpaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentCgpaDto>> GetCgpa(Guid studentId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var result = await academics.CalculateCgpaAsync(userId, studentId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{studentId:guid}/target-grade")]
    [ProducesResponseType<TargetGradeResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TargetGradeResponse>> CalculateTargetGrade(Guid studentId, TargetGradeRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var result = await academics.CalculateTargetGradeAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{studentId:guid}/academic-plan")]
    [ProducesResponseType<AcademicPlanResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AcademicPlanResponse>> CreateAcademicPlan(Guid studentId, AcademicPlanRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await academics.CreateAcademicPlanAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{studentId:guid}/semesters/{semesterId:guid}/insight")]
    [ProducesResponseType<SemesterInsightDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SemesterInsightDto>> GenerateInsight(Guid studentId, Guid semesterId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var result = await academics.GenerateSemesterInsightAsync(userId, studentId, semesterId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
}
