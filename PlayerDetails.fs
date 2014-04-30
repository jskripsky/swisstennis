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

let downloadDetails licNo =
  let html = client.DownloadString(detailUrl licNo) |> reduceHtml
  let fileName = sprintf "%s/%s.html" downloadsDir (licNo.Replace(".", "-"))
  File.WriteAllText(fileName, html)

let downloadAll licNoFile =
  let list = File.ReadAllLines(licNoFile)
  list |> Array.iter downloadDetails



(* HTML processing *)
let extractDetails (htmlDoc: HtmlDocument) =
  let rootEl = htmlDoc.DocumentNode
  let persP = rootEl.SelectSingleNode ("//table[1]//td[1]//table//tr[2]//p")
  let clubsP = rootEl.SelectSingleNode ("//table[1]//td[1]//table//tr[4]//p")
  let keysP = rootEl.SelectSingleNode ("//table[1]//td[2]//table//tr[2]/td[1]/p")
  let valuesP = rootEl.SelectSingleNode ("//table[1]//td[2]//table//tr[2]/td[2]/p")

  let innerText (n: HtmlNode) = n.InnerText.Trim()
  let extractLines (n: HtmlNode) =
    n.ChildNodes
    |> Seq.cast<HtmlNode>
    |> Seq.filter (fun n -> n.NodeType = HtmlNodeType.Text)
    |> Seq.map innerText

  let parseKeyValueLine line =
    let split (s: String) = s.Split(':')
    let trim (s: String) = s.Trim()
    let toTuple = function
      | k::v::_ -> (k, v)
      | _ as list -> failwith (sprintf "Invalid '[key; value]' list: %A." list)
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

  // Note: We assume that the cells contain only text (i.e. no elements).
  let extractCellTexts (rows: seq<HtmlNode>) =
    let extractCells (r: HtmlNode) =
      let tds = r.SelectNodes("td")
      if tds <> null then
        tds
        |> Seq.cast<HtmlNode>
        |> Seq.map innerText
      else
        Seq.empty
    rows
    |> Seq.map extractCells

  let pers =
    persP
    |> extractLines
    |> Seq.map parseKeyValueLine
  let clubs =
    clubsP
    |> extractLines
    |> Seq.filter ((<>) String.Empty)
  let classificationData =
    Seq.zip (extractLines keysP) (extractLines valuesP)

  (pers, clubs, classificationData)

let keys = [
  "Aktuelle Klassierung"  // Classification (e.g. "R1", "N4 (100)",..)
  "Aktueller Klassierungswert"  // value
  "Aktueller Wettkampfwert"  // value
  "Aktueller Risikozuschlag"  // value
  "Ranglistennummer"  // int
  "Anzahl Spiele" // int
  "Anzahl w.o."  // int
  "Abzug w.o."  // DU
  "Alterskategorie"  // DU
  "Status Lizenz"  // DU
  "Status IC"  // DU
  "Status JIC" // DU, optional
  "Letzte Klassierung"]  // Classification

let duKeys = [
  "Abzug w.o."
  "Alterskategorie"
  "Status Lizenz"
  "Status IC"
  "Status JIC"]
