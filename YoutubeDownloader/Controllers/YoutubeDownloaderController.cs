using Microsoft.AspNetCore.Mvc;
using YoutubeDownloader.Models;
using YoutubeDownloader.Services;

namespace YoutubeDownloader.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class YoutubeDownloaderController : ControllerBase
    {
        private readonly YoutubeDataService _downloaderService;
        private readonly ILogger<YoutubeDownloaderController> _logger;
        public YoutubeDownloaderController(ILogger<YoutubeDownloaderController> logger,
            YoutubeDataService downloaderService)
        {
            _logger = logger;
            _downloaderService = downloaderService;
        }

        [HttpPost]
        public async Task<IActionResult> Post(Download download)
        {
            download.Id = Guid.NewGuid().ToString();
            var newDirectory = _downloaderService.TryCreateDownloadDirectory(download);
            await _downloaderService.DownloadMetaData(download);
            var videoLocalFullPath = _downloaderService.DownloadFromYoutubeToLocalFileSystem(download, newDirectory);
            return Ok(videoLocalFullPath);
        }
    }
}