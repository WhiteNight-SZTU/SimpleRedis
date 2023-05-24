// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Networking;
using System.Text.Json.Nodes;
using ABI.System;
using TimeSpan = System.TimeSpan;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SimpleRedis.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Home : Page
    {
        public Home()
        {
            this.InitializeComponent();
            TTLSetter.ValueChanged += TTLSetter_ValueChanged;
            KeyNameBox.TextChanging += KeyName_ValueChanged;
        }


        private async void ErrorMessage_CantFindRedis()
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Error!\n错误原因：未能连接至数据库。\n 请检查数据库IP和密码是否设置正确";
            dialog.PrimaryButtonText = "返回";
            dialog.DefaultButton = ContentDialogButton.Primary;
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
            {
                dialog.XamlRoot = this.Content.XamlRoot;
            }
            var result = await dialog.ShowAsync();
        }
        private async void ErrorMessage_CantFindTTLType()
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Error!\n错误原因：未设置TTL类型。\n 请选择TTL的单位";
            dialog.PrimaryButtonText = "返回";
            dialog.DefaultButton = ContentDialogButton.Primary;
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
            {
                dialog.XamlRoot = this.Content.XamlRoot;
            }
            var result = await dialog.ShowAsync();
        }
        private async void ErrorMessage_UnsupportedMethod()
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Error!\n错误原因：不支持的操作。\n 请检查是否设置必要的参数";
            dialog.PrimaryButtonText = "返回";
            dialog.DefaultButton = ContentDialogButton.Primary;
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
            {
                dialog.XamlRoot = this.Content.XamlRoot;
            }
            var result = await dialog.ShowAsync();
        }

        private async void SuccessMessage_SetKeySuccessful()
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = "Success!\n操作成功：SET指令执行成功。\n ";
            dialog.PrimaryButtonText = "返回";
            dialog.DefaultButton = ContentDialogButton.Primary;
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 8))
            {
                dialog.XamlRoot = this.Content.XamlRoot;
            }
            var result = await dialog.ShowAsync();
        }
        public class StringKey
        {
            public string KeyName { get; set; }
            public string KeyValue { get;set; }
            public int TTL { get; set; }
            public StringKey(string keyName,string keyValue,int ttl)
            {
               KeyName= keyName;
               KeyValue = keyValue;
               TTL = ttl;
            }

        }

        private void RedisTypeChanged(object sender,RoutedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if(comboBox.SelectedIndex == 0)
            {
                KeyType = "String";
            }
        }
        private void RedisTTLChanged(object sender,RoutedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox.SelectedIndex == 0)
            {
                TTLType = "s";
            }else if(comboBox.SelectedIndex == 1)
            {
                TTLType = "ms";
            }
            TTLSetter_ValueChanged(null,null);
        }
        private void TTLSetter_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if(TTLType==null)
            {
                ErrorMessage_CantFindTTLType();
            }else
            {
                if (TTLSetter.Value == 0)
                {
                    TimeText.Text = "Key被设置的TTL为: 永不过期";
                }
                else if (TTLType == "s")
                {
                    long seconds = (long)TTLSetter.Value;
                    TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
                    TimeText.Text = $"设置的时间为: {timeSpan.Days}天{timeSpan.Hours}小时{timeSpan.Minutes}分钟{timeSpan.Seconds}秒";
                }
                else if (TTLType == "ms")
                {
                    long milliseconds=(long)TTLSetter.Value;
                    TimeSpan timeSpan = TimeSpan.FromMilliseconds(milliseconds);
                    TimeText.Text = $"设置的时间为: {timeSpan.Days}天{timeSpan.Hours}小时{timeSpan.Minutes}分钟{timeSpan.Seconds}秒{timeSpan.Milliseconds}毫秒";
                }
            }

        }
        private void KeyName_ValueChanged(TextBox e,TextBoxTextChangingEventArgs args)
        {
            if(KeyNameBox.Text!=null)
            {
                KeyExample.Text = $"示例:{KeyNameBox.Text}1 {KeyNameBox.Text}2 ..";
            }
        }
        private void SetRedisKey(object sender,RoutedEventArgs e)
        {
            if(KeyType=="String")
            {
                long times = (long)SetKeyTimes.Value;
                try
                {
                    configurationOptions = new ConfigurationOptions
                    {
                        EndPoints = { $"{Settings.databaseInfo.databaseIP}" },
                        Password = $"{Settings.databaseInfo.databasePassword}"
                    };
                    connectionMultiplexer = ConnectionMultiplexer.Connect(configurationOptions);
                    database = connectionMultiplexer.GetDatabase();

                    long TTL = (long)TTLSetter.Value;
                    TimeSpan timeSpan;
                    if (TTLType== "s")
                    {
                         timeSpan = TimeSpan.FromSeconds((long)TTLSetter.Value);
                    }else if(TTLType == "ms")
                    {
                        timeSpan = TimeSpan.FromMilliseconds((long)TTLSetter.Value);
                    }else
                    {
                        timeSpan = TimeSpan.Zero;
                    }
                    for (int i=1;i<=times;i++)
                    {
                        string keyName = KeyNameBox.Text+$"{i}";
                        string keyValue = KeyValueBox.Text;
                        if(TTL!=0)
                        {
                            database.StringSet(keyName, keyValue, timeSpan);
                        }else
                        {
                            database.StringSet(keyName, keyValue);
                        }
                    }
                    SuccessMessage_SetKeySuccessful();
                }
                catch
                {
                    ErrorMessage_CantFindRedis();
                }

            }
            else
            {
                ErrorMessage_UnsupportedMethod();
            }
        }
        private async void JsonToRedis(object sender, RoutedEventArgs e)
        {
            string fileName = "AppData/RedisKey.json";
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
           try
           { 
                configurationOptions = new ConfigurationOptions
                {
                    EndPoints = { $"{Settings.databaseInfo.databaseIP}" },
                    Password = $"{Settings.databaseInfo.databasePassword}"
                };
                connectionMultiplexer = ConnectionMultiplexer.Connect(configurationOptions);
                database = connectionMultiplexer.GetDatabase();
            }
            catch
            {
                ErrorMessage_CantFindRedis();
            }

            try
            {
                string keyName = null;
                string keyValue = null;
                long TTL = (long)TTLSetter.Value;
                if (File.Exists(filePath))
                {
                    string json = await File.ReadAllTextAsync(filePath);
                    JsonArray data = (JsonArray)JsonArray.Parse(json);
                    TimeSpan timeSpan;
                    if (TTLType == "s")
                    {
                        timeSpan = TimeSpan.FromSeconds((long)TTLSetter.Value);
                    }
                    else if (TTLType == "ms")
                    {
                        timeSpan = TimeSpan.FromMilliseconds((long)TTLSetter.Value);
                    }
                    else
                    {
                        timeSpan = TimeSpan.Zero;
                    }
                    foreach (var item in data)
                    {
                        var props = item.AsObject();
                        foreach (var prop in props)
                        {
                            keyName = prop.Key;
                            keyValue = prop.Value.ToString();
                            if (TTL != 0)
                            {
                                database.StringSet(keyName, keyValue, timeSpan);
                            }
                            else
                            {
                                database.StringSet(keyName, keyValue);
                            }
                        }

                    }
                }
            }
            catch
            {
                ErrorMessage_UnsupportedMethod();
            }
        }
        /*如果不增加“编写.json”的功能
         * 删掉下面的class
         */
        public class KeyInfo
        {
            public string KeyName { get; set; }
            public string KeyValue { get; set; }
        }

        private static string KeyType;
        private static string TTLType;
        private static ConfigurationOptions configurationOptions;
        private static ConnectionMultiplexer connectionMultiplexer;
        private static IDatabase database;
    }
}
