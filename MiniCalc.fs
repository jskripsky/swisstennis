module SwissTennis.Calculations

let removeVoidLosses (wonWs, lostWs) =
  let totalMatchCount = (List.length wonWs) + (List.length lostWs)
  let voidLosses =
    (totalMatchCount / 6)
    |> min 4
    |> min (List.length lostWs)

  lostWs
  |> List.sort
  |> List.skip voidLosses

let term factor sign (wonWs: list<float>, lostWs: list<float>) =
  let sum = List.sumBy exp >> log
  let inv = (~-)

  let s' = sum wonWs
  let n' = sum (lostWs |> List.map inv) |> inv

  factor * (s' + sign * n')

type Calc = (list<float> * list<float>) -> float
let W: Calc = term (1.0 / 2.0) 1.0
let R: Calc = term (1.0 / 6.0) -1.0

let round3 (x: float) = System.Math.Round (x, 3)

let calcCWR w0 (wonWs, lostWs) =
  let lostWs' = removeVoidLosses (wonWs, lostWs)
  let results =   (w0::wonWs, w0::lostWs')
  let (w, r) = (W results, R results)

  (w + r |> round3, w, r)

