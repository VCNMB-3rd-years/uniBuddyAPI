//using uniBuddyAPI.Data;
using Microsoft.EntityFrameworkCore;
using uniBuddyAPI.Services;

namespace uniBuddyAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //builder.Services.AddDbContext<AppDbContext>(opt =>
            //    opt.UseInMemoryDatabase("UniBuddyDb"));

            builder.Services.AddSingleton<RealTimeDbService>();

            builder.Services.AddControllers().AddJsonOptions(o =>
            {
                o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();

        }
    }
}
