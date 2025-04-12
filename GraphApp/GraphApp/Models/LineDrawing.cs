using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace GraphApp.Models
{
    public class LineDrawing
    {
        public static List<Point> BresenhamLine (Point p1, Point p2) {
            List<Point> DrawPoints = new List<Point>();
            int x1, x2, y1, y2;
            bool reverse = Math.Abs(p2.Y - p1.Y) > Math.Abs(p2.X - p1.X);
            if (!reverse)
            {
                x1 = p1.X;
                x2 = p2.X;
                y1 = p1.Y;
                y2 = p2.Y;
            }
            else
            {
                x1 = p1.Y;
                x2 = p2.Y;
                y1 = p1.X;
                y2 = p2.X;
            }
            double m = (double)(y2 - y1) / (x2 - x1);
            int c = y1;
            int y = 0;
            int x = x1;

            while (x != x2)
            {
                y = (int)Math.Round(m * (x - x1) + c) % 256;
                if (reverse)
                    DrawPoints.Add(new Point(y, x));
                else
                    DrawPoints.Add(new Point(x, y));
                x = x1 > x2 ? --x: ++x;
            }
            return DrawPoints;
        }

    }
}
