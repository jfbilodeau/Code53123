using Microsoft.Extensions.Configuration;

Console.WriteLine("Starting control center");

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();  

var clientId = configuration["ClientId"];
var tenantId = configuration["TenantId"];
var functionUri = configuration["FunctionUri"]; 

var controlCenter = new ControlCenter(clientId, tenantId, functionUri);

await controlCenter.Run();        

Console.WriteLine("Control center closed");

