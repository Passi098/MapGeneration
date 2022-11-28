using System;
using System.Collections.Generic;
using System.Drawing;
using PNGConverter;

// Diese Klasse implementiert eine Vergleichsmethode für das Sortieren der Punkte
class PointComparer : Comparer<PointF>
{
	// Vergleichsmethode, die die Punkte von links nach rechts und bei Gleichheit von oben nach unten sortiert
	public override int Compare(PointF point1, PointF point2)
	{
		int result = point1.X.CompareTo(point2.X);
		if (result == 0)
		{
			return point1.Y.CompareTo(point2.Y);
		}
		return result;
	}
}

// Diese Klasse implementiert eine Vergleichsmethode für das Sortieren der CircleEvents
class EventComparer : Comparer<CircleEvent>
{
	// Vergleichsmethode, die die CircleEvents
	public override int Compare(CircleEvent event1, CircleEvent event2)
	{
		return event1.x.CompareTo(event2.x);
	}
}

// Klasse für die CircleEvents
class CircleEvent
{
	public double x; // x-Koordinate der Sweep Line
	public PointF point; // Das Zentrum, das der Brennpunkt der Parabel ist
	public ParabolaArc arc; // Parabelbogen
	public bool isValid; // Gibt an, ob das CircleEvent aktuell ist

	// Konstruktor für die Initialisierung der Attribute
	public CircleEvent(double _x, PointF _point, ParabolaArc _arc)
	{
		x = _x;
		point = _point;
		arc = _arc;
		isValid = true;
	}
};

// Klasse für die Parabelbögen
class ParabolaArc
{
	public PointF point; // Das Zentrum, das der Brennpunkt der Parabel ist
	public ParabolaArc previousArc, nextArc; // Benachbarte Parabelbögen
	public CircleEvent circleEvent; // Zugeordnetes CircleEvent
	private VEdge edge1, edge2; // Benachbarte Voronoi-Kanten

	public VEdge Edge1
    {
        get { return edge1; }
		set { edge1 = value; m_Face.AddEdge(edge1); }
    }

	public VEdge Edge2
    {
        get { return edge2; }
		set { edge2 = value; m_Face.AddEdge(edge2); }
    }

	public VFace m_Face;

	// Konstruktor für die Initialisierung der Attribute
	public ParabolaArc(PointF _point, ParabolaArc arc1, ParabolaArc arc2)
	{
		point = _point;
		previousArc = arc1;
		nextArc = arc2;
		circleEvent = null;
		edge1 = null;
		edge2 = null;

		m_Face = new VFace(point);
	}
}

public class VFace
{
	private static int m_Count = 0;
	public readonly int m_Index;
	public static List<VFace> m_Instaces = new List<VFace>();

	public PointF m_Point;

	public List<VEdge> m_Edges = new List<VEdge>();

    public VFace(PointF _point)
	{
		m_Index = m_Count;
		m_Count++;
		m_Point = _point;
		m_Instaces.Add(this);
    }

	public void AddEdge(VEdge _edge)
    {
		if(m_Edges.Contains(_edge) || _edge == null)
        {
			return;
        }
		m_Edges.Add(_edge);
		_edge.m_Faces.Add(this);
    }
}

// Klasse für die Voronoi-Kanten
public class VEdge
{
	private static int m_Count = 0;
	public readonly int m_Index;
	public VVertex m_Vertex1, m_Vertex2; // Endpunkte der Kante
	public bool isFinished; // Gibt an, ob die Kante fertiggestellt wurde
	public static List<VEdge> edges = new List<VEdge>(); // Liste der Kanten
	public List<VFace> m_Faces = new List<VFace>();

	// Konstruktor für die Initialisierung der Attribute
	public VEdge(VVertex _start)
	{
		m_Index = m_Count;
		m_Count++;
		m_Vertex1 = _start; // Setzt den ersten Endpunkt
		m_Vertex1.AddEdge(this);
		m_Vertex2 = null;
		isFinished = false;
		edges.Add(this); // Fügt die Kante der Liste hinzu
	}

	// Diese Methode stellt die Kante fertig
	public void Finish(VVertex _end)
	{
		if (!isFinished)
		{
			m_Vertex2 = _end; // Setzt den zweiten Endpunkt
			m_Vertex2.AddEdge(this);
			isFinished = true;
		}
	}

	public void GotReplaced(VEdge _edge)
    {
        foreach (VFace face in m_Faces)
        {
			face.m_Edges.Remove(this);
			face.AddEdge(_edge);
        }
    }
}

