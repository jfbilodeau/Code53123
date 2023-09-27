using System.ComponentModel;
using System.Net.Http.Headers;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.AppConfig;

public class ControlCenter
{
    private readonly string _clientId;
    private readonly string _tenantId;
    private readonly string _functionUri;

    public bool _active = false;
    private string _accessToken = "";
    private string _idToken = "";
    private IAccount? _account = null;
    private readonly String[] _scopes = new[] { "user.read" };
    private readonly IList<Command> _commands = new List<Command>();

    public ControlCenter(string clientId, string tenantId, string functionUri)
    {
        _clientId = clientId;
        _tenantId = tenantId;
        _functionUri = functionUri;

        // Initialize commands.
        _commands.Add(new Command("quit", CommandQuit));
        _commands.Add(new Command("help", CommandHelp));
        _commands.Add(new Command("whoami", CommandWhoAmI));
        _commands.Add(new Command("id-token", CommandGetIdToken));
        _commands.Add(new Command("access-token", CommandGetAccessToken));
        _commands.Add(new Command("login", CommandLogInInteractive));
        _commands.Add(new Command("login-device", CommandLogInDeviceCode));
        _commands.Add(new Command("managed-identity", CommandManagedIdentity));
        _commands.Add(new Command("login-silent", CommandLogInSilent));
        _commands.Add(new Command("function", CommandFunction));
        _commands.Add(new Command("get-profile-picture", CommandGetProfilePicture));
    }

    public async Task Run()
    {
        _active = true;

        while (_active)
        {
            Console.WriteLine("> What is thy bidding, my master? ");
            var input = Console.ReadLine();

            if (input == null || input.Trim().Length == 0)
            {
                Console.WriteLine("Speak up! I can't hear you.");
                continue;
            }
            var command = _commands.FirstOrDefault(c => c.Name == input);

            if (command != null)
            {
                try
                {
                    await command.Action();
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("Looks like a proton torpedo entered the exhaust port.");
                    Console.Error.WriteLine($"Error: {e.Message}");
                    Console.Error.WriteLine($"Stack: {e.StackTrace}");
                }
            }
            else
            {
                Console.WriteLine("Your babbling is incomprehensible to me.");
            }
        }
    }

    private void ClearAccount()
    {
        _account = null;
        _accessToken = "";
        _idToken = "";
    }

    private void InitAccount(AuthenticationResult result)
    {
        _account = result.Account;
        _accessToken = result.AccessToken;
        _idToken = result.IdToken;
    }

    private Task CommandQuit()
    {
        _active = false;

        return Task.FromResult(0);
    }

    private Task CommandHelp()
    {
        Console.WriteLine("Available commands:");

        foreach (var command in _commands)
        {
            Console.WriteLine($"  {command.Name}");
        }

        Console.WriteLine("(n.b.: a Sith lord should not need help)");

        return Task.FromResult(0);
    }

    private async Task CommandWhoAmI()
    {
        if (_account == null)
        {
            Console.WriteLine("You are not logged in");
        }
        else
        {
            Console.WriteLine($"You are {_account.Username}");
        }
    }

    private async Task CommandGetIdToken()
    {
        if (_idToken.Length == 0)
        {
            Console.WriteLine("No id token");
        }
        else
        {
            Console.WriteLine($"Your id token is:\n{_idToken[..^20]}...");
        }
    }

    private async Task CommandGetAccessToken()
    {
        if (_accessToken.Length == 0)
        {
            Console.WriteLine("No access token");
        }
        else
        {
            Console.WriteLine($"Your access token is:\n{_accessToken[..^20]}...");
        }
    }

    private async Task CommandLogInInteractive()
    {
        ClearAccount();

        var publicClientApp = PublicClientApplicationBuilder
            .Create(_clientId)
            .WithAuthority(AzureCloudInstance.AzurePublic, _tenantId)
            .WithDefaultRedirectUri()
            .Build();

        var result = await publicClientApp.AcquireTokenInteractive(_scopes).ExecuteAsync();

        InitAccount(result);
    }

        private async Task CommandLogInDeviceCode()
    {
        ClearAccount();

        var publicClientApp = PublicClientApplicationBuilder
            .Create(_clientId)
            .WithAuthority(AzureCloudInstance.AzurePublic, _tenantId)
            .WithDefaultRedirectUri()
            .Build();

        var result = await publicClientApp.AcquireTokenWithDeviceCode(_scopes, deviceCodeResult =>
        {
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("Login via Device Code.");
            Console.WriteLine($"Message: {deviceCodeResult.Message}");
            Console.WriteLine($"User Code: {deviceCodeResult.UserCode}");
            Console.WriteLine($"Verification URL: {deviceCodeResult.VerificationUrl}");
            Console.WriteLine("----------------------------------------");

            return Task.FromResult(0);
        }).ExecuteAsync();

        InitAccount(result);
    }

    private async Task CommandManagedIdentity()
    {
        ClearAccount();

        var app = ManagedIdentityApplicationBuilder.Create(ManagedIdentityId.SystemAssigned)
            .Build();

        AuthenticationResult result = await app.AcquireTokenForManagedIdentity("https://management.azure.com")
            .ExecuteAsync();

        InitAccount(result);
    }


    private async Task CommandLogInSilent()
    {
        ClearAccount();

        var publicClientApp = PublicClientApplicationBuilder
            .Create(_clientId)
            .WithAuthority(AzureCloudInstance.AzurePublic, _tenantId)
            .WithDefaultRedirectUri()
            .Build();

        var result = await publicClientApp.AcquireTokenSilent(_scopes, _account)
            .ExecuteAsync();

        InitAccount(result);
    }

    private async Task CommandFunction()
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        var response = await httpClient.GetAsync(_functionUri);

        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine(content);
        }
        else
        {
            Console.WriteLine($"Error [{response.StatusCode}]: {content}");
        }
    }

    private async Task CommandGetProfilePicture()
    {
        if (_account == null)
        {
            Console.WriteLine("You are not logged in");
        }
        else
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await httpClient.GetAsync("https://graph.microsoft.com/v1.0/me/photo/$value");

            if (response.IsSuccessStatusCode)
            {
                var image = await response.Content.ReadAsByteArrayAsync();

                using var fileStream = File.Create("profile.png");

                fileStream.Write(image);
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
            }
        }
    }

    private class Command
    {
        public string Name { get; set; }
        public Func<Task> Action { get; set; }

        public Command(string name, Func<Task> action) => (this.Name, this.Action) = (name, action);
    }
}