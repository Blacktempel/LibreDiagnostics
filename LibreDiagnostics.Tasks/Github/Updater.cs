/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using BlackSharp.Core.Logging;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.IO;
using SharpCompress.Readers;
using System.Text.Json;

namespace LibreDiagnostics.Tasks.Github
{
    /// <summary>
    /// Provides functionality to check for, download, and apply updates from the latest release of a specified GitHub repository.
    /// </summary>
    public sealed class Updater
    {
        #region Constructor

        public Updater(string owner, string repository)
        {
            Owner = owner;
            Repository = repository;
        }

        #endregion

        #region Fields

        public const string CallingApplicationArg = "--calling-app";
        public const string StartUpdateArg        = "--start-update";
        public const string StartSelfUpdateArg    = "--start-self-update";
        public const string SourceDirectoryArg    = "--source-directory";

        #endregion

        #region Properties

        public string Owner { get; }
        public string Repository { get; }

        public TimeSpan Timeout { get; } = TimeSpan.FromSeconds(15);

        public string APIUrl => $"https://api.github.com/repos/{Owner}/{Repository}/releases/latest";

        #endregion

        #region Public

        /// <summary>
        /// Determines whether a newer version is available compared to the specified current version.
        /// </summary>
        /// <remarks>This method retrieves the latest release information from a remote API and compares it to the provided version.<br/>
        /// Network connectivity is required for this operation.</remarks>
        /// <param name="currentVersion">The current version of the application to compare against the latest available release. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation.<br/>
        /// The task result is <see langword="true"/> if a newer version is available; otherwise, <see langword="false"/>.</returns>
        public async Task<UpdateCheckResult> IsUpdateAvailable(Version currentVersion)
        {
            using var client = CreateHttpClient();
            using var cts = new CancellationTokenSource(Timeout);

            //Get release info
            var json = await client.GetStringAsync(APIUrl, cts.Token);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            //Get tag name as version
            string tagName = root.GetProperty("tag_name").GetString();
            Version latestVersion = new Version(tagName.TrimStart('v'));

            //Check version
            if (latestVersion <= currentVersion)
            {
                return new(false, null);
            }

            string releaseNotes = null;

            if (root.TryGetProperty("body", out var body))
            {
                releaseNotes = body.GetString();
            }

            return new(true, releaseNotes);
        }

        /// <summary>
        /// Downloads the latest available update if a newer version exists than the specified current version.
        /// </summary>
        /// <remarks>The downloaded update file is saved to the system's temporary directory.<br/>
        /// This method performs network operations and may take time to complete.<br/>
        /// Ensure that the caller has appropriate permissions to write to the temporary directory.</remarks>
        /// <param name="currentVersion">The current version of the application.<br/>
        /// The method will only download an update if a newer version is available.</param>
        /// <param name="progress">An optional progress reporter to receive download progress updates as a percentage.</param>
        /// <returns>The full file path to the downloaded update asset if a newer version is available; otherwise, null if no update is found.</returns>
        /// <exception cref="Exception">Thrown if a new release is available but no downloadable asset is provided for that release.</exception>
        public async Task<string> DownloadUpdate(Version currentVersion, IProgress<double> progress = null)
        {
            using var client = CreateHttpClient();
            using var cts = new CancellationTokenSource(Timeout);

            //Get release info
            var json = await client.GetStringAsync(APIUrl, cts.Token);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            //Get tag name as version
            string tagName = root.GetProperty("tag_name").GetString();
            Version latestVersion = new Version(tagName.TrimStart('v'));

            //Only download if update is available
            if (latestVersion <= currentVersion)
            {
                return null;
            }

            //Get asset
            var assets = root.GetProperty("assets");

            string assetUrl = null;
            foreach (var asset in assets.EnumerateArray())
            {
                //Get asset name and url
                string name = asset.GetProperty("name").GetString();
                string url = asset.GetProperty("browser_download_url").GetString();

                if (name != null && url != null)
                {
                    assetUrl = url;
                }
            }

            //No asset was provided in release
            if (assetUrl == null)
            {
                throw new Exception($"New release (version: {latestVersion}) has no asset to download.{Environment.NewLine}Please try again later.");
            }

            //Get temp download path
            var tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(assetUrl));

            //Download file
            await DownloadFileAsync(client, assetUrl, tempFilePath, progress);

            //Return downloaded file path
            return tempFilePath;
        }

