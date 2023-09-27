using Microsoft.Identity.Client;

Console.WriteLine("MSAL Demo");

const string clientId = "deb3f7f3-fe6f-4f70-a2f6-e5016f2adb7d";
const string tenantId = "14f08b43-3b8c-4f1c-87c5-71e2bb2177f3";

var app = PublicClientApplicationBuilder
    .Create(clientId)
    .WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
    .WithDefaultRedirectUri()
    .Build();

var scopes = new[] { "user.read" };

var result = await app.AcquireTokenInteractive(scopes).ExecuteAsync();

var token = result.AccessToken;
var account = result.Account;

Console.WriteLine($"Token: {token.Substring(0, token.Length-10)}...");
Console.WriteLine($"Account: {account.Username}");  

Console.WriteLine("Done!");