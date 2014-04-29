module HotFeet.OldAHVNumber

// For more info see:
// http://www.ahvnummer.ch/aufbau-alt.htm
// http://www.ahvnummer.ch/ahv-kalender.htm

// AVH No Components:
// Name * BirthYear * SexAndBirthDay * CheckSum
type AHVNo = int * int * int * int
type Sex = Male | Female

let parseAHVNo (s: string) =
  match (s.Split ('.') |> Array.map int) with
  | [|a; b; c; d|] -> (a, b, c, d)
  | _ -> failwith (sprintf "Invalid AHV number: %s" s)

let getSex ((_, _, sexAndBirthDay, _): OldAHVNo) =
  match sexAndBirthDay with
  | _ when sexAndBirthDay < 500 -> Male
  | _ -> Female

let getBirthDate ((_, birthYear, sexAndBirthDay, _): OldAHVNo as n) =
  let dayInYear = (sexAndBirthDay - 100) % 400
  if dayInYear = 0 then failwith (sprintf "Birthdate unknown: %A" n)
  // see http://www.ahvnummer.ch/ahv-kalender.htm
  let monthStarts = [1; 32; 63; 101; 132; 163; 201; 232; 263; 301; 332; 363; 401]
  let month = monthStarts |> List.findIndex (fun x -> dayInYear < x)
  let day = dayInYear - monthStarts.[month - 1] + 1
  let year = birthYear + (if birthYear >= 10 then 1900 else 2000)
  DateTime(year, month, day)

let parseBirthDate (s: string) = s |> parseAHVNo |> getBirthDate

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
