namespace DoubleO7evenResignerCore.Definitions;

public class DataSaveDefinition : SaveDataFileDefinition
{
    public override string FileName => "data";
    public override bool SupportsCompression => true;
}
