using _01_SampleWebApi.Services;
using Microsoft.Extensions.Options;
using _01_SampleWebApi.Models;

namespace _01_SampleWebApi
{
    #region 01. Program Class - Entry Point of Application
    /*
        ✔ Main entry point of ASP.NET Core Web API.
        ✔ From .NET 6+, Startup.cs is merged into Program.cs.
        ✔ All configuration + service registration happens here.
    */
    #endregion

    public class Program
    {
        public static void Main(string[] args)
        {
            #region 02. Create Builder

            /*
                ✔ Creates WebApplicationBuilder
                ✔ Automatically loads:

                    - appsettings.json
                    - appsettings.{Environment}.json
                    - Environment Variables
                    - Command-line arguments
                    - Logging configuration
                    - DI Container
            */

            #endregion

            var builder = WebApplication.CreateBuilder(args);


            #region 03. Register MVC Controllers

            builder.Services.AddControllers();

            #endregion


            #region 04. Register Custom Services

            /*
                Scoped → One instance per HTTP request
                Recommended for business logic layer
            */

            builder.Services.AddScoped<IProductService, ProductService>();

            #endregion


            #region 05. OPTION 1 - Using IConfiguration (Direct Access)

            /*
                ✔ IConfiguration is automatically registered by ASP.NET Core.
                ✔ No need to register manually.
                ✔ Can be injected directly in Controller/Service.

                Example usage inside controller:

                    private readonly IConfiguration _configuration;

                    public TestController(IConfiguration configuration)
                    {
                        _configuration = configuration;
                    }

                    var value = _configuration["MySettings:AppName"];
            */

            #endregion


            #region 06. OPTION 2 - Using IOptions<T> (Strongly Typed Settings)

            /*
                ✔ Recommended approach for structured configuration.
                ✔ Clean & strongly typed.
                ✔ Avoids magic strings.

                Step 1: Create Settings Class

                    public class MySettings
                    {
                        public string AppName { get; set; }
                        public string Version { get; set; }
                    }

                Step 2: Register binding here:
            */

            builder.Services.Configure<MySettings>(
                builder.Configuration.GetSection("MySettings"));

            /*
                ✔ This binds "MySettings" section from appsettings.json
                ✔ Makes it available via IOptions<MySettings>
            */

            #endregion


            #region 07. Build Application

            var app = builder.Build();

            #endregion


            #region 08. Middleware Pipeline

            app.UseAuthorization();

            #endregion


            #region 09. Map Controllers

            app.MapControllers();

            #endregion


            #region 10. Run Application

            app.Run();

            #endregion
        }
    }

}