public class VVertex
{
	private static int m_Count = 0;
	public static List<VVertex> m_Instances = new List<VVertex>();

	public readonly int m_Index;
	public readonly PointF m_Pos;

	public List<VEdge> m_Edges = new List<VEdge>();

    public VVertex(PointF _pos)
    {
		m_Index = m_Count;
		m_Count++;
		m_Pos = _pos;
		m_Instances.Add(this);
    }

	public void AddEdge(VEdge _edge)
    {
		m_Edges.Add(_edge);
    }

	public void RemoveEdge(VEdge _edge)
    {
		m_Edges.Remove(_edge);
    }
}

// Klasse, die die Methoden für den Algorithmus von Fortune deklariert
class Fortune
{
	public ParabolaArc root = null;
	public List<PointF> points = new List<PointF>(); // Liste der Punkte
	public List<CircleEvent> circleEvents = new List<CircleEvent>(); // Liste der CircleEvents

	// Verarbeitet den verbleibenden Punkt (rechts der Sweep Line), der ganz links liegt
	public void ProcessPoint(double x)
	{
		PointF point = points[0];
		points.RemoveAt(0); // Entfernt den ersten Punkt aus der Liste
		AddNewArc(point, x); // Fügt der Beach Line einen Parabelbogen hinzu
	}

	// Diese Methode verarbeitet das CircleEvent mit der kleinsten x-Koordinate
	public void ProcessCircleEvent()
	{
		CircleEvent circleEvent = circleEvents[0];
		circleEvents.RemoveAt(0); // Entfernt das erste CircleEvent aus der Liste
		if (circleEvent.isValid) // Wenn das CircleEvent aktuell ist
		{
			VVertex vertex = new VVertex(circleEvent.point);
			VEdge edge = new VEdge(vertex); // Erzeugt eine neue Kante
																   // Entfernt den zugehörigen Parabelbogen
			ParabolaArc arc = circleEvent.arc;
			if (arc.previousArc != null)
			{
				arc.previousArc.nextArc = arc.nextArc;
				arc.previousArc.Edge2 = edge;
			}
			if (arc.nextArc != null)
			{
				arc.nextArc.previousArc = arc.previousArc;
				arc.nextArc.Edge1 = edge;
			}
			// Stellt die benachbarten Kanten des Parabelbogens fertig
			if (arc.Edge1 != null)
			{
				arc.Edge1.Finish(vertex);
			}
			if (arc.Edge2 != null)
			{
				arc.Edge2.Finish(vertex);
			}
			// Prüft die CircleEvents auf beiden Seiten des Parabelbogens
			if (arc.previousArc != null)
			{
				CheckCircleEvent(arc.previousArc, circleEvent.x);
			}
			if (arc.nextArc != null)
			{
				CheckCircleEvent(arc.nextArc, circleEvent.x);
			}
		}
	}

	// Diese Methode fügt einen neuen Parabelbogen mit dem gegebenen Brennpunkt hinzu
	public void AddNewArc(PointF point, double x)
	{
		if (root == null)
		{
			root = new ParabolaArc(point, null, null);
			return;
		}
		// Bestimmt den aktuellen Parabelbogen mit der y-Koordinate des Punkts point
		ParabolaArc arc;
		for (arc = root; arc != null; arc = arc.nextArc) // Diese for-Schleife durchläuft die Parabelbögen
		{
			PointF intersection1 = new PointF(0, 0), intersection2 = new PointF(0, 0);
			if (GetIntersection(point, arc, ref intersection1)) // Wenn die neue Parabel den Parabelbogen schneidet
			{
				// Dupliziert gegebenenfalls den Parabelbogen
				if (arc.nextArc != null && !GetIntersection(point, arc.nextArc, ref intersection2))
				{
					arc.nextArc.previousArc = new ParabolaArc(arc.point, arc, arc.nextArc);
					arc.nextArc = arc.nextArc.previousArc;
				}
				else
				{
					arc.nextArc = new ParabolaArc(arc.point, arc, null);
				}
				arc.nextArc.Edge2 = arc.Edge2;
				// Fügt einen neuen Parabelbogen zwischen den Parabelbögen arc und arc.nextArc ein.
				arc.nextArc.previousArc = new ParabolaArc(point, arc, arc.nextArc);
				arc.nextArc = arc.nextArc.previousArc;
				arc = arc.nextArc;
				// Verbindet 2 neue Kanten mit den Endpunkten des Parabelbogens
				VVertex vertex = new VVertex(intersection1);
				arc.previousArc.Edge2 = arc.Edge1 = new VEdge(vertex);
				arc.nextArc.Edge1 = arc.Edge2 = new VEdge(vertex);
				// Prüft die benachbarten CircleEvents des Parabelbogens
				CheckCircleEvent(arc, point.X);
				CheckCircleEvent(arc.previousArc, point.X);
				CheckCircleEvent(arc.nextArc, point.X);
				return;
			}
		}
		// Spezialfall: Wenn der Parabelbogen keinen anderen schneidet, wird er der doppelt verketteten Liste hinzugefügt
		for (arc = root; arc.nextArc != null; arc = arc.nextArc) ; // Bestimmt den letzten Parabelbogen
		arc.nextArc = new ParabolaArc(point, arc, null);
		// Fügt eine Kante zwischen den Parabelbögen ein
		PointF start = new PointF(0, 0);
		start.X = (float)x;
		start.Y = (arc.nextArc.point.Y + arc.point.Y) / 2;

		VVertex vertex2 = new VVertex(start);
		arc.Edge2 = arc.nextArc.Edge1 = new VEdge(vertex2);
	}

