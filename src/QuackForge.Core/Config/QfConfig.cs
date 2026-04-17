using System;
using BepInEx.Configuration;

namespace QuackForge.Core.Config
{
    public sealed class QfConfig
    {
        private readonly ConfigFile _file;

        public QfConfig(ConfigFile file)
        {
            _file = file ?? throw new ArgumentNullException(nameof(file));
        }

        public ConfigEntry<T> Bind<T>(string section, string key, T defaultValue, string description)
            => _file.Bind(section, key, defaultValue, description);
    }
}
