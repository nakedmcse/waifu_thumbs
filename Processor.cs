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
            using (var im = Image.NewFromFile(file.filename))
            {
                var scale = 400.0 / im.Width;
                var thumb = Convert.ToBase64String(im.Resize(scale).WebpsaveBuffer(50));
                return Tuple.Create(file.fileId, thumb);
            }
        }

        private Tuple<int,string> ProcessVideo(Dto.FileWithMediaType file)
        {
            // TODO: Implement
            return Tuple.Create(0, "");
        }
    }
};
