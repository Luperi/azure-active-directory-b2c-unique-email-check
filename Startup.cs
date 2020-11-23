using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyProject.Settings;

[assembly: FunctionsStartup(typeof(MyProject.Function.Startup))]

namespace MyProject.Function
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            ConfigureServices(builder.Services);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            ReadOptions(services);
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddFilter(level => true);
            });
        }

        public void ReadOptions(IServiceCollection services)
        {
            services.AddOptions<AdminConfiguration>()
               .Configure<IConfiguration>((settings, configuration) =>
               {
                   configuration.GetSection("AdminConfiguration").Bind(settings);
               });
        }
    }
}