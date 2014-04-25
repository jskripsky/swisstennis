#r "HtmlAgilityPack.dll"

open System
open System.Collections.Specialized
open System.Text
open System.Text.RegularExpressions
open System.IO
open System.Net
open HtmlAgilityPack

type Player = {
  LicNo: string
  Name: string
  Classification: string
  Rank: int
  ClassificationValue: float
  AgeCategory: int
  State: char
}

// Search form: http://www.swisstennis.ch/?rub=24&id=102509
// Vereinigung: All, 201, 210, 205, 206, 301, 302, 101, 103, 108, 204, 207, 211, 401, 203, 104, 212, 305, 304, 209, 105, 111

let url = "http://www.swisstennis.ch/?rub=24&id=105057&abfrage=3"
let client = new WebClient()

let fetchHtml (birthYear: int) (classification: string) =
  printf "Birth Year %A: " birthYear
  let nvc = NameValueCollection()
  let postData =
    ["AbfrageArt", "Region" 
     "AltersKlasse", "All"
     "Geschlecht2", "/?rub=24&id=102509&Geschlecht2=1"
     "Jahrgang", (birthYear |> string)
     "KlassierungsKategorie", classification
     "Vereinigung", "All"
    ]
  postData |> List.iter (fun (n, v) -> nvc.Add(n, v))
  let response = client.UploadValues(url, nvc)
  Encoding.Default.GetString(response)

let readPlayers html =
  let toPlayer (row: HtmlNode) =
    let cells = row.SelectNodes("td") |> Seq.toList
    let text idx = cells.[idx].InnerText.Trim ([|'\n'; '\r'; '\t'|])

    let classificationRegex = Regex(@"([RN]\d)\s*\((\d+)\)", RegexOptions.Singleline)
    let cleanClassification = (text 2).Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ').Replace(" ", "")
    let clMatch = classificationRegex.Match (cleanClassification)

    try
      { LicNo = text 0
        Name = text 1
        Classification = clMatch.Groups.[1].Value
        Rank = clMatch.Groups.[2].Value |> int
        ClassificationValue = (text 3) |> float
        AgeCategory = 0 //(text 4).TrimEnd ([|'+'|]) |> int
        State = (text 5).[0]}
    with
    | _ ->
      printfn "%A: %A, %A" (text 1) cleanClassification (text 4)
      { LicNo = text 0
        Name = text 1
        Classification = clMatch.Groups.[1].Value
        Rank = -1
        ClassificationValue = (text 3) |> float
        AgeCategory = 0
        State = (text 5).[0]}


  let htmlDoc = HtmlDocument()
  htmlDoc.LoadHtml(html)
  let rows = htmlDoc.DocumentNode.SelectNodes ("//table[@class='stTable']/tr")

  if rows <> null then
    printfn "%d rows." (Seq.length rows)
    rows |> Seq.map toPlayer
  else
    printfn "0 rows."
    Seq.empty

let writeToFile (filename: string) (players: seq<Player>) =
  let list =
    players
    |> Seq.map (fun p -> p.LicNo)
    |> String.concat "\n"
  File.WriteAllText (filename, list)

let download (classification: string) =
  let all =
    [1910..2010]
    |> List.map (fun year -> fetchHtml year classification |> readPlayers)
    |> Seq.concat
    |> Seq.sortBy (fun p -> p.Rank)
  writeToFile (classification + ".txt") all

download "R9"

