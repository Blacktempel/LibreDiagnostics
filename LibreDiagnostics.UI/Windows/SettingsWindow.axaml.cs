/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using LibreDiagnostics.MVVM.ViewModels;

namespace LibreDiagnostics.UI.Windows;

public partial class SettingsWindow : FlatWindow
{
    #region Constructor

    public SettingsWindow()
    {
        if (Design.IsDesignMode)
        {
            Background = Brushes.Black;
            Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark;
        }
        else
        {
            DataContext = new SettingsViewModel() { CloseCommand = new RelayCommand(Close) };
        }

        InitializeComponent();
    }

    #endregion

    #region Properties

    protected override Type StyleKeyOverride => typeof(FlatWindow);

    #endregion
}
