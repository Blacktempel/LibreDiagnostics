/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2026 Florian K.
*
*/

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using BlackSharp.Core.Extensions;
using BlackSharp.Core.Logging;
using BlackSharp.UI.Avalonia.Markdown.Events;
using LibreDiagnostics.Models.Enums;
using LibreDiagnostics.Models.Logging;
using LibreDiagnostics.MVVM.ViewModels;

namespace LibreDiagnostics.UI.Windows;

/// <summary>
/// Displays the release notes and lets the user confirm or defer an update.
/// </summary>
public partial class UpdateWindow : Window
{
    #region Constructor

    /// <summary>
    /// Creates the update confirmation window.
    /// </summary>
    public UpdateWindow()
    {
        if (Design.IsDesignMode)
        {
            Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
        }

        InitializeComponent();
    }

    internal UpdateWindow(UpdateViewModel viewModel)
        : this()
    {
        DataContext = _viewModel = viewModel;

        _viewModel.CloseRequested += OnCloseRequested;
        Opened += OnWindowOpened;
    }

    #endregion

    #region Fields

    private readonly UpdateViewModel _viewModel;
    private CancellationTokenSource _countdownCancellation;

    #endregion

    #region Internal

    internal async Task<UpdateAction> ShowAsync()
    {
        var desktop = Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var owner = desktop?.MainWindow;

        if (owner?.IsVisible == true)
        {
            await ShowDialog(owner);
        }
        else
        {
            var closed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            Closed += (_, _) => closed.SetResult();

            Show();
            await closed.Task;
        }

        return _viewModel.Result;
    }

    #endregion

    #region Protected

    protected override void OnClosed(EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.CloseRequested -= OnCloseRequested;
            _countdownCancellation?.Cancel();
        }

        base.OnClosed(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (!e.Handled && e.Key == Key.Escape)
        {
            e.Handled = true;
            _viewModel?.CloseCommand.Execute(null);
        }
    }

    #endregion

    #region Private

    private async void OnWindowOpened(object sender, EventArgs e)
    {
        using var cancellation = new CancellationTokenSource();
        _countdownCancellation = cancellation;

        try
        {
            await _viewModel.RunCountdownAsync(cancellation.Token);
        }
        finally
        {
            _countdownCancellation = null;
        }
    }

    private void OnCloseRequested(object sender, EventArgs e)
    {
        Close();
    }

    private async void OnLinkClicked(object sender, MarkdownLinkClickedEventArgs e)
    {
        var launcher = PlatformImpl?.TryGetFeature<ILauncher>();

        if (launcher == null)
        {
            return;
        }

        try
        {
            await launcher.LaunchUriAsync(e.Uri);
        }
        catch (Exception exception)
        {
            LoggerUtilities.ApplicationLogger.Add(LogLevel.Warn, exception.FullExceptionString(), DateTime.Now);
        }
    }

    #endregion
}
