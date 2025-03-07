using Microsoft.AspNetCore.Mvc;
using Npgsql;
using NetVips;
using Thumbnails;

string baseLocation = "../WaifuVault/";

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var dao = new Dao(baseLocation);
var processor = new Processor();

app.MapGet("/generateThumbnails/supported", () =>
{
    Utils.Log("Supported image and video formats");
    var retval = processor.GetSupportedImageFormats();
    retval.AddRange(processor.GetSupportedVideoFormats());
    Utils.Log(String.Join(",",retval));
    return retval;
});

app.MapPost("/generateThumbnails", async ([FromQuery] int albumId, [FromQuery] bool addingAdditionalFiles, [FromBody]Dto.FileWithMediaType[] files) =>
{
    Utils.Log($"Generating Thumbnails");
    Utils.Log($"Creating {files.Count()} thumbnails");
    if (files.Any())
    {
        var thumbnails = new List<Tuple<int,string>>();
        var semaphore = new SemaphoreSlim(4);
        var tasks = files.Select(async f =>
        {
            await semaphore.WaitAsync();
            try
            {
                var thumb = await Task.Run(() => processor.CreateThumbnail(f));
                return thumb;
            }
            finally
            {
                semaphore.Release();
            }
        }).ToList();
        var results = await Task.WhenAll(tasks);
        thumbnails.AddRange(results.Where(t => t != null));
        await dao.SaveThumnbails(thumbnails);
    }
    Utils.Log("Finished generating thumbnails");

    return true;
});

app.Run();