	// Diese Methode erzeugt wenn nötig ein neues CircleEvent für den gegebenen Parabelbogen
	public void CheckCircleEvent(ParabolaArc arc, double _x)
	{
		// Setzt das bisherige CircleEvent auf nicht aktuell
		if (arc.circleEvent != null && arc.circleEvent.x != _x)
		{
			arc.circleEvent.isValid = false;
		}
		arc.circleEvent = null;
		if (arc.previousArc == null || arc.nextArc == null)
		{
			return;
		}
		double x = 0;
		PointF point = new PointF(0, 0);
		if (GetRightmostCirclePoint(arc.previousArc.point, arc.point, arc.nextArc.point, ref x, ref point) && x > _x)
		{
			arc.circleEvent = new CircleEvent(x, point, arc); // Erzeugt ein neues CircleEvent
			circleEvents.Add(arc.circleEvent);
			circleEvents.Sort(new EventComparer()); // Sortiert die Liste
		}
	}

	// Diese Methode bestimmt die x-Koordinate des Kreises durch die 3 Punkte, der ganz rechts liegt und prüft, ob die 3 Punkte auf einer Geraden liegen
	public bool GetRightmostCirclePoint(PointF point1, PointF point2, PointF point3, ref double x, ref PointF point)
	{
		// Prüft, dass die 3 Punkt im Uhrzeigersinn orientiert sind
		if ((point2.X - point1.X) * (point3.Y - point1.Y) > (point3.X - point1.X) * (point2.Y - point1.Y))
		{
			return false;
		}
		double x1 = point2.X - point1.X;
		double y1 = point2.Y - point1.Y;
		double a = 2 * (x1 * (point3.Y - point2.Y) - y1 * (point3.X - point2.X));
		if (a == 0)
		{
			return false;  // Wenn die 3 Punkte auf einer Geraden liegen, wird false zurückgegeben
		}
		double x2 = point3.X - point1.X;
		double y2 = point3.Y - point1.Y;
		double a1 = x1 * (point1.X + point2.X) + y1 * (point1.Y + point2.Y);
		double a2 = x2 * (point1.X + point3.X) + y2 * (point1.Y + point3.Y);
		// Berechnet den Mittelpunkt des Kreises durch die 3 Punkte
		point.X = (float)((y2 * a1 - y1 * a2) / a);
		point.Y = (float)((x1 * a2 - x2 * a1) / a);
		x = point.X + Math.Sqrt(Math.Pow(point1.X - point.X, 2) + Math.Pow(point1.Y - point.Y, 2)); // x-Koordinate des Mittelpunkts plus Radius
		return true;
	}

	// Diese Methode bestimmt gegebenenfalls den Schnittpunkt zwischen der Parabel mit dem gegebenen Brennpunkt und dem Parabelbogen und prüft, ob er existiert
	public bool GetIntersection(PointF point, ParabolaArc arc, ref PointF intersection)
	{
		if (arc.point.X == point.X)
		{
			return false; // Wenn die Brennpunkte übereinstimmen, wird false zurückgegeben
		}
		double y1 = 0, y2 = 0;
		if (arc.previousArc != null)
		{
			y1 = GetParabolasIntersection(arc.previousArc.point, arc.point, point.X).Y; // Berechnet die y-Koordinate des Schnittpunkts zwischen dem aktuellen und dem vorherigen Parabelbogen
		}
		if (arc.nextArc != null)
		{
			y2 = GetParabolasIntersection(arc.point, arc.nextArc.point, point.X).Y; // Berechnet die y-Koordinate des Schnittpunkts zwischen dem aktuellen und dem nächsten Parabelbogen
		}
		// Berechnet die Koordinaten des Schnittpunkts, falls vorhanden
		if ((arc.previousArc == null || y1 <= point.Y) && (arc.nextArc == null || point.Y <= y2))
		{
			intersection.Y = point.Y;
			intersection.X = (arc.point.X * arc.point.X + (arc.point.Y - intersection.Y) * (arc.point.Y - intersection.Y) - point.X * point.X) / (2 * arc.point.X - 2 * point.X);
			return true;
		}
		return false;
	}

