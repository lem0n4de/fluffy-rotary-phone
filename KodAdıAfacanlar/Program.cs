using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using KodAdıAfacanlar.Core;
using KodAdıAfacanlar.Models;
using Serilog;

namespace KodAdıAfacanlar
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            try
            {

                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Console()
                    .WriteTo.File(Utils.GetLogFileName(), rollingInterval: RollingInterval.Day)
                    .CreateLogger();
                Log.Debug("Starting avalonia");
                Utils.CopyFilesToLocal();
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
                Log.CloseAndFlush();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while starting the app {e.Message}");
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}