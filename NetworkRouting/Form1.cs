using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NetworkRouting
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void clearAll()
        {
            startNodeIndex = -1;
            stopNodeIndex = -1;
            sourceNodeBox.Clear();
            sourceNodeBox.Refresh();
            targetNodeBox.Clear();
            targetNodeBox.Refresh();
            arrayTimeBox.Clear();
            arrayTimeBox.Refresh();
            heapTimeBox.Clear();
            heapTimeBox.Refresh();
            differenceBox.Clear();
            differenceBox.Refresh();
            pathCostBox.Clear();
            pathCostBox.Refresh();
            arrayCheckBox.Checked = false;
            arrayCheckBox.Refresh();
            return;
        }

        private void clearSome()
        {
            arrayTimeBox.Clear();
            arrayTimeBox.Refresh();
            heapTimeBox.Clear();
            heapTimeBox.Refresh();
            differenceBox.Clear();
            differenceBox.Refresh();
            pathCostBox.Clear();
            pathCostBox.Refresh();
            return;
        }

        private void generateButton_Click(object sender, EventArgs e)
        {
            int randomSeed = int.Parse(randomSeedBox.Text);
            int size = int.Parse(sizeBox.Text);

            Random rand = new Random(randomSeed);
            seedUsedLabel.Text = "Random Seed Used: " + randomSeed.ToString();

            clearAll();
            this.adjacencyList = generateAdjacencyList(size, rand);
            List<PointF> points = generatePoints(size, rand);
            resetImageToPoints(points);
            this.points = points;
        }

        // Generates the distance matrix.  Values of -1 indicate a missing edge.  Loopbacks are at a cost of 0.
        private const int MIN_WEIGHT = 1;
        private const int MAX_WEIGHT = 100;
        private const double PROBABILITY_OF_DELETION = 0.35;

        private const int NUMBER_OF_ADJACENT_POINTS = 3;

        private List<HashSet<int>> generateAdjacencyList(int size, Random rand)
        {
            List<HashSet<int>> adjacencyList = new List<HashSet<int>>();

            for (int i = 0; i < size; i++)
            {
                HashSet<int> adjacentPoints = new HashSet<int>();
                while (adjacentPoints.Count < 3)
                {
                    int point = rand.Next(size);
                    if (point != i) adjacentPoints.Add(point);
                }
                adjacencyList.Add(adjacentPoints);
            }

            return adjacencyList;
        }

        private List<PointF> generatePoints(int size, Random rand)
        {
            List<PointF> points = new List<PointF>();
            for (int i = 0; i < size; i++)
            {
                points.Add(new PointF((float) (rand.NextDouble() * pictureBox.Width), (float) (rand.NextDouble() * pictureBox.Height)));
            }
            return points;
        }

        private void resetImageToPoints(List<PointF> points)
        {
            pictureBox.Image = new Bitmap(pictureBox.Width, pictureBox.Height);
            Graphics graphics = Graphics.FromImage(pictureBox.Image);
            Pen pen;

            if (points.Count < 100)
                pen = new Pen(Color.Blue);
            else
                pen = new Pen(Color.LightBlue);
            foreach (PointF point in points)
            {
                graphics.DrawEllipse(pen, point.X, point.Y, 2, 2);
            }

            this.graphics = graphics;
            pictureBox.Invalidate();
        }

        // These variables are instantiated after the "Generate" button is clicked
        private List<PointF> points = new List<PointF>();
        private Graphics graphics;
        private List<HashSet<int>> adjacencyList;

        // Use this to generate paths (from start) to every node; then, just return the path of interest from start node to end node
        private void solveButton_Click(object sender, EventArgs e)
        {
            // This was the old entry point, but now it is just some form interface handling
            bool ready = true;

            if(startNodeIndex == -1)
            {
                sourceNodeBox.Focus();
                sourceNodeBox.BackColor = Color.Red;
                ready = false;
            }
            if(stopNodeIndex == -1)
            {
                if(!sourceNodeBox.Focused)
                    targetNodeBox.Focus();
                targetNodeBox.BackColor = Color.Red;
                ready = false;
            }
            if (points.Count > 0)
            {
                resetImageToPoints(points);
                paintStartStopPoints();
            }
            else
            {
                ready = false;
            }
            if(ready)
            {
                clearSome();

                System.Diagnostics.Stopwatch timerH = new System.Diagnostics.Stopwatch();
                timerH.Start();
                solveButton_Clicked(false);  
                timerH.Stop();
                heapTimeBox.Text = "" + timerH.Elapsed.TotalSeconds;

                if (arrayCheckBox.Checked) //do the same thing but solveButton_Clicked(true) for array implementation
                {
                    System.Diagnostics.Stopwatch timerA = new System.Diagnostics.Stopwatch();
                    timerA.Start();
                    solveButton_Clicked(true);
                    timerA.Stop();
                    arrayTimeBox.Text = "" + timerA.Elapsed.TotalSeconds;

                    differenceBox.Text = "" + timerA.Elapsed.TotalSeconds / timerH.Elapsed.TotalSeconds;
                }


            }
        }

        //O(|V|) * (O(OnKeyDecreased) + O(Deletemin))
        private void solveButton_Clicked(bool useArray)
        {
            //***************** Dijkstra's algorithm: *****************//
            double[] dist = new double[points.Count];
            int[] prev = new int[points.Count];
            //O(|V|)
            for(int i = 0; i < adjacencyList.Count; i++)
            {
                dist[i] = Int32.MaxValue;
                prev[i] = -1;
            }
            dist[startNodeIndex] = 0;

            IPriorityQ pq;
            if (useArray)
                pq = new ArrayQ();
            else
                pq = new HeapQ();
            pq.Makequeue(dist);
            //O(|V|) * (O(OnKeyDecreased) + O(Deletemin))
            while(pq.NotEmpty())
            {
                int u = pq.Deletemin();

                //O(3) * O(OnKeyDecreased)
                foreach (int v in adjacencyList.ElementAt(u))
                {
                    if(dist[v] > dist[u] + EuclDist(points.ElementAt(u), points.ElementAt(v)))
                    {
                        dist[v] = dist[u] + EuclDist(points.ElementAt(u), points.ElementAt(v));
                        prev[v] = u;
                        pq.OnKeyDecreased(v);
                    }
                }
            }
            //****************** End Dijkstra's **********************//

            //Get shortest path from prev. Draw it to screen
            if (prev[stopNodeIndex] == -1)
            {
                displayNoPath();
                return;
            }
            List<PointF> path = new List<PointF>();
            int curr = stopNodeIndex;
            //O(|V|)
            while(curr != -1)
            {
                path.Add(points.ElementAt(curr));
                curr = prev[curr];
            }

            drawPath(path);
            labelPath(path);
        }

        private double EuclDist(PointF pt1, PointF pt2)
        {
            return Math.Sqrt(Math.Pow((pt1.X - pt2.X),2) + Math.Pow((pt1.Y - pt2.Y), 2));
        }

        //O(1)
        private void displayNoPath()
        {
            Font drawFont = new Font("Arial", 14);
            SolidBrush drawBrush = new SolidBrush(Color.Black);
            PointF midpt = new PointF((points[startNodeIndex].X + points[stopNodeIndex].X) / 2, (points[startNodeIndex].Y + points[stopNodeIndex].Y) / 2);
            graphics.DrawString("No Path Exists", drawFont, drawBrush, midpt);
            pathCostBox.Text = "INFINITY";
        }

        //O(|V|)
        private void drawPath(List<PointF> pts)
        {
            // Create pen.
            Pen pen = new Pen(Color.Red, 1);

            //Draw lines to screen.
            if (pts.Count == 1) return;
            graphics.DrawLines(pen, pts.ToArray());
        }

        //O(|V|)
        private void labelPath(List<PointF> pts)
        {
            Font drawFont = new Font("Arial", 10);
            SolidBrush drawBrush = new SolidBrush(Color.Black);

            double totalDist = 0;
            for (int i = 0; i < pts.Count - 1; i++)
            {
                PointF midpt = new PointF((pts[i].X + pts[i + 1].X) / 2, (pts[i].Y + pts[i + 1].Y) / 2);
                double legDist = EuclDist(pts[i], pts[i + 1]);
                totalDist += legDist;
                String dist = "" + (int)legDist;
                graphics.DrawString(dist, drawFont, drawBrush, midpt);
            }
            pathCostBox.Text = "" + totalDist;
        }

        private Boolean startStopToggle = true;
        private int startNodeIndex = -1;
        private int stopNodeIndex = -1;
        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (points.Count > 0)
            {
                Point mouseDownLocation = new Point(e.X, e.Y);
                int index = ClosestPoint(points, mouseDownLocation);
                if (startStopToggle)
                {
                    startNodeIndex = index;
                    sourceNodeBox.ResetBackColor();
                    sourceNodeBox.Text = "" + index;
                }
                else
                {
                    stopNodeIndex = index;
                    targetNodeBox.ResetBackColor();
                    targetNodeBox.Text = "" + index;
                }
                resetImageToPoints(points);
                paintStartStopPoints();
            }
        }

        private void sourceNodeBox_Changed(object sender, EventArgs e)
        {
            if (points.Count > 0)
            {
                try{ startNodeIndex = int.Parse(sourceNodeBox.Text); }
                catch { startNodeIndex = -1; }
                if (startNodeIndex < 0 | startNodeIndex > points.Count-1)
                    startNodeIndex = -1;
                if(startNodeIndex != -1)
                {
                    sourceNodeBox.ResetBackColor();
                    resetImageToPoints(points);
                    paintStartStopPoints();
                    startStopToggle = !startStopToggle;
                }
            }
        }

        private void targetNodeBox_Changed(object sender, EventArgs e)
        {
            if (points.Count > 0)
            {
                try { stopNodeIndex = int.Parse(targetNodeBox.Text); }
                catch { stopNodeIndex = -1; }
                if (stopNodeIndex < 0 | stopNodeIndex > points.Count-1)
                    stopNodeIndex = -1;
                if(stopNodeIndex != -1)
                {
                    targetNodeBox.ResetBackColor();
                    resetImageToPoints(points);
                    paintStartStopPoints();
                    startStopToggle = !startStopToggle;
                }
            }
        }
        
        private void paintStartStopPoints()
        {
            if (startNodeIndex > -1)
            {
                Graphics graphics = Graphics.FromImage(pictureBox.Image);
                graphics.DrawEllipse(new Pen(Color.Green, 6), points[startNodeIndex].X, points[startNodeIndex].Y, 1, 1);
                this.graphics = graphics;
                pictureBox.Invalidate();
            }

            if (stopNodeIndex > -1)
            {
                Graphics graphics = Graphics.FromImage(pictureBox.Image);
                graphics.DrawEllipse(new Pen(Color.Red, 2), points[stopNodeIndex].X - 3, points[stopNodeIndex].Y - 3, 8, 8);
                this.graphics = graphics;
                pictureBox.Invalidate();
            }
        }

        private int ClosestPoint(List<PointF> points, Point mouseDownLocation)
        {
            double minDist = double.MaxValue;
            int minIndex = 0;

            for (int i = 0; i < points.Count; i++)
            {
                double dist = Math.Sqrt(Math.Pow(points[i].X-mouseDownLocation.X,2) + Math.Pow(points[i].Y - mouseDownLocation.Y,2));
                if (dist < minDist)
                {
                    minIndex = i;
                    minDist = dist;
                }
            }

            return minIndex;
        }
    }
}
