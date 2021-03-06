module SwissTennis.Model

open System

type Date = System.DateTime

(* Classification *)

// Art. 3.1
type Classification =
  | N of int
  | R of int // N 1..4, R 1..9
  override x.ToString () = (sprintf "%A" x).Replace (" ", String.Empty)
  static member All = [for i in [1..4] -> N i] @ [for i in [1..9] -> R i]
  static member Parse (s: string) =
    let (letter, digit) = (s.[0], s.[1] |> string |> int)
    let constr = match letter with | 'N' -> N | 'R' -> R | _ -> failwithf "Invalid classification: '%s'" s
    constr digit

type value = float

(* Swiss Tennis Data *)
type Player = {
  LicNo: string
  Name: string
  Classification: string
  Rank: int
  ClassificationValue: float
  AgeCategory: int
  State: char
}

(* Player Details and Match Results *)
type TournamentType =
  | InterClub
  | Regular

type Tournament = {
  ID: int  // 229353
  Type: TournamentType  // IC
  Name: string  // "IC 45+ NLC"
}

type MatchOutcome =
  | Win // S
  | Loss // N

type MatchSetResults =
  | WO // W
  | Sets of (int * int) list  // e.g. [(6, 3); (6, 3)], (Me, Opponent)

//type MatchResultValue = MatchOutcome * value  // value of rival

// TODO: DB primary key: (Date, Tournament, MyID, OpponentID)?
type MatchResult = {
  Date: Date  // "05.05.13"
  Tournament: Tournament
  Outcome: MatchOutcome
  SetResults: MatchSetResults

  OpponentName: string  // TODO: remove? (redundancy)
  OpponentLicenseNo: string
  OpponentCompetitionValue: value  // (4.L.)...
  OpponentNewClassification: Classification option  // "R5"

  IsDiscardedLoss: bool  // "X" or " "
}

type PlayerDetails = {
  Name: string  // "Master"
  FirstName: string  // "Hans"
  LicenseNo: string  // "000-00-000-0"
  Clubs: string list  // ["TC Sporting Derendingen (Stammmitglied)"]

  CurrentClassification: Classification  // "R3"
  CurrentClassificationValue: value  // 7.820
  CurrentCompetitionValue: value  // 7.291
  CurrentRiskValue: value  // 0.529
  RankNo: int  // 757
  MatchCount: int // 21
  WOCount: int  // 0
  WODeduction: bool  // "nein", "ja"
  AgeCategory: string  // "35+"
  StatusLicense: bool  // "aktiv", "suspendiert"
  StatusIC: bool  // "IC berechtigt", "nicht IC berechtigt"
  StatusJIC: bool option // "JIC  berechtigt", "nicht JIC  berechtigt", null
  LastClassification: Classification option  // "R3"

  // take from MatchResults of opponents detail sheet
  // LastCompetitionsValue4L: value

  MatchResults: MatchResult list
}

let ageCategories = seq {
  for i in [10..2..18] do yield ((string i) + "&U")
  yield "A"; yield "35+"  // women: "30+"; "40+"
  for i in [45..5..75] do yield ((string i) + "+")
}
