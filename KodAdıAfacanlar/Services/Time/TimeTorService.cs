using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Knapcode.TorSharp;
using Serilog;

namespace KodAdıAfacanlar.Services.Time
{
    public class TimeTorService
    {
        private readonly TorSharpSettings settings = new()
        {
            ZippedToolsDirectory =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Kod Adı Afacanlar", "TorZipped"),
            ExtractedToolsDirectory =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Kod Adı Afacanlar", "TorExtracted")
        };

        private Process torProcess;

        public async Task SetupToolsAsync()
        {
            Debug.WriteLine("Setting up tor service");
            await new TorSharpToolFetcher(settings, new HttpClient()).FetchAsync();
            TorSharpProxy torSharpProxy = new(settings);
            await torSharpProxy.ConfigureAsync();
        }

        public async Task StartTorAsync()
        {
            Log.Information("Starting tor service.");
            ProcessStartInfo startInfo;

            Regex reg = new(@"tor-.+");
            var torExtractedPath = Directory
                .GetDirectories(settings.ExtractedToolsDirectory).First(path => reg.IsMatch(path));
            if (OperatingSystem.IsWindows())
            {
                var torPath = $"{torExtractedPath}{Path.DirectorySeparatorChar}Tor{Path.DirectorySeparatorChar}tor.exe";
                Log.Debug("Tor path is {torPath}", torPath);
                startInfo = new ProcessStartInfo(torPath);
            }
            else if (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
            {
                var torPath = $"{torExtractedPath}{Path.DirectorySeparatorChar}Tor{Path.DirectorySeparatorChar}tor";
                Log.Debug("Tor path is {torPath}", torPath);
                startInfo = new ProcessStartInfo(torPath);
            }
            else
            {
                Log.Error("Operating System not supported. Shutting down.");
                return;
            }

            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            torProcess = Process.Start(startInfo) ?? throw new InvalidOperationException();
#if DEBUG
            torProcess.OutputDataReceived += (sender, args) => { Log.Debug("TOR OUTPUT: {data}", args.Data); };
            torProcess.ErrorDataReceived += (sender, args) => { Log.Debug("TOR ERROR: {data}", args.Data); };
#endif
            await testProxy();
        }

        internal void Close()
        {
            if (torProcess != null) torProcess.Kill();
            Process[] chromeDriver = Process.GetProcessesByName("chromedriver");
            foreach (var p in chromeDriver)
            {
                p.Kill();
            }
        }

        private async Task testProxy()
        {
            Log.Debug("Testing proxy");
            var originalIp = "";
            var proxyIp = "";
            using (var client = new HttpClient())
            {
                originalIp = await client.GetStringAsync("https://icanhazip.com/");
            }

            using (var client = new HttpClient(new HttpClientHandler()
            {
                Proxy = new WebProxy("socks5://localhost:9050")
            }))
            {
                for (var i = 0; i < 10; i++)
                {
                    try
                    {
                        proxyIp = await client.GetStringAsync("https://icanhazip.com/");
                        if (!String.IsNullOrWhiteSpace(proxyIp) && proxyIp != originalIp)
                        {
                            Log.Information("Tor connection established.");
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Information("Couldn't connect to tor, retrying in 5 seconds", e);
                        Thread.Sleep(5000);
                    }
                }
            }

            if (String.IsNullOrWhiteSpace(proxyIp))
            {
                Log.Error("Couldn't connect to tor. Shutting down.");
                torProcess.Kill();
            }
        }
    }
}