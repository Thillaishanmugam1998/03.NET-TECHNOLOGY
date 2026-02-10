using Microsoft.AspNetCore.Hosting;      // Provides web hosting features
using Microsoft.Extensions.Configuration; // Provides configuration support
using Microsoft.Extensions.Hosting;       // Generic host infrastructure
using Microsoft.Extensions.Logging;       // Logging infrastructure

namespace _02_SampleWebApi
{
    public class Program
    {
        #region Application Entry Point

        /// <summary>
        /// Main method – execution starts here
        /// Responsible for starting the web application
        /// </summary>
        public static void Main(string[] args)
        {
            /*
             * CreateHostBuilder builds the host
             * Build() prepares the application
             * Run() starts listening for HTTP requests
             */
            CreateHostBuilder(args)
                .Build()
                .Run();
        }

        #endregion

        #region Host Configuration

        /// <summary>
        /// Creates and configures the IHostBuilder
        /// This sets up:
        /// - Kestrel web server
        /// - Configuration (appsettings.json)
        /// - Logging
        /// - Dependency Injection
        /// </summary>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                /*
                 * Adds default configurations:
                 * - appsettings.json
                 * - appsettings.{Environment}.json
                 * - Environment variables
                 * - Command-line arguments
                 * - Logging (Console, Debug)
                 */
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    /*
                     * Specifies Startup.cs as the startup class
                     * Startup.cs contains:
                     * - ConfigureServices()
                     * - Configure()
                     */
                    webBuilder.UseStartup<Startup>();
                });

        #endregion
    }
}
