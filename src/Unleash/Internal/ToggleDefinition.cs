using Yggdrasil;

public class ToggleDefinition
{
    public ToggleDefinition(string name, string project, string type)
    {
        Name = name;
        Project = project;
        Type = type;
    }
    public string Name { get; private set; }

    public string Project { get; private set; }

    public string Type { get; private set; }

    internal static ToggleDefinition FromYggdrasilDef(FeatureDefinition definition)
    {
        return new ToggleDefinition(definition.Name, definition.Project, definition.Type);
    }
}