namespace DoubleO7evenResignerCore.Definitions;

public static class SaveDefinitionRegistry
{
    private static readonly Dictionary<string, SaveDataFileDefinition> Definitions;

    static SaveDefinitionRegistry()
    {
        Definitions = new Dictionary<string, SaveDataFileDefinition>(StringComparer.OrdinalIgnoreCase);

        var baseType = typeof(SaveDataFileDefinition);

        var types = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => baseType.IsAssignableFrom(t)
                        && !t.IsAbstract
                        && t != typeof(DefaultSaveDefinition));

        foreach (var type in types)
        {
            if (Activator.CreateInstance(type) is SaveDataFileDefinition def)
            {
                Definitions[def.FullFileName] = def;
            }
        }
    }

    /// <summary>
    /// Gets the SaveData file definition for the specified file name. If no specific definition is found, returns a default definition based on the file name.
    /// </summary>
    /// <param name="fileName">The name of the file for which to get the SaveData file definition.</param>
    /// <returns>A <see cref="SaveDataFileDefinition"/> instance representing the definition for the specified file.</returns>
    public static SaveDataFileDefinition Get(string fileName)
    {
        if (Definitions.TryGetValue(fileName, out var def))
            return def;

        return new DefaultSaveDefinition(
            Path.GetFileNameWithoutExtension(fileName)
        );
    }
}