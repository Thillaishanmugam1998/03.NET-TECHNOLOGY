/*
 * ================================================================================
 * ASP.NET CORE DEPLOYMENT CONCEPTS - COMPLETE LEARNING GUIDE
 * ================================================================================
 * 
 * This file contains comprehensive explanations and examples for understanding
 * ASP.NET Core deployment concepts including:
 * - Kestrel Web Server Process Name
 * - Self-Contained Deployment (SCD)
 * - Framework-Dependent Deployment (FDD)
 * - Publishing in ASP.NET Core
 * - Step-by-Step Publishing in Visual Studio
 * - Why Publishing is Needed
 * 
 * Perfect for beginners learning ASP.NET Core deployment!
 * ================================================================================
 */

using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ASPNETCore.DeploymentGuide
{
    #region 1. KESTREL WEB SERVER - PROCESS NAME

    /*
     * ============================================================================
     * WHAT IS KESTREL?
     * ============================================================================
     * 
     * Kestrel is the default, cross-platform web server for ASP.NET Core.
     * It's built into ASP.NET Core and can run on Windows, Linux, and macOS.
     * 
     * PROCESS NAME OF KESTREL:
     * ------------------------
     * The process name depends on how your application is deployed:
     * 
     * 1. DURING DEVELOPMENT:
     *    - Process Name: "dotnet.exe" (Windows) or "dotnet" (Linux/Mac)
     *    - Command: dotnet run
     *    - Kestrel runs as part of the dotnet CLI process
     * 
     * 2. FRAMEWORK-DEPENDENT DEPLOYMENT (FDD):
     *    - Process Name: "dotnet.exe" (Windows) or "dotnet" (Linux/Mac)
     *    - Command: dotnet YourApp.dll
     *    - Still requires .NET Runtime installed on the server
     * 
     * 3. SELF-CONTAINED DEPLOYMENT (SCD):
     *    - Process Name: "YourAppName.exe" (Windows) or "./YourAppName" (Linux/Mac)
     *    - Command: YourApp.exe (Windows) or ./YourApp (Linux/Mac)
     *    - Runs as a standalone executable with embedded .NET Runtime
     * 
     * EXAMPLE: Checking Kestrel Process Name Programmatically
     */

    public class KestrelProcessInfo
    {
        public static void DisplayKestrelProcessInformation()
        {
            // Get the current process (the running application)
            Process currentProcess = Process.GetCurrentProcess();

            Console.WriteLine("=== KESTREL WEB SERVER PROCESS INFORMATION ===");
            Console.WriteLine($"Process Name: {currentProcess.ProcessName}");
            Console.WriteLine($"Process ID: {currentProcess.Id}");
            Console.WriteLine($"Process Path: {currentProcess.MainModule?.FileName}");
            Console.WriteLine($"Working Directory: {Environment.CurrentDirectory}");
            Console.WriteLine($"Is 64-bit Process: {Environment.Is64BitProcess}");
            Console.WriteLine($"Operating System: {Environment.OSVersion}");
            Console.WriteLine("===============================================\n");

            /*
             * EXPECTED OUTPUT EXAMPLES:
             * 
             * Development (dotnet run):
             * - Process Name: dotnet
             * - Process Path: C:\Program Files\dotnet\dotnet.exe
             * 
             * Framework-Dependent Deployment:
             * - Process Name: dotnet
             * - Process Path: C:\Program Files\dotnet\dotnet.exe
             * 
             * Self-Contained Deployment:
             * - Process Name: MyWebApp
             * - Process Path: C:\MyApp\MyWebApp.exe
             */
        }
    }

    #endregion

    #region 2. SELF-CONTAINED DEPLOYMENT (SCD)

    /*
     * ============================================================================
     * SELF-CONTAINED DEPLOYMENT (SCD)
     * ============================================================================
     * 
     * DEFINITION:
     * -----------
     * A deployment model where your application is published with the .NET Runtime
     * and all required libraries bundled together. The target machine does NOT
     * need .NET Runtime pre-installed.
     * 
     * CHARACTERISTICS:
     * ----------------
     * ✓ Includes .NET Runtime in the deployment package
     * ✓ Runs independently without requiring .NET installation on target
     * ✓ Larger deployment size (60-100+ MB depending on trimming)
     * ✓ You control the exact .NET version your app uses
     * ✓ Different apps can use different .NET versions on same machine
     * ✓ Creates platform-specific executables (.exe for Windows)
     * 
     * ADVANTAGES:
     * -----------
     * 1. No dependency on installed .NET Runtime
     * 2. Guaranteed compatibility (your tested runtime version)
     * 3. Side-by-side deployment of different .NET versions
     * 4. Isolated from system-wide .NET updates
     * 
     * DISADVANTAGES:
     * --------------
     * 1. Larger deployment package size
     * 2. More disk space required
     * 3. No automatic security updates from system .NET Runtime
     * 4. Need separate builds for each platform (Windows/Linux/macOS)
     * 
     * HOW TO CREATE SCD:
     * ------------------
     */

    public class SelfContainedDeploymentExample
    {
        /*
         * COMMAND LINE PUBLISHING (SCD):
         * ------------------------------
         * 
         * For Windows 64-bit:
         * dotnet publish -c Release -r win-x64 --self-contained true
         * 
         * For Linux 64-bit:
         * dotnet publish -c Release -r linux-x64 --self-contained true
         * 
         * For macOS 64-bit (ARM):
         * dotnet publish -c Release -r osx-arm64 --self-contained true
         * 
         * With Trimming (Reduce Size):
         * dotnet publish -c Release -r win-x64 --self-contained true /p:PublishTrimmed=true
         * 
         * With Single File:
         * dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
         */

        /*
         * PROJECT FILE CONFIGURATION (.csproj):
         * -------------------------------------
         * 
         * Add these properties to your .csproj file:
         * 
         * <PropertyGroup>
         *   <TargetFramework>net8.0</TargetFramework>
         *   <RuntimeIdentifier>win-x64</RuntimeIdentifier>
         *   <SelfContained>true</SelfContained>
         *   <PublishSingleFile>true</PublishSingleFile>
         *   <PublishTrimmed>true</PublishTrimmed>
         * </PropertyGroup>
         */

        public static void ExplainSCDOutputStructure()
        {
            Console.WriteLine("=== SELF-CONTAINED DEPLOYMENT OUTPUT ===");
            Console.WriteLine("\nTypical SCD folder structure:");
            Console.WriteLine("publish/");
            Console.WriteLine("  ├── YourApp.exe                  (Your executable - Windows)");
            Console.WriteLine("  ├── YourApp.dll                  (Your application code)");
            Console.WriteLine("  ├── YourApp.pdb                  (Debug symbols)");
            Console.WriteLine("  ├── appsettings.json             (Configuration files)");
            Console.WriteLine("  ├── web.config                   (IIS configuration)");
            Console.WriteLine("  ├── hostfxr.dll                  (Part of .NET Runtime)");
            Console.WriteLine("  ├── hostpolicy.dll               (Part of .NET Runtime)");
            Console.WriteLine("  ├── System.*.dll                 (Framework libraries)");
            Console.WriteLine("  ├── Microsoft.*.dll              (Framework libraries)");
            Console.WriteLine("  └── ... (Many more runtime DLLs)");
            Console.WriteLine("\nTotal Size: 60-100+ MB (can be reduced with trimming)");
            Console.WriteLine("=========================================\n");
        }

        /*
         * WHEN TO USE SCD:
         * ----------------
         * ✓ Deploying to servers without .NET Runtime installed
         * ✓ Need specific .NET version guarantees
         * ✓ Want isolation from system updates
         * ✓ Creating portable applications
         * ✓ Docker containers (though FDD is also common)
         */
    }

    #endregion

    #region 3. FRAMEWORK-DEPENDENT DEPLOYMENT (FDD)

    /*
     * ============================================================================
     * FRAMEWORK-DEPENDENT DEPLOYMENT (FDD)
     * ============================================================================
     * 
     * DEFINITION:
     * -----------
     * A deployment model where your application is published WITHOUT the .NET
     * Runtime. The target machine MUST have the .NET Runtime installed separately.
     * 
     * CHARACTERISTICS:
     * ----------------
     * ✓ Does NOT include .NET Runtime in deployment
     * ✓ Requires .NET Runtime pre-installed on target machine
     * ✓ Smaller deployment size (typically 5-20 MB)
     * ✓ Uses shared .NET Runtime from the system
     * ✓ Cross-platform compatible (same DLL runs on Windows/Linux/macOS)
     * ✓ Benefits from system-wide .NET security updates
     * 
     * ADVANTAGES:
     * -----------
     * 1. Much smaller deployment package
     * 2. Single deployment works across platforms
     * 3. Automatic security updates through system .NET updates
     * 4. Less disk space required
     * 5. Faster deployment and updates
     * 
     * DISADVANTAGES:
     * --------------
     * 1. Requires .NET Runtime on target machine
     * 2. Dependency on compatible runtime version
     * 3. System updates might affect your app
     * 4. Cannot control exact runtime version
     * 
     * HOW TO CREATE FDD:
     * ------------------
     */

    public class FrameworkDependentDeploymentExample
    {
        /*
         * COMMAND LINE PUBLISHING (FDD):
         * ------------------------------
         * 
         * Default (Framework-Dependent):
         * dotnet publish -c Release
         * 
         * Explicit Framework-Dependent:
         * dotnet publish -c Release --self-contained false
         * 
         * With Runtime Identifier (creates platform-specific FDD):
         * dotnet publish -c Release -r win-x64 --self-contained false
         */

        /*
         * PROJECT FILE CONFIGURATION (.csproj):
         * -------------------------------------
         * 
         * Default FDD (no special configuration needed):
         * 
         * <PropertyGroup>
         *   <TargetFramework>net8.0</TargetFramework>
         * </PropertyGroup>
         * 
         * Explicit FDD:
         * 
         * <PropertyGroup>
         *   <TargetFramework>net8.0</TargetFramework>
         *   <SelfContained>false</SelfContained>
         * </PropertyGroup>
         */

        public static void ExplainFDDOutputStructure()
        {
            Console.WriteLine("=== FRAMEWORK-DEPENDENT DEPLOYMENT OUTPUT ===");
            Console.WriteLine("\nTypical FDD folder structure:");
            Console.WriteLine("publish/");
            Console.WriteLine("  ├── YourApp.dll                  (Your application code)");
            Console.WriteLine("  ├── YourApp.pdb                  (Debug symbols)");
            Console.WriteLine("  ├── YourApp.deps.json            (Dependencies manifest)");
            Console.WriteLine("  ├── YourApp.runtimeconfig.json   (Runtime configuration)");
            Console.WriteLine("  ├── appsettings.json             (Configuration files)");
            Console.WriteLine("  ├── web.config                   (IIS configuration)");
            Console.WriteLine("  └── ThirdParty.dll               (NuGet package DLLs)");
            Console.WriteLine("\nTotal Size: 5-20 MB (much smaller!)");
            Console.WriteLine("\nREQUIRED ON TARGET:");
            Console.WriteLine("  • .NET Runtime (same version or compatible)");
            Console.WriteLine("  • Run with: dotnet YourApp.dll");
            Console.WriteLine("==============================================\n");
        }

        /*
         * WHEN TO USE FDD:
         * ----------------
         * ✓ Deploying to servers with .NET Runtime already installed
         * ✓ Azure App Service, AWS Elastic Beanstalk (managed platforms)
         * ✓ Corporate environments with standardized .NET installations
         * ✓ Want smaller deployment packages
         * ✓ Want automatic security updates from system
         * ✓ Cross-platform scenarios with same binary
         */
    }

    #endregion

    #region 4. SCD vs FDD - COMPARISON TABLE

    /*
     * ============================================================================
     * SELF-CONTAINED vs FRAMEWORK-DEPENDENT DEPLOYMENT - COMPARISON
     * ============================================================================
     * 
     * ┌──────────────────────────┬────────────────────────┬─────────────────────────┐
     * │ Feature                  │ Self-Contained (SCD)   │ Framework-Dependent (FDD)│
     * ├──────────────────────────┼────────────────────────┼─────────────────────────┤
     * │ .NET Runtime Included    │ YES ✓                  │ NO ✗                    │
     * │ Deployment Size          │ 60-100+ MB             │ 5-20 MB                 │
     * │ Runtime Required         │ NO ✗                   │ YES ✓                   │
     * │ Process Name             │ YourApp.exe            │ dotnet.exe              │
     * │ Cross-Platform Binary    │ NO (platform-specific) │ YES ✓                   │
     * │ Version Control          │ Full control           │ System dependent        │
     * │ Security Updates         │ Manual                 │ Automatic (system)      │
     * │ Disk Space               │ More                   │ Less                    │
     * │ Deployment Command       │ ./YourApp.exe          │ dotnet YourApp.dll      │
     * └──────────────────────────┴────────────────────────┴─────────────────────────┘
     * 
     * DECISION GUIDE:
     * ---------------
     * Choose SCD if:
     *   • Target machine doesn't have .NET Runtime
     *   • Need specific .NET version guarantee
     *   • Want complete isolation
     *   • Creating standalone tools/utilities
     * 
     * Choose FDD if:
     *   • Target has .NET Runtime installed
     *   • Deploying to managed platforms (Azure, AWS)
     *   • Want smaller deployment size
     *   • Need cross-platform single binary
     *   • Corporate environment with standard .NET
     */

    #endregion

    #region 5. WHAT IS PUBLISH IN ASP.NET CORE?

    /*
     * ============================================================================
     * WHAT IS PUBLISH IN ASP.NET CORE?
     * ============================================================================
     * 
     * DEFINITION:
     * -----------
     * Publishing is the process of compiling your ASP.NET Core application and
     * preparing all necessary files for deployment to a production environment.
     * It creates an optimized, production-ready version of your application.
     * 
     * WHAT HAPPENS DURING PUBLISH:
     * ----------------------------
     * 
     * 1. COMPILATION:
     *    - Source code (.cs files) → Compiled to IL (Intermediate Language)
     *    - Creates optimized Release build (not Debug)
     *    - Removes unnecessary debug information
     * 
     * 2. DEPENDENCY RESOLUTION:
     *    - Collects all NuGet package dependencies
     *    - Copies required third-party libraries
     *    - Resolves version conflicts
     * 
     * 3. ASSET PREPARATION:
     *    - Copies static files (wwwroot folder contents)
     *    - Includes configuration files (appsettings.json)
     *    - Bundles and minifies JavaScript/CSS (if configured)
     * 
     * 4. RUNTIME PACKAGING (if SCD):
     *    - Includes .NET Runtime libraries
     *    - Platform-specific native binaries
     *    - Creates executable file
     * 
     * 5. OPTIMIZATION:
     *    - IL trimming (removes unused code)
     *    - ReadyToRun compilation (ahead-of-time compilation)
     *    - Single-file packaging (optional)
     *    - Compression (optional)
     * 
     * 6. OUTPUT GENERATION:
     *    - Creates publish folder with all deployment files
     *    - Generates web.config for IIS (Windows)
     *    - Creates runtime configuration files
     */

    public class PublishExplanation
    {
        public static void ExplainPublishProcess()
        {
            Console.WriteLine("=== ASP.NET CORE PUBLISH PROCESS ===\n");

            Console.WriteLine("BEFORE PUBLISH (Development):");
            Console.WriteLine("  YourProject/");
            Console.WriteLine("    ├── Controllers/");
            Console.WriteLine("    ├── Models/");
            Console.WriteLine("    ├── Views/");
            Console.WriteLine("    ├── wwwroot/");
            Console.WriteLine("    ├── appsettings.json");
            Console.WriteLine("    ├── Program.cs");
            Console.WriteLine("    └── YourProject.csproj");
            Console.WriteLine();

            Console.WriteLine("PUBLISH COMMAND:");
            Console.WriteLine("  dotnet publish -c Release -o ./publish");
            Console.WriteLine();

            Console.WriteLine("AFTER PUBLISH (Production-Ready):");
            Console.WriteLine("  publish/");
            Console.WriteLine("    ├── YourProject.dll         (Compiled application)");
            Console.WriteLine("    ├── YourProject.exe         (If SCD)");
            Console.WriteLine("    ├── appsettings.json        (Configuration)");
            Console.WriteLine("    ├── appsettings.Production.json");
            Console.WriteLine("    ├── web.config              (For IIS)");
            Console.WriteLine("    ├── wwwroot/                (Static files)");
            Console.WriteLine("    │   ├── css/");
            Console.WriteLine("    │   ├── js/");
            Console.WriteLine("    │   └── images/");
            Console.WriteLine("    └── [Runtime files]         (If SCD)");
            Console.WriteLine();

            Console.WriteLine("READY FOR DEPLOYMENT! ✓");
            Console.WriteLine("=====================================\n");
        }

        /*
         * PUBLISH COMMANDS - COMMON SCENARIOS:
         * -------------------------------------
         */

        public static void ShowCommonPublishCommands()
        {
            Console.WriteLine("=== COMMON PUBLISH COMMANDS ===\n");

            Console.WriteLine("1. Basic Publish (FDD):");
            Console.WriteLine("   dotnet publish -c Release\n");

            Console.WriteLine("2. Publish to Specific Folder:");
            Console.WriteLine("   dotnet publish -c Release -o C:\\Deploy\\MyApp\n");

            Console.WriteLine("3. Self-Contained for Windows:");
            Console.WriteLine("   dotnet publish -c Release -r win-x64 --self-contained true\n");

            Console.WriteLine("4. Self-Contained with Single File:");
            Console.WriteLine("   dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true\n");

            Console.WriteLine("5. Optimized Publish (Trimmed + AOT):");
            Console.WriteLine("   dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishTrimmed=true /p:PublishReadyToRun=true\n");

            Console.WriteLine("6. Publish for Docker:");
            Console.WriteLine("   dotnet publish -c Release -o ./publish\n");

            Console.WriteLine("=====================================\n");
        }
    }

    #endregion

    #region 6. WHY IS PUBLISHING NEEDED?

    /*
     * ============================================================================
     * WHY IS PUBLISHING NEEDED?
     * ============================================================================
     * 
     * Publishing is essential for several critical reasons:
     */

    public class WhyPublishIsNeeded
    {
        /*
         * REASON 1: OPTIMIZATION FOR PRODUCTION
         * --------------------------------------
         * Development builds include debugging overhead that slows performance.
         * Publishing creates optimized Release builds.
         */
        public static void Reason1_Optimization()
        {
            Console.WriteLine("REASON 1: OPTIMIZATION");
            Console.WriteLine("----------------------");
            Console.WriteLine("Debug Build (Development):");
            Console.WriteLine("  • Includes debug symbols (.pdb files)");
            Console.WriteLine("  • No compiler optimizations");
            Console.WriteLine("  • Detailed error information");
            Console.WriteLine("  • Slower execution");
            Console.WriteLine("  • Larger file sizes\n");

            Console.WriteLine("Release Build (Published):");
            Console.WriteLine("  • Optimized IL code");
            Console.WriteLine("  • Minimal debug information");
            Console.WriteLine("  • Faster execution (20-50% improvement)");
            Console.WriteLine("  • Smaller file sizes");
            Console.WriteLine("  • Production-ready error handling\n");
        }

        /*
         * REASON 2: DEPENDENCY RESOLUTION
         * --------------------------------
         * Collects all required DLLs and packages in one location.
         */
        public static void Reason2_Dependencies()
        {
            Console.WriteLine("REASON 2: DEPENDENCY RESOLUTION");
            Console.WriteLine("--------------------------------");
            Console.WriteLine("Development:");
            Console.WriteLine("  • Dependencies scattered in NuGet cache");
            Console.WriteLine("  • References to local packages");
            Console.WriteLine("  • May have unnecessary dev dependencies\n");

            Console.WriteLine("Published:");
            Console.WriteLine("  • All runtime dependencies in one folder");
            Console.WriteLine("  • Only production dependencies included");
            Console.WriteLine("  • Self-contained and portable");
            Console.WriteLine("  • Easy to deploy as a unit\n");
        }

        /*
         * REASON 3: CONFIGURATION TRANSFORMATION
         * --------------------------------------
         * Applies production-specific settings.
         */
        public static void Reason3_Configuration()
        {
            Console.WriteLine("REASON 3: CONFIGURATION");
            Console.WriteLine("-----------------------");
            Console.WriteLine("Development (appsettings.Development.json):");
            Console.WriteLine("  • Debug logging enabled");
            Console.WriteLine("  • Detailed error pages");
            Console.WriteLine("  • Local database connections");
            Console.WriteLine("  • Development API keys\n");

            Console.WriteLine("Published (appsettings.Production.json):");
            Console.WriteLine("  • Error logging only");
            Console.WriteLine("  • Generic error pages");
            Console.WriteLine("  • Production database connections");
            Console.WriteLine("  • Production API keys");
            Console.WriteLine("  • Security hardening applied\n");
        }

        /*
         * REASON 4: SECURITY
         * ------------------
         * Removes sensitive development files and information.
         */
        public static void Reason4_Security()
        {
            Console.WriteLine("REASON 4: SECURITY");
            Console.WriteLine("------------------");
            Console.WriteLine("Excluded from Publish:");
            Console.WriteLine("  • Source code (.cs files)");
            Console.WriteLine("  • User secrets");
            Console.WriteLine("  • Development certificates");
            Console.WriteLine("  • Test files");
            Console.WriteLine("  • Detailed stack traces\n");

            Console.WriteLine("Included in Publish:");
            Console.WriteLine("  • Compiled code only (.dll)");
            Console.WriteLine("  • Production configurations");
            Console.WriteLine("  • Required runtime files");
            Console.WriteLine("  • Obfuscated assemblies (optional)\n");
        }

        /*
         * REASON 5: SIZE OPTIMIZATION
         * ---------------------------
         * Removes unnecessary files and optimizes for deployment.
         */
        public static void Reason5_SizeOptimization()
        {
            Console.WriteLine("REASON 5: SIZE OPTIMIZATION");
            Console.WriteLine("---------------------------");
            Console.WriteLine("Development Folder: ~500 MB");
            Console.WriteLine("  • All NuGet packages");
            Console.WriteLine("  • Debug symbols");
            Console.WriteLine("  • Build artifacts");
            Console.WriteLine("  • Test projects");
            Console.WriteLine("  • Source control files\n");

            Console.WriteLine("Published Folder (FDD): ~10 MB");
            Console.WriteLine("Published Folder (SCD): ~70 MB (with trimming)");
            Console.WriteLine("  • Only runtime dependencies");
            Console.WriteLine("  • Trimmed unused code");
            Console.WriteLine("  • Compressed assets");
            Console.WriteLine("  • Optimized binaries\n");
        }

        /*
         * REASON 6: DEPLOYMENT READINESS
         * ------------------------------
         * Creates a standardized package ready for any hosting environment.
         */
        public static void Reason6_DeploymentReadiness()
        {
            Console.WriteLine("REASON 6: DEPLOYMENT READINESS");
            Console.WriteLine("-------------------------------");
            Console.WriteLine("Published output can be deployed to:");
            Console.WriteLine("  ✓ IIS (Internet Information Services)");
            Console.WriteLine("  ✓ Nginx/Apache (Linux web servers)");
            Console.WriteLine("  ✓ Docker containers");
            Console.WriteLine("  ✓ Azure App Service");
            Console.WriteLine("  ✓ AWS Elastic Beanstalk");
            Console.WriteLine("  ✓ Google Cloud Platform");
            Console.WriteLine("  ✓ Kubernetes clusters");
            Console.WriteLine("  ✓ On-premises servers");
            Console.WriteLine("  ✓ Any hosting environment!\n");
        }

        /*
         * WHAT HAPPENS WITHOUT PUBLISHING?
         * ---------------------------------
         */
        public static void WithoutPublishing()
        {
            Console.WriteLine("WITHOUT PUBLISHING (Copying bin/Debug folder):");
            Console.WriteLine("-----------------------------------------------");
            Console.WriteLine("❌ Includes debug overhead (slow performance)");
            Console.WriteLine("❌ Missing runtime configuration");
            Console.WriteLine("❌ Broken dependency paths");
            Console.WriteLine("❌ No production optimizations");
            Console.WriteLine("❌ Exposes source code structure");
            Console.WriteLine("❌ Larger deployment size");
            Console.WriteLine("❌ May not run on target server");
            Console.WriteLine("❌ Security vulnerabilities exposed");
            Console.WriteLine("\nCONCLUSION: Always publish before deployment! ✓\n");
        }
    }

    #endregion

    #region 7. HOW TO PUBLISH IN VISUAL STUDIO (STEP BY STEP)

    /*
     * ============================================================================
     * HOW TO PUBLISH CODE IN VISUAL STUDIO - STEP BY STEP GUIDE
     * ============================================================================
     * 
     * This section provides detailed steps for publishing an ASP.NET Core
     * application using Visual Studio's publish features.
     */

    public class VisualStudioPublishGuide
    {
        /*
         * METHOD 1: PUBLISH TO FOLDER
         * ---------------------------
         * Most common method for manual deployment.
         */
        public static void Method1_PublishToFolder()
        {
            Console.WriteLine("=== METHOD 1: PUBLISH TO FOLDER ===\n");

            Console.WriteLine("STEP 1: Open Your Project");
            Console.WriteLine("  • Launch Visual Studio");
            Console.WriteLine("  • Open your ASP.NET Core project (.csproj or .sln)\n");

            Console.WriteLine("STEP 2: Right-Click Project");
            Console.WriteLine("  • In Solution Explorer");
            Console.WriteLine("  • Right-click your project name (not solution)");
            Console.WriteLine("  • Select 'Publish...' from context menu\n");

            Console.WriteLine("STEP 3: Choose Publish Target");
            Console.WriteLine("  • Select 'Folder' from the options");
            Console.WriteLine("  • Click 'Next'\n");

            Console.WriteLine("STEP 4: Configure Folder Location");
            Console.WriteLine("  • Default: bin\\Release\\net8.0\\publish\\");
            Console.WriteLine("  • Or click 'Browse' to choose custom location");
            Console.WriteLine("  • Example: C:\\Deploy\\MyWebApp\\");
            Console.WriteLine("  • Click 'Finish'\n");

            Console.WriteLine("STEP 5: Configure Publish Settings");
            Console.WriteLine("  • Click 'Show all settings' link");
            Console.WriteLine("  • Configuration: Release (not Debug!)");
            Console.WriteLine("  • Target Framework: net8.0 (or your version)");
            Console.WriteLine("  • Target Runtime: (choose based on deployment type)");
            Console.WriteLine("    - 'Portable' = Framework-Dependent Deployment (FDD)");
            Console.WriteLine("    - 'win-x64' = Self-Contained for Windows 64-bit");
            Console.WriteLine("    - 'linux-x64' = Self-Contained for Linux 64-bit");
            Console.WriteLine("  • Deployment Mode:");
            Console.WriteLine("    - 'Framework-dependent' = FDD (smaller size)");
            Console.WriteLine("    - 'Self-contained' = SCD (includes runtime)");
            Console.WriteLine("  • File Publish Options:");
            Console.WriteLine("    ☑ Delete existing files (recommended)");
            Console.WriteLine("    ☑ Produce single file (optional, for SCD)");
            Console.WriteLine("    ☐ Enable ReadyToRun (faster startup)");
            Console.WriteLine("  • Click 'Save'\n");

            Console.WriteLine("STEP 6: Publish");
            Console.WriteLine("  • Click the 'Publish' button");
            Console.WriteLine("  • Wait for build and publish to complete");
            Console.WriteLine("  • Output window shows progress\n");

            Console.WriteLine("STEP 7: Verify Published Files");
            Console.WriteLine("  • Navigate to publish folder");
            Console.WriteLine("  • Verify all files are present:");
            Console.WriteLine("    ✓ YourApp.dll (main application)");
            Console.WriteLine("    ✓ YourApp.exe (if SCD)");
            Console.WriteLine("    ✓ appsettings.json");
            Console.WriteLine("    ✓ web.config");
            Console.WriteLine("    ✓ wwwroot folder");
            Console.WriteLine("    ✓ Dependencies (DLLs)\n");

            Console.WriteLine("STEP 8: Test Published Application (Optional)");
            Console.WriteLine("  • Framework-Dependent: dotnet YourApp.dll");
            Console.WriteLine("  • Self-Contained: YourApp.exe");
            Console.WriteLine("  • Open browser: https://localhost:5000\n");

            Console.WriteLine("STEP 9: Deploy to Server");
            Console.WriteLine("  • Copy entire publish folder to server");
            Console.WriteLine("  • Configure IIS/Nginx/Apache");
            Console.WriteLine("  • Update appsettings.Production.json");
            Console.WriteLine("  • Start application\n");

            Console.WriteLine("=====================================\n");
        }

        /*
         * METHOD 2: PUBLISH TO AZURE APP SERVICE
         * ---------------------------------------
         */
        public static void Method2_PublishToAzure()
        {
            Console.WriteLine("=== METHOD 2: PUBLISH TO AZURE ===\n");

            Console.WriteLine("STEP 1: Right-Click Project → Publish\n");

            Console.WriteLine("STEP 2: Choose Azure");
            Console.WriteLine("  • Select 'Azure' as target");
            Console.WriteLine("  • Click 'Next'\n");

            Console.WriteLine("STEP 3: Choose Azure Service");
            Console.WriteLine("  • Select 'Azure App Service (Windows)' or");
            Console.WriteLine("  • Select 'Azure App Service (Linux)'");
            Console.WriteLine("  • Click 'Next'\n");

            Console.WriteLine("STEP 4: Sign in to Azure");
            Console.WriteLine("  • Sign in with your Azure account");
            Console.WriteLine("  • Select subscription\n");

            Console.WriteLine("STEP 5: Create or Select App Service");
            Console.WriteLine("  • Option A: Select existing App Service");
            Console.WriteLine("  • Option B: Click 'Create New'");
            Console.WriteLine("    - Name: mywebapp-unique-name");
            Console.WriteLine("    - Resource Group: MyResourceGroup");
            Console.WriteLine("    - Hosting Plan: Choose or create");
            Console.WriteLine("  • Click 'Create' or 'Finish'\n");

            Console.WriteLine("STEP 6: Publish to Azure");
            Console.WriteLine("  • Click 'Publish' button");
            Console.WriteLine("  • Visual Studio deploys to Azure");
            Console.WriteLine("  • Browser opens with deployed app\n");

            Console.WriteLine("=====================================\n");
        }

        /*
         * METHOD 3: PUBLISH TO IIS
         * ------------------------
         */
        public static void Method3_PublishToIIS()
        {
            Console.WriteLine("=== METHOD 3: PUBLISH TO IIS ===\n");

            Console.WriteLine("STEP 1: Right-Click Project → Publish\n");

            Console.WriteLine("STEP 2: Choose IIS/FTP");
            Console.WriteLine("  • Select 'Web Server (IIS)'");
            Console.WriteLine("  • Click 'Next'\n");

            Console.WriteLine("STEP 3: Choose Deployment Method");
            Console.WriteLine("  • Select 'Web Deploy'");
            Console.WriteLine("  • Click 'Next'\n");

            Console.WriteLine("STEP 4: Configure Web Deploy");
            Console.WriteLine("  • Server: your-server.com");
            Console.WriteLine("  • Site name: Default Web Site/MyApp");
            Console.WriteLine("  • Username: deployment-user");
            Console.WriteLine("  • Password: ••••••••");
            Console.WriteLine("  • Destination URL: https://your-server.com");
            Console.WriteLine("  • Click 'Finish'\n");

            Console.WriteLine("STEP 5: Validate Connection");
            Console.WriteLine("  • Click 'Validate Connection'");
            Console.WriteLine("  • Ensure green checkmark appears\n");

            Console.WriteLine("STEP 6: Publish to IIS");
            Console.WriteLine("  • Click 'Publish' button");
            Console.WriteLine("  • Application deploys to IIS\n");

            Console.WriteLine("=====================================\n");
        }

        /*
         * METHOD 4: PUBLISH USING CLI (COMMAND LINE)
         * ------------------------------------------
         */
        public static void Method4_PublishViaCLI()
        {
            Console.WriteLine("=== METHOD 4: COMMAND LINE PUBLISH ===\n");

            Console.WriteLine("STEP 1: Open Terminal/Command Prompt");
            Console.WriteLine("  • Open CMD, PowerShell, or Terminal");
            Console.WriteLine("  • Navigate to project folder:");
            Console.WriteLine("    cd C:\\Projects\\MyWebApp\n");

            Console.WriteLine("STEP 2: Run Publish Command");
            Console.WriteLine("  • Basic publish:");
            Console.WriteLine("    dotnet publish -c Release\n");
            Console.WriteLine("  • Publish to specific folder:");
            Console.WriteLine("    dotnet publish -c Release -o C:\\Deploy\\MyApp\n");
            Console.WriteLine("  • Self-contained:");
            Console.WriteLine("    dotnet publish -c Release -r win-x64 --self-contained true\n");

            Console.WriteLine("STEP 3: Wait for Completion");
            Console.WriteLine("  • Build process runs");
            Console.WriteLine("  • See output: 'Publish succeeded'\n");

            Console.WriteLine("STEP 4: Files Ready in Output Folder");
            Console.WriteLine("  • Default: bin\\Release\\net8.0\\publish\\");
            Console.WriteLine("  • Or your specified -o folder\n");

            Console.WriteLine("=====================================\n");
        }

        /*
         * TROUBLESHOOTING COMMON PUBLISH ISSUES
         * --------------------------------------
         */
        public static void TroubleshootingTips()
        {
            Console.WriteLine("=== COMMON PUBLISH ISSUES & SOLUTIONS ===\n");

            Console.WriteLine("ISSUE 1: 'Publish Failed'");
            Console.WriteLine("Solution:");
            Console.WriteLine("  • Clean solution: Build → Clean Solution");
            Console.WriteLine("  • Rebuild: Build → Rebuild Solution");
            Console.WriteLine("  • Delete bin and obj folders manually");
            Console.WriteLine("  • Try publish again\n");

            Console.WriteLine("ISSUE 2: Missing Dependencies");
            Console.WriteLine("Solution:");
            Console.WriteLine("  • Restore NuGet packages:");
            Console.WriteLine("    dotnet restore");
            Console.WriteLine("  • Check .csproj for missing references");
            Console.WriteLine("  • Ensure all packages are compatible\n");

            Console.WriteLine("ISSUE 3: Application Won't Run After Publish");
            Console.WriteLine("Solution:");
            Console.WriteLine("  • Check target runtime matches server");
            Console.WriteLine("  • Ensure .NET Runtime installed (for FDD)");
            Console.WriteLine("  • Verify appsettings.json configuration");
            Console.WriteLine("  • Check file permissions on server\n");

            Console.WriteLine("ISSUE 4: 'File is Being Used'");
            Console.WriteLine("Solution:");
            Console.WriteLine("  • Stop the application if running");
            Console.WriteLine("  • Close all instances in Task Manager");
            Console.WriteLine("  • Use 'Delete existing files' option");
            Console.WriteLine("  • Publish to different folder\n");

            Console.WriteLine("==========================================\n");
        }
    }

    #endregion

    #region 8. PRACTICAL EXAMPLE - COMPLETE WORKFLOW

    /*
     * ============================================================================
     * COMPLETE PRACTICAL EXAMPLE
     * ============================================================================
     * 
     * A realistic scenario showing the entire workflow from development to
     * production deployment.
     */

    public class CompletePublishWorkflow
    {
        public static void ShowCompleteWorkflow()
        {
            Console.WriteLine("=== COMPLETE PUBLISH WORKFLOW EXAMPLE ===\n");

            Console.WriteLine("SCENARIO:");
            Console.WriteLine("You've built an ASP.NET Core MVC application and need to");
            Console.WriteLine("deploy it to a Windows Server 2022 production environment.\n");

            Console.WriteLine("─────────────────────────────────────────────────────────");
            Console.WriteLine("PHASE 1: DEVELOPMENT (Your Computer)");
            Console.WriteLine("─────────────────────────────────────────────────────────\n");

            Console.WriteLine("1. Build and test application:");
            Console.WriteLine("   dotnet run");
            Console.WriteLine("   → Runs on https://localhost:5001");
            Console.WriteLine("   → Uses appsettings.Development.json");
            Console.WriteLine("   → Debug mode, detailed errors\n");

            Console.WriteLine("2. Ensure all features work:");
            Console.WriteLine("   ✓ All pages load correctly");
            Console.WriteLine("   ✓ Database connections work");
            Console.WriteLine("   ✓ API endpoints respond");
            Console.WriteLine("   ✓ File uploads functional\n");

            Console.WriteLine("─────────────────────────────────────────────────────────");
            Console.WriteLine("PHASE 2: PREPARE FOR PRODUCTION");
            Console.WriteLine("─────────────────────────────────────────────────────────\n");

            Console.WriteLine("3. Update appsettings.Production.json:");
            Console.WriteLine("   {");
            Console.WriteLine("     \"ConnectionStrings\": {");
            Console.WriteLine("       \"DefaultConnection\": \"Server=prod-server;Database=ProdDB;...\"");
            Console.WriteLine("     },");
            Console.WriteLine("     \"Logging\": {");
            Console.WriteLine("       \"LogLevel\": { \"Default\": \"Warning\" }");
            Console.WriteLine("     }");
            Console.WriteLine("   }\n");

            Console.WriteLine("4. Review .csproj settings:");
            Console.WriteLine("   <PropertyGroup>");
            Console.WriteLine("     <TargetFramework>net8.0</TargetFramework>");
            Console.WriteLine("     <RuntimeIdentifier>win-x64</RuntimeIdentifier>");
            Console.WriteLine("     <SelfContained>true</SelfContained>");
            Console.WriteLine("   </PropertyGroup>\n");

            Console.WriteLine("─────────────────────────────────────────────────────────");
            Console.WriteLine("PHASE 3: PUBLISH");
            Console.WriteLine("─────────────────────────────────────────────────────────\n");

            Console.WriteLine("5. Open Visual Studio:");
            Console.WriteLine("   • Right-click project → Publish");
            Console.WriteLine("   • Select 'Folder'");
            Console.WriteLine("   • Choose: C:\\Deploy\\MyWebApp");
            Console.WriteLine("   • Configuration: Release");
            Console.WriteLine("   • Target Runtime: win-x64");
            Console.WriteLine("   • Deployment Mode: Self-contained");
            Console.WriteLine("   • Click 'Publish'\n");

            Console.WriteLine("6. Publish output:");
            Console.WriteLine("   Build started...");
            Console.WriteLine("   Restoring packages...");
            Console.WriteLine("   Compiling...");
            Console.WriteLine("   Publishing...");
            Console.WriteLine("   Publish succeeded! ✓");
            Console.WriteLine("   Time: 45 seconds");
            Console.WriteLine("   Output: C:\\Deploy\\MyWebApp\\ (78 MB)\n");

            Console.WriteLine("─────────────────────────────────────────────────────────");
            Console.WriteLine("PHASE 4: TEST PUBLISHED APPLICATION LOCALLY");
            Console.WriteLine("─────────────────────────────────────────────────────────\n");

            Console.WriteLine("7. Test before deploying:");
            Console.WriteLine("   cd C:\\Deploy\\MyWebApp");
            Console.WriteLine("   MyWebApp.exe");
            Console.WriteLine("   → Application starts");
            Console.WriteLine("   → Visit: https://localhost:5000");
            Console.WriteLine("   → Verify everything works");
            Console.WriteLine("   → Stop application (Ctrl+C)\n");

            Console.WriteLine("─────────────────────────────────────────────────────────");
            Console.WriteLine("PHASE 5: DEPLOY TO PRODUCTION SERVER");
            Console.WriteLine("─────────────────────────────────────────────────────────\n");

            Console.WriteLine("8. Copy files to server:");
            Console.WriteLine("   • Zip publish folder: MyWebApp.zip");
            Console.WriteLine("   • Upload to server: via FTP/RDP/SCP");
            Console.WriteLine("   • Extract to: C:\\inetpub\\MyWebApp\\\n");

            Console.WriteLine("9. Configure IIS:");
            Console.WriteLine("   • Open IIS Manager");
            Console.WriteLine("   • Add New Site:");
            Console.WriteLine("     - Name: MyWebApp");
            Console.WriteLine("     - Physical Path: C:\\inetpub\\MyWebApp");
            Console.WriteLine("     - Binding: http, port 80, *");
            Console.WriteLine("   • Application Pool:");
            Console.WriteLine("     - .NET CLR Version: No Managed Code");
            Console.WriteLine("     - Identity: ApplicationPoolIdentity\n");

            Console.WriteLine("10. Set environment variable:");
            Console.WriteLine("    • In web.config:");
            Console.WriteLine("      <environmentVariable name=\"ASPNETCORE_ENVIRONMENT\"");
            Console.WriteLine("                          value=\"Production\" />\n");

            Console.WriteLine("11. Set file permissions:");
            Console.WriteLine("    • Grant IIS_IUSRS read access");
            Console.WriteLine("    • Grant write access to logs folder\n");

            Console.WriteLine("─────────────────────────────────────────────────────────");
            Console.WriteLine("PHASE 6: START AND VERIFY");
            Console.WriteLine("─────────────────────────────────────────────────────────\n");

            Console.WriteLine("12. Start application:");
            Console.WriteLine("    • In IIS, start MyWebApp site");
            Console.WriteLine("    • Open browser: http://your-server.com");
            Console.WriteLine("    • Application loads! ✓\n");

            Console.WriteLine("13. Verify production mode:");
            Console.WriteLine("    ✓ Generic error pages (not detailed stack traces)");
            Console.WriteLine("    ✓ Production database connection");
            Console.WriteLine("    ✓ Logging to production location");
            Console.WriteLine("    ✓ HTTPS redirection working");
            Console.WriteLine("    ✓ Performance optimized\n");

            Console.WriteLine("14. Monitor:");
            Console.WriteLine("    • Check application logs");
            Console.WriteLine("    • Monitor server resources");
            Console.WriteLine("    • Test all functionality");
            Console.WriteLine("    • Set up health checks\n");

            Console.WriteLine("===========================================");
            Console.WriteLine("DEPLOYMENT COMPLETE! 🎉");
            Console.WriteLine("===========================================\n");
        }
    }

    #endregion

    #region 9. MAIN PROGRAM - DEMONSTRATION

    /*
     * ============================================================================
     * MAIN PROGRAM
     * ============================================================================
     * 
     * This is a demonstration program that shows all the concepts.
     * Uncomment the sections you want to learn about.
     */

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("\n");
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   ASP.NET CORE DEPLOYMENT & PUBLISHING - COMPLETE GUIDE        ║");
            Console.WriteLine("║   Learn Everything About Kestrel, SCD, FDD, and Publishing     ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.WriteLine("\n");

            // 1. Kestrel Process Information
            Console.WriteLine("▶ SECTION 1: KESTREL WEB SERVER PROCESS NAME");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            KestrelProcessInfo.DisplayKestrelProcessInformation();
            WaitForUser();

            // 2. Self-Contained Deployment
            Console.WriteLine("▶ SECTION 2: SELF-CONTAINED DEPLOYMENT (SCD)");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            SelfContainedDeploymentExample.ExplainSCDOutputStructure();
            WaitForUser();

            // 3. Framework-Dependent Deployment
            Console.WriteLine("▶ SECTION 3: FRAMEWORK-DEPENDENT DEPLOYMENT (FDD)");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            FrameworkDependentDeploymentExample.ExplainFDDOutputStructure();
            WaitForUser();

            // 4. What is Publish?
            Console.WriteLine("▶ SECTION 4: WHAT IS PUBLISH IN ASP.NET CORE?");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            PublishExplanation.ExplainPublishProcess();
            PublishExplanation.ShowCommonPublishCommands();
            WaitForUser();

            // 5. Why Publishing is Needed
            Console.WriteLine("▶ SECTION 5: WHY IS PUBLISHING NEEDED?");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            WhyPublishIsNeeded.Reason1_Optimization();
            WhyPublishIsNeeded.Reason2_Dependencies();
            WhyPublishIsNeeded.Reason3_Configuration();
            WhyPublishIsNeeded.Reason4_Security();
            WhyPublishIsNeeded.Reason5_SizeOptimization();
            WhyPublishIsNeeded.Reason6_DeploymentReadiness();
            WhyPublishIsNeeded.WithoutPublishing();
            WaitForUser();

            // 6. How to Publish in Visual Studio
            Console.WriteLine("▶ SECTION 6: HOW TO PUBLISH IN VISUAL STUDIO");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            VisualStudioPublishGuide.Method1_PublishToFolder();
            VisualStudioPublishGuide.Method2_PublishToAzure();
            VisualStudioPublishGuide.Method3_PublishToIIS();
            VisualStudioPublishGuide.Method4_PublishViaCLI();
            VisualStudioPublishGuide.TroubleshootingTips();
            WaitForUser();

            // 7. Complete Workflow Example
            Console.WriteLine("▶ SECTION 7: COMPLETE PRACTICAL WORKFLOW");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            CompletePublishWorkflow.ShowCompleteWorkflow();
            WaitForUser();

            // Final Summary
            Console.WriteLine("\n");
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    SUMMARY - KEY TAKEAWAYS                     ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.WriteLine("\n");

            Console.WriteLine("✓ KESTREL PROCESS NAME:");
            Console.WriteLine("  • Development/FDD: dotnet.exe");
            Console.WriteLine("  • Self-Contained: YourApp.exe\n");

            Console.WriteLine("✓ DEPLOYMENT TYPES:");
            Console.WriteLine("  • SCD: Includes runtime, larger, standalone");
            Console.WriteLine("  • FDD: Needs runtime, smaller, shared\n");

            Console.WriteLine("✓ PUBLISHING:");
            Console.WriteLine("  • Optimizes for production");
            Console.WriteLine("  • Resolves dependencies");
            Console.WriteLine("  • Applies security");
            Console.WriteLine("  • Creates deployment-ready package\n");

            Console.WriteLine("✓ VISUAL STUDIO STEPS:");
            Console.WriteLine("  1. Right-click project → Publish");
            Console.WriteLine("  2. Choose target (Folder/Azure/IIS)");
            Console.WriteLine("  3. Configure settings");
            Console.WriteLine("  4. Click Publish\n");

            Console.WriteLine("✓ WHY PUBLISH:");
            Console.WriteLine("  • Performance optimization");
            Console.WriteLine("  • Security hardening");
            Console.WriteLine("  • Deployment readiness");
            Console.WriteLine("  • Production configuration\n");

            Console.WriteLine("Happy Deploying! 🚀\n");
        }

        private static void WaitForUser()
        {
            Console.WriteLine("Press any key to continue to next section...\n");
            Console.ReadKey();
            Console.Clear();
        }

        /*
         * BONUS: ACTUAL ASP.NET CORE WEB APPLICATION EXAMPLE
         * ---------------------------------------------------
         * Below is a minimal ASP.NET Core app that you can publish.
         */

        // Uncomment this to create an actual runnable web app:
        /*
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            app.MapGet("/", () => "Hello from Kestrel Web Server!");
            app.MapGet("/process", () => 
            {
                var process = Process.GetCurrentProcess();
                return new
                {
                    ProcessName = process.ProcessName,
                    ProcessId = process.Id,
                    ProcessPath = process.MainModule?.FileName,
                    Environment = app.Environment.EnvironmentName
                };
            });

            app.Run();
        }
        */
    }

    #endregion

    #region 10. QUICK REFERENCE CHEAT SHEET

    /*
     * ============================================================================
     * QUICK REFERENCE - CHEAT SHEET
     * ============================================================================
     * 
     * Copy this section for quick reference!
     */

    public class QuickReference
    {
        /*
         * PUBLISH COMMANDS CHEAT SHEET
         * =============================
         * 
         * Basic Publish (FDD):
         * --------------------
         * dotnet publish -c Release
         * 
         * Self-Contained (Windows):
         * ------------------------
         * dotnet publish -c Release -r win-x64 --self-contained true
         * 
         * Self-Contained (Linux):
         * ----------------------
         * dotnet publish -c Release -r linux-x64 --self-contained true
         * 
         * Single File (Windows):
         * ---------------------
         * dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
         * 
         * Trimmed (Optimized):
         * -------------------
         * dotnet publish -c Release -r win-x64 --self-contained true /p:PublishTrimmed=true
         * 
         * All Optimizations:
         * -----------------
         * dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true /p:PublishReadyToRun=true
         * 
         * 
         * RUNTIME IDENTIFIERS (RIDs)
         * ==========================
         * 
         * Windows:
         * --------
         * win-x64          Windows 64-bit
         * win-x86          Windows 32-bit
         * win-arm64        Windows ARM 64-bit
         * 
         * Linux:
         * ------
         * linux-x64        Linux 64-bit
         * linux-arm        Linux ARM
         * linux-arm64      Linux ARM 64-bit
         * 
         * macOS:
         * ------
         * osx-x64          macOS Intel 64-bit
         * osx-arm64        macOS Apple Silicon (M1/M2)
         * 
         * 
         * VISUAL STUDIO SHORTCUTS
         * =======================
         * 
         * Publish:             Alt + B, H
         * Clean Solution:      Alt + B, C
         * Rebuild Solution:    Alt + B, R
         * Build Solution:      Ctrl + Shift + B
         * 
         * 
         * DEPLOYMENT DECISION TREE
         * ========================
         * 
         * Need .NET on server? → YES → Use FDD (smaller, shared runtime)
         *                      → NO  → Use SCD (larger, standalone)
         * 
         * Multiple .NET apps?  → YES → Use FDD (save space)
         *                      → NO  → Consider SCD
         * 
         * Control runtime?     → YES → Use SCD (specific version)
         *                      → NO  → Use FDD (system managed)
         * 
         * Cloud deployment?    → YES → Use FDD (Azure/AWS provide runtime)
         *                      → NO  → Consider SCD
         */
    }

    #endregion
}

/*
 * ================================================================================
 * END OF ASP.NET CORE DEPLOYMENT GUIDE
 * ================================================================================
 * 
 * CONGRATULATIONS! 🎉
 * 
 * You now understand:
 * ✓ What Kestrel is and how to identify its process
 * ✓ Self-Contained vs Framework-Dependent Deployment
 * ✓ What publishing is and why it's essential
 * ✓ How to publish in Visual Studio (multiple methods)
 * ✓ Complete deployment workflows
 * 
 * NEXT STEPS:
 * -----------
 * 1. Practice publishing a simple ASP.NET Core app
 * 2. Try both SCD and FDD deployments
 * 3. Deploy to IIS or Azure
 * 4. Experiment with different publish settings
 * 5. Monitor your published application
 * 
 * ADDITIONAL RESOURCES:
 * --------------------
 * • Microsoft Docs: https://learn.microsoft.com/aspnet/core/host-and-deploy/
 * • .NET CLI Reference: https://learn.microsoft.com/dotnet/core/tools/
 * • Runtime Identifiers: https://learn.microsoft.com/dotnet/core/rid-catalog
 * 
 * Happy Learning! 📚✨
 * 
 * ================================================================================
 */