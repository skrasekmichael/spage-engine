using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapDriver
{
    public class Vector
    {
        protected double x, y, len;

        public double X => x;
        public double Y => y;
        public double Lenght => len;

        public Vector(double x, double y)
        {
            this.x = x; this.y = y;
            len = Math.Sqrt(x * x + y * y);
        }

        public override string ToString() => $"({x.ToString()}; {y.ToString()})";

        public static Vector FromPoints(PointF a, PointF b) => new Vector(b.X - a.X, b.Y - a.Y);

        public double Atan() => Math.Atan2(y, x);

        public static double Deviation(Vector a, Vector b)
        {
            double max(double val)
            {
                if (val < 0) return val + Math.PI * 2;
                return val;
            }
            return max(Math.Atan2(a.y, a.x)) - max(Math.Atan2(b.y, b.x));
        }
        //public static double Deviation2(Vector a, Vector b) => Math.Acos((a.x * b.x + a.y * b.y) / (a.len * b.len));
    }
}
