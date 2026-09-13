using LabExtended.Core;
using LabExtended.Utilities;

using NiveraAPI.IO.Configs;

using YoutubeDLSharp;

namespace LabExtended.Audio;

/// <summary>
/// Represents the audio settings and configuration for the application, including paths to required executables and initialization logic.
/// </summary>
public static class AudioSettings
{
    /// <summary>
    /// Indicates whether the audio settings have been initialized and are ready for use.
    /// </summary>
    public static bool IsReady { get; private set; }

    /// <summary>
    /// Gets the singleton instance of the YoutubeDL class used for audio processing.
    /// </summary>
    public static YoutubeDL Instance { get; } = new();

    /// <summary>
    /// Path to ffmpeg executable. If not set, it will default to the current directory with the appropriate executable name based on the operating system.
    /// </summary>
    [Config("audio", "ffmpeg-path", "Path to ffmpeg executable.")]
    public static string FfmpegPath { get; set; } = $"{Directory.GetCurrentDirectory()}/{(Environment.OSVersion.Platform == PlatformID.Win32NT ? "ffmpeg.exe" : "ffmpeg")}";

    /// <summary>
    /// Path to yt-dlp executable. If not set, it will default to the current directory with the appropriate executable name based on the operating system.
    /// </summary>
    [Config("audio", "yt-dlp-path", "Path to yt-dlp executable.")]
    public static string YtDlpPath { get; set; } = $"{Directory.GetCurrentDirectory()}/{(Environment.OSVersion.Platform == PlatformID.Win32NT ? "yt-dlp.exe" : "yt-dlp")}";

    /// <summary>
    /// Path to deno executable. If not set, it will default to the current directory with the appropriate executable name based on the operating system.
    /// </summary>
    [Config("audio", "deno-path", "Path to deno executable.")]
    public static string DenoPath { get; set; } = $"{Directory.GetCurrentDirectory()}/{(Environment.OSVersion.Platform == PlatformID.Win32NT ? "deno.exe" : "deno")}";

    /// <summary>
    /// Path to the output directory for yt-dlp downloads. If not set, it will default to a "yt-dlp-output" folder in the current directory.
    /// </summary>
    [Config("audio", "yt-dlp-output-path", "Path to the output directory for yt-dlp downloads.")]
    public static string YtDlpOutputPath { get; set; } = $"{Directory.GetCurrentDirectory()}/yt-dlp-output";

    internal static void Initialize()
    {
        Instance.FFmpegPath = FfmpegPath;
        Instance.YoutubeDLPath = YtDlpPath;
        Instance.OutputFolder = YtDlpOutputPath;

        Instance.OverwriteFiles = true;
        Instance.RestrictFilenames = true;

        void StartUpdate()
        {
            Task.Run(async () =>
            {
                ApiLog.Info("AudioSettings", "Checking for YT-DLP updates...");

                var result = await Instance.RunUpdate();

                if (!string.IsNullOrEmpty(result))
                    ApiLog.Info("AudioSettings", $"YT-DLP update:\n{result}");
            }).ContinueWithOnMain(task =>
            {
                if (task.IsFaulted)
                {
                    ApiLog.Error("AudioSettings", $"Failed to update YT-DLP:\n{task.Exception?.GetBaseException()?.ToString() ?? "no exception"}");
                }
                else
                {
                    IsReady = true;

                    ApiLog.Info("AudioSettings", "Audio settings initialized successfully.");
                }
            });
        }

        if (!Directory.Exists(YtDlpOutputPath))
            Directory.CreateDirectory(YtDlpOutputPath);

        ApiLog.Info("AudioSettings", "Checking for required dependencies (FFmpeg, YT-DLP, Deno)...");

        if (File.Exists(FfmpegPath)
            && File.Exists(YtDlpPath)
            && File.Exists(DenoPath))
        {
            StartUpdate();
        }
        else
        {
            var instance = Instance;
            var ffmpegPath = FfmpegPath;
            var ytDlpPath = YtDlpPath;
            var denoPath = DenoPath;

            Task.Run(async () =>
            {
                if (!File.Exists(ffmpegPath))
                {
                    var ffmpegDir = Path.GetDirectoryName(ffmpegPath);

                    ApiLog.Info("AudioSettings", "FFmpeg not found. Downloading FFmpeg and FFprobe...");

                    await YoutubeDLSharp.Utils.DownloadFFmpeg(ffmpegDir);
                    await YoutubeDLSharp.Utils.DownloadFFprobe(ffmpegDir);

                    ApiLog.Info("AudioSettings", "FFmpeg and FFprobe downloaded successfully.");
                }

                if (!File.Exists(ytDlpPath))
                {
                    ApiLog.Info("AudioSettings", "YT-DLP not found. Downloading YT-DLP...");
                    await YoutubeDLSharp.Utils.DownloadYtDlp(Path.GetDirectoryName(ytDlpPath));
                    ApiLog.Info("AudioSettings", "YT-DLP downloaded successfully.");
                }

                if (!File.Exists(denoPath))
                {
                    ApiLog.Info("AudioSettings", "Deno not found. Downloading Deno...");
                    await YoutubeDLSharp.Utils.DownloadDeno(Path.GetDirectoryName(denoPath));
                    ApiLog.Info("AudioSettings", "Deno downloaded successfully.");
                }
            }).ContinueWithOnMain(task =>
            {
                if (task.IsFaulted)
                {
                    ApiLog.Error("AudioSettings", $"Failed to download dependencies:\n{task.Exception?.GetBaseException()?.ToString() ?? "no exception"}");
                }
                else
                {
                    StartUpdate();
                }
            });
        }
    }
}
