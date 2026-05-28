namespace DoubleO7evenResignerCore.Definitions;

public class DefaultSaveDefinition(string fileName) : SaveDataFileDefinition
{
    public override string FileName => fileName;
    public override bool SupportsCompression => false;
}
