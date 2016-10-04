#load "MiniCalc.fs"
//open SwissTennis.Calculations

// JS
let wins = [1.145; 1.000; 1.033; 1.203; 1.00; 2.668; 0.694; 1.154]
let losses = [3.024; 3.290; 2.743; 2.121; 2.846; 2.272; 2.661]
calcCWR 1.563  (wins, losses)
//val it : float * float * float = (2.683, 2.192, 0.491)


// Patrick Voellm
let wins = [1.000; 1.000; 0.623; 0.596; 1.086; 1.049]
let losses = [1.632; 2.967; 3.282; 1.278; 2.649; 1.720; 4.693; 0.602; 2.188; 2.392; 4.160; 1.543; 4.678; 2.791; 2.149; 2.831]
calcCWR 1.185 (wins, losses)
//val it : float * float * float = (1.843, 1.314, 0.529)


// Stefan Burger
calcCWR 1.781 ([1.000; 0.703], [2.574; 0.870; 2.668])
//val it : float * float * float = (1.682, 1.339, 0.343)

// Roman Schnelli
let wins = [2.412; 3.272; 2.067]
let losses = [
  3.418; 3.456; 3.985; 3.917; 4.828; 3.494; 3.262; 3.387; 3.662; 3.351;
  3.927; 4.304; 3.087; 3.503; 3.779; 3.832; 3.202; 4.249; 2.065]
calcCWR 3.265 (wins, losses)
//val it : float * float * float = (3.125, 2.552, 0.573)

// Martin
calcCWR 2.0 ([1.382; 1.533], [])
//val it : float * float * float = (2.515, 2.386, 0.129)

