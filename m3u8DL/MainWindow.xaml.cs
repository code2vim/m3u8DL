using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace m3u8DL
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        [Obsolete]
        public MainWindow()
        {
            InitializeComponent();

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 2 && args[1].ToLower().StartsWith("m3u8dl:")) {
                Window downloadWindow = new DownloadWindow();
                App.Current.MainWindow = downloadWindow;

                this.Close();
                downloadWindow.Show();
            }

            if (Properties.Settings.Default.FilePath == null || Properties.Settings.Default.FilePath == "")
            {
                Properties.Settings.Default.FilePath = "C:\\Users\\USER_NAME\\Downloads".Replace("USER_NAME", Environment.UserName);
                Properties.Settings.Default.Save();
            }

            userDownloadPath.Text = Properties.Settings.Default.FilePath;

            Show_Ad(mainAd_0, mainAd_1, mainAd_2);
        }

        private void Change_Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                Properties.Settings.Default.FilePath = dialog.FileName;
                Properties.Settings.Default.Save();

                userDownloadPath.Text = Properties.Settings.Default.FilePath;
            }
        }

        public static bool RegisterUriScheme(string scheme, string applicationPath)
        {
            try
            {
                using (var schemeKey = Registry.ClassesRoot.CreateSubKey(scheme, writable: true))
                {
                    schemeKey.SetValue("", "URL:m3u8DL Protocol");
                    schemeKey.SetValue("URL Protocol", "");
                    using (var defaultIconKey = schemeKey.CreateSubKey("DefaultIcon"))
                    {
                        defaultIconKey.SetValue("", $"\"{applicationPath}\",1");
                    }
                    using (var shellKey = schemeKey.CreateSubKey("shell"))
                    using (var openKey = shellKey.CreateSubKey("open"))
                    using (var commandKey = openKey.CreateSubKey("command"))
                    {
                        commandKey.SetValue("", $"\"{applicationPath}\" \"%1\"");
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return false;
        }

        public static void RequireElevated(string cmd)
        {
            if (!UACHelper.UACHelper.IsElevated)
            {
                MessageBox.Show("请以管理员身份运行");
                return;
                //Environment.Exit(0);
            }
        }

        private void Download_Button_Click(object sender, RoutedEventArgs e)
        {
            string downloadUrlText = downUrl.Text.ToString();
            if(downloadUrlText.Trim() == "")
            {
                MessageBox.Show("下载链接不能为空!");
                return;
            }

            string downloadPath = Properties.Settings.Default.FilePath;
            if (!Directory.Exists(downloadPath)) {
                MessageBox.Show("下载目录不存在!");
                return;
            }

            string[] downloadUrlArgs = { downloadUrlText, "--workDir", downloadPath };
            Application.Current.Properties["downloadUrlArgs"] = downloadUrlArgs;

            //转向 DownloadWindow 窗口
            Window downloadWindow = new DownloadWindow();
            App.Current.MainWindow = downloadWindow;

            this.Close();
            downloadWindow.Show();
        }

        private void Reg_Button_Click(object sender, RoutedEventArgs e)
        {
            string[] args = { "--registerUrlProtocol " };
            RequireElevated(string.Join(" ", args));
            bool result = RegisterUriScheme("m3u8dl", Assembly.GetExecutingAssembly().Location);
            MessageBox.Show(result ? "成功" : "失败");
        }

        [Obsolete]
        private void Show_Ad(Image mainAd_0, Image mainAd_1, Image mainAd_2)
        {
            string adJsonData = Global.GetAdJsonData();
            if (string.IsNullOrEmpty(adJsonData)) { return; }
            JObject adJson = (JObject)JsonConvert.DeserializeObject(adJsonData);
            if ((int)adJson["code"] != 0) { return; }

            JArray adData = (JArray)adJson["data"];
            // 0
            JObject ad_0 = (JObject)adData[0];
            mainAd_0.Source = BitmapFrame.Create(new Uri((string)ad_0["image"], false), BitmapCreateOptions.None, BitmapCacheOption.Default);
            if ((string)ad_0["type"] == "link") 
            {
                mainAd_0.MouseDown += (sender, e) => { AdClickSkip(sender, e, (string)ad_0["link"]); };
                mainAd_0.Cursor = Cursors.Hand;
            }

            // 1
            JObject ad_1 = (JObject)adData[1];
            mainAd_1.Source = BitmapFrame.Create(new Uri((string)ad_1["image"], false), BitmapCreateOptions.None, BitmapCacheOption.Default);
            if ((string)ad_1["type"] == "link")
            {
                mainAd_1.MouseDown += (sender, e) => { AdClickSkip(sender, e, (string)ad_1["link"]); };
                mainAd_1.Cursor = Cursors.Hand;
            }

            // 2
            JObject ad_2 = (JObject)adData[2];
            mainAd_2.Source = BitmapFrame.Create(new Uri((string)ad_2["image"], false), BitmapCreateOptions.None, BitmapCacheOption.Default);
            if ((string)ad_2["type"] == "link")
            {
                mainAd_2.MouseDown += (sender, e) => { AdClickSkip(sender, e, (string)ad_2["link"]); };
                mainAd_2.Cursor = Cursors.Hand;
            }
        }

        private void AdClickSkip(object sender, RoutedEventArgs e, string link)
        {
            //System.Diagnostics.Process.Start("explorer.exe", link);
            System.Diagnostics.Process.Start(link);
        }
    }
}