	// Diese Methode bestimmt den Schnittpunkt zwischen den Parabeln mit den gegebenen Brennpunkten für die Sweep Line mit der gegebenen x-Koordinate
	public PointF GetParabolasIntersection(PointF point1, PointF point2, double x)
	{
		PointF intersection = new PointF(0, 0), point = point1;
		// Spezialfälle
		if (point1.X == point2.X)
		{
			intersection.Y = (point1.Y + point2.Y) / 2;
		}
		else if (point2.X == x)
		{
			intersection.Y = point2.Y;
		}
		else if (point1.X == x)
		{
			intersection.Y = point1.Y;
			point = point2;
		}
		else // Standardfall
		{
			// Verwendet die Lösungsformel für quadratische Gleichungen, um die y-Koordinate des Schnittpunkts zu berechnen
			double x1 = 2 * (point1.X - x);
			double x2 = 2 * (point2.X - x);
			double a = 1 / x1 - 1 / x2;
			double b = -2 * (point1.Y / x1 - point2.Y / x2);
			double c = (point1.Y * point1.Y + point1.X * point1.X - x * x) / x1 - (point2.Y * point2.Y + point2.X * point2.X - x * x) / x2;
			intersection.Y = (float)((-b - Math.Sqrt(b * b - 4 * a * c)) / (2 * a));
		}
		// Setzt die y-Koordinate in die Parabelgleichung ein, um die x-Koordinate zu berechnen
		intersection.X = (float)((point.X * point.X + (point.Y - intersection.Y) * (point.Y - intersection.Y) - x * x) / (2 * point.X - 2 * x));
		return intersection;
	}

	// Diese Methode stellt die benachbarten Kanten der Parabelbögen fertig
	public void FinishEdges(double x1, double y1, double x2, double y2)
	{
		// Verschiebt die Sweep Line, sodass keine Parabel die Zeichenfläche schneiden kann
		double x = x2 + (x2 - x1) + (y2 - y1);
		// Verlängert jede benachbarte Kante bis zum Schnittpunkt der neuen Parabeln
		for (ParabolaArc arc = root; arc.nextArc != null; arc = arc.nextArc)
		{
			if (arc.Edge2 != null)
			{
				arc.Edge2.Finish(new VVertex(GetParabolasIntersection(arc.point, arc.nextArc.point, 2 * x)));
			}
		}
	}
}

