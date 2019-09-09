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
        public static GeneticAlgorithm GA;

        List<float> prosekFCTrenutnePopulacije = new List<float>();
        List<List<float>> prosekFCSvihPopulacija = new List<List<float>>();

        int InPopulationSize, InVelicinaTurnira, InBrojElitnihJedinki, InBrojIteracija;
        float InProcenatMutacijePopulacije, InProcenatMutacijeJedinke;
        bool InruletskaSelekcija, InfiksanBrojIteracija, InOnePoint;
        float InMapStartX, InMapStartY;

        float GraphicWidth, GraphicHeight;
        List<TerrainBlock> TerrainSquares = new List<TerrainBlock>();

        List<Tuple<Tuple<float, float>, Tuple<float, float>, int>> TerrainLines;//=new List<Tuple<float, float, int>>();
        List<SpaceShip> currentPopulation = new List<SpaceShip>();
        int GACurrentIteration = 0;

        private bool IsPointInsideSquare(Rectangle rect, PointF point)
        {
            return rect.Contains((int)point.X, (int)point.Y);
        }

        void loadMap(string filepath= @"..\..\Maps\matrix_15x27_1.txt")
        {
            TerrainSquares.Clear();
            int SquareWidth = 0;

            //var path = Path.Combine(Directory.GetCurrentDirectory(), @"Maps\matrix_14x26_1.txt");

            string absolutePath = filepath;

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
                    if (terrainMatrix[i, j] == -1)
                    {
                        InMapStartX = (float)(j * SquareWidth + SquareWidth / 2.0);
                        InMapStartY = (float)(i * SquareWidth + SquareWidth / 2.0);
                    }

                    j++;
                }
                i++;
            }
            pictureBox1.Refresh();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GraphicWidth = pictureBox1.Width;
            GraphicHeight = pictureBox1.Height;

            loadMap();
            //currSpaceship = new SpaceShip();
            pictureBox1.BackColor = Color.Black;

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            foreach (SpaceShip item in currentPopulation)
            {
                if (!item.AIAlive)
                    continue;

                item.AIMovement();
                item.move();

                bool linesIntersect = false;

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

                if (!linesIntersect && (item.MainLineB.X < 0 || item.MainLineB.X > pictureBox1.Width || item.MainLineB.Y < 0 || item.MainLineB.Y > pictureBox1.Height))
                {
                    item.AIAlive = false;
                    SpaceShip.AliveNumber--;
                    continue;
                }
            }

            float sumaFunkcijaCilja = 0, prosekFunkcijaCilja;
            if (SpaceShip.AliveNumber <= 0)
            {
                //izracunavanje proseka funkcije cilja svih jedinki
                foreach (SpaceShip item in currentPopulation)
                {
                    sumaFunkcijaCilja += item.GAFitnessFunction;
                }
                prosekFunkcijaCilja = sumaFunkcijaCilja / currentPopulation.Count;
                prosekFCTrenutnePopulacije.Add(prosekFunkcijaCilja);
                pictureBox2.Refresh();

                if (GACurrentIteration < GeneticAlgorithm.MaxIterations)
                {

                    List<SpaceShip> selected = GeneticAlgorithm.Selection(currentPopulation);
                    List<SpaceShip> newPopulation = GeneticAlgorithm.CreateGeneration(selected);
                    currentPopulation = newPopulation;
                    SpaceShip.AliveNumber = currentPopulation.Count();
                    GACurrentIteration++;
                }
                else
                {
                    prosekFCSvihPopulacija.Add(prosekFCTrenutnePopulacije);
                    prosekFCTrenutnePopulacije = new List<float>();
                    pictureBox3.Refresh();
                    timer1.Stop();

                }
            }
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
            }

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            FunctionUtil.drawFunction(pictureBox2.Width, pictureBox2.Height, e.Graphics, new List<List<float>> { prosekFCTrenutnePopulacije },InBrojIteracija);
        }

        private void pictureBox3_Paint(object sender, PaintEventArgs e)
        {
            FunctionUtil.drawFunction(pictureBox3.Width, pictureBox3.Height, e.Graphics, prosekFCSvihPopulacija, InBrojIteracija);

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
            InOnePoint = radioButton6.Checked;

            if (!Int32.TryParse(textBox5.Text, out InPopulationSize) || InPopulationSize <= 0 || InPopulationSize > 4000)
            {
                MessageBox.Show("Velicina populacije nije broj izmedju 0 i 4000");
                return;
            }

            if (!InruletskaSelekcija && (!Int32.TryParse(textBox6.Text, out InVelicinaTurnira) || InVelicinaTurnira <= 5 || InVelicinaTurnira > 20))
            {
                MessageBox.Show("Velicina turnira nije broj izmedju 5 i 20");
                return;
            }
            if (!float.TryParse(textBox1.Text, out InProcenatMutacijePopulacije) || InProcenatMutacijePopulacije <= 0 || InProcenatMutacijePopulacije > 40)
            {
                MessageBox.Show("Procenat mutacije populacije nije broj izmedju 0 i 40");
                return;
            }
            if (!float.TryParse(textBox2.Text, out InProcenatMutacijeJedinke) || InProcenatMutacijeJedinke <= 0 || InProcenatMutacijeJedinke > 40)
            {
                MessageBox.Show("Procenat mutacije jedinke nije broj izmedju 0 i 40");
                return;
            }
            if (!Int32.TryParse(textBox4.Text, out InBrojElitnihJedinki) || InBrojElitnihJedinki <= 0 || InBrojElitnihJedinki > 5000)
            {
                MessageBox.Show("Broj elitnih jedinki nije broj izmedju 0 i 4000");
                return;
            }
            if (InfiksanBrojIteracija && (!Int32.TryParse(textBox7.Text, out InBrojIteracija) || InBrojIteracija <= 0 || InBrojIteracija > 5000))
            {
                MessageBox.Show("Broj iteracija nije broj izmedju 0 i 4000");
                return;
            }

            prosekFCTrenutnePopulacije.Clear();
            GeneticAlgorithm.SetParameters(InPopulationSize, 100, InBrojIteracija, InProcenatMutacijeJedinke, InruletskaSelekcija==true ? "roulette" : "tournament", "onepoint",InMapStartX,InMapStartY);
            currentPopulation = GeneticAlgorithm.CreateInitialGeneration();
            SpaceShip.AliveNumber = GeneticAlgorithm.GenerationSize;
            GACurrentIteration = 0;
            timer1.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";
            openFileDialog1.Title = "Učitavanje mape";
            DialogResult putanjaFajla = openFileDialog1.ShowDialog();
            if (!String.IsNullOrEmpty(openFileDialog1.FileName))
            {
                loadMap(openFileDialog1.FileName);
            }
        }

        private void Form1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {

        }
    }
}
