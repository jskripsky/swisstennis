// From http://www.swisstennis.ch/
// Wettkampf > Rules & Regulations > Reglemente > Klassierungsrichtlinien (KR)
// http://www.swisstennis.ch/upload/docs/wettkampf/2014_Klassierungsrichtlinien-d.pdf

// Art. 3.1
type Classification = N of int | R of int // N 1..4, R 1..9
let allClassifications = [for i in [1..4] -> N i] @ [for i in [1..9] -> R i]

// Art. 3.2
let allPlayers = 50323 // Swisstennis Website, 2014-04
let r8Players = 10300
let n1r7Players = (10 <<< 11) - 10
let r9Players = allPlayers - n1r7Players - r8Players

let allContingents = List.init 11 (fun i -> 10 <<< i) @ [r8Players; r9Players]
let allRanks =
  allContingents
  |> List.scan (+) 0
  |> Seq.pairwise
  |> Seq.map (fun (l, h) -> (l + 1, h))
  |> Seq.toList

let interpolatedRanking rank =
  List.zip allClassifications allRanks
  |> Seq.find (fun (_, (r1, r2)) -> r1 <= rank && rank <= r2)
  |> function (c, (r1, r2)) -> (c, (float (rank - r1)) / (float (r2 - r1 + 1)))


type w = float

type MatchOutcome =
	| Win // S
	| Loss // N
	| WinWO // W
	| LossWO // Z
//	| Skip? (Art. 5.8)

type MatchResult = MatchOutcome * w  // w of rival


// Art. 5.6, 5.7
let term factor sign w0 (defeatedWs: seq<w>) (lostWs: seq<w>) =
	let eSum w0 ws = Seq.sumBy exp ws + exp w0
	let neg ws = Seq.map (~-) ws

	let defeatSum = eSum w0 defeatedWs
	let lostSum = eSum -w0 (neg lostWs) 

	factor * (log defeatSum + sign * (log lostSum))

type Calc = w -> seq<w> -> seq<w> -> w

// Art. 5.6
let W : Calc = term (1.0 / 2.0) -1.0
// Art. 5.7
let R: Calc = term (1.0 / 6.0) 1.0


// Art. 5.1
let roundN n x =
	let shift = 10.0 ** (float n)
	round (x * shift) / shift

let round3 = roundN 3
// round3 0.1234567 = 0.123

let C w0 defeatedWs lostWs = W w0 defeatedWs lostWs + R w0 defeatedWs lostWs  |> round3
//  C 3.750 [4.0; 2.5] [4.6; 3.0; 2.4; 1.9] = 3.505
// (http://www.tc-rosental.ch/IC_Interessenten.htm, rounding of C instead of W and R)


// Art. 5.5
let numOfLossesToIgnore matches = matches / 6 |> min 4
// [0; 1; 5; 6; 7; 12; 18; 24; 30; 36] |> List.map numOfLossesToIgnore = [0; 0; 0; 1; 1; 2; 3; 4; 4; 4]


// Art. 5.2: Ausgangspunkt für die Berechnung des neuen Wettkampfwertes W ist der Wettkampfwert W0, der vom Wettkampfwert 5 (W5) der vorangegangenen Periode abgeleitet wird. Der Mindestausgangswert beträgt 1.
// Art. 5.3: 
// Art. 5.4
// Art. 5.8
// Art. 5.9: Alle lizenzierten Spieler werden nach dem gemäss Abs.1 bis 8 berechneten Klassierungswert C sortiert.
// Art. 5.10

// Art. 8.1 Berücksichtigung der Resultate: Das Klassierungsjahr wird in zwei Klassierungsperioden eingeteilt.
// Die erste dauert vom 1. April bis zum 30. September, die zweite vom 1. Oktober bis zum 31. März des folgenden Jahres.
type Date = System.DateTime
let periods startYear = [(Date (startYear, 4, 1), Date (startYear, 9,30)); (Date (startYear, 10, 1), Date (startYear + 1, 3, 31))]
// periods 2014 = FIXME!

// Art. 8.2: Für die Klassierungsberechnung werden jeweils die Resultate der beiden letzten Klassierungsperioden berücksichtigt. 