public static class VoronoiGenerator
{
	public static VoronoiDiagramm GenerateVoronoi(int _numberOfPoints, int _seed)
	{
		List<PointF> points = new List<PointF>(); // Liste der Punkte
		Random random = new Random(_seed); // Initialisiert den Zufallsgenerator
		for (int i = 0; i < _numberOfPoints; i++) // Diese for-Schleife erzeugt 10 zufällige Punkte innerhalb der quadratischen Zeichenfläche
		{
			PointF point = new PointF();
			point.X = (float)(random.NextDouble());
			point.Y = (float)(random.NextDouble());
			points.Add(point); // Fügt den Punkt der Liste hinzu
		}

		return GenerateVoronoi(points.ToArray());
    }
	public static VoronoiDiagramm GenerateVoronoi(PointF[] _points)
	{
		VFace.m_Instaces.Clear();
		VVertex.m_Instances.Clear();
		VEdge.edges.Clear();
		List<PointF> points = new List<PointF>();
		points.AddRange(_points);
		points.Sort(new PointComparer()); // Sortiert die Punkte
		Fortune fortune = new Fortune(); // Erzeugt ein Objekt der Klasse Fortune
		fortune.points.AddRange(points); // Fügt die Punkte der Liste hinzu

		// Diese for-Schleife verarbeitet bei jedem Durchlauf das Element mit der kleinsten x-Koordinate
		while (fortune.points.Count != 0) // Solange die Liste der Punkte nicht leer ist
		{
			if (fortune.circleEvents.Count != 0 && fortune.circleEvents[0].x <= fortune.points[0].X)
			{
				fortune.ProcessCircleEvent(); // Aufruf der Methode, verarbeitet das CircleEvent
			}
			else
			{
				fortune.ProcessPoint(1); // Aufruf der Methode, verarbeitet den Punkt
			}
		}
		// Nachdem alle Punkte verarbeitet wurden, werden die verbleibenden circleEvents verarbeitet.
		while (fortune.circleEvents.Count != 0) // Solange die Liste der CircleEvents nicht leer ist
		{
			fortune.ProcessCircleEvent();
		}
		fortune.FinishEdges(0, 0, 1, 1); // Aufruf der Methode, stellt die benachbarten Kanten der Parabelbögen fertig


		//Vertex mit nur 2 Edges entfernen und Edges verbinden
        foreach (VVertex vertex in VVertex.m_Instances)
        {
			if(vertex.m_Edges.Count == 2)
            {
				VVertex other1 = vertex.m_Edges[0].m_Vertex1 == vertex ? vertex.m_Edges[0].m_Vertex2 : vertex.m_Edges[0].m_Vertex1;
				VVertex other2 = vertex.m_Edges[1].m_Vertex1 == vertex ? vertex.m_Edges[1].m_Vertex2 : vertex.m_Edges[1].m_Vertex1;

				VEdge newEdge = new VEdge(other1);
				newEdge.Finish(other2);

				other1.RemoveEdge(vertex.m_Edges[0]);
				other2.RemoveEdge(vertex.m_Edges[1]);

				vertex.m_Edges[0].GotReplaced(newEdge);
				vertex.m_Edges[1].GotReplaced(newEdge);

				VEdge.edges.Remove(vertex.m_Edges[0]);
				VEdge.edges.Remove(vertex.m_Edges[1]);
				vertex.m_Edges.Clear();
            }
        }

        for (int i = 0; i < VVertex.m_Instances.Count;)
        {
			if(VVertex.m_Instances[i].m_Edges.Count <= 0)
            {
				VVertex.m_Instances.RemoveAt(i);
            }
			else
            {
				i++;
            }
        }

        foreach (VFace face in VFace.m_Instaces)
        {
			face.m_Edges.RemoveAll(item => item == null);
        }

        for (int current = 0; current < VFace.m_Instaces.Count; current++)
        {
            for (int other = current + 1; other < VFace.m_Instaces.Count; )
            {
				if(VFace.m_Instaces[current].m_Point.Equals(VFace.m_Instaces[other].m_Point))
                {
                    foreach (VEdge edge in VFace.m_Instaces[other].m_Edges)
                    {
						edge.m_Faces.Remove(VFace.m_Instaces[other]);
						edge.m_Faces.Add(VFace.m_Instaces[current]);

						if(!VFace.m_Instaces[current].m_Edges.Contains(edge))
                        {
							VFace.m_Instaces[current].m_Edges.Add(edge);
						}
                    }

					VFace.m_Instaces.RemoveAt(other);
                }
				else
                {
					other++;
                }
            }
        }

        foreach (VVertex vertex in VVertex.m_Instances)
        {
            if (vertex.m_Edges.Count == 1)
            {
                if(vertex.m_Pos.X < 0)
                {

                }
				else if(vertex.m_Pos.X > 1)
                {

                }

				if (vertex.m_Pos.Y < 0)
				{

				}
				else if (vertex.m_Pos.Y > 1)
				{

				}
			}
        }

		return new VoronoiDiagramm(VFace.m_Instaces.ToArray(), VVertex.m_Instances.ToArray(), VEdge.edges.ToArray());

    }

	private static void LogVertex()
	{
		foreach (VVertex vertex in VVertex.m_Instances)
		{
			Console.Write("Vertex" + vertex.m_Index + ": {");

			for (int i = 0; i < vertex.m_Edges.Count; i++)
			{
				if (i > 0)
				{
					Console.Write(", ");
				}
				Console.Write(vertex.m_Edges[i].m_Index);
			}
			Console.WriteLine("}");
		}
	}

	private static void LogEdges()
	{
		foreach (VEdge edge in VEdge.edges)
		{
			Console.WriteLine("Edge" + edge.m_Index + ": {" + edge.m_Vertex1.m_Index + "/" + edge.m_Vertex2.m_Index + "}");
		}
	}

	private static void LogFaces()
	{
		foreach (VFace face in VFace.m_Instaces)
		{
			Console.Write("Face" + face.m_Index + ": {");

			for (int i = 0; i < face.m_Edges.Count; i++)
			{
				if (i > 0)
				{
					Console.Write(", ");
				}
				Console.Write(face.m_Edges[i].m_Index);
			}
			Console.WriteLine("}");
		}
	}
}

