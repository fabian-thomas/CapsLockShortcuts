using System.IO;
using System.Reflection;

namespace CapsLockMacros
{
    class Constants
    {
        public static readonly string APPNAME = Assembly.GetExecutingAssembly().GetName().Name;
        public static readonly string EXECUTION_DIR = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly string CONFIG_PATH = $"{EXECUTION_DIR}\\config.json";
    }
}
