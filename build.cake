#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
#tool "nuget:?package=GitVersion.CommandLine"
// #tool "nuget:?package=gitlink"

// ARGUMENTS
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var solutionPath = "./src/Unleash.sln";
var unleashProjectFile = "./src/Unleash/Unleash.csproj";
var buildDir = Directory("./src/Unleash/bin") + Directory(configuration);

//
// TASKS
//
Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(solutionPath);
});

Task("Version")
    .Does(() => 
{
    var versionInfo = GitVersion(new GitVersionSettings 
    { 
        OutputType = GitVersionOutput.Json 
    });
    
    var updatedProjectFile = System.IO.File.ReadAllText(unleashProjectFile)
        .Replace("1.0.0", versionInfo.NuGetVersion);

    System.IO.File.WriteAllText(unleashProjectFile, updatedProjectFile);
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    if(IsRunningOnWindows())
    {
      // Use MSBuild
      MSBuild(solutionPath, settings =>
        settings.SetConfiguration(configuration));
    }
    else
    {
      // Use XBuild
      XBuild(solutionPath, settings =>
        settings.SetConfiguration(configuration));
    }
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    NUnit3("./src/*.Tests/bin/" + configuration + "/*.Tests.dll", new NUnit3Settings {
        NoResults = true
    });
});

//
// TASK TARGETS
//
Task("Default")
    .IsDependentOn("Run-Unit-Tests");

Task("AppVeyor")  
    .IsDependentOn("Version")
    .IsDependentOn("Default");

//
// EXECUTION
//
RunTarget(target);