public class VoronoiDiagramm
{
	private readonly VoronoiFace[] m_Faces;
	private readonly PointF m_SearchArea;
	private readonly PointF m_HalfSearchArea;

	public VoronoiFace[] Faces
    {
		get { return m_Faces; }
    }

    public VoronoiFace this[float _x, float _y]
    {
        get
        {
			//_x = Math.Clamp(_x, 0f, 1f);
			//_y = Math.Clamp(_y, 0f, 1f);

			VoronoiFace face = null;
			float distance = -1;

            for (int i = 0; i < m_Faces.Length; i++)
            {
				float sqrdX = m_Faces[i].Point.Position.X - _x;
				sqrdX *= sqrdX;

				float sqrdY = m_Faces[i].Point.Position.Y - _y;
				sqrdY *= sqrdY;

				if (distance > (sqrdX + sqrdY) || distance < 0)
                {
					distance = (sqrdX + sqrdY);
					face = m_Faces[i];
				}
            }
			return face;
        }
    }

    public VoronoiDiagramm(VFace[] _faces, VVertex[] _vertex, VEdge[] _edges)
    {
		Dictionary<int, VoronoiFace> faceDictionary = new Dictionary<int, VoronoiFace>();
		Dictionary<int, VoronoiVertex> vertexDictionary = new Dictionary<int, VoronoiVertex>();
		Dictionary<int, VoronoiEdge> edgeDictionary = new Dictionary<int, VoronoiEdge>();

		List<VoronoiFace> sortFaces = new List<VoronoiFace>();

        for (int i = 0; i < _faces.Length; i++)
        {
			VoronoiFace face = new VoronoiFace(_faces[i]);
			sortFaces.Add(face);
			faceDictionary.Add(_faces[i].m_Index, face);
        }

        foreach (VVertex vertex in _vertex)
		{
			List<VoronoiFace> faces = new List<VoronoiFace>();

            foreach (VEdge edge in vertex.m_Edges)
            {
                foreach (VFace face in edge.m_Faces)
				{
					if (!faces.Contains(faceDictionary[face.m_Index]))
					{
						faces.Add(faceDictionary[face.m_Index]);
					}
				}
			}

			vertexDictionary.Add(vertex.m_Index, new VoronoiVertex(vertex.m_Pos, faces.ToArray()));
		}

        foreach (VEdge edge in _edges)
        {
			List<VoronoiFace> faces = new List<VoronoiFace>();

            foreach (VFace face in edge.m_Faces)
            {
				if(!faces.Contains(faceDictionary[face.m_Index]))
                {
					faces.Add(faceDictionary[face.m_Index]);
                }
            }

			List<VoronoiVertex> vertecies = new List<VoronoiVertex>();

            if(edge.m_Vertex1 != null)
            {
				vertecies.Add(vertexDictionary[edge.m_Vertex1.m_Index]);
			}

			if (edge.m_Vertex2 != null)
			{
				vertecies.Add(vertexDictionary[edge.m_Vertex2.m_Index]);
			}

			edgeDictionary.Add(edge.m_Index, new VoronoiEdge(faces.ToArray(), vertecies.ToArray()));
		}

        foreach (VVertex vertex in _vertex)
        {
			List<VoronoiEdge> edges = new List<VoronoiEdge>();

            foreach (VEdge edge in vertex.m_Edges)
            {
				edges.Add(edgeDictionary[edge.m_Index]);
            }

			vertexDictionary[vertex.m_Index].SetEdges(edges.ToArray());
        }

        foreach (VFace face in _faces)
        {
			List<VoronoiEdge> edges = new List<VoronoiEdge>();
			List<VoronoiVertex> vertecies = new List<VoronoiVertex>();

            foreach (VEdge edge in face.m_Edges)
            {
				edges.Add(edgeDictionary[edge.m_Index]);

				if(!vertecies.Contains(vertexDictionary[edge.m_Vertex1.m_Index]))
                {
					vertecies.Add(vertexDictionary[edge.m_Vertex1.m_Index]);
				}

				if (!vertecies.Contains(vertexDictionary[edge.m_Vertex2.m_Index]))
				{
					vertecies.Add(vertexDictionary[edge.m_Vertex2.m_Index]);
				}
			}

			faceDictionary[face.m_Index].SetEdges(edges.ToArray());
			faceDictionary[face.m_Index].SetVertex(vertecies.ToArray());
        }

		sortFaces.Sort(new FaceComparer());

		m_Faces = sortFaces.ToArray();

		m_SearchArea = CalcSearchArea();
		m_HalfSearchArea = new PointF(m_SearchArea.X / 2f, m_SearchArea.Y / 2f);

		PointF CalcSearchArea()
        {
			float lowestX = 1, lowestY = 1, highestX = 0, highestY = 0;
			PointF result = new PointF(0, 0);
            foreach (VoronoiFace face in m_Faces)
            {
				if (face.Point.Position.X > highestX)
				{
					highestX = face.Point.Position.X;
				}
				if(face.Point.Position.X < lowestX)
                {
					lowestX = face.Point.Position.X;
				}

				if (face.Point.Position.Y > highestY)
				{
					highestY = face.Point.Position.Y;
				}
				if (face.Point.Position.Y < lowestY)
				{
					lowestY = face.Point.Position.Y;
				}

				foreach (VoronoiFace neighbor in face.Neighbors)
                {
					float x = MathF.Abs(face.Point.Position.X - neighbor.Point.Position.X);
					float y = MathF.Abs(face.Point.Position.Y - neighbor.Point.Position.Y);
					
					if (x > result.X)
                    {
						result.X = x;
					}
					if (y > result.Y)
					{
						result.Y = y;
					}
				}
            }

			if(lowestX > result.X)
            {
				result.X = lowestX * 2;
            }
			if(1f - highestX > result.X)
            {
				result.X = (1f - highestX) * 2;
            }

			if (lowestY > result.Y)
			{
				result.Y = lowestY * 2;
			}
			if (1f - highestY > result.Y)
			{
				result.Y = (1f - highestY) * 2;
			}
			return result;
        }
    }

