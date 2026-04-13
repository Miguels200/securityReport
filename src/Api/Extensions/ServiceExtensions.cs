using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace SecurityReport.Api.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddSwaggerDocs(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SecurityReport API", Version = "v1" });
            });

            return services;
        }
    }
}