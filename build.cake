#addin "Cake.Powershell"

#load "scripts/utilities.cake"

var target = Argument("target", "Default");
var home = Directory(HomeFolder());

Task("Default")
  .IsDependentOn("choco")
  .IsDependentOn("git")
  .IsDependentOn("vscode")
  .IsDependentOn("ssh")
  .Does(() =>
{
});

Task("git")
  .Does(() =>
{
  dotfile("git/gitconfig", home);
  dotfile("git/gitconfig.local", home);
  dotfile("git/gitignore.global", home);
});

Task("vscode")
  .Does(() =>
{
  var app_home = home;
  if (IsRunningOnWindows())
  {
    app_home = Directory($"{EnvironmentVariable("APPDATA")}/Code/User");
  } else if (IsRunningOnLinux()) {
    app_home = Directory($"{home}/.config/Code"); 
  } else if (IsRunningOnMac()) {
    app_home = Directory($"{home}/Library/Application Support/Code");
  } else {
    return;
  }
  EnsureDirectoryExists(app_home);
  dotfile("vscode/settings.json", app_home, false);
});

Task("ssh")
  .Does(() =>
{
  var app_home = Directory($"{  home}/.ssh");
  EnsureDirectoryExists(app_home);
  dotfile("ssh/config", app_home, false);
});

/// <summary>
/// When you cannot get enough package managers 🤣
/// </summary>
Task("choco")
  .WithCriteria(IsRunningOnWindows())
  .WithCriteria(!DirectoryExists($"{EnvironmentVariable("HOMEDRIVE")}/ProgramData/chocolatey"))
  .WithCriteria(!HasEnvironmentVariable("ChocolateyInstall"))
  .Does(() =>
{
  StartPowershellScript("Start-Process", args => {
    args
      .Append("powershell")
      .Append("Verb", "Runas")
      .AppendStringLiteral(
        "ArgumentList", "Set-ExecutionPolicy Bypass -Scope Process -Force; " +
        "iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))"
      );
  });
});

RunTarget(target);
