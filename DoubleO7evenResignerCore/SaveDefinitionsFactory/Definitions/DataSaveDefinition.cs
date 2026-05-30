namespace DoubleO7evenResignerCore.SaveDefinitionsFactory.Definitions;

[SaveDefinitionType(SaveDefinitionEnum.Data)]
public sealed class DataSaveDefinition : DefaultSaveDefinition
{
    public override string FileName => "data";
    public override bool SupportsCompression => true;
}
