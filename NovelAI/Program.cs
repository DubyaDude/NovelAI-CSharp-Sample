using NovelAI;
using NovelAI.OpenApi;

string email = "notarealemail@gmail.com";
string password = "notarealpassword";

NovelClient novelAI = new NovelClient("https://api.novelai.net/", new HttpClient());
var authResp = await novelAI.UserLoginAsync(new LoginRequest() { Key = NovelUtils.GenerateLoginString(email, password) });

Console.WriteLine("Got AccessToken: " + authResp.AccessToken);
Console.WriteLine();

// Getting current subscription
var userResp = await novelAI.UserSubscriptionAsync();
Console.WriteLine("Got Account Tier: " + userResp.Tier);

// Setting up image we want to generate
var imageParameters = new AiGenerateImageParameters();
var userInput = "anime girl";

// Getting price of image
var imagePriceResp = await novelAI.AiGenerateImageRequestPriceAsync(new AiGenerateImagePriceRequest()
{
    Tier = (AiGenerateImagePriceRequestTier)userResp.Tier - 1,
    Request = new Request()
    {
        Input = new string[] { userInput },
        Model = AiGenerateImageModals.NaiDiffusion,
        Parameters = imageParameters,
    }
});

// Rendering only if free
if (imagePriceResp.RequestEligibleForUnlimitedGeneration || imagePriceResp.CostPerPrompt == 0)
{
    Console.WriteLine("Image price free!");
    var imageResp = await novelAI.AiGenerateImageAsync(new AiGenerateImageRequest()
    {
        Input = userInput,
        Model = AiGenerateImageModals.NaiDiffusion,
        Parameters = imageParameters,
    });

    // Format (raw string):
    //
    // event: newImage
    // id: 1
    // data: ACTUAL-IMAGE-DATA
    string? imageRespStr = imageResp.ToString();
    if (imageRespStr != null)
    {
        Console.WriteLine("Got Image Resp: \n" + imageRespStr);
        
        var image = Convert.FromBase64String(imageRespStr.Split("data:")[1]);
        File.WriteAllBytes("image.png", image);
    }
}