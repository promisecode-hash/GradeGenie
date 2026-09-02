using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace GradeGenie.Api.Tests;

public sealed class StudentsEndpointsTests(GradeGenieApiFactory factory) : IClassFixture<GradeGenieApiFactory>
{
    private const string SigningKey = "integration-test-signing-key-that-is-long-enough";

    [Fact]
    public async Task Authenticated_owner_can_create_academic_history_and_read_calculated_cgpa()
    {
        await factory.InitializeDatabaseAsync();
        using var client = CreateClientFor("student-a");
        var studentResponse = await client.PostAsJsonAsync("/api/students", new { fullName = "Ada Lovelace", institutionType = 0 });
        Assert.Equal(HttpStatusCode.Created, studentResponse.StatusCode);
        var student = await studentResponse.Content.ReadFromJsonAsync<StudentResponse>();
        Assert.NotNull(student);

        var semesterResponse = await client.PostAsJsonAsync($"/api/students/{student!.Id}/semesters", new { name = "First Semester", academicYear = 2026 });
        Assert.Equal(HttpStatusCode.Created, semesterResponse.StatusCode);
        var semester = await semesterResponse.Content.ReadFromJsonAsync<SemesterResponse>();
        Assert.NotNull(semester);

        var courseResponse = await client.PostAsJsonAsync($"/api/students/{student.Id}/semesters/{semester!.Id}/courses", new { code = "CSC101", title = "Computing", creditUnits = 3, grade = 0 });
        Assert.Equal(HttpStatusCode.Created, courseResponse.StatusCode);

        var cgpaResponse = await client.GetAsync($"/api/students/{student.Id}/cgpa");
        Assert.Equal(HttpStatusCode.OK, cgpaResponse.StatusCode);
        var cgpa = await cgpaResponse.Content.ReadFromJsonAsync<CgpaResponse>();
        Assert.Equal(5m, cgpa!.Cgpa);
    }

    [Fact]
    public async Task Different_authenticated_user_cannot_read_another_students_cgpa()
    {
        await factory.InitializeDatabaseAsync();
        using var owner = CreateClientFor("student-owner");
        var createResponse = await owner.PostAsJsonAsync("/api/students", new { fullName = "Owner" });
        var student = await createResponse.Content.ReadFromJsonAsync<StudentResponse>();

        using var stranger = CreateClientFor("student-stranger");
        var response = await stranger.GetAsync($"/api/students/{student!.Id}/cgpa");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Authenticated_user_can_convert_between_international_grade_scales()
    {
        await factory.InitializeDatabaseAsync();
        using var client = CreateClientFor("student-converter");

        var response = await client.PostAsJsonAsync("/api/conversion/convert", new { value = 75m, sourceScale = 4, targetScale = 1 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ScaleConversionResponse>();
        Assert.NotNull(result);
        Assert.Equal(3.75m, result!.ConvertedValue);
    }

    [Fact]
    public async Task Authenticated_user_can_calculate_required_grade_for_target_cgpa()
    {
        await factory.InitializeDatabaseAsync();
        using var client = CreateClientFor("student-target-grade");

        var response = await client.PostAsJsonAsync($"/api/students/{Guid.NewGuid()}/target-grade", new { currentCgpa = 3.0m, targetCgpa = 3.5m, completedCreditUnits = 20m, remainingCreditUnits = 20m });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TargetGradeResponse>();
        Assert.NotNull(result);
        Assert.Equal(4.00m, result!.RequiredGradePoint);
    }

    [Fact]
    public async Task Authenticated_user_can_request_an_academic_plan_recommendation()
    {
        await factory.InitializeDatabaseAsync();
        using var client = CreateClientFor("student-academic-plan");

        var response = await client.PostAsJsonAsync($"/api/students/{Guid.NewGuid()}/academic-plan", new { currentCgpa = 2.8m, targetCgpa = 3.5m, completedCreditUnits = 40m, remainingCreditUnits = 20m });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AcademicPlanResponse>();
        Assert.NotNull(result);
        Assert.Equal(4.90m, result!.RequiredGradePoint);
        Assert.Contains("A", result.RecommendedPriority, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Authenticated_user_can_generate_a_local_semester_insight_without_external_ai_configuration()
    {
        await factory.InitializeDatabaseAsync();
        using var client = CreateClientFor("student-insight");

        var studentResponse = await client.PostAsJsonAsync("/api/students", new { fullName = "Ada Insight", institutionType = 0 });
        Assert.Equal(HttpStatusCode.Created, studentResponse.StatusCode);
        var student = await studentResponse.Content.ReadFromJsonAsync<StudentResponse>();
        Assert.NotNull(student);

        var semesterResponse = await client.PostAsJsonAsync($"/api/students/{student!.Id}/semesters", new { name = "Second Semester", academicYear = 2026 });
        Assert.Equal(HttpStatusCode.Created, semesterResponse.StatusCode);
        var semester = await semesterResponse.Content.ReadFromJsonAsync<SemesterResponse>();
        Assert.NotNull(semester);

        var courseResponse = await client.PostAsJsonAsync($"/api/students/{student.Id}/semesters/{semester!.Id}/courses", new { code = "ENG201", title = "Academic Writing", creditUnits = 3, grade = 2 });
        Assert.Equal(HttpStatusCode.Created, courseResponse.StatusCode);

        var insightResponse = await client.PostAsync($"/api/students/{student.Id}/semesters/{semester.Id}/insight", null);
        Assert.Equal(HttpStatusCode.OK, insightResponse.StatusCode);

        var insight = await insightResponse.Content.ReadFromJsonAsync<SemesterInsightResponse>();
        Assert.NotNull(insight);
        Assert.False(string.IsNullOrWhiteSpace(insight!.Insight));
        Assert.Contains("semester", insight.Insight, StringComparison.OrdinalIgnoreCase);
    }

    private HttpClient CreateClientFor(string userId)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(userId));
        return client;
    }

    private static string CreateToken(string userId)
    {
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken("GradeGenie", "GradeGenie.Client", [new Claim("sub", userId)], expires: DateTime.UtcNow.AddMinutes(10), signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed record StudentResponse(Guid Id);
    private sealed record SemesterResponse(Guid Id);
    private sealed record CgpaResponse(decimal Cgpa);
    private sealed record ScaleConversionResponse(decimal ConvertedValue);
    private sealed record TargetGradeResponse(decimal RequiredGradePoint, string RequiredLetterGrade);
    private sealed record AcademicPlanResponse(decimal RequiredGradePoint, string RecommendedPriority, string Summary);
    private sealed record SemesterInsightResponse(Guid SemesterId, decimal Gpa, string Insight);
}
