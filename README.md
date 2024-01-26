# RconCli

A simple command line tool to use RCON protocol to communicate with a game server.

## Installation

You need to have [.NET 8](https://dotnet.microsoft.com/download/dotnet/8.0) installed.

Run the following command to install the tool:

```bash
dotnet tool install -g AlisaLab.RconCli
```

Then you can use the tool by running `rcon` command.

## Sample Usages

You can run `rcon --help` to see the help message.

### RCON Library

RconCli use 2 different RCON libraries to communicate with the server:

- [RconSharp](https://github.com/stefanodriussi/rconsharp)
- [CoreRCON](https://github.com/Challengermode/CoreRcon)

You can change which library to use by command line options or through the interactive RCON shell command.

If the library to use is not specified, RconCli will use `RconSharp` by default.

### Direct connect

```bash
rcon direct -H <hostname or IPv4 address> -p <port> -w <password>
```

### Profile management

Profile file location:
- Windows: `${APPDATA}\alisa-lab\rcon-cli\profiles.json`
- macOS: `/Users/${USER}/.config/alisa-lab/rcon-cli/profiles.json`
- Linux (1): `${XDG_CONFIG_HOME}/alisa-lab/rcon-cli/profiles.json`
- Linux (2): `${HOME}/.config/alisa-lab/rcon-cli/profiles.json`
- Linux (3): `/home/${USER}/.config/alisa-lab/rcon-cli/profiles.json`

```bash
# Create a profile
rcon profile add <profile name> -H <hostname or IPv4 address> -p <port> -w <password> -d <description> -e <rcon library>

# Remove a profile
rcon profile remove <profile name>

# List all profiles
rcon profile list

# Connect to a profile
rcon connect <profile name>
```

## Third-party libraries

- [RconSharp](https://github.com/stefanodriussi/rconsharp) under `MIT`, for RCON communication
- [CoreRCON](https://github.com/Challengermode/CoreRcon) under `MIT`, for RCON communication
- [Cocona.Lite](https://github.com/mayuki/Cocona) under `MIT`, for command line parsing
- [Spectre.Console](https://github.com/spectreconsole/spectre.console) under `MIT`, for command line UI
- [SonarAnalyzer.CSharp](https://github.com/SonarSource/sonar-dotnet) under `LGPL-3.0`, for code analysis, not included in the release binary

## License

This project is licensed under the MIT License - see the [LICENSE](./LICENSE) file for details
