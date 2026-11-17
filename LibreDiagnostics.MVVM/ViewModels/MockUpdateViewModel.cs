/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2026 Florian K.
*
*/

namespace LibreDiagnostics.MVVM.ViewModels;

/// <summary>
/// Provides sample release notes for the AXAML designer.
/// </summary>
public class MockUpdateViewModel : UpdateViewModel
{
    #region Constructor

    /// <summary>
    /// Creates a preview of the update reminder.
    /// </summary>
    public MockUpdateViewModel()
        : base(ReleaseNotesSample, TimeSpan.FromMinutes(1), true)
    {
    }

    #endregion

    #region Fields

    private const string ReleaseNotesSample = """
        ## What's changed

        - Added more detailed **sensor readings**.
        - Improved the layout of the hardware window.

        ### Fixes

        - Restored the last position after switching monitors.

        [View the release on GitHub](https://github.com/Blacktempel/LibreDiagnostics/releases)
        """;

    #endregion
}
