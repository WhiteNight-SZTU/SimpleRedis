// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.Storage;
using Windows.Graphics.Printing.Workflow;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Microsoft.UI.Windowing;
using Microsoft.UI;
using Windows.UI.WindowManagement;
using Windows.System;
using System.Net;
using StackExchange.Redis;
using Windows.Foundation.Metadata;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SimpleRedis.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {
        public Settings()
        {
            this.InitializeComponent();
            if (databaseInfo.databaseIP != null)
            {
                accountBox.Password = databaseInfo.databaseIP;
            }
            if (databaseInfo.databasePassword != null)
            {
                passwordBox.Password = databaseInfo.databasePassword;
            }
            if (showDatabase_ip == true)
            {
                revealModeCheckBox_ip.IsChecked = true;
                accountBox.PasswordRevealMode = PasswordRevealMode.Visible;
            }
            if (showDatabase_password == true)
            {
                revealModeCheckBox.IsChecked = true;
                passwordBox.PasswordRevealMode = PasswordRevealMode.Visible;
            }

            preLoadSetting = false;

        }
        private async void changeAppBackground(object sender, RoutedEventArgs e)
        {
            var filePicker = new FileOpenPicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.current);
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);
            filePicker.FileTypeFilter.Add("*");
            var file = await filePicker.PickSingleFileAsync();
            if (file != null)
            {
                BitmapImage bitmapImage = new BitmapImage();
                FileRandomAccessStream stream = (FileRandomAccessStream)await file.OpenAsync(FileAccessMode.Read);
                bitmapImage.SetSource(stream);
                MainWindow.backgroundImage.ImageSource = bitmapImage;
            }
        }
        private void RevealModeCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            if (preLoadSetting == false)
            {
                if (revealModeCheckBox.IsChecked == true)
                {
                    passwordBox.PasswordRevealMode = PasswordRevealMode.Visible;
                    showDatabase_password = true;
                }
                else
                {
                    passwordBox.PasswordRevealMode = PasswordRevealMode.Hidden;
                    showDatabase_password = false;
                }
            }
        }
        private void RevealModeCheckbox_ip_Changed(object sender, RoutedEventArgs e)
        {
            if (preLoadSetting == false)
            {
                if (revealModeCheckBox_ip.IsChecked == true)
                {
                    accountBox.PasswordRevealMode = PasswordRevealMode.Visible;
                    showDatabase_ip = true;
                }
                else
                {
                    accountBox.PasswordRevealMode = PasswordRevealMode.Hidden;
                    showDatabase_password = false;
                }
            }
        }
        private void SETIP(object sender, RoutedEventArgs e)
        {
            databaseInfo.databaseIP = accountBox.Password;
        }
        private void SETPassword(object sender, RoutedEventArgs e)
        {
            databaseInfo.databasePassword = passwordBox.Password;
        }
        public void changeAppWindow(object sender, RoutedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.current);
            WindowId myWndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            MainWindow.appWindow= Microsoft.UI.Windowing.AppWindow.GetFromWindowId(myWndId);

            if(comboBox.SelectedIndex == 0)
            {
                MainWindow.appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
                MainWindow.current.TitleBarState(false);
            }
            else if(comboBox.SelectedIndex == 1)
            {
                MainWindow.appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
                MainWindow.current.TitleBarState(true);
            }else if(comboBox.SelectedIndex == 2)
            {
               MainWindow.appWindow.SetPresenter(AppWindowPresenterKind.Default);
                MainWindow.current.TitleBarState(true);
            }
           
        }
        private void SaveIPAndPassword(object sender, RoutedEventArgs e)
        {
            string fileName = "AppData/InitSettings.json";
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            
            try
            {
                List<DatabaseInfo>_data=new List<DatabaseInfo>();
                _data.Add(new DatabaseInfo()
                {
                    databaseIP = databaseInfo.databaseIP,
                    databasePassword = databaseInfo.databasePassword
                });
                string json = JsonSerializer.Serialize(_data);
                File.WriteAllText(filePath, json);
            }catch
            {
                ErrorMessage_CantSaveIPAndAddress();
            }
        }

        private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.current);
            WindowId myWndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            MainWindow.appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(myWndId);
            if (e.Key == VirtualKey.Escape)
            {
                MainWindow.appWindow.SetPresenter(AppWindowPresenterKind.Default);
                MainWindow.current.TitleBarState(true);
            }
            else if(e.Key==VirtualKey.F11)
            {
                MainWindow.appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
                MainWindow.current.TitleBarState(false);
            }
        }

        private void ConnectToRedis(object sender, RoutedEventArgs e)
        {
            try
            {
                var connectionString = $"{IPAddress.Parse(databaseInfo.databaseIP)},password={databaseInfo.databasePassword}";
                var connection = ConnectionMultiplexer.Connect(connectionString);
                var database = connection.GetDatabase();
                SuccessMessage_CanFindRedis();
                /* Don't forget to change the code to this one!
                   var configurationOptions = new ConfigurationOptions
                   {
                        EndPoints = { "192.168.0.1:6379" },
                        Password = "yourPassword"
                    };
                   var connectionMultiplexer = ConnectionMultiplexer.Connect(configurationOptions);
                   var database = connectionMultiplexer.GetDatabase();
                 */
            }
            catch
            {
                ErrorMessage_CantFindRedis();
            }
        }

        private async void ErrorMessage_CantFindRedis()
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Error!\n错误原因：未能连接至数据库。\n";
            dialog.PrimaryButtonText = "返回";
            dialog.DefaultButton = ContentDialogButton.Primary;
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
            {
                dialog.XamlRoot = this.Content.XamlRoot;
            }
            var result = await dialog.ShowAsync();
        }

        private async void ErrorMessage_CantSaveIPAndAddress()
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Error!\n错误原因：无法保存数据至本地。\n";
            dialog.PrimaryButtonText = "返回";
            dialog.DefaultButton = ContentDialogButton.Primary;
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
            {
                dialog.XamlRoot = this.Content.XamlRoot;
            }
            var result = await dialog.ShowAsync();
        }
        private async void SuccessMessage_CanFindRedis()
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Success!\n操作成功：已成功连接数据库。\n";
            dialog.PrimaryButtonText = "返回";
            dialog.DefaultButton = ContentDialogButton.Primary;
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
            {
                dialog.XamlRoot = this.Content.XamlRoot;
            }
            var result = await dialog.ShowAsync();
        }


        public class DatabaseInfo
        {
            public string databaseIP { get; set; }
            public string databasePassword { get; set; }
        }
        public static DatabaseInfo databaseInfo = new DatabaseInfo();
        private static bool showDatabase_ip = false;
        private static bool showDatabase_password = false;
        private bool preLoadSetting = true;

      
    }
}
