#tool "nuget:?package=GitVersion.CommandLine"

var target = Argument("target", "Default");
var configuration = Argument("Configuration", "Release");
var key_nuget = EnvironmentVariable("NUGET_KEY") ?? "";
var url_nuget_push = EnvironmentVariable("NUGET_URL") ?? "https://www.nuget.org/api/v2/package";

var root = Directory("./");
var solution = root + File("ModSink.Common.sln");

var src = root + Directory("src");

var modSinkCore_dir = src + Directory("ModSink.Core");
var modSinkCore_csproj = modSinkCore_dir + File("ModSink.Core.csproj");

var modSinkCommon_dir = src + Directory("ModSink.Common");
var modSinkCommon_csproj = modSinkCommon_dir + File("ModSink.Common.csproj");


var modSinkCommonTests_dir = src + Directory("ModSink.Common.Tests");
var modSinkCommonTests_csproj = modSinkCommonTests_dir + File("ModSink.Common.Tests.csproj");

var modSinkCli_dir = src + Directory("ModSink.CLI");
var modSinkCli_csproj = modSinkCli_dir + File("ModSink.CLI.csproj");

var output = root + Directory("out");
var out_nuget = output + Directory("nuget");

var SquirrelVersion = "";
var NuGetVersion = "";
var Version = "";

Setup(context =>
{
    Information("Getting versions");
    var v = GitVersion();
    SquirrelVersion = v.LegacySemVerPadded;
    Information("Version for Squirrel: " + SquirrelVersion);
    NuGetVersion = v.NuGetVersionV2;
    Information("Version for NuGet libraries: " + NuGetVersion);
    Version = v.FullSemVer;
    Information("Full version info: "+ v.InformationalVersion);

    Information("Updating version in AssemblyInfo files");
    GitVersion(new GitVersionSettings { 
        UpdateAssemblyInfo = true, 
        OutputType = GitVersionOutput.BuildServer 
    });
});

Task("Test")
    .Does(()=>{
        DotNetCoreTest(modSinkCommonTests_csproj);
    });

Task("Pack.Common")
    .Does(() =>
{
    DotNetCorePack(modSinkCommon_csproj, new DotNetCorePackSettings
     {
         Configuration = "Release",
         OutputDirectory = out_nuget,
         ArgumentCustomization = args=>args.Append("/p:PackageVersion="+NuGetVersion)
     });
});

Task("Pack.Core")
    .Does(() =>
{
    DotNetCorePack(modSinkCore_csproj, new DotNetCorePackSettings
     {
         Configuration = "Release",
         OutputDirectory = out_nuget,
         ArgumentCustomization = args=>args.Append("/p:PackageVersion="+NuGetVersion)
     });
});

Task("Publish")
    .IsDependentOn("Pack.Core")
    .IsDependentOn("Pack.Common")
    .WithCriteria(!String.IsNullOrWhiteSpace(key_nuget))
    .Does(()=>{
        foreach(var nupkg in GetFiles(out_nuget.ToString()+"/**/*.nupkg")){
            Information("Publishing: {0}", nupkg);
            NuGetPush(nupkg, new NuGetPushSettings {
                Source = url_nuget_push,
                ApiKey = key_nuget
            });
        }
    });

Task("Default")
    .IsDependentOn("Test")
    .IsDependentOn("Publish");

RunTarget(target);
