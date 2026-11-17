/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2026 Florian K.
*
*/

using BlackSharp.MVVM.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibreDiagnostics.Language.Resources;
using LibreDiagnostics.Models.Enums;
using System.Diagnostics;

namespace LibreDiagnostics.MVVM.ViewModels;

/// <summary>
/// Provides the release notes, countdown and actions for the update dialog.
/// </summary>
public partial class UpdateViewModel : ViewModelBase
{
    #region Constructor

    /// <summary>
    /// Creates the update dialog state for a manual check or an update reminder.
    /// </summary>
    public UpdateViewModel(string releaseNotes, TimeSpan timeout, bool showReminderButtons)
    {
        _timeout = timeout;

        ReleaseNotes        = releaseNotes ?? string.Empty;
        Countdown           = FormatCountdown(timeout);
        ShowReminderButtons = showReminderButtons;
    }

    #endregion

    #region Fields

    private static readonly TimeSpan s_countdownInterval = TimeSpan.FromSeconds(1);

    private readonly TimeSpan _timeout;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the localized dialog title.
    /// </summary>
    public string Title => ShowReminderButtons ? $"{Resources.AppName} {Resources.UpdateAvailableTitle}" : Resources.UpdateAvailableTitle;

    /// <summary>
    /// Gets the localized prompt displayed above the release notes.
    /// </summary>
    public string UpdateMessage =>
        string.Format(Resources.UpdateAvailableMessage.Replace(@"\n", Environment.NewLine), string.Empty).TrimEnd();

    /// <summary>
    /// Gets the unformatted Markdown from the release.
    /// </summary>
    public string ReleaseNotes { get; }

    /// <summary>
    /// Gets whether the dialog offers a reminder instead of a close button.
    /// </summary>
    public bool ShowReminderButtons { get; }

    private string _Countdown;
    /// <summary>
    /// Gets the time left to respond.
    /// </summary>
    public string Countdown
    {
        get { return _Countdown; }
        private set { SetField(ref _Countdown, value); }
    }

    /// <summary>
    /// Gets the selected action, if any.
    /// </summary>
    public UpdateAction Result { get; private set; }

    #endregion

    #region Public

    /// <summary>
    /// Updates the countdown and requests closure when the time to respond elapses.
    /// </summary>
    public async Task RunCountdownAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var remaining = _timeout - stopwatch.Elapsed;

                if (remaining <= TimeSpan.Zero)
                {
                    RequestClose(UpdateAction.None);

                    return;
                }

                Countdown = FormatCountdown(remaining);

                await Task.Delay(remaining < s_countdownInterval ? remaining : s_countdownInterval, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The window closed before the countdown elapsed.
        }
    }

    #endregion

    #region Private

    [RelayCommand]
    private void Update()
    {
        RequestClose(UpdateAction.Update);
    }

    [RelayCommand]
    private void RemindInOneWeek()
    {
        RequestClose(UpdateAction.RemindInOneWeek);
    }

    [RelayCommand]
    private void RemindInOneMonth()
    {
        RequestClose(UpdateAction.RemindInOneMonth);
    }

    [RelayCommand]
    private void Close()
    {
        RequestClose(UpdateAction.None);
    }

    private void RequestClose(UpdateAction action)
    {
        Result = action;
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private static string FormatCountdown(TimeSpan remaining)
    {
        var time = TimeSpan.FromSeconds(Math.Ceiling(remaining.TotalSeconds));

        return time.ToString(@"m\:ss");
    }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when an action is selected or the countdown elapses.
    /// </summary>
    public event EventHandler CloseRequested;

    #endregion
}
