using CommandLine;
using CommandLine.Text;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m3u8DL
{
    internal class MyOptions
    {
        [Value(0, Hidden = true, MetaName = "Input Source", HelpText = "Help_input")]
        public string Input { get; set; }

        [Option("workDir", HelpText = "Help_workDir")]
        public string WorkDir { get; set; }

        [Option("saveName", HelpText = "Help_saveName")]
        public string SaveName { get; set; } = "";

        [Option("baseUrl", HelpText = "Help_baseUrl")]
        public string BaseUrl { get; set; }

        [Option("headers", HelpText = "Help_headers")]
        public string Headers { get; set; } = "";

        [Option("maxThreads", Default = 32U, HelpText = "Help_maxThreads")]
        public uint MaxThreads { get; set; }

        [Option("minThreads", Default = 16U, HelpText = "Help_minThreads")]
        public uint MinThreads { get; set; }

        [Option("retryCount", Default = 15U, HelpText = "Help_retryCount")]
        public uint RetryCount { get; set; }

        [Option("timeOut", Default = 10U, HelpText = "Help_timeOut")]
        public uint TimeOut { get; set; }

        [Option("muxSetJson", HelpText = "Help_muxSetJson")]
        public string MuxSetJson { get; set; }

        [Option("useKeyFile", HelpText = "Help_useKeyFile")]
        public string UseKeyFile { get; set; }

        [Option("useKeyBase64", HelpText = "Help_useKeyBase64")]
        public string UseKeyBase64 { get; set; }

        [Option("useKeyIV", HelpText = "Help_useKeyIV")]
        public string UseKeyIV { get; set; }

        [Option("downloadRange", HelpText = "Help_downloadRange")]
        public string DownloadRange { get; set; }

        [Option("liveRecDur", HelpText = "Help_liveRecDur")]
        public string LiveRecDur { get; set; }

        [Option("stopSpeed", HelpText = "Help_stopSpeed")]
        public long StopSpeed { get; set; } = 0L;

        [Option("maxSpeed", HelpText = "Help_maxSpeed")]
        public long MaxSpeed { get; set; } = 0L;

        [Option("proxyAddress", HelpText = "Help_proxyAddress")]
        public string ProxyAddress { get; set; }

        [Option("enableDelAfterDone", HelpText = "Help_enableDelAfterDone")]
        public bool EnableDelAfterDone { get; set; }

        [Option("enableMuxFastStart", HelpText = "Help_enableMuxFastStart")]
        public bool EnableMuxFastStart { get; set; }

        [Option("enableBinaryMerge", HelpText = "Help_enableBinaryMerge")]
        public bool EnableBinaryMerge { get; set; }

        [Option("enableParseOnly", HelpText = "Help_enableParseOnly")]
        public bool EnableParseOnly { get; set; }

        [Option("enableAudioOnly", HelpText = "Help_enableAudioOnly")]
        public bool EnableAudioOnly { get; set; }

        [Option("disableDateInfo", HelpText = "Help_disableDateInfo")]
        public bool DisableDateInfo { get; set; }

        [Option("disableIntegrityCheck", HelpText = "Help_disableIntegrityCheck")]
        public bool DisableIntegrityCheck { get; set; }

        [Option("noMerge", HelpText = "Help_noMerge")]
        public bool NoMerge { get; set; }

        [Option("noProxy", HelpText = "Help_noProxy")]
        public bool NoProxy { get; set; }

        [Option("registerUrlProtocol", HelpText = "Help_registerUrlProtocol")]
        public bool RegisterUrlProtocol { get; set; }

        [Option("unregisterUrlProtocol", HelpText = "Help_unregisterUrlProtocol")]
        public bool UnregisterUrlProtocol { get; set; }

        [Option("enableChaCha20", HelpText = "enableChaCha20")]
        public bool EnableChaCha20 { get; set; }

        [Option("chaCha20KeyBase64", HelpText = "ChaCha20KeyBase64")]
        public string ChaCha20KeyBase64 { get; set; }

        [Option("chaCha20NonceBase64", HelpText = "ChaCha20NonceBase64")]
        public string ChaCha20NonceBase64 { get; set; }

    }
}
