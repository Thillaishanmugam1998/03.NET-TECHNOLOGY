{
      // Defines schema for validation & IntelliSense in Visual Studio
      "$schema": "https://json.schemastore.org/launchsettings.json",

      // Settings specifically for IIS Express
      "iisSettings": {

            // If true → Uses Windows login authentication
            // Mostly used in corporate/internal apps
            "windowsAuthentication": false,

            // If true → Allows anonymous users to access app
            // For normal Web API development, this is true
            "anonymousAuthentication": true,

            // Configuration for IIS Express server
            "iisExpress": {

                    // The URL where IIS Express will host the application
                    // Only used when running with IIS Express
                    // ← IIS Express uses THIS
                    "applicationUrl": "http://localhost:5155", 

                    // HTTPS secure port for IIS Express
                    // If 0 → HTTPS disabled
                    "sslPort": 44388


            }
    },

      // Profiles section → What appears in Visual Studio Run dropdown
      "profiles": {

            // IIS Express Profile
            "IIS Express": {

                            // Tells Visual Studio to launch IIS Express
                            "commandName": "IISExpress",

                            // Automatically open browser when app starts
                            "launchBrowser": true,

                            // Environment variables for this profile
                            "environmentVariables": 
                            {
                                // Determines which appsettings file loads
                                // Loads:
                                // appsettings.json
                                // appsettings.Development.json
                                "ASPNETCORE_ENVIRONMENT": "Development"
                            }

                            // "applicationUrl": "http://localhost:5155",  
                            // ❌ NO applicationUrl here - it's ignored
                            // IIS Express reads from iisSettings.iisExpress above
        },

            // HTTP Profile (Runs using Kestrel directly)
            "http": {

                    // Tells Visual Studio to run the project directly
                    // This starts Kestrel server
                    "commandName": "Project",

                    // Shows extra startup messages in console
                    "dotnetRunMessages": true,

                    // Automatically open browser
                    "launchBrowser": true,

                    // URL where Kestrel will listen
                    // Used ONLY for Project profile
                    "applicationUrl": "http://localhost:5000",

                    // Environment settings for this profile
                    "environmentVariables": 
                    {
                        // Loads Development configuration
                        "ASPNETCORE_ENVIRONMENT": "Development"
                    }
            }

      }
}
