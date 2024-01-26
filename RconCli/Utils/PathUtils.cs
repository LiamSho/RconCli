namespace RconCli.Utils;

public static class PathUtils
{
    public static DirectoryInfo GetAppDataDirectory()
    {
        // On Windows: C:\Users\{username}\AppData\Roaming\alisa-lab\rcon-cli
        // On Linux:
        //      - ${XDG_CONFIG_HOME}/alisa-lab/rcon-cli
        //      - ${HOME}/.config/alisa-lab/rcon-cli
        //      - /home/${USER}/.config/alisa-lab/rcon-cli
        // On macOS: /Users/${USER}/.config/alisa-lab/rcon-cli

        string appDataDirectory;

        if (OperatingSystem.IsWindows())
        {
            var windowsRoamingAppDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            appDataDirectory = Path.Combine(windowsRoamingAppDataDirectory, "alisa-lab", "rcon-cli");
        }
        else if (OperatingSystem.IsMacOS())
        {
            var macCurrentUserHomeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            appDataDirectory = Path.Combine(macCurrentUserHomeDirectory, ".config", "alisa-lab", "rcon-cli");
        }
        else if (OperatingSystem.IsLinux())
        {
            var linuxXdgConfigHomeDirectory = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");

            if (linuxXdgConfigHomeDirectory is not null)
            {
                appDataDirectory = Path.Combine(linuxXdgConfigHomeDirectory, "alisa-lab", "rcon-cli");
            }
            else
            {
                var linuxHomeEnvironmentVariable = Environment.GetEnvironmentVariable("HOME");
                var linuxCurrentUserHomeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                var linuxHome = linuxHomeEnvironmentVariable ?? linuxCurrentUserHomeDirectory;
                appDataDirectory = Path.Combine(linuxHome, ".config", "alisa-lab", "rcon-cli");
            }
        }
        else
        {
            throw new PlatformNotSupportedException();
        }

        appDataDirectory = Path.GetFullPath(appDataDirectory);

        var appDataDirectoryInfo = new DirectoryInfo(appDataDirectory);

        if (appDataDirectoryInfo.Exists is false)
        {
            appDataDirectoryInfo.Create();
        }

        return appDataDirectoryInfo;
    }
}
