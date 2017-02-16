module EyeType
open System
open System.Threading
open System.Drawing
open System.Windows.Forms
open ListExtensions

type point = System.Drawing.PointF
type symbol = (string * point)
type symtable = Map<string,point list>
type timewindow = point list * symtable

let FRAME_SIZE = 60
let CIRCLE_RADIUS = 150.0; //pixels
let FRAMES_PER_SECOND = 60.0
let REVOLUTIONS_PER_SECOND = 1.0/5.0
let WINDOW_SIZE = int <| CIRCLE_RADIUS * 2.0 * 1.4

let letters = ["A"; "B"; "C"; "D"; "E"; "F"; "G"; "H"; "I"; "J"; "K"; "L"; "M";
               "N"; "O"; "P"; "Q"; "R"; "S"; "T"; "U"; "V"; "W"; "X"; "Y"; "Z";]

let empty_symtable = List.fold (fun m l -> Map.add l [] m) Map.empty letters
let empty_timeWindow = ([], empty_symtable)

let radiansOfDegrees d = System.Math.PI/180.0 * d
let degreesOfRadians r = 180.0/System.Math.PI * r

let convertPoint (p : Point) : point = new point (float32 p.X, float32 p.Y)

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

let drawSymbol idx ((s,p) : symbol) (chosen : int option) (font : Font) (gr : System.Drawing.Graphics) : Unit =
    let col = Option.fold (fun col ch -> if abs (ch-idx) < 2 then Color.Red else col) Color.Black chosen
    let brush = new SolidBrush(col)
    let letterSizeCorrection = int(-font.GetHeight()) / 2 //only approximates the correction on the y-axis
    let p' = PointF.Add(p, Size(letterSizeCorrection, letterSizeCorrection))

    gr.DrawString(s, font, brush, (p : System.Drawing.PointF))

let drawSymbols (syms : symbol list) (chosen : int option) =
    let chart = new Bitmap(WINDOW_SIZE,WINDOW_SIZE)
    use gr = Graphics.FromImage(chart)
    List.iteri (fun idx sym -> drawSymbol idx sym chosen mainForm.Font gr) syms
    mainForm.Invoke(System.Func<unit,unit>(fun () -> ignore <| boxChart.Image <- chart), ())

let mainLoop () =
    let rec _mainLoop (fpsAdjust : float) (offset : float) (tw : timewindow) (corr : (string * float) list) (chosen : int option) (latent_period : float) : Unit =
        let sleepTime = max 0 (int (1000.0/FRAMES_PER_SECOND - fpsAdjust))
        Thread.Sleep sleepTime
        let start = DateTime.Now
        let syms = placeSyms letters offset
        let offset' = offset + REVOLUTIONS_PER_SECOND/FRAMES_PER_SECOND * 360.0

        let coord = convertPoint <| boxChart.PointToClient Control.MousePosition
        let tw' = addFrame coord syms tw
        if 0.0 >= latent_period then
            let correlations = correlationsInWindow tw'
                               |> List.filter (not << System.Double.IsNaN << snd)
                               |> List.filter (fun (_,c) -> c >= 0.99)
            let correlated = not <| List.isEmpty correlations
            let chosen' = if correlated then List.index (fst (List.head correlations)) letters else None

            if correlated then printfn "follows %A" (List.head correlations)
            let corr' = if correlated then corr @ [(List.head correlations)]
                        else corr
            let latent_period = if correlated then 600.0 else 0.0
            ignore <| drawSymbols syms chosen'

            let time = (DateTime.Now - start).TotalMilliseconds
            _mainLoop time offset' tw' corr' chosen' latent_period
        else
            ignore <| drawSymbols syms None

            let time = (DateTime.Now - start).TotalMilliseconds
            _mainLoop time offset' tw' corr None (latent_period - (time + float sleepTime))
    ignore <| _mainLoop 0.0 0.0 empty_timeWindow [] None 0.0

[<STAThread>]
[<EntryPoint>]
let main args = 
    mainForm.Closed.Add (fun _ -> Environment.Exit 0)
    Async.Start (async {mainLoop ()})
    Application.EnableVisualStyles()
    Application.Run(mainForm);
    0

// vim: set ts=4 sw=4 et:
