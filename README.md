# Unleash FeatureToggle Client for .Net

[![Build status](https://ci.appveyor.com/api/projects/status/x8xuyffpjc60keqg?svg=true)](https://ci.appveyor.com/project/StianOlafsen/unleash-client-dotnet)

## Introduction

Unleash is a feature toggle system, that gives you a great overview over all feature toggles across all your applications and services. It comes with official client implementations for Java, Node.js and Go. This is for now, `an unofficial client`.

The main motivation for doing feature toggling is to decouple the process for deploying code to production and releasing new features. This helps reducing risk, and allow us to easily manage which features to enable.

Feature toggles decouple deployment of code from release of new features.

Take a look at the demonstration site at [unleash.herokuapp.com](http://unleash.herokuapp.com/)

Read more of the main project at [github.com/unleash/unleash](https://github.com/Unleash/unleash)

## Getting started
Install the latest version of `Unleash.FeatureToggle.Client` from [nuget.org](https://www.nuget.org/packages/Unleash.FeatureToggle.Client/).

### Create a new a Unleash instance

It is easy to get a new instance of Unleash. In your app you typically *just want one instance of Unelash*, and inject that where you need it. You will typically use a dependency injection frameworks to manage this. 

To create a new instance of Unleash you need to pass in a config object:
```csharp

UnleashConfig config = new UnleashConfig()
                .SetAppName("dotnet-test")
                .SetInstanceId("instance z")
                .SetUnleashApi("http://unleash.herokuapp.com/");

IUnleash unleash = new DefaultUnleash(config);
```

When your application shuts down, remember to dispose the unleash instance.

```csharp
unleash?.Dispose()
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
    HttpContext.Current.Items["UnleashContext"] = new UnleashContext.Builder()
            .UserId(HttpContext.Current.User?.Identity?.Name)
            .SessionId(HttpContext.Current.Session?.SessionID)
            .RemoteAddress(HttpContext.Current.Request.UserHostAddress)
            .AddProperty("CustomPropertyForCustomStrategy", "some-interesting-value") // Optional
            .Build()
        ;
}

var contextProvider = new AspNetContextProvider();

UnleashConfig config = new UnleashConfig()
                .SetAppName("dotnet-test")
                .SetInstanceId("instance z")
                .SetUnleashApi("http://unleash.herokuapp.com/")
                .UnleashContextProvider(new AspNetContextProvider());
``` 

### Custom HTTP headers
If you want the client to send custom HTTP Headers with all requests to the Unleash api you can define that by setting them via the `UnleashConfig`. 

```csharp
UnleashConfig config = new UnleashConfig()
                .SetAppName("dotnet-test")
                .SetInstanceId("instance z")
                .SetUnleashApi("http://unleash.herokuapp.com/")
                .UnleashContextProvider(new AspNetContextProvider())
                .AddCustomHttpHeader("Authorization", "some-secret");
                
```

## Local backup
By default unleash-client fetches the feature toggles from unleash-server every 20s, and stores the result in temporary .json file which is located in `System.IO.Path.GetTempPath()` directory. This means that if the unleash-server becomes unavailable, the unleash-client will still be able to toggle the features based on the values stored in .json file. As a result of this, the second argument of `IsEnabled` will be returned in two cases:

* When .json file does not exists
* When the named feature toggle does not exist in .json file

## Json Serialization
The unleash client is dependant on a json serialization library. If your application already have Newtonsoft.Json >= 9.0.1 installed, everything should work out of the box. If not, you will get an error message during startup telling you to implement an 'IJsonSerializer' interface, which needs to be added to the configuration.

Your implementation can be naively validated by the `JsonSerializerTester.Assert` function. (Work in progress).

## Run unleash server with Docker locally
The Unleash team have made a separate project which runs unleash server inside docker. Please see [unleash-docker](https://github.com/Unleash/unleash-docker) for more details.

## Development

Visual Studio 2017
