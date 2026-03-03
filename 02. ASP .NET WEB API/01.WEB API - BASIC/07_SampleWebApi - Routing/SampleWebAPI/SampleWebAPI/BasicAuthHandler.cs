using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace SampleWebAPI.Auth
{
    /// <summary>
    /// Handles HTTP Basic Authentication.
    ///
    /// Expected request header:
    ///   Authorization: Basic &lt;base64(username:password)&gt;
    ///
    /// The password is compared against "ApiSettings:ApiKey" in appsettings.json.
    /// Username can be anything (e.g. "admin").
    /// </summary>
    public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IConfiguration _configuration;

        public BasicAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IConfiguration configuration): base(options, logger, encoder)
        {
            _configuration = configuration;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // ── 1. Check that the Authorization header exists ────────────────
            if (!Request.Headers.ContainsKey("Authorization"))
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization header."));

            string authHeader = Request.Headers["Authorization"].ToString();

            // ── 2. Must start with "Basic " ──────────────────────────────────
            if (!authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(AuthenticateResult.Fail("Invalid authorization scheme."));

            // ── 3. Decode Base64 credentials ─────────────────────────────────
            string encodedCredentials = authHeader["Basic ".Length..].Trim();
            string decodedCredentials;

            try
            {
                decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
            }
            catch
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Base64 encoding."));
            }

            // ── 4. Split into username:password ──────────────────────────────
            int colonIndex = decodedCredentials.IndexOf(':');
            if (colonIndex < 0)
                return Task.FromResult(AuthenticateResult.Fail("Invalid credentials format."));

            string username = decodedCredentials[..colonIndex];
            string password = decodedCredentials[(colonIndex + 1)..];

            // ── 5. Compare password against the API key in appsettings.json ──
            string? expectedKey = _configuration["ApiSettings:ApiKey"];

            if (string.IsNullOrEmpty(expectedKey) || password != expectedKey)
                return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));

            // ── 6. Build claims and return success ────────────────────────────
            // Give different roles based on who logged in
            string role = username.ToLower() == "admin" ? "Admin" : "ApiUser";

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role),   // ← "Admin" or "ApiUser"
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}