using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Physics
{
    public class Transform2D
    {
        public static Transform2D NoTransform = new Transform2D();

        public Vector Translation { get; set; }

        public float Rotation { get; set; }

        public Vector Scale { get; set; }

        public Transform2D()
        {
            Translation = new Vector(0, 0);
            Rotation = 0;
            Scale = new Vector(1, 1);
        }

        public void Copy(Transform2D other)
        {
            other.Translation = Translation;
            other.Rotation = Rotation;
            other.Scale = Scale;
        }

        public Transform2D Clone()
        {
            var ret = new Transform2D();
            Copy(ret);
            return ret;
        }

        // v にtransform変形を適応
        public Vector Apply(Vector v)
        {
            var c = Math.Cos(Rotation);
            var s = Math.Sin(Rotation);

            Vector ret = new Vector();
            ret.X = Scale.X * v.X * c - Scale.Y * v.Y * s;
            ret.Y = Scale.X * v.X * s + Scale.Y * v.Y * c;
            return ret + Translation;
        }

        public Vector ReverseApply(Vector vv)
        {
            var v = vv - Translation;
            
            var c = Math.Cos(-Rotation);
            var s = Math.Sin(-Rotation);

            var ret = new Vector();
            ret.X = v.X * c - v.Y * s;
            ret.Y = v.X * s + v.Y * c;
            
            ret.X /= Scale.X;
            ret.Y /= Scale.Y;

            return ret;
        }
    }

    public class Figure
    {
        public Transform2D Transform { get { return transform; } set { transform = value; } }
        public Vector Translation { get { return Transform.Translation; } set { Transform.Translation = value; } }
        public float Rotation { get { return Transform.Rotation; } set { Transform.Rotation = value; } }
        public Vector Scale { get { return Transform.Scale; } set { Transform.Scale = value; } }

        Transform2D transform = new Transform2D();

        public void Copy(Figure other)
        {
            other.Translation = new Vector(Translation.X, Translation.Y);
            other.Rotation = Rotation;
            other.Scale = new Vector(Scale.X, Scale.Y);
        }

        public Figure Clone()
        {
            var ret = new Figure();
            Copy(ret);
            return ret;
        }
    }

}
