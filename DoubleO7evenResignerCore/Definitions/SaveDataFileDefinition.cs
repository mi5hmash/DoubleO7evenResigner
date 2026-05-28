using DoubleO7evenResignerCore.Infrastructure;

namespace DoubleO7evenResignerCore.Definitions;

public abstract class SaveDataFileDefinition
{
    public abstract string FileName { get; }
    public virtual string FileExtension => SaveDataFileIo.FileExtension;
    public abstract bool SupportsCompression { get; }
    public string FullFileName => $"{FileName}{FileExtension}";
}