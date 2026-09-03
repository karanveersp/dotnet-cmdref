module Model

type Action =
    | View
    | Create
    | Update
    | Delete
    | DisplayPath
    | Exit

let ActionFromString (s: string) : Action =
    match s with
    | "Create" -> Create
    | "View" -> View
    | "Update" -> Update
    | "Delete" -> Delete
    | "Show storage file path" -> DisplayPath
    | "Exit" -> Exit
    | _ -> failwith $"{s} is not a valid action"

type Command =
    { Name: string
      Command: string
      Platform: string
      Description: string }
