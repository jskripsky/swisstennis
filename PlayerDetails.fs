#r "HtmlAgilityPack.dll"
#r "SwissTennis.Scraping.dll"

open System
open System.Text
open System.Text.RegularExpressions
open System.IO
open System.Net
open HtmlAgilityPack
open OldAHVNumber
open SwissTennis.Model

let private downloadsDir = "downloads/players"

(* Detail scraper *)
let detailUrl licNo =
  "http://www.swisstennis.ch/custom/includes/wettkampf/klassierung_window/" +
  sprintf "?rub=47&show=detail&LizenzNr=%s&lang=D" licNo

let htmlCommentRegex = Regex(@"\<!--[\s\S]*?--\>")
let htmlHeadRegex = Regex(@"\<head\>[\s\S]*?\</head\>")
let simpleHead = "\n<head><meta http-equiv=\"content-type\" content=\"text/html; charset=UTF-8\" /></head>\n"
let emptyLineRegex = Regex(@"^\s+$[\r\n]*", RegexOptions.Multiline)
let replaceWith (rx: Regex) (s': string) s = rx.Replace(s, s')
let remove (rx: Regex) s = rx.Replace(s, String.Empty)
let reduceHtml html =
  html
  |> remove htmlCommentRegex
  |> replaceWith htmlHeadRegex simpleHead
  |> remove emptyLineRegex

let downloadDetails (wc: WebClient) licNo =
  let html = wc.DownloadString(detailUrl licNo) |> reduceHtml
  let fileName = sprintf "%s/%s.html" downloadsDir (licNo.Replace(".", "-"))
  File.WriteAllText(fileName, html)

let downloadAll (wc: WebClient) licNoFile =
  let list = File.ReadAllLines(licNoFile)
  list |> Array.iter (downloadDetails wc)



(* HTML processing *)
let extractDetails (htmlDoc: HtmlDocument) =
  let rootEl = htmlDoc.DocumentNode
  let persP = rootEl.SelectSingleNode ("//table[1]//td[1]//table//tr[2]//p")
  let clubsP = rootEl.SelectSingleNode ("//table[1]//td[1]//table//tr[4]//p")
  let keysP = rootEl.SelectSingleNode ("//table[1]//td[2]//table//tr[2]/td[1]/p")
  let valuesP = rootEl.SelectSingleNode ("//table[1]//td[2]//table//tr[2]/td[2]/p")

  let innerText (n: HtmlNode) = n.InnerText.Trim()
  let innerHtml (n: HtmlNode) = n.InnerHtml.Trim()
  let extractLines (n: HtmlNode) =
    n.ChildNodes
    |> Seq.cast<HtmlNode>
    |> Seq.filter (fun n -> n.NodeType = HtmlNodeType.Text)
    |> Seq.map innerHtml

  let parseKeyValueLine line =
    let split (s: String) = s.Split(':')
    let trim (s: String) = s.Trim()
    let toTuple = function
      | k::v::_ -> (k, v)
      | _ as list -> failwithf "Invalid '[key; value]' list: %A." list
    line
    |> split
    |> Array.map trim
    |> Array.toList
    |> toTuple

  (* Competition Results *)
  let matchResultRows =
    let trs = rootEl.SelectNodes("//table[@class='listing']//tr[@class='tableRowWhite' or @class='tableRowGrey']")
    if trs <> null then
      trs |> Seq.cast<HtmlNode>
    else
      Seq.empty

  let extractCellTexts (row: HtmlNode) =
    let tds = row.SelectNodes("td")
    if tds <> null then
      tds
      |> Seq.cast<HtmlNode>
      |> Seq.map innerHtml
    else
      Seq.empty

  let parseMatchResult (cells: string[]) =
    let (outcome, discardedLoss) =
      let (first, last) = (cells.[0], cells.[7])
      match (first, last) with
      | (_, "S") -> (Win, false)
      | ("W", _) -> (WinWO, false)
      | (c, "N") -> (Loss, c = "X")
      | ("Z", _) -> (LossWO, false)
      | _ -> failwithf "Invalid combination of first and last column: '%s', '%s'" first last
    let setResults =
      let parseSet s =
        let toTuple =
          function
          |  [|me; opponent|] -> (me, opponent)
          | _ -> failwithf "Invalid set result: %s" s
        s.Split [|':'|]
        |> Array.map int
        |> toTuple

      (cells.[6].Replace ("&nbsp;", " ")).Split [|' '|]
      |> Array.map parseSet
      |> Array.toList
      
    {
      IsDiscardedLoss = discardedLoss
      Date = DateTime.ParseExact (cells.[1], "dd.MM.yy", null)
      Tournament =
        { ID = -1; Type = Regular; Name = cells.[2] }  // FIXME
      OpponentName = cells.[3] // FIXME
      OpponentLicenseNo = "[FIXME]" // cells.[3]
      OpponentCompetitionValue = cells.[4] |> float
      OpponentNewClassification = cells.[5] |> Classification.Parse
      SetResults = setResults
      OutCome = outcome
    }    

  let matchResults =
    matchResultRows
    |> Seq.map (extractCellTexts >> Seq.toArray)
    |> Seq.map parseMatchResult
    |> Seq.toList

  let pers =
    persP
    |> extractLines
    |> Seq.map parseKeyValueLine
    |> dict

  let clubs =
    clubsP
    |> extractLines
    |> Seq.filter ((<>) String.Empty)
    |> Seq.toList

  let cl =
    Seq.zip (extractLines keysP) (extractLines valuesP)
    |> dict

  let v key =
    let (found, res) = cl.TryGetValue key
    if found then res else null

  {
    Name = pers.["Name"]
    FirstName = pers.["Vorname"]
    LicenseNo = pers.["Lizenz-Nr"]
    Clubs = clubs

    CurrentClassification = v "Aktuelle Klassierung" |> Classification.Parse
    CurrentClassificationValue = v "Aktueller Klassierungswert" |> float
    CurrentCompetitionValue = v "Aktueller Wettkampfwert" |> float
    CurrentRiskValue = v "Aktueller Risikozuschlag" |> float
    RankNo = v "Ranglistennummer" |> int
    MatchCount = v "Anzahl Spiele" |> int
    WOCount = v "Anzahl w.o." |> int
    WODeduction = v "Abzug w.o."
    AgeCategory = v "Alterskategorie"
    StatusLicense = v "Status Lizenz"
    StatusIC = v "Status IC"
    StatusJIC = v "Status JIC"
    LastClassification = v "Letzte Klassierung" |> Classification.Parse

    // take from MatchResults of opponents detail sheet
    //LastCompetitionsValue4L = ...

    MatchResults = matchResults
  }
