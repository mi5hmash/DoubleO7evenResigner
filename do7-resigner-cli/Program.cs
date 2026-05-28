using DoubleO7evenResignerCore;
using DoubleO7evenResignerCore.Helpers;
using DoubleO7evenResignerCore.Infrastructure;
using Mi5hmasH.AppInfo;
using Mi5hmasH.ConsoleHelper;
using Mi5hmasH.Logger;
using Mi5hmasH.Logger.Models;
using Mi5hmasH.Logger.Providers;

#region SETUP

// CONSTANTS
const string breakLine = "---";

// Initialize APP_INFO
var appInfo = new MyAppInfo("do7-resigner-cli");

// Initialize LOGGER
var logger = new SimpleLogger
{
    LoggedAppName = appInfo.Name
};
// Configure ConsoleLogProvider
var consoleLogProvider = new ConsoleLogProvider();
logger.AddProvider(consoleLogProvider);
// Configure FileLogProvider
var fileLogProvider = new FileLogProvider(MyAppInfo.RootPath, 2);
fileLogProvider.CreateLogFile();
logger.AddProvider(fileLogProvider);
// Add event handler for unhandled exceptions
AppDomain.CurrentDomain.UnhandledException += (_, e) =>
{
    if (e.ExceptionObject is not Exception exception) return;
    var logEntry = new LogEntry(SimpleLogger.LogSeverity.Critical, $"Unhandled Exception: {exception}");
    fileLogProvider.Log(logEntry);
    fileLogProvider.Flush();
};
// Flush log providers on process exit
AppDomain.CurrentDomain.ProcessExit += (_, _) => logger.Flush();

//Initialize ProgressReporter
var progressReporter = new ProgressReporter(new Progress<string>(Console.WriteLine), null);

// Initialize CORE
var core = new Core(logger, progressReporter);

// Print HEADER
ConsoleHelper.PrintHeader(appInfo, breakLine);

// Say HELLO
ConsoleHelper.SayHello(breakLine);

// Get ARGUMENTS from command line
#if DEBUG
// For debugging purposes, you can manually set the arguments...
if (args.Length < 1)
{
    // ...below
    const string localArgs = "-m TEST";
    args = ConsoleHelper.GetArgs(localArgs);
}
#endif
var arguments = ConsoleHelper.ReadArguments(args);
#if DEBUG
// Write the arguments to the console for debugging purposes
ConsoleHelper.WriteArguments(arguments);
Console.WriteLine(breakLine);
#endif

#endregion

#region MAIN

// Optional argument: doNotWait
var doNotWait = arguments.ContainsKey("-q");

// Show HELP if no arguments are provided or if -h is provided
if (arguments.Count == 0 || arguments.ContainsKey("-h"))
{
    PrintHelp();
    goto EXIT;
}

// Get MODE
arguments.TryGetValue("-m", out var mode);
switch (mode)
{
    case "unsign" or "u":
        await UnsignAll();
        break;
    case "sign" or "s":
        await SignAll();
        break;
    case "resign" or "r":
        await ResignAll();
        break;
    case "guess" or "g":
        await GuessUserId();
        break;
    default:
        throw new ArgumentException($"Unknown mode: '{mode}'.");
}

// EXIT the application
EXIT:
Console.WriteLine(breakLine); // print a break line
ConsoleHelper.SayGoodbye(breakLine);
if (!doNotWait) ConsoleHelper.PressAnyKeyToExit();
return;

#endregion

#region HELPERS

