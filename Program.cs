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
    using (var context = new NpgsqlConnection(dao.connection))
    {
        await context.OpenAsync();
        var albumFileIds = dao.GetAlbumFileIds(context, album.albumToken);
        var selectedFileIds = album.fileIds.Any() ? albumFileIds.Where(id => album.fileIds.Contains(id)).ToList() : albumFileIds;
        var alreadyCachedFileIds = dao.GetCachedFileIds(context, selectedFileIds);
        var finalFileIds = selectedFileIds.Where(id => !alreadyCachedFileIds.Contains(id)).ToList();
        if (finalFileIds.Any())
        {
            var files = dao.GetFilePaths(context, finalFileIds);
            var thumbnails = new List<Tuple<int,string>>();
            files.ForEach(x =>
            {
                var thumb = processor.CreateThumbnail(x);
                if (thumb != null)
                {
                    thumbnails.Add(thumb);
                }
            });
            dao.SaveThumnbails(context, thumbnails);
        }
    }

    return true;
});

app.Run();
