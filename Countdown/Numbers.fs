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
let write expr =
    let rec write' expr =
        let writeOp op x y = sprintf "(%s %s %s)" (write' x) op (write' y)
        match expr with
        | Number x        -> x.ToString()
        | Add (x, y)      -> writeOp "+" x y
        | Subtract (x, y) -> writeOp "-" x y
        | Multiply (x, y) -> writeOp "*" x y
        | Divide (x, y)   -> writeOp "/" x y
    write' expr + " = " + (eval expr).ToString()

let generate exprs =
    // for two given expressions, get possible combinations
    let getExpressions x y =
        let isDivisible x y =
            let denom = eval y
            denom <> 0 && eval x % denom = 0
            
        let a,b = if eval x > eval y then x,y else y,x
        seq {
            yield Multiply(x, y)
            yield Add(x, y)
            yield Subtract(a, b) // don't bother with negatives
            if isDivisible a b
                then yield Divide(a, b)
            yield x // yielding each partial result prevents near-misses getting lost
            yield y // although expands the search tree
        }
    
    let indexed = List.mapi (fun i x -> i,x) exprs

    seq {
        for i,x in indexed do
        for j,y in indexed do
        if j > i then
            for expr in getExpressions x y do
            yield expr :: [ for k,z in indexed do if k <> i && k <> j then yield z ] }


let compute numbers target =
    let distance = eval >> (-) target >> abs
    let fitness = Seq.map distance >> Seq.min

    let iterate =
        Seq.collect generate
        >> Seq.sortBy fitness

    let rec compute' candidates =
        let first = Seq.head candidates
        match first, fitness first with
        | x::[], _ -> x // no more iterations, this is the best one
        | exprs, 0 -> List.minBy distance exprs // woop, got it - now find which expression met the criteria
        | _        -> candidates |> iterate |> compute'
    seq { yield numbers |> List.map Number }
    |> compute'