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

let url = "http://www.swisstennis.ch/?rub=24&id=105057&abfrage=3"
let client = new WebClient()

let fetchHtml (birthYear: int) (classification: string) =
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
	let htmlDoc = HtmlDocument()
	htmlDoc.LoadHtml(html)
	let rows = htmlDoc.DocumentNode.SelectNodes "//table[@class='stTable']/tr"

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

	rows |> Seq.map toPlayer

let writeToFile (filename: string) (players: seq<Player>) =
	let wrap (el: string) (text: string) =
		sprintf "<%s>%s</%s>" el text el

	let toTableRow (p: Player) =
		wrap "tr" ((p.LicNo |> wrap "td") + (p.Rank |> string |> wrap "td") + (p.Name |> wrap "td") + (p.State |> string |> wrap "td"))

	let result = wrap "html" (wrap "body" (wrap "table" (players |> Seq.map toTableRow |> String.concat "\n")))
	File.WriteAllText (filename, result)

let allR9 = [1950..2008] |> List.map (fun year -> printfn "%A" year; fetchHtml year "R9" |> readPlayers) |> Seq.concat |> Seq.sortBy (fun p -> p.Rank)
writeToFile "/tmp/result.html" allR9
