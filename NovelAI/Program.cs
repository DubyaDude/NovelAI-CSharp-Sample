using NovelAI;
using NovelAI.OpenApi;
using System.Text;

Console.WriteLine("Hello, World!");

string email = "notarealemail@gmail.com";
string password = "notarealpassword";

NovelClient novelAI = new NovelClient("https://api.novelai.net/", new HttpClient());
var resp = await novelAI.UserLoginAsync(new LoginRequest() { Key = NovelUtils.GenerateLoginString(email, password) });

Console.WriteLine("Got AccessToken: " + resp.AccessToken);
Console.WriteLine();

var resp2 = await novelAI.UserInformationAsync();
Console.WriteLine("Got AccountCreatedAt: " + resp2.AccountCreatedAt);

