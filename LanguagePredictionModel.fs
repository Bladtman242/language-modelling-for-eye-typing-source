module LanguagePredictionModel
open System.IO
    type prediction = {
        value: string;
        probability: double;
    }

    let fourGramsFromFile (filepath) = File.ReadLines(filepath)

    let predictionOfFourGram (line : string) : (string * prediction) =
        let splits = line.Split [|' '|]
        let pred = { value = splits.[3]; probability = System.Double.Parse splits.[4] }
        (splits.[0] + " " + splits.[1] + " " + splits.[2], pred)

    let addPredToMap m (s,p) =
        let existing = Map.tryFind s m
        let newVal = if Option.isNone existing then [p]
                     else p :: Option.get existing
        Map.add s newVal m

    let predictionsOfFourGrams (grams : seq<string>) =
        Seq.map predictionOfFourGram grams
     |> Seq.fold (addPredToMap) Map.empty

    let orderPrediction _ (ps : prediction list) =
        let sum = List.sumBy (fun p -> p.probability) ps
        List.map (fun p -> {p with probability = p.probability / sum}) ps
     |> List.sortBy (fun p -> - p.probability)

    let orderPredictions (m : Map<string, prediction list>) = Map.map orderPrediction m

    let predictionsFromFileName (filepath) =
        fourGramsFromFile filepath
     |> predictionsOfFourGrams
     |> orderPredictions

// vim: set ts=4 sw=4 et:
