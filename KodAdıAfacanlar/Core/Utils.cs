using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KodAdıAfacanlar.Core
{
    internal static class Utils
    {
        internal static string GetLocalApplicationDataFolder()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Kod Adı Afacanlar");
        }

        internal static string GetContentFile(string filename)
        {
            return Path.Combine(GetLocalApplicationDataFolder(), filename);
        }

        internal static string GetLogFileName()
        {
            return Path.Combine(GetLocalApplicationDataFolder(), "logs", "log-.log");
        }
    }
}
