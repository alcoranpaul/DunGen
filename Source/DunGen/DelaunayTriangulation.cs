using System;
using System.Collections.Generic;
using System.Linq;
using FlaxEngine;

namespace DunGen;

/// <summary>
/// DelaunayTriangulationTriangulation Script.
/// </summary>
public class DelaunayTriangulation
{
	public List<Triangle> Triangulation { get; private set; }
	public HashSet<Edge> Edges { get; private set; }
	public List<Point> Points { get; private set; }

	public DelaunayTriangulation()
	{
		Triangulation = new List<Triangle>();
	}

	public string EdgesToString()
	{
		Debug.Log("Edges count: " + Edges.Count);
		string retVal = "";
		List<Edge> edges = Edges.ToList();
		for (int i = 0; i < edges.Count; i++)
		{

			Point pointA = edges[i].A;
			Point pointB = edges[i].B;
			retVal += $"Edge {i}: {pointA}, {pointB}\n";
			BoundingSphere sphereA = new(pointA.VPoint, 15f);
			BoundingSphere sphereB = new(pointB.VPoint, 15f);
			DebugDraw.DrawSphere(sphereA, Color.BlanchedAlmond, 10f);
			DebugDraw.DrawSphere(sphereB, Color.BlanchedAlmond, 10f);
		}
		return retVal;
	}

	public override string ToString()
	{
		if (Triangulation.Count == 0)
		{
			return "No triangles in triangulation";
		}

		string retVal = "";
		int pointCount = 0;
		for (int i = 0; i < Triangulation.Count; i++)
		{

			Point a = Triangulation[i].A;
			Point b = Triangulation[i].B;
			Point c = Triangulation[i].C;
			retVal += $"Triangle {i}: {a}, {b}, {c}\n";
			pointCount += 3;
		}
		return retVal;
	}

#if FLAX_EDITOR
	public static void DebugTriangulation(DelaunayTriangulation DelaunayTriangulation, Color edgeColor, Color pointColor, float yOffset = 20, float duration = 16f, float sphereRadius = 2f)
	{
		if (DelaunayTriangulation == null || DelaunayTriangulation.Triangulation == null || DelaunayTriangulation.Triangulation.Count == 0)
		{
			return;
		}

		foreach (var triangle in DelaunayTriangulation.Triangulation)
		{
			Vector3 aPoint = triangle.A.VPoint;
			Vector3 bPoint = triangle.B.VPoint;
			Vector3 cPoint = triangle.C.VPoint;

			if (yOffset > 0)
			{
				aPoint = new Vector3(aPoint.X, aPoint.Y + yOffset, aPoint.Z);
				bPoint = new Vector3(bPoint.X, bPoint.Y + yOffset, bPoint.Z);
				cPoint = new Vector3(cPoint.X, cPoint.Y + yOffset, cPoint.Z);
			}

			DebugDraw.DrawLine(aPoint, bPoint, edgeColor, duration);
			DebugDraw.DrawLine(bPoint, cPoint, edgeColor, duration);
			DebugDraw.DrawLine(cPoint, aPoint, edgeColor, duration);

			BoundingSphere sphereA = new(aPoint, sphereRadius);
			BoundingSphere sphereB = new(bPoint, sphereRadius);
			BoundingSphere sphereC = new(cPoint, sphereRadius);
			DebugDraw.DrawSphere(sphereA, pointColor, duration);
			DebugDraw.DrawSphere(sphereB, pointColor, duration);
			DebugDraw.DrawSphere(sphereC, pointColor, duration);
		}
	}
#endif
	/// <summary>
	/// Triangulate a list of points
	/// </summary>
	/// <param name="points"></param>
	/// <returns></returns>
	/// <returns></returns>
	public static DelaunayTriangulation Triangulate(List<Point> points)
	{
		DelaunayTriangulation DelaunayTriangulation = new DelaunayTriangulation();
		DelaunayTriangulation.Edges = new HashSet<Edge>();
		DelaunayTriangulation.Points = points;
		DelaunayTriangulation.Generate(points);
		return DelaunayTriangulation;
	}

	/// <summary>
	/// Generate the DelaunayTriangulation triangulation: O(n)
	/// </summary>
	/// <param name="points"></param>
	public void Generate(List<Point> points)
	{
		// Calculate bounding box using the input points
		float minX = float.MaxValue, minY = float.MaxValue;
		float maxX = float.MinValue, maxY = float.MinValue;

		foreach (var p in points)
		{
			if (p.X < minX) minX = p.X;
			if (p.Y < minY) minY = p.Y;
			if (p.X > maxX) maxX = p.X;
			if (p.Y > maxY) maxY = p.Y;
		}

		// Define a supertriangle that encompasses the input points
		float dx = maxX - minX;
		float dy = maxY - minY;
		float deltaMax = Math.Max(dx, dy) * 2;

		Point superA = new Point(minX - deltaMax, minY - deltaMax);
		Point superB = new Point(maxX + deltaMax, minY - deltaMax);
		Point superC = new Point(minX + deltaMax, maxY + deltaMax);

		Triangle superTriangle = new Triangle(superA, superB, superC);
		Triangulation.Add(superTriangle);


		foreach (var point in points)
		{
			InsertPoint(point);
		}

		// Cleanup: Remove triangles that contain vertices of the supertriangle
		Triangulation.RemoveAll(triangle =>
			triangle.A == superA || triangle.A == superB || triangle.A == superC ||
			triangle.B == superA || triangle.B == superB || triangle.B == superC ||
			triangle.C == superA || triangle.C == superB || triangle.C == superC
		);

		foreach (var triangle in Triangulation)
		{
			Edges.Add(new Edge(triangle.A, triangle.B));
			Edges.Add(new Edge(triangle.B, triangle.C));
			Edges.Add(new Edge(triangle.C, triangle.A));
		}




	}

