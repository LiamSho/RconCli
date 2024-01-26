using Cocona;
using RconCli.Commands;

CoconaLiteApp.Run<RootCommand>(configureOptions: options =>
{
    options.EnableShellCompletionSupport = true;
});
