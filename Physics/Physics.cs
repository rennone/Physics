using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;

namespace Physics
{
    
    public static class Physics
    {
        const double Epsilon = 1.0e-6;  //誤差項

        // 線分同士の交点
        public static bool CrossSegments2D(Segment s1, Segment s2, ref Vector ret)
        {
            return CrossSegments2D(s1.P1, s1.P2, s2.P1, s2.P2, ref ret);
        }

        // 線分同士の交点
        public static bool CrossSegments2D(Vector v1, Vector v2, Vector w1, Vector w2, ref Vector ret, bool boarderIgnore = false)
        {
            var dv = v2 - v1;
            var dw = w2 - w1;

            var A = Vector.CrossProduct(v1 - w1, dv);
            var C = Vector.CrossProduct(dw, dv);

            // 2線分が平行なら交差しない
            if (Math.Abs(C) < Epsilon)
                return false;

            var u = A / C;


            double e = boarderIgnore ? Epsilon : -Epsilon;

            // 交点がwの線分上にない(境界は含まない
            if (u < e || u > 1.0 - e)
                return false;

            var dvw = v1 - w1;

            var t = -1.0;
            if( Math.Abs(dv.X) < 1.0e-6)
            {
                t = (w1.Y - v1.Y + dw.Y * u) / dv.Y;
            }
            else
            {
                t = (w1.X - v1.X + dw.X * u) / dv.X;
            }

            if (t < e || t > 1.0 - e)
                return false;

            // 交点を代入
            ret = dv*t + v1;

            return true;
        }


        // 線分と点の距離
        public static double SqrDistanceSegmentPoint(Vector v1, Vector v2, Vector p)
        {
            var d1 = p - v1;
            var d2 = p - v2;
            var dv = v2 - v1;

            if( dv.LengthSquared < 1.0e-6)
            {
                return d1.LengthSquared;
            }

            var a = Vector.Multiply(dv, d1);
            if( a <= 0)
            {
                return d1.LengthSquared;
            }


            var b = Vector.Multiply(-dv, d2);
            if(b <= 0)
            {
                return d2.LengthSquared;
            }


            return Math.Pow(Vector.CrossProduct(d1, dv) / dv.Length, 2);
        }

        public static double DistanceSegmentPoint(Vector v1, Vector v2, Vector p)
        {
            return Math.Sqrt(SqrDistanceSegmentPoint(v1, v2, p));
        }

       


        public static List<Tuple<int, int, Vector>> Intersections(Polygon p1, Polygon p2)
        {
            var ret = new List<Tuple<int, int, Vector>>();
            for (int i = 0; i < p1.Count; i++)
            {
                var v1 = p1.GetVertex(i);
                var v2 = p1.GetVertex((i+1) % p1.Count);
                var inters = new List<Tuple<int, int, Vector>>();
                for (int j = 0; j < p2.Count; j++)
                {
                    var w1 = p2.GetVertex(j);
                    var w2 = p2.GetVertex(j + 1);

                    Vector intersection = new Vector();
                    if (CrossSegments2D(v1, v2, w1, w2, ref intersection))
                    {                        
                        inters.Add(new Tuple<int, int, Vector>(i, j, intersection));
                    }
                }

                inters.Sort((a, b) =>  (int)( (a.Item3 - v1).LengthSquared - (b.Item3 - v1).LengthSquared) );
                foreach (var k in inters)
                    ret.Add(k);
            }

            return ret;
        }

    }



}
