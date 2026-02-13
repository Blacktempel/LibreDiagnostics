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
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Styling;
using LibreDiagnostics.Models.Configuration;
using LibreDiagnostics.Models.Globals;
using LibreDiagnostics.UI.Models;
using LibreDiagnostics.UI.Utilities;
using LibreDiagnostics.UI.Windows;
using System.Runtime.CompilerServices;

using OS = BlackSharp.Core.Platform.OperatingSystem;

namespace LibreDiagnostics.UI
{
    public partial class App : Application
    {
        #region Public

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp(bool isDesigner)
        {
            var builder = AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();


            if (!isDesigner
             && !Global.Settings.UseHardwareAcceleration
             && OS.IsWindows())
            {
                //No hardware acceleration
                return builder
                    .With(new Win32PlatformOptions()
                    {
                        RenderingMode = [Win32RenderingMode.Software]
                    });
            }
            else
            {
                //Avalonia uses hardware acceleration by default
                return builder;
            }
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            if (Design.IsDesignMode)
            {
                Global.Settings = Settings.GetMock();
            }

            //Set font manager
            Global.FontManager = new FontManagerImplementation();

            RuntimeHelpers.RunClassConstructor(typeof(NavigationResolver).TypeHandle);

            if (!Design.IsDesignMode)
            {
                Global.TrayIcon = new TrayIconImplementation(Global.Settings);
            }
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();

                ApplySystemTheme();

                Current.PlatformSettings.ColorValuesChanged += (s, e) =>
                {
                    ApplySystemTheme();
                };

                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();

            Global.IsReady = true;
        }

        #endregion

        #region Private

        void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }

        void ApplySystemTheme()
        {
            var currentTheme = Current.PlatformSettings.GetColorValues()?.ThemeVariant;

            if (currentTheme == PlatformThemeVariant.Dark)
            {
                Current.RequestedThemeVariant = ThemeVariant.Dark;
            }

            if (currentTheme == PlatformThemeVariant.Light)
            {
                Current.RequestedThemeVariant = ThemeVariant.Light;
            }

            //Others are not supported
        }

        #endregion
    }
}
