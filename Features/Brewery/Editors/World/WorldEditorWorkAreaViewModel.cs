using AIRPG.Core.ViewModels;
using AIRPG.Core.Navigation;
using AIRPG.Features.Brewery.Settings;
using AIRPG.Features.Brewery.Editors;

using System.Collections.ObjectModel;
using ReactiveUI;
using System.Diagnostics;
using System.Reactive;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Text;
using Avalonia.Threading;

namespace AIRPG.Features.Brewery.Editors.World;

public class WorldCreateWorkAreaViewModel : ViewModelBase
{
    private ViewModelBase _currentSettings = new BreweryWorldSettingsViewModel();
    
    public ViewModelBase CurrentSettings
    {
        get => _currentSettings;
        set => this.RaiseAndSetIfChanged(ref _currentSettings, value);
    }
    private string _world_path = "world.md";
    private string _world_text = string.Empty;
    private string _user_prompt = string.Empty;
    private string _worldTextWatermark = string.Empty;
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
    private Process? _generationProcess;
    private readonly object _generationLock = new();
    public ReactiveCommand<Unit, Unit> StartWorldGenerationCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelWorldGenerationCommand { get; }
    public WorldCreateWorkAreaViewModel()
    {
        if (!File.Exists(_world_path))
        {
            File.WriteAllText(_world_path, "");
        }

        WorldText = File.ReadAllText(_world_path);

        StartWorldGenerationCommand = ReactiveCommand.CreateFromTask(() => StartWorldGeneration());
        CancelWorldGenerationCommand = ReactiveCommand.Create(() => TerminateGeneration());
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
            var worldSettings = CurrentSettings as BreweryWorldSettingsViewModel;

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"\"{scriptPath}\" " +
                          $"--model {worldSettings?.CurrentLLM} " +
                          $"--stream " +
                          $"--critic_num {worldSettings?.Critics} " +
                          $"--timeout {worldSettings?.TimeOut} " +
                          $"--user_prompt \"{(UserPrompt ?? "").Replace("\"", "\\\"")}\"",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            lock (_generationLock)
            {
                _generationProcess = process;
            }

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
                string? line;

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

        lock (_generationLock)
        {
            _generationProcess = null;
        }

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
    private void TerminateGeneration()
    {
        Process? processToKill = null;
        lock (_generationLock)
        {
            processToKill = _generationProcess;
            _generationProcess = null;
        }

        if (processToKill is not null && !processToKill.HasExited)
        {
            try
            {
                processToKill.Kill(true);
                WorldTextWatermark += "\n[INFO] Generation cancelled by user.";
            }
            catch (Exception ex)
            {
                WorldTextWatermark += $"\n[ERROR] Failed to cancel generation: {ex.Message}";
            }
        }
    }

}

public class WorldLoreWorkAreaViewModel : ViewModelBase{}

public class WorldAbstractWorkAreaViewModel : ViewModelBase{}

public class WorldMapWorkAreaViewModel : ViewModelBase{}
