using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using projekat.Model;
//using Point = projekat.Model.Point;

namespace projekat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Point topLeft;

        public double newX, newY;
        public double minX, maxX, minY, maxY;

        public const int n = 265;
        public bool[,] Matrix = new bool[n, n];
        public double defaultWidthHeight = 6;

        List<SubstationEntity> listSubstationEntity = new List<SubstationEntity>();
        List<NodeEntity> listNodeEntity = new List<NodeEntity>();
        List<SwitchEntity> listSwitchEntity = new List<SwitchEntity>();
        List<LineEntity> listLineEntity = new List<LineEntity>();

        Dictionary<LineEntity, double> allLineEntity_Distances = new Dictionary<LineEntity, double>();
        Dictionary<long, Ellipse> AllEllipse = new Dictionary<long, Ellipse>();
        Dictionary<long, PowerEntity> AllPowerEntityMatrix = new Dictionary<long, PowerEntity>();
        Dictionary<Polyline, LineEntity> AllPolylines = new Dictionary<Polyline, LineEntity>();

        public long oldIndexEntity1 = -1, oldIndexEntity2 = -1;

        private Double zoomMax = 5;
        private Double zoomMin = 0.9;
        private Double zoomSpeed = 0.001;
        private Double zoom = 1;

        private string chosenOption;
        public static string drawOrCancel;
        private List<System.Windows.Point> polygonPoints;

        Stack<ICommand> undoList = new Stack<ICommand>();
        Stack<ICommand> redoList = new Stack<ICommand>();

        public static Color fillColor;
        public static Color borderColor;
        public static double width;
        public static double height;
        public static double borderThickness;
        public static double textFont;
        public static string textShow;



        public MainWindow()
        {
            InitializeComponent();
            chosenOption = "";
            drawOrCancel = "";
            polygonPoints = new List<System.Windows.Point>();
        }
		public static void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
		{
			bool isNorthHemisphere = true;

			var diflat = -0.00066286966871111111111111111111111111;
			var diflon = -0.0003868060578;

			var zone = zoneUTM;
			var c_sa = 6378137.000000;
			var c_sb = 6356752.314245;
			var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
			var e2cuadrada = Math.Pow(e2, 2);
			var c = Math.Pow(c_sa, 2) / c_sb;
			var x = utmX - 500000;
			var y = isNorthHemisphere ? utmY : utmY - 10000000;

			var s = ((zone * 6.0) - 183.0);
			var lat = y / (c_sa * 0.9996);
			var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
			var a = x / v;
			var a1 = Math.Sin(2 * lat);
			var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
			var j2 = lat + (a1 / 2.0);
			var j4 = ((3 * j2) + a2) / 4.0;
			var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
			var alfa = (3.0 / 4.0) * e2cuadrada;
			var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
			var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
			var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
			var b = (y - bm) / v;
			var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
			var eps = a * (1 - (epsi / 3.0));
			var nab = (b * (1 - epsi)) + lat;
			var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
			var delt = Math.Atan(senoheps / (Math.Cos(nab)));
			var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

			longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
			latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
		}

        private void LoadGraph_Click(object sender, RoutedEventArgs e)
        {
			if (LoadGraph.IsEnabled)
			{
                LoadNodesFromXML();
                DrawNodes();
                LoadLinesFromXML();
                DrawLines();
                LoadGraph.IsEnabled = false;
			}
		}
		public void LoadNodesFromXML()
        {
			bool isFirstEntity = true;

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load("Geographic.xml");

			XmlNodeList nodeList;

			SubstationEntity substationEntity;
			nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity");
            foreach (XmlNode node in nodeList)
            {
                substationEntity = new SubstationEntity();
                substationEntity.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                substationEntity.Name = node.SelectSingleNode("Name").InnerText;
                substationEntity.X = double.Parse(node.SelectSingleNode("X").InnerText);
                substationEntity.Y = double.Parse(node.SelectSingleNode("Y").InnerText);

                ToLatLon(substationEntity.X, substationEntity.Y, 34, out newY, out newX);
                substationEntity.X = newX;
                substationEntity.Y = newY;
                listSubstationEntity.Add(substationEntity);

                if (isFirstEntity)
                {
                    minX = newX; maxX = newX; minY = newY; maxY = newY;
                    isFirstEntity = false;
                }

                if (minX > newX)
                    minX = newX;
                else if (maxX < newX)
                    maxX = newX;

                if (minY > newY)
                    minY = newY;
                else if (maxY < newY)
                    maxY = newY;
            }

            NodeEntity nodeEntity;
            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity");
            foreach (XmlNode node in nodeList)
            {
                nodeEntity = new NodeEntity();
                nodeEntity.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                nodeEntity.Name = node.SelectSingleNode("Name").InnerText;
                nodeEntity.X = double.Parse(node.SelectSingleNode("X").InnerText);
                nodeEntity.Y = double.Parse(node.SelectSingleNode("Y").InnerText);

                ToLatLon(nodeEntity.X, nodeEntity.Y, 34, out newY, out newX);
                nodeEntity.X = newX;
                nodeEntity.Y = newY;
                listNodeEntity.Add(nodeEntity);

                if (minX > newX)
                    minX = newX;
                else if (maxX < newX)
                    maxX = newX;

                if (minY > newY)
                    minY = newY;
                else if (maxY < newY)
                    maxY = newY;
            }

            SwitchEntity switchEntity;
            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity");
            foreach (XmlNode node in nodeList)
            {
                switchEntity = new SwitchEntity();
                switchEntity.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                switchEntity.Name = node.SelectSingleNode("Name").InnerText;
                switchEntity.Status = node.SelectSingleNode("Status").InnerText;
                switchEntity.X = double.Parse(node.SelectSingleNode("X").InnerText);
                switchEntity.Y = double.Parse(node.SelectSingleNode("Y").InnerText);

                ToLatLon(switchEntity.X, switchEntity.Y, 34, out newY, out newX);
                switchEntity.X = newX;
                switchEntity.Y = newY;
                listSwitchEntity.Add(switchEntity);

                if (minX > newX)
                    minX = newX;
                else if (maxX < newX)
                    maxX = newX;

                if (minY > newY)
                    minY = newY;
                else if (maxY < newY)
                    maxY = newY;
            }
        }
        public void DrawNodes()
        {
            int i, j, x, y;
            foreach (SubstationEntity entity in listSubstationEntity)
            {
                j = CalculateNew(maxX, minX, Matrix.GetLength(1) - 1, 0, entity.X); // (double oldMax, double oldMin, double newMax, double newMin, double oldValue)
                i = CalculateNew(maxY, minY, Matrix.GetLength(0) - 1, 0, entity.Y);
                //Matrix[i, j] = true;
                MatrixAvailability(ref i, ref j);

                x = CalculateNew(Matrix.GetLength(1) - 1, 0, MyCanvas.ActualWidth - 5, 5, j);
                y = CalculateNew(Matrix.GetLength(0) - 1, 0, MyCanvas.ActualHeight - 5, 5, i);

                Ellipse ellipse = new Ellipse();
                ellipse.Width = defaultWidthHeight;
                ellipse.Height = defaultWidthHeight;

                ImageBrush imageBrush = new ImageBrush();
                ellipse.Fill = imageBrush;
                ellipse.Stroke = Brushes.ForestGreen;
                ellipse.StrokeThickness = 1;

                var toolTip = new ToolTip();
                toolTip.Content = "    * Substation   \nID: " + entity.Id + "\nName: " + entity.Name;
                toolTip.Background = Brushes.ForestGreen;
                toolTip.Foreground = Brushes.White;
                ellipse.ToolTip = toolTip;

                Canvas.SetLeft(ellipse, x);
                Canvas.SetBottom(ellipse, y);
                MyCanvas.Children.Add(ellipse);
                AllEllipse.Add(entity.Id, ellipse);

                entity.X = j;
                entity.Y = i;
                AllPowerEntityMatrix.Add(entity.Id, entity);
            }

            foreach (NodeEntity entity in listNodeEntity)
            {
                j = CalculateNew(maxX, minX, Matrix.GetLength(1) - 1, 0, entity.X);
                i = CalculateNew(maxY, minY, Matrix.GetLength(0) - 1, 0, entity.Y);
                //Matrix[i, j] = true;
                MatrixAvailability(ref i, ref j);

                x = CalculateNew(Matrix.GetLength(1) - 1, 0, MyCanvas.ActualWidth - 5, 5, j);
                y = CalculateNew(Matrix.GetLength(0) - 1, 0, MyCanvas.ActualHeight - 5, 5, i);

                Ellipse ellipse = new Ellipse();
                ellipse.Width = defaultWidthHeight;
                ellipse.Height = defaultWidthHeight;

                ImageBrush imageBrush = new ImageBrush();
                ellipse.Fill = imageBrush;
                ellipse.Stroke = Brushes.Orange;
                ellipse.StrokeThickness = 1;

                var toolTip = new ToolTip();
                toolTip.Content = "    * Node   \nID: " + entity.Id + "\nName: " + entity.Name;
                toolTip.Background = Brushes.Orange;
                toolTip.Foreground = Brushes.White;
                ellipse.ToolTip = toolTip;

                Canvas.SetLeft(ellipse, x);
                Canvas.SetBottom(ellipse, y);
                MyCanvas.Children.Add(ellipse);
                AllEllipse.Add(entity.Id, ellipse);

                entity.X = j;
                entity.Y = i;
                AllPowerEntityMatrix.Add(entity.Id, entity);
            }

            foreach (SwitchEntity entity in listSwitchEntity)
            {
                j = CalculateNew(maxX, minX, Matrix.GetLength(1) - 1, 0, entity.X);
                i = CalculateNew(maxY, minY, Matrix.GetLength(0) - 1, 0, entity.Y);
                //Matrix[i, j] = true;
                MatrixAvailability(ref i, ref j);

                x = CalculateNew(Matrix.GetLength(1) - 1, 0, MyCanvas.ActualWidth - 5, 5, j);
                y = CalculateNew(Matrix.GetLength(0) - 1, 0, MyCanvas.ActualHeight - 5, 5, i);

                Ellipse ellipse = new Ellipse();
                ellipse.Width = defaultWidthHeight;
                ellipse.Height = defaultWidthHeight;

                ImageBrush imageBrush = new ImageBrush();
                ellipse.Fill = imageBrush;
                ellipse.Stroke = Brushes.Purple;
                ellipse.StrokeThickness = 1;

                var toolTip = new ToolTip();
                toolTip.Content = "    * Switch   \nID: " + entity.Id + "\nName: " + entity.Name + "\nStatus: " + entity.Status;
                toolTip.Background = Brushes.Purple;
                toolTip.Foreground = Brushes.White;
                ellipse.ToolTip = toolTip;

                Canvas.SetLeft(ellipse, x);
                Canvas.SetBottom(ellipse, y);
                MyCanvas.Children.Add(ellipse);
                AllEllipse.Add(entity.Id, ellipse);

                entity.X = j;
                entity.Y = i;
                AllPowerEntityMatrix.Add(entity.Id, entity);
            }
        }
        public void LoadLinesFromXML()
        {
            double distance;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("Geographic.xml");

            XmlNodeList nodeList;

            LineEntity lineEntity;
            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity");
            foreach (XmlNode node in nodeList)
            {
                lineEntity = new LineEntity();
                lineEntity.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                lineEntity.Name = node.SelectSingleNode("Name").InnerText;
                if (node.SelectSingleNode("IsUnderground").InnerText.Equals("true"))
                {
                    lineEntity.IsUnderground = true;
                }
                else
                {
                    lineEntity.IsUnderground = false;
                }
                lineEntity.R = float.Parse(node.SelectSingleNode("R").InnerText);
                lineEntity.ConductorMaterial = node.SelectSingleNode("ConductorMaterial").InnerText;
                lineEntity.LineType = node.SelectSingleNode("LineType").InnerText;
                lineEntity.ThermalConstantHeat = long.Parse(node.SelectSingleNode("ThermalConstantHeat").InnerText);
                lineEntity.FirstEnd = long.Parse(node.SelectSingleNode("FirstEnd").InnerText);
                lineEntity.SecondEnd = long.Parse(node.SelectSingleNode("SecondEnd").InnerText);

                if (ExistEntitiesWithId(lineEntity.FirstEnd, lineEntity.SecondEnd)) // Before = 2336; After = 2223
                {
                    distance = Math.Sqrt(Math.Pow((AllPowerEntityMatrix[lineEntity.SecondEnd].X - AllPowerEntityMatrix[lineEntity.FirstEnd].X), 2) + Math.Pow((AllPowerEntityMatrix[lineEntity.SecondEnd].Y - AllPowerEntityMatrix[lineEntity.FirstEnd].Y), 2));
                    allLineEntity_Distances.Add(lineEntity, distance);

                    listLineEntity.Add(lineEntity);
                }
            }
        }
        public void DrawLines()
        {
            var listDistances = allLineEntity_Distances.ToList();
            listDistances.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

            PowerEntity startNode;
            PowerEntity endNode;

            foreach (var item in allLineEntity_Distances)
            {
                startNode = AllPowerEntityMatrix[item.Key.FirstEnd];
                endNode = AllPowerEntityMatrix[item.Key.SecondEnd];

                List<System.Windows.Point> pointsForLines = BFS(startNode, endNode);
                Polyline polyline;

                polyline = new Polyline();
                polyline.StrokeThickness = 1;
                polyline.Stroke = Brushes.LightSkyBlue;

                var toolTip = new ToolTip();
                toolTip.Content = "    * Line   \nID: " + item.Key.Id + "\nName: " + item.Key.Name;
                toolTip.Background = Brushes.Red;
                toolTip.Foreground = Brushes.White;
                polyline.ToolTip = toolTip;

                System.Windows.Point point;
                double tempX, tempY;

                for (int i = 0; i < pointsForLines.Count; i++)
                {
                    tempX = CalculateNew(Matrix.GetLength(1) - 1, 0, MyCanvas.ActualWidth - 5, 5, pointsForLines[i].X);
                    tempX += defaultWidthHeight / 2; // center of Ellipse
                    tempY = CalculateNew(Matrix.GetLength(0) - 1, 0, MyCanvas.ActualHeight - 5, 5, pointsForLines[i].Y);
                    tempY = MyCanvas.ActualHeight - tempY - defaultWidthHeight / 2; // set right y and center of Ellipse

                    point = new System.Windows.Point(tempX, tempY);
                    polyline.Points.Add(point);
                }

                polyline.MouseRightButtonDown += ColorLine_MouseRightButtonDown;

                MyCanvas.Children.Add(polyline);
                AllPolylines.Add(polyline, item.Key);
            }
        }
        public List<System.Windows.Point> BFS(PowerEntity startNode, PowerEntity endNode)
        {
            Tuple<int, int>[,] previousPoint = new Tuple<int, int>[n, n];
            int startRow = (int)startNode.X, startColumn = (int)startNode.Y;
            Queue<int> queueRow = new Queue<int>();
            Queue<int> queueColumn = new Queue<int>();

            bool reached_end = false;   // Variable used to track whether the secondNode ever gets reached during the BFS

            bool[,] visited = new bool[n, n];   // R x C matrix of false values used to track whether the node at position (i, j) has been reached

            int r = (int)startNode.X, c = (int)startNode.Y; // currentPosition

            // FUNCION: solve
            queueRow.Enqueue(startRow);
            queueColumn.Enqueue(startColumn);
            visited[startRow, startColumn] = true;

            List<System.Windows.Point> pointsForLine = new List<System.Windows.Point>();

            while (queueRow.Count > 0)
            {
                r = queueRow.Dequeue();
                c = queueColumn.Dequeue();

                if (r == AllPowerEntityMatrix[endNode.Id].X && c == AllPowerEntityMatrix[endNode.Id].Y)
                {
                    reached_end = true;
                    break;
                }

                Explore_neighbours(r, c, ref visited, ref queueRow, ref queueColumn, ref previousPoint);
            }

            if (reached_end)
            {
                pointsForLine.Add(new System.Windows.Point(AllPowerEntityMatrix[endNode.Id].X, AllPowerEntityMatrix[endNode.Id].Y));

                Tuple<int, int> currentTupple = previousPoint[(int)AllPowerEntityMatrix[endNode.Id].X, (int)AllPowerEntityMatrix[endNode.Id].Y];

                while (!(currentTupple.Item1 == startNode.X && currentTupple.Item2 == startNode.Y))
                {
                    pointsForLine.Add(new System.Windows.Point(currentTupple.Item1, currentTupple.Item2));
                    currentTupple = previousPoint[currentTupple.Item1, currentTupple.Item2];
                }

                pointsForLine.Add(new System.Windows.Point(AllPowerEntityMatrix[startNode.Id].X, AllPowerEntityMatrix[startNode.Id].Y));
            }

            return pointsForLine;
        }
        private void ColorLine_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Polyline)
            {
                if (oldIndexEntity1 != -1 && oldIndexEntity2 != -1)
                {
                    foreach (SubstationEntity entity in listSubstationEntity)
                    {
                        if (entity.Id == oldIndexEntity1)
                            AllEllipse[entity.Id].Stroke = Brushes.ForestGreen;
                        if (entity.Id == oldIndexEntity2)
                            AllEllipse[entity.Id].Stroke = Brushes.ForestGreen;
                    }
                    foreach (NodeEntity entity in listNodeEntity)
                    {
                        if (entity.Id == oldIndexEntity1)
                            AllEllipse[entity.Id].Stroke = Brushes.Orange;
                        if (entity.Id == oldIndexEntity2)
                            AllEllipse[entity.Id].Stroke = Brushes.Orange;
                    }
                    foreach (SwitchEntity entity in listSwitchEntity)
                    {
                        if (entity.Id == oldIndexEntity1)
                            AllEllipse[entity.Id].Stroke = Brushes.Purple;
                        if (entity.Id == oldIndexEntity2)
                            AllEllipse[entity.Id].Stroke = Brushes.Purple;
                    }
                    oldIndexEntity1 = -1;
                    oldIndexEntity2 = -1;
                }

                foreach (var ellipse in AllEllipse)
                {
                    if (AllPolylines[((Polyline)e.OriginalSource)].FirstEnd == ellipse.Key)
                    {
                        oldIndexEntity1 = AllPolylines[((Polyline)e.OriginalSource)].FirstEnd;
                        ellipse.Value.Stroke = Brushes.Blue;
                    }
                    else if (AllPolylines[((Polyline)e.OriginalSource)].SecondEnd == ellipse.Key)
                    {
                        oldIndexEntity2 = AllPolylines[((Polyline)e.OriginalSource)].SecondEnd;
                        ellipse.Value.Stroke = Brushes.Blue;
                    }
                }
            }
        }
        public void Explore_neighbours(int r, int c, ref bool[,] visited, ref Queue<int> queueRow, ref Queue<int> queueColumn, ref Tuple<int, int>[,] previousPoint)
        {
            int[] dRow = { -1, +1, 0, 0 };       // North, south, east, west direction vectors
            int[] dColumn = { 0, 0, +1, -1 };

            int rr, cc;                         // neighbouring cell of currentPosition (r, c)

            for (int i = 0; i < 4; i++)
            {
                rr = r + dRow[i];
                cc = c + dColumn[i];
                // Skip invalid cells. Assume R and c for numn=ber of rows and columns
                if (rr < 0 || cc < 0)
                    continue;
                if (rr >= n || cc >= n)
                    continue;
                // Skip visited locations or blocked cells
                if (visited[rr, cc])
                    continue;
                queueRow.Enqueue(rr);
                queueColumn.Enqueue(cc);
                previousPoint[rr, cc] = new Tuple<int, int>(r, c);
                visited[rr, cc] = true;
            }
        }

        private void DrawEllipse_Click(object sender, RoutedEventArgs e)
        {
            chosenOption = "ellipse";
            //EllipseWin ellipseWin = new EllipseWin();
            //ellipseWin.Show();
        }

        private void DrawPolygon_Click(object sender, RoutedEventArgs e)
        {
            chosenOption = "polygon";
            //PolygonWin polygonWin = new PolygonWin();
            //polygonWin.Show();
        }

        private void AddText_Click(object sender, RoutedEventArgs e)
        {
            chosenOption = "addText";
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            chosenOption = "undo";

            if (undoList.Count > 0)
            {
                ICommand command = undoList.Pop();
                redoList.Push(command);
                command.Unexecute();

            }
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            chosenOption = "redo";

            if (redoList.Count > 0)
            {
                ICommand command = redoList.Pop();
                undoList.Push(command);
                command.Execute();

            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            chosenOption = "clear";
            List<FrameworkElement> cleared = new List<FrameworkElement>();
            foreach (var item in MyCanvas.Children)
            {
                cleared.Add((FrameworkElement)item);
            }
            ClearCommand command = new ClearCommand(MyCanvas, cleared);
            undoList.Push(command);
            //LoadGraph.IsEnabled = true;
            command.Execute();
        }
        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            topLeft = e.GetPosition(MyCanvas);

            switch (chosenOption)
            {
                case "ellipse":
                    EllipseWin ellipseWindow = new EllipseWin();
                    ellipseWindow.ShowDialog();

                    DrawEllipses();

                    break;
                case "polygon":
                    System.Windows.Point point = e.GetPosition(MyCanvas);
                    Ellipse ellipse = new Ellipse();

                    ellipse.Width = 3;
                    ellipse.Height = 3;

                    ellipse.Fill = Brushes.Black;
                    ellipse.Stroke = Brushes.Black;

                    MyCanvas.Children.Add(ellipse);

                    Canvas.SetTop(ellipse, point.Y);
                    Canvas.SetLeft(ellipse, point.X);

                    polygonPoints.Add(point);

                    break;
                case "addText":
                    AddTextWin addTextWin = new AddTextWin();
                    addTextWin.ShowDialog();

                    AddTexts();
                    break;
                default:
                    break;
            }
        }
        private void AddTexts()
        {

            TextBlock TB = new TextBlock();
            TB.Text = textShow;
            TB.Foreground = new SolidColorBrush(fillColor);
            TB.FontSize = textFont;
            TB.Name = "addText";

            if (drawOrCancel == "draw")
            {
                drawOrCancel = "";

                AddCommand command = new AddCommand(MyCanvas, TB);
                undoList.Push(command);
                command.Execute();
            }

            Canvas.SetTop(TB, topLeft.Y);
            Canvas.SetLeft(TB, topLeft.X);
        }
        private void DrawEllipses()
        {
            GeometryGroup ellipses = new GeometryGroup();
            ellipses.Children.Add(new EllipseGeometry(topLeft, width, height));

            GeometryDrawing aGeometryDrawing = new GeometryDrawing();
            aGeometryDrawing.Geometry = ellipses;

            aGeometryDrawing.Brush = new SolidColorBrush(fillColor);

            aGeometryDrawing.Pen = new Pen(new SolidColorBrush(borderColor), borderThickness);


            DrawingImage geometryImage = new DrawingImage(aGeometryDrawing);


            geometryImage.Freeze();

            Image myEllipse = new Image();
            myEllipse.Source = geometryImage;
            myEllipse.Stretch = Stretch.None;
            myEllipse.HorizontalAlignment = HorizontalAlignment.Left;
            myEllipse.Name = "ellipse";

            myEllipse.MouseLeftButtonDown += new MouseButtonEventHandler(ChangeClick);

            if (drawOrCancel == "draw")
            {
                drawOrCancel = "";

                AddCommand command = new AddCommand(MyCanvas, myEllipse);
                undoList.Push(command);
                command.Execute();
            }

            Canvas.SetTop(myEllipse, topLeft.Y);
            Canvas.SetLeft(myEllipse, topLeft.X);

        }
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (chosenOption == "polygon")
            {
                if (polygonPoints.Count < 3)
                {
                    MessageBox.Show("Polygon must contain minimum three points!", "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    polygonPoints.Add(polygonPoints.First());
                    PolygonWin polygonWindow = new PolygonWin();
                    polygonWindow.ShowDialog();

                    DrawPolygons();
                    polygonPoints.Clear();
                }

            }

        }
        private void DrawPolygons()
        {
            int numOfPoints = polygonPoints.Count;
            for (int i = 0; i < numOfPoints - 1; i++)
            {
                MyCanvas.Children.RemoveAt(MyCanvas.Children.Count - 1);
            }
            Polygon myPolygon = new Polygon();
            foreach (System.Windows.Point point in polygonPoints)
            {
                myPolygon.Points.Add(point);
            }
            myPolygon.Stroke = new SolidColorBrush(borderColor);
            myPolygon.StrokeThickness = borderThickness;
            myPolygon.Fill = new SolidColorBrush(fillColor);
            myPolygon.MouseLeftButtonDown += new MouseButtonEventHandler(ChangeClick);

            AddCommand command = new AddCommand(MyCanvas, myPolygon);
            undoList.Push(command);
            command.Execute();

        }

        private void ChangeClick(object sender, MouseButtonEventArgs e)
        {
            chosenOption = "";

            HitTestResult target = VisualTreeHelper.HitTest(MyCanvas, e.GetPosition(MyCanvas));
            if (target.VisualHit.GetType() == typeof(Polygon))
            {
                Polygon polygon = (Polygon)e.Source;

                ChangeWin changeWindow = new ChangeWin();

                SolidColorBrush fill = (SolidColorBrush)polygon.Fill;
                SolidColorBrush border = (SolidColorBrush)polygon.Stroke;

                changeWindow.cmbColors.SelectedColor = fill.Color; // ?????????
                changeWindow.borderColors.SelectedColor = border.Color; // ?????????

                changeWindow.thicknessTb.Text = polygon.StrokeThickness.ToString();
                changeWindow.ShowDialog();

                if (drawOrCancel == "change")
                {
                    polygon.Fill = new SolidColorBrush(fillColor);
                    polygon.Stroke = new SolidColorBrush(borderColor);
                    polygon.StrokeThickness = borderThickness;
                }
            }
        }

        public int CalculateNew(double oldMax, double oldMin, double newMax, double newMin, double oldValue)
        {
            double oldRange = oldMax - oldMin;
            double newRange = newMax - newMin;
            return (int)Math.Round(((oldValue - oldMin) * newRange / oldRange) + newMin);
        }
        public bool MatrixAvailability(ref int x, ref int y)
        {
            bool done = false;
            if (Matrix[x, y] == false)
            {
                // nonDrawn = 3197 [100, 100]  || notDrawn = 2371 [320, 320]
                Matrix[x, y] = true;
                done = true;
            }
            else
            {
                //// nonDrawn =  1200 [100, 100]  || nonDrawn =  3 [250, 250]
                int direction;              // 0 - UP, 1 - RIGHT,  2 -DOWN, 3 -LEFT
                int i = 0;
                int j = 0;
                int stepCount = 1;
                bool sameStepCount = false;
                bool finished = false;

                while (!finished)
                {
                    for (direction = 0; direction < 4; direction++)
                    {
                        for (int k = 0; k < stepCount; k++)
                        {
                            switch (direction)
                            {
                                case (0): i--; break;
                                case (1): j++; break;
                                case (2): i++; break;
                                case (3): j--; break;
                            }
                            if (i == -3 && j == -3)
                            {
                                finished = true;
                                break;
                            }
                            if (x + j > (Matrix.GetLength(1) - 1) || x + j < 0)
                            {
                                j += stepCount - 1;
                                break;
                            }
                            if (y + i > (Matrix.GetLength(0) - 1) || y + i < 0)
                            {
                                i += stepCount - 1;
                                break;
                            }
                            if (Matrix[x + j, y + i] == false)
                            {
                                x += j;
                                y += i;
                                Matrix[x, y] = true;
                                done = true;
                                finished = true;
                                break;
                            }
                        }
                        if (finished)
                        {
                            break;
                        }
                        if (sameStepCount)
                        {
                            sameStepCount = false;
                            stepCount++;
                        }
                        else
                        {
                            sameStepCount = true;
                        }
                    }
                }
            }
            return done;
        }
        public bool ExistEntitiesWithId(long FirstEnd, long secondEnd)
        {
            bool existsFirsEnd = false, existSecondEnd = false;

            foreach (SubstationEntity entity in listSubstationEntity)
            {
                if (entity.Id == FirstEnd)
                    existsFirsEnd = true;
                else if (entity.Id == secondEnd)
                    existSecondEnd = true;

                if (existsFirsEnd && existSecondEnd)
                    return true;
            }

            foreach (NodeEntity entity in listNodeEntity)
            {
                if (entity.Id == FirstEnd)
                    existsFirsEnd = true;
                else if (entity.Id == secondEnd)
                    existSecondEnd = true;

                if (existsFirsEnd && existSecondEnd)
                    return true;
            }

            foreach (SwitchEntity entity in listSwitchEntity)
            {
                if (entity.Id == FirstEnd)
                    existsFirsEnd = true;
                else if (entity.Id == secondEnd)
                    existSecondEnd = true;

                if (existsFirsEnd && existSecondEnd)
                    return true;
            }

            return false;
        }
        private void MyCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            zoom += zoomSpeed * e.Delta; // Adjust zooming speed (e.Delta = Mouse spin value )
            if (zoom < zoomMin) // Limit Min Scale
                zoom = zoomMin;

            if (zoom > zoomMax) // Limit Max Scale
                zoom = zoomMax;

            System.Windows.Point mousePos = e.GetPosition(MyCanvas);

            if (zoom > 1)
            {
                MyCanvas.RenderTransform = new ScaleTransform(zoom, zoom, mousePos.X, mousePos.Y); // transform Canvas size from mouse position
            }
            else
            {
                MyCanvas.RenderTransform = new ScaleTransform(zoom, zoom); // transform Canvas size
            }
        }
    }
}
