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
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.System;
using static System.Net.Mime.MediaTypeNames;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SimpleRedis.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Github : Page
    {
        public Github()
        {
            this.InitializeComponent();

            MyWebView.NavigationStarting += UpdateUri;
        }
        private void JumpTo(object sender, RoutedEventArgs e)
        {
            try
            {
                string uri=addressBar.Text;
                if (!uri.StartsWith("https://")&&!uri.StartsWith("http://"))
                {
                    uri = "https://" + uri;
                }
                Uri targetUri = new Uri(uri);
                MyWebView.Source = targetUri;

            }
            catch
            {
                ErrorMessage_IHaveNoIdea();
            }
        }
        private void ReturnTo(object sender, RoutedEventArgs e)
        {
            if (MyWebView.CanGoBack)
            {
                MyWebView.GoBack();
            }
        }
        private void Refresh(object sender, RoutedEventArgs e)
        {
            MyWebView.Reload();
        }
        private void UpdateUri(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        {
            addressBar.Text = args.Uri;
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
            else if (e.Key == VirtualKey.F11)
            {
                MainWindow.appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
                MainWindow.current.TitleBarState(false);
            }else if(e.Key==VirtualKey.Enter)
            {
                JumpTo(null,null);
            }
        }

        private async void ErrorMessage_IHaveNoIdea()
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Error!\n错误原因：发生未知错误。\n";
            dialog.PrimaryButtonText = "返回";
            dialog.DefaultButton = ContentDialogButton.Primary;
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
            {
                dialog.XamlRoot = this.Content.XamlRoot;
            }
            var result = await dialog.ShowAsync();
        }
    }
}