	/// <summary>
	/// Insert a point into the triangulation
	/// </summary>
	/// <param name="point"></param>
	private void InsertPoint(Point point)
	{
		List<Triangle> badTriangles = new List<Triangle>();
		foreach (var triangle in Triangulation) // for each triangle in triangulation do
		{
			//  if point is inside circumcircle of triangle
			if (triangle.IsPointInCircumcircle(point))
			{
				badTriangles.Add(triangle); //  add triangle to badTriangles
			}
		}

		List<Edge> polygon = new List<Edge>();

		for (int i = 0; i < badTriangles.Count; i++) // for each triangle in badTriangles do
		{
			// Get the edges of the triangle
			Edge[] edges =
			[
				new Edge(badTriangles[i].A, badTriangles[i].B),
				new Edge(badTriangles[i].B, badTriangles[i].C),
				new Edge(badTriangles[i].C, badTriangles[i].A),
			];
			foreach (Edge edge in edges) // for each edge in triangle do
			{
				bool shared = false;
				for (int k = 0; k < badTriangles.Count; k++)
				{
					if (k != i && badTriangles[k].IsEdgeInTriangle(edge))
					{
						shared = true;
						break; // Exit loop if the edge is found in another triangle
					}
				}
				if (!shared)
				{
					// add edge to polygon
					polygon.Add(edge);
				}
			}
		}


		// for each triangle in badTriangles do 
		foreach (var triangle in badTriangles)
		{
			Triangulation.Remove(triangle); // remove triangle from triangulation
		}

		//for each edge in polygon do
		foreach (var edge in polygon)
		{
			// newTri := form a triangle from edge to point
			Triangle newTriangle = new Triangle(edge.A, edge.B, point);
			//  add newTri to triangulation
			Triangulation.Add(newTriangle);

		}

	}

	public class Point : IEquatable<Point>
	{
		public float X;
		public float Y;

		/// <summary>
		/// Vector3 representation of the point
		/// </summary>
		public Vector3 VPoint => new Vector3(X, 0, Y);

		public Point(float x, float y)
		{
			X = x;
			Y = y;
		}

		public override string ToString()
		{
			return $"({X}, {Y})";
		}

		public static bool operator ==(Point a, Point b)
		{
			return (a.X == b.X) && (a.Y == b.Y);
		}
		public static bool operator !=(Point a, Point b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (obj is Point e)
			{
				return this == e;
			}

			return false;
		}

		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode();
		}

		public bool Equals(Point other)
		{
			return this == other;
		}

	}

	public class Edge : IEquatable<Edge>
	{
		public Point A { get; private set; }
		public Point B { get; private set; }
		public Edge(Point a, Point b)
		{
			A = a;
			B = b;
		}

		public static void DebugEdges(List<Edge> edges, Color color, float yOffset = 30f, float duration = 20f)
		{
			// Draw edges or any other post-processing
			foreach (var edge in edges)
			{

				Vector3 a = new Vector3(edge.A.X, yOffset, edge.A.Y);
				Vector3 b = new Vector3(edge.B.X, yOffset, edge.B.Y);
				DebugDraw.DrawLine(a, b, color, duration);
			}
		}

		public override string ToString()
		{
			return $"Edge: [{A}, {B}]";
		}

		public static bool operator ==(Edge a, Edge b)
		{
			return (a.A == b.A || a.A == b.B) && (a.B == b.A || a.B == b.B);
		}
		public static bool operator !=(Edge a, Edge b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (obj is Edge e)
			{
				return this == e;
			}

			return false;
		}

		public override int GetHashCode()
		{
			return A.GetHashCode() ^ B.GetHashCode();
		}

		public bool Equals(Edge other)
		{
			return this == other;
		}
	}

	public class Triangle
	{
		public Point A { get; }
		public Point B { get; }
		public Point C { get; }

		public Triangle(Point a, Point b, Point c)
		{
			A = a;
			B = b;
			C = c;
		}

		// Check if a point is inside the circumcircle of this triangle
		// From @ https://en.wikipedia.org/wiki/Circumcircle
		public bool IsPointInCircumcircle(Point point)
		{
			// Using the circumcircle formula
			// From matrix @ "Using the polarization identity, these equations reduce to the condition that the matrix" from wikipedia website above
			float ax = A.X - point.X;
			float ay = A.Y - point.Y;
			float bx = B.X - point.X;
			float by = B.Y - point.Y;
			float cx = C.X - point.X;
			float cy = C.Y - point.Y;

			// Determinant of the matrix
			float det = (ax * ax + ay * ay) * (bx * cy - by * cx) -
						(bx * bx + by * by) * (ax * cy - ay * cx) +
						(cx * cx + cy * cy) * (ax * by - ay * bx);

			return det > 0 || float.IsNaN(det); // true if point is inside circumcircle
		}

		public bool IsEdgeInTriangle(Edge edge)
		{
			Edge[] edges =
			[
				new Edge(A, B),
				new Edge(B, C),
				new Edge(C, A),
			];

			foreach (var e in edges)
			{
				if (e == edge) return true;
			}
			return false;
		}

		public override string ToString()
		{
			return $"Triangle: {A}, {B}, {C}";
		}
	}
}