static void PrintHelp()
{
    const string userIdInput = "76561197960265729";
    const string userIdOutput = "76561197960265730";
    var inputPath = Path.Combine(".", "InputDirectory");
    var exeName = Path.Combine(".", Path.GetFileName(Environment.ProcessPath) ?? "ThisExecutableFileName.exe");
    var helpMessage = $"""
                       Usage: {exeName} -m <mode> [options]

                       Modes:
                         -m u  Unsign SaveData files
                         -m s  Sign SaveData files
                         -m r  Re-sign SaveData files
                         -m g  Guess the User ID from an index file

                       Options:
                         -p <path>     Path to folder containing SaveData files
                         -u <user_id>  User ID (used in unsign/sign modes)
                         -uI <old_id>  Original User ID (used in re-sign mode)
                         -uO <new_id>  New User ID (used in re-sign mode)
                         -g            Guess the User ID from the first index file (only for unsign/re-sign modes, overrides -u and -uI)
                         -z            Compress (when in sign mode) or Decompress (when in unsign mode) files
                         -q            Don't wait for user input to exit after operation completes (auto-close)
                         -h            Show this help message

                       Examples:
                         Unsign:  {exeName} -m u -p "{inputPath}" -u {userIdInput}
                         Sign:  {exeName} -m s -p "{inputPath}" -u {userIdOutput}
                         Re-sign:  {exeName} -m r -p "{inputPath}" -uI {userIdInput} -uO {userIdOutput}
                         Guess User ID:  {exeName} -m g -p "{inputPath}"
                         Guess + Unsign:  {exeName} -m u -p "{inputPath}" -g
                         Guess + Re-sign:  {exeName} -m r -p "{inputPath}" -g -uO {userIdOutput}
                         Decompress:  {exeName} -m u -p "{inputPath}" -z
                         Compress:  {exeName} -m s -p "{inputPath}" -z
                         Unsign + Decompress:  {exeName} -m u -p "{inputPath}" -u {userIdInput} -z
                         Compress + Sign:  {exeName} -m s -p "{inputPath}" -u {userIdOutput} -z
                       """;
    Console.WriteLine(helpMessage);
}

string GetValidatedInputRootPath()
{
    arguments.TryGetValue("-p", out var inputRootPath);
    if (File.Exists(inputRootPath)) inputRootPath = Path.GetDirectoryName(inputRootPath);
    return !Directory.Exists(inputRootPath)
        ? throw new DirectoryNotFoundException(
            $"The provided path '{inputRootPath}' is not a valid directory or does not exist.")
        : inputRootPath.TrimDirectorySeparator();
}

#endregion

#region MODES

async Task UnsignAll()
{
    var cts = new CancellationTokenSource();
    var inputRootPath = GetValidatedInputRootPath();
    var shouldGuess = arguments.ContainsKey("-g");
    string? userId;
    if (shouldGuess)
    {
        var guessedValue = await core.GuessUserIdAsync(inputRootPath, cts);
        userId = guessedValue.ToString();
    }
    else
    {
        arguments.TryGetValue("-u", out userId);
    }
    var shouldDecompress = arguments.ContainsKey("-z");
    
    // Process Files
    await core.UnsignFilesAsync(inputRootPath, cts, userId, shouldDecompress);
    cts.Dispose();
}

async Task SignAll()
{
    var cts = new CancellationTokenSource();
    arguments.TryGetValue("-u", out var userId);
    var shouldCompress = arguments.ContainsKey("-z");
    var inputRootPath = GetValidatedInputRootPath();
    // Process Files
    await core.SignFilesAsync(inputRootPath, cts, userId, shouldCompress);
    cts.Dispose();
}

async Task ResignAll()
{
    var cts = new CancellationTokenSource();
    var inputRootPath = GetValidatedInputRootPath();
    var shouldGuess = arguments.ContainsKey("-g");
    string? userIdInput;
    if (shouldGuess)
    {
        var guessedValue = await core.GuessUserIdAsync(inputRootPath, cts);
        userIdInput = guessedValue.ToString() ?? throw new ArgumentException("Failed to guess User ID.");
    }
    else
    {
        arguments.TryGetValue("-uI", out userIdInput);
        if (string.IsNullOrEmpty(userIdInput))
            throw new ArgumentException("Input User ID is missing.");
    }

    arguments.TryGetValue("-uO", out var userIdOutput);
    if (string.IsNullOrEmpty(userIdOutput))
        throw new ArgumentException("Output User ID is missing.");
    
    // Re-sign Files
    await core.ResignFilesAsync(inputRootPath, userIdInput, userIdOutput, cts);
    cts.Dispose();
}

async Task GuessUserId()
{
    var cts = new CancellationTokenSource();
    var inputRootPath = GetValidatedInputRootPath();
    await core.GuessUserIdAsync(inputRootPath, cts);
    cts.Dispose();
}

#endregion