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
    [ProducesResponseType<StudentDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<StudentDto>> CreateStudent(CreateStudentRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
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
