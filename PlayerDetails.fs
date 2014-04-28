

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
  let fileName = sprintf "data/players/%s.html" (licNo.Replace(".", "-"))
  File.WriteAllText(fileName, html)

let downloadAll licNoFile =
  let list = File.ReadAllLines(licNoFile)
  list |> Array.iter downloadDetails





(* HTML processing *)
let rootEl = htmlDoc.DocumentNode
let persP = rootEl.SelectSingleNode ("//table[1]//td[1]//table//tr[2]//p")
let clubsP = rootEl.SelectSingleNode ("//table[1]//td[1]//table//tr[4]//p")
let keysP = rootEl.SelectSingleNode ("//table[1]//td[2]//table//tr[2]/td[1]/p")
let valuesP = rootEl.SelectSingleNode ("//table[1]//td[2]//table//tr[2]/td[1]/p")

let matchResultTRs = rootEl.SelectNodes("//table[@class='listing']//tr[@class='tableRowWhite' or @class='tableRowGrey']")
let rowsOfCells =
  let cast = Seq.cast<HtmlNode>
  let getText (n: HtmlNode) = n.InnerText.Trim()
  matchResultTRs
  |> cast
  |> Seq.map (fun r -> (r.SelectNodes("td") |> cast |> Seq.map getText))

res.ChildNodes
|> Seq.cast<HtmlNode>
|> Seq.filter (fun n -> n.NodeType = HtmlNodeType.Text)
|> Seq.iter (fun n -> printfn "%s" <| n.InnerText.Trim ())

