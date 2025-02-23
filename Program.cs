using Npgsql;
using NetVips;
using Thumbnails;

string baseLocation = "../WaifuVault/";

List<Tuple<int,string>> CreateThumnbails(NpgsqlConnection context, List<int> fileIds, Dao dao) 
{
    var thumbs = new List<Tuple<int,string>>();
    var files = dao.GetFilePaths(context, fileIds);
    
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

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var dao = new Dao(baseLocation);

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
            var thumbnails = CreateThumnbails(context, finalFileIds, dao);
            dao.SaveThumnbails(context, thumbnails);
        }
    }

    return true;
});

app.Run();
