#r "HtmlAgilityPack.dll"
#r "SwissTennis.Scraping.dll"

open System
open System.Text
open System.Text.RegularExpressions
open System.IO
open System.Net
open HtmlAgilityPack
open SwissTennis.Model

// Search form: http://www.swisstennis.ch/?rub=24&id=102509

let url = "http://www.swisstennis.ch/?rub=24&id=105057&abfrage=3"
// POST data:
//  AbfrageArt=Region&Vereinigung=All&
//  Geschlecht2=%2F%3Frub%3D24%26id%3D102509%26Geschlecht2%3D1&
//  Jahrgang=All&AltersKlasse=All&KlassierungsKategorie=N1
let client = new WebClient()

// "Vereinigungen"
let regions = [
  201; 210; 205; 206; 301
  302; 101; 103; 108; 204
  207; 211; 401; 203; 104
  212; 305; 304; 209; 105
  111]

let maxResultsPerPage = 500
let birthYearRange = seq { 1910..2010 }

/// region = 0 => all regions
let fetchHtml (classification: string) (birthYear: int) (region: int)=
  printf "Birth Year %A, Region %A: " birthYear region
  let nvc = NameValueCollection()
  let toString x = if x > 0 then (string x) else "All"
  let postData =
    ["AbfrageArt", "Region"
     "AltersKlasse", "All"
     "Geschlecht2", "/?rub=24&id=102509&Geschlecht2=1"
     "Jahrgang", (birthYear |> toString)
     "KlassierungsKategorie", classification
     "Vereinigung", (region |> toString)
    ]
  postData |> List.iter (fun (n, v) -> nvc.Add(n, v))
  let response = client.UploadValues(url, nvc)
  Encoding.Default.GetString(response)

let extractPlayers html =
  let parsePlayer (row: HtmlNode) =
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
    rows |> Seq.map parsePlayer
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
  let fetchAll year =
    let fetch region = fetchHtml classification year region |> extractPlayers
    // Try fetching all at once
    let all = fetch 0
    // Fetching each region separately an delayed
    let each () = regions |> Seq.map fetch |> Seq.concat
    // If we reached the items-per-page limit, fetch regions separately
    if Seq.length all < maxResultsPerPage then all else each ()

  let all = fetchAll 0
  let each () = birthYearRange |> Seq.map fetchAll |> Seq.concat
  let list = if Seq.length all < maxResultsPerPage then all else each ()

  list
  |> Seq.sortBy (fun p -> p.Rank)
  |> writeToFile (sprintf "data/lists/%s.txt" classification)

let downloadAll () =
  let toString classification = (sprintf "%A" classification).Replace (" ", String.Empty)
  allClassifications |> List.iter (fun x -> printf "%s" (toString x); download (toString x))

