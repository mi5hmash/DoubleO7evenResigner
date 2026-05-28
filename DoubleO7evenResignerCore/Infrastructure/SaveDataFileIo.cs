namespace DoubleO7evenResignerCore.Infrastructure;

public static class SaveDataFileIo
{
    public const string FileExtension = ".save";

    public static string[] GetFiles(string inputDir)
        => Directory.GetFiles(inputDir, $"*{FileExtension}", SearchOption.AllDirectories);
}