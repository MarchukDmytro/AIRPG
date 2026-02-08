using ReactiveUI;
using System;
using System.Diagnostics;
using System.Reactive;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using Avalonia.Threading;
using Avalonia.Logging;
using AIRPG.Core.ViewModels;
using AIRPG.Core.Navigation;
namespace AIRPG.Features.Brewery;

public class BreweryViewModel : ViewModelBase
{
    public ObservableCollection<string> Models { get; } = new ObservableCollection<string>();
    public ObservableCollection<string> WorldList { get; } = new ObservableCollection<string>();

    private string _world_path = "world.md";
    private string _current_model;
    private int _critics = 3;
    private int _timeout = 5;
    private bool _isLoadingModels;
    private string _world_text = "";
    private string _user_prompt;
    private string _worldTextWatermark = "";
    private string _currentWorld;


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

    public string CurrentModel
    {
        get => _current_model;
        set => this.RaiseAndSetIfChanged(ref _current_model, value);
    }
    public bool IsLoadingModels
    {
        get => _isLoadingModels;
        set => this.RaiseAndSetIfChanged(ref _isLoadingModels, value);
    }
    public string WorldText
    {
        get => _world_text;
        set
        {
            this.RaiseAndSetIfChanged(ref _world_text, value);
        }
    }

    public string UserPrompt
    {
        get => _user_prompt;
        set => this.RaiseAndSetIfChanged(ref _user_prompt, value);
    }

    public string WorldTextWatermark
    {
        get => _worldTextWatermark;
        set => this.RaiseAndSetIfChanged(ref _worldTextWatermark, value);
    }

    private readonly INavigationService _navigation;
    public ReactiveCommand<Unit, Unit> OpenMainMenuCommand { get; }
    public ReactiveCommand<Unit, Unit> StartWorldGenerationCommand { get; }

    public BreweryViewModel(INavigationService navigation)
    {
        if (!File.Exists(_world_path))
        {
            File.WriteAllTextAsync(_world_path, "");
        }
        WorldText = File.ReadAllText(_world_path);

        load_models();
        _navigation = navigation;
        //CurrentWorkZone = new CrateWorldViewMap();
        StartWorldGenerationCommand = ReactiveCommand.CreateFromTask(() => StartWorldGeneration());
        OpenMainMenuCommand = ReactiveCommand.Create(() => _navigation.ToMainMenu());
    }


    private async Task StartWorldGeneration()
    {
        string scriptPath = "python_scripts/world_generator_v1.py";
        string logPath = "error_log.txt"; // Log file path
        object logLock = new object();
        // Clear old log at start
        File.WriteAllText(logPath, $"[{DateTime.Now}] Session started\n");

        WorldTextWatermark = "Generating...\n";

        string lastLine = string.Empty;
        string fullOutput = string.Empty;

        var result = await Task.Run(async () =>
        {
            var tcs = new TaskCompletionSource<bool>();

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"\"{scriptPath}\" " +
                          $"--model {CurrentModel} " +
                          $"--stream " +
                          $"--critic_num {Critics} " +
                          $"--timeout {TimeOut} " +
                          $"--user_prompt \"{(UserPrompt ?? "").Replace("\"", "\\\"")}\"",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            process.Exited += (_, _) => tcs.TrySetResult(true);
            process.Start();

            // === BYTE-LEVEL STDOUT READING ===
            var stdoutTask = Task.Run(async () =>
            {
                var stream = process.StandardOutput.BaseStream;
                byte[] buffer = new byte[1024];
                Decoder decoder = Encoding.UTF8.GetDecoder();
                char[] charBuffer = new char[1024];

                string currentLine = "";
                string finalLastLine = "";
                string allText = "";

                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    int charCount = decoder.GetChars(buffer, 0, bytesRead, charBuffer, 0);

                    for (int i = 0; i < charCount; i++)
                    {
                        char c = charBuffer[i];
                        string charStr = c.ToString();

                        allText += charStr;

                        Dispatcher.UIThread.Post(() =>
                        {
                            WorldTextWatermark += charStr;
                        });

                        if (c == '\n')
                        {
                            finalLastLine = currentLine;
                            currentLine = "";
                        }
                        else if (c != '\r')
                        {
                            currentLine += charStr;
                        }
                    }
                }

                if (currentLine.Length > 0)
                {
                    finalLastLine = currentLine;
                }

                lastLine = finalLastLine;
                fullOutput = allText;
            });

            // === STDERR - SYNC FILE WRITE (crash-safe) ===
            _ = Task.Run(() =>
            {
                using var reader = process.StandardError;
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    string timestampedError = $"[{DateTime.Now:HH:mm:ss.fff}] [ERR] {line}\n";

                    // Write to file IMMEDIATELY (synchronous = crash-safe)
                    File.AppendAllText(logPath, timestampedError);

                    // Also update UI
                    Dispatcher.UIThread.Post(() =>
                    {
                        WorldTextWatermark += $"[ERR] {line}\n";
                    });
                }
            });

            // 1. WAIT FOR PROCESS TO ACTUALLY EXIT
            await tcs.Task;

            // 2. WAIT FOR STREAMS TO FINISH READING
            await Task.WhenAll(stdoutTask, stdoutTask);

            // 3. CAPTURE EXIT CODE NOW (Process is done, but object is open)
            int finalExitCode = process.ExitCode;

            // 4. CLOSE PROCESS (Releases handle)
            process.Close();

            // 5. LOG COMPLETION (Safe because streams are done and we have the int value)
            lock (logLock)
            {
                File.AppendAllText(logPath, $"[{DateTime.Now}] Process exited with code: {finalExitCode}\n");
            }

            // 6. RETURN LOCAL VARIABLE (Do not use process.ExitCode here)
            return new { ExitCode = finalExitCode, LastLine = lastLine, FullOutput = fullOutput };
        });

        // === READ FILE FROM LAST LINE ===
        if (!string.IsNullOrWhiteSpace(result.LastLine))
        {
            string path = result.LastLine.Trim();

            try
            {
                if (File.Exists(path))
                {
                    byte[] fileBytes = await File.ReadAllBytesAsync(path);
                    string fileText = Encoding.UTF8.GetString(fileBytes);

                    Dispatcher.UIThread.Post(() =>
                    {
                        WorldText += $"\n\n[FILE: {path}]\n{fileText}";
                    });
                }
                else
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        WorldTextWatermark += $"\n\n[LAST LINE]: {result.LastLine}";
                    });
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"[{DateTime.Now}] [EXCEPTION] {ex}\n";
                File.AppendAllText(logPath, errorMsg);

                Dispatcher.UIThread.Post(() =>
                {
                    WorldTextWatermark += $"\n[ERROR]: {ex.Message}";
                });
            }
        }
    }


    private async Task load_models()
    {
        IsLoadingModels = true;
        Models.Clear();

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
                    Models.Add(parts[0]);
                }
            }

            // Set default selection if available
            if (Models.Count > 0 && string.IsNullOrEmpty(CurrentModel))
            {
                CurrentModel = Models[0];
            }
        }
        catch (Exception ex)
        {
            Logger.TryGet(LogEventLevel.Error, "Brewery")?
                .Log(this, $"Failed to load models: {ex.Message}");
        }
        finally
        {
            IsLoadingModels = false;
        }
    }

}
