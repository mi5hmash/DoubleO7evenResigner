namespace DoubleO7evenResignerCore.SaveDefinitionsFactory;

public interface ISaveDefinition
{
    string FileName { get; }
    string FileExtension { get; }
    string FullFileName { get; }
    bool SupportsCompression { get; }
}