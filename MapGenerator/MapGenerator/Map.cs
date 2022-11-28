﻿using PerlinNoise;
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
        Land = 1,
        Lake = 2,
    }

    class Map
    {

        
    }

    class MapGenerator
    {
        public static PNG GenerateMap(int _detail, int _seed, int _relaxions, int _smoothness, Point _imageSize)
        {
            PNG image = new PNG(_imageSize.X, _imageSize.Y, Color.White);

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

            #region Bioms

            Dictionary<VoronoiFace, EBioms> faceToBiom = new Dictionary<VoronoiFace, EBioms>();

            foreach (VoronoiFace face in faceToMapHeight.Keys)
            {
                if (faceToMapHeight[face] <= (1f / maxHeight) * 1.5f)
                {
                    faceToBiom.Add(face, EBioms.Lake);
                }
                else
                {
                    faceToBiom.Add(face, EBioms.Land);
                }
            }

            //Fill Ocean

            foreach (VoronoiFace face in faceToHeight.Keys)
            {
                if (faceToHeight[face] == 1)
                {
                    FillOcean(face);
                    break;
                }
            }

            void FillOcean(VoronoiFace face)
            {
                faceToBiom[face] = EBioms.Ocean;

                foreach (VoronoiFace neighbor in face.Neighbors)
                {
                    if(faceToBiom[neighbor] == EBioms.Lake)
                    {
                        FillOcean(neighbor);
                    }
                }
            }

            #endregion Bioms

            #region Elevation

            Dictionary<VoronoiVertex, int> vertexToElevation = new Dictionary<VoronoiVertex, int>();

            List<VoronoiVertex> highestVertecies = new List<VoronoiVertex>();

            foreach (VoronoiFace face in faceToBiom.Keys)
            {
                if(faceToBiom[face] == EBioms.Ocean)
                {
                    foreach (VoronoiVertex vertex in face.Vertex)
                    {
                        if (!vertexToElevation.ContainsKey(vertex))
                        {
                            vertexToElevation.Add(vertex, 0);
                            highestVertecies.Add(vertex);
                        }
                    }
                }
            }

            List<VoronoiVertex> currentProcessedVertecies = new List<VoronoiVertex>();
            currentProcessedVertecies.AddRange(highestVertecies.ToArray());
            highestVertecies.Clear();

            while (currentProcessedVertecies.Count > 0)
            {
                foreach (VoronoiVertex vertex in currentProcessedVertecies)
                {
                    foreach (VoronoiEdge edge in vertex.Edges)
                    {
                        foreach (VoronoiVertex neighbor in edge.Vertex)
                        {
                            if(!vertexToElevation.ContainsKey(neighbor))
                            {
                                vertexToElevation.Add(neighbor, vertexToElevation[vertex] + 1);
                                highestVertecies.Add(neighbor);
                            }
                        }
                    }
                }
                currentProcessedVertecies.Clear();
                currentProcessedVertecies.AddRange(highestVertecies.ToArray());
                highestVertecies.Clear();
            }
            
            Dictionary<VoronoiFace, float> faceToElevation = new Dictionary<VoronoiFace, float>();
            float highestElevation = 0;

            foreach (VoronoiFace face in voronoi.Faces)
            {
                float height = 0;
                foreach (VoronoiVertex vertex in face.Vertex)
                {
                    height += vertexToElevation[vertex];
                }
                height /= face.Vertex.Length;

                if(height > highestElevation)
                {
                    highestElevation = height;
                }

                faceToElevation.Add(face, height);
            }

            #endregion Elevation

            //Draw Bioms
            //float x, y;

            //for (float i = 0; i < _imageSize.X; i++)
            //{
            //    x = i / _imageSize.X;
            //    for (float j = 0; j < _imageSize.Y; j++)
            //    {
            //        y = j / _imageSize.Y;

            //        switch (faceToBiom[voronoi[x, y]])
            //        {
            //            case EBioms.None:
            //                break;
            //            case EBioms.Ocean:
            //                {
            //                    image[(int)i, (int)j] = Color.Blue;
            //                    break;
            //                }
            //            case EBioms.Land:
            //                {
            //                    image[(int)i, (int)j] = Color.SandyBrown;
            //                    break;
            //                }
            //            case EBioms.Lake:
            //                {
            //                    image[(int)i, (int)j] = Color.Aqua;
            //                    break;
            //                }
            //            default:
            //                break;
            //        }
            //    }
            //}

            //Draw Elevation
            float x, y;

            for (float i = 0; i < _imageSize.X; i++)
            {
                x = i / _imageSize.X;
                for (float j = 0; j < _imageSize.Y; j++)
                {
                    y = j / _imageSize.Y;

                    VoronoiFace face = voronoi[x, y];
                    switch (faceToBiom[face])
                    {
                        case EBioms.None:
                            break;
                        case EBioms.Ocean:
                            {
                                image[(int)i, (int)j] = Color.MidnightBlue;
                                break;
                            }
                        case EBioms.Land:
                        case EBioms.Lake:
                            {
                                float elevationMultiplier = faceToElevation[face] / highestElevation;
                                image[(int)i, (int)j] = Color.FromArgb(255, 50 + (int)(205 * elevationMultiplier), 100 + (int)(155 * elevationMultiplier), 50 + (int)(205 * elevationMultiplier));
                                break;
                            }
                        default:
                            break;
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

            List<VoronoiVertex> highVertecies = new List<VoronoiVertex>();

            foreach (VoronoiVertex vertex in vertexToElevation.Keys)
            {
                if(vertexToElevation[vertex] > 0)
                {
                    highestVertecies.Add(vertex);
                }
            }

            int riverCount = (int)(((rng.NextDouble() * 0.2) + 0.1) * highestVertecies.Count);
            for (int i = 0; i < riverCount; i++)
            {
                VoronoiVertex curVertex = highestVertecies[rng.Next(0, highestVertecies.Count)];
                highestVertecies.Remove(curVertex);

                while (vertexToElevation[curVertex] > 0)
                {
                    VoronoiVertex nextVertex = GetLowestNeighbor(curVertex);
                    if(nextVertex == null)
                    {
                        break;
                    }
                    image.DrawLine(Color.Blue, 3, (int)(curVertex.Position.X * image.SizeX), (int)(curVertex.Position.Y * image.SizeY), (int)(nextVertex.Position.X * image.SizeX), (int)(nextVertex.Position.Y * image.SizeY));
                    curVertex = nextVertex;
                }

                VoronoiVertex GetLowestNeighbor(VoronoiVertex vertex)
                {
                    int lowest = vertexToElevation[vertex];
                    VoronoiVertex result = null;
                    foreach (VoronoiEdge edge in vertex.Edges)
                    {
                        foreach (VoronoiVertex neighbor in edge.Vertex)
                        {
                            if(vertexToElevation[neighbor] < lowest)
                            {
                                lowest = vertexToElevation[neighbor];
                                result = neighbor;
                            }
                        }
                    }
                    return result;
                }
            }


            image.Save(Environment.CurrentDirectory + @"\Step7.png");


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
