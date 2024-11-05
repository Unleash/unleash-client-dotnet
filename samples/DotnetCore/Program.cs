using Unleash;
using Unleash.ClientFactory;
using Unleash.Strategies;

var settings = new UnleashSettings()
{
    AppName = "dotnet-test",
    UnleashApi = new Uri("http://localhost:4242/api"), //setup for running against a local unleash instance, feel free to change this
    CustomHttpHeaders = new Dictionary<string, string>()
    {
      {"Authorization","add a valid client token here" }
    },
    SendMetricsInterval = TimeSpan.FromSeconds(1)
};

const string TOGGLE_NAME = "test";

Console.WriteLine("Starting Unleash SDK");

var unleashFactory = new UnleashClientFactory();
IUnleash unleash = await unleashFactory.CreateClientAsync(settings, synchronousInitialization: true, new MyCustomStrategy());


while (true)
{
    var enabled = unleash.IsEnabled(TOGGLE_NAME);
    var variant = unleash.GetVariant(TOGGLE_NAME);

    Console.WriteLine($"Toggle enabled: {enabled}, variant: {System.Text.Json.JsonSerializer.Serialize(variant)}");
    await Task.Delay(1000);
}

// If you want to test this, you'll need to setup a custom strategy in your
// Unleash UI and add it to the 'test' toggle.
class MyCustomStrategy : IStrategy
{
    public string Name => "my-custom-strategy";

    public bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context)
    {
        return true;
    }
}
