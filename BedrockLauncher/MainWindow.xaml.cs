﻿using System;
using System.Globalization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Forms.Design;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Windows.Data;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Management.Core;
using Windows.Management.Deployment;
using Windows.System;
using BedrockLauncher.Classes;
using System.Windows.Media.Animation;
using BedrockLauncher.Pages;
using BedrockLauncher.Pages.FirstLaunch;
using BedrockLauncher.ViewModels;
using BedrockLauncher.Pages.Settings;
using BedrockLauncher.Pages.Play;
using BedrockLauncher.Pages.News;
using BedrockLauncher.Pages.Preview;
using BedrockLauncher.Pages.Common;
using BedrockLauncher.Controls.Toolbar;
using BedrockLauncher.Handlers;
using BedrockLauncher.UI.Pages.Common;
using BedrockLauncher.UI.Components;

namespace BedrockLauncher
{
    //TODO: (Later On) Community Content / Personal Donations Section

    public partial class MainWindow : Window, IDisposable
    {
        private GameTabs MainPage = new GameTabs();
        private SettingsTabs settingsScreenPage = new SettingsTabs();
        private NewsScreenTabs newsScreenPage = new NewsScreenTabs();

        private Navigator Navigator { get; set; } = new Navigator(true);

        public MainWindow()
        {
            this.DataContext = MainViewModel.Default;
            InitializeComponent();
        }

        private async void Window_Initialized(object sender, EventArgs e)
        {
            Panel.SetZIndex(OverlayFrame, 0);
            Panel.SetZIndex(ErrorFrame, 1);
            Panel.SetZIndex(UpdateButton, 2);

            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
            {
                await Program.OnApplicationLoaded();
                this.NavigateToMainPage();
                StartupArgsHandler.RunStartupArgs();

                bool isFirstLaunch = Properties.LauncherSettings.Default.GetIsFirstLaunch(MainViewModel.Default.Config.profiles.Count());
                if (isFirstLaunch) MainViewModel.Default.SetOverlayFrame(new WelcomePage(), true);
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            MainViewModel.Default.AttemptClose(sender, e);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.Default.PackageManager.Cancel();
        }

        #region Navigation

        public void ResetButtonManager(string buttonName)
        {
            this.Dispatcher.Invoke(() =>
            {
                // just all buttons list
                // ya i know this is really bad, i need to learn mvvm instead of doing this shit
                // but this works fine, at least
                List<ToggleButton> toggleButtons = new List<ToggleButton>() { 
                // main window
                NewsButton.Button,
                BedrockEditionButton.Button,
                SettingsButton.Button,
            };

                foreach (ToggleButton button in toggleButtons) { button.IsChecked = false; }

                if (toggleButtons.Exists(x => x.Name == buttonName))
                {
                    toggleButtons.Where(x => x.Name == buttonName).FirstOrDefault().IsChecked = true;
                }
            });

        }
        public void ButtonManager(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                var toggleButton = sender as ToggleButton;
                string name = toggleButton.Name;
                Task.Run(() => ButtonManager_Base(name));
            });
        }
        public void ButtonManager_Base(string senderName)
        {
            this.Dispatcher.Invoke(() =>
            {
                ResetButtonManager(senderName);

                if (senderName == BedrockEditionButton.Name) NavigateToMainPage();
                else if (senderName == NewsButton.Name) NavigateToNewsPage();
                else if (senderName == SettingsButton.Name) NavigateToSettings();
            });

        }

        public void NavigateToNewsPage()
        {
            this.Dispatcher.Invoke(() =>
            {
                Navigator.UpdatePageIndex(0);
                NewsButton.Button.IsChecked = true;
                Task.Run(() => Navigator.Navigate(MainWindowFrame, newsScreenPage));
            });

        }
        public void NavigateToMainPage()
        {
            this.Dispatcher.Invoke(() =>
            {
                Navigator.UpdatePageIndex(1);
                BedrockEditionButton.Button.IsChecked = true;
                Task.Run(() => Navigator.Navigate(MainWindowFrame, MainPage));
            });

        }

        public void NavigateToSettings()
        {
            this.Dispatcher.Invoke(() =>
            {
                Navigator.UpdatePageIndex(4);
                SettingsButton.Button.IsChecked = true;
                Task.Run(() => Navigator.Navigate(MainWindowFrame, settingsScreenPage));
            });

        }

        #endregion

        #region Toolbar Button Events

        private void BedrockEditionButton_Click(object sender, EventArgs e)
        {
            if (sender != null && sender is ToolbarButtonBase) ButtonManager_Base((sender as ToolbarButtonBase).Name);
        }

        private void NewsButton_Click(object sender, EventArgs e)
        {
            if (sender != null && sender is ToolbarButtonBase) ButtonManager_Base((sender as ToolbarButtonBase).Name);
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            if (sender != null && sender is ToolbarButtonBase) ButtonManager_Base((sender as ToolbarButtonBase).Name);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {

        }

        #endregion


    }
}
