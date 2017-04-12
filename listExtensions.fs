module ListExtensions
module List =
    let truncateLeftTo n l =
        let len = List.length l
        List.skip (len - n) l

// vim: set ts=4 sw=4 et:
