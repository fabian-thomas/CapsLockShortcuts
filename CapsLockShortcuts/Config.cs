using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace CapsLockMacros
{
    class Config
    {
        public bool DisableCapsLockToggle { get; set; }
        public List<Keys> BaseKeys { get; set; }
        public List<ParsedShortcut> Shortcuts { get; set; }

        public Config()
        {
            Load();
        }

        public void Load()
        {
            ConfigFormat storedConfig;

            if (!File.Exists(Constants.CONFIG_PATH))
            {
                storedConfig = DEFAULTCONFIG;
                SaveConfig(storedConfig);
            }
            else
            {
                var json = File.ReadAllText(Constants.CONFIG_PATH);
                storedConfig = string.IsNullOrEmpty(json) ? DEFAULTCONFIG : JsonSerializer.Deserialize<ConfigFormat>(json);
            }

            // parse/copy from stored config
            DisableCapsLockToggle = storedConfig.DisableCapsLockToggle;

            if (storedConfig.BaseKeys == null)
                BaseKeys = new List<Keys>();
            else
                BaseKeys = storedConfig.BaseKeys.Select(s => Enum.Parse<Keys>(s)).ToList();


            if (storedConfig.Shortcuts == null)
                Shortcuts = new List<ParsedShortcut>();
            else
                Shortcuts = storedConfig.Shortcuts.Select(s => new ParsedShortcut
                {
                    InputKey = Enum.Parse<Keys>(s.InputKey),
                    OutputKey = Enum.Parse<Keys>(s.OutputKey)
                }).ToList();
        }

        public void Save()
        {
            var configToWrite = new ConfigFormat
            {
                BaseKeys = BaseKeys.Select(k => k.ToString()).ToList(),
                Shortcuts = Shortcuts.Select(ps => new Shortcut
                {
                    InputKey = ps.InputKey.ToString(),
                    OutputKey = ps.InputKey.ToString(),
                }).ToList()
            };

            SaveConfig(configToWrite);
        }

        private void SaveConfig(ConfigFormat configToWrite)
        {
            var json = JsonSerializer.Serialize(configToWrite, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Constants.CONFIG_PATH, json);
        }


        #region classes for storage
        public class ConfigFormat
        {
            public bool DisableCapsLockToggle { get; set; }
            public List<string> BaseKeys { get; set; }

            public List<Shortcut> Shortcuts { get; set; }
        }

        public class Shortcut
        {
            public string InputKey { get; set; }
            public string OutputKey { get; set; }
        }

        public class ParsedShortcut
        {
            public Keys InputKey { get; set; }
            public Keys OutputKey { get; set; }
        }
        #endregion


        private static readonly ConfigFormat DEFAULTCONFIG = new ConfigFormat
        {
            DisableCapsLockToggle = true,
            BaseKeys = new List<string> { Keys.CapsLock.ToString() },
            Shortcuts = new List<Shortcut>
            {
                // left hand side of keyboard
                new Shortcut { InputKey = Keys.A.ToString(), OutputKey = Keys.Left.ToString() },
                new Shortcut { InputKey = Keys.W.ToString(), OutputKey = Keys.Up.ToString() },
                new Shortcut { InputKey = Keys.D.ToString(), OutputKey = Keys.Right.ToString() },
                new Shortcut { InputKey = Keys.S.ToString(), OutputKey = Keys.Down.ToString() },

                new Shortcut { InputKey = Keys.Q.ToString(), OutputKey = Keys.Back.ToString() },
                new Shortcut { InputKey = Keys.E.ToString(), OutputKey = Keys.Delete.ToString() },

                new Shortcut { InputKey = Keys.R.ToString(), OutputKey = Keys.Home.ToString() },
                new Shortcut { InputKey = Keys.F.ToString(), OutputKey = Keys.End.ToString() },

                // right hand side of keyboard
                new Shortcut { InputKey = Keys.J.ToString(), OutputKey = Keys.Left.ToString() },
                new Shortcut { InputKey = Keys.I.ToString(), OutputKey = Keys.Up.ToString() },
                new Shortcut { InputKey = Keys.L.ToString(), OutputKey = Keys.Right.ToString() },
                new Shortcut { InputKey = Keys.K.ToString(), OutputKey = Keys.Down.ToString() },

                new Shortcut { InputKey = Keys.U.ToString(), OutputKey = Keys.Back.ToString() },
                new Shortcut { InputKey = Keys.O.ToString(), OutputKey = Keys.Delete.ToString() }
            }
        };
    }
}
