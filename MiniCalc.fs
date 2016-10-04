module SwissTennis.Calculations

let term factor sign w0 (wonWs: seq<float>, lostWs: seq<float>) =
  let s = seq { yield w0; yield! wonWs }
  let n = seq { yield w0; yield! lostWs }
  let sum ws = ws |> Seq.sumBy exp |> log

  let s' = sum s
  let n' = sum (n |> Seq.map (~-))

  factor * (s' + sign * n')

type Calc = float -> (seq<float> * seq<float>) -> float
let W: Calc = term (1.0 / 2.0) -1.0
let R: Calc = term (1.0 / 6.0) 1.0

let roundN n x =
  let shift = 10.0 ** (float n)
  round (x * shift) / shift

let round3 = roundN 3

let removeVoidLosses (wonWs, lostWs) =
  let totalMatchCount = (Seq.length wonWs) + (Seq.length lostWs)
  let voidLosses =
    (totalMatchCount / 6)
    |> min 4
    |> min (Seq.length lostWs)

  lostWs
  |> Seq.sort
  |> Seq.skip voidLosses

let calcCWR w0 (wonWs, lostWs) =
  let lostWs' = removeVoidLosses (wonWs, lostWs)
  let eval f = f w0 (wonWs, lostWs')
  let (w, r) = (eval W, eval R)

  (w + r |> round3, w, r)

