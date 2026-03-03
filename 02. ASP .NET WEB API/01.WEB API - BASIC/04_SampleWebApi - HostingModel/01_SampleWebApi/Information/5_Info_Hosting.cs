namespace _01_SampleWebApi.Information
{
    public class _5_Info_Hosting
    {

        #region 01 - What is Web Server?

        /*
            A Web Server is a software that:

            1. Listens for HTTP requests from browser
            2. Processes the request
            3. Sends HTTP response back to browser

            Example Flow:

            Browser  --->  Web Server  --->  ASP.NET Core App  --->  Response

            In ASP.NET Core:
            - Kestrel is the default web server.
            - IIS can also act as web server (Windows only).
        */

        #endregion


        #region 02 - What is Hosting?

        /*
            Hosting means:

            Running your application on a server machine
            so that it can be accessed over internet.

            Example:

            You build Web API on your laptop.
            That is NOT hosting.

            When you deploy it to:
            - IIS Server
            - Cloud Server
            - Linux Server
            - Docker Container

            That is called Hosting.
        */

        #endregion


        #region 03 - What is Kestrel?

        /*
            Kestrel is:

            - Default web server for ASP.NET Core
            - Cross-platform (Windows, Linux, macOS)
            - Very fast and lightweight

            IMPORTANT:

            ASP.NET Core ALWAYS runs on Kestrel internally.

            Even when using IIS,
            Kestrel is still used internally.

            If you run:

                dotnet run

            Then:

                Browser ---> Kestrel ---> ASP.NET Core App
        */

        #endregion


        #region 04 - What is IIS?

        /*
            IIS = Internet Information Services

            - Microsoft Web Server
            - Works only on Windows
            - Used for hosting ASP.NET & ASP.NET Core apps

            IIS provides:
            - Security
            - SSL Support
            - Authentication
            - Logging
            - Process Management

            In Windows Production Server,
            IIS is commonly used.
        */

        #endregion


        #region 05 - What is IIS Express?

        /*
            IIS Express is:

            - Lightweight version of IIS
            - Used only for Development
            - Automatically installed with Visual Studio

            When you run project with IIS Express:

                Browser ---> IIS Express ---> Kestrel ---> App

            It is NOT used in Production.
        */

        #endregion


        #region 06 - Hosting Models in ASP.NET Core (When Using IIS)

        /*
            There are TWO hosting models:

            1) In-Process Hosting
            2) Out-Of-Process Hosting
        */

        #endregion


        #region 07 - In-Process Hosting Model (Recommended)

        /*
            In-Process means:

            ASP.NET Core runs inside IIS worker process.

            Architecture:

                Browser ---> IIS ---> ASP.NET Core App

            No separate Kestrel process.

            Advantages:
            - Better performance
            - Less overhead
            - Recommended by Microsoft
            - Default in .NET Core 3.0+

            web.config setting:

                hostingModel="inprocess"
        */

        #endregion


        #region 08 - Out-Of-Process Hosting Model

        /*
            Out-Of-Process means:

            IIS forwards request to Kestrel.

            Architecture:

                Browser ---> IIS ---> Kestrel ---> ASP.NET Core App

            Here:
            - Kestrel runs separately
            - IIS acts as Reverse Proxy

            web.config setting:

                hostingModel="outofprocess"
        */

        #endregion


        #region 09 - How to Know Which Hosting Model is Used?

        /*
            During Development:

            Check:
                Properties → launchSettings.json

            If:
                "commandName": "Project"
                => Kestrel only

            If:
                "commandName": "IISExpress"
                => IIS Express + Kestrel


            After Publish to IIS:

            Check:
                PublishedFolder → web.config

            Look at:
                hostingModel="inprocess"
                OR
                hostingModel="outofprocess"
        */

        #endregion


        #region 10 - Which Hosting Should You Use?

        /*
            Development:
                - IIS Express (Easy)
                - Kestrel (Project profile)

            Windows Production:
                - IIS + InProcess (Best choice)

            Linux Production:
                - Kestrel + Nginx

            Docker / Cloud:
                - Kestrel directly
        */

        #endregion


        #region 11 - Simple Real-Life Analogy

        /*
            Think like Restaurant:

            Customer   = Browser
            Waiter     = IIS
            Kitchen    = Kestrel
            Chef       = ASP.NET Core App

            InProcess:
                Waiter and Chef in same room (Fast)

            OutProcess:
                Waiter runs to another building (Slightly slower)
        */

        #endregion

    }
}
