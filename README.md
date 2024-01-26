# RconCli

A simple command line tool to use RCON protocol to communicate with a game server.

## Installation

You need to have [.NET 8](https://dotnet.microsoft.com/download/dotnet/8.0) installed.

Run the following command to install the tool:

```bash
dotnet tool install -g AlisaLab.RconCli
```

Then you can use the tool by running `rcon` command.

## Usage

You can run `rcon --help` to see the usage.

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
rcon profile add <profile name> -H <hostname or IPv4 address> -p <port> -w <password> -d <description>

# Remove a profile
rcon profile remove <profile name>

# List all profiles
rcon profile list

# Connect to a profile
rcon connect <profile name>
```

## License

This project is licensed under the MIT License - see the [LICENSE](./LICENSE) file for details
