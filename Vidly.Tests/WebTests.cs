using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Vidly.Tests;

public class VidlyFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"vidly-test-{Guid.NewGuid():N}.db");

    /// <summary>When true, every request is signed in as a test user.</summary>
    public bool SignedIn { get; init; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={_dbPath}");
        if (SignedIn)
        {
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
                services.PostConfigure<AuthenticationOptions>(o =>
                {
                    o.DefaultScheme = "Test";
                    o.DefaultAuthenticateScheme = "Test";
                    o.DefaultChallengeScheme = "Test";
                });
            });
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        SqliteConnection.ClearAllPools();
        if (File.Exists(_dbPath)) File.Delete(_dbPath);
    }
}

public class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "tester@example.com"), new Claim(ClaimTypes.NameIdentifier, "1")], "Test");
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), "Test")));
    }
}

public class AnonymousTests(VidlyFactory factory) : IClassFixture<VidlyFactory>
{
    private CancellationToken Ct => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData("/")]
    [InlineData("/Movies")]
    [InlineData("/Customers")]
    [InlineData("/Identity/Account/Login")]
    [InlineData("/Identity/Account/Register")]
    public async Task Pages_Load(string url)
    {
        var response = await factory.CreateClient().GetAsync(url, Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Movies_ListsSeededCatalogue()
    {
        var html = await factory.CreateClient().GetStringAsync("/Movies", Ct);
        Assert.Contains("The Hangover", html);
        Assert.Contains("Comedy", html);
    }

    [Fact]
    public async Task Movies_SearchFiltersByName()
    {
        var html = await factory.CreateClient().GetStringAsync("/Movies?search=Love", Ct);
        Assert.Contains("A Love So Beautiful", html);
        Assert.DoesNotContain("The Hangover", html);
    }

    [Fact]
    public async Task MissingMovie_Returns404()
    {
        var response = await factory.CreateClient().GetAsync("/Movies/Details/9999", Ct);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Editing_RequiresLogin()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var response = await client.GetAsync("/Customers/New", Ct);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Identity/Account/Login", response.Headers.Location!.ToString());
    }
}

public class SignedInTests : IClassFixture<SignedInTests.Factory>
{
    public class Factory : VidlyFactory
    {
        public Factory() { SignedIn = true; }
    }

    private readonly Factory _factory;
    private CancellationToken Ct => TestContext.Current.CancellationToken;

    public SignedInTests(Factory factory) => _factory = factory;

    private static string Token(string html) =>
        Regex.Match(html, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;

    [Fact]
    public async Task CreateCustomer_ShowsInList()
    {
        var client = _factory.CreateClient();
        var form = await client.GetStringAsync("/Customers/New", Ct);

        var response = await client.PostAsync("/Customers/Save", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Customer.Name"] = "Ada Lovelace",
            ["Customer.MembershipTypeId"] = "2",
            ["Customer.Birthdate"] = "1990-12-10",
            ["Customer.Id"] = "0",
            ["__RequestVerificationToken"] = Token(form),
        }), Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync(Ct);
        Assert.Contains("Ada Lovelace", html);
        Assert.Contains("Monthly", html);
    }

    [Fact]
    public async Task CreateCustomer_UnderageOnPaidMembership_ShowsError()
    {
        var client = _factory.CreateClient();
        var form = await client.GetStringAsync("/Customers/New", Ct);

        var response = await client.PostAsync("/Customers/Save", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Customer.Name"] = "Young Person",
            ["Customer.MembershipTypeId"] = "4",
            ["Customer.Birthdate"] = DateTime.Today.AddYears(-10).ToString("yyyy-MM-dd"),
            ["Customer.Id"] = "0",
            ["__RequestVerificationToken"] = Token(form),
        }), Ct);

        var html = await response.Content.ReadAsStringAsync(Ct);
        Assert.Contains("at least 18 years old", html);
    }

    [Fact]
    public async Task EditMovie_UpdatesStock()
    {
        var client = _factory.CreateClient();
        var form = await client.GetStringAsync("/Movies/Edit/2", Ct);

        var response = await client.PostAsync("/Movies/Save", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Id"] = "2",
            ["Name"] = "The Hangover",
            ["GenreId"] = "5",
            ["ReleaseDate"] = "2009-06-05",
            ["NumberInStock"] = "9",
            ["__RequestVerificationToken"] = Token(form),
        }), Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var details = await client.GetStringAsync("/Movies/Details/2", Ct);
        Assert.Matches(@"Number in Stock</dt>\s*<dd class=""col-sm-9"">9</dd>", details);
    }

    [Fact]
    public async Task Save_WithoutAntiforgeryToken_IsRejected()
    {
        var response = await _factory.CreateClient().PostAsync("/Movies/Save", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Name"] = "Sneaky", ["GenreId"] = "1", ["ReleaseDate"] = "2020-01-01", ["NumberInStock"] = "1",
        }), Ct);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
