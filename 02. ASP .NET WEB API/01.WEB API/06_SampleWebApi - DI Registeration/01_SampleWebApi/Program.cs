using Microsoft.Extensions.Options;

namespace _01_SampleWebApi
{

    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            var app = builder.Build();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();

        }
    }

}
