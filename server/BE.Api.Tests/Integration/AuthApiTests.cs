using System.Net;
using System.Net.Http.Json;

namespace BE.Api.Tests.Integration;

[Collection(ApiCollection.Name)]
public class AuthApiTests(ApiFactory api)
{
    [Fact]
    public async Task Login_WithBootstrappedSystemOwner_ReturnsTokenThatWorks()
    {
        var client = api.Anonymous();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email = ApiFactory.OwnerEmail, password = ApiFactory.OwnerPassword });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LoginJson>(TestData.Json);
        Assert.False(string.IsNullOrEmpty(body!.Token));
        Assert.Equal("SystemOwner", body.User.Role);

        client.DefaultRequestHeaders.Authorization = new("Bearer", body.Token);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/auth/me")).StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Is401()
    {
        var response = await api.Anonymous().PostAsJsonAsync("/api/auth/login",
            new { email = ApiFactory.OwnerEmail, password = "wrong-password-123" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Users_AsAccountant_Is403()
    {
        var response = await api.As("Accountant").GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
