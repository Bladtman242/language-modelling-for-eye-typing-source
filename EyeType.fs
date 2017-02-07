module EyeType
open System
open System.Threading
open System.Drawing
open System.Windows.Forms

type point = System.Drawing.PointF
type symbol = (string * point)
type symtable = Map<string,point list>
type timewindow = point list * symtable

let FRAME_SIZE = 60
let CIRCLE_RADIUS = 150.0; //pixels
let FRAMES_PER_SECOND = 60.0
let REVOLUTIONS_PER_SECOND = 1.0/5.0
let WINDOW_SIZE = int <| CIRCLE_RADIUS * 2.0 * 1.4

let radiansOfDegrees d = System.Math.PI/180.0 * d
let degreesOfRadians r = 180.0/System.Math.PI * r

let convertPoint (p : Point) : point = new point (float32 p.X, float32 p.Y)

module List =
    let truncateTo n l =
        let rec skip n xs = if (n <= 0) then xs
                            else skip (n-1) (List.tail xs)

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

let truncateWindow = List.truncateTo FRAME_SIZE

let addFrame (mousePos : point) (syms: symbol list) (tw : timewindow) : timewindow =
    let mousepositions = fst tw @ [mousePos] |> truncateWindow
    let symtable = List.fold (fun tw (s,p) -> Map.add s ((Map.find s tw) @ [p]) tw)
                             (snd tw)
                             syms
    let symtable' = Map.map (fun k v -> truncateWindow v) symtable
    (mousepositions, symtable')

let pearson (xs : float list) (ys : float list) : float =
    if (List.length xs <> List.length ys) then raise (System.ArgumentException("xs and ys must be of equal length"))
    else let xsAvg = List.average xs
         let ysAvg = List.average ys
         let xs' = List.map (fun x -> x - xsAvg) xs
         let ys' = List.map (fun y -> y - ysAvg) ys
         let covariance = List.sum (List.map2 (*) xs' ys')
         let xsStdDev = List.map (fun x -> x ** 2.0) xs' |> List.sum |> sqrt
         let ysStdDev = List.map (fun x -> x ** 2.0) ys' |> List.sum |> sqrt
         covariance / (xsStdDev * ysStdDev)

let pearsonXY xs ys xs' ys' = min (pearson xs xs') (pearson ys ys')

let unzipPoints (ps : point list) : float list * float list =
    List.map (fun (p : System.Drawing.PointF) -> (float p.X, float p.Y)) ps
    |> List.unzip

let correlationsInWindow (tw : timewindow) : (string * float) list =
    let (xs,ys) = unzipPoints (fst tw)
    Map.map (fun s ps ->
                let (xs',ys') = unzipPoints ps
                pearsonXY xs ys xs' ys')
            (snd tw)
    |> Map.toList
    |> List.sortWith (fun a b -> -(compare (snd a) (snd b)))

// Create main form window
let mainForm = new Form(Width = WINDOW_SIZE, Height = WINDOW_SIZE, Text = "Pie Chart")

// Create the menu with two items (Open and Save)
//let menu = new ToolStrip()
//let btnOpen = new ToolStripButton("Open")
//let btnSave = new ToolStripButton("Save", Enabled = false)

//ignore(menu.Items.Add(btnOpen))
//ignore(menu.Items.Add(btnSave))
//mainForm.Controls.Add(menu)

// Create picture box for displaying the chart
let boxChart = 
  new PictureBox
    (BackColor = Color.White, Dock = DockStyle.Fill)
     //SizeMode = PictureBoxSizeMode.AutoSize)
     //SizeMode = PictureBoxSizeMode.CenterImage)

mainForm.Controls.Add(boxChart) 

let placeSyms (syms : string list) (offset : float) : symbol list =
    let center = PointF(float32 <| WINDOW_SIZE / 2, float32 <| WINDOW_SIZE / 2)
    let noSyms = List.length syms
    let symAndPoint (i : int) (s : string) : symbol =
       let degsPerSym = 360.0/float noSyms
       let angle = radiansOfDegrees (degsPerSym * (float i) + offset - 90.0)
       let p = (
           CIRCLE_RADIUS * (cos angle),
           CIRCLE_RADIUS * (sin angle)
           )
       (s, PointF.Add(center, (Size(int (fst p), int (snd p)))))

    List.mapi (fun i s -> symAndPoint i s) syms

let drawSymbol idx ((s,p) : symbol) (chosen : int option) font (gr : System.Drawing.Graphics) : Unit =
    let col = Option.fold (fun col ch -> if abs (ch-idx) < 2 then Color.Red else col) Color.Black chosen
    let brush = new SolidBrush(col)

    gr.DrawString(s, mainForm.Font, brush, (p : System.Drawing.PointF))

let drawSymbols (syms : symbol list) (chosen : int option) =
    let chart = new Bitmap(WINDOW_SIZE,WINDOW_SIZE)
    use gr = Graphics.FromImage(chart)
    List.iteri (fun idx sym -> drawSymbol idx sym chosen mainForm.Font gr) syms //fsharp's typesystem needs a hint here.
    boxChart.Image <- chart
    ()

let letters = ["A"; "B"; "C"; "D"; "E"; "F"; "G"; "H"; "I"; "J"; "K"; "L"; "M";
               "N"; "O"; "P"; "Q"; "R"; "S"; "T"; "U"; "V"; "W"; "X"; "Y"; "Z";]

let runner () =
    let rec _runner (fpsAdjust : float) (offset : float) (tw : timewindow) (corr : (string * float) list) (chosen : int option) : Unit =
        Thread.Sleep (max 0 (int (1000.0/FRAMES_PER_SECOND - fpsAdjust)))
        let start = DateTime.Now
        let syms = placeSyms letters offset
        ignore <| drawSymbols syms chosen
        let coord = convertPoint <| boxChart.PointToClient Control.MousePosition
        let tw' = addFrame coord syms tw
        let correlations = correlationsInWindow tw' |> List.filter (snd >> System.Double.IsNaN >> not)
                                              |> List.filter (fun (_,c) -> c >= 0.99)
        let correlated = not <| List.isEmpty correlations
        let offset' = offset + REVOLUTIONS_PER_SECOND/FRAMES_PER_SECOND * 360.0
        let chosen' = if correlated then List.index (fst (List.head correlations)) letters else None

        if correlated then printfn "follows %A" (List.head correlations)
        let corr' = if correlated then corr @ [(List.head correlations)]
                    else corr

        let time = (DateTime.Now - start).TotalMilliseconds
        _runner time offset' tw' corr' chosen'
    ignore <| _runner 0.0 0.0 ([], List.fold (fun m l -> Map.add l [] m) Map.empty letters) [] None

[<STAThread>]
[<EntryPoint>]
let main args = 
    mainForm.Closed.Add (fun _ -> Environment.Exit 0)
    Async.Start (async {runner ()})
    //btnOpen.Click.Add(drawPieChart)
    Application.EnableVisualStyles()
    Application.Run(mainForm);
    0

// vim: set ts=4 sw=4 et:
