using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunarLander
{
    static class FunctionUtil
    {

        public static void drawFunction(float width, float height, Graphics g, List<List<float>> func, int xRange)
        {
            float yRange = 0;
            foreach (List<float> lista in func)
            {
                for (int i = 0; i < lista.Count - 1; i++)
                {
                    if (lista[i] > yRange)
                        yRange = lista[i];
                }
            }

            Pen pen = new Pen(Color.Black);
            float horMargin = (float)(width * 0.1);
            float verMargin = (float)(height * 0.1);
            float fheight = height - 2 * verMargin;
            float fwidth = width - 2 * horMargin;
            g.DrawLine(pen, horMargin, height - verMargin, horMargin, verMargin);
            g.DrawLine(pen, horMargin, height - verMargin, width - horMargin, height - verMargin);
            float xTick = fwidth / xRange;
            float yCoeff = fheight / yRange;



            for (int k = 0; k < func.Count; k++)
            {
                var lista = func[k];
                if (k == func.Count - 1)
                    pen.Color = Color.Red;
                else
                    pen.Color = Color.Black;
                for (int i = 0; i < lista.Count - 1; i++)
                {
                    float yCurr = height - verMargin - lista[i] * yCoeff;
                    float yNext = height - verMargin - lista[i + 1] * yCoeff;

                    g.DrawLine(pen, horMargin + i * xTick, yCurr, horMargin + (i + 1) * xTick, yNext);
                }
            }


            Font drawFont = new Font("Arial", 6);
            SolidBrush sb = new SolidBrush(Color.Black);
            g.DrawString(((int)yRange).ToString(), drawFont, sb, horMargin / 5, verMargin);
        }
    }
}
