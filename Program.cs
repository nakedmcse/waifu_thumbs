using Npgsql;
using NetVips;

string connection;
string baseLocation = "../WaifuVault/";

void GetPgConfig()
{
    try
    {
        var configLines = File.ReadAllLines(baseLocation + "postgres.env");
        var username = configLines.First(l => l.StartsWith("POSTGRES_USER")).Replace("POSTGRES_USER=", "");
        var password = configLines.First(l => l.StartsWith("POSTGRES_PASSWORD")).Replace("POSTGRES_PASSWORD=", "");
        var db = configLines.First(l => l.StartsWith("POSTGRES_DB")).Replace("POSTGRES_DB=", "");
        var port = configLines.First(l => l.StartsWith("POSTGRES_PORT")).Replace("POSTGRES_PORT=", "");
        connection = $"Server=localhost;Port={port};Database={db};Username={username};Password={password}";
    }
    catch (Exception ex)
    {
        throw new Exception("Error in getPGConfig: " + ex.Message);
    }
}

List<int> GetAlbumFileIds(NpgsqlConnection context, string token)
{
    List<int> albumIds = new List<int>();
    string query = "SELECT id FROM file_upload_model WHERE \"albumToken\" = @token";
    using (var command = new NpgsqlCommand(query, context))
    {
        command.Parameters.AddWithValue("@token", token);
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                albumIds.Add(reader.GetInt32(0));
            }
        }
    }
    return albumIds;
}

List<int> GetCachedFileIds(NpgsqlConnection context, List<int> fileIds)
{
    List<int> CachedFileIds = new List<int>();
    string query = "SELECT \"fileId\" FROM thumbnail_cache_model WHERE \"fileId\" = ANY(@files)";
    using (var command = new NpgsqlCommand(query, context))
    {
        command.Parameters.AddWithValue("@files", fileIds);
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                CachedFileIds.Add(reader.GetInt32(0));
            }
        }
    }
    return CachedFileIds;
}

List<Tuple<int, string>> GetFilePaths(NpgsqlConnection context, List<int> fileIds)
{
    List<Tuple<int, string>> filePaths = new List<Tuple<int, string>>();
    string query = "SELECT \"id\",\"fileName\",\"fileExtension\" FROM file_upload_model WHERE \"id\" = ANY(@files)";
    using (var command = new NpgsqlCommand(query, context))
    {
        command.Parameters.AddWithValue("@files", fileIds);
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var fileId = reader.GetInt32(0);
                var fileName = baseLocation + "files/" + reader.GetString(1)+"."+reader.GetString(2);
                filePaths.Add(Tuple.Create(fileId,fileName));
            }
        }
    }
    return filePaths;
}

List<Tuple<int,string>> CreateThumnbails(NpgsqlConnection context, List<int> fileIds) 
{
    var thumbs = new List<Tuple<int,string>>();
    var files = GetFilePaths(context, fileIds);
    
    files.ForEach(x => 
    {
        using (var im = Image.NewFromFile(x.Item2))
        {
            var scale = 400.0 / im.Width;
            var thumb = Convert.ToBase64String(im.Resize(scale).WebpsaveBuffer(50));
            thumbs.Add(Tuple.Create(x.Item1, thumb));
        }
    });
    
    return thumbs;
}

void SaveThumnbails(NpgsqlConnection context, List<Tuple<int, string>> thumbnails)
{
    thumbnails.ForEach(x =>
    {
        string query = "INSERT INTO thumbnail_cache_model (\"fileId\", \"data\") VALUES (@fileId, @data);";
        using (var command = new NpgsqlCommand(query, context))
        {
            command.Parameters.AddWithValue("@fileId", x.Item1);
            command.Parameters.AddWithValue("@data", x.Item2);
            command.ExecuteNonQuery();
        }
    });
}

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
GetPgConfig();

app.MapPost("/thumbs", async (AlbumFiles album) =>
{
    using (var context = new NpgsqlConnection(connection))
    {
        await context.OpenAsync();
        var albumFileIds = GetAlbumFileIds(context, album.albumToken);
        var selectedFileIds = album.fileIds.Any() ? albumFileIds.Where(id => album.fileIds.Contains(id)).ToList() : albumFileIds;
        var alreadyCachedFileIds = GetCachedFileIds(context, selectedFileIds);
        var finalFileIds = selectedFileIds.Where(id => !alreadyCachedFileIds.Contains(id)).ToList();
        if (finalFileIds.Any())
        {
            var thumbnails = CreateThumnbails(context, finalFileIds);
            SaveThumnbails(context, thumbnails);
        }
    }

    return true;
});

app.Run();

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
