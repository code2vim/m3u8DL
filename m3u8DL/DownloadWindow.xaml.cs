using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualBasic;
using System.Reflection;
using NiL.JS;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Net;
using Microsoft.JScript;
using Convert = System.Convert;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Collections;
using Newtonsoft.Json;

namespace m3u8DL
{
    /// <summary>
    /// DownloadWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadWindow : Window
    {
        [Obsolete]
        public DownloadWindow()
        {
            InitializeComponent();

            //FFmpeg.FFMPEG_PATH = Path.Combine(Environment.CurrentDirectory, "ffmpeg.exe");

            if (!File.Exists(Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "NO_UPDATE")))
            {
                Thread checkUpdate = new Thread(() =>
                {
                    //Global.CheckUpdate();
                });
                checkUpdate.IsBackground = true;
                checkUpdate.Start();
            }

            string[] CommandArgs = Environment.GetCommandLineArgs();
            string[] useArgs = { };

            ObservableCollection<Piece> pieceItems = new ObservableCollection<Piece>();
            filePiece.ItemsSource = pieceItems;

            DownloadDetail downloadDetail = new DownloadDetail() { FileName = "获取中...", FilePath = "获取中...", FileDuration = "时长: 0m0s 进度: 0MB/0MB", ProgressDesc = "进度: 0/0 &lt;0%>" };
            downloadWrap.DataContext = downloadDetail;

            if (Application.Current.Properties.Contains("downloadUrlArgs"))
            {
                useArgs = (string[])Application.Current.Properties["downloadUrlArgs"];
                Application.Current.Properties.Remove("downloadUrlArgs");
            }
            
            if (CommandArgs.Length == 2 && CommandArgs[1].ToLower().StartsWith("m3u8dl:"))
            {
                var base64 = CommandArgs[1].Replace("m3u8dl://", "").Replace("m3u8dl:", "");
                var cmd = "";
                try { cmd = Encoding.UTF8.GetString(Convert.FromBase64String(base64)); }
                catch (FormatException) { cmd = Encoding.UTF8.GetString(Convert.FromBase64String(base64.TrimEnd('/'))); }
                //修正工作目录
                Environment.CurrentDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                useArgs = Global.ParseArguments(cmd).ToArray();  //解析命令行
            }

            var cmdParser = new CommandLine.Parser(with => with.HelpWriter = null);
            var parserResult = cmdParser.ParseArguments<MyOptions>(useArgs);

            //解析命令行
            Task.Run(() =>
            {
                parserResult
                  .WithParsed(o => DoWork(o, pieceItems, pieceProgressBar, downloadDetail))
                  .WithNotParsed(errs => DisplayHelp(parserResult, errs));
            });

            Show_Ad(mainAd_0, mainAd_1, mainAd_2);
        }

