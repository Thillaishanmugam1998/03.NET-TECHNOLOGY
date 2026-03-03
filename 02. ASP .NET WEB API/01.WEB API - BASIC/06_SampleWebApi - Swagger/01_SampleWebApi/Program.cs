using Microsoft.OpenApi;
using ShoppingCartAPI.Services;

namespace ShoppingCartAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region 🔹 01. Add Controllers

            /*
             WHAT IS THIS?

             ✔ Registers Controller support in ASP.NET Core
             ✔ Without this, your API controllers will not work

             WHY REQUIRED?

             ✔ Enables MVC pattern
             ✔ Enables routing like:
                  GET /api/cart
                  POST /api/cart

             SUMMARY:
             This tells ASP.NET Core:
             "My application uses Controllers"
            */

            builder.Services.AddControllers();

            #endregion


            #region 🔹 02. Swagger Registration (Service Registration Phase)

            /*
            ============================================================
            WHAT IS SWAGGER?
            ============================================================

            ✔ Swagger is an API Documentation and Testing Tool

            ✔ It automatically:

                • Lists all API endpoints
                • Shows request format
                • Shows response format
                • Allows testing API from browser

            ✔ No need Postman

            ✔ Swagger is based on OpenAPI Specification

            ✔ ASP.NET Core uses:

                Swashbuckle.AspNetCore Library


            ============================================================
            WHY SWAGGER IS USED?
            ============================================================

            Example Problem WITHOUT Swagger:

                ❌ You don't know:

                    What API exists?
                    What parameters required?
                    What response returns?


            Solution WITH Swagger:

                ✔ Shows all API list
                ✔ Shows request format
                ✔ Shows response format
                ✔ Allows testing


            Real-world usage:

                ✔ Banking APIs
                ✔ Payment APIs
                ✔ Enterprise APIs


            ============================================================
            HOW SWAGGER WORKS INTERNALLY?
            ============================================================

            Controller
                ↓

            EndpointsApiExplorer
                ↓

            SwaggerGen
                ↓

            Swagger JSON Generated
                ↓

            Swagger UI Displays in Browser


            ============================================================
            THIS METHOD:
            ============================================================
            */

            builder.Services.AddEndpointsApiExplorer();

            /*
             PURPOSE:

             ✔ Reads all controller endpoints

             Example:

                 GET /api/cart
                 POST /api/cart

             ✔ Sends endpoint information to Swagger Generator
            */


            //builder.Services.AddSwaggerGen(); -- default configuration

            // Optional : Suppose If we Need To Customize Swagger Documentation, We Can Pass Configuration To AddSwaggerGen Method

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("V10", new OpenApiInfo
                {
                    Title = "Thillai API",
                    Version = "V10",
                    Description = "A Brief Description of My APIs",
                    TermsOfService = new Uri("https://dotnettutorials.net/privacy-policy/"),
                    Contact = new OpenApiContact
                    {
                        Name = "Support",
                        Email = "support@thillai.net",
                        Url = new Uri("https://dotnettutorials.net/contact/")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Tamil Pvtd",
                        Url = new Uri("https://dotnettutorials.net/about-us/")
                    }
                });
            });

            /*
            PURPOSE:

            ✔ Generates Swagger Documentation

            ✔ Creates JSON file:

               https://localhost:5001/swagger/v1/swagger.json

            ✔ This JSON contains:

                All API Information
                Request format
                Response format

            ✔ Swagger UI uses this JSON to display interface

            */

            #endregion


            #region 🔹 03. Register Application Services (Dependency Injection)

            /*
             Dependency Injection (DI)

             Register Services with Lifetime:

             Singleton  → One instance entire application

             Scoped     → One instance per request

             Transient  → New instance every time
            */

            builder.Services.AddSingleton<IAppConfigService, AppConfigService>();

            builder.Services.AddScoped<ICartService, CartService>();

            builder.Services.AddTransient<IDiscountService, DiscountService>();

            builder.Services.AddScoped<ICartSummaryService, CartSummaryService>();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddMemoryCache();

            #endregion



            #region 🔹 04. Build Application

            /*
             This builds the application pipeline
            */

            var app = builder.Build();

            #endregion



            #region 🔹 05. Enable Swagger Middleware (Execution Pipeline Phase)

            /*
            ============================================================
            WHAT IS THIS?
            ============================================================

            This enables Swagger UI in Browser


            ============================================================
            THESE TWO METHODS ENABLE SWAGGER:
            ============================================================
            */

            if (app.Environment.IsDevelopment())
            {

                app.UseSwagger();

                /*
                 PURPOSE:

                 ✔ Generates Swagger JSON endpoint

                 URL:

                 https://localhost:5001/swagger/v1/swagger.json

                */



                app.UseSwaggerUI();

                /*
                PURPOSE:

                ✔ Displays Swagger UI in Browser


                ===================================================
                HOW TO OPEN SWAGGER UI?
                ===================================================

                Run application

                Browser open:

                https://localhost:5001/swagger


                OR


                https://localhost:5000/swagger



                ===================================================
                WHAT YOU WILL SEE?
                ===================================================

                ✔ All Controllers

                ✔ All API Endpoints

                Example:

                    GET /api/cart

                    POST /api/cart



                ===================================================
                HOW TO TEST API?
                ===================================================

                Step 1:

                Click endpoint

                Step 2:

                Click "Try it out"

                Step 3:

                Click Execute

                Step 4:

                See Response



                ===================================================
                REAL WORLD ANALOGY:
                ===================================================

                Swagger is like:

                Restaurant Menu Card

                Without Menu → Don't know items

                Swagger → API Menu



                ===================================================
                IMPORTANT:

                Swagger usually enabled only in Development

                Not in Production

                Security reason

                ===================================================

                */

            }

            #endregion



            #region 🔹 06. Middleware Pipeline

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            #endregion



            #region 🔹 07. Run Application

            /*
             Starts the application

             Application ready to receive HTTP requests


             After run:

             Open browser:

             https://localhost:5001/swagger

            */

            app.Run();

            #endregion


        }
    }
}
