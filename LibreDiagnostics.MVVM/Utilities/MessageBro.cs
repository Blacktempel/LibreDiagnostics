/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using BlackSharp.MVVM.Dialogs;
using BlackSharp.MVVM.Dialogs.Enums;
using LibreDiagnostics.Models.Helper;

namespace LibreDiagnostics.MVVM.Utilities
{
    public static class MessageBro
    {
        #region Public

        public static DialogButtonType DoShowMessage(string title, string message, DialogButtons buttons = DialogButtons.OK)
        {
            return ShowMessage.Invoke(title, message, buttons);
        }

        /// <summary>
        /// Displays a message dialog with the specified title and message, and waits for user interaction or until the
        /// specified timeout elapses.
        /// </summary>
        /// <param name="title">The title text to display in the message dialog. Cannot be null.</param>
        /// <param name="message">The message content to display in the dialog. Cannot be null.</param>
        /// <param name="buttons">The standard dialog buttons to display in the dialog.</param>
        /// <param name="dialogButtons">The custom dialog buttons to display in the dialog. Can be null.</param>
        /// <param name="timeout">The maximum duration to wait for user interaction before automatically closing the dialog.</param>
        /// <param name="timeouted">Outputs whether the dialog timed out due to no user interaction.</param>
        /// <returns>The type of button that was clicked by the user, or a default value if the dialog timed out.</returns>
        public static DialogButtonType DoShowMessageTimeout(string title, string message, DialogButtons buttons, IList<DialogButton> dialogButtons, TimeSpan? timeout, out bool timeouted)
        {
            return ShowMessageTimeout.Invoke(title, message, buttons, dialogButtons, timeout, out timeouted);
        }

        public static DialogButtonType DoShowMessageTimeout(string title, string message, DialogButtons buttons, TimeSpan? timeout, out bool timeouted)
        {
            return DoShowMessageTimeout(title, message, buttons, null, timeout, out timeouted);
        }

        public static void DoOpenSettings()
        {
            OpenSettings?.Invoke();
        }

        public static async Task<string> DoSaveFile()
        {
            return await SaveFile?.Invoke();
        }

        public static void DoShutdownApplication()
        {
            ShutdownApplication?.Invoke();
        }

        public static void DoCheckForUpdate()
        {
            CheckForUpdate?.Invoke();
        }

        public static List<TextValuePair<string>> DoGetScreens()
        {
            return GetScreens?.Invoke() ?? new();
        }

        #endregion

        #region Events

        public delegate DialogButtonType ShowMessageTimeoutHandler(string title, string message, DialogButtons buttons, IList<DialogButton> dialogButtons, TimeSpan? timeout, out bool timeouted);

        public static event Func<string, string, DialogButtons, DialogButtonType> ShowMessage;
        public static event ShowMessageTimeoutHandler ShowMessageTimeout;
        public static event Action OpenSettings;
        public static event Func<Task<string>> SaveFile;
        public static event Action ShutdownApplication;
        public static event Func<List<TextValuePair<string>>> GetScreens;
        public static event Action CheckForUpdate;

        #endregion
    }
}