        private static void DoWork(MyOptions o, ObservableCollection<Piece> pieceItems, ProgressBar pieceProgressBar, DownloadDetail downloadDetail)
        {
            try
            {
                Global.WriteInit();
                //当前程序路径（末尾有\）
                string CURRENT_PATH = Directory.GetCurrentDirectory();
                string fileName = Global.GetValidFileName(o.SaveName);
                string reqHeaders = o.Headers;
                string muxSetJson = o.MuxSetJson ?? "MUXSETS.json";
                string workDir = CURRENT_PATH + "\\Downloads";
                string keyFile = "";
                string keyBase64 = "";
                string keyIV = "";
                string baseUrl = "";
                Global.STOP_SPEED = o.StopSpeed;
                Global.MAX_SPEED = o.MaxSpeed;
                if (!string.IsNullOrEmpty(o.UseKeyBase64)) keyBase64 = o.UseKeyBase64;
                if (!string.IsNullOrEmpty(o.UseKeyIV)) keyIV = o.UseKeyIV;
                if (!string.IsNullOrEmpty(o.BaseUrl)) baseUrl = o.BaseUrl;
                if (o.EnableBinaryMerge) DownloadManager.BinaryMerge = true;
                if (o.DisableDateInfo) FFmpeg.WriteDate = false;
                if (o.NoProxy) Global.NoProxy = true;
                if (o.DisableIntegrityCheck) DownloadManager.DisableIntegrityCheck = true;
                if (o.EnableAudioOnly) Global.VIDEO_TYPE = "IGNORE";
                if (!string.IsNullOrEmpty(o.WorkDir))
                {
                    workDir = Environment.ExpandEnvironmentVariables(o.WorkDir);
                    downloadDetail.FileName = fileName;
                    downloadDetail.FilePath = workDir;
                    DownloadManager.HasSetDir = true;
                }

                //CHACHA20
                if (o.EnableChaCha20 && !string.IsNullOrEmpty(o.ChaCha20KeyBase64) && !string.IsNullOrEmpty(o.ChaCha20NonceBase64))
                {
                    Downloader.EnableChaCha20 = true;
                    Downloader.ChaCha20KeyBase64 = o.ChaCha20KeyBase64;
                    Downloader.ChaCha20NonceBase64 = o.ChaCha20NonceBase64;
                }

                //Proxy
                if (!string.IsNullOrEmpty(o.ProxyAddress))
                {
                    var proxy = o.ProxyAddress;
                    if (proxy.StartsWith("http://"))
                        Global.UseProxyAddress = proxy;
                    if (proxy.StartsWith("socks5://"))
                        Global.UseProxyAddress = proxy;
                }
                //Key
                if (!string.IsNullOrEmpty(o.UseKeyFile))
                {
                    if (File.Exists(o.UseKeyFile))
                        keyFile = o.UseKeyFile;
                }

                if (File.Exists(Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "headers.txt")))
                    reqHeaders = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "headers.txt"));

                if (!string.IsNullOrEmpty(o.LiveRecDur))
                {
                    //时间码
                    Regex reg2 = new Regex(@"(\d+):(\d+):(\d+)");
                    var t = o.LiveRecDur;
                    if (reg2.IsMatch(t))
                    {
                        int HH = Convert.ToInt32(reg2.Match(t).Groups[1].Value);
                        int MM =  Convert.ToInt32(reg2.Match(t).Groups[2].Value);
                        int SS = Convert.ToInt32(reg2.Match(t).Groups[3].Value);
                        HLSLiveDownloader.REC_DUR_LIMIT = SS + MM * 60 + HH * 60 * 60;
                    }
                }
                if (!string.IsNullOrEmpty(o.DownloadRange))
                {
                    string p = o.DownloadRange;

                    if (p.Contains(":"))
                    {
                        //时间码
                        Regex reg2 = new Regex(@"((\d+):(\d+):(\d+))?-((\d+):(\d+):(\d+))?");
                        if (reg2.IsMatch(p))
                        {
                            Parser.DurStart = reg2.Match(p).Groups[1].Value;
                            Parser.DurEnd = reg2.Match(p).Groups[5].Value;
                            if (Parser.DurEnd == "00:00:00") Parser.DurEnd = "";
                            Parser.DelAd = false;
                        }
                    }
                    else
                    {
                        //数字
                        Regex reg = new Regex(@"(\d*)-(\d*)");
                        if (reg.IsMatch(p))
                        {
                            if (!string.IsNullOrEmpty(reg.Match(p).Groups[1].Value))
                            {
                                Parser.RangeStart = Convert.ToInt32(reg.Match(p).Groups[1].Value);
                                Parser.DelAd = false;
                            }
                            if (!string.IsNullOrEmpty(reg.Match(p).Groups[2].Value))
                            {
                                Parser.RangeEnd = Convert.ToInt32(reg.Match(p).Groups[2].Value);
                                Parser.DelAd = false;
                            }
                        }
                    }
                }

                int inputRetryCount = 20;
            input:
                string testurl = o.Input;

                //重试太多次，退出
                if (inputRetryCount == 0)
                    Environment.Exit(-1);

                if (fileName == "")
                    fileName = Global.GetUrlFileName(testurl) + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");


                if (testurl.Contains("twitcasting") && testurl.Contains("/fmp4/"))
                {
                    DownloadManager.BinaryMerge = true;
                }

                string m3u8Content = string.Empty;
                bool isVOD = true;

                //避免文件路径过长
                if (workDir.Length >= 200)
                {
                    //目录不能随便改 直接抛出异常
                    throw new Exception("保存目录过长!");
                }
                else if (workDir.Length + fileName.Length >= 200)
                {
                    //尝试缩短文件名
                    while (workDir.Length + fileName.Length >= 200)
                    {
                        fileName = fileName.Substring(0, fileName.Length - 1);
                    }
                }

                //开始解析

                LOGGER.PrintLine($"文件名称：{fileName}");
                LOGGER.PrintLine($"存储路径：{Path.GetDirectoryName(Path.Combine(workDir, fileName))}");

                Parser parser = new Parser();
                parser.DownName = fileName;
                parser.DownDir = Path.Combine(workDir, parser.DownName);
                parser.M3u8Url = testurl;
                parser.KeyBase64 = keyBase64;
                parser.KeyIV = keyIV;
                parser.KeyFile = keyFile;
                if (baseUrl != "")
                    parser.BaseUrl = baseUrl;
                parser.Headers = reqHeaders;
                string exePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                LOGGER.LOGFILE = Path.Combine(exePath, "Logs", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff") + ".log");
                LOGGER.InitLog();
                LOGGER.WriteLine("开始解析" + testurl);
                LOGGER.PrintLine("开始解析" + " " + testurl, LOGGER.Warning);
                if (testurl.EndsWith(".json") && File.Exists(testurl))  //可直接跳过解析
                {
                    if (!Directory.Exists(Path.Combine(workDir, fileName)))//若文件夹不存在则新建文件夹   
                        Directory.CreateDirectory(Path.Combine(workDir, fileName)); //新建文件夹  
                    File.Copy(testurl, Path.Combine(Path.Combine(workDir, fileName), "meta.json"), true);
                }
                else
                {
                    parser.Parse();  //开始解析
                }

                //仅解析模式
                if (o.EnableParseOnly)
                {
                    LOGGER.PrintLine("解析m3u8成功, 程序退出");
                    Environment.Exit(0);
                }

                if (File.Exists(Path.Combine(Path.Combine(workDir, fileName), "meta.json")))
                {
                    JObject initJson = JObject.Parse(File.ReadAllText(Path.Combine(Path.Combine(workDir, fileName), "meta.json")));
                    isVOD = Convert.ToBoolean(initJson["m3u8Info"]["vod"].ToString());
                    string segCount = initJson["m3u8Info"]["count"].ToString();
                    //传给Watcher总时长
                    Watcher.TotalDuration = initJson["m3u8Info"]["totalDuration"].Value<double>();
                    LOGGER.PrintLine($"文件时长：{Global.FormatTime((int)Watcher.TotalDuration)}");
                    LOGGER.PrintLine("总分片：" + initJson["m3u8Info"]["originalCount"].Value<int>()
                        + $", 已选择分片：" + initJson["m3u8Info"]["count"].Value<int>());

                    // 创建文件分片 items
                    //int count = initJson["m3u8Info"]["count"].Value<int>();
                    Application.Current?.Dispatcher.InvokeAsync(() =>
                    {
                        for (int i = 0; i < Convert.ToInt32(segCount); i++)
                        {
                            pieceItems.Add(new Piece() { Status = 0, Color = "#fff" });
                        }
                    });
                }
                else
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(workDir, fileName));
                    directoryInfo.Delete(true);
                    LOGGER.PrintLine("地址无效", LOGGER.Error);
                    inputRetryCount--;
                    goto input;
                }
                //pieceProgressBar.Value = 10;
                //点播
                if (isVOD == true)
                {
                    ServicePointManager.DefaultConnectionLimit = 10000;
                    DownloadManager md = new DownloadManager();
                    md.DownDir = parser.DownDir;
                    md.Headers = reqHeaders;
                    md.Threads = Environment.ProcessorCount;
                    if (md.Threads > o.MaxThreads)
                        md.Threads = (int)o.MaxThreads;
                    if (md.Threads < o.MinThreads)
                        md.Threads = (int)o.MinThreads;
                    if (File.Exists("minT.txt"))
                    {
                        int t = Convert.ToInt32(File.ReadAllText("minT.txt"));
                        if (md.Threads <= t)
                            md.Threads = t;
                    }
                    md.TimeOut = (int)(o.TimeOut * 1000);
                    md.NoMerge = o.NoMerge;
                    md.DownName = fileName;
                    md.DelAfterDone = o.EnableDelAfterDone;
                    md.MuxFormat = "mp4";
                    md.RetryCount = (int)o.RetryCount;
                    md.MuxSetJson = muxSetJson;
                    md.MuxFastStart = o.EnableMuxFastStart;
                    md.DoDownload(pieceItems, Application.Current?.Dispatcher, pieceProgressBar, downloadDetail);
                }
                //直播
                if (isVOD == false)
                {
                    LOGGER.WriteLine("识别为直播流, 开始录制...");
                    LOGGER.PrintLine("识别为直播流, 开始录制...");
                    //LOGGER.STOPLOG = true;  //停止记录日志
                    //开辟文件流，且不关闭。（便于播放器不断读取文件）
                    string LivePath = Path.Combine(Directory.GetParent(parser.DownDir).FullName
                        , DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "_" + fileName + ".ts");
                    FileStream outputStream = new FileStream(LivePath, FileMode.Append);

                    HLSLiveDownloader live = new HLSLiveDownloader();
                    live.DownDir = parser.DownDir;
                    live.Headers = reqHeaders;
                    live.LiveStream = outputStream;
                    live.LiveFile = LivePath;
                    live.TimerStart();  //开始录制
                    //Console.ReadKey();
                }

                LOGGER.WriteLineError("下载失败, 程序退出");
                LOGGER.PrintLine("下载失败, 程序退出", LOGGER.Error);
                Environment.Exit(-1);
                //Console.Write("按任意键继续..."); Console.ReadKey(); return;
            }
            catch (Exception ex)
            {
                LOGGER.PrintLine(ex.Message, LOGGER.Error);
            }
        }

        private static void DisplayHelp(ParserResult<MyOptions> result, IEnumerable<Error> errs)
        {
            var helpText = HelpText.AutoBuild(result, h =>
            {
                h.AdditionalNewLineAfterOption = false;
                h.Copyright = "\r\nUSAGE:\r\n\r\n  N_m3u8DL-CLI <URL|JSON|FILE> [OPTIONS]\r\n\r\nOPTIONS:";
                return HelpText.DefaultParsingErrorsHandler(result, h);
            }, e => e);
            //Console.WriteLine(helpText);
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
