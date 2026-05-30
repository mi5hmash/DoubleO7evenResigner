using DoubleO7evenResignerCore;
using DoubleO7evenResignerCore.Helpers;
using DoubleO7evenResignerCore.SaveDefinitionsFactory;
using DoubleO7evenResignerCore.SaveDefinitionsFactory.Definitions;
using Mi5hmasH.GameLaunchers.Steam.Types;
using Mi5hmasH.Logger;

namespace QualityControl.xUnit;

public sealed class DoubleO7evenResignerCoreTests : IDisposable
{
    private readonly Core _core;
    private readonly ITestOutputHelper _output;
    
    public DoubleO7evenResignerCoreTests(ITestOutputHelper output)
    {
        _output = output;
        _output.WriteLine("SETUP");

        // Setup
        var logger = new SimpleLogger();
        var progressReporter = new ProgressReporter(null, null);
        _core = new Core(logger, progressReporter);
    }

    public void Dispose()
    {
        _output.WriteLine("CLEANUP");
    }

    [Theory]
    [InlineData("save_12345.sav")]
    [InlineData("index.save")]
    [InlineData("data.save")]
    public void SaveDefinitionRegistry_DoesNotThrow_WhenGetDefinition(string fileName)
    {
        // Arrange
        var testResult = true;

        // Act
        try
        {
            _ = SaveDefinitionRegistry.Get(fileName);
        }
        catch
        {
            testResult = false;
        }

        // Assert
        Assert.True(testResult);
    }

    [Fact]
    public async Task UnsignFilesAsync_DoesNotThrow_WhenNoFiles()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var testResult = true;

        // Act
        try
        {
            await _core.UnsignFilesAsync(tempDir, cts);
        }
        catch
        {
            testResult = false;
        }
        Directory.Delete(tempDir);

        // Assert
        Assert.True(testResult);
    }

    [Fact]
    public async Task SignFilesAsync_DoesNotThrow_WhenNoFiles()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var testResult = true;

        // Act
        try
        {
            await _core.SignFilesAsync(tempDir, cts);
        }
        catch
        {
            testResult = false;
        }
        Directory.Delete(tempDir);

        // Assert
        Assert.True(testResult);
    }

    [Fact]
    public async Task ResignFilesAsync_DoesNotThrow_WhenNoFiles()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var testResult = true;

        // Act
        try
        {
            await _core.ResignFilesAsync(tempDir, "0", "1", cts);
        }
        catch
        {
            testResult = false;
        }
        Directory.Delete(tempDir);

        // Assert
        Assert.True(testResult);
    }

    [Fact]
    public void UnSignFile_DoesUnSign()
    {
        // Arrange
        const string userId = "76561197960265729";
        var steamId = new SteamId(userId);
        var data = Properties.Resources.signedFile;

        // Act
        DoubleO7igner.UnSign(data, steamId.GetSteamId64());

        // Assert
        Assert.Equal(Properties.Resources.unsignedDecompressedFile, (ReadOnlySpan<byte>)data);
    }

    [Fact]
    public void ResignFile_DoesResign()
    {
        // Arrange
        const string userIdInput = "76561197960265729";
        const string userIdOutput = "76561197960265730";
        var steamIdInput = new SteamId(userIdInput);
        var steamIdOutput = new SteamId(userIdOutput);
        var data = Properties.Resources.signedFile;

        // Act
        DoubleO7igner.ReSign(data, steamIdInput.GetSteamId64(), steamIdOutput.GetSteamId64());
        var result = IndexSaveDefinition.GuessXorMask(data);
        var resultUserId = BitConverter.ToUInt64(result).ToString();

        // Assert
        Assert.Equal(userIdOutput, resultUserId);
    }

    [Fact]
    public void GuessFileOwner_DoesSucceed()
    {
        // Arrange
        const string expectedId = "76561197960265729";
        var data = Properties.Resources.signedFile;

        // Act
        var result = IndexSaveDefinition.GuessXorMask(data);
        var resultUserId = BitConverter.ToUInt64(result).ToString();

        // Assert
        Assert.Equal(expectedId, resultUserId);
    }

    [Fact]
    public void DecompressFile_DoesDecompress()
    {
        // Arrange
        var data = Properties.Resources.unsignedCompressedFile;

        // Act
        var decompressedData = Zlib.Decompress(data);

        // Assert
        Assert.Equal(Properties.Resources.unsignedDecompressedFile, (ReadOnlySpan<byte>)decompressedData);
    }

    [Fact]
    public void CompressFile_DoesCompress()
    {
        // Arrange
        var data = Properties.Resources.unsignedDecompressedFile;

        // Act
        var compressedData = Zlib.Compress(data);
        var decompressedData = Zlib.Decompress(compressedData);

        // Assert
        Assert.Equal(Properties.Resources.unsignedDecompressedFile, (ReadOnlySpan<byte>)decompressedData);
    }
}