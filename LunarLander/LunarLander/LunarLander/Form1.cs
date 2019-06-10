using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LunarLander
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        float GraphicWidth, GraphicHeight;

        private void FindIntersection(
            PointF p1, PointF p2, PointF p3, PointF p4,
            out bool lines_intersect, out bool segments_intersect,
            out PointF intersection)
        {
            // Get the segments' parameters.
            float dx12 = p2.X - p1.X;
            float dy12 = p2.Y - p1.Y;
            float dx34 = p4.X - p3.X;
            float dy34 = p4.Y - p3.Y;

            // Solve for t1 and t2
            float denominator = (dy12 * dx34 - dx12 * dy34);

            float t1 =
                ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34)
                    / denominator;
            if (float.IsInfinity(t1))
            {
                // The lines are parallel (or close enough to it).
                lines_intersect = false;
                segments_intersect = false;
                intersection = new PointF(float.NaN, float.NaN);
                //close_p1 = new PointF(float.NaN, float.NaN);
                //close_p2 = new PointF(float.NaN, float.NaN);
                return;
            }
            lines_intersect = true;

            float t2 =
                ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12)
                    / -denominator;

            // Find the point of intersection.
            intersection = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);

            // The segments intersect if t1 and t2 are between 0 and 1.
            segments_intersect =
                ((t1 >= 0) && (t1 <= 1) &&
                 (t2 >= 0) && (t2 <= 1));

            // Find the closest points on the segments.
            if (t1 < 0)
            {
                t1 = 0;
            }
            else if (t1 > 1)
            {
                t1 = 1;
            }

            if (t2 < 0)
            {
                t2 = 0;
            }
            else if (t2 > 1)
            {
                t2 = 1;
            }

            //close_p1 = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);
            //close_p2 = new PointF(p3.X + dx34 * t2, p3.Y + dy34 * t2);
        }


        class SpaceShip
        {
            public static int AliveNumber { get; set; } = 0;

            List<Tuple<string, int>> run;
            int currAction = -1;
            string currActionCode;
            int currActionTicksLeft = 0;
            bool AIDone = false;


            float currX, currY;
            public float xM;
            public float yM;
            public float angle;
            bool thrustersState = false;

            public SpaceShip(List<Tuple<string, int>> inRun)
            {
                xM = 2f;
                yM = 0f;
                currX = 30;
                currY = 30;
                run = inRun;
            }


            public PointF MainLineA { get; set; }
            public PointF MainLineB { get; set; }


            public PointF BottomLineA { get; set; }
            public PointF BottomLineB { get; set; }


            public bool AIAlive { get; set; } = true;

            public void AIMovement()
            {
                if (currActionTicksLeft == 0)
                {
                    currAction++;
                    if (run.Count() > currAction)
                    {
                        currActionCode = run[currAction].Item1;
                        currActionTicksLeft = run[currAction].Item2;
                    }
                    else
                    {
                        AIDone = true;
                    }
                }
                if (!AIDone)
                {
                    switch (currActionCode)
                    {
                        case "RL":
                            decreaseAngle();
                            break;
                        case "RR":
                            increaseAngle();
                            break;
                        case "TH":
                            applyThrusters();
                            break;
                        case "NT":
                            break;
                    }
                    currActionTicksLeft--;
                }
            }

            public double ConvertToRadians(double angleIn)
            {
                return (Math.PI / 180) * angleIn;
            }

            public void increaseAngle()
            {
                this.angle += 2;
            }

            public void decreaseAngle()
            {
                this.angle -= 2;
            }

            public void move()
            {
                //apply gravity
                if (yM < 2.0)
                    yM += 0.007f;

                //apply thrusters
                if (thrustersState)
                {
                    xM += (float)Math.Sin(ConvertToRadians(angle)) * 0.09f;//, currY - (float)Math.Cos(ConvertToRadians(angle)) * 50
                    yM -= (float)Math.Cos(ConvertToRadians(angle)) * 0.09f;
                }

                //apply force movement
                currX += xM;
                currY += yM;


                MainLineA = new PointF(currX + (float)Math.Sin(ConvertToRadians(angle)) * 10, currY - (float)Math.Cos(ConvertToRadians(angle)) * 10);
                MainLineB = new PointF(currX, currY);
            }
            public void draw(Graphics g)
            {
                PointF ptL = new PointF(currX - 5, currY + 2);
                PointF ptR = new PointF(currX + 5, currY + 2);
                PointF ptU = new PointF(currX, currY - 8);
                SolidBrush sb = new SolidBrush(Color.White);
                PointF[] points = { ptL, ptU, ptR };
                g.FillPolygon(sb, points);

                PointF ptAngle = new PointF(currX + (float)Math.Sin(ConvertToRadians(angle)) * 10, currY - (float)Math.Cos(ConvertToRadians(angle)) * 10);
                Pen pencile = new Pen(Color.Red);
                g.DrawLine(pencile, ptAngle, new PointF(currX, currY));
                thrustersState = false;

            }

            public void applyThrusters()
            {
                thrustersState = true;
            }
        }

        List<Tuple<Tuple<float, float>, Tuple<float, float>, int>> TerrainLines;//=new List<Tuple<float, float, int>>();
        List<SpaceShip> currentPopulation = new List<SpaceShip>();
        SpaceShip currSpaceship;

        int EliteUnits = 100;
        int PopulationSize = 2000;

        private void Form1_Load(object sender, EventArgs e)
        {
            GraphicWidth = pictureBox1.Width;
            GraphicHeight = pictureBox1.Height;

            TerrainLines = new List<Tuple<Tuple<float, float>, Tuple<float, float>, int>>
            {
                new Tuple<Tuple<float, float>, Tuple<float, float>, int>(new Tuple<float, float>(0,680),new Tuple<float, float>(100,680),10),
                new Tuple<Tuple<float, float>, Tuple<float, float>, int>(new Tuple<float, float>(100,680),new Tuple<float, float>(200,680),20),
                new Tuple<Tuple<float, float>, Tuple<float, float>, int>(new Tuple<float, float>(200,680),new Tuple<float, float>(300,680),30),
                new Tuple<Tuple<float, float>, Tuple<float, float>, int>(new Tuple<float, float>(300,680),new Tuple<float, float>(400,680),40),
                new Tuple<Tuple<float, float>, Tuple<float, float>, int>(new Tuple<float, float>(400,680),new Tuple<float, float>(500,680),45),
                new Tuple<Tuple<float, float>, Tuple<float, float>, int>(new Tuple<float, float>(500,680),new Tuple<float, float>(500,250),55),
                new Tuple<Tuple<float, float>, Tuple<float, float>, int>(new Tuple<float, float>(500,250),new Tuple<float, float>(580,250),60),
                new Tuple<Tuple<float, float>, Tuple<float, float>, int>(new Tuple<float, float>(580,250),new Tuple<float, float>(580,680),80),
                new Tuple<Tuple<float, float>, Tuple<float, float>, int>(new Tuple<float, float>(580,680),new Tuple<float, float>(1300,680),100),

                new Tuple<Tuple<float, float>, Tuple<float, float>, int>(new Tuple<float, float>(750,0),new Tuple<float, float>(750,50),45),
                new Tuple<Tuple<float, float>, Tuple<float, float>, int>(new Tuple<float, float>(750,50),new Tuple<float, float>(850,50),55),
                new Tuple<Tuple<float, float>, Tuple<float, float>, int>(new Tuple<float, float>(850,50),new Tuple<float, float>(850,450),60),
                new Tuple<Tuple<float, float>, Tuple<float, float>, int>(new Tuple<float, float>(850,450),new Tuple<float, float>(900,450),80),
                new Tuple<Tuple<float, float>, Tuple<float, float>, int>(new Tuple<float, float>(900,450),new Tuple<float, float>(900,50),100),
                new Tuple<Tuple<float, float>, Tuple<float, float>, int>(new Tuple<float, float>(900,50),new Tuple<float, float>(1300,50),100),

                new Tuple<Tuple<float, float>, Tuple<float, float>, int>(new Tuple<float, float>(750,0),new Tuple<float, float>(1300,0),45),
                new Tuple<Tuple<float, float>, Tuple<float, float>, int>(new Tuple<float, float>(1300,0),new Tuple<float, float>(1300,700),45),
                //new Tuple<Tuple<float, float>, Tuple<float, float>, int>(new Tuple<float, float>(1300,0),new Tuple<float, float>(1300,700),45),


            };




            //currSpaceship = new SpaceShip();
            pictureBox1.BackColor = Color.Black;
            timer1.Start();

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            foreach (SpaceShip item in currentPopulation)
            {
                if (!item.AIAlive)
                    continue;

                item.AIMovement();
                item.move();

                bool linesIntersect = false ;

                foreach (var line in TerrainLines)
                {
                    float Ax = line.Item1.Item1;
                    float Ay = line.Item1.Item2;
                    float Bx = line.Item2.Item1;
                    float By = line.Item2.Item2;
                    bool segmentIntersect;
                    PointF segmentPointIntersect;
                    FindIntersection(item.MainLineA, item.MainLineB, new PointF(Ax, Ay), new PointF(Bx, By), out linesIntersect, out segmentIntersect, out segmentPointIntersect);
                    if (segmentIntersect)
                    {
                        linesIntersect = true;
                        item.AIAlive = false;
                        SpaceShip.AliveNumber--;
                        break; 
                    }
                }

                if (!linesIntersect &&(item.MainLineA.X < 0 || item.MainLineA.X > pictureBox1.Width || item.MainLineA.Y < 0 || item.MainLineA.Y > pictureBox1.Height))
                {
                    item.AIAlive = false;
                    SpaceShip.AliveNumber--;
                    continue;
                }
            }

            if (SpaceShip.AliveNumber <= 0)
            {
                //Ocena populacije

                //Sledeca populacija

                //Ukrstanje

                //Mutacija

                //Prebacivanje elitnih

                Random r = new Random();
                for (int i = 0; i < PopulationSize; i++)
                {
                    List<Tuple<string, int>> currRun = new List<Tuple<string, int>>();
                    for (int j = 0; j < 20; j++)
                    {
                        int actionR = r.Next(4);
                        if (actionR == 0)
                            currRun.Add(new Tuple<string, int>("NT", r.Next(100, 400)));
                        if (actionR == 1)
                            currRun.Add(new Tuple<string, int>("RL", r.Next(10, 25)));
                        if (actionR == 2)
                            currRun.Add(new Tuple<string, int>("RR", r.Next(10, 25)));
                        if (actionR == 3)
                            currRun.Add(new Tuple<string, int>("TH", r.Next(10, 25)));
                    }
                    currentPopulation.Add(new SpaceShip(currRun));
                }
                SpaceShip.AliveNumber = PopulationSize;
            }

            //currSpaceship.AIMovement();
            pictureBox1.Refresh();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            foreach (SpaceShip item in currentPopulation)
            {
                if (item.AIAlive)
                    item.draw(g);
            }

            foreach (var line in TerrainLines)
            {
                float Ax = line.Item1.Item1;
                float Ay = line.Item1.Item2;
                float Bx = line.Item2.Item1;
                float By = line.Item2.Item2;
                int fitness = line.Item3;
                Pen p = new Pen(Color.FromArgb(255 - (fitness / 100) * 255, 244, 8));
                g.DrawLine(p, Ax, Ay, Bx, By);
            }

            Font drawFont = new Font("Arial", 16);
            SolidBrush sb = new SolidBrush(Color.White);
            g.DrawString("Alive units:" + SpaceShip.AliveNumber, drawFont, sb, 1000, 500);

        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Form1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {

        }
    }
}
