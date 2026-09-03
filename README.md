# dotnet-cmdref

A dotnet tool to store your commands for future reference.
Use it to
* recall long commands you tend to forget
* build a reference library of commands

## Install

The command to install the tool can be found on the [NuGet page](https://www.nuget.org/packages/dotnet-cmdref/).

The tool can be invoked with `cmdref`.

--- 

## Usage

The tool displays a list of actions.

```
Create
View
Update
Delete
Show storage file path
Exit
```

When creating a command, the user will be prompted for the following:
1. `Command Name` - A short but descriptive name of the command
2. `Platform` - Which platform the command is related to, such as _dotnet cli_, _docker_, or _windows_. Commands of the same platform will be listed together in alphabetical order when selecting which one to view/update/delete. Use the same platform in terms of to case/symbols for neat organization of commands.
3. `Description` - A longer description explaining the command.
4. `Command` - The command itself. Multiline commands currently not supported but contributions are welcome.

When updating a command, existing values are pre-filled so you can just press Enter to keep them, or edit only what changed.

When viewing a command, it is displayed in a bordered panel, and you're offered to copy it straight to your clipboard.

All commands are stored in a `.json` file in a local app config directory named `cmdref`.

This file can be backed up or copied to other machines to access your saved commands. It is intentionally kept in a pretty-printed json format so it can also be edited manually by a user.

