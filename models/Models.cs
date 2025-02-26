namespace Thumbnails
{
    public class Model
    {
        public class file_upload_model
        {
            public int id { get; set; }
            public string fileName { get; set; }
            public string fileExtension { get; set; }
            public string mediaType { get; set; }
            public string albumToken { get; set; }
        }

        public class thumbnail_cache_model
        {
            public int id { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
            public string data { get; set; }
            public int fileId { get; set; }
        }
    }
}