module Prompts

open Sharprompt
open Spectre.Console

/// Prompts the user for a y/n response with the given default.
let ConfirmPrompt (message: string) (defaultVal: bool) : bool = Prompt.Confirm(message, defaultVal)

/// Prompts the user for non-empty text input.
let RequiredTextPrompt (msg: string) : string =
    Prompt.Input<string>(
        msg,
        validators =
            [| Validators.Required()
               Validators.MinLength(1) |]
    )

/// Prompts the user for non-empty text input, pre-filled with an existing value
/// that is kept as-is if the user just presses Enter.
let TextPromptWithDefault (msg: string) (defaultVal: string) : string =
    Prompt.Input<string>(
        msg,
        defaultValue = defaultVal,
        validators =
            [| Validators.Required()
               Validators.MinLength(1) |]
    )

/// Prompts the user for a selection from the given choices.
let SelectionPrompt (message: string) (choices: seq<string>) : string = Prompt.Select(message, choices)

/// Prompts the user to select one of the given items, rendered via textSelector,
/// and returns the selected item itself (avoids re-parsing text back into data).
let SelectionPromptOf<'T> (message: string) (items: seq<'T>) (textSelector: 'T -> string) : 'T =
    Prompt.Select(message, items, textSelector = textSelector)

/// Reads multiline input until the user types ";;" on its own line.
let ReadMultilineInput (msg: string) : string =
    AnsiConsole.MarkupLine($"[bold]{Markup.Escape(msg)}[/] [dim](type ;; on a new line when done)[/]")
    let rec loop acc =
        let line = AnsiConsole.Prompt(TextPrompt<string>("> ").AllowEmpty())
        if line = ";;" then
            let result = String.concat "\n" (List.rev acc)
            if result.Trim() = "" then
                AnsiConsole.MarkupLine("[red]Input cannot be empty. Try again.[/]")
                loop []
            else result
        else
            loop (line :: acc)
    loop []

/// Shows the current value of a field, then offers to keep or replace it with new multiline input.
let MultilineInputWithDefault (msg: string) (existing: string) : string =
    AnsiConsole.MarkupLine($"\n[bold]Current {Markup.Escape(msg)}:[/]\n{Markup.Escape(existing)}\n")
    if Prompt.Confirm($"Keep existing {msg}?", true) then existing
    else ReadMultilineInput msg
