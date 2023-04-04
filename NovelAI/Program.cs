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
var userInput = "anime girl";
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

    Stopwatch stopwatch = new();
    stopwatch.Start();
    
    var imageResp = await novelAI.AiGenerateImageAsync(new AiGenerateImageRequest()
    {
        Input = userInput,
        Model = model,
        Parameters = imageParameters,
    });

    stopwatch.Stop();

    var zipArchive = new ZipArchive(imageResp.Stream);
    List<(string Name, byte[] Data)> images = new();
    foreach (var entry in zipArchive.Entries)
        {
        using var entryStream = entry.Open();
            byte[] bytes;
            using (var ms = new MemoryStream())
    {
                entryStream.CopyTo(ms);
                bytes = ms.ToArray();
            }

            images.Add((entry.Name, bytes));
        }
    imageResp.Dispose();

    foreach (var image in images)
    {
        Console.WriteLine("Saving " + image.Name);
        File.WriteAllBytes(image.Name, image.Data);
    }
        
    Console.WriteLine($"Got Image Resp in {stopwatch.Elapsed.TotalSeconds} seconds");
}