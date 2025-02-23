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
    }
}