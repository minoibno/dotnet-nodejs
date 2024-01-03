# dotnet-nodejs

`dotnet-nodejs` is a convenient dotnet tool that enables easy installation of Node.js through the dotnet CLI. Once installed and initialized, you can
seamlessly use `node`, `npm`, and `npx` commands on your machine.

## Installation

### Current User

Execute the following commands to install `dotnet-nodejs` for your user.
```sh
dotnet tool install -g dotnet-nodejs
dotnet nodejs init
```

### All Users

Execute the following commands to install `dotnet-nodejs` globally for all user.
```sh
dotnet tool install --tool-path $TOOL_PATH dotnet-nodejs
$TOOL_PATH/dotnet-nodejs init -t Machine
```
