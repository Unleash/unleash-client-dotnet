# Unleash FeatureToggle Client for .Net

[![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-v1.4%20adopted-ff69b4.svg)](code-of-conduct.md)
[![NuGet](https://img.shields.io/nuget/v/Unleash.Client.svg)](https://www.nuget.org/packages/Unleash.Client/)


## Introduction

Unleash Client SDK for .Net. It is compatible with the
[Unleash-hosted.com SaaS offering](https://www.unleash-hosted.com/) and
[Unleash Open-Source](https://github.com/finn-no/unleash).

The main motivation for doing feature toggling is to decouple the process for deploying code to production and releasing new features. This helps reducing risk, and allow us to easily manage which features to enable.

Feature toggles decouple deployment of code from release of new features.

Take a look at the demonstration site at [unleash.herokuapp.com](http://unleash.herokuapp.com/)

Read more of the main project at [github.com/unleash/unleash](https://github.com/Unleash/unleash)

![dashboard](https://raw.githubusercontent.com/stiano/unleash-client-dotnet/master/resources/dashboard.png "Unleash Server Dashboard")

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
Install the latest version of `Unleash.Client` from [nuget.org](https://www.nuget.org/packages/Unleash.Client/).

### Create a new a Unleash instance

It is easy to get a new instance of Unleash. In your app you typically *just want one instance of Unleash*, and inject that where you need it. You will typically use a dependency injection frameworks to manage this. 

To create a new instance of Unleash you need to pass in a settings object:
```csharp

var settings = new UnleashSettings()
{
    AppName = "dotnet-test",
    InstanceTag = "instance z",
    UnleashApi = new Uri("http://unleash.herokuapp.com/api/"),
    CustomHttpHeaders = new Dictionary()
    {
      {"Authorization","API token" }
    }
};

var unleash = new DefaultUnleash(settings);
```
Note that the `DefaultUnleash` constructor sets up the toggle caching and periodic background fetching. If you want the cache to be populated immediately, see the [synchronous startup](#synchronous-startup) section

When your application shuts down, remember to dispose the unleash instance.

```csharp
unleash?.Dispose()
```

### Configuring projects in unleash client

If you're organizing your feature toggles in `Projects` in Unleash Enterprise, you can specify the `ProjectId` on the `UnleashSettings` to select which project to fetch feature toggles for.

```csharp

var settings = new UnleashSettings()
{
    AppName = "dotnet-test",
    InstanceTag = "instance z",
    UnleashApi = new Uri("http://unleash.herokuapp.com/api/"),
    ProjectId = "projectId"
};

```

### Feature toggle api

It is really simple to use unleash.

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

Calling `unleash.IsEnabled("SuperAwesomeFeature")` is the equvivalent of calling `unleash.IsEnabled("SuperAwesomeFeature", false)`. 
Which means that it will return `false` if it cannot find the named toggle. 

If you want it to default to `true` instead, you can pass `true` as the second argument:

```csharp
unleash.IsEnabled("SuperAwesomeFeature", true)
```

### Activation strategies

The .Net client comes with implementations for the built-in activation strategies provided by unleash. 

- DefaultStrategy
- UserWithIdStrategy
- GradualRolloutRandomStrategy
- GradualRolloutUserWithIdStrategy
- GradualRolloutSessionIdStrategy
- RemoteAddressStrategy
- ApplicationHostnameStrategy
- FlexibleRolloutStrategy

Read more about the strategies in [activation-strategy.md](https://github.com/Unleash/unleash/blob/master/docs/activation-strategies.md).

#### Custom strategies
You may also specify and implement your own strategies. The specification must be registered in the Unleash UI and you must register the strategy implementation when you wire up unleash. 

```csharp
IStrategy s1 = new MyAwesomeStrategy();
IStrategy s2 = new MySuperAwesomeStrategy();

IUnleash unleash = new DefaultUnleash(config, s1, s2);
```

### Unleash context

In order to use some of the common activation strategies you must provide an [unleash-context](https://github.com/Unleash/unleash/blob/master/docs/unleash-context.md).

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

#### UnleashContextProvider
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
    InstanceTag = "instance z",
    UnleashApi = new Uri("http://unleash.herokuapp.com/api/"),
    UnleashContextProvider = new AspNetContextProvider(),
    CustomHttpHeaders = new Dictionary()
    {
      {"Authorization", "API token" }
    }
};
``` 

### Custom HTTP headers
If you want the client to send custom HTTP Headers with all requests to the Unleash api you can define that by setting them via the `UnleashSettings`. 

```csharp
var settings = new UnleashSettings()
{
    AppName = "dotnet-test",
    InstanceTag = "instance z",
    UnleashApi = new Uri("http://unleash.herokuapp.com/api/"),
    UnleashContextProvider = new AspNetContextProvider(),
    CustomHttpHeaders = new Dictionary<string, string>()
    {
        {"Authorization", "API token" }
    }
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

## Local backup
By default unleash-client fetches the feature toggles from unleash-server every 20s, and stores the result in temporary .json file which is located in `System.IO.Path.GetTempPath()` directory. This means that if the unleash-server becomes unavailable, the unleash-client will still be able to toggle the features based on the values stored in .json file. As a result of this, the second argument of `IsEnabled` will be returned in two cases:

* When .json file does not exists
* When the named feature toggle does not exist in .json file

## Bootstrapping
* Unleash supports bootstrapping from a JSON string.
* Configure your own custom provider implementing the `IToggleBootstrapProvider` interface's single method `string Read()`.
  This should return a JSON string in the same format returned from `/api/client/features`
* Example bootstrap files can be found in the json files located in [tests/Unleash.Tests/App_Data](tests/Unleash.Tests/App_Data)
* Our assumption is this can be use for applications deployed to ephemeral containers or more locked down file systems where Unleash's need to write the backup file is not desirable or possible.
* Loading with bootstrapping only occurs if there were no feature toggles loaded from Local Backup

Configuring it with the UnleashSettings:
```csharp
var settings = new UnleashSettings()
{
    AppName = "dotnet-test",
    InstanceTag = "instance z",
    UnleashApi = new Uri("http://unleash.herokuapp.com/api/"),
    CustomHttpHeaders = new Dictionary()
    {
      {"Authorization","API token" }
    },
    ToggleBootstrapProvider = new MyToggleBootstrapProvider()
};
```

### Provided Bootstrappers
* These are found in the `Unleash.Utilities` namespace
* To configure them instantiate with constructor parameters and set on the `UnleashSettings.ToggleBootstrapProvider` property

#### ToggleBootstrapFileProvider
* Unleash comes with a `ToggleBootstrapFileProvider` which implements the `IToggleBootstrapProvider` interface.
* Constructor takes the `path` parameter used to find the JSON source file

#### ToggleBootstrapUrlProvider
* Unleash also comes with a `ToggleBootstrapUrlProvider` which implements the `IToggleBootstrapProvider` interface.
* Fetches JSON from a webaddress using `HttpMethod.Get`
* The constructor takes the `path` parameter as the webaddress for the JSON
* The constructor takes an optional `client` `HttpClient` parameter, use when reusing clients or configuring custom headers
* The constructpr also takes an optional `throwOnFail` `bool` parameter that defaults to `false`. Set it to `true` if you need HTTP 404:s and 500:s etc to throw

## Json Serialization
The unleash client is dependant on a json serialization library. If your application already have Newtonsoft.Json >= 9.0.1 installed, everything should work out of the box. If not, you will get an error message during startup telling you to implement an 'IJsonSerializer' interface, which needs to be added to the configuration.

With Newtonsoft.Json version 7.0.0.0, the following implementation can be used. For older versions, consider to upgrade.

```csharp
var settings = new UnleashSettings()
{
    AppName = "dotnet-test",
    InstanceTag = "instance z",
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

## Synchronous Startup
This unleash client does not throw any exceptions if the unleash server is unreachable. Also, fetching features will return the default value if the feature toggle cache has not yet been populated. In many situations it is perferable to throw an error than allow an application to startup with incorrect feature toggle values. In this case, we provice a client factory with the option for synchronous initialization. 

```csharp
var settings = new UnleashSettings()
{
    AppName = "dotnet-test",
    InstanceTag = "instance z",
    UnleashApi = new Uri("https://app.unleash-hosted.com/demo/api/"),
    CustomHttpHeaders = new Dictionary<string, string>(),
    {
       {"Authorization","56907a2fa53c1d16101d509a10b78e36190b0f918d9f122d" }
    }
};
var unleashFactory = new UnleashClientFactory();

IUnleash unleash = await unleashFactory.CreateClientAsync(settings, synchronousInitialization: true);

// this `unleash` has successfully fetched feature toggles and written them to its cache.
// if network errors or disk permissions prevented this from happening, the above await would have thrown an exception

var awesome = unleash.IsEnabled("SuperAwesomeFeature");
```

The `CreateClientAsync` method was introduced in version 1.5.0, making the previous `Generate` method obsolete. There's also a `CreateClient` method available if you don't prefer the async version.

## Run unleash server with Docker locally
The Unleash team have made a separate project which runs unleash server inside docker. Please see [unleash-docker](https://github.com/Unleash/unleash-docker) for more details.

## Development

Visual Studio 2017 / Code

Cakebuild
