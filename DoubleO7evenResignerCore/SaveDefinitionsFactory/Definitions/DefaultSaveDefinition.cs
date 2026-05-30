using DoubleO7evenResignerCore.Infrastructure;

namespace DoubleO7evenResignerCore.SaveDefinitionsFactory.Definitions;

public abstract class DefaultSaveDefinition : ISaveDefinition
{
    public virtual string FileName => string.Empty;
    public virtual string FileExtension => SaveDataFileIo.FileExtension;
    public virtual string FullFileName => $"{FileName}{FileExtension}";
    public virtual bool SupportsCompression => false;
}