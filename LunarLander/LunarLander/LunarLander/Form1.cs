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

        public class RunActions
        {
            public List<Tuple<string, int>> Actions;
        }



        class SpaceShip
        {
            List<Tuple<string, int>> run;
            int currAction=-1;
            string currActionCode;
            int currActionTicksLeft=0;
            bool AIDone = false;

            float currX, currY;
            public float xM;
            public float yM;
            public float angle;
            bool thrustersState = false;

            public SpaceShip(List<Tuple<string,int>> inRun)
            {
                xM = 0f;
                yM = 0f;
                currX = 300;
                currY = 300;
                run = inRun;
                /*run = new List<Tuple<string, int>>
                {
                    new Tuple<string, int>("RL",20),
                    new Tuple<string, int>("TH",20),
                    new Tuple<string, int>("NT",50),
                    new Tuple<string, int>("RR",50),
                    new Tuple<string, int>("TH",20),
                    new Tuple<string, int>("RL",30),
                    new Tuple<string, int>("NT", 500),
                    new Tuple<string, int>("TH",27)

                };*/
            }

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
                this.angle+=2;
            }

            public void decreaseAngle()
            {
                this.angle-=2;
            }


            public void move()
            {
                //apply gravity
                if(yM<2.0)
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
            }
            public void draw(Graphics g)
            {
                PointF ptL = new PointF(currX - 5, currY + 2);
                PointF ptR = new PointF(currX + 5, currY + 2);
                PointF ptU = new PointF(currX, currY -8);
                SolidBrush sb = new SolidBrush(Color.White);
                PointF[] points = { ptL, ptU,ptR };
                g.FillPolygon(sb, points);

                PointF ptAngle = new PointF(currX+(float)Math.Sin(ConvertToRadians(angle)) *50, currY-(float)Math.Cos(ConvertToRadians(angle)) *50);
                Pen pencile = new Pen(Color.Red);
                g.DrawLine(pencile, ptAngle, new PointF(currX, currY));
                thrustersState = false;

            }

            public void applyThrusters()
            {
                thrustersState = true;
            }
        }

        List<SpaceShip> currentPopulation = new List<SpaceShip>();
        SpaceShip currSpaceship;
        private void Form1_Load(object sender, EventArgs e)
        {
            Random r = new Random();
            for (int i = 0; i < 1000; i++)
            {
                List<Tuple<string, int>> currRun = new List<Tuple<string, int>>();
                for (int j = 0; j < 20; j++)
                {
                    int actionR = r.Next(4);
                    if (actionR == 0)
                        currRun.Add(new Tuple<string, int>("NT", r.Next(100, 600)));
                    if (actionR == 1)
                        currRun.Add(new Tuple<string, int>("RL", r.Next(10, 25)));
                    if (actionR == 2)
                        currRun.Add(new Tuple<string, int>("RR", r.Next(10, 25)));
                    if (actionR == 3)
                        currRun.Add(new Tuple<string, int>("TH", r.Next(10, 25)));
                }
                currentPopulation.Add(new SpaceShip(currRun));
            }


            //currSpaceship = new SpaceShip();
            pictureBox1.BackColor = Color.Black;
            timer1.Start();

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            foreach (SpaceShip item in currentPopulation)
            {
                item.AIMovement();
                item.move();

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
            //currSpaceship.draw(g);
            //SolidBrush myBrush = new SolidBrush(System.Drawing.Color.Red);


            foreach (SpaceShip item in currentPopulation)
            {
                item.draw(g);
            }
            //g = this.CreateGraphics();
            //g.FillRectangle(myBrush, new Rectangle(0, 0, 200, 300));
            //myBrush.Dispose();
            //g.Dispose();
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Form1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            /*if (e.KeyCode == Keys.Up)
            {
                currSpaceship.applyThrusters();
            }
            if (e.KeyCode == Keys.Left)
            {
                currSpaceship.decreaseAngle();
            }
            if (e.KeyCode == Keys.Right)
            {
                currSpaceship.increaseAngle();
            }*/
        }
    }
}
