using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace AutoCuttingMusicAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SongSplitController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostEnvironment;

        public SongSplitController(IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }
        [HttpPost]
        public async Task<IActionResult> SplitSong(IFormFile songFile)
        {
            TimeSpan startTime = TimeSpan.FromSeconds(0);
            TimeSpan endTime = TimeSpan.FromSeconds(30);
            if (songFile == null || songFile.Length == 0)
            {
                return BadRequest("Vui lòng tải lên một bài hát.");
            }
            try
            {
                var outputDirectory = Path.Combine(Directory.GetCurrentDirectory(), "output");
                Directory.CreateDirectory("output");
                string outputFilePath = Path.Combine(outputDirectory, "trimmed_music.mp3");

                using (var reader = new Mp3FileReader(songFile.OpenReadStream()))
                {
                    var waveFormat = reader.WaveFormat;
                    var startBytes = (int)(startTime.TotalSeconds * waveFormat.AverageBytesPerSecond);
                    var endBytes = (int)(endTime.TotalSeconds * waveFormat.AverageBytesPerSecond);

                    var bytesToRead = endBytes - startBytes;

                    var buffer = new byte[bytesToRead];
                    reader.Position = startBytes;
                    reader.Read(buffer, 0, buffer.Length);
                    using (var writer = new WaveFileWriter(outputFilePath, waveFormat))
                    {
                        writer.Write(buffer, 0, buffer.Length);
                    }

                }
                return StatusCode(201);
            }
            catch (Exception ex)
            {

                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi: {ex.Message}");
            }
            
        }
    }
}
