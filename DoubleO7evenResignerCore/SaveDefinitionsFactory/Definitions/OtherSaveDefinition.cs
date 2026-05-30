namespace DoubleO7evenResignerCore.SaveDefinitionsFactory.Definitions;

public sealed class OtherSaveDefinition(string fileName, string fileExtension) : DefaultSaveDefinition
{
    public override string FileName => fileName;
    public override string FileExtension => fileExtension;
    public override bool SupportsCompression => false;
}
