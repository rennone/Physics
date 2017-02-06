using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
namespace Physics
{
    public static class DrawUtil
    {
        // 正方形
        public static Rectangle Square(int size, Point o = new Point())
        {
            return new Rectangle(o.X - size / 2, o.Y - size / 2, size, size);
        }
    }
}
