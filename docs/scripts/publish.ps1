dotnet publish $PSScriptRoot\..\..\src\MinionBot.Streamers\MinionBot.Streamers.csproj `
    -c Release `
    -o $PSScriptRoot\..\..\src\MinionBot.Streamers\bin\Release\net6.0\win-x64\publish `
    -r win-x64 `
    -p:PublishReadyToRun=true `
    -p:PublishSingleFile=true

dotnet publish $PSScriptRoot\..\..\src\MinionBot.Streamers\MinionBot.Streamers.csproj `
    -c Release `
    -o $PSScriptRoot\..\..\src\MinionBot.Streamers\bin\Release\net6.0\win-x86\publish `
    -r win-x86 `
    -p:PublishReadyToRun=true `
    -p:PublishSingleFile=true

dotnet publish $PSScriptRoot\..\..\src\MinionBot.Streamers\MinionBot.Streamers.csproj `
    -c Release `
    -o $PSScriptRoot\..\..\src\MinionBot.Streamers\bin\Release\net6.0\linux-x64\publish `
    -r linux-x64 `
    -p:PublishSingleFile=true

dotnet publish $PSScriptRoot\..\..\src\MinionBot.Streamers\MinionBot.Streamers.csproj `
    -c Release `
    -o $PSScriptRoot\..\..\src\MinionBot.Streamers\bin\Release\net6.0\osx-x64\publish `
    -r osx-x64 `
    -p:PublishSingleFile=true    