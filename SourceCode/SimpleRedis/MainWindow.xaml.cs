// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SimpleRedis.Pages;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel.Core;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.WindowManagement;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SimpleRedis
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            current = this;
            backgroundImage = ApplicationBackgroundImage;
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
            InitDatabase();
        }

        private void LoadPages(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = (Microsoft.UI.Xaml.Controls.NavigationViewItem)args.SelectedItem;
            string pageName = "SimpleRedis.Pages." + ((string)selectedItem.Tag);
            Type pageType = Type.GetType(pageName);
            if (pageType == null)
            {
                //Should never happened.
            }
            else
            {
                pagesFrame.Navigate(pageType);
                currentFrame = pagesFrame;
                currentPagesFrame = pagesFrame;
            }

        }
       
        private async void InitDatabase()
        {
            string fileName = "AppData/InitSettings.json";
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,fileName);

            string databaseIP = null;
            string databasePassword = null;
            if (File.Exists(filePath))
            {
                string json = await File.ReadAllTextAsync(filePath);
                JsonArray data = (JsonArray)JsonArray.Parse(json);
                foreach (var item in data)
                {
                    if(item["databaseIP"] != null)
                        databaseIP = item["databaseIP"].ToString();
                    if (item["databasePassword"] != null)
                        databasePassword = item["databasePassword"].ToString();
                }

                Settings.databaseInfo.databaseIP = databaseIP;
                Settings.databaseInfo.databasePassword = databasePassword;
            }

        }
        public void TitleBarState(bool visibility)
        {
            if(visibility==false)
            {
                AppTitleBar.Visibility = Visibility.Collapsed;
                MainFrame.Margin=new Thickness(0);
                ExtendsContentIntoTitleBar = false;
            }else if(visibility==true)
            {
                AppTitleBar.Visibility=Visibility.Visible;
                MainFrame.Margin = new Thickness(0, 29.5, 0, 0);
                ExtendsContentIntoTitleBar = true;
            }
        }

    
        private void OnKeyDown(CoreWindow sender, KeyEventArgs e)
        {
            if (e.VirtualKey == VirtualKey.Escape)
            {
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.current);
                WindowId myWndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
                MainWindow.appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(myWndId);
                MainWindow.appWindow.SetPresenter(AppWindowPresenterKind.Default);
                MainWindow.current.TitleBarState(true);
            }
        }


        public static Microsoft.UI.Windowing.AppWindow appWindow;
        public static MainWindow current;
        public static Microsoft.UI.Xaml.Controls.Frame currentFrame;
        public static Microsoft.UI.Xaml.Media.ImageBrush backgroundImage;
        public static Frame currentPagesFrame;
    }
}
