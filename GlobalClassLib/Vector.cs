using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroFormatter;

namespace GlobalClassLib
{
    [ZeroFormattable]
    public struct Vector
    {
        [Index(0)]
        public float X;
        [Index(1)]
        public float Y;

        // arg0 = Index0, arg1 = Index1
        public Vector(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public double Mag()
        {
            double mag = Math.Sqrt(X * X + Y * Y);
            return mag;
        }

        public void multi(float faktor)
        {
            X = X * faktor;
            Y = Y * faktor;
        }
    }
}

