Compress-Archive $PSScriptRoot\..\..\src\MinionBot.Streamers\bin\Release\net6.0\linux-x64\publish\* `
    -DestinationPath $PSScriptRoot\..\..\linux-x64.zip -Force 

Compress-Archive $PSScriptRoot\..\..\src\MinionBot.Streamers\bin\Release\net6.0\osx-x64\publish\* `
    -DestinationPath $PSScriptRoot\..\..\osx-x64.zip -Force 

Compress-Archive $PSScriptRoot\..\..\src\MinionBot.Streamers\bin\Release\net6.0\win-x64\publish\* `
    -DestinationPath $PSScriptRoot\..\..\win-x64.zip -Force 

Compress-Archive $PSScriptRoot\..\..\src\MinionBot.Streamers\bin\Release\net6.0\win-x86\publish\* `
    -DestinationPath $PSScriptRoot\..\..\win-x86.zip -Force 