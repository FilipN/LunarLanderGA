using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunarLander
{
    public class SpaceShip : IComparable
    {

        public static int ChromosomeSize = 20;
        public static int AliveNumber { get; set; } = 0;
        public static float ShipR { get; set; } = 10;

        private float GAFinalVerticalSpeed { get; set; }
        private float GAFinalHorizontalSpeed { get; set; }
        private float GALandingBlock { get; set; } = 0;
        public float GAFitnessFunction { get; set; }

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

        public SpaceShip(List<Tuple<string, int>> inRun, float startX, float startY, bool inElite = false)
        {
            xM = 0f;
            yM = 0f;
            currX = startX;
            currY = startY;
            run = inRun;
            elite = inElite;
        }

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

            GAFitnessFunction = GALandingBlock * 1.5f + WGAFinalHorizontalSpeed + WGAFinalVerticalSpeed;// + 15.0f / GAFinalHorizontalSpeed + 10.0f / GAFinalVerticalSpeed;
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

            if (thrustersState)
                g.FillEllipse(sbThruster, new RectangleF(ThrusterPoint.X - SpaceShip.ShipR / 4.0f, ThrusterPoint.Y - SpaceShip.ShipR / 4.0f, SpaceShip.ShipR / 2, SpaceShip.ShipR / 2));
            g.FillEllipse(sb, new RectangleF(currX - SpaceShip.ShipR / 2.0f, currY - SpaceShip.ShipR / 2.0f, SpaceShip.ShipR, SpaceShip.ShipR));
            g.FillEllipse(sbWindow, new RectangleF(currX - SpaceShip.ShipR / 4.0f, currY - SpaceShip.ShipR / 4.0f, SpaceShip.ShipR / 2, SpaceShip.ShipR / 2));


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
}
