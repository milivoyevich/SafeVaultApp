using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
public class SecurityTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SecurityTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_Succeeds()
    {
        var response = await _client.PostAsync("/Account/Login", new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Username", "admin"),
            new KeyValuePair<string, string>("Password", "SecurePass123!")
        }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Dashboard", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Register_WithMismatchedPasswords_Fails()
    {
        var response = await _client.PostAsync("/Account/Register", new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Username", "testuser"),
            new KeyValuePair<string, string>("Password", "Test123!"),
            new KeyValuePair<string, string>("ConfirmPassword", "Test1234!"),
            new KeyValuePair<string, string>("Role", "User")
        }));
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Passwords do not match", content);
    }

    [Fact]
    public async Task AdminPanel_RequiresAdminRole()
    {
        var response = await _client.GetAsync("/Account/AdminPanel");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location.ToString());
    }
}