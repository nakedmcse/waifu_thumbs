namespace Thumbnails
{
    public class Dto
    {
        public class FileWithMediaType
        {
            public int id { get; set; }
            public string fileOnDisk { get; set; }
            public string mediaType { get; set; }
            public string extension { get; set; }
        }
    }
}