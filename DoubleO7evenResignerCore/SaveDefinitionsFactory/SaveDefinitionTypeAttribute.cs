namespace DoubleO7evenResignerCore.SaveDefinitionsFactory;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class SaveDefinitionTypeAttribute(SaveDefinitionEnum saveDefinitionType) : Attribute
{
    public SaveDefinitionEnum SaveDefinitionType { get; } = saveDefinitionType;
}