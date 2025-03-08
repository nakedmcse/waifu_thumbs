using System.Drawing;
using System.IO.Pipelines;
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using NetVips;

namespace Thumbnails
{
    public class Processor
    {
        public List<string> GetSupportedVideoFormats()
        {
            return FFMpeg.GetContainerFormats().Select(x => x.Name).ToList();
        }

        public List<string> GetSupportedImageFormats()
        {
            return ["gif", "jpg", "jpeg", "png", "bmp", "pdf", "tiff", "svg", "magick", "webp", "heif", "jp2k", "jxl"];
        }
        
        public Tuple<int, string>? CreateThumbnail(Dto.FileWithMediaType file)
        {
            switch (file.mediaType.Split("/")[0])
            {
                case "image":
                    return ProcessImage(file);
                case "video":
                    return ProcessVideo(file);
            }
            return null;
        }

        private Tuple<int,string> ProcessImage(Dto.FileWithMediaType file)
        {
            string options = file.mediaType.EndsWith("gif") || file.mediaType.EndsWith("webp") ? "[n=-1]" : "";
            using (var im = Image.Thumbnail(file.fileOnDisk + options,400))
            {
                var thumb = Convert.ToBase64String(im.WebpsaveBuffer(50));
                return Tuple.Create(file.id, thumb);
            }
        }

        private Tuple<int,string> ProcessVideo(Dto.FileWithMediaType file)
        {
            var rnd = new Random();
            var probe = FFProbe.Analyse(file.fileOnDisk);
            var randomDur = rnd.Next((int)probe.Duration.TotalSeconds);
            var pipe = new MemoryStream();
            FFMpegArguments.FromFileInput(file.fileOnDisk)
                .OutputToPipe(new StreamPipeSink(pipe), options => options
                    .Seek(TimeSpan.FromSeconds(randomDur))
                    .WithFrameOutputCount(1)
                    .WithVideoFilters(filter => filter.Scale(-1, 200))
                    .ForceFormat("webp")
                    .WithFastStart())
                .ProcessSynchronously();
            return Tuple.Create(file.id, Convert.ToBase64String(pipe.ToArray()));
        }
    }
};
