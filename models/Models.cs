namespace Thumbnails
{
    public class Model
    {
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