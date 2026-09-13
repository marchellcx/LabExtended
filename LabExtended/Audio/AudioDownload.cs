using LabExtended.Core;
using LabExtended.Utilities;

using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace LabExtended.Audio;

public static class AudioDownload
{
    public static void DownloadYoutubeAudio(string youtubeUrl, Action<bool, string> callback, Action<DownloadProgress>? progressCallback)
    {
        if (string.IsNullOrEmpty(youtubeUrl))
            throw new ArgumentException("YouTube URL cannot be null or empty.", nameof(youtubeUrl));

        if (callback == null)
            throw new ArgumentNullException(nameof(callback), "Callback cannot be null.");

        if (!AudioSettings.IsReady)
            throw new InvalidOperationException("AudioSettings is not initialized. Please call AudioSettings.Initialize() before using this method.");

        var progress = new Progress<DownloadProgress>();

        if (progressCallback != null)
            progress.ProgressChanged += (sender, args) => progressCallback(args);

        progress.ProgressChanged += (_, args) =>
        {
            if (args.State is DownloadState.Downloading)
            {
                ApiLog.Info("AudioDownload", $"Downloading &1{youtubeUrl}&r: {Math.Ceiling(args.Progress * 100f)}% ({args.ETA})");
            }
            else if (args.State is DownloadState.PreProcessing or DownloadState.PostProcessing)
            {
                ApiLog.Info("AudioDownload", $"Processing &1{youtubeUrl}&r: {Math.Ceiling(args.Progress * 100f)}% ({args.ETA})");
            }
            else if (args.State is DownloadState.Error)
            {
                ApiLog.Error("AudioDownload", $"Error downloading &1{youtubeUrl}&r!");
            }
            else if (args.State is DownloadState.Success)
            {
                ApiLog.Info("AudioDownload", $"Successfully downloaded &1{youtubeUrl}&r!");
            }
        };

        ApiLog.Info("AudioDownload", $"Starting download for &1{youtubeUrl}&r...");

        Task.Run(async () =>
        {
            var result = await AudioSettings.Instance.RunAudioDownload(youtubeUrl, AudioConversionFormat.Mp3, default, progress);

            result.EnsureSuccess();
            return result.Data;
        }).ContinueWithOnMain(task =>
        {
            if (task.IsFaulted)
            {
                var error = task.Exception?.GetBaseException().Message ?? "Unknown error";

                ApiLog.Error("AudioDownload", $"Error downloading &1{youtubeUrl}&r:\n{error}");

                callback(false, error);
            }
            else
            {
                if (!File.Exists(task.Result))
                {
                    ApiLog.Error("AudioDownload", $"Error downloading &1{youtubeUrl}&r: File not found.");

                    callback(false, "File not found.");
                }
                else
                {
                    ApiLog.Info("AudioDownload", $"Successfully downloaded &1{youtubeUrl}&r! Converting ..");

                    
                }
            }
        });
    }
}
