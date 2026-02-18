using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.RateLimiting;
using SampleWebAPI.Auth;
using SampleWebAPI.Services;

namespace SampleWebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ================================================================
            // SECTION 1: REGISTER SERVICES
            // All registrations must happen BEFORE builder.Build() is called.
            // ================================================================

            #region 1.1 — Controllers
            // Enables MVC controller routing and model binding.
            builder.Services.AddControllers();
            #endregion

            #region 1.2 — Application Services (Dependency Injection)
            // ProductService is Singleton: one shared in-memory list for the app's lifetime.
            // Swap to AddScoped/AddTransient when connecting to a real database.
            builder.Services.AddSingleton<IProductService, ProductService>();
            #endregion

            #region 1.3 — Authentication (Basic Auth)
            // Registers the "BasicAuth" scheme backed by our custom BasicAuthHandler.
            // The handler reads ApiSettings:ApiKey from appsettings.json and compares
            // it against the password in the Authorization: Basic <base64> header.
            builder.Services.AddAuthentication("BasicAuth")
                .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("BasicAuth", null);
            #endregion

            #region 1.4 — Authorization
            // Enables the [Authorize] attribute on controllers and actions.
            // Works together with the authentication scheme above.
            builder.Services.AddAuthorization();
            #endregion

            #region 1.5 — CORS
            // Allows any client origin for development convenience.
            // ⚠️  Restrict AllowAnyOrigin() to specific domains in production.
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
            #endregion

            #region 1.6 — Rate Limiting
            // Fixed window: max 5 requests per 1-minute window per client.
            // Applied via [EnableRateLimiting("fixed")] on the controller.
            // Clients that exceed the limit receive HTTP 429 Too Many Requests.
            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("fixed", opt =>
                {
                    opt.PermitLimit = 5;
                    opt.Window = TimeSpan.FromMinutes(1);
                });
            });
            #endregion

            // ================================================================
            // Build the application — no service registrations after this line.
            // ================================================================
            var app = builder.Build();

            // ================================================================
            // SECTION 2: MIDDLEWARE PIPELINE
            // Order is critical — each middleware runs in the sequence below.
            // ================================================================

            #region 2.1 — Custom Request/Response Logger
            // Inline middleware that logs every incoming request and outgoing response.
            // Runs first so it captures the full lifecycle, including auth failures.
            app.Use(async (context, next) =>
            {
                Console.WriteLine($"[→ Request]  {context.Request.Method} {context.Request.Path}");
                await next(); // Pass to the next middleware
                Console.WriteLine($"[← Response] {context.Response.StatusCode}");
            });
            #endregion

            #region 2.2 — CORS
            // Must come before UseRouting so CORS headers are added to
            // all responses, including preflight OPTIONS requests.
            app.UseCors("AllowAll");
            #endregion

            #region 2.3 — Routing
            // Parses the URL and matches it to an endpoint (controller action).
            // Must come before UseAuthentication and UseAuthorization.
            app.UseRouting();
            #endregion

            #region 2.4 — Authentication
            // Runs BasicAuthHandler to decode the Authorization header and
            // populate HttpContext.User with claims (name, role).
            // Must come AFTER UseRouting and BEFORE UseAuthorization.
            app.UseAuthentication();
            #endregion

            #region 2.5 — Authorization
            // Checks [Authorize] attributes — verifies the authenticated user
            // has permission to access the matched endpoint.
            // Must come AFTER UseAuthentication.
            app.UseAuthorization();
            #endregion

            #region 2.6 — Rate Limiting
            // Enforces per-client request limits defined in Section 1.6.
            // Placed after auth so limits can eventually be applied per-user.
            app.UseRateLimiter();
            #endregion

            #region 2.7 — Endpoint Mapping
            // Wires controller action methods as the final HTTP handlers.
            app.MapControllers();
            #endregion

            // Start the web server.
            app.Run();
        }
    }
}



#region Sample Calling Code (for testing with HttpClient)

/*
 * # base64("admin:MySuperSecretKey123!") = YWRtaW46TXlTdXBlclNlY3JldEtleTEyMyE=
     curl -H "Authorization: Basic YWRtaW46TXlTdXBlclNlY3JldEtleTEyMyE=" \
     https://localhost:5001/api/products
 */
#endregion