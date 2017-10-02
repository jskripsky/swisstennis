#r "FSharp.Data"

module SwissTennis.Download

open System
open FSharp.Data

let loadResults userId =
        let curCompValueCss = ".profile .field-name-field-current-competition-value .field-item"
        let matchResultRowsCss = "#quicktabs-tabpage-results_summary-1 table > tbody > tr"
        let codeIdx, setsIdx, valIdx = 0, 1, 3

        let url = sprintf "https://www.swisstennis.ch/user/%d/results-summary" userId
        let doc = HtmlDocument.Load(url)

        let div = doc.CssSelect(curCompValueCss) |> Seq.head
        let curCompetitionValue = float (div.InnerText())

        let matchResults =
          let rows = doc.CssSelect(matchResultRowsCss)
          let processRow (row: HtmlNode) =
                  let cells = row.Elements("td") |> List.rev
                  let getText idx = cells.[idx].InnerText().Trim()
                  if getText setsIdx <> String.Empty then
                          Some (getText codeIdx, getText valIdx |> float)
                  else
                          None

          let isWin (code, _) = (code = "S" || code = "W")
          let getValues (x, y) = (x |> List.map snd, y |> List.map snd)

          rows
          |> List.choose processRow
          |> List.partition isWin
          |> getValues

        (curCompetitionValue, matchResults)

type MatchResult = {
  Code: string
  SetResults: string
  OpponentID: int
  OpponentValue: float
}

let users = [
  "Juraj", 88633
  "Patrick", 2733
  "Andreas", 104031
  "Martin", 68713
  "Martin Frei", 108476
  "Michel Gassmann", 13072
  "Claude Häberli", 33345] |> Map.ofList

let calc name =
  let cur, res = loadResults (users.[name])
  calcCWR cur res

calc "Claude Häberli"

