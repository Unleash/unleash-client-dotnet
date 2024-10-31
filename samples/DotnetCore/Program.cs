using Unleash;
using Unleash.ClientFactory;
using Unleash.Internal;

var settings = new UnleashSettings()
{
    AppName = "dotnet-test",
    UnleashApi = new Uri("http://localhost:3063/api"),
    CustomHttpHeaders = new Dictionary<string, string>()
    {
      {"Authorization","*:development.86efeeed04ed49e6389fe848925289475b89996404c320154437d8e0" }
    },
    SendMetricsInterval = TimeSpan.FromSeconds(1)
};

const string TOGGLE_NAME = "test";

Console.WriteLine("Starting Unleash SDK");

var unleashFactory = new UnleashClientFactory();
IUnleash unleash = await unleashFactory.CreateClientAsync(settings, synchronousInitialization: true);


while (true)
{
    var enabled = unleash.IsEnabled(TOGGLE_NAME);
    var variant = unleash.GetVariant(TOGGLE_NAME);

    Console.WriteLine($"Toggle enabled: {enabled}, variant: {System.Text.Json.JsonSerializer.Serialize(variant)}");
    await Task.Delay(1000);
}