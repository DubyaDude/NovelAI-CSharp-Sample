using NovelAI;
using NovelAI.OpenApi;
using System.Diagnostics;
using System.IO.Compression;

string email = "notarealemail@gmail.com";
string password = "notarealpassword";

// Setting up the client
NovelClient novelAI = new NovelClient(new HttpClient());

// Logging in
var authResp = await novelAI.UserLoginAsync(new LoginRequest() { Key = NovelUtils.GenerateLoginString(email, password) });
Console.WriteLine("Got AccessToken: " + authResp.AccessToken);

// Getting current subscription
var userResp = await novelAI.UserSubscriptionAsync();
Console.WriteLine("Got Account Tier: " + userResp.Tier);

// Setting up image we want to generate
var imageParameters = new AiGenerateImageParameters()
{
    // Put parameters here (width, heignt, etc)
    Width = AiGenerateImageDimension._640,
    Height = AiGenerateImageDimension._640,
};
var userInput = "masterpiece, best quality, fox girl, red, galaxy, space, planets";
var model = AiGenerateImageModals.NaiDiffusion;

// Getting price of image
var imagePriceResp = await novelAI.AiGenerateImageRequestPriceAsync(new AiGenerateImagePriceRequest()
{
    Tier = (AiGenerateImagePriceRequestTier)userResp.Tier - 1,
    Request = new Request()
    {
        Input = new string[] { userInput },
        Model = model,
        Parameters = imageParameters,
    }
});

// Rendering only if free
if (imagePriceResp.RequestEligibleForUnlimitedGeneration || imagePriceResp.CostPerPrompt == 0)
{
    Console.WriteLine("Image price free!");

    var imageRequest = new AiGenerateImageRequest()
    {
        Input = userInput,
        Model = model,
        Parameters = imageParameters,
    };



    Stopwatch stopwatch = new();
    stopwatch.Restart();
    Console.WriteLine($"Generating Image...");

    string filePrefix = $"NovelAI";
    List<(string Name, byte[] Data)> images = new();
    byte[] zipBytes;
        {
        using (MemoryStream zipMemoryStream = new MemoryStream())
        {
            using (FileResponse generationResp = await novelAI.AiGenerateImageAsync(imageRequest))
            {
                await generationResp.Stream.CopyToAsync(zipMemoryStream);
            }

            using (ZipArchive zipArchive = new ZipArchive(zipMemoryStream, ZipArchiveMode.Update))
            {
                int count = 0;

                foreach (var entry in zipArchive.Entries.ToArray())
                {
                    using (Stream entryStream = entry.Open())
                    {
                        string entryName = $"{filePrefix}_{++count}.png";

                        using (MemoryStream ms = new MemoryStream())
                        {
                            await entryStream.CopyToAsync(ms);
                            images.Add((entryName, ms.ToArray()));
                        }

                        entryStream.Position = 0;
                        using (Stream newEntryStream = zipArchive.CreateEntry(entryName, CompressionLevel.NoCompression).Open())
    {
                            await entryStream.CopyToAsync(newEntryStream);
                        }
                    }
                    entry.Delete();
                }
            }
            zipBytes = zipMemoryStream.ToArray();
        }
            }

    stopwatch.Stop();
    Console.WriteLine($"Finished Request in {stopwatch.Elapsed.TotalSeconds} seconds");
    stopwatch.Restart();

    Console.WriteLine("Saving");

    if (File.Exists($"{filePrefix}.zip"))
    {
        File.Delete($"{filePrefix}.zip");
        }
    File.WriteAllBytes($"{filePrefix}.zip", zipBytes);

    foreach (var image in images)
    {
        if (File.Exists(image.Name))
        {
            File.Delete(image.Name);
        }
        File.WriteAllBytes(image.Name, image.Data);
    }
        
    stopwatch.Stop();
    Console.WriteLine($"Saved in {stopwatch.Elapsed.TotalSeconds} seconds");

    Console.ReadLine();
}