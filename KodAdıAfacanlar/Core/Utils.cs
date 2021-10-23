using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace KodAdıAfacanlar.Core
{
    internal static class Utils
    {
        internal static string GetLocalApplicationDataFolder()
        {
            if (OperatingSystem.IsMacOS())
            {
                var user = Environment.UserName;
                var y = Path.Combine("/Users", user, ".local", "Kod Adı Afacanlar");
                Directory.CreateDirectory(y);
                return y;
            }
            var x = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Kod Adı Afacanlar");
            Directory.CreateDirectory(x);
            return x;
        }

        internal static string GetContentFile(string filename)
        {
            return Path.Combine(GetLocalApplicationDataFolder(), filename);
        }

        internal static string GetLogFileName()
        {
            var x = Path.Combine(GetLocalApplicationDataFolder(), "logs");
            Directory.CreateDirectory(x);
            var y = Path.Combine(x, "log-.log");
            return y;
        }

        internal static void CopyFilesToLocal()
        {
            try
            {
#if TIME
                File.Copy(Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule!.FileName)!, "time.db"), GetContentFile("time.db"));
                File.Copy(Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule!.FileName)!, "video-links.json"), GetContentFile("video-links.json"));
#else
                File.Copy(
                    Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule!.FileName)!, "world.db"),
                    GetContentFile("world.db"));
                File.Copy(
                    Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule!.FileName)!,
                        "lessons.json"), GetContentFile("lessons.json"));
#endif
            }
            catch (Exception e)
            {
                Log.Error("Error while copying files to local: {e}", e);
            }
        }
    }
}