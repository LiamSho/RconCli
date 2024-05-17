using Cocona;
using RconCli.Commands;

await CoconaLiteApp.RunAsync<RootCommand>(configureOptions: options =>
{
    options.EnableShellCompletionSupport = true;
});
