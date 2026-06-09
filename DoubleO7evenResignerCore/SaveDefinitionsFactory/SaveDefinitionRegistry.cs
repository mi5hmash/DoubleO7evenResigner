using DoubleO7evenResignerCore.SaveDefinitionsFactory.Definitions;
using System.Reflection;

namespace DoubleO7evenResignerCore.SaveDefinitionsFactory;

public static class SaveDefinitionRegistry
{
    private static readonly Dictionary<string, ISaveDefinition> Definitions;

    static SaveDefinitionRegistry()
    {
        Definitions = new Dictionary<string, ISaveDefinition>(StringComparer.OrdinalIgnoreCase);

        var ns = typeof(DefaultSaveDefinition).Namespace!;
        var baseType = typeof(ISaveDefinition);

        var elements = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                t.Namespace != null &&
                t.Namespace.StartsWith(ns) &&
                !t.IsAbstract &&
                baseType.IsAssignableFrom(t) &&
                t.GetCustomAttribute<SaveDefinitionTypeAttribute>(false) != null);

        foreach (var type in elements)
        {
            var instance = (ISaveDefinition)Activator.CreateInstance(type)!;
            if (!Definitions.TryAdd(instance.FullFileName, instance))
                throw new InvalidOperationException($"Duplicate SaveDefinition for file '{instance.FullFileName}' in {type.FullName}");
        }
    }

    /// <summary>
    /// Gets the SaveData file definition for the specified file name. If no specific definition is found, returns a default definition based on the file name.
    /// </summary>
    /// <param name="fileName">The name of the file for which to get the SaveData file definition.</param>
    /// <returns>A <see cref="ISaveDefinition"/> instance representing the definition for the specified file.</returns>
    public static ISaveDefinition Get(string fileName)
    {
        if (Definitions.TryGetValue(fileName, out var def))
            return def;

        return new OtherSaveDefinition(
            Path.GetFileNameWithoutExtension(fileName),
            Path.GetExtension(fileName)
        );
    }
}