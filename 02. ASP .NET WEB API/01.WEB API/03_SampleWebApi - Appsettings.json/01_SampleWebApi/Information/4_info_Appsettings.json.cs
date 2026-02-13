namespace _01_SampleWebApi.Information
{
    public class _4_info_Appsettings
    {
        #region =========================================================
        #region 01. What is appsettings.json in ASP.NET Core Web API?
        /*
            appsettings.json is the main configuration file in ASP.NET Core Web API.

            It is used to store:

            • Database Connection Strings
            • Logging settings
            • API Keys
            • Custom application settings
            • Third-party service configurations

            Example:

            {
                "ConnectionStrings": {
                    "DefaultConnection": "Server=.;Database=MyDb;Trusted_Connection=True;"
                },
                "Logging": {
                    "LogLevel": {
                        "Default": "Information",
                        "Microsoft.AspNetCore": "Warning"
                    }
                },
                "MySettings": {
                    "AppName": "My Web API",
                    "Version": "1.0"
                }
            }

            This file is automatically loaded when the application starts.
        */
        #endregion
        #endregion



        #region =========================================================
        #region 02. How ASP.NET Core Loads Configuration
        /*
            In .NET 6+ (Minimal Hosting Model):

                var builder = WebApplication.CreateBuilder(args);

            The builder.Configuration automatically loads configuration from multiple sources.

            You can access values like this:

                builder.Configuration["ConnectionStrings:DefaultConnection"];

            Nested values use ":" (colon) separator.
        */
        #endregion
        #endregion



        #region =========================================================
        #region 03. Different Configuration Sources in ASP.NET Core
        /*
            ASP.NET Core supports multiple configuration sources.

            IMPORTANT:
            Later sources OVERRIDE earlier ones.
            (Last loaded wins)
        */
        #endregion
        #endregion



        #region =========================================================
        #region 04. appsettings.json (Default Configuration)
        /*
            • Main configuration file
            • Always loaded
            • Used for default settings
            • Lowest priority among override sources
        */
        #endregion
        #endregion



        #region =========================================================
        #region 05. appsettings.{Environment}.json
        /*
            Example:
                appsettings.Development.json
                appsettings.Production.json
                appsettings.Staging.json

            These files override values from appsettings.json
            based on the current environment.

            Environment is controlled using:

                ASPNETCORE_ENVIRONMENT=Development

            If environment is Development,
            appsettings.Development.json overrides appsettings.json.
        */
        #endregion
        #endregion



        #region =========================================================
        #region 06. User Secrets (Development Only)
        /*
            Used to store sensitive data during development.

            Example:
                API Keys
                Passwords

            Commands:

                dotnet user-secrets init
                dotnet user-secrets set "ApiKey" "SuperSecretKey"

            • Only works in Development environment
            • Overrides appsettings.json
        */
        #endregion
        #endregion



        #region =========================================================
        #region 07. Environment Variables
        /*
            Configuration values can come from OS Environment Variables.

            Example (Windows):

                setx ConnectionStrings__DefaultConnection "ProdConnectionString"

            NOTE:
                ":" is replaced with "__" (double underscore)

            Environment variables override:
                • appsettings.json
                • appsettings.Environment.json
                • User Secrets
        */
        #endregion
        #endregion



        #region =========================================================
        #region 08. Command-Line Arguments
        /*
            Configuration values can be passed when running the application.

            Example:

                dotnet run --ConnectionStrings:DefaultConnection="AnotherConnection"

            Command-line arguments have the HIGHEST priority.
            They override all other configuration sources.
        */
        #endregion
        #endregion



        #region =========================================================
        #region 09. In-Memory Configuration
        /*
            Configuration values can be added manually in Program.cs:

                builder.Configuration.AddInMemoryCollection(
                    new Dictionary<string, string>
                    {
                        { "MySettings:AppName", "CustomApp" }
                    });

            Used mainly for:
                • Testing
                • Dynamic configuration
        */
        #endregion
        #endregion



        #region =========================================================
        #region 10. Azure Key Vault (Production Secrets)
        /*
            In production environments, sensitive data can be stored in Azure Key Vault.

            This is more secure than storing secrets in:
                • appsettings.json
                • Environment Variables

            Common in cloud deployments.
        */
        #endregion
        #endregion



        #region =========================================================
        #region 11. Configuration Order of Precedence (Very Important)
        /*
            From LOW priority → HIGH priority:

            1) appsettings.json
            2) appsettings.{Environment}.json
            3) User Secrets
            4) Environment Variables
            5) Command-line arguments  (Highest)

            Rule:
                Last loaded source overrides previous ones.

            Example:

                appsettings.json:
                    "ApiKey": "DefaultKey"

                Environment Variable:
                    ApiKey = "ProductionKey"

                Final value used:
                    "ProductionKey"
        */
        #endregion
        #endregion



        #region =========================================================
        #region 12. Why ASP.NET Core Configuration is Powerful
        /*
            • Easy environment switching (Dev / QA / Prod)
            • Secure secret management
            • No code changes required between environments
            • Cloud-friendly design
            • Highly flexible and extendable
        */
        #endregion
        #endregion

    }
}
