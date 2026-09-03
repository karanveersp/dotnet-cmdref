module Prompts

open Sharprompt

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
