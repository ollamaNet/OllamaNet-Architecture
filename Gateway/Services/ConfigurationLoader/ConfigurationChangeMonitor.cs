using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Gateway.Services.ConfigurationLoader
{
    public class ConfigurationChangeMonitor : BackgroundService
    {
        private readonly ILogger<ConfigurationChangeMonitor> _logger;
        private readonly FileSystemWatcher _watcher;
        private readonly Action _onConfigurationChanged;

        public ConfigurationChangeMonitor(ILogger<ConfigurationChangeMonitor> logger, Action onConfigurationChanged)
        {
            _logger = logger;
            _onConfigurationChanged = onConfigurationChanged;
            
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "Configurations");
            
            _watcher = new FileSystemWatcher(configPath)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                Filter = "*.json",
                IncludeSubdirectories = false,
                EnableRaisingEvents = false
            };
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _watcher.Changed += OnChanged;
            _watcher.Created += OnChanged;
            _watcher.Renamed += OnChanged;
            _watcher.Deleted += OnChanged;
            
            _watcher.EnableRaisingEvents = true;
            
            return Task.CompletedTask;
        }
        
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation($"Configuration file {e.Name} was {e.ChangeType}. Reloading configuration...");
            
            // Debounce multiple events
            Task.Delay(500).ContinueWith(_ => 
            {
                try
                {
                    _onConfigurationChanged();
                    _logger.LogInformation("Configuration reloaded successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reloading configuration");
                }
            });
        }

        public override void Dispose()
        {
            _watcher.Changed -= OnChanged;
            _watcher.Created -= OnChanged;
            _watcher.Renamed -= OnChanged;
            _watcher.Deleted -= OnChanged;
            
            _watcher.Dispose();
            
            base.Dispose();
        }
    }
} 