using ConverterCore.Recording;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ConverterCore
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            // create service collection
            var services = new ServiceCollection();
            ConfigureServices(services);

            // create service provider
            var serviceProvider = services.BuildServiceProvider();

            // entry to run app
            await serviceProvider.GetService<App>().Run();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // configure logging
            services.AddLogging(builder =>
                builder
                    .AddDebug()
                    .AddConsole()
            );

            // add services
            services.AddTransient<RecordingConverter, RecordingConverter>();
            services.AddTransient<RecordingWatcher, RecordingWatcher>();

            // add app
            services.AddTransient<App>();
        }
    }
}
