using _01_SampleWebApi.Services;   // 🔹 Required for IProductService & ProductService

namespace _01_SampleWebApi
{
    #region 01. Program Class - Entry Point of Application

    /*
        ✔ This is the main entry class of ASP.NET Core application.
        ✔ Execution starts from Main() method.
        ✔ From .NET 6 onwards, Startup.cs is removed.
        ✔ All configuration & service registration happens here.
        ✔ ASP.NET Core uses Kestrel server by default.
        ✔ Modern Web API Architecture:

                Controller
                    ↓
                Service Layer
                    ↓
                Repository Layer
                    ↓
                Database
    */

    #endregion

    public class Program
    {
        #region 02. Main Method - Application Starting Point

        /*
            ✔ Main() is entry point.
            ✔ CLR starts execution from here.
            ✔ static → No object required.
            ✔ args → Command-line arguments.
        */

        #endregion

        public static void Main(string[] args)
        {
            #region 03. Create WebApplication Builder

            /*
                ✔ WebApplication.CreateBuilder(args) creates builder.
                ✔ It automatically configures:

                    - Kestrel Web Server
                    - Configuration (appsettings.json)
                    - Logging
                    - Dependency Injection Container
                    - Hosting Environment

                ✔ Used to register services BEFORE building the app.
            */

            #endregion

            var builder = WebApplication.CreateBuilder(args);


            #region 04. Register Services (Dependency Injection)

            /*
                🔹 What is Dependency Injection (DI)?

                Instead of creating objects manually using:

                    new ProductService(); ❌

                We register services in DI container,
                and ASP.NET Core injects them automatically.

                ------------------------------------------------

                ✔ AddControllers()
                    Registers controller services.
                    Enables:
                        - Routing
                        - Model Binding
                        - API behavior
            */

            builder.Services.AddControllers();

            /*
                🔹 Register Custom Application Services

                AddScoped<IProductService, ProductService>();

                Meaning:

                Whenever Controller asks for IProductService,
                give ProductService object.

                ------------------------------------------------

                Why AddScoped?

                ✔ One object per HTTP request
                ✔ Recommended for Web API business logic
                ✔ Safe for database operations
            */

            builder.Services.AddScoped<IProductService, ProductService>();

            #endregion


            #region 05. Build the Application

            /*
                ✔ builder.Build() creates WebApplication instance.
                ✔ Finalizes all configurations.
                ✔ After this, services cannot be added.
            */

            #endregion

            var app = builder.Build();


            #region 06. Configure Middleware Pipeline

            /*
                ✔ Middleware handles HTTP request/response pipeline.
                ✔ Order matters.

                Example flow:

                    Client Request
                        ↓
                    Middleware
                        ↓
                    Controller
                        ↓
                    Response
            */

            app.UseAuthorization();  // Enables Authorization middleware

            #endregion


            #region 07. Map Controllers (Routing)

            /*
                ✔ app.MapControllers() connects HTTP routes to Controllers.
                ✔ Without this, APIs will not respond.

                Example:

                    GET /api/product
                        ↓
                    ProductController
            */

            app.MapControllers();

            #endregion


            #region 08. Run the Application

            /*
                ✔ app.Run() starts Kestrel server.
                ✔ Application begins listening for HTTP requests.
                ✔ Runs continuously until stopped.
            */

            #endregion

            app.Run();
        }
    }
}
