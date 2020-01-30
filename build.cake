#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
#tool "nuget:?package=GitVersion.CommandLine"
#addin "nuget:?package=Cake.Json"

// #tool "nuget:?package=gitlink"

// ARGUMENTS
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var solutionPath = "./Unleash.sln";
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

    Console.WriteLine(SerializeJsonPretty(versionInfo));

    var updatedProjectFile = System.IO.File.ReadAllText(unleashProjectFile)
        .Replace("1.0.0", versionInfo.AssemblySemVer);

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

Task("Download-Client-Specifications")
    .Does(() =>
{
    var indexPath = File("./tests/Unleash.Tests/Integration/Data/index.json");
    DownloadFile("https://raw.githubusercontent.com/Unleash/client-specification/master/specifications/index.json", indexPath);
	
	foreach (var fileName in DeserializeJsonFromFile<string[]>(indexPath)) 
	{
		var filePath = File("./tests/Unleash.Tests/Integration/Data/" + fileName);
		DownloadFile("https://raw.githubusercontent.com/Unleash/client-specification/master/specifications/" + fileName, filePath);
	}
	
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
	.IsDependentOn("Download-Client-Specifications")
    .Does(() =>
{
    NUnit3("./tests/*.Tests/bin/" + configuration + "/*.Tests.dll", new NUnit3Settings {
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