        /// <summary>
        /// Applies an update to the application by replacing its files with those from the specified update package.
        /// </summary>
        /// <remarks>All files in the application directory are permanently deleted before the update is applied.<br/>
        /// Ensure that any important data is backed up prior to calling this method.<br/>
        /// This operation is not reversible.</remarks>
        /// <param name="appPath">The path to the directory where the current application is installed.<br/>
        /// All existing files in this directory will be deleted and replaced by the update.</param>
        /// <param name="updateFilePath">The path to the update package file, typically a ZIP archive containing the updated application files.</param>
        /// <param name="progress">An optional progress reporter to receive download progress updates as a percentage.</param>
        /// <param name="removeUpdateFile">Specifies whether to delete the update package file after the update is applied. Set to <see
        /// langword="true"/> to remove the file; otherwise, it will be retained.</param>
        public async Task ApplyUpdate(string appPath, string updateFilePath, IProgress<double> progress = null, bool removeUpdateFile = true)
        {
            bool success = false;
            Exception lastException = null;

            //Retry extraction a few times in case of file locks
            for (int i = 0; i < 5; ++i)
            {
                try
                {
                    //Clear original files
                    if (Directory.Exists(appPath))
                    {
                        Directory.Delete(appPath, true);
                    }

                    Logger.Instance.Add(LogLevel.Trace, $"Removed directory '{appPath}'.", DateTime.Now);

                    //Extract update
                    using var zipFile = File.OpenRead(updateFilePath);
                    using var stream = SharpCompressStream.Create(zipFile);

                    var options = new ReaderOptions()
                    {
                        ExtractFullPath = true,
                        PreserveFileTime = true,
                        PreserveAttributes = true,
                    };

                    await using var archive = await ArchiveFactory.OpenAsyncArchive(stream, options);

                    //WriteToDirectoryAsync requires to create the directory before extracting to it
                    if (!Directory.Exists(appPath))
                    {
                        Directory.CreateDirectory(appPath);
                    }

                    var prog = new Progress<ProgressReport>(pr => progress?.Report(pr.PercentComplete.GetValueOrDefault()));
                    await archive.WriteToDirectoryAsync(appPath, prog);

                    //Check if extraction path contains only one folder (the main folder of the archive)
                    //If yes, move all files from that folder to the extraction path and delete the folder
                    var entries = Directory.GetFileSystemEntries(appPath);

                    //Only one entry and it's a directory
                    if (entries.Length == 1 && Directory.Exists(entries[0]))
                    {
                        var entry = entries[0];

                        //Move all files from that directory to the extraction path
                        var extractedData = Directory.GetFileSystemEntries(entry);

                        foreach (var file in extractedData)
                        {
                            var destFile = Path.Combine(appPath, Path.GetFileName(file));
                            Logger.Instance.Add(LogLevel.Trace, $"Moving '{file}' -> '{destFile}'.", DateTime.Now);

                            if (Directory.Exists(file))
                            {
                                Directory.Move(file, destFile);
                            }
                            else
                            {
                                File.Move(file, destFile);
                            }
                        }

                        //Delete the now empty directory
                        Directory.Delete(entry, true);
                    }

                    Logger.Instance.Add(LogLevel.Trace, $"Extracted files from '{updateFilePath}' to directory '{appPath}'.", DateTime.Now);

                    success = true;
                }
                catch (Exception e)
                {
                    Logger.Instance.Add(LogLevel.Trace, $"Retrying clear of original files and extraction (counter = {i}).", DateTime.Now);
                    lastException = e;

                    //Wait a bit and retry
                    Thread.Sleep(500);
                    continue;
                }

                break;
            }

            if (!success && lastException != null)
            {
                throw new Exception($"{nameof(ApplyUpdate)} failed.", lastException);
            }

            if (removeUpdateFile)
            {
                Logger.Instance.Add(LogLevel.Trace, $"Removing update file '{updateFilePath}'.", DateTime.Now);

                //Remove update file
                File.Delete(updateFilePath);
            }
        }

        #endregion

        #region Private

        HttpClient CreateHttpClient()
        {
            return new HttpClient
            {
                DefaultRequestHeaders =
                {
                    { "User-Agent", $"{nameof(LibreDiagnostics)}Updater" }
                },
                Timeout = Timeout,
            };
        }

        async Task DownloadFileAsync(HttpClient client, string url, string path, IProgress<double> progress = null)
        {
            const int BufferSize = 4096; //Default size of FileStream

            using var resp = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            resp.EnsureSuccessStatusCode();

            var contentLength = resp.Content.Headers.ContentLength ?? -1;
            await using var input = await resp.Content.ReadAsStreamAsync();
            await using var output = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize, true);

            var buffer = new byte[BufferSize];
            long totalRead = 0;
            int read;

            while ((read = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await output.WriteAsync(buffer, 0, read);
                totalRead += read;

                if (contentLength > 0 && progress != null)
                {
                    double percent = (double)totalRead / contentLength * 100.0;
                    progress.Report(percent);
                }
            }

            await output.FlushAsync();

            if (contentLength > 0 && progress != null)
            {
                progress.Report(100.0);
            }
        }

        #endregion

        #region Records

        public sealed record UpdateCheckResult(bool IsUpdateAvailable, string ReleaseNotes);

        #endregion
    }
}
