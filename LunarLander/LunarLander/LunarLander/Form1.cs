using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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

        int InPopulationSize, InVelicinaTurnira, InBrojElitnihJedinki, InBrojIteracija;
        float InProcenatMutacijePopulacije, InProcenatMutacijeJedinke;
        string InmapPathFile = "";
        bool InruletskaSelekcija, InfiksanBrojIteracija;

        float GraphicWidth, GraphicHeight;


        private bool IsPointInsideSquare(Rectangle rect, PointF point)
        {
            return rect.Contains((int)point.X, (int)point.Y);
        }


        class TerrainBlock
        {
            public Rectangle Bounds{get;set;}
            public int Fitness { get; set; }
            public int NumberOfAIEnds { get; set; } = 0;
            public TerrainBlock(Rectangle boundsIn,int fitnessIn)
            {
                Bounds = boundsIn;
                Fitness = fitnessIn;
            }
            public void draw(Graphics g)
            {
                SolidBrush sb = new SolidBrush(Color.FromArgb(255,255- (int)((Fitness/100.0f)*255.0f), (int)((Fitness / 100.0f) * 255),0));
                if (Fitness == 1)
                    sb = new SolidBrush(Color.LightSlateGray);
                g.FillRectangle(sb, Bounds);


                Font drawFont = new Font("Arial", 8);
                SolidBrush fn = new SolidBrush(Color.White);
                g.DrawString(NumberOfAIEnds.ToString(), drawFont, fn, Bounds.X, Bounds.Y);
            }

        }

        class GeneticAlgorithm
        {
            public static int GenerationSize = 1000;
            public static int ReproductionSize = 200;
            public static int MaxIterations = 1000;
            public static double MutationRate = 0.1;


            private static double GetRandomDouble(Random random, double min, double max)
            {
                return min + (random.NextDouble() * (max - min));
            }

            


            private static SpaceShip RouletteSelction(List<SpaceShip> population)
            {
                List<SpaceShip> selected = new List<SpaceShip>();

                double totalFitness = population.Sum(x => x.GAFitnessFunction);
                var random = new Random();
                double selectedValue = GetRandomDouble(random, 0, totalFitness);

                double currentSum = 0;

                for (int i = 0; i < GenerationSize; i++)
                {
                    currentSum += population[i].GAFitnessFunction;
                    //vraca se prva jedinka koja ispuni uslov
                    if (currentSum > selectedValue)
                        return population[i];
                }

                return null;
            }

            //jednopoziciono ukrstanje sa nasumicnom tackom
            private static List<SpaceShip> Crossover(SpaceShip parent1 , SpaceShip parent2)
            {

                //public List<Tuple<string, int>> run
                List<SpaceShip> children = new List<SpaceShip>(2);
                Random r = new Random();
                int breakPoint = r.Next(1, SpaceShip.ChromosomeSize);
                
                List<Tuple<string, int>> child1Run = parent1.run.Take(breakPoint).ToList().Concat(parent2.run.Skip(breakPoint).ToList()).ToList();
                List<Tuple<string, int>> child2Run = parent2.run.Take(breakPoint).ToList().Concat(parent1.run.Skip(breakPoint).ToList()).ToList();
                SpaceShip child1 = new SpaceShip(child1Run);
                SpaceShip child2 = new SpaceShip(child2Run);

                children.Add(child1);
                children.Add(child2);
                return children;
            }


            private static SpaceShip Mutate(SpaceShip ship)
            {
                Random r = new Random();
                double randomValue = r.NextDouble();
                if(randomValue< MutationRate)
                {
                    int randomIndex = r.Next(SpaceShip.ChromosomeSize);
                    int actionR = r.Next(4);
                    if (actionR == 0)
                    {
                        Tuple<string, int> newRun = new Tuple<string, int>("NT", r.Next(100, 400));
                        ship.run[randomIndex] = newRun;
                    }
                    if (actionR == 1)
                    {
                        Tuple<string, int> newRun = new Tuple<string, int>("RL", r.Next(10, 25));
                        ship.run[randomIndex] = newRun;
                    }
                        
                    if (actionR == 2)
                    {
                        Tuple<string, int> newRun = new Tuple<string, int>("RR", r.Next(10, 25));
                        ship.run[randomIndex] = newRun;
                    }
                    if (actionR == 3)
                    {
                        Tuple<string, int> newRun = new Tuple<string, int>("TH", r.Next(10, 25));
                        ship.run[randomIndex] = newRun;
                    }
                }
                return ship;
            }

            //kreiranje nove generacije od selektovanih jedinki
            private static List<SpaceShip> CreateGeneration(List<SpaceShip> selectedPopulation)
            {
                int generationSize = 0;
                List<SpaceShip> newGeneration = new List<SpaceShip>();
                Random r = new Random();
                while(generationSize<GenerationSize)
                {
                    List<SpaceShip> parents = selectedPopulation.OrderBy(x => r.Next()).Take(2).ToList();
                    SpaceShip p1 = parents[0]; SpaceShip p2 = parents[1];


                    //dobijaju se deca ukrstanjem
                    List<SpaceShip> children = Crossover(p1, p2);
                    SpaceShip c1 = children[0]; SpaceShip c2 = children[1];

                    //deca mutiraju
                    c1 = Mutate(c1); c2 = Mutate(c2);

                    //dodaju se u generaciju
                    newGeneration.Add(c1); newGeneration.Add(c2);
                    generationSize += 2;
                }
                return newGeneration;
            }
            
        }

        class SpaceShip : IComparable
        {

            public static int ChromosomeSize = 20;

            private float GAFinalVerticalSpeed { get; set; }
            private float GAFinalHorizontalSpeed { get; set; }
            private float GALandingBlock { get; set; } = 0;
            public float GAFitnessFunction { get; set; }

            public void StopShip(float blockFitness)
            {
                GAFinalHorizontalSpeed = Math.Abs(xM);
                GAFinalVerticalSpeed = Math.Abs(yM);
                GALandingBlock = blockFitness;

                float WGAFinalHorizontalSpeed = GAFinalHorizontalSpeed;
                if (GAFinalHorizontalSpeed > 1)
                    WGAFinalHorizontalSpeed = 10.0f / GAFinalHorizontalSpeed;
                else
                    WGAFinalHorizontalSpeed = 30;

                float WGAFinalVerticalSpeed = GAFinalVerticalSpeed;
                if (GAFinalVerticalSpeed > 1)
                    WGAFinalVerticalSpeed = 10.0f / GAFinalVerticalSpeed;
                else
                    WGAFinalVerticalSpeed = 30;

                GAFitnessFunction = GALandingBlock*1.5f+WGAFinalHorizontalSpeed+WGAFinalVerticalSpeed;// + 15.0f / GAFinalHorizontalSpeed + 10.0f / GAFinalVerticalSpeed;
            }

            public static int AliveNumber { get; set; } = 0;
            public static float ShipR { get; set; } = 10;

            public List<Tuple<string, int>> run { get; set; }
            int currAction = -1;
            string currActionCode;
            int currActionTicksLeft = 0;
            bool AIDone = false;


            float currX, currY;
            public float xM;
            public float yM;
            public float angle;
            bool thrustersState = false;
            bool elite;

            public SpaceShip(List<Tuple<string, int>> inRun, bool inElite = false)
            {
                xM = 1f;
                yM = 0f;
                currX = 30;
                currY = 30;
                run = inRun;
                elite = inElite;
            }


            public PointF MainLineA { get; set; }
            public PointF MainLineB { get; set; }


            public PointF BottomLineA { get; set; }
            public PointF BottomLineB { get; set; }
            public PointF ThrusterPoint { get; set; }


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

                ThrusterPoint = new PointF(currX - (float)Math.Sin(ConvertToRadians(angle)) * 5, currY + (float)Math.Cos(ConvertToRadians(angle)) * 5);
                MainLineA = new PointF(currX + (float)Math.Sin(ConvertToRadians(angle)) * 10, currY - (float)Math.Cos(ConvertToRadians(angle)) * 10);
                MainLineB = new PointF(currX, currY);

            }
            public void draw(Graphics g)
            {

                Color shipColor = Color.LightGray;
                if (elite)
                    shipColor = Color.GreenYellow;

                SolidBrush sb = new SolidBrush(shipColor);
                SolidBrush sbWindow = new SolidBrush(Color.SkyBlue);
                SolidBrush sbThruster = new SolidBrush(Color.Orange);

                if(thrustersState)
                    g.FillEllipse(sbThruster, new RectangleF(ThrusterPoint.X - SpaceShip.ShipR/ 4.0f, ThrusterPoint.Y - SpaceShip.ShipR / 4.0f, SpaceShip.ShipR /2, SpaceShip.ShipR/2 ));
                g.FillEllipse(sb, new RectangleF(currX - SpaceShip.ShipR / 2.0f, currY - SpaceShip.ShipR / 2.0f, SpaceShip.ShipR, SpaceShip.ShipR));
                g.FillEllipse(sbWindow, new RectangleF(currX - SpaceShip.ShipR / 4.0f, currY - SpaceShip.ShipR / 4.0f, SpaceShip.ShipR/2, SpaceShip.ShipR/2));


                PointF ptAngle = new PointF(currX + (float)Math.Sin(ConvertToRadians(angle)) * 10, currY - (float)Math.Cos(ConvertToRadians(angle)) * 10);
                Pen pencile = new Pen(Color.Purple);
                g.DrawLine(pencile, ptAngle, new PointF(currX, currY));
                thrustersState = false;

            }

            public void applyThrusters()
            {
                thrustersState = true;
            }

            public int CompareTo(object obj)
            {
                if (obj == null)
                    return 1;

                SpaceShip sp = obj as SpaceShip;
                return GAFitnessFunction.CompareTo(GAFitnessFunction);
            }
        }

        List<TerrainBlock> TerrainSquares = new List<TerrainBlock>();

        List<Tuple<Tuple<float, float>, Tuple<float, float>, int>> TerrainLines;//=new List<Tuple<float, float, int>>();
        List<SpaceShip> currentPopulation = new List<SpaceShip>();
        SpaceShip currSpaceship;

        int EliteUnits = 50;
        int PopulationSize = 1000;

        private void Form1_Load(object sender, EventArgs e)
        {
            GraphicWidth = pictureBox1.Width;
            GraphicHeight = pictureBox1.Height;
            int SquareWidth = 0;

            var path = Path.Combine(Directory.GetCurrentDirectory(), @"Maps\matrix_14x26.txt");

            string absolutePath = @"..\..\Maps\matrix_14x26.txt";

            String input = File.ReadAllText(absolutePath);
            int i = 0, j = 0;
            int r = 0, c = 0;

            string[] rows = input.Split('\n');

            r = int.Parse(rows[0].Trim());
            c = int.Parse(rows[1].Trim());
            SquareWidth = (int)(GraphicWidth / c);

            int[,] terrainMatrix = new int[r, c];

            foreach (var row in rows.Skip(2))
            {
                j = 0;
                foreach (var col in row.Trim().Split(' '))
                {
                    terrainMatrix[i, j] = int.Parse(col.Trim());
                    if (terrainMatrix[i, j] > 0)
                        TerrainSquares.Add(new TerrainBlock(new Rectangle(j * SquareWidth, i * SquareWidth, SquareWidth, SquareWidth), terrainMatrix[i, j]));
                    j++;
                }
                i++;
            }






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

                foreach (TerrainBlock block in TerrainSquares)
                {
                    if (IsPointInsideSquare(block.Bounds, item.MainLineB))
                    {
                        block.NumberOfAIEnds++;
                        item.AIAlive = false;
                        item.StopShip(block.Fitness);
                        linesIntersect = true;
                        SpaceShip.AliveNumber--;
                        break;
                    }
                }

                if (!linesIntersect &&(item.MainLineB.X < 0 || item.MainLineB.X > pictureBox1.Width || item.MainLineB.Y < 0 || item.MainLineB.Y > pictureBox1.Height))
                {
                    item.AIAlive = false;
                    SpaceShip.AliveNumber--;
                    continue;
                }
            }

            if (SpaceShip.AliveNumber <= 0)
            {

                /*List<SpaceShip> oldPopulation = currentPopulation;
                if (oldPopulation.Count > 0)
                {
                    oldPopulation = oldPopulation.OrderByDescending(it=>it.GAFitnessFunction).ToList<SpaceShip>();
                    currentPopulation = new List<SpaceShip>();

                    for (int i = 0; i < EliteUnits; i++)
                    {
                        currentPopulation.Add(new SpaceShip(oldPopulation[i].run, true));
                    }
                } */


                //Prebacivanje elitnih jedinki



                //Sledeca populacija ruletska selekcija




                //Ukrstanje

                //Mutacija


                Random r = new Random();
                while(currentPopulation.Count< PopulationSize)
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


            Font drawFont = new Font("Arial", 16);
            SolidBrush sb = new SolidBrush(Color.DarkGray);
            g.DrawString("Alive units:" + SpaceShip.AliveNumber, drawFont, sb, 1000, 500);

            //crtanje pravougaonika na osnovu matrice
            foreach (TerrainBlock block in TerrainSquares)
            {
                block.draw(g);
                //g.FillRectangle(sb, rect);
            }

        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            InfiksanBrojIteracija = radioButton3.Checked;
            InruletskaSelekcija = radioButton1.Checked;

            if (!Int32.TryParse(textBox5.Text,out InPopulationSize) || InPopulationSize<=0 || InPopulationSize>4000)
                MessageBox.Show("Velicina populacije nije broj izmedju 0 i 4000");

            if (!InruletskaSelekcija && (!Int32.TryParse(textBox6.Text, out InVelicinaTurnira) || InVelicinaTurnira <= 5 || InVelicinaTurnira > 20))
                MessageBox.Show("Velicina turnira nije broj izmedju 5 i 20");

            if (!float.TryParse(textBox1.Text, out InProcenatMutacijePopulacije) || InProcenatMutacijePopulacije <= 0 || InProcenatMutacijePopulacije > 40)
                MessageBox.Show("Procenat mutacije populacije nije broj izmedju 0 i 40");

            if (!float.TryParse(textBox2.Text, out InProcenatMutacijeJedinke) || InProcenatMutacijeJedinke <= 0 || InProcenatMutacijeJedinke > 40)
                MessageBox.Show("Procenat mutacije jedinke nije broj izmedju 0 i 40");

            if (!Int32.TryParse(textBox4.Text, out InBrojElitnihJedinki) || InBrojElitnihJedinki <= 0 || InBrojElitnihJedinki > 5000)
                MessageBox.Show("Broj elitnih jedinki nije broj izmedju 0 i 4000");

            if (InfiksanBrojIteracija && (!Int32.TryParse(textBox7.Text, out InBrojIteracija) || InBrojIteracija <= 0 || InBrojIteracija > 5000))
                MessageBox.Show("Broj iteracija nije broj izmedju 0 i 4000");
            InmapPathFile = openFileDialog1.FileName;


        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";
            openFileDialog1.Title = "Učitavanje mape";
            DialogResult putanjaFajla = openFileDialog1.ShowDialog();
        }

        private void Form1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {

        }
    }
}
