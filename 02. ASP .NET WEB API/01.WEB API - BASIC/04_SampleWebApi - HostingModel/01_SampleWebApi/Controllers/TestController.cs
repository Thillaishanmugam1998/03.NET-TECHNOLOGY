using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Reflection.PortableExecutable;

namespace _01_SampleWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        // The IServer interface represents the actual server implementation (IISHttpServer or KestrelServer)
        private readonly IServer _server;

        // Constructor Dependency Injection - ASP.NET Core injects the active IServer service
        public TestController(IServer server)
        {
            _server = server;
        }

        // GET: api/test/ping
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            // Get the current process information (e.g., w3wp, iisexpress, dotnet)
            var proc = Process.GetCurrentProcess();
            
            // Return hosting environment details as JSON response
            return Ok(new
            {
                message = "Hosting Info", // Custom message
                pid = Environment.ProcessId, // Numeric process ID
                process = proc.ProcessName, // Running process name
                serverType = _server.GetType().Name, // Hosting server type (IISHttpServer or KestrelServer)
                machine = Environment.MachineName // Server machine name
            });
        }
    }
}
