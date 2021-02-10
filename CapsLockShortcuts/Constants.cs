using System;
using System.IO;
using System.Reflection;

namespace CapsLockMacros
{
    class Constants
    {
        public static readonly string APPNAME = Assembly.GetExecutingAssembly().GetName().Name;
        public static readonly string EXECUTION_DIR = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly string APPDATA_Path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static readonly string CONFIG_PATH = Path.Combine(APPDATA_Path, APPNAME, "config.json");
    }
}
