using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Physics
{
    public class FigureBoolean
    {
        public static List<Polygon> Product(Polygon p1, Polygon p2)
        {
            if (p1.Count < 3 || p2.Count < 3)
                return new List<Polygon>();

            return null;
        }

        public static List<Polygon> ProductSet(Polygon p1, Polygon p2)
        {
            List<Polygon> ret = new List<Polygon>();

            if (p1.Count < 3 || p2.Count < 3)
                return ret;


            if (p1.ClockWise == false)
            {
                p1 = p1.Clone();
                p1.Reverse();
            }

            if (p2.ClockWise == false)
            {
                p2 = p2.Clone();
                p2.Reverse();
            }

            /*
                頂点がもう一方のポリゴンの辺と重なっている場合を考える必要がある
             */

            // p1とp2の交点を求める
            var inter1 = Intersections(p1, p2);

            // 交点がない場合, 包括しているか離れているか
            if (inter1.Count == 0)
            {
                if (p2.IsHit(p1.GetVertex(0)))
                    ret.Add(p1.Clone());

                else if (p1.IsHit(p2.GetVertex(0)))
                    ret.Add(p2.Clone());

                return ret;
            }

            // p2周りを基準に並び替える
            var inter2 = inter1.Select(a => new Tuple<int, int, Vector>(a.Item2, a.Item1, a.Item3))
                .OrderBy(b => b.Item1)
                .ThenBy(c => (c.Item3 - p2.GetVertex(c.Item1)).LengthSquared)
                .ThenBy(d => d.Item2)
                .ToList();

            for(int i=0; i < inter2.Count; i++)
            {
                var next = (i + 1) % inter2.Count;
                
                {
                    int n = 0;
                    while (inter2[i].Item3 == inter2[next].Item3 && n != inter2.Count-1)
                    {
                        next = (next + 1) % inter2.Count;
                        n++;
                    }
                }
                next = (next + inter2.Count - 1) % inter2.Count;

                if (i != next)
                {
                    if (inter2[next].Item2 == p1.Count - 1)
                    {
                        var tmp = inter2[next];
                        inter2.RemoveAt(next);
                        inter2.Insert(i, tmp);
                    }

                    i = next;
                }
            }

            //var inter2 = Intersections(p2, p1);
            List<Tuple<int, int, Vector>>[] inters = { inter1, inter2 };
            Polygon[] polygons = { p1, p2 };
            int[] starts = { 0, 0 };
            List<int>[] memos = { new List<int>(), new List<int>() };


            
           // int offset = p2.IsHit(p1.GetVertex(inter1[0].Item1)) ? 1 : 0;

            int offset = 0;
            {
                var v = p1.GetVertex(inter1[0].Item1);
                var state = p2.GetHitState(v);
                if(state == Polygon.HitPlace.OnLine)
                {
                    int index = 0;
                   // while(  )
                    for (int i = 0; i < inter1.Count; i++)
                    {
                        if (inter1[i].Item1 != inter1[0].Item1)
                            break;

                        // 同じ場所にあるのは無視
                        if (v == inter1[i].Item3)
                            continue;


                    }
                }               

                if( state == Polygon.HitPlace.OnLine)
                {
                    Console.WriteLine("Error");
                }
                else if( state == Polygon.HitPlace.In)
                {
                    offset = 1;
                }
                else
                {
                    offset = 0;
                }
            }

            int cnt = 0;
            while (cnt < inter1.Count)
            {
                for (int i = 0; i < inter1.Count; i += 2)
                {
                    int ind = (i + offset) % inter1.Count;
                    if (memos[0].Contains(ind) == false)
                    {
                        starts[0] = ind;
                        break;
                    }
                }

                int index1 = 0;
                Polygon p = new Polygon();
                while (true)
                {
                    var index2 = (index1 + 1) % 2;

                    var inter = inters[index1];
                    var poly = polygons[index1];
                    var memo = memos[index1];

                    var st1 = starts[index1];
                    var nx1 = (st1 + 1) % inter.Count;

                    if (memo.Contains(st1))
                    {
                        ret.Add(p);
                        break;
                    }

                    memo.Add(st1);

                    p.PushBack(inter[st1].Item3);//交点を追加

                    // 一つの線分が2点と交差している時は, 逆方向に一周してしまわないように頂点の追加をしない
                    // ただし, nx1 != st+1 (stのほうがnxよりも後ろにある) 場合は一周して追加させる
                    if ( !( inter[nx1].Item1 == inter[st1].Item1 && nx1 == st1+1))
                    {
                        int fr = (inter[st1].Item1 + 1) % poly.Count;
                        int to = inter[nx1].Item1;

                        foreach (var ind in poly.GetIndices(fr, to))
                        {
                            p.PushBack(poly.GetVertex(ind));
                        }
                        p.PushBack(poly.GetVertex(to));
                    }

                    starts[index2] = inters[index2].FindIndex((a) => a.Item1 == inter[nx1].Item2 && a.Item2 == inter[nx1].Item1);
                    cnt++;
                    index1 = index2;
                }
            }

            return ret;
        }


        private static List<Tuple<int, int, Vector>> Intersections(Polygon p1, Polygon p2)
        {
            var ret = new List<Tuple<int, int, Vector>>();
            for (int i = 0; i < p1.Count; i++)
            {
                var v1 = p1.GetVertex(i);
                var v2 = p1.GetVertex((i + 1) % p1.Count);
                var inters = new List<Tuple<int, int, Vector>>();
                for (int j = 0; j < p2.Count; j++)
                {
                    var w1 = p2.GetVertex(j);
                    var w2 = p2.GetVertex(j + 1);

                    Vector intersection = new Vector();
                    if (Physics.CrossSegments2D(v1, v2, w1, w2, ref intersection))
                    {
                        inters.Add(new Tuple<int, int, Vector>(i, j, intersection));
                    }
                }

                inters.Sort((a, b) => (int)((a.Item3 - v1).LengthSquared - (b.Item3 - v1).LengthSquared));
                
                foreach (var k in inters)
                    ret.Add(k);
            }

            return ret;
        }

        const double Epsilon = 1.0e-6;
        const double SqEpsilon = Epsilon * Epsilon;
    }
}