	public void Draw(Point _size, string _name)
    {
		PNG png = new PNG(_size.X, _size.Y, Color.LightGray);

    //    for (int x = 0; x < _size.X; x++)
    //    {
    //        for (int y = 0; y < _size.Y; y++)
    //        {
				//VoronoiFace face = this[(x / (float)_size.X), (y / (float)_size.Y)];
				//png[x, y] = Color.FromArgb(255, (int)(Math.Clamp(face.Point.Position.X, 0f, 1f) * 255), (int)(Math.Clamp(face.Point.Position.Y, 0f, 1f) * 255), 0);
    //        }
    //    }

        foreach (VoronoiFace face in m_Faces)
        {
            png.DrawRect(Color.Red, face.Point.Position.X * _size.X, face.Point.Position.Y * _size.Y, _size.X / 100, _size.Y / 100);

            foreach (VoronoiEdge edge in face.Edges)
            {
				//Edges
                png.DrawLine(Color.White, (int)(Math.Clamp(edge.Vertex[0].Position.X, 0f, 1f) * _size.X), (int)(Math.Clamp(edge.Vertex[0].Position.Y, 0f, 1f) * _size.Y), (int)(Math.Clamp(edge.Vertex[1].Position.X, 0f, 1f) * _size.X), (int)(Math.Clamp(edge.Vertex[1].Position.Y, 0f, 1f) * _size.Y));
                //Triangulation
				png.DrawLine(Color.Black, (int)(Math.Clamp(edge.Faces[0].Point.Position.X, 0f, 1f) * _size.X), (int)(Math.Clamp(edge.Faces[0].Point.Position.Y, 0f, 1f) * _size.Y), (int)(Math.Clamp(edge.Faces[1].Point.Position.X, 0f, 1f) * _size.X), (int)(Math.Clamp(edge.Faces[1].Point.Position.Y, 0f, 1f) * _size.Y));

                foreach (VoronoiVertex vertex in edge.Vertex)
                {
					png.DrawRect(Color.Blue, (int)(Math.Clamp(vertex.Position.X, 0f, 1f) * _size.X), (int)(Math.Clamp(vertex.Position.Y, 0f, 1f) * _size.Y), _size.X / 200, _size.Y / 200);
                }
			}
        }

        png.Save(Environment.CurrentDirectory + @"\" + _name + ".png");
    }

	public VoronoiDiagramm Relax()
	{
		List<PointF> points = new List<PointF>();

		foreach (VoronoiFace face in m_Faces)
		{
			points.Add(CalcCentroid(face));
		}

		return VoronoiGenerator.GenerateVoronoi(points.ToArray());

		PointF CalcCentroid(VoronoiFace _face)
        {
			List<PointF> centers = new List<PointF>();
			PointF[] vertecies = new PointF[3];

			vertecies[1] = Clamp(_face.Vertex[0].Position);
			vertecies[2] = Clamp(_face.Vertex[1].Position);

            for (int i = 2; i < _face.Vertex.Length; i++)
            {
				vertecies[0] = vertecies[1];
				vertecies[1] = vertecies[2];
				vertecies[2] = Clamp(_face.Vertex[i].Position);

				centers.Add(new PointF((vertecies[0].X + vertecies[1].X + vertecies[2].X) / 3, (vertecies[0].Y + vertecies[1].Y + vertecies[2].Y) / 3));
            }

			PointF result = new PointF(0, 0);

            for (int i = 0; i < centers.Count; i++)
            {
				result.X += centers[i].X;
				result.Y += centers[i].Y;
            }

			result.X /= centers.Count;
			result.Y /= centers.Count;

			return result;

			PointF Clamp(PointF _point)
            {
				return new PointF(Math.Clamp(_point.X, 0f, 1f), Math.Clamp(_point.Y, 0f, 1f));
            }
        }
	}
}

public class FaceComparer : IComparer<VoronoiFace>
{
    public int Compare(VoronoiFace a, VoronoiFace b)
    {
        if(a.Point.Position.X == b.Point.Position.X)
        {
			return a.Point.Position.Y.CompareTo(b.Point.Position.Y);
        }
		else
        {
			return a.Point.Position.X.CompareTo(b.Point.Position.X);
        }
    }
}

public class VoronoiPoint
{
	private readonly PointF m_Pos;
	private readonly VoronoiFace m_Face;

