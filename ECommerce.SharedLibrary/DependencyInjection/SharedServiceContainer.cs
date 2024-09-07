using ECommerce.SharedLibrary.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ECommerce.SharedLibrary.DependencyInjection
{
    public static class SharedServiceContainer
    {

        public static IServiceCollection AddSharedService<TContext>
            (this IServiceCollection services, IConfiguration configuration, string fileName)
            where TContext : DbContext
        {
            //Add Generic Database Context
            services.AddDbContext<TContext>(options => options.UseSqlServer(
                configuration.GetConnectionString("eCommerceConnection"), sqlserveroption =>
                sqlserveroption.EnableRetryOnFailure()));

            //Configure serilog logging
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Debug()
                .WriteTo.Console()
                .WriteTo.File(path: $"{fileName}-.text",
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();
            //Add JWT Authentication Scheme
            JWTAuthenticationScheme.AddJWTAuthenticationScheme(services, configuration);
            return services;
        }
        public static IApplicationBuilder UseSharedPolicies
            (this IApplicationBuilder app)
        {
            //Use global Exception
            app.UseMiddleware<GlobalExceptions>();

            //Register middleware to block all outsiders API Calls
            app.UseMiddleware<ListenToOnlyAPIGateway>();
            return app;
        }
    }
}
