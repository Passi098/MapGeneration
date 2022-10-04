using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace MapGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Voronoi voronoi = new Voronoi(1000, 1000, 255, 1);
            PerlinNoise perlin = new PerlinNoise(100, 1);

            Console.WriteLine(perlin[0.125f, 0.175f]);

            DrawVoronoi(voronoi, perlin, "Voronoi0");
            Console.WriteLine("Created 0");
            voronoi.Relax();
            DrawVoronoi(voronoi, perlin, "Voronoi1");
            Console.WriteLine("Created 1");
            voronoi.Relax();
            DrawVoronoi(voronoi, perlin, "Voronoi2");
            Console.WriteLine("Created 2");
            voronoi.Relax();
            DrawVoronoi(voronoi, perlin, "Voronoi3");
            Console.WriteLine("Created 3");
            voronoi.Relax();
            DrawVoronoi(voronoi, perlin, "Voronoi4");
            Console.WriteLine("Created 4");
            voronoi.Relax();
            DrawVoronoi(voronoi, perlin, "Voronoi5");
            Console.WriteLine("Created 5");

        }

        public static void DrawVoronoi(Voronoi _voronoi, PerlinNoise _perlin, string _filename)
        {
            PNG image = new PNG(_voronoi.SizeX, _voronoi.SizeY, Color.White);
            //image = new PNG(Environment.CurrentDirectory + @"\green.png");

            for (int x = 0; x < _voronoi.SizeX; x++)
            {
                for (int y = 0; y < _voronoi.SizeY; y++)
                {
                    if (_voronoi[x, y] != -1)
                    {
                        Color color = GetColor(_voronoi.m_Dots[_voronoi[x, y]].m_Index, _voronoi, _perlin);
                        image[x, y] = color;
                    }
                }
            }

            image.Save(Environment.CurrentDirectory + @"\" + _filename + ".png");
            image.Dispose();
        }

        public static Color GetColor(int _index, Voronoi _voronoi, PerlinNoise _perlin)
        {
            float height = _perlin[_voronoi.m_Dots[_index].m_Pos.x / (float)_voronoi.SizeX, _voronoi.m_Dots[_index].m_Pos.y / (float)_voronoi.SizeY];


            if (height < 0.2f)
            {
                return Color.Aqua;
            }
            else if (height < 0.3f)
            {
                return Color.Yellow;
            }
            else if (height < 0.4f)
            {
                return Color.LightGreen;
            }
            else if (height < 0.6f)
            {
                return Color.LightGray;
            }
            else
            {
                return Color.White;
            }
        }
    }

    class Voronoi
    {
        private readonly int[,] m_Voronoi;
        public Dot[] m_Dots;
        public readonly TopEdge m_TopEdge;
        public readonly BottomEdge m_BottomEdge;
        public readonly LeftEdge m_LeftEdge;
        public readonly RightEdge m_RightEdge;

        public int SizeX
        {
            get { return m_Voronoi.GetLength(0); }
        }

        public int SizeY
        {
            get { return m_Voronoi.GetLength(1); }
        }

        public int this[int _x, int _y]
        {
            get { return m_Voronoi[_x, _y]; }
            set { m_Voronoi[_x, _y] = value; }
        }

        public Voronoi(int _sizeX, int _sizeY, int _dotAmount, int _seed)
        {
            if(_dotAmount > _sizeX * _sizeY)
            {
                _dotAmount = _sizeX * _sizeY;
            }

            m_TopEdge = new TopEdge(_sizeY);
            m_BottomEdge = new BottomEdge(_sizeY);
            m_LeftEdge = new LeftEdge(_sizeX);
            m_RightEdge = new RightEdge(_sizeX);

            m_Dots = new Dot[_dotAmount];
            m_Voronoi = new int[_sizeX, _sizeY];

            Init(_sizeX, _sizeY, _dotAmount, _seed);
        }

        private void Init(int _sizeX, int _sizeY, int _dotAmount, int _seed)
        {
            for (int x = 0; x < _sizeX; x++)
            {
                for (int y = 0; y < _sizeY; y++)
                {
                    m_Voronoi[x, y] = -1;
                }
            }

            int randomX, randomY;
            Random random = new Random(_seed);

            for (int dot = 0; dot < _dotAmount;)
            {
                randomX = random.Next(0, _sizeX);
                randomY = random.Next(0, _sizeY);
                if (m_Voronoi[randomX, randomY] == -1)
                {
                    m_Dots[dot] = new Dot(dot, new Vector2Int(randomX, randomY));
                    m_Voronoi[randomX, randomY] = m_Dots[dot].m_Index;
                    dot++;
                }
            }

            Calc();
        }

        private void Recalc()
        {
            m_TopEdge.ClearNeightbors();
            m_BottomEdge.ClearNeightbors();
            m_LeftEdge.ClearNeightbors();
            m_RightEdge.ClearNeightbors();

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    m_Voronoi[x, y] = -1;
                }
            }

            foreach (Dot dot in m_Dots)
            {
                m_Voronoi[dot.m_Pos.x, dot.m_Pos.y] = dot.m_Index;
            }

            Calc();
        }

        private void Calc()
        {
            Queue<Vector2Int> dotsToCalc = new Queue<Vector2Int>();

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    if(m_Voronoi[x, y] != -1)
                    {
                        dotsToCalc.Enqueue(new Vector2Int(x, y));
                    }
                }
            }

            while (dotsToCalc.Count > 0)
            {
                CalcDot(dotsToCalc.Dequeue());
            }

            m_TopEdge.CalcHeight(-2);
            m_BottomEdge.CalcHeight(-2);
            m_LeftEdge.CalcHeight(-2);
            m_RightEdge.CalcHeight(-2);

            void CalcDot(Vector2Int _dot)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if(x != 0 || y != 0)
                        {
                            EnqueueDot(_dot.x + x, _dot.y + y, m_Voronoi[_dot.x, _dot.y]);
                        }
                    }
                }
            }

            void EnqueueDot(int _x, int _y, int _dot)
            {
                if (_x >= 0 && _y >= 0 && _x < SizeX && _y < SizeY)
                {
                    if (m_Voronoi[_x, _y] == -1)
                    {
                        m_Voronoi[_x, _y] = _dot;
                        dotsToCalc.Enqueue(new Vector2Int(_x, _y));
                    }
                    else
                    {
                        m_Dots[_dot].AddNeightbor(m_Dots[m_Voronoi[_x, _y]]);
                    }
                }
                else
                {
                    if(_x < 0)
                    {
                        m_Dots[_dot].AddNeightbor(m_LeftEdge);
                        m_LeftEdge.AddNeightbor(m_Dots[_dot]);
                    }
                    else if(_x >= SizeX)
                    {
                        m_Dots[_dot].AddNeightbor(m_RightEdge);
                        m_RightEdge.AddNeightbor(m_Dots[_dot]);
                    }
                    if(_y < 0)
                    {
                        m_Dots[_dot].AddNeightbor(m_TopEdge);
                        m_TopEdge.AddNeightbor(m_Dots[_dot]);
                    }
                    else if(_y >= SizeY)
                    {
                        m_Dots[_dot].AddNeightbor(m_BottomEdge);
                        m_BottomEdge.AddNeightbor(m_Dots[_dot]);
                    }
                }
            }
        }

        public void Relax()
        {
            Dot[] newDots = new Dot[m_Dots.Length];

            for (int i = 0; i < newDots.Length; i++)
            {
                newDots[i] = new Dot(m_Dots[i].m_Index, m_Dots[i].Centroid());
            }

            m_Dots = newDots;
            Recalc();
        }

        public class Dot
        {
            public readonly int m_Index;
            public readonly Vector2Int m_Pos;

            public int m_Height { get; private set; }

            private List<Dot> m_Neighbors = new List<Dot>();

            public Dot(int _index, Vector2Int _pos)
            {
                m_Index = _index;
                m_Pos = _pos;
                m_Height = -1;
            }

            public void AddNeightbor(Dot _dot)
            {
                if(!m_Neighbors.Contains(_dot))
                {
                    m_Neighbors.Add(_dot);
                }
            }

            protected virtual Vector2Int GetPull(Dot _other)
            {
                return m_Pos - _other.m_Pos;
            }

            public Vector2Int Centroid()
            {
                Vector2Int centroid = new Vector2Int(0, 0);
                foreach (Dot neighbor in m_Neighbors)
                {
                    centroid += neighbor.GetPull(this);
                }

                centroid /= m_Neighbors.Count;

                return m_Pos + centroid;
            }

            public virtual void CalcHeight(int _height)
            {
                _height += 1;
                if (_height < m_Height || m_Height < 0)
                {
                    m_Height = _height;
                    foreach (Dot dot in m_Neighbors)
                    {
                        dot.CalcHeight(m_Height);
                    }
                }
            }

            public void ClearNeightbors()
            {
                m_Neighbors.Clear();
            }
        }

        public abstract class Edge : Dot
        {
            protected readonly int m_Size;
            public Edge(int _size) : base(-1, new Vector2Int(0, 0))
            {
                m_Size = _size;
            }
        }

        public class TopEdge : Edge
        {
            public TopEdge(int _size) : base(_size)
            {

            }

            protected override Vector2Int GetPull(Dot _other)
            {
                return new Vector2Int(0, (int)(-_other.m_Pos.y * 2));
            }
        }

        public class BottomEdge : Edge
        {
            public BottomEdge(int _size) : base(_size)
            {

            }

            protected override Vector2Int GetPull(Dot _other)
            {
                return new Vector2Int(0, (int)((m_Size - _other.m_Pos.y) * 2));
            }
        }

        public class LeftEdge : Edge
        {
            public LeftEdge(int _size) : base(_size)
            {

            }

            protected override Vector2Int GetPull(Dot _other)
            {
                return new Vector2Int((int)(-_other.m_Pos.x * 2), 0);
            }
        }

        public class RightEdge : Edge
        {
            public RightEdge(int _size) : base(_size)
            {

            }

            protected override Vector2Int GetPull(Dot _other)
            {
                return new Vector2Int((int)((m_Size - _other.m_Pos.x) * 2), 0);
            }
        }
    }

    class PerlinNoise
    {
        private int m_Detail;
        private float[,] m_Noise;

        public PerlinNoise(int _detail, int _seed)
        {
            m_Detail = _detail;
            m_Noise = new float[_detail, _detail];
            Random random = new Random(_seed);

            for (int x = 0; x < _detail; x++)
            {
                for (int y = 0; y < _detail; y++)
                {
                    m_Noise[x, y] = random.Next(0, 1000000001) / 1000000000f;
                }
            }
        }

        public float this[float _x, float _y]
        {
            get
            {
                _x = Math.Clamp(_x, 0f, 1f);
                _y = Math.Clamp(_y, 0f, 1f);

                _x *= m_Detail;
                _y *= m_Detail;

                if (_x % 1 != 0)
                {

                    if (_y % 1 != 0)
                    {
                        return Lerp(Lerp(m_Noise[(int)_x, (int)_y], m_Noise[1 + (int)_x, (int)_y], _x % 1), Lerp(m_Noise[(int)_x, 1 + (int)_y], m_Noise[1 + (int)_x, 1 + (int)_y], _x % 1), _y % 1);
                    }
                    else
                    {
                        return Lerp(m_Noise[(int)_x, (int)_y], m_Noise[1 + (int)_x, (int)_y], _x % 1);
                    }
                }
                else
                {

                    if (_y % 1 != 0)
                    {
                        return Lerp(m_Noise[(int)_x, (int)_y], m_Noise[(int)_x, 1 + (int)_y], _y % 1);
                    }
                    else
                    {
                        return m_Noise[(int)_x, (int)_y];
                    }
                }

                float Lerp(float _a, float _b, float _t)
                {
                    return _a + ((_b - _a) * _t);
                }
            }
        }
    }
        
    struct Vector2Int
    {
        public int x, y;

        public Vector2Int(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public static Vector2Int operator -(Vector2Int a, Vector2Int b) { return new Vector2Int(a.x - b.x, a.y - b.y); }
        public static Vector2Int operator +(Vector2Int a, Vector2Int b) { return new Vector2Int(a.x + b.x, a.y + b.y); }
        public static Vector2Int operator /(Vector2Int a, int b) { return new Vector2Int(a.x / b, a.y / b); }
        public static Vector2Int operator *(Vector2Int a, int b) { return new Vector2Int(a.x * b, a.y * b); }
    }

    class PNG
    {
        Bitmap m_Bitmap;
        Graphics m_Graphic;

        public PNG(int _sizeX, int _sizeY)
        {
            m_Bitmap = new Bitmap(_sizeX, _sizeY);
            m_Graphic = Graphics.FromImage(m_Bitmap);
        }

        public PNG(int _sizeX, int _sizeY, Color _baseColor)
        {
            m_Bitmap = new Bitmap(_sizeX, _sizeY);
            m_Graphic = Graphics.FromImage(m_Bitmap);
            Clear(_baseColor);
        }

        public PNG(string _filename)
        {
            FileStream stream = new FileStream(_filename, FileMode.Open, FileAccess.Read);
            m_Bitmap = new Bitmap(Image.FromStream(stream));
            m_Graphic = Graphics.FromImage(m_Bitmap);
            stream.Dispose();
        }

        ~PNG()
        {
            Dispose();
        }

        public void Dispose()
        {
            if(m_Graphic != null)
            {
                m_Graphic.Dispose();
            }

            if(m_Bitmap != null)
            {
                m_Bitmap.Dispose();
            }
        }

        public Color this[int _x, int _y]
        {
            get { return m_Bitmap.GetPixel(_x, _y); }
            set { m_Graphic.FillRectangle(new SolidBrush(value), _x, _y, 1, 1); }
        }

        public void Clear(Color _color)
        {
            m_Graphic.Clear(_color);
        }

        public void Save(string _filename)
        {
            FileStream stream = new FileStream(_filename, FileMode.OpenOrCreate, FileAccess.Write);
            m_Bitmap.Save(stream, ImageFormat.Png);
            stream.Dispose();
        }
    }
}