	public PointF Position
	{
		get { return m_Pos; }
	}

	public VoronoiFace Face
	{
		get { return m_Face; }
	}

	public VoronoiEdge[] Edges
	{
		get { return m_Face.Edges; }
	}

	public VoronoiVertex[] Vertex
	{
		get { return m_Face.Vertex; }
	}

    public VoronoiPoint(PointF _point, VoronoiFace _face)
    {
		m_Pos = _point;
		m_Face = _face;
    }
}

public class VoronoiFace
{
	private readonly VoronoiPoint m_Point;
	private VoronoiVertex[] m_Vertex;
	private VoronoiEdge[] m_Edges;
	private VoronoiFace[] m_Neighbors;

	public	VoronoiFace[] Neighbors
    {
		get { return m_Neighbors; }
    }

	public VoronoiPoint Point
    {
        get { return m_Point; }
    }

	public VoronoiVertex[] Vertex
    {
        get { return m_Vertex; }
    }

	public VoronoiEdge[] Edges
    {
        get { return m_Edges; }
    }

    public VoronoiFace(VFace _face)
    {
		m_Point = new VoronoiPoint(_face.m_Point, this);
	}

	public void SetEdges(VoronoiEdge[] _edges)
	{
		if (m_Edges != null)
		{
			return;
		}
		m_Edges = _edges;

		m_Neighbors = new VoronoiFace[m_Edges.Length];

        for (int i = 0; i < m_Edges.Length; i++)
        {
            for (int j = 0; j < m_Edges[i].Faces.Length; j++)
            {
				if(m_Edges[i].Faces[j] != this)
                {
					m_Neighbors[i] = m_Edges[i].Faces[j];

				}
            }
        }
	}

	public void SetVertex(VoronoiVertex[] _vertex)
	{
		if (m_Vertex != null)
		{
			return;
		}
		m_Vertex = _vertex;
	}
}

public class VoronoiVertex
{
	private readonly PointF m_Pos;
	private VoronoiEdge[] m_Edges;
	private readonly VoronoiFace[] m_Faces;

	public PointF Position
    {
        get { return m_Pos; }
    }

	public VoronoiEdge[] Edges
    {
        get { return m_Edges; }
    }

	public VoronoiFace[] Faces
    {
        get { return m_Faces; }
    }

    public VoronoiVertex(PointF _pos, VoronoiFace[] _faces)
    {
		m_Pos = _pos;
		m_Faces = _faces;
    }

	public void SetEdges(VoronoiEdge[] _edges)
    {
		if(m_Edges != null)
        {
			return;
        }
		m_Edges = _edges;
    }
}

public class VoronoiEdge
{
	private readonly VoronoiFace[] m_Faces;
	private readonly VoronoiVertex[] m_Vertex;

	public VoronoiVertex[] Vertex
	{
		get { return m_Vertex; }
	}

	public VoronoiFace[] Faces
	{
		get { return m_Faces; }
	}

    public VoronoiEdge()
    {

	}

    public VoronoiEdge(VoronoiFace[] _faces, VoronoiVertex[] _vertex)
    {
		m_Faces = _faces;
		m_Vertex = _vertex;
    }
}

