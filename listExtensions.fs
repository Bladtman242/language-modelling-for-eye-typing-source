module ListExtensions
module List =
    let truncateLeftTo n l =
        let len = List.length l
        if (n < len) then
            List.skip (len - n) l
        else l
