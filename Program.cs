using Npgsql;
using NetVips;
using Thumbnails;

string baseLocation = "../WaifuVault/";

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var dao = new Dao(baseLocation);
var processor = new Processor();

app.MapPost("/thumbs", async (Dto.AlbumFiles album) =>
{
    Utils.Log($"Generating Thumbnails for {album.albumToken}");
    using (var context = new NpgsqlConnection(dao.connection))
    {
        await context.OpenAsync();
        var albumFileIds = dao.GetAlbumFileIds(context, album.albumToken);
        var selectedFileIds = album.fileIds.Any() ? albumFileIds.Where(id => album.fileIds.Contains(id)).ToList() : albumFileIds;
        var alreadyCachedFileIds = dao.GetCachedFileIds(context, selectedFileIds);
        var finalFileIds = selectedFileIds.Where(id => !alreadyCachedFileIds.Contains(id)).ToList();
        Utils.Log($"Creating {finalFileIds.Count} thumbnails");
        if (finalFileIds.Any())
        {
            var files = dao.GetFilePaths(context, finalFileIds);
            var thumbnails = new List<Tuple<int,string>>();
            var tasks = files.Select(f => Task.Run(() =>
            {
                var thumb = processor.CreateThumbnail(f);
                return thumb;
            })).ToList();
            var results = await Task.WhenAll(tasks);
            thumbnails.AddRange(results.Where(t => t != null));
            dao.SaveThumnbails(context, thumbnails);
        }
    }
    Utils.Log("Finished generating thumbnails");

    return true;
});

app.Run();
