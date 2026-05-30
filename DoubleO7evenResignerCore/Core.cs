using DoubleO7evenResignerCore.Helpers;
using DoubleO7evenResignerCore.Infrastructure;
using DoubleO7evenResignerCore.SaveDefinitionsFactory;
using DoubleO7evenResignerCore.SaveDefinitionsFactory.Definitions;
using Mi5hmasH.GameLaunchers.Steam.Types;
using Mi5hmasH.Logger;

namespace DoubleO7evenResignerCore;

public class Core(SimpleLogger logger, ProgressReporter progressReporter)
{
    /// <summary>
    /// Creates a new ParallelOptions instance configured with the specified cancellation token and an optimal degree of parallelism for the current environment.
    /// </summary>
    /// <param name="cts">The CancellationTokenSource whose token will be used to support cancellation of parallel operations.</param>
    /// <returns>A ParallelOptions object initialized with the provided cancellation token and a maximum degree of parallelism based on the number of available processors.</returns>
    private static ParallelOptions GetParallelOptions(CancellationTokenSource cts)
        => new()
        {
            CancellationToken = cts.Token,
            MaxDegreeOfParallelism = Math.Max(Environment.ProcessorCount - 1, 1)
        };

    /// <summary>
    /// Signs save files from the specified directory using a Steam user ID and writes the results to a new output directory.
    /// </summary>
    /// <param name="inputDir">Directory path containing the save files to sign.</param>
    /// <param name="cts">Cancellation token source to cancel the signing operation.</param>
    /// <param name="userId">Steam user ID for signing the files. Defaults to <see langword="null"/>.</param>
    /// <param name="shouldCompress">Indicates whether to compress data save files before signing. Defaults to <see langword="false"/>.</param>
    public async Task SignFilesAsync(string inputDir, CancellationTokenSource cts, string? userId = null, bool shouldCompress = false)
        => await Task.Run(() => SignFiles(inputDir, cts, userId, shouldCompress));

