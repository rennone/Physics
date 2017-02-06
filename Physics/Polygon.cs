using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Physics
{
    public class Polygon : Figure
    {
        // 面積
        public double Square { get { return Math.Abs(SignedSquare); }  }

        // 回転方向
        public bool ClockWise { get { return IsClockwise(SignedSquare); } }

        public int Count { get { return Vertices.Count; } }

        public Vector this[int index]
        {
            get { 
                if (index < 0)
                    index = -(-index % Count) + Count;
                else
                    index = index % Count;

                return Vertices[index];
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public List<Vector> TransformedVertices
        {
            get
            {
                return Vertices.Select(v => v + Translation).ToList();
            }
        }
        
        //! cloneでコピーする必要がある変数

        // 符号付き面積
        double SignedSquare { get; set; }

        // 頂点群
        public List<Vector> Vertices { get; private set; }

        // 凹部分となるインデックス
        [System.Xml.Serialization.XmlIgnore]
        public List<Tuple<int, int>> IncisedIndices { get; private set; }


        private const double Epsilon = 1.0e-6;
        // メソッド

        // コンストラクタ
        public Polygon()
        {
            SignedSquare = 0;
            Vertices = new List<Vector>();
            IncisedIndices = new List<Tuple<int, int>>();
        }

        public void Copy(Polygon other)
        {
            base.Copy(other);
            other.Vertices = new List<Vector>(Vertices);
            other.IncisedIndices = new List<Tuple<int, int>>(IncisedIndices);
            other.SignedSquare = SignedSquare;
        }
        public new Polygon Clone()
        {
            Polygon ret = new Polygon();
            Copy(ret);
            return ret;
        }

        // 回転方向を反転させる
        public void Reverse()
        {
            // 頂点の並びを反転
            for(int i=1; i<(Vertices.Count+1)/2; i++)
            {
                Vector tmp = Vertices[i];
                Vertices[i] = Vertices[Count - i];
                Vertices[Count - i] = tmp;
            }

            // 面積も反転
            SignedSquare = -SignedSquare;

            // 凹形状の配列番号も上に合わせる
            for(int i=0; i<IncisedIndices.Count; i++)
            {
                var tmp = IncisedIndices[i];
                IncisedIndices[i] = new Tuple<int, int>(
                    (Vertices.Count - tmp.Item2)%Vertices.Count, (Vertices.Count - tmp.Item1)%Vertices.Count
                    );
            }
        }

        public Vector GetVertex(int index)
        {
            return Transform.Apply(this[index]);
        }

        // 最後に点を追加
        public void PushBack(Vector vertex)
        {
            Insert(Vertices.Count, vertex);
        }

        // 途中に点を追加
        virtual public bool Insert(int index, Vector vertex)
        {
            vertex -= Translation;
            if (index < 0 || index > Vertices.Count)
                return false;

            Vertices.Insert(index, vertex);

            // ポリゴンができていなければ面積計算しない
            if (Vertices.Count < 3)
            {
                SignedSquare = 0;
                return true;
            }

            // 加えた頂点にとその両端による三角形の符号付き面積を加える
            int back = (index + Vertices.Count - 1) % Vertices.Count;
            int next = (index + 1) % Vertices.Count;
            var square = Vector.CrossProduct(Vertices[index] - Vertices[back], Vertices[next] - Vertices[index]);
            SignedSquare += square;

            CalcIncisedIndices();
            return true;
        }

        // vertexを頂点として追加
        public void AddAt(Vector vertex)
        {
            Insert( GetNearestVertexIndex(vertex), vertex);
        }

        // 頂点を移動
        public bool Move(int index, Vector delta)
        {
            if (index < 0 || index >= Vertices.Count)
                return false;

            int back, next;
            GetBothEndIndex(index, out back, out next);

            // 動いた分の面積を計算
            SignedSquare += Vector.CrossProduct(delta, Vertices[next] - Vertices[back] - delta);
            Vertices[index] += delta;
            CalcIncisedIndices();
            return true;
        }

        // 多角形ともっとも近い線分上の前側の端点を返す
        public int GetNearestVertexIndex(Vector from)
        {
            if (Vertices.Count == 0)
                return 0;

            from -= Translation;

            var minSqrLen = -1.0;
            var index = 0;
            foreach (var item in Vertices.Select((v, i) => new { v, i }))
            {
                var sqrLen = Physics.SqrDistanceSegmentPoint(item.v, Vertices[(item.i + 1) % Vertices.Count], from);

                if (minSqrLen < 0 || sqrLen < minSqrLen)
                {
                    index = item.i;
                    minSqrLen = sqrLen;
                }
            }

            
            if (Vector.Multiply(Vertices[(index + 1) % Vertices.Count] - Vertices[index], from - Vertices[index]) >= 0)
                index = (index+1)%Vertices.Count;

            return index;
        }

        public int GetNearestIndex(Vector from, double limit = -1.0)
        {
            if (Vertices.Count == 0)
                return -1;

            var index = -1;
            var minSqLength = -1.0;
            foreach (var item in GetVertices().Select((v, i) => new { v, i }))
            {
                var sqLen = (item.v - from).LengthSquared;

                if(index < 0 || sqLen < minSqLength)
                {
                    index = item.i;
                    minSqLength = sqLen;
                }
            }

            if (limit > 0 && minSqLength < limit*limit)
                return index;

            return -1;
        }

        // indexの頂点を削除
        public bool Remove(int index)
        {
            if (index < 0 || index >= Vertices.Count)
                return false;

            Vector vertex = Vertices[index];

            // 加えた頂点にとその両端による三角形の符号付き面積を減らす
            SignedSquare -=TriangleSquare(index);

            Vertices.RemoveAt(index);

            CalcIncisedIndices();
            return true;
        }

        // from -> to のインデックスの長さ(循環)
        public int CircuitLength(int from, int to)
        {
            // from == to の場合を考えて <= にしないといけない
            if (from <= to)
                return to - from;
            else
                return to - from + Vertices.Count;
        }

        public int[] GetIndices(int from, int to)
        {
            int[] ret = new int[CircuitLength(from, to)];

            for(int i=0; i < ret.Length; i++)
            {
                ret[i] = (from + i) % Count;
            }

            return ret;
        }

        // indexの両端の点
        public void GetBothEndIndex(int index, out int back, out int next)
        {
            back = (index + Vertices.Count - 1) % Vertices.Count;
            next = (index + 1) % Vertices.Count;
        }


        // indexとその両端の点による平行四辺形の符号付き面積
        double ParallelogramSquare(int index)
        {
            int back, next;
            GetBothEndIndex(index, out back, out next);
            return Vector.CrossProduct(Vertices[index] - Vertices[back], Vertices[next] - Vertices[index]);
        }

        // indexとその両端の点による三角形の符号付き面積
        double TriangleSquare(int index)
        {
            return 0.5f * ParallelogramSquare(index);
        }

        // 点Pとの衝突判定
        public bool IsHit(Vector p)
        {
            // 完全な凸多角形の場合は, 回転方向と一致しているかだけ調べる
            if (IncisedIndices.Count == 0)
                return CheckHandness(0, Vertices.Count, p, ClockWise) == HitPlace.In;

            var clockwise = ClockWise;

            for (int i = 0; i < IncisedIndices.Count; i++ )
            {
                var st = IncisedIndices[i].Item1;
                var en = IncisedIndices[i].Item2;

                // 凹形状部分の両端を繋いだ点の内部に無いといけない
                var cross = Vector.CrossProduct(GetVertex(en) - GetVertex(st), p - GetVertex(st));
 
                // 線上もダメ
                if( (clockwise && !IsClockwise(cross)) || (!clockwise && !IsCounterClockwise(cross)))
                    return false; 

                // 凹形状部分の内部にあってはいけない

                if (CheckHandness(st, CircuitLength(st, en), p, !clockwise) != HitPlace.Out)
                    return false;
                
               

                // 凹型部分最後から, 次の凹型部分の間の凸型部分の内部にないといけない
                if (CheckHandness(en, CircuitLength(en, IncisedIndices[ (i+1) % IncisedIndices.Count].Item1), p, clockwise) != HitPlace.In)
                    return false;
            }

            return true;
        }

        public HitPlace GetHitState(Vector p)
        {
            // 完全な凸多角形の場合は, 回転方向と一致しているかだけ調べる
            if (IncisedIndices.Count == 0)
                return CheckHandness(0, Vertices.Count, p, ClockWise);

            var clockwise = ClockWise;

            for (int i = 0; i < IncisedIndices.Count; i++)
            {
                var st = IncisedIndices[i].Item1;
                var en = IncisedIndices[i].Item2;


                var d1 = GetVertex(en) - GetVertex(st);
                var d2 = p - GetVertex(st);

                if (d2.LengthSquared < Epsilon * Epsilon)
                    return HitPlace.OnLine;

                d1.Normalize();
                d2.Normalize();
                // 凹形状部分の両端を繋いだ点の内部に無いといけない
                var cross = Vector.CrossProduct(d1, d2);

                // 線上にある
                if (IsParallel(cross))
                    return HitPlace.OnLine;

                // 線上もダメ
                if ((clockwise && !IsClockwise(cross)) || (!clockwise && !IsCounterClockwise(cross)))
                    return HitPlace.Out;

                // 凹形状部分の内部にあってはいけない
                {
                    var place = CheckHandness(st, CircuitLength(st, en), p, !clockwise);
                    
                    if (place == HitPlace.In)
                        return HitPlace.Out;

                    if (place == HitPlace.OnLine)
                        return place;
                }

                // 凹型部分最後から, 次の凹型部分の間の凸型部分の内部にないといけない
                {
                    var place = CheckHandness(en, CircuitLength(en, IncisedIndices[(i + 1) % IncisedIndices.Count].Item1), p, clockwise);
                    if (place != HitPlace.In)
                         return place;
                }
               
            }

            return HitPlace.In;
        }

        public enum HitPlace
        {
            In,       //内部
            Out,      //外部
            OnLine    //線上
        }

        // 回転方向が全部clockwiseと同じかどうか
        public HitPlace CheckHandness(int index, int count, Vector p, bool clockwise)
        {
            for(int i = index; i < index + count; i++)
            {
                var a = GetVertex(i % Vertices.Count);
                var b = GetVertex((i + 1) % Vertices.Count);

                var d1 = b - a;
                var d2 = p - a;

                if (d1.LengthSquared < Epsilon*Epsilon)
                    return HitPlace.OnLine;

                d1.Normalize();
                d2.Normalize();
                var cross = Vector.CrossProduct(d1, d2);

                if (IsParallel(cross))
                    return HitPlace.OnLine;

                if( (clockwise && !IsClockwise(cross)) || (!clockwise && !IsCounterClockwise(cross)))
                    return HitPlace.Out;
            }

            return HitPlace.In;
        }
      


        // cross : 外積
        public static bool IsClockwise(double crossProduct)
        {
            return crossProduct >= Epsilon;//右手系なので
        }

        public static bool IsCounterClockwise(double crossProduct)
        {
            return crossProduct <= -Epsilon;
        }

        public static bool IsParallel(double crossProduct)
        {
            return Math.Abs(crossProduct) < Epsilon;
        }

        // 凹形状部分を探索
        public void CalcIncisedIndices()
        {
            IncisedIndices.Clear();
            if( Vertices.Count < 3)
                return;

            bool clockwise = ClockWise;

            int st = -1;
            for(int i=0; i < Vertices.Count; i++)
            {
                var c = ParallelogramSquare(i);

                if (SignedSquare * c >= 0)
                {
                    if( st >= 0){
                        IncisedIndices.Add(new Tuple<int, int>(st, i));
                        st = -1;
                    }
                    continue;
                }

                // 凸 -> 凹になった場所
                if(st < 0){
                    st = ( i + Vertices.Count - 1) % Vertices.Count;
                }
            }

            // 対になる凹->凸となるのが見つからなかった
            if (st >= 0)
            {
                // 0 が凹形状の一部で繋がる場合
                if (IncisedIndices.Count > 0 && IncisedIndices[0].Item1 == 0)
                    IncisedIndices[0] = new Tuple<int,int>(st, IncisedIndices[0].Item2);

                // 一番最後が分岐点
                else
                    IncisedIndices.Add(new Tuple<int, int>(st, (st + 2) % Vertices.Count));
            }
        }

        // otherを内包するかどうか
        public bool Include(Polygon other)
        {
            foreach(var v in other.TransformedVertices)
            {
                if (IsHit(v) == false)
                    return false;
            }

            return true;
        }

        public double CalcSignedSquare()
        {
            double ret = 0.0;

            for(int i=0; i < Vertices.Count - 1; i++)
            {
                ret += Vector.CrossProduct(Vertices[i], Vertices[i + 1]);
            }

            return 0.5 * (ret + Vector.CrossProduct(Vertices[Vertices.Count - 1], Vertices[0]));
        }

        public List<System.Drawing.Point> ToPoints()
        {
            return Vertices.Select(
                (v,i) =>
                    {
                        var a = GetVertex(i);
                        return new System.Drawing.Point((int)a.X, (int)a.Y);
                    }).ToList();
        }

        public Vector[] GetVertices()
        {
            return Vertices.Select(v => Transform.Apply(v)).ToArray();
        }
    }
}
