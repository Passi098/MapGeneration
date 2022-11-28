using PerlinNoise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PNGConverter;
using System.Drawing;

namespace MapGenerator
{
    enum EBioms
    {
        None = -1,
        Ocean = 0,
        Land = 1
    }

    class Map
    {

        
    }

    class MapGenerator
    {
        public static PNG GenerateMap(int _detail, int _seed, int _relaxions, int _smoothness, Point _imageSize)
        {
            PNG image = new PNG(_imageSize.X, _imageSize.Y, Color.White);
            Dictionary<VoronoiFace, EBioms> m_FaceToBiom = new Dictionary<VoronoiFace, EBioms>();

            VoronoiDiagramm voronoi = VoronoiGenerator.GenerateVoronoi(_detail, _seed);

            for (int i = 0; i < _relaxions; i++)
            {
                voronoi = voronoi.Relax();
            }

            #region FaceHeight
            
            Dictionary<VoronoiFace, int> faceToHeight = new Dictionary<VoronoiFace, int>();
            int maxHeight = 1;

            foreach (VoronoiFace face in voronoi.Faces)
            {
                if (IsFaceAtTheEdge(face))
                {
                    faceToHeight.Add(face, 1);
                }
            }

            VoronoiFace[] outerFaces = new VoronoiFace[faceToHeight.Count];
            faceToHeight.Keys.CopyTo(outerFaces, 0);

            List<VoronoiFace> innerFaces = new List<VoronoiFace>();

            while (outerFaces.Length > 0)
            {
                foreach (VoronoiFace face in outerFaces)
                {
                    foreach (VoronoiFace neighbor in face.Neighbors)
                    {
                        if (!faceToHeight.ContainsKey(neighbor))
                        {
                            maxHeight = faceToHeight[face] + 1;
                            faceToHeight.Add(neighbor, faceToHeight[face] + 1);
                            innerFaces.Add(neighbor);
                        }
                    }
                }

                outerFaces = innerFaces.ToArray();
                innerFaces.Clear();
            }

            #endregion FaceHeight

            #region MapHeight

            PerlinNoiseDiagramm perlin = new PerlinNoiseDiagramm(_smoothness, _seed);

            Dictionary<VoronoiFace, float> faceToMapHeight = new Dictionary<VoronoiFace, float>();

            Random rng = new Random(_seed);

            foreach (VoronoiFace face in voronoi.Faces)
            {
                if (faceToHeight[face] == 1)
                {
                    faceToMapHeight.Add(face, 0.0f);
                }
                else
                {


                    //int mapHeight = faceToHeight[face];
                    //float height = ((((float)rng.NextDouble()) * (1f / maxHeight)) + ((mapHeight - 1) * (1f / maxHeight)));
                    //faceToMapHeight.Add(face, height);

                    float height = perlin[face.Point.Position.X, face.Point.Position.Y];
                    faceToMapHeight.Add(face, height);
                }
            }

            #endregion MapHeight

            float x, y;

            for (float i = 0; i < _imageSize.X; i++)
            {
                x = i / _imageSize.X;
                for (float j = 0; j < _imageSize.Y; j++)
                {
                    y = j / _imageSize.Y;

                    if (faceToHeight.ContainsKey(voronoi[x, y]))
                    {
                        if (faceToMapHeight[voronoi[x, y]] <= (1f / maxHeight) * 1.5f)
                        {
                            image[(int)i, (int)j] = Color.Blue;
                        }
                        else 
                        {
                            image[(int)i, (int)j] = Color.SandyBrown;
                        }
                    }
                }
            }

            foreach (VoronoiFace face in voronoi.Faces)
            {
                image.DrawRect(Color.Black, face.Point.Position.X * _imageSize.X, face.Point.Position.Y * _imageSize.Y, _imageSize.X / 100, _imageSize.Y / 100);

                foreach (VoronoiEdge edge in face.Edges)
                {
                    //Edges
                    image.DrawLine(Color.Black, (int)(Math.Clamp(edge.Vertex[0].Position.X, 0f, 1f) * _imageSize.X), (int)(Math.Clamp(edge.Vertex[0].Position.Y, 0f, 1f) * _imageSize.Y), (int)(Math.Clamp(edge.Vertex[1].Position.X, 0f, 1f) * _imageSize.X), (int)(Math.Clamp(edge.Vertex[1].Position.Y, 0f, 1f) * _imageSize.Y));
                }
            }

            image.Save(Environment.CurrentDirectory + @"\Step4.png");


            return null;

            bool IsFaceAtTheEdge(VoronoiFace face)
            {
                foreach (VoronoiVertex vertex in face.Vertex)
                {
                    if(vertex.Position.X <= 0 || vertex.Position.X >= 1 || vertex.Position.Y <= 0 || vertex.Position.Y >= 1)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
