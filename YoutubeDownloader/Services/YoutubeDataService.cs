using System.Diagnostics;
using System.Text;
using YoutubeDownloader.Models;

namespace YoutubeDownloader.Services
{
    public class YoutubeDataService
    {
        private string _sharedPath;
        public YoutubeDataService()
        {
            _sharedPath = @"\shared\";
        }

        public string TryCreateDownloadDirectory(Download download)
        {
            string newDirectory = Path.Combine(_sharedPath, download.Id);

            if (Directory.Exists(newDirectory) == false)
                Directory.CreateDirectory(newDirectory);

            return newDirectory;
        }

        private static TimeSpan ParseDuration(string durationRaw)
        {
            if (string.IsNullOrWhiteSpace(durationRaw)) return TimeSpan.Zero;
            int foundSplitters = durationRaw.ToArray().Count(it => it == ':');
            for (int i = foundSplitters; i < 2; i++)
            {
                durationRaw = "00:" + durationRaw;
            }
            return TimeSpan.Parse(durationRaw);
        }
        private async Task<string> RunAsync(string action, string mediaUrl)
        {
            var process = Process.Start(new ProcessStartInfo("yt-dlp", $"{action} {mediaUrl}")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            process.WaitForExit();

            string standardError = await process.StandardError.ReadToEndAsync();
            string standardOutput = await process.StandardOutput.ReadToEndAsync();

            if (process.ExitCode != 0)
                throw new ApplicationException("Download Failure", new Exception(standardError + standardOutput));

            return standardOutput.Replace(System.Environment.NewLine, string.Empty);
        }
        public async Task DownloadMetaData(Download download)
        {
            string title = await this.RunAsync("--get-title", download.OriginalMediaUrl);
            string thumbnail = await this.RunAsync("--get-thumbnail", download.OriginalMediaUrl);
            string description = await this.RunAsync("--get-description", download.OriginalMediaUrl);
            string durationRaw = await this.RunAsync("--get-duration", download.OriginalMediaUrl);
            TimeSpan duration = ParseDuration(durationRaw);

            //await this.dataService.Update(download.Id, (update) =>
            //     update.Combine(new[] {
            //        update.Set(it => it.Title, title),
            //        update.Set(it => it.ThumbnailUrl, thumbnail),
            //        update.Set(it => it.Description, description),
            //        update.Set(it => it.Duration, duration)
            //     })
            // );
        }

        private (string output, string error, int exitCode) Run(ProcessStartInfo processStartInfo)
        {
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardOutput = true;

            var process = Process.Start(processStartInfo);
            process.WaitForExit();

            string standardError = process.StandardError.ReadToEndAsync().GetAwaiter().GetResult();
            string standardOutput = process.StandardOutput.ReadToEndAsync().GetAwaiter().GetResult();

            if (process.ExitCode != 0) throw new ApplicationException("Download Failure", new Exception(standardError + Environment.NewLine + standardOutput));
            return (standardOutput, standardError, process.ExitCode);
        }
        public string DownloadFromYoutubeToLocalFileSystem(Download download, string newDirectory)
        {
            var mediaFilePath = Path.Combine(newDirectory, $"{download.Id}.mp4");

            var args = new StringBuilder();
            args.Append($"--output {mediaFilePath} ");
            args.Append($"--format mp4 ");
            args.Append($" {download.OriginalMediaUrl} ");

            var processStartInfo = new ProcessStartInfo("yt-dlp", args.ToString());

            (string output, string error, int exitCode) = this.Run(processStartInfo);

            return Directory.GetFiles(newDirectory).Single();
        }
    }
}
