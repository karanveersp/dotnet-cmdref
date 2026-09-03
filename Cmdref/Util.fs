module Util

open System.IO
open System
open System.Text.Json
open System.Text.Json.Serialization
open Spectre.Console

open Model
open Prompts

/// Gets the path to the user's application data folder
/// which can be used to store application artifacts.
let AppDataDir =
    Environment.GetFolderPath(
        Environment.SpecialFolder.LocalApplicationData,
        Environment.SpecialFolderOption.DoNotVerify
    )

/// Gets the path of the file used to store commands
let CommandsFilePath (appName: string) (fname: string) =
    Path.Combine(
        List.toArray [ AppDataDir
                       appName
                       fname ]
    )

let CreateDirectoryIfNotExist (dirpath: string) =
    if not (Directory.Exists dirpath) then
        Directory.CreateDirectory(dirpath) |> ignore

let private jsonOptions =
    JsonSerializerOptions(WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase)

/// Converts json string to list of commands.
let JsonToCommands (content: string) : Command list =
    match content with
    | "" -> list.Empty
    | _ -> JsonSerializer.Deserialize<Command[]>(content, jsonOptions) |> List.ofArray


/// Parses commands from the result of the provider into a map
/// where command name is the key.
let ParseCommands (cmdsProvider: unit -> string) : Map<string, Command> =
    cmdsProvider ()
    |> JsonToCommands
    |> List.map (fun cmd -> (cmd.Name, cmd))
    |> Map.ofList

/// Reads the file contents and returns them.
/// Returns empty string if file does not exist.
let ReadFileText (fpath: string) : string =
    if (File.Exists(fpath)) then
        File.ReadAllText(fpath)
    else
        ""

/// Converts command list into indented json string
let CommandsToJson (commands: Command List) =
    JsonSerializer.Serialize(commands |> Array.ofList, jsonOptions)


/// Writes list of commands into the file as json
let WriteCommands (cmdsFilePath: string) (commandsMap: Map<string, Command>) =
    let json =
        commandsMap.Values
        |> Seq.cast
        |> List.ofSeq
        |> CommandsToJson

    File.WriteAllText(cmdsFilePath, json)

/// Commands sorted by platform then name, so entries of the same platform are grouped together.
let SortedCommands (cmdMap: Map<string, Command>) : Command seq =
    cmdMap.Values |> Seq.sortBy (fun cmd -> (cmd.Platform, cmd.Name))

/// Renders a command's display text for selection prompts, e.g. "dotnet cli - run tests".
let DisplayText (cmd: Command) : string = sprintf "%s - %s" cmd.Platform cmd.Name

/// Lets the user pick a command from the map and returns the selected entry directly,
/// so no risky re-parsing of the display text is needed to recover the command name.
let SelectCommand (message: string) (cmdMap: Map<string, Command>) : Command =
    SelectionPromptOf message (SortedCommands cmdMap) DisplayText

let CreateCmdWithName (name: string) (existing: Command option) : Command =
    let command, platform, description =
        match existing with
        | Some e -> TextPromptWithDefault "Command" e.Command, TextPromptWithDefault "Platform" e.Platform, TextPromptWithDefault "Description" e.Description
        | None -> RequiredTextPrompt "Command", RequiredTextPrompt "Platform", RequiredTextPrompt "Description"

    { Name = name
      Command = command
      Platform = platform
      Description = description }

let CreateCmd () =
    let name = RequiredTextPrompt "Command name"
    CreateCmdWithName name None


let CreateHandler (cmdFilePath: string) (cmdMap: Map<string, Command>) =
    let cmd = CreateCmd()
    let newMap = Map.add cmd.Name cmd cmdMap
    WriteCommands cmdFilePath newMap
    AnsiConsole.MarkupLine($"[green]Saved '{Markup.Escape(cmd.Name)}'.[/]")
    newMap

/// Renders a command in a bordered panel, with the command text highlighted
/// so it's easy to spot and copy when recalling it.
let PrintCommand (cmd: Command) : unit =
    let body =
        $"[bold]Description:[/] {Markup.Escape(cmd.Description)}\n\n[bold]Command:[/]\n[yellow]{Markup.Escape(cmd.Command)}[/]"

    let panel = Panel(body)
    panel.Header <- PanelHeader($"{Markup.Escape(cmd.Platform)} · {Markup.Escape(cmd.Name)}")
    panel.Border <- BoxBorder.Rounded
    AnsiConsole.WriteLine()
    AnsiConsole.Write(panel)

/// Offers to copy the command text to the clipboard. Clipboard access can fail
/// in some environments (e.g. missing xclip/xsel on Linux), so failures are reported
/// without crashing the app.
let OfferClipboardCopy (cmd: Command) : unit =
    if ConfirmPrompt "Copy command to clipboard?" true then
        try
            TextCopy.ClipboardService.SetText(cmd.Command)
            AnsiConsole.MarkupLine("[green]Copied to clipboard.[/]")
        with ex ->
            AnsiConsole.MarkupLine($"[red]Could not copy to clipboard: {Markup.Escape(ex.Message)}[/]")

let ViewHandler (itemsMap: Map<string, Command>) =
    if (itemsMap.IsEmpty) then
        printfn $"No existing commands found."
    else
        let cmd = SelectCommand "Select a command" itemsMap
        PrintCommand cmd
        OfferClipboardCopy cmd

    itemsMap


let GetUserAction () : Action =
    let action =
        SelectionPrompt
            "Select action"
            [| "Create"
               "View"
               "Update"
               "Delete"
               "Show storage file path"
               "Exit" |]

    ActionFromString action

let UpdateHandler (cmdsFilePath: string) (itemsMap: Map<string, Command>) =
    let cmd = SelectCommand "Select command to update" itemsMap
    PrintCommand cmd

    let updated = CreateCmdWithName cmd.Name (Some cmd)
    let newMap = Map.add cmd.Name updated itemsMap
    WriteCommands cmdsFilePath newMap
    AnsiConsole.MarkupLine($"[green]Updated '{Markup.Escape(cmd.Name)}'.[/]")
    newMap


let DeleteHandler (cmdsFilePath: string) (itemsMap: Map<string, Command>) =
    if (itemsMap.IsEmpty) then
        printfn "No commands to delete."
        itemsMap
    else
        let cmd = SelectCommand "Select command to delete" itemsMap
        PrintCommand cmd

        let confirm =
            ConfirmPrompt $"Are you sure you want to delete entry ({cmd.Name})?" false

        if confirm then
            let newMap = Map.remove cmd.Name itemsMap
            WriteCommands cmdsFilePath newMap
            AnsiConsole.MarkupLine($"[green]Deleted '{Markup.Escape(cmd.Name)}'.[/]")
            newMap
        else
            itemsMap

let DisplayPathHandler (cmdsFilePath: string) =
    printfn "Command store file: %s" cmdsFilePath

let ProcessAction (cmdsFilePath: string) (itemsMap: Map<string, Command>) (action: Action) =
    match action with
    | Create -> CreateHandler cmdsFilePath itemsMap
    | View -> ViewHandler itemsMap
    | Update -> UpdateHandler cmdsFilePath itemsMap
    | Delete -> DeleteHandler cmdsFilePath itemsMap
    | DisplayPath ->
        DisplayPathHandler cmdsFilePath
        itemsMap
    | Exit -> itemsMap
