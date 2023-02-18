
open SymbolExtrator
open FSharp.SystemCommandLine
open System.IO

let findUnicodeChars (htmlFile: FileInfo) = 

    let unicodeChars=extractSymbols htmlFile.FullName
    eprintfn "found %A unicode chars" unicodeChars.Length

    let chars=unicodeChars|>List.map (fun x->sprintf "U+%04X" (x.Chars 0|>int))|>String.concat ","

    printf "%s" chars

        
[<EntryPoint>]
let main argv = 
    let htmlFile = Input.Argument<FileInfo>("The html file to search for charactors")    

    rootCommand argv {
        description "Searches a html file and it's imported css for the unicode glyphs it uses"
        inputs (htmlFile)
        setHandler findUnicodeChars
    }