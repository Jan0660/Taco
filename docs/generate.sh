#!/bin/sh

# Revolt.Net
cd ../Revolt.Net
dotnet build
cd ../docs
docfx metadata
dfmg

# Revolt.Net.Commands
# C:\Users\Jan\RiderProjects\DocFxMarkdownGen\bin\Release\net6.0\publish\
cd ../Revolt.Net.Commands
dotnet build
cd ../docs
docfx metadata ./docfx-commands.json
DFMG_CONFIG="./config-commands.yaml" dfmg
