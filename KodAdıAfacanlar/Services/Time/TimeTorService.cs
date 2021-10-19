using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DiscUtils.Dmg;
using Knapcode.TorSharp;
using Serilog;
using Utils = KodAdıAfacanlar.Core.Utils;

namespace KodAdıAfacanlar.Services.Time
{
    public class TimeTorService
    {
        private readonly TorSharpSettings settings = new()
        {
            ZippedToolsDirectory = Utils.GetContentFile("TorZipped"),
            ExtractedToolsDirectory = Utils.GetContentFile("TorExtracted")
        };

        private Process torProcess;

        public async Task SetupToolsAsync()
        {
            Log.Debug("Setting up tor service");
            if (OperatingSystem.IsMacOS())
            {
                Directory.CreateDirectory(Path.Combine(settings.ExtractedToolsDirectory, "Tor"));
                File.Copy(Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule!.FileName)!, "tor"),
                    Path.Combine(settings.ExtractedToolsDirectory, "Tor", "tor"));
            }
            else
            {
                await new TorSharpToolFetcher(settings, new HttpClient()).FetchAsync();
                TorSharpProxy torSharpProxy = new(settings);
                await torSharpProxy.ConfigureAsync();
            }
        }

        public async Task StartTorAsync()
        {
            Log.Information("Starting tor service");
            ProcessStartInfo startInfo;

            Regex reg = new(@"tor-.+");
            if (OperatingSystem.IsWindows())
            {
                // For some reason, this line hangs on MacOS
                var torExtractedPath = Directory
                    .GetDirectories(settings.ExtractedToolsDirectory).First(path => reg.IsMatch(path));
                var torPath = $"{torExtractedPath}{Path.DirectorySeparatorChar}Tor{Path.DirectorySeparatorChar}tor.exe";
                Log.Debug("Tor path is {torPath}", torPath);
                startInfo = new ProcessStartInfo(torPath);
            }
            else if (OperatingSystem.IsMacOS())
            {
                // var torPath = Path.Combine(settings.ExtractedToolsDirectory, "Tor", "tor");
                var torPath = "/Applications/Tor Browser.app/Contents/MacOS/Tor/tor.real";
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
            Log.Debug("Tor process started");
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