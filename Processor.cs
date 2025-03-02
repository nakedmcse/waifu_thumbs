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
            using (var im = Image.NewFromFile(file.filename + options,true))
            {
                var thumb = Convert.ToBase64String(im.ThumbnailImage(400).WebpsaveBuffer(50));
                return Tuple.Create(file.fileId, thumb);
            }
        }

        private Tuple<int,string> ProcessVideo(Dto.FileWithMediaType file)
        {
            var rnd = new Random();
            var probe = FFProbe.Analyse(file.filename);
            var randomDur = rnd.Next((int)probe.Duration.TotalSeconds);
            var pipe = new MemoryStream();
            FFMpegArguments.FromFileInput(file.filename)
                .OutputToPipe(new StreamPipeSink(pipe), options => options
                    .Seek(TimeSpan.FromSeconds(randomDur))
                    .WithFrameOutputCount(1)
                    .WithVideoFilters(filter => filter.Scale(-1, 200))
                    .ForceFormat("webp")
                    .WithFastStart())
                .ProcessSynchronously();
            return Tuple.Create(file.fileId, Convert.ToBase64String(pipe.ToArray()));
        }
    }
};
