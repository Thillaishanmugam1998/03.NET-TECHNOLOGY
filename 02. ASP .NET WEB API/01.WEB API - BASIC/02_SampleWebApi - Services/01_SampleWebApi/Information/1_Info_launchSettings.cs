/* 
   info_lanuchSettings.json => This file defines what is lauchsettings.json and how it is used in a .NET project. 
   Created by : [Author] Not System, Just understand for you to learn and understand the concept of launchsettings.json in .NET projects.
*/


{
  // lanunchsettings.json is a configuration file used in .NET projects to define how the application should be launched during development. // It contains settings for different launch profiles, which can specify various parameters such as the command to run, environment variables, and application URLs.
  // JSON Schema reference
  // Used by IDE for IntelliSense and validation
  "$schema": "https://json.schemastore.org/launchsettings.json",

  // Profiles define how the application should launch
  "profiles": {

    // Profile name (You can create multiple profiles like http, https, IISExpress)
    "http": {

      // Specifies how the project should be launched
      // "Project" means run using Kestrel server
      // Other option: "IISExpress"
      "commandName": "Project",

      // If true → Shows application start messages in console
      // Example: Now listening on: http://localhost:5155
      "dotnetRunMessages": true,

      // If true → Automatically opens browser after application starts
      // false → Browser will NOT open automatically
      "launchBrowser": false,

      // Defines application URL and Port number
      // localhost → Your local machine
      // 5155 → Port number (must be unique)
      "applicationUrl": "http://localhost:5155",

      // Environment variables used while running application
      "environmentVariables": {

        // Defines the environment mode of the application
        // Possible values:
        // Development → Detailed error messages
        // Staging → Testing environment
        // Production → Live environment
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
