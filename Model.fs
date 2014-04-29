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

type PlayerDetails = {
  Name: string  // "Master"
  FirstName: string  // "Hans"
  LicenseNo: string  // "000-00-000-0"
  Clubs: string[]  // ["TC Sporting Derendingen (Stammmitglied)"]

  CurrentClassification: Classification  // "R3"
  CurrentClassificationValue: value  // 7.820
  CurrentCompetitionValue: value  // 7.291
  CurrentRiskValue: value  // 0.529
  RankNo: int  // 757
  MatchCount: int // 21
  WOCount: int  // 0
  WODeduction: string  // "nein"
  AgeCategory: string  // "50+"
  StatusLicense: string  // "aktiv" // bool?
  StatusIC: string  // "IC berechtigt" // bool?
  StatusJIC: string  // "nicht JIC" // bool?
  LastClassification: Classification  // "R3"
}

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
  | WinWO // W
  | LossWO // Z
//	| Skip? (Art. 5.8)

//type MatchResultValue = MatchOutcome * value  // value of rival

type MatchResult = {
  DiscardedLoss: bool  // "X" or " "
  Date: Date  // "05.05.13"
  Tournament: Tournament
  OpponentName: string  // TODO: remove? (redundancy)
  OpponentLicenseNo: string
  CompetitionValue: value  // (4.L.)...
  SetResults: (int * int)[]  // [(6, 3); (6, 3)]
  OutCome: MatchOutcome
}

