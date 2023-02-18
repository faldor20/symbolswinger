
module SymbolExtrator
open HtmlAgilityPack

module private Intern=
    open System
    open ExCSS
    open System.IO
    open System.Text.RegularExpressions
    open System.Collections.Generic
    open System.Net
    ///Finds css paths within links in the head of the html document and returns their absolute path
    let getCssFilePaths (doc: HtmlDocument) htmlFileDirectory =
        let head = doc.DocumentNode.SelectSingleNode("//head")
        let cssLinks = head.Descendants("link") |> Seq.filter (fun el -> 
            el.Attributes.["rel"].Value.ToLower() = "stylesheet" &&
            el.Attributes.["href"].Value.EndsWith(".css"))
        let cssPaths = cssLinks |> Seq.map (fun el -> el.Attributes.["href"].Value)

        cssPaths 
        //Make path relative to html file
        |>Seq.map(fun x ->
        if x.StartsWith "http" then
        
            x
        else    
            Path.Join(htmlFileDirectory,x)) 
        |>Seq.toList

    ///Parses a css file with Excss
    let parseCss files=
        use client=new Http.HttpClient()
        files|>List.map( fun (file:string)->
            let parser=ExCSS.StylesheetParser()
            let content=
                if file.StartsWith("http://")||file.StartsWith("https://") then
                    client.GetStringAsync(file).Result
                else File.ReadAllText(file);
            let parsed=parser.Parse(content);
            parsed
        )

    ///Uses regex to extract the class name from a css selector
    let getClassNameFromSelector (selector: string) =
        let regex = new Regex(@"\.([\w-]+)")
        let matches = regex.Match(selector)
        if matches.Success then
            Some matches.Groups.[1].Value
        else
            None

    ///Generates a Dictionary that maps between a css classname and the Content field of that css class.
    let makeCssClassDictionary (cssStyles: Stylesheet list) =
        let dic=new Dictionary<string,string>()
        cssStyles|>List.iter(fun x->
            
            x.StyleRules
            |>Seq.choose(fun x->
                match x.Style.Content|>String.IsNullOrEmpty with
                |true->  None
                |false->
                    x.SelectorText
                    |>getClassNameFromSelector
                    |>Option.map(fun className->className ,x.Style.Content )
            )
            |>Seq.distinctBy(fun (name,_)->name)
            |>Seq.iter (fun (name,content)->
                if dic.TryAdd(name,content)|>not then
                    eprintfn "css class %A Failed to be added to map. This is likely because of a duplicate " (name,content)
                )
        )
        dic
    /// Looks up the className and returns it's content field if it exists
    /// *cssdict*: mapping between css className and it's content field
    let findCssClassContent (cssDict:Dictionary<string,string>) (className:string)=
        match cssDict.TryGetValue(className) with
        |(false,_)->None
        |(true,content)->
            if content|>String.IsNullOrEmpty then
                eprintfn $"Couldn't find an entry for class: {className}. This means either something has gone wrong, or that class has no content field in it's css"
                None
            //Remove the quotes from the content 
            else Some (content.Trim('"'))

    ///Gets a list of all css classes used within the document
    let locateClasses (doc:HtmlDocument)=
        doc.DocumentNode.Descendants()  
        |>Seq.collect( fun x->x.GetClasses())

    let findHexUnicodeEntities (doc: HtmlDocument) =
        let regex = new Regex(@"&#x([0-9a-fA-F]+);")
        let mutable entitiesStrings = []
        doc.DocumentNode.Descendants() |>Seq.collect(fun node->
            let matches = regex.Matches(node.InnerText)
            matches|>Seq.map(fun rgMatch->rgMatch.Groups[1].Value)
        )
        |>Seq.map(fun x->Int32.Parse(x,Globalization.NumberStyles.HexNumber)|>Char.ConvertFromUtf32)

open Intern
open System.IO


let extractSymbols (htmlPath:string)=
    let timer= System.Diagnostics.Stopwatch()
    timer.Start()
    let timerUpdate message=
        timer.Stop()
        eprintfn " %s : %A ms" message timer.Elapsed.TotalMilliseconds
        timer.Reset()
        timer.Start()


    let doc=HtmlDocument()
    doc.Load(htmlPath)

    let cssFiles =getCssFilePaths doc (Path.GetDirectoryName(htmlPath))
    eprintfn "cssFiles: %A" cssFiles
    timerUpdate "getting css File Paths: "
    let styleSheets=parseCss cssFiles 
    timerUpdate "parsing css:"
    let classMap=makeCssClassDictionary(styleSheets)
    timerUpdate" making class map "

    let classes=locateClasses doc

    let unicodeHtmlEntity= findHexUnicodeEntities doc 
    let contents=
        classes
        |>Seq.choose( findCssClassContent classMap)
        
    let out=unicodeHtmlEntity|> Seq.append contents|>Seq.distinct|>Seq.toList 
    timerUpdate "process took"
    out