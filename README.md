# Playground

export AZURE_CONFIG_DIR=~/.azure_T1
mkdir -p ~/.azure_T1
az login
# After logging in, set the specific subscription for this session
az account set --subscription "Subscription 1 Name"



export AZURE_CONFIG_DIR=~/.azure_T2
mkdir -p ~/.azure_T2
az login
# After logging in, set the specific subscription for this session
az account set --subscription "Subscription 2 Name"




$ dotnet nuget update source github-packages --username developer --password pat_token --store-password-in-clear-text 




For Visual Studio Code, here's how to enable debugging into NuGet packages:
1. Add SourceLink to your projects (same as before)
In each .csproj you want to debug:
xml<PropertyGroup>
  <PublishRepositoryUrl>true</PublishRepositoryUrl>
  <EmbedUntrackedSources>true</EmbedUntrackedSources>
  <DebugType>embedded</DebugType>
  <IncludeSymbols>true</IncludeSymbols>
  <SymbolPackageFormat>snupkg</SymbolPackageFormat>
</PropertyGroup>

<ItemGroup>
  <PackageReference Include="Microsoft.SourceLink.GitHub" Version="8.0.0" PrivateAssets="All"/>
</ItemGroup>
2. Configure VS Code debugger settings
Add/update your .vscode/launch.json:
json{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/bin/Debug/net8.0/YourApp.dll",
      "args": [],
      "cwd": "${workspaceFolder}",
      "stopAtEntry": false,
      "console": "internalConsole",
      "justMyCode": false,
      "enableStepFiltering": false,
      "suppressJITOptimizations": true,
      "symbolOptions": {
        "searchPaths": [],
        "searchMicrosoftSymbolServer": true,
        "searchNuGetOrgSymbolServer": true
      },
      "sourceFileMap": {
        "/builds/": "${workspaceFolder}/"
      }
    }
  ]
}
Key settings:

"justMyCode": false - Allows stepping into external code
"enableStepFiltering": false - Disables step filtering
"suppressJITOptimizations": true - Makes debugging easier
"searchNuGetOrgSymbolServer": true - Searches NuGet symbol server

3. For GitHub Packages (no public symbol server):
Since GitHub Packages doesn't have a symbol server, use embedded symbols:
xml<PropertyGroup>
  <DebugType>embedded</DebugType>
  <EmbedAllSources>true</EmbedAllSources>
  <PublishRepositoryUrl>true</PublishRepositoryUrl>
</PropertyGroup>

<ItemGroup>
  <PackageReference Include="Microsoft.SourceLink.GitHub" Version="8.0.0" PrivateAssets="All"/>
</ItemGroup>
4. Alternative: Add local symbol path
If you have the .snupkg files locally, add them to your launch.json:
json"symbolOptions": {
  "searchPaths": [
    "/path/to/your/packages/folder"
  ],
  "searchMicrosoftSymbolServer": true
}
5. Verify C# extension settings
In VS Code settings (Ctrl+,), search for:

"Omnisharp: Enable Decompilation Support" - Enable this
This allows you to see decompiled source if symbols aren't available

The easiest approach: Use embedded symbols with SourceLink. This makes the symbols and source links part of the package itself, so debugging works automatically without needing symbol servers.


Step 2: Configure Visual Studio Debugger

Tools → Options → Debugging → General
Uncheck these:

☐ Enable Just My Code
☐ Enable .NET Framework source stepping


Check these:

☑ Enable Source Link support
☑ Enable source server support
☑ Suppress JIT optimization on module load (Managed only)


Tools → Options → Debugging → Symbols
Add symbol locations:

☑ Microsoft Symbol Servers
☑ NuGet.org Symbol Server
Add custom location if needed: https://nuget.smbsrc.net/