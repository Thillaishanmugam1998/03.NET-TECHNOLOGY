using _01_SampleWebApi.Services;
using Microsoft.Extensions.Options;

namespace _01_SampleWebApi
{

    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            /*
                🔵 THIS IS WHERE DEPENDENCY INJECTION IS CONFIGURED

                Meaning:

                Whenever IProductService is required,
                provide ProductService object.

                AddScoped → One object per request
            */

            builder.Services.AddScoped<IProductService, ProductService>();

            var app = builder.Build();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();

        }
    }

}
