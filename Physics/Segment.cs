using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Physics
{
    public class Segment : Transform2D
    {
        public Vector P1 { get; private set; }
        public Vector P2 { get; private set; }

        public Segment(Vector p1, Vector p2)
        {
            P1 = p1;
            P2 = p2;
        }

        public double Distance(Vector p)
        {
            return Physics.DistanceSegmentPoint(P1,P2,  p);
        }

        public double Distance(System.Drawing.Point p)
        {
            return Distance(new Vector(p.X, p.Y));
        }
    }
}
