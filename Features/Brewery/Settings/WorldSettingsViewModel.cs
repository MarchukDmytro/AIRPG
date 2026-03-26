using AIRPG.Core.ViewModels;
using System.Collections.ObjectModel;
using ReactiveUI;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Logging;
using System;

namespace AIRPG.Features.Brewery.Editors.Settings;

public class WorldCreateSettingsViewModel : ViewModelBase
{
    private string _currentLLM = string.Empty;
    private int _critics = 3;
    private int _timeout = 5;
    private bool _isLoadingLLMs = false;
    public ObservableCollection<string> LLMs { get; } = new ObservableCollection<string>();

    public int Critics
    {
        get => _critics;
        set
        {
            this.RaiseAndSetIfChanged(ref _critics, value);
        }
    }

    public int TimeOut
    {
        get => _timeout;
        set => this.RaiseAndSetIfChanged(ref _timeout, value);
    }

    public string CurrentLLM
    {
        get => _currentLLM;
        set => this.RaiseAndSetIfChanged(ref _currentLLM, value);
    }
    
    public bool IsLoadingLLMs
    {
        get => _isLoadingLLMs;
        set => this.RaiseAndSetIfChanged(ref _isLoadingLLMs, value);
    }

    public WorldCreateSettingsViewModel()
    {
        _ = load_models();
    }
    private async Task load_models()
    {
        IsLoadingLLMs = true; 
        LLMs.Clear();

        try
        {
            var result = await Task.Run(() =>
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ollama",
                        Arguments = "list",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                return new { Output = output, Error = error, ExitCode = process.ExitCode };
            });

            if (result.ExitCode != 0)
            {
                Logger.TryGet(LogEventLevel.Error, "Brewery")?
                    .Log(this, $"Ollama error: {result.Error}");
                return;
            }

            // Parse "ollama list" output (NAME, ID, SIZE, MODIFIED columns)
            var lines = result.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                // Skip header line
                if (trimmed.StartsWith("NAME", StringComparison.OrdinalIgnoreCase))
                    continue;

                // First column is the model name
                var parts = trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]))
                {
                    LLMs.Add(parts[0]);
                }
            }

            // Set default selection if available
            if (LLMs.Count > 0 && string.IsNullOrEmpty(CurrentLLM))
            {
                CurrentLLM = LLMs[0];
            }
        }
        catch (Exception ex)
        {
            Logger.TryGet(LogEventLevel.Error, "Brewery")?
                .Log(this, $"Failed to load models: {ex.Message}");
        }
        finally
        {
            IsLoadingLLMs = false;
        }
    }

}
public class WorldLoreSettingsViewModel : ViewModelBase{}
public class WorldAbstractSettingsViewModel : ViewModelBase{}

public class WorldMapSettingsViewModel : ViewModelBase{}
