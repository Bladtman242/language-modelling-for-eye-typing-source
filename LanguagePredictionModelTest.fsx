#r "dlls/FsCheck.2.8.0/lib/net452/FsCheck.dll"
#load "LanguagePredictionModel.fs"

module LanguagePredictionModelTest =
    open FsCheck
    open LanguagePredictionModel
    open ListExtensions

    type PredictionOfFourGram =
        static member isIdempotent (n: NonNegativeInt) (xs:list<int>) =
            let n' = n.Get
            let res1 = List.truncateLeftTo n' xs 
            let res2 = List.truncateLeftTo n' res1
            res1 = res2
