# Unleash FeatureToggle Client for .Net

[![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-v1.4%20adopted-ff69b4.svg)](code-of-conduct.md)
[![NuGet](https://img.shields.io/nuget/v/Unleash.Client.svg)](https://www.nuget.org/packages/Unleash.Client/)


## Introduction

Unleash Client SDK for .Net. It is compatible with the
[Unleash-hosted.com SaaS offering](https://www.unleash-hosted.com/) and
[Unleash Open-Source](https://github.com/unleash/unleash).

The main motivation for doing feature toggling is to decouple the process for deploying code to production and releasing new features. This helps reducing risk, and allow us to easily manage which features to enable.

Feature toggles decouple deployment of code from release of new features.

Take a look at the demonstration site at [unleash.herokuapp.com](http://unleash.herokuapp.com/)

Read more of the main project at [github.com/unleash/unleash](https://github.com/Unleash/unleash)

![dashboard](https://raw.githubusercontent.com/unleash/unleash-client-dotnet/master/resources/dashboard.png "Unleash Server Dashboard")

## Features
Supported Frameworks
* NET Standard 2.0
* .Net 4.7
* .Net 4.6.1
* .Net 4.6
* .Net 4.5.1
* .Net 4.5

No direct dependencies

Extendable architecture
- Inject your own implementations of key components (Json serializer, background task scheduler, http client factory)

## Getting started

### Install the package

Install the latest version of `Unleash.Client` from [nuget.org](https://www.nuget.org/packages/Unleash.Client/) or use the `dotnet` cli:

``` bash
dotnet add package unleash.client
```

### Create a new a Unleash instance

---

**⚠️ Important:** In almost every case, you only want a **single, shared instance of the Unleash  client (a *singleton*)** in your application . You would typically use a dependency injection framework to inject it where you need it. Having multiple instances of the client in your application could lead to inconsistencies and performance degradation.

If you create more than 10 instances, Unleash will attempt to log warnings about your usage.

---

To create a new instance of Unleash you need to create and pass in an `UnleashSettings` object.

When creating an instance of the Unleash client, you can choose to do it either **synchronously** or **asynchronously**.
The SDK will synchronize with the Unleash API on initialization, so it can take a moment for the it to reach the correct state. With an asynchronous startup, this would happen in the background while the rest of your code keeps executing. In most cases, this isn't an issue. But if you want to **wait until the SDK is fully synchronized**, then you should use the configuration explained in the [synchronous startup](#synchronous-startup) section.
This is usually not an issue and Unleash will do this in the background as soon as you initialize it.
However, if it's important that you do not continue execution until the SDK has synchronized, then you should use the configuration explained in the [synchronous startup](#synchronous-startup) section.

```csharp

var settings = new UnleashSettings()
{
    AppName = "dotnet-test",
    UnleashApi = new Uri("<your-api-url>"),
    CustomHttpHeaders = new Dictionary()
    {
      {"Authorization","<your-api-token>" }
    }
};

var unleash = new DefaultUnleash(settings);

// Add to Container as Singleton
// .NET Core 3/.NET 5/...
services.AddSingleton<IUnleash>(c => unleash);

```

When your application shuts down, remember to dispose the unleash instance.

```csharp
unleash?.Dispose()
```

#### Synchronous startup

This unleash client does not throw any exceptions if the unleash server is unreachable. Also, fetching features will return the default value if the feature toggle cache has not yet been populated. In many situations it is perferable to throw an error than allow an application to startup with incorrect feature toggle values. For these cases, we provide a client factory with the option for synchronous initialization.

```csharp
var settings = new UnleashSettings()
{
    AppName = "dotnet-test",
    UnleashApi = new Uri("<your-api-url>"),
    CustomHttpHeaders = new Dictionary<string, string>()
    {
       {"Authorization","<your-api-token>" }
    }
};
var unleashFactory = new UnleashClientFactory();

IUnleash unleash = await unleashFactory.CreateClientAsync(settings, synchronousInitialization: true);

// this `unleash` has successfully fetched feature toggles and written them to its cache.
// if network errors or disk permissions prevented this from happening, the above await would have thrown an exception

var awesome = unleash.IsEnabled("SuperAwesomeFeature");
```

The `CreateClientAsync` method was introduced in version 1.5.0, making the previous `Generate` method obsolete. There's also a `CreateClient` method available if you don't prefer the async version.


#### Configuring projects in unleash client

If you're organizing your feature toggles in `Projects` in Unleash Enterprise, you can specify the `ProjectId` on the `UnleashSettings` to select which project to fetch feature toggles for.

```csharp

var settings = new UnleashSettings()
{
    AppName = "dotnet-test",
    UnleashApi = new Uri("http://unleash.herokuapp.com/api/"),
    ProjectId = "projectId"
};

```

### Check feature toggles

The `IsEnabled` method allows you to check whether a feature is enabled:

```csharp
if(unleash.IsEnabled("SuperAwesomeFeature"))
{
  //do some magic
}
else
{
  //do old boring stuff
}
```

If the Unleash client can't find the feature you're trying to check, it will default to returning `false`. You can change this behavior on a per-invocation basis by providing a fallback value as a second argument.

For instance, `unleash.IsEnabled("SuperAwesomeFeature")` would return `false` if `SuperAwesomeFeature` doesn't exist. But if you'd rather it returned `true`, then you could pass that as the second argument:

```csharp
unleash.IsEnabled("SuperAwesomeFeature", true)
```

#### Providing context

You can also **provide an [Unleash context](https://docs.getunleash.io/reference/unleash-context)** to the `IsEnabled` method:

```csharp
var context = new UnleashContext
{
  UserId = "61"
};

unleash.IsEnabled("someToggle", context);
```

Refer to the [Unleash context](#unleash-context) section for more information about using the Unleash context in the .NET SDK.

## Handling events

Currently supported events:
-  [Impression data events](https://docs.getunleash.io/advanced/impression-data#impression-event-data)
-  Error events
-  Toggles updated event

```csharp

var settings = new UnleashSettings()
{
    // ...
};

var unleash = new DefaultUnleash(settings);

// Set up handling of impression and error events
unleash.ConfigureEvents(cfg =>
{
    cfg.ImpressionEvent = evt => { Console.WriteLine($"{evt.FeatureName}: {evt.Enabled}"); };
    cfg.ErrorEvent = evt => { /* Handling code here */ Console.WriteLine($"{evt.ErrorType} occured."); };
    cfg.TogglesUpdatedEvent = evt => { /* Handling code here */ Console.WriteLine($"Toggles updated on: {evt.UpdatedOn}"); };
});

```


## Activation strategies

The .Net client comes with implementations for the built-in activation strategies provided by unleash.

- DefaultStrategy
- UserWithIdStrategy
- GradualRolloutRandomStrategy
- GradualRolloutUserWithIdStrategy
- GradualRolloutSessionIdStrategy
- RemoteAddressStrategy
- ApplicationHostnameStrategy
- FlexibleRolloutStrategy

Read more about the strategies in [the activation strategy reference docs](https://docs.getunleash.io/reference/activation-strategies).

### Custom strategies

You can also specify and implement your own [custom strategies](https://docs.getunleash.io/reference/custom-activation-strategies). The specification must be registered in the Unleash UI and you must register the strategy implementation when you wire up unleash.

```csharp
IStrategy s1 = new MyAwesomeStrategy();
IStrategy s2 = new MySuperAwesomeStrategy();

IUnleash unleash = new DefaultUnleash(config, s1, s2);
```

## Unleash context

In order to use some of the common activation strategies you must provide an [Unleash context](https://docs.getunleash.io/reference/unleash-context).

If you have configured custom stickiness and want to use that with the FlexibleRolloutStrategy or Variants, add the custom stickiness parameters to the Properties dictionary on the Unleash Context:

```csharp
HttpContext.Current.Items["UnleashContext"] = new UnleashContext
{
    UserId = HttpContext.Current.User?.Identity?.Name,
    SessionId = HttpContext.Current.Session?.SessionID,
    RemoteAddress = HttpContext.Current.Request.UserHostAddress,
    Properties = new Dictionary<string, string>
    {
        // Obtain "customField" and add it to the context properties
        { "customField", HttpContext.Current.Items["customField"].ToString() }
    }
};
```

### UnleashContextProvider

The provider typically binds the context to the same thread as the request. If you are using Asp.Net the `UnleashContextProvider` will typically be a 'request scoped' instance.


```csharp
public class AspNetContextProvider : IUnleashContextProvider
{
    public UnleashContext Context
    {
       get { return HttpContext.Current?.Items["UnleashContext"] as UnleashContext; }
    }
}

protected void Application_BeginRequest(object sender, EventArgs e)
{
    HttpContext.Current.Items["UnleashContext"] = new UnleashContext
    {
        UserId = HttpContext.Current.User?.Identity?.Name,
        SessionId = HttpContext.Current.Session?.SessionID,
        RemoteAddress = HttpContext.Current.Request.UserHostAddress,
        Properties = new Dictionary<string, string>()
        {
            {"UserRoles", "A,B,C"}
        }
    };
}

var settings = new UnleashSettings()
{
    AppName = "dotnet-test",
    UnleashApi = new Uri("http://unleash.herokuapp.com/api/"),
    UnleashContextProvider = new AspNetContextProvider(),
    CustomHttpHeaders = new Dictionary()
    {
      {"Authorization", "API token" }
    }
};
```

## Custom HTTP headers

If you want the client to send custom HTTP Headers with all requests to the Unleash api you can define that by setting them via the `UnleashSettings`.

```csharp
var settings = new UnleashSettings()
{
    AppName = "dotnet-test",
    UnleashApi = new Uri("http://unleash.herokuapp.com/api/"),
    UnleashContextProvider = new AspNetContextProvider(),
    CustomHttpHeaders = new Dictionary<string, string>()
    {
        {"Authorization", "API token" }
    }
};
```

### HttpMessageHandlers/Custom HttpClient initialization
If you need to specify HttpMessageHandlers or to control the instantiation of the HttpClient, you can create a custom
HttpClientFactory that inherits from DefaultHttpClientFactory, and override the method CreateHttpClientInstance.
Then configure UnleashSettings to use your custom HttpClientFactory. 

```csharp
internal class CustomHttpClientFactory : DefaultHttpClientFactory
{
    protected override HttpClient CreateHttpClientInstance(Uri unleashApiUri)
    {
        var messageHandler = new CustomHttpMessageHandler();
        var httpClient = new HttpClient(messageHandler)
        {
            BaseAddress = apiUri,
            Timeout = TimeSpan.FromSeconds(5)
        };
    }
}

var settings = new UnleashSettings
{
    AppName = "dotnet-test",
    //...
    HttpClientFactory = new CustomHttpClientFactory()
};
```

### Dynamic custom HTTP headers
If you need custom http headers that change during the lifetime of the client, a provider can be defined via the `UnleashSettings`. 

```vb
Public Class CustomHttpHeaderProvider
    Implements IUnleashCustomHttpHeaderProvider

    Public Function GetCustomHeaders() As Dictionary(Of String, String) Implements IUnleashCustomHttpHeaderProvider.GetCustomHeaders
        Dim token = ' Acquire or refresh a token
        Return New Dictionary(Of String, String) From
                {{"Authorization", "Bearer " & token}}
    End Function
End Class

' ...

Dim unleashSettings As New UnleashSettings()
unleashSettings.AppName = "dotnet-test"
unleashSettings.InstanceTag = "instance z"
' add the custom http header provider to the settings
unleashSettings.UnleashCustomHttpHeaderProvider = New CustomHttpHeaderProvider()
unleashSettings.UnleashApi = new Uri("http://unleash.herokuapp.com/api/")
unleashSettings.UnleashContextProvider = New AspNetContextProvider()
Dim unleash = New DefaultUnleash(unleashSettings)
                
```

## Logging

By default Unleash-client uses LibLog to integrate with the currently configured logger for your application.
The supported loggers are:
- Serilog
- NLog
- Log4Net
- EntLib
- Loupe

### Custom logger integration
To plug in your own logger you can implement the `ILogProvider` interface, and register it with Unleash:

```csharp
Unleash.Logging.LogProvider.SetCurrentLogProvider(new CustomLogProvider());
var settings = new UnleashSettings()
//...
```

 The `GetLogger` method is responsible for returning a delegate to be used for logging, and your logging integration should be placed inside that delegate:

```csharp
using System;
using Unleash.Logging;

namespace Unleash.Demo.CustomLogging
{
    public class CustomLogProvider : ILogProvider
    {
        public Logger GetLogger(string name)
        {
            return (logLevel, messageFunc, exception, formatParameters) =>
            {
                // Plug in your logging code here

                return true;
            };
        }

        public IDisposable OpenMappedContext(string key, object value, bool destructure = false)
        {
            return new EmptyIDisposable();
        }

        public IDisposable OpenNestedContext(string message)
        {
            return new EmptyIDisposable();
        }
    }

    public class EmptyIDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
```



## Local backup
By default unleash-client fetches the feature toggles from unleash-server every 20s, and stores the result in temporary .json file which is located in `System.IO.Path.GetTempPath()` directory. This means that if the unleash-server becomes unavailable, the unleash-client will still be able to toggle the features based on the values stored in .json file. As a result of this, the second argument of `IsEnabled` will be returned in two cases:

* When .json file does not exists
* When the named feature toggle does not exist in .json file

## Bootstrapping
* Unleash supports bootstrapping from a JSON string.
* Configure your own custom provider implementing the `IToggleBootstrapProvider` interface's single method `ToggleCollection Read()`.
  This should return a `ToggleCollection`. The `UnleashSettings.JsonSerializer` can be used to deserialize a JSON string in the same format returned from `/api/client/features`.
* Example bootstrap files can be found in the json files located in [tests/Unleash.Tests/App_Data](tests/Unleash.Tests/App_Data)
* Our assumption is this can be use for applications deployed to ephemeral containers or more locked down file systems where Unleash's need to write the backup file is not desirable or possible.
* Loading with bootstrapping defaults to override feature toggles loaded from Local Backup, this override can be switched off by setting the `UnleashSettings.ToggleOverride` property to `false`

Configuring with the UnleashSettings:
```csharp
var settings = new UnleashSettings()
{
    AppName = "dotnet-test",
    UnleashApi = new Uri("http://unleash.herokuapp.com/api/"),
    CustomHttpHeaders = new Dictionary()
    {
      {"Authorization","API token" }
    },
    ToggleOverride = false, // Defaults to true
    ToggleBootstrapProvider = new MyToggleBootstrapProvider() // A toggle bootstrap provider implementing IToggleBootstrapProvider here
};
```

### Provided Bootstrappers
* Two ToggleBootstrapProviders are provided
* These are found in the `Unleash.Utilities`:

#### ToggleBootstrapFileProvider
* Unleash comes with a `ToggleBootstrapFileProvider` which implements the `IToggleBootstrapProvider` interface.
* Configure with `UnleashSettings` helper method:

```csharp
settings.UseBootstrapFileProvider("./path/to/file.json");
```

#### ToggleBootstrapUrlProvider
* Unleash also comes with a `ToggleBootstrapUrlProvider` which implements the `IToggleBootstrapProvider` interface.
* Fetches JSON from a webaddress using `HttpMethod.Get`

* Configure with `UnleashSettings` helper method:

```csharp
var shouldThrowOnError = true; // Throws for 500, 404, etc
var customHeaders = new Dictionary<string, string>()
{
    { "Authorization", "Bearer ABCdefg123" } // Or whichever set of headers would be required to GET this file
}; // Defaults to null
settings.UseBootstrapUrlProvider("://domain.top/path/to/file.json", shouldThrowOnError, customHeaders);
```

## Json Serialization
The unleash client is dependant on a json serialization library. If your application already have Newtonsoft.Json >= 9.0.1 installed, everything should work out of the box. If not, you will get an error message during startup telling you to implement an 'IJsonSerializer' interface, which needs to be added to the configuration.

With Newtonsoft.Json version 7.0.0.0, the following implementation can be used. For older versions, consider to upgrade.

```csharp
var settings = new UnleashSettings()
{
    AppName = "dotnet-test",
    UnleashApi = new Uri("http://unleash.herokuapp.com/api/"),
    JsonSerializer = new NewtonsoftJson7Serializer()
};

public class NewtonsoftJson7Serializer : IJsonSerializer
{
    private readonly Encoding utf8 = Encoding.UTF8;

    private static readonly JsonSerializer Serializer = new JsonSerializer()
    {
        ContractResolver = new CamelCaseExceptDictionaryKeysResolver()
    };

    public T Deserialize<T>(Stream stream)
    {
        using (var streamReader = new StreamReader(stream, utf8))
        using (var textReader = new JsonTextReader(streamReader))
        {
            return Serializer.Deserialize<T>(textReader);
        }
    }

    public void Serialize<T>(Stream stream, T instance)
    {
        using (var writer = new StreamWriter(stream, utf8, 1024 * 4, leaveOpen: true))
        using (var jsonWriter = new JsonTextWriter(writer))
        {
            Serializer.Serialize(jsonWriter, instance);
            
            jsonWriter.Flush();
			stream.Position = 0;
        }
    }

    class CamelCaseExceptDictionaryKeysResolver : CamelCasePropertyNamesContractResolver
    {
        protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
        {
            var contract = base.CreateDictionaryContract(objectType);

            contract.DictionaryKeyResolver = propertyName =>
            {
                return propertyName;
            };

            return contract;
        }
    }
}
```

The server api needs camel cased json, but not for certain dictionary keys. The implementation can be naively validated by the `JsonSerializerTester.Assert` function. (Work in progress).

## Run unleash server with Docker locally
The Unleash team have made a separate project which runs unleash server inside docker. Please see [unleash-docker](https://github.com/Unleash/unleash-docker) for more details.

## Development

Visual Studio 2017 / Code

Cakebuild

### Other information

- Check out our guide for more information on how to build and scale [feature flag](https://docs.getunleash.io/topics/feature-flags/feature-flag-best-practices) systems