namespace _01_SampleWebApi
{
    #region 01. Program Class - Entry Point of Application
    /*
        ✔ This is the main entry class of ASP.NET Core application.
        ✔ Execution of the application starts from the Main() method.
        ✔ From .NET 6 onwards, Startup.cs is removed.
        ✔ All configuration and service registration happens inside Program.cs.
        ✔ Before .NET 6 Have both Startup.cs and Program.cs file
        ✔ Net Core have configuration in .json file but .Net framework have configuration in .config file
        ✔ In .Net framework we have IIS server but in .Net core we have Kestrel server
        ✔ In Web API New Architechture is Controller -> Service Layer -> Repository Layer -> Database
        ✔ In Web API Old Architechture is Controller -> Business Logic -> Data Access Layer 
        ✔ While Creating ASP.NET Core Web API Project Create Controller folder for adding controller
        ✔ While Creating ASP.NET Core Web API Project create Models folder for adding model classes
        ✔ While Creating ASP.NET Core Web API Project create Data folder for adding database related classes
        ✔ While Creating ASP.NET Core Web API Project create Services folder for adding service related classes
        ✔ While Creating ASP.NET Core Web API Project create Repositories folder for adding repository related classes

    */
    #endregion

    public class Program
    {
        #region 02. Main Method - Application Starting Point
        /*
            ✔ Main() is the entry point of the application.
            ✔ CLR starts execution from here.
            ✔ static → Called without creating object.
            ✔ string[] args → Command line arguments.
        */
        #endregion

        public static void Main(string[] args)
        {
            #region 03. Create WebApplication Builder
            /*
                ✔ WebApplication.CreateBuilder(args) creates builder object.
                ✔ It sets up:
                    - Kestrel Web Server
                    - Configuration (appsettings.json)
                    - Logging
                    - Dependency Injection (DI) container
                    - Hosting Environment
                ✔ Used to configure services before building the app.
            */
            #endregion

            var builder = WebApplication.CreateBuilder(args);


            #region 04. Register Services (Dependency Injection)
            /*
                ✔ builder.Services is DI container.
                ✔ AddControllers() registers controller services.
                ✔ Enables:
                    - Attribute Routing
                    - Model Binding
                    - API Controller Features
                ✔ Without this, controllers will not work.
            */
            #endregion

            builder.Services.AddControllers();


            #region 05. Build the Application
            /*
                ✔ builder.Build() creates the WebApplication instance.
                ✔ Finalizes configurations.
                ✔ Prepares middleware pipeline.
            */
            #endregion

            var app = builder.Build();


            #region 06. Configure Middleware Pipeline
            /*
                ✔ Middleware handles HTTP requests & responses.
                ✔ app.UseAuthorization() adds authorization middleware.
                ✔ Every request passes through middleware in order.
                ✔ If authorization fails, request stops here.
            */
            #endregion

            app.UseAuthorization();


            #region 07. Map Controllers (Routing Configuration)
            /*
                ✔ app.MapControllers() connects routes to controller actions.
                ✔ Example:
                    GET /api/employee
                    → Routed to EmployeeController
                ✔ Without this, APIs will not respond.
            */
            #endregion

            app.MapControllers();


            #region 08. Run the Application
            /*
                ✔ app.Run() starts the application.
                ✔ Kestrel server starts listening for HTTP requests.
                ✔ Application runs continuously until stopped.
            */
            #endregion

            app.Run();
        }
    }
}

