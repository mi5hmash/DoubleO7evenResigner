namespace DoubleO7evenResignerCore.Infrastructure;

public static class SaveDataFileIo
{
    public const string FileExtension = ".save";

    /// <summary>
    /// Recursively gets all files with the specified save data file extension from the input directory and its subdirectories.
    /// </summary>
    /// <param name="inputDir">The directory to search for save data files.</param>
    /// <returns>An array of file paths matching the save data file extension.</returns>
    public static string[] GetFiles(string inputDir)
        => Directory.GetFiles(inputDir, $"*{FileExtension}", SearchOption.AllDirectories);
}