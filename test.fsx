#r "dlls/FsCheck.2.8.0/lib/net452/FsCheck.dll"
#load "listExtensions.fs"
#load "EyeType.fs"

open FsCheck
open EyeType
open ListExtensions

type ListTrucateTo =
    static member isIdempotent (n: NonNegativeInt) (xs:list<int>) =
        let n' = n.Get
        let res1 = List.truncateLeftTo n' xs 
        let res2 = List.truncateLeftTo n' res1
        res1 = res2

    static member providesAtMostNLengthList (xs:list<int>) (n: NonNegativeInt) =
            let n' = n.Get
            let len = List.length xs
            let res = List.truncateLeftTo n' xs
            if n' < len then
                n' = List.length res
            else
             xs = res

    //I strongly suspect this is implied by other properties
    static member IsCommutative (xs:list<int>) (n: NonNegativeInt) (m: NonNegativeInt) =
        let n' = n.Get
        let m' = m.Get
        (List.truncateLeftTo n' xs |> List.truncateLeftTo m') = (List.truncateLeftTo m' xs
        |> List.truncateLeftTo n')

    static member truncateNegOneIsConsInverse (xs:list<int>) (x: int) =
        let xs' = x :: xs
        let len = List.length xs'
        xs = List.truncateLeftTo (len - 1) xs'

    static member negativeThrows (xs:list<int>) (n: PositiveInt)=
            let n' = -n.Get
            Prop.throws<System.ArgumentException,_> (lazy List.truncateLeftTo n' xs)

    static member zeroIsEmptyList (xs:list<int>) =
        List.empty = List.truncateLeftTo 0 xs

Check.QuickAll<ListTrucateTo> ()

// vim: set ts=4 sw=4 et:
