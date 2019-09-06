using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunarLander
{
    class TerrainBlock
    {
        public Rectangle Bounds { get; set; }
        public int Fitness { get; set; }
        public int NumberOfAIEnds { get; set; } = 0;
        public TerrainBlock(Rectangle boundsIn, int fitnessIn)
        {
            Bounds = boundsIn;
            Fitness = fitnessIn;
        }
        public void draw(Graphics g)
        {
            SolidBrush sb = new SolidBrush(Color.FromArgb(255, 255 - (int)((Fitness / 100.0f) * 255.0f), (int)((Fitness / 100.0f) * 255), 0));
            if (Fitness == 1)
                sb = new SolidBrush(Color.LightSlateGray);
            g.FillRectangle(sb, Bounds);


            Font drawFont = new Font("Arial", 8);
            SolidBrush fn = new SolidBrush(Color.White);
            g.DrawString(NumberOfAIEnds.ToString(), drawFont, fn, Bounds.X, Bounds.Y);
        }

    }
}
