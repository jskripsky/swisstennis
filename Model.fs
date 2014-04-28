module SwissTennis.Model

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

  CurrentClassification: string  // "R3"
  CurrentClassificationValue: float  // 7.820
  CurrentCompetitionValue: float  // 7.291
  CurrentRiskValue: float  // 0.529
  RankNo: int  // 757
  MatchCount: int // 21
  WOCount: int  0
  WODeduction: string  // "nein"
  AgeCategory: string  // "50+"
  StatusLicense: string  // "aktiv"
  StatusIC: string  // "IC berechtigt"
  StatusJIC: string  // "nicht JIC"
  LastClassification: string  // "R3"
}

type TournamentType =
  | IC
  | Reg

type Tournament = {
  ID: int  // 229353
  Type: TournamentType  // IC
  Name: string  // "IC 45+ NLC"
}

type Match = {
  DiscardedLoss: bool  // "X" or " "
  Date: DateTime  // "05.05.13"
  Tournament: Tournament
  OpponentName: string
  OpponentLicenseNo: string
  CompetitionValue: float  // (4.L.)...
  SetResults: (int * int)[]  // [(6, 3); (6, 3)]
  Code: char
}

