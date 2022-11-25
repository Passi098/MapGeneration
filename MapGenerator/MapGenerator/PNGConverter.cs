using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace PNGConverter
{
    class PNG
    {
        Bitmap m_Bitmap;
        Graphics m_Graphic;

        public int SizeX
        {
            get { return m_Bitmap.Width; }
        }

        public int SizeY
        {
            get { return m_Bitmap.Height; }
        }

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
            if (m_Graphic != null)
            {
                m_Graphic.Dispose();
            }

            if (m_Bitmap != null)
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

        public void DrawLine(Color _color, int _aX, int _aY, int _bX, int _bY)
        {
            m_Graphic.DrawLine(new Pen(new SolidBrush(_color)), new Point(_aX, _aY), new Point(_bX, _bY));
        }

        public void DrawLine(Color _color, PointF _a, PointF _b)
        {
            m_Graphic.DrawLine(new Pen(new SolidBrush(_color)), _a, _b);
        }

        public void DrawRect(Color _color, float _x, float _y, float _width, float _height)
        {
            m_Graphic.DrawRectangle(new Pen(new SolidBrush(_color)), _x, _y, _width, _height);
        }
    }
}
