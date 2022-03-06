# Revolt.Net
cd ../Revolt.Net
dotnet build
cd ../docs
docfx metadata
dfmg

# Revolt.Net.Commands
cd ../Revolt.Net.Commands
dotnet build
cd ../docs
docfx metadata ./docfx-commands.json
DFMG_CONFIG="./config-commands.yaml" dfmg
