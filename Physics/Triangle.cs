using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
namespace Physics
{
    class Triangle : Polygon
    {
        public Triangle(Vector p1, Vector p2, Vector p3)
        {
            PushBack(p1);
            PushBack(p2);
            PushBack(p3);
        }    
    }
}
