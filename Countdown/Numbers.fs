﻿module Numbers

type Expr =
| Number of int
| Add of Expr * Expr
| Subtract of Expr * Expr
| Multiply of Expr * Expr
| Divide of Expr * Expr

let rec eval = function
| Number x        -> x     
| Add (x, y)      -> eval x + eval y
| Subtract (x, y) -> eval x - eval y
| Multiply (x, y) -> eval x * eval y
| Divide (x, y)   -> eval x / eval y

// string representation of an expression, with evaluated result
let write n =
    let rec write' = function
    | Number x        -> x.ToString()
    | Add (x, y)      -> "(" + write' x + " + " + write' y + ")"
    | Subtract (x, y) -> "(" + write' x + " - " + write' y + ")"
    | Multiply (x, y) -> "(" + write' x + " * " + write' y + ")"
    | Divide (x, y)   -> "(" + write' x + " / " + write' y + ")"
    write' n + " = " + (eval n).ToString()

let generate exprs =
    // a specialised choose2 function that returns the pair plus the remainders
    // uses indices for comparison in case of duplicate values
    let pairTwo exprs =
        let indexed = List.mapi (fun i x -> i,x) exprs
        [ for (i,x) in indexed do
          for (j,y) in indexed do
          if j > i then
            yield (x,y), [ for (k,z) in indexed do if k <> i && k <> j then yield z ] ]

    // for two given expressions, get possible combinations
    let getExpressions x y =
        let isDivisible x y =
            let denom = eval y
            denom <> 0 && eval x % denom = 0
            
        let a = max x y
        let b = min x y
        seq {
            yield Multiply (x, y)
            yield Add (x, y)
            yield Subtract(a, b) // don't bother with negatives
            if isDivisible a b
                then yield Divide(a, b)
            yield x // yielding each partial result prevents near-misses getting lost
            yield y // although expands the search tree
        }
    
    seq {
        for (x,y),zs in pairTwo exprs do
        for expr in getExpressions x y do
        yield expr :: zs }


let compute numbers target =
    let distance = eval >> ((-) target) >> abs
    let fitness = Seq.map distance >> Seq.min

    let iterate =
        Seq.collect generate
        >> Seq.sortBy fitness

    let rec compute' candidates =
        let first = Seq.head candidates
        match first, fitness first with
        | x::[], _ -> x // no more iterations, this is the best one
        | exprs,  0 -> Seq.minBy distance exprs // woop, got it - now find which expression met the criteria
        | _,     _ -> candidates |> iterate |> compute'
    seq { yield numbers |> List.map Number }
    |> compute'