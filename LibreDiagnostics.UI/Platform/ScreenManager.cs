/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2026 Florian K.
*
*/

using Avalonia.Platform;
using BlackSharp.Core.Logging;
using BlackSharp.UI.Avalonia.Extensions;
using LibreDiagnostics.Models.Enums;
using LibreDiagnostics.Models.Platform;
using System.Text.RegularExpressions;

namespace LibreDiagnostics.UI.Platform
{
    internal static class ScreenManager
    {
        #region Public

        public static List<ScreenModel> GetScreens(IReadOnlyList<Screen> screens)
        {
            var list = new List<ScreenModel>();

            //First try to use serial number as unique identifier
            foreach (var screen in screens)
            {
                list.Add(new()
                {
                    Name = screen.DisplayName,
                    Strategy = ScreenIdentificationStrategy.SerialNumber,
                    ScreenID = GetScreenID(screen, ScreenIdentificationStrategy.SerialNumber)
                });
            }

            //If no duplicate IDs, return the list
            if (!HasDuplicateID(list))
            {
                return list;
            }

            //Serial numbers are not unique (probably lazy and/or cheap manufacturer), so we have to use a different strategy
            foreach (var screen in screens)
            {
                list.Add(new()
                {
                    Name = screen.DisplayName,
                    Strategy = ScreenIdentificationStrategy.HardwareID,
                    ScreenID = GetScreenID(screen, ScreenIdentificationStrategy.HardwareID)
                });
            }

            //If no duplicate IDs, return the list
            if (!HasDuplicateID(list))
            {
                return list;
            }

            //Hardware IDs are not unique so just use the full device path as a last resort
            foreach (var screen in screens)
            {
                list.Add(new()
                {
                    Name = screen.DisplayName,
                    Strategy = ScreenIdentificationStrategy.FullDevicePath,
                    ScreenID = GetScreenID(screen, ScreenIdentificationStrategy.FullDevicePath)
                });
            }

            if (HasDuplicateID(list))
            {
                Logger.Instance.Add(LogLevel.Warn, $"{nameof(GetScreens)}: {nameof(ScreenIdentificationStrategy)} failed for all options.", DateTime.Now);
            }

            return list;
        }

        public static string GetScreenID(Screen screen, ScreenIdentificationStrategy strategy)
        {
            switch (strategy)
            {
                case ScreenIdentificationStrategy.SerialNumber:
                    return screen.GetSerialNumber();
                case ScreenIdentificationStrategy.HardwareID:
                    var path = screen.GetDevicePath();

                    var regex = new Regex(@"DISPLAY#([^#]+)#([^#]+)", RegexOptions.IgnoreCase);

                    var match = regex.Match(path);
                    if (match.Success && match.Groups.Count >= 3)
                    {
                        return @$"{match.Groups[1].Value}\{match.Groups[2].Value}";
                    }

                    break;
                case ScreenIdentificationStrategy.FullDevicePath:
                    return screen.GetDevicePath();
            }

            return null;
        }

        #endregion

        #region Private

        static bool HasDuplicateID(List<ScreenModel> list)
        {
            //Check if any ID is "valid" for more than one screen
            return list.Where(x => x.ScreenID != null)
                      .GroupBy(x => x.ScreenID)
                      .Any(g => g.Count() > 1);
        }

        #endregion
    }
}
