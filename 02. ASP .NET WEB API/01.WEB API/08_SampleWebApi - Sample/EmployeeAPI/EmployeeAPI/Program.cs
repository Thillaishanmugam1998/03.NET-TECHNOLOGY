using EmployeeAPI.Services;
using Microsoft.OpenApi;

namespace EmployeeAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {

            // ─────────────────────────────────────────────────────────────
            // STEP 1 – Create a "builder"
            //
            // WebApplication.CreateBuilder(args) sets up everything we need
            // to BUILD the application.  Think of it like a construction
            // blueprint before the actual building is erected.
            // 'args' are command-line arguments passed when starting the app.
            // ──────────────────────────────────────────────────────────────

            var builder = WebApplication.CreateBuilder(args);


            // ──────────────────────────────────────────────────────────────
            // STEP 2 – Register Services (Dependency Injection Container)
            //
            // "Services" are classes that do work in our app.
            // We register them here and ASP.NET Core will automatically
            // create and inject them wherever they are needed.
            //
            // AddScoped  → one instance per HTTP request
            // AddSingleton → one instance for the whole app lifetime  ← we use this
            //               because our in-memory list must survive across requests
            // AddTransient → a brand new instance every time it is requested
            // ──────────────────────────────────────────────────────────────


            // Register IEmployeeService → EmployeeService as a Singleton.
            // Singleton = one shared instance for the whole app lifetime.
            // This matters because our in-memory List lives inside this class –
            // the same instance is injected into every controller request.

            builder.Services.AddSingleton<IEmployeeService, EmployeeService>();

            // Tell ASP.NET Core to find and register all Controllers
            // (files that inherit from ControllerBase) automatically.
            builder.Services.AddControllers();


            // ──────────────────────────────────────────────────────────────
            // STEP 3 – Add Swagger / OpenAPI
            //
            // Swagger generates an interactive UI at /swagger so you can
            // test all your API endpoints directly in the browser without
            // needing Postman or any other tool.
            // ──────────────────────────────────────────────────────────────
            builder.Services.AddEndpointsApiExplorer();  // Discovers all minimal API endpoints
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Employee Management API",
                    Version = "v1",
                    Description = "A simple CRUD API for managing employee information. Data is stored in-memory."
                });
            });


            // ──────────────────────────────────────────────────────────────
            // STEP 4 – Build the app
            //
            // builder.Build() takes everything we registered above and
            // creates the actual runnable WebApplication object.
            // After this line we can no longer add services.
            // ──────────────────────────────────────────────────────────────
            var app = builder.Build();

            // ──────────────────────────────────────────────────────────────
            // STEP 5 – Configure the HTTP Middleware Pipeline
            //
            // Middleware is code that runs on EVERY request/response.
            // Order matters! Each middleware can:
            //   a) Do something BEFORE passing to the next middleware
            //   b) Pass the request to the NEXT middleware (next())
            //   c) Do something AFTER the next middleware finishes
            //
            // Think of it like airport security layers –
            // every passenger (request) passes through each checkpoint.
            // ──────────────────────────────────────────────────────────────

            // Enable Swagger only when we are running in Development mode.
            // In Production we don't expose the Swagger UI for security reasons.
            if (app.Environment.IsDevelopment())
            {
                // Activates the Swagger JSON endpoint  (/swagger/v1/swagger.json)
                app.UseSwagger();

                // Activates the Swagger browser UI  (/swagger/index.html)
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Employee API v1");
                    options.RoutePrefix = string.Empty; // Makes Swagger the default page at  /
                });
            }

            // Redirect HTTP requests to HTTPS automatically.
            // Example: http://localhost:5000  →  https://localhost:7000
            app.UseHttpsRedirection();

            // Enables Authorization middleware.
            // Even though we have no auth in this project, it is best practice
            // to include it so the pipeline is ready if auth is added later.
            app.UseAuthorization();

            // Maps incoming HTTP requests to the correct Controller action.
            // Example: GET /api/employees  →  EmployeesController.GetAll()
            app.MapControllers();

            // ──────────────────────────────────────────────────────────────
            // STEP 6 – Run the application
            //
            // app.Run() starts the web server and begins listening for
            // incoming HTTP requests. This line blocks – the app keeps
            // running until you stop it (Ctrl+C or process kill).
            // ──────────────────────────────────────────────────────────────
            app.Run();
        }
    }
}