using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerlinNoise
{
    class PerlinNoiseDiagramm
    {
        public readonly int m_Detail;
        private float[,] m_Noise;

        public PerlinNoiseDiagramm(int _detail, int _seed)
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

                _x *= m_Detail - 1;
                _y *= m_Detail - 1;

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
}
