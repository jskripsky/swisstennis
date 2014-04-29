module OldAHVNumber

// For more info see:
// http://www.ahvnummer.ch/aufbau-alt.htm
// http://www.ahvnummer.ch/ahv-kalender.htm

open System

// AVH No Components:
// Name * BirthYear * SexAndBirthDay * CheckSum
type AHVNo = int * int * int * int
type Sex = Male | Female

let parseAHVNo (s: string) =
  match (s.Split ('.') |> Array.map int) with
  | [|a; b; c; d|] -> (a, b, c, d)
  | _ -> failwith (sprintf "Invalid AHV number: %s" s)

let getSex ((_, _, sexAndBirthDay, _): AHVNo) =
  match sexAndBirthDay with
  | _ when sexAndBirthDay < 500 -> Male
  | _ -> Female

// 'monthStarts' are taken from http://www.ahvnummer.ch/ahv-kalender.htm,
// the last number was added as sentinel.
let getBirthDate ((_, birthYear, sexAndBirthDay, _): AHVNo as n) =
  let dayInYear = (sexAndBirthDay - 100) % 400
  if dayInYear = 0 then failwith (sprintf "Birthdate unknown: %A" n)
  let monthStarts = [1; 32; 63; 101; 132; 163; 201; 232; 263; 301; 332; 363; 401]
  let month = monthStarts |> List.findIndex (fun x -> dayInYear < x)
  let day = dayInYear - monthStarts.[month - 1] + 1
  let year = birthYear + (if birthYear >= 10 then 1900 else 2000)
  DateTime(year, month, day)

let parseAndExtractAHVData (s: string) =
  let no = parseAHVNo s
  (no |> getSex, no |> getBirthDate)

