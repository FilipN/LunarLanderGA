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

        float GraphicWidth, GraphicHeight;


        private bool IsPointInsideSquare(Rectangle rect, PointF point)
        {
            return rect.Contains((int)point.X, (int)point.Y);
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

        private void Form1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {

        }
    }
}
