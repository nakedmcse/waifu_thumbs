namespace Thumbnails
{
    public class Dto
    {
        public class AlbumFiles
        {
            public string albumToken { get; set; }
            public List<int> fileIds { get; set; }

            public AlbumFiles()
            {
                albumToken = "";
                fileIds = new List<int>();
            }
        }

        public class FileWithMediaType
        {
            public int fileId { get; set; }
            public string filename { get; set; }
            public string mediaType { get; set; }
        }
    }
}