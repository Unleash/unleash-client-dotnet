# Migrating to Unleash-Client-Dotnet 5.0.0

This guide highlights the key changes you should be aware of when upgrading to v5.0.0 of the Unleash client.

## Custom strategy changes

Custom strategies no longer provide the option to access the constraints in their interface. The method `bool IsEnabled(Dictionary<string, string> parameters, UnleashContext context, IEnumerable<Constraint> constraints)` no longer exists. Other than that, custom strategies remain unchanged.

## Direct access to feature toggles

Direct access to the feature toggle objects through `UnleashClient.FeatureToggles` has been removed. All classes related to the internal representation of feature toggles are no longer publicly accessible in the SDK.

The SDK now provides a `UnleashClient.ListKnownToggles` method, which returns a list of feature toggle names, their type, and the project they're bound to.

The client also no longer provides access to list the variants bound to a feature toggle through `UnleashClient.GetVariants`. We believe this was inappropriately leaking the internal abstractions out of the SDK but if you have a strong use case for this, please open an issue.

## Bootstrapping changes

Due to the changes in the previous section, bootstrapping classes are now required to return a `String` instead of a `FeatureToggleCollection`. The string should be a JSON string representing the response returned from your Unleash instance's `api/client/features` endpoint. In practice, that means if you previously had a `Read` method in your bootstrapping class like so:

``` dotnet

public ToggleCollection Read()
{
    var json = settings.FileSystem.ReadAllText(filePath);
    return settings.JsonSerializer.Deserialize<ToggleCollection>(json);
}

```

You can simplify it to:

``` dotnet

public string Read()
{
    return settings.FileSystem.ReadAllText(filePath);
}

```

## Custom serializers

In v4.x and before, the SDK provided the option to use NewtonSoft or mount a custom JSON serializer. This option has been removed in v5.x, the SDK now relies on System.Text.Json and no option to override that is provided. If you were previously providing a custom serializer to access System.Text.Json, that's now safe to remove. Users who were previously relying on NewtonSoft shouldn't have to make any changes.