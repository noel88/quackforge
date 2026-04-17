using System;
using BepInEx.Configuration;
using BepInEx.Logging;
using QuackForge.Core.Config;
using QuackForge.Core.Events;
using QuackForge.Core.Logging;
using QuackForge.Core.Save;

namespace QuackForge.Core
{
    public sealed class QfCore
    {
        public static QfCore? Instance { get; private set; }

        public QfConfig Config { get; }
        public QfEventBus Events { get; }
        public QfSaveContext Save { get; }

        private QfCore(QfConfig config, QfEventBus events, QfSaveContext save)
        {
            Config = config;
            Events = events;
            Save = save;
        }

        public static QfCore Initialize(ManualLogSource rootLog, ConfigFile configFile)
        {
            if (Instance != null) throw new InvalidOperationException("QfCore already initialized.");
            if (rootLog == null) throw new ArgumentNullException(nameof(rootLog));
            if (configFile == null) throw new ArgumentNullException(nameof(configFile));

            QfLogger.Init(rootLog);
            Instance = new QfCore(new QfConfig(configFile), new QfEventBus(), new QfSaveContext());
            QfLogger.For("Core").Info("QfCore initialized.");
            return Instance;
        }
    }
}
