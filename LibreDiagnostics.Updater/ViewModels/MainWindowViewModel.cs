/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using BlackSharp.Core.Extensions;
using BlackSharp.Core.Logging;
using BlackSharp.MVVM.ComponentModel;
using LibreDiagnostics.Tasks.Github;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace LibreDiagnostics.Updater.ViewModels
{
    /// <summary>
    /// ViewModel class.
    /// </summary>
    public partial class MainWindowViewModel : ViewModelBase
    {
        #region Constructor

        protected MainWindowViewModel(object o)
        {
            InitDesignTime();
        }

        public MainWindowViewModel()
        {
            InitRunTime();
        }

        #endregion

        #region Init

        /// <summary>
        /// Initializes Design Time data.
        /// </summary>
        void InitDesignTime()
        {
            StatusText = "Design Time Status";

            Progress = 50d;
        }

        /// <summary>
        /// Initializes Run Time data.
        /// </summary>
        async void InitRunTime()
        {
            await DoUpdate();
        }

        #endregion

        #region Fields

        int CurrentStep = 0;

        /// <summary>
        /// Steps:
        /// 1. Check if update is available
        /// 2. If yes, do a self-copy and run
        /// ---
        /// 1. Download update
        /// 2. Extract update
        /// (3. Run new version)
        /// </summary>
        int TotalSteps = 2;

        static readonly DateTime FileNameDateTime = DateTime.Now;

        static Thread _LogThread;

        #endregion

        #region Properties

        string _StatusText;
        public string StatusText
        {
            get { return _StatusText; }
            set { SetField(ref _StatusText, value); }
        }

        double _Progress;
        public double Progress
        {
            get { return _Progress; }
            set { SetField(ref _Progress, value); }
        }

        #endregion

        #region Private

        static void LogTrace(string message = "")
        {
            if (Logger.Instance.IsEnabled)
            {
                Logger.Instance.Add(LogLevel.Trace, message, DateTime.Now);
                SaveLogFile();
            }
        }

        static void SaveLogFile()
        {
            Logger.Instance.SaveToFile(@$"C:\LDT\UpdaterLog_{FileNameDateTime:yyyyMMdd_HHmmss}.txt", false);
        }

        async Task DoUpdate()
        {
            var args = Client.Args;

            //Enable this for logging output (and verify directory of log file exists)
            //Logger.Instance.LogLevel = LogLevel.Trace;
            //Logger.Instance.IsEnabled = true;

            if (Logger.Instance.IsEnabled)
            {
                _LogThread = new Thread(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(5000);
                        SaveLogFile();
                    }
                });
                _LogThread.IsBackground = true;
                _LogThread.Start();
            }

            LogTrace("Arguments:");
            foreach (var arg in args)
            {
                LogTrace($"\"{arg}\"");
            }
            LogTrace();

            var versionOfMyself = Assembly.GetEntryAssembly().GetName().Version;

            //Get own file path
            var currentFilePath = Environment.ProcessPath;
            LogTrace($"{nameof(currentFilePath)} = '{currentFilePath}'");

            //Get current directory
            var currentDirectory = Path.GetDirectoryName(currentFilePath);
            LogTrace($"{nameof(currentDirectory)} = '{currentDirectory}'");

            if (args.Count == 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Please do NOT start the updater manually.");
                sb.AppendLine();
                sb.AppendLine("This can result in PERMANENT loss of files");
                sb.AppendLine("in the directory where the updater is located.");
                sb.AppendLine();
                sb.AppendLine($"This would currently be this directory:");
                sb.AppendLine(currentDirectory);
                sb.AppendLine();
                sb.AppendLine("You may close this window.");

                StatusText = sb.ToString();
            }
            //Check if an update is available, do a self-copy and run
            else if (args.Count == 2
                  && args[0].StartsWith(Tasks.Github.Updater.CallingApplicationArg)
                  && args[1] == Tasks.Github.Updater.StartUpdateArg)
            {
                StatusText = "Preparing for update... please wait.";

                //Get calling application path
                var callingApplication = args[0].Split('=')[1].Trim('"');
                LogTrace($"{nameof(callingApplication)} = '{callingApplication}'");

                var updater = new Tasks.Github.Updater(Constants.Owner, Constants.Repository);

                var updateCheckResult = await updater.IsUpdateAvailable(versionOfMyself);

                //If no update is available, exit
                if (!updateCheckResult.IsUpdateAvailable)
                {
                    LogTrace($"No update available.");
                    Environment.Exit(666);
                }

                LogTrace($"Update available.");

                //Get required assemblies
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.Location.StartsWith(currentDirectory, StringComparison.OrdinalIgnoreCase))
                    .Select(a => a.Location)
                    .ToList();

                //Get file name without extension
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(currentFilePath);
                LogTrace($"{nameof(fileNameWithoutExtension)} = '{fileNameWithoutExtension}'");

                //Get required files, based on file name
                var files = Directory
                    .EnumerateFiles(currentDirectory)
                    .Where(f => f.StartsWith(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                //Create temp directory
                var tempDir = Directory.CreateTempSubdirectory();
                LogTrace($"{nameof(tempDir)} = '{tempDir}'");

                //Target updater path
                var targetUpdaterPath = Path.Combine(tempDir.FullName, Path.GetFileName(currentFilePath));
                LogTrace($"{nameof(targetUpdaterPath)} = '{targetUpdaterPath}'");

                //Copy self to temp directory
                File.Copy(currentFilePath, targetUpdaterPath);

                //Copy required assemblies
                assemblies.ForEach(assembly =>
                {
                    var destination = Path.Combine(tempDir.FullName, Path.GetFileName(assembly));

                    File.Copy(assembly, destination);
                });

                //Copy required files
                files.ForEach(file =>
                {
                    var destination = Path.Combine(tempDir.FullName, Path.GetFileName(file));
                    File.Copy(file, destination, false); //Do not overwrite, only copy missing files
                });

                LogTrace($"Starting copied version of myself.");

                //Start updater from temp directory
                Process.Start(new ProcessStartInfo(targetUpdaterPath, [$"{Tasks.Github.Updater.CallingApplicationArg}=\"{callingApplication}\"", Tasks.Github.Updater.StartSelfUpdateArg, $"{Tasks.Github.Updater.SourceDirectoryArg}=\"{currentDirectory}\""])
                {
                    CreateNoWindow = true,
                    WorkingDirectory = tempDir.FullName,
                });

                Environment.Exit(0);
            }
            //Start self-update process
            else if (args.Count == 3
                  && args[0].StartsWith(Tasks.Github.Updater.CallingApplicationArg)
                  && args[1] == Tasks.Github.Updater.StartSelfUpdateArg
                  && args[2].StartsWith(Tasks.Github.Updater.SourceDirectoryArg))
            {
                //Get calling application path
                var callingApplication = args[0].Split('=')[1].Trim('"');
                LogTrace($"{nameof(callingApplication)} = '{callingApplication}'");

                //Get source (target for update) directory
                var sourceDirectory = args[2].Split('=')[1].Trim('"');

                var updater = new Tasks.Github.Updater(Constants.Owner, Constants.Repository);

                try
                {
                    var progress = new Progress<double>((Action<double>)(p => Progress = p));

                    StatusText = $"Downloading update... please wait. (Step {++CurrentStep} of {TotalSteps})";

                    //Download update
                    var downloadedFile = await updater.DownloadUpdate(versionOfMyself, progress);

                    if (string.IsNullOrEmpty(downloadedFile))
                    {
                        //Throw exception, skip update and continue with cleanup
                        throw new Exception("No file to download. Likely already up to date.");
                    }

                    LogTrace($"{nameof(downloadedFile)} = '{downloadedFile}'");

                    StatusText = $"Extracting update... please wait. (Step {++CurrentStep} of {TotalSteps})";

                    //Apply update
                    await updater.ApplyUpdate(sourceDirectory, downloadedFile, progress, true);

                    LogTrace($"Applied update.");

                    var callingAppFileName = Path.GetFileName(callingApplication);
                    var callingAppFullPath = Path.Combine(sourceDirectory, callingAppFileName);

                    //Start updated application
                    Process.Start(callingAppFullPath);
                }
                catch (Exception e)
                {
                    var updateFailedText = $"Update failed: {e.FullExceptionString()}";

                    StatusText = updateFailedText;
                    LogTrace(updateFailedText);
                }

                LogTrace($"Update procedure done. Removing myself.");

                //Remove myself
                if (OperatingSystem.IsWindows())
                {
                    //Remove myself with a small delay
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C choice /C Y /N /D Y /T 1 & rmdir /S /Q \"{currentDirectory}\"",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                }
                else if (OperatingSystem.IsLinux())
                {
                    //Remove myself with a small delay
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "/bin/sh",
                        Arguments = $"-c \"sleep 1 && rm -rf '{currentDirectory}'\"",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                }

                Environment.Exit(0);
            }
        }

        #endregion
    }

    public class MockMainWindowViewModel : MainWindowViewModel
    {
        public MockMainWindowViewModel() : base(null)
        {
        }
    }
}
