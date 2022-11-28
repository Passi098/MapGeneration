using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using PNGConverter;
using PerlinNoise;

namespace MapGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            //Test(100, 2, 5, new Point(400, 400));
            //int perlinSeed = 3;
            //int perlinDetail = 10;
            //DrawPerlin(new PerlinNoiseDiagramm(perlinDetail, perlinSeed), "Perlin_" + perlinDetail + "_" + perlinSeed);
            MapGenerator.GenerateMap(200, 2, 2, 100, new Point(400, 400));
            Console.WriteLine("Done");
            //for (int seed = 0; seed < 10; seed++)
            //{
            //    VoronoiDiagramm voronoi = VoronoiGenerator.GenerateVoronoi(100, seed);
            //    PerlinNoise perlin = new PerlinNoise(10, seed);

            //    for (int relax = 0; relax < 2; relax++)
            //    {
            //        voronoi.Relax();
            //    }

            //    //DrawVoronoi(voronoi, perlin, "Seed" + seed.ToString());
            //    Console.WriteLine("Created " + seed.ToString());
            //}

            //DrawVoronoi(voronoi, perlin, "Voronoi0");
            //Console.WriteLine("Created 0");
            //voronoi.Relax();
            //DrawVoronoi(voronoi, perlin, "Voronoi1");
            //Console.WriteLine("Created 1");
            //voronoi.Relax();
            //DrawVoronoi(voronoi, perlin, "Voronoi2");
            //Console.WriteLine("Created 2");
            //voronoi.Relax();
            //DrawVoronoi(voronoi, perlin, "Voronoi3");
            //Console.WriteLine("Created 3");
            //voronoi.Relax();
            //DrawVoronoi(voronoi, perlin, "Voronoi4");
            //Console.WriteLine("Created 4");
            //voronoi.Relax();
            //DrawVoronoi(voronoi, perlin, "Voronoi5");
            //Console.WriteLine("Created 5");

        }

        //public static void DrawVoronoi(VoronoiDiagramm _voronoi, PerlinNoise _perlin, string _filename)
        //{
        //    PNG image = new PNG(_voronoi.SizeX, _voronoi.SizeY, Color.White);
        //    //image = new PNG(Environment.CurrentDirectory + @"\green.png");

        //    for (int x = 0; x < _voronoi.SizeX; x++)
        //    {
        //        for (int y = 0; y < _voronoi.SizeY; y++)
        //        {
        //            if (_voronoi[x, y] != -1)
        //            {
        //                Color color = GetColor(_voronoi.m_Dots[_voronoi[x, y]].m_Index, _voronoi, _perlin);
        //                image[x, y] = color;
        //            }
        //        }
        //    }

        //    foreach (Voronoi.Dot dot in _voronoi.m_Dots)
        //    {
        //        //image[dot.m_Pos.x, dot.m_Pos.y] = Color.Red;
        //        foreach (Voronoi.Dot neighbor in dot.m_Neighbors)
        //        {
        //            if (neighbor.m_Index != -1)
        //            {
        //                image.DrawLine(Color.Red, dot.m_Pos.x, dot.m_Pos.y, neighbor.m_Pos.x, neighbor.m_Pos.y);
        //            }
        //        }
        //    }

        //    image.Save(Environment.CurrentDirectory + @"\" + _filename + ".png");
        //    image.Dispose();
        //}

        public static void DrawPerlin(PerlinNoiseDiagramm _perlin, string _filename)
        {
            PNG image = new PNG(_perlin.m_Detail * 10, _perlin.m_Detail * 10, Color.Green);
            //image = new PNG(Environment.CurrentDirectory + @"\" + _filename + ".png");

            for (int x = 0; x < image.SizeX; x++)
            {
                for (int y = 0; y < image.SizeY; y++)
                {
                     int value = (int)(255 * _perlin[((float)x / image.SizeX), ((float)y / image.SizeY)]);
                     image[x, y] = Color.FromArgb(255, value, value, value);
                }
            }

            image.Save(Environment.CurrentDirectory + @"\" + _filename + ".png");
            image.Dispose();
        }

        //public static Color GetColor(int _index, VoronoiDiagramm _voronoi, PerlinNoise _perlin)
        //{
        //    int cellHeight = _voronoi.m_Dots[_index].m_Height;
        //    int clampCellHeight = Math.Clamp(cellHeight, 0, 1);
        //    int clampCellHeightAdd = 0;// Math.Clamp(cellHeight, 0, 4);
        //    float x = (_voronoi.m_Dots[_index].m_Pos.x / (float)_voronoi.SizeX);
        //    float y = (_voronoi.m_Dots[_index].m_Pos.y / (float)_voronoi.SizeY);
        //    float perlinValue = _perlin[x, y] - 0.5f;
        //    float height = perlinValue * clampCellHeight + (clampCellHeightAdd * 0.1f);

        //    //return Color.FromArgb(255, _index * 2, _index * 2, _index * 2);
        //    //return Color.FromArgb(255, cellHeight * 10, cellHeight * 10, cellHeight * 10);
        //    //return Color.FromArgb(255 ,(int)(height * 255), (int)(height * 255), (int)(height * 255));
        //    if (height <= 0f)
        //    {
        //        return Color.Blue;
        //    }
        //    else
        //    {
        //        return Color.LightGreen;
        //    }
        //}

        static void Test(int _faces, int _seed, int _repeat, Point _size)
        {
            VoronoiDiagramm _diagramm = VoronoiGenerator.GenerateVoronoi(_faces, _seed);
            _diagramm.Draw(_size, "Voronoi0");

            for (int i = 0; i < _repeat; i++)
            {
                _diagramm = _diagramm.Relax();
                _diagramm.Draw(_size, "Voronoi" + (i + 1).ToString());
            }
        }

    }
        
    //struct Vector2Int
    //{
    //    public int x, y;

    //    public Vector2Int(int _x, int _y)
    //    {
    //        x = _x;
    //        y = _y;
    //    }

    //    public static Vector2Int operator -(Vector2Int a, Vector2Int b) { return new Vector2Int(a.x - b.x, a.y - b.y); }
    //    public static Vector2Int operator +(Vector2Int a, Vector2Int b) { return new Vector2Int(a.x + b.x, a.y + b.y); }
    //    public static Vector2Int operator /(Vector2Int a, int b) { return new Vector2Int(a.x / b, a.y / b); }
    //    public static Vector2Int operator *(Vector2Int a, int b) { return new Vector2Int(a.x * b, a.y * b); }
    //}

    //class PNG
    //{
    //    Bitmap m_Bitmap;
    //    Graphics m_Graphic;

    //    public int SizeX
    //    {
    //        get { return m_Bitmap.Width; }
    //    }

    //    public int SizeY
    //    {
    //        get { return m_Bitmap.Height; }
    //    }

    //    public PNG(int _sizeX, int _sizeY)
    //    {
    //        m_Bitmap = new Bitmap(_sizeX, _sizeY);
    //        m_Graphic = Graphics.FromImage(m_Bitmap);
    //    }

    //    public PNG(int _sizeX, int _sizeY, Color _baseColor)
    //    {
    //        m_Bitmap = new Bitmap(_sizeX, _sizeY);
    //        m_Graphic = Graphics.FromImage(m_Bitmap);
    //        Clear(_baseColor);
    //    }

    //    public PNG(string _filename)
    //    {
    //        FileStream stream = new FileStream(_filename, FileMode.Open, FileAccess.Read);
    //        m_Bitmap = new Bitmap(Image.FromStream(stream));
    //        m_Graphic = Graphics.FromImage(m_Bitmap);
    //        stream.Dispose();
    //    }

    //    ~PNG()
    //    {
    //        Dispose();
    //    }

    //    public void Dispose()
    //    {
    //        if(m_Graphic != null)
    //        {
    //            m_Graphic.Dispose();
    //        }

    //        if(m_Bitmap != null)
    //        {
    //            m_Bitmap.Dispose();
    //        }
    //    }

    //    public Color this[int _x, int _y]
    //    {
    //        get { return m_Bitmap.GetPixel(_x, _y); }
    //        set { m_Graphic.FillRectangle(new SolidBrush(value), _x, _y, 1, 1); }
    //    }

    //    public void Clear(Color _color)
    //    {
    //        m_Graphic.Clear(_color);
    //    }

    //    public void Save(string _filename)
    //    {
    //        FileStream stream = new FileStream(_filename, FileMode.OpenOrCreate, FileAccess.Write);
    //        m_Bitmap.Save(stream, ImageFormat.Png);
    //        stream.Dispose();
    //    }

    //    public void DrawLine(Color _color, int _aX, int _aY, int _bX, int _bY)
    //    {
    //        m_Graphic.DrawLine(new Pen(new SolidBrush(_color)), new Point(_aX, _aY), new Point(_bX, _bY));
    //    }
    //}
}
