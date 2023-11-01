using Microsoft.EntityFrameworkCore;

namespace KafkaDataService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            // Add services to the container.
            builder.Services.AddDbContext<WebContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("WebContext") ?? throw new InvalidOperationException("Connection string 'WebContext' not found.")));

            builder.Services.AddControllers();
            builder.Services.AddHostedService<KafkaPostsService>();
            builder.Services.AddHostedService<KafkaCommentService>();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}