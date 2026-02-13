/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using LibreDiagnostics.UI;
using System.Linq;
using System.Threading.Tasks;

namespace LibreDiagnostics
{
    internal sealed class Program
    {
        public static async Task Main(string[] args)
        {
            await Client.Start(args?.ToList() ?? new());
        }

        //For Avalonia Designer; do not remove, even if it shows "unused"
        static object BuildAvaloniaApp() => App.BuildAvaloniaApp(true);
    }
}
