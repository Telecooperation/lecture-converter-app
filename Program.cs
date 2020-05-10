using ConverterCore.Recordings;
using ConverterCore.Services;
using ConverterCore.Studio;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TkRecordingConverter.util;

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
            services.AddTransient<FFMpegConvertService, FFMpegConvertService>();
            services.AddTransient<Converter, Converter>();
            services.AddTransient<RecordingConverter, RecordingConverter>();
            services.AddTransient<CourseWatcher, CourseWatcher>();
            services.AddTransient<PublishService, PublishService>();

            // add app
            services.AddTransient<App>();
        }
    }
}
