module ListExtensions
module List =
    let rec skip n xs = if (n <= 0) then xs
                        else skip (n-1) (List.tail xs)
    let truncateTo n l =
        let len = List.length l
        if (n < len) then
            skip (len - n) l
        else l

    let index (e : 'a) (l: 'a list) : int option =
        try
            Some <| List.findIndex (fun x -> x = e) l
        with
        | :? System.Collections.Generic.KeyNotFoundException as ex -> 
              None

