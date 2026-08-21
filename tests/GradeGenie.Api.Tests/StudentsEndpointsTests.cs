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
        var studentResponse = await client.PostAsJsonAsync("/api/students", new { fullName = "Ada Lovelace" });
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
}
