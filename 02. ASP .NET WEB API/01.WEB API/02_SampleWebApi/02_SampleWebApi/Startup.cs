using Microsoft.AspNetCore.Builder;              // Interfaces for configuring middleware
using Microsoft.AspNetCore.Hosting;              // Hosting environment (Development, Production)
using Microsoft.Extensions.Configuration;        // Access to appsettings.json
using Microsoft.Extensions.DependencyInjection;  // Dependency Injection services
using Microsoft.Extensions.Hosting;               // Environment helpers (IsDevelopment)

namespace _02_SampleWebApi
{
    public class Startup
    {
        #region Constructor & Configuration

        /// <summary>
        /// Startup constructor
        /// IConfiguration is automatically injected by the runtime
        /// Used to read values from appsettings.json
        /// </summary>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Holds application configuration values
        /// Example: Connection strings, app settings
        /// </summary>
        public IConfiguration Configuration { get; }

        #endregion

        #region ConfigureServices (Service Registration)

        /// <summary>
        /// This method is used to register services into the Dependency Injection container.
        /// This method is executed FIRST when the application starts.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            /*
             * Adds support for Controllers (Web API)
             * - Enables attribute routing
             * - Enables model binding & validation
             * - Required for MapControllers()
             */
            services.AddControllers();

            /*
             * Additional services like:
             * - Swagger
             * - Entity Framework
             * - Authentication
             * are added here
             */
        }

        #endregion

        #region Configure (HTTP Request Pipeline)

        /// <summary>
        /// This method defines how HTTP requests are processed.
        /// Middleware is executed in the order it is added.
        /// </summary>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            /*
             * Enables detailed error page only in Development environment
             * Helps developers see stack trace during errors
             */
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            /*
             * Adds routing middleware
             * This decides WHICH controller/action should handle the request
             */
            app.UseRouting();

            /*
             * Adds authorization middleware
             * Checks user permissions (used with [Authorize] attribute)
             * Even if not used now, good to keep it for future
             */
            app.UseAuthorization();

            /*
             * Maps attribute-routed controllers
             * Without this, APIs will NOT be reachable
             */
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        #endregion
    }
}