    /// <summary>
    /// Asynchronously signs save files from the specified directory using a Steam user ID and writes the results to a new output directory.
    /// </summary>
    /// <param name="inputDir">Directory path containing the save files to sign.</param>
    /// <param name="cts">Cancellation token source to cancel the signing operation.</param>
    /// <param name="userId">Steam user ID for signing the files. Defaults to <see langword="null"/>.</param>
    /// <param name="shouldCompress">Indicates whether to compress data save files before signing. Defaults to <see langword="false"/>.</param>
    public void SignFiles(string inputDir, CancellationTokenSource cts, string? userId = null, bool shouldCompress = false)
    {
        // GET FILES TO PROCESS
        var filesToProcess = SaveDataFileIo.GetFiles(inputDir);
        if (filesToProcess.Length == 0) return;
        // PROCESS FILES
        logger.LogInfo($"Processing [{filesToProcess.Length}] files...");
        // Get signature from Steam ID
        var localUserId = userId ?? "0";
        var steamId = new SteamId(localUserId);
        // Define action
        var action = shouldCompress ? "signed_compressed" : "signed";
        // Create a new folder in OUTPUT directory
        var outputDir = Directories.GetNewOutputDirectory(action).AddUserId(localUserId);
        Directory.CreateDirectory(outputDir);
        // Crate the folder structure in the newly created output directory
        Directories.CreateOutputFolderStructure(filesToProcess, inputDir, outputDir);
        // Setup parallel options
        var po = GetParallelOptions(cts);
        // Process files in parallel
        var progress = 0;
        try
        {
            Parallel.For((long)0, filesToProcess.Length, po, (ctr, _) =>
            {
                while (true)
                {
                    var fileName = Path.GetFileName(filesToProcess[ctr]);
                    var group = $"Task {ctr}";

                    // Try to read file data
                    byte[] data;
                    try { data = File.ReadAllBytes(filesToProcess[ctr]); }
                    catch (Exception ex)
                    {
                        logger.LogError($"[{progress}/{filesToProcess.Length}] Failed to read the [{fileName}] file: {ex}", group);
                        break; // Skip to the next file
                    }
                    // Try to process the file data
                    var dataSpan = data.AsSpan(); 
                    try
                    {
                        // If the file supports compression and shouldCompress is true then compress it before signing
                        var def = SaveDefinitionRegistry.Get(fileName);
                        if (def.SupportsCompression && shouldCompress)
                        {
                            var compressedData = Zlib.Compress(data);
                            dataSpan = compressedData.AsSpan();
                        }
                        // Sign the file based on the user ID
                        if (localUserId != "0")
                            DoubleO7igner.UnSign(dataSpan, steamId.GetSteamId64());
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"[{progress}/{filesToProcess.Length}] Failed to process the [{fileName}] file: {ex}", group);
                        break; // Skip to the next file
                    }
                    // Try to save the processed file data
                    try
                    {
                        // Save the decrypted data to the output directory, preserving the folder structure
                        var outputFilePath = filesToProcess[ctr].Replace(inputDir, outputDir);
                        File.WriteAllBytes(outputFilePath, dataSpan);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Failed to save the file: {ex}", group);
                        break; // Skip to the next file
                    }
                    logger.LogInfo($"[{progress}/{filesToProcess.Length}] Processed the [{fileName}] file.", group);
                    break;
                }
                Interlocked.Increment(ref progress);
                progressReporter.Report((int)((double)progress / filesToProcess.Length * 100));
            });
            logger.LogInfo($"[{progress}/{filesToProcess.Length}] All tasks completed.");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex.Message);
        }
        finally
        {
            // Ensure progress is set to 100% at the end
            progressReporter.Report(100);
        }
    }

    /// <summary>
    /// Asynchronously removes signatures from save files and optionally decompresses them, processing files in parallel and preserving the folder structure in the output directory.
    /// </summary>
    /// <param name="inputDir">The directory containing the files to process.</param>
    /// <param name="cts">The cancellation token source for cancelling the operation.</param>
    /// <param name="userId">The Steam user ID used for unsigning. Defaults to <see langword="null"/>.</param>
    /// <param name="shouldDecompress">Indicates whether to decompress files. Defaults to <see langword="false"/>.</param>
    public async Task UnsignFilesAsync(string inputDir, CancellationTokenSource cts, string? userId = null, bool shouldDecompress = false)
        => await Task.Run(() => UnsignFiles(inputDir, cts, userId, shouldDecompress));

    /// <summary>
    /// Removes signatures from save files and optionally decompresses them, processing files in parallel and preserving the folder structure in the output directory.
    /// </summary>
    /// <param name="inputDir">The directory containing the files to process.</param>
    /// <param name="cts">The cancellation token source for cancelling the operation.</param>
    /// <param name="userId">The Steam user ID used for unsigning. Defaults to <see langword="null"/>.</param>
    /// <param name="shouldDecompress">Indicates whether to decompress files. Defaults to <see langword="false"/>.</param>
    public void UnsignFiles(string inputDir, CancellationTokenSource cts, string? userId = null, bool shouldDecompress = false)
    {
        // GET FILES TO PROCESS
        var filesToProcess = SaveDataFileIo.GetFiles(inputDir);
        if (filesToProcess.Length == 0) return;
        // PROCESS FILES
        logger.LogInfo($"Processing [{filesToProcess.Length}] files...");
        // Get signature from Steam ID
        var localUserId = userId ?? "0";
        var steamId = new SteamId(localUserId);
        // Define action
        var action = shouldDecompress ? "unsigned_decompressed" : "unsigned";
        // Create a new folder in OUTPUT directory
        var outputDir = Directories.GetNewOutputDirectory(action).AddUserId(localUserId);
        Directory.CreateDirectory(outputDir);
        // Crate the folder structure in the newly created output directory
        Directories.CreateOutputFolderStructure(filesToProcess, inputDir, outputDir);
        // Setup parallel options
        var po = GetParallelOptions(cts);
        // Process files in parallel
        var progress = 0;
        try
        {
            Parallel.For((long)0, filesToProcess.Length, po, (ctr, _) =>
            {
                while (true)
                {
                    var fileName = Path.GetFileName(filesToProcess[ctr]);
                    var group = $"Task {ctr}";

                    // Try to read file data
                    byte[] data;
                    try { data = File.ReadAllBytes(filesToProcess[ctr]); }
                    catch (Exception ex)
                    {
                        logger.LogError($"[{progress}/{filesToProcess.Length}] Failed to read the [{fileName}] file: {ex}", group);
                        break; // Skip to the next file
                    }
                    // Try to process the file data
                    var dataSpan = data.AsSpan();
                    try
                    {
                        // Unsign the file based on the user ID
                        if (localUserId != "0")
                            DoubleO7igner.UnSign(dataSpan, steamId.GetSteamId64());

                        // If the file supports compression and shouldDecompress is true then decompress it
                        var def = SaveDefinitionRegistry.Get(fileName);
                        if (def.SupportsCompression && shouldDecompress)
                        {
                            var decompressedData = Zlib.Decompress(data);
                            dataSpan = decompressedData.AsSpan();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"[{progress}/{filesToProcess.Length}] Failed to process the [{fileName}] file: {ex}", group);
                        break; // Skip to the next file
                    }
                    // Try to save the processed file data
                    try
                    {
                        // Save the decrypted data to the output directory, preserving the folder structure
                        var outputFilePath = filesToProcess[ctr].Replace(inputDir, outputDir);
                        File.WriteAllBytes(outputFilePath, dataSpan);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Failed to save the file: {ex}", group);
                        break; // Skip to the next file
                    }
                    logger.LogInfo($"[{progress}/{filesToProcess.Length}] Processed the [{fileName}] file.", group);
                    break;
                }
                Interlocked.Increment(ref progress);
                progressReporter.Report((int)((double)progress / filesToProcess.Length * 100));
            });
            logger.LogInfo($"[{progress}/{filesToProcess.Length}] All tasks completed.");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex.Message);
        }
        finally
        {
            // Ensure progress is set to 100% at the end
            progressReporter.Report(100);
        }
    }

    /// <summary>
    /// Asynchronously re-signs files in the provided directory based on a Steam ID, with parallel processing and progress reporting.
    /// </summary>
    /// <param name="inputDir">The directory containing save data files to process.</param>
    /// <param name="userIdInput">The user ID to convert to Steam ID for input signature operations.</param>
    /// <param name="userIdOutput">The user ID to convert to Steam ID for output signature operations.</param>
    /// <param name="cts">The cancellation token source to support operation cancellation.</param>
    public async Task ResignFilesAsync(string inputDir, string userIdInput, string userIdOutput, CancellationTokenSource cts)
        => await Task.Run(() => ResignFiles(inputDir, userIdInput, userIdOutput, cts));

    /// <summary>
    /// Re-signs files in the provided directory based on a Steam ID, with parallel processing and progress reporting.
    /// </summary>
    /// <param name="inputDir">The directory containing save data files to process.</param>
    /// <param name="userIdInput">The user ID to convert to Steam ID for input signature operations.</param>
    /// <param name="userIdOutput">The user ID to convert to Steam ID for output signature operations.</param>
    /// <param name="cts">The cancellation token source to support operation cancellation.</param>
    public void ResignFiles(string inputDir, string userIdInput, string userIdOutput, CancellationTokenSource cts)
    {
        // GET FILES TO PROCESS
        var filesToProcess = SaveDataFileIo.GetFiles(inputDir);
        if (filesToProcess.Length == 0) return;
        // PROCESS FILES
        logger.LogInfo($"Processing [{filesToProcess.Length}] files...");
        // Get signature from Steam ID
        var steamIdInput = new SteamId(userIdInput);
        var steamIdOutput = new SteamId(userIdOutput);
        // Create a new folder in OUTPUT directory
        var outputDir = Directories.GetNewOutputDirectory("resigned").AddUserId(userIdOutput);
        Directory.CreateDirectory(outputDir);
        // Crate the folder structure in the newly created output directory
        Directories.CreateOutputFolderStructure(filesToProcess, inputDir, outputDir);
        // Setup parallel options
        var po = GetParallelOptions(cts);
        // Process files in parallel
        var progress = 0;
        try
        {
            Parallel.For((long)0, filesToProcess.Length, po, (ctr, _) =>
            {
                while (true)
                {
                    var fileName = Path.GetFileName(filesToProcess[ctr]);
                    var group = $"Task {ctr}";

                    // Try to read file data
                    byte[] data;
                    try { data = File.ReadAllBytes(filesToProcess[ctr]); }
                    catch (Exception ex)
                    {
                        logger.LogError($"[{progress}/{filesToProcess.Length}] Failed to read the [{fileName}] file: {ex}", group);
                        break; // Skip to the next file
                    }
                    // Process file data
                    var dataSpan = data.AsSpan();
                    DoubleO7igner.ReSign(dataSpan, steamIdInput.GetSteamId64(), steamIdOutput.GetSteamId64());
                    // Try to save the processed file data
                    try
                    {
                        // Save the decrypted data to the output directory, preserving the folder structure
                        var outputFilePath = filesToProcess[ctr].Replace(inputDir, outputDir);
                        File.WriteAllBytes(outputFilePath, dataSpan);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Failed to save the file: {ex}", group);
                        break; // Skip to the next file
                    }
                    logger.LogInfo($"[{progress}/{filesToProcess.Length}] Re-signed the [{fileName}] file.", group);
                    break;
                }
                Interlocked.Increment(ref progress);
                progressReporter.Report((int)((double)progress / filesToProcess.Length * 100));
            });
            logger.LogInfo($"[{progress}/{filesToProcess.Length}] All tasks completed.");
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex.Message);
        }
        finally
        {
            // Ensure progress is set to 100% at the end
            progressReporter.Report(100);
        }
    }

    /// <summary>
    /// Asynchronously attempts to guess the UserID from the specified input directory.
    /// </summary>
    /// <param name="inputDir">The input directory containing the files to process.</param>
    /// <param name="cts">The cancellation token source.</param>
    /// <returns>The guessed UserID if successful; otherwise, <see langword="null"/>.</returns>
    public async Task<ulong?> GuessUserIdAsync(string inputDir, CancellationTokenSource cts)
        => await Task.Run(() => GuessUserId(inputDir, cts));

    /// <summary>
    /// Attempts to guess the UserID from the specified input directory.
    /// </summary>
    /// <param name="inputDir">The input directory containing the files to process.</param>
    /// <param name="cts">The cancellation token source.</param>
    /// <returns>The guessed UserID if successful; otherwise, <see langword="null"/>.</returns>
    public ulong? GuessUserId(string inputDir, CancellationTokenSource cts)
    {
        // GET FILES TO PROCESS
        var def = new IndexSaveDefinition();
        var filesToProcess = Directory.GetFiles(inputDir, def.FullFileName, SearchOption.AllDirectories);
        if (filesToProcess.Length == 0) return null;
        // PROCESS FILE
        var file = filesToProcess[0];
        var fileName = Path.GetFileName(file);
        // Try to read file data
        byte[] data;
        try { data = File.ReadAllBytes(file); }
        catch (Exception ex)
        {
            logger.LogError($"Failed to read the [{fileName}] file: {ex}");
            return null;
        }
        // Guessing UserID
        logger.LogInfo("Guessing the UserID...");
        try
        {
            var result = IndexSaveDefinition.GuessXorMask(data);
            var userId = BitConverter.ToUInt64(result);
            logger.LogInfo($"Found UserID: {userId}.");
            return userId;
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to guess the userId for the [{fileName}] file data: {ex}");
            return null;
        }
    }
}