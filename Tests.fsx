#r "HtmlAgilityPack.dll"
#r "Newtonsoft.Json.dll"
#r "SwissTennis.Scraping.dll"

open System
open System.IO
open Newtonsoft.Json
open HotFeet.OldAHVNumber
open SwissTennis.Model


let doc = HtmlDocument()

let allPlayers =
  Directory.GetFiles("downloads/players/", "*.html")
  |> Array.map (fun path ->
    printfn "%s" path
    doc.Load(path)
    extractDetails doc
  )

let serializer = JsonSerializer ()
use sw = new StreamWriter ("/tmp/test.json")
use writer = new JsonTextWriter (sw)
serializer.Serialize(writer, players)

//doc.Load("downloads/players/790-74-165-0.html")
//extractDetails doc


(*
// Juraj, Martin, Jan, Paris (4.11.71):
parseBirthDate "790.76.127.0"
parseBirthDate "790.74.165.0"
parseBirthDate  "264.76.159.0"
parseBirthDate  "278.71.435.0"

// 29.2.80, 1.3.80:
parseBirthDate "000.80.160.0"
parseBirthDate "000.80.163.0"

// 1.1.50:
parseBirthDate "000.50.101.0"
parseBirthDate "000.50.501.0"

// 31.12.50:
parseBirthDate "000.50.493.0"
parseBirthDate "000.50.893.0"

// 10.1.2001:
parseBirthDate "722.01.110.0"
// 16.4.1911:
parseBirthDate "117.11.216.0"
*)
