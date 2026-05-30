[![License: MIT](https://img.shields.io/badge/License-MIT-blueviolet.svg)](https://opensource.org/license/mit)
[![Release Version](https://img.shields.io/github/v/tag/mi5hmash/DoubleO7evenResigner?label=Version)](https://github.com/mi5hmash/DoubleO7evenResigner/releases/latest)
[![Visual Studio 2026](https://custom-icon-badges.demolab.com/badge/Visual%20Studio%202026-F0ECF8.svg?&logo=visual-studio-26)](https://visualstudio.microsoft.com/)
[![.NET10](https://img.shields.io/badge/.NET%2010-512BD4?logo=dotnet&logoColor=fff)](#)

> [!IMPORTANT]
> **This software is free and open source. If someone asks you to pay for it, it's likely a scam.**

# 💣💣7️⃣ DoubleO7evenResigner - What is it :interrobang:
This application can **sign and unsign SaveData files** from the "007 First Light" game. It can also **compress and decompress these SaveData files** and **re-sign them** with your own SteamID to **use anyone’s SaveData on your Steam Account**.

## Supported Platforms
|Platform|File Signature|
|---|---|
|STEAM|✅ signed with Steam ID|
|EPIC|❌ not signed|

# 🤯 Why was it created :interrobang:
I wanted to share a SaveData file with a friend, but it isn't possible by default.

# :scream: Is it safe?
The short answer is: **No.** 
> [!CAUTION]
> If you unreasonably edit your SaveData files, you risk corrupting them or getting banned from playing online. In both cases, you will lose your progress.

> [!IMPORTANT]
> Always create a backup of any files before editing them.

> [!IMPORTANT]
> Disable the Steam Cloud before you replace any SaveData files.

You’ve been warned. Now that you fully understand the possible consequences, you may proceed to the next chapter.

# :scroll: How to use this tool
## [CLI] - 🪟 Windows | 🐧 Linux | 🍎 macOS

```plaintext
Usage: .\do7-resigner-cli.exe -m <mode> [options]

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
  -g            Guess the User ID from the first index file (only for 
                unsign/re-sign modes, overrides -u and -uI)
  -z            Compress (when in sign mode) or Decompress (when in unsign mode) files
  -q            Don't wait for user input to exit after operation completes (auto-close)
  -h            Show this help message
```

### Examples:
#### Unsign (Convert from Steam to Epic)
```bash
.\do7-resigner-cli.exe -m u -p ".\InputDirectory" -u 76561197960265729
```
#### Sign (Convert from Epic to Steam)
```bash
.\do7-resigner-cli.exe -m s -p ".\InputDirectory" -u 76561197960265729
```
#### Re-sign (Convert to other Steam Account)
```bash
.\do7-resigner-cli.exe -m r -p ".\InputDirectory" -uI 76561197960265729 -uO 76561197960265730
```
#### Guess User ID
```bash
.\do7-resigner-cli.exe -m u -p ".\InputDirectory" -g
```
#### Guess + Unsign (when you do not know the User ID)
```bash
.\do7-resigner-cli.exe -m u -p ".\InputDirectory" -g
```
#### Guess + Re-sign (when you do not know the Original User ID)
```bash
.\do7-resigner-cli.exe -m r -p ".\InputDirectory" -g -uO 76561197960265730
```
#### Decompress
```bash
.\do7-resigner-cli.exe -m u -p ".\InputDirectory" -z
```
#### Compress
```bash
.\do7-resigner-cli.exe -m s -p ".\InputDirectory" -z
```
#### Unsign + Decompress
```bash
.\do7-resigner-cli.exe -m u -p ".\InputDirectory" -u 76561197960265729 -z
```
#### Compress + Sign
```bash
.\do7-resigner-cli.exe -m s -p ".\InputDirectory" -u 76561197960265729 -z
```

> [!NOTE]
> Modified files are being placed in a newly created folder within the ***"DoubleO7evenResigner/_OUTPUT/"*** folder.

# :fire: Issues
All the problems I've encountered during my tests have been fixed on the go. If you find any other issues (which I hope you won't) feel free to report them [there](https://github.com/mi5hmash/DoubleO7evenResigner/issues).

> [!TIP]
> This application creates a log file that may be helpful in troubleshooting.
It can be found in the same directory as the executable file.
Application stores up to two log files from the most recent sessions.

## [ISSUE] Game doesn’t detect SaveData file\s on Steam
If you purchased the game on Steam and it doesn’t detect your re‑signed SaveData files after you’ve placed them in the correct folder, the issue is usually that a valid `remotecache.vdf` file also needs to be generated.

Instructions on how to generate a proper `remotecache.vdf` can be found in [this gist](https://gist.github.com/mi5hmash/47f1be53d213be9b00f2c7e0aa151b11).

# 🔄 Alternatives  
If you’re looking for an alternative, you might check out [007-First-Light-Save-Tools](https://github.com/Dxian998/007-First-Light-Save-Tools) by [Dxian998](https://github.com/Dxian998).
