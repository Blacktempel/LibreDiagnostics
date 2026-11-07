/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using BlackSharp.Core.Logging;

namespace LibreDiagnostics.Models.Logging
{
    public class LoggerUtilities
    {
        #region Fields

        const string LogFolder = "AppLogs";
        static string LogFile = string.Empty;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the in-memory logger shared by the application and its hardware models.
        /// </summary>
        public static Logger ApplicationLogger { get; } = new();

        #endregion

        #region Public

        public static void SetLogFilePath(DateTime dateTime)
        {
            if (!Directory.Exists(LogFolder))
            {
                Directory.CreateDirectory(LogFolder);
            }

            LogFile = Path.Combine(LogFolder, $"{dateTime:yyyyMMdd-HHmmss}_Log.txt");
        }

        public static void SaveLogFile()
        {
            ApplicationLogger.SaveToFile(LogFile, false);
        }

        #endregion
    }
}
