using System;
using System.Collections.Generic;
using System.Linq;
using FlaxEngine;

namespace DunGen;

/// <summary>
/// Prim Script.
/// </summary>
public class Prim
{
	public class Edge : DelaunayTriangulation.Edge
	{
		public float Distance { get; private set; }
		public Edge(DelaunayTriangulation.Point a, DelaunayTriangulation.Point b) : base(a, b)
		{
			Distance = Vector3.Distance(a.VPoint, b.VPoint);
		}

		public Vertex GetOtherVertex(Vertex vertex, List<Vertex> vertices)
		{
			if (A == vertex.Point)
			{
				return FindVertex(B, vertices);
			}
			else
			{
				return FindVertex(A, vertices);
			}
		}
		public Vertex GetOtherVertex(Vertex vertex, HashSet<Vertex> vertices)
		{
			if (A == vertex.Point)
			{
				return FindVertex(B, vertices);
			}
			else
			{
				return FindVertex(A, vertices);
			}
		}

		public bool IsVertexInEdge(Vertex vertex)
		{
			return A == vertex.Point || B == vertex.Point;
		}

		public static bool operator ==(Edge a, Edge b)
		{
			return (a.A == b.A && a.B == b.B) || (a.B == b.B && a.B == b.B);
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

		public override string ToString()
		{
			return $"Edge: ({A},{B}) - {Distance}";
		}
	}

	public class Vertex : IEquatable<Vertex>
	{
		public List<Prim.Edge> ConnectedEdges { get; private set; }
		public List<Vertex> Neighbors { get; private set; }
		public DelaunayTriangulation.Point Point { get; private set; }

		public Vertex(DelaunayTriangulation.Point point)
		{
			Neighbors = new List<Vertex>();
			ConnectedEdges = new List<Prim.Edge>();
			Point = point;
		}

		public void AddEdge(Prim.Edge edge)
		{
			// if (!ConnectedEdges.Contains(edge))
			ConnectedEdges.Add(edge);
		}

		public void AddNeighbor(Vertex vertex)
		{
			// if (!Neighbors.Contains(vertex))
			Neighbors.Add(vertex);
		}

		public override string ToString()
		{
			return $"Vertex: {Point} | Edges: {ConnectedEdges.Count} | Neighbors: {Neighbors.Count}";
		}

		public static bool operator ==(Vertex a, Vertex b)
		{
			return a.Point == b.Point;
		}
		public static bool operator !=(Vertex a, Vertex b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (obj is Vertex e)
			{
				return this == e;
			}

			return false;
		}

		public override int GetHashCode()
		{
			return Point.GetHashCode() ^ ConnectedEdges.GetHashCode();
		}

		public bool Equals(Vertex other)
		{
			return this == other;
		}
	}

	private static Vertex FindVertex(DelaunayTriangulation.Point point, HashSet<Vertex> vertices)
	{
		foreach (var vertex in vertices)
		{
			if (vertex.Point == point)
			{
				return vertex;
			}
		}

		return null;
	}

	private static Vertex FindVertex(DelaunayTriangulation.Point point, List<Vertex> vertices)
	{
		foreach (var vertex in vertices)
		{
			if (vertex.Point == point)
			{
				return vertex;
			}
		}

		return null;
	}

	public static void DebugMST(List<Prim.Edge> mst, Color color, float yOffset = 30f, float duration = 20f)
	{
		// Draw edges or any other post-processing
		foreach (var edge in mst)
		{
			Debug.Log(edge);
			Vector3 a = new Vector3(edge.A.X, yOffset, edge.A.Y);
			Vector3 b = new Vector3(edge.B.X, yOffset, edge.B.Y);
			DebugDraw.DrawLine(a, b, color, duration);
		}
	}


	/// <summary>
	/// Computes the Minimum Spanning Tree (MST) using Prim's algorithm.
	/// Time Complexity: O(V * E + E * log(E)), where V is the number of vertices and E is the number of edges.
	/// </summary>
	/// <param name="weightedEdges">List of edges with weights.</param>
	/// <param name="start">The starting point for Prim's algorithm.</param>
	/// <returns>A list of edges that make up the Minimum Spanning Tree.</returns>
	public static HashSet<DelaunayTriangulation.Edge> MinimumSpanningTree(List<Prim.Edge> weightedEdges, DelaunayTriangulation.Point start)
	{
		HashSet<DelaunayTriangulation.Edge> mst = new HashSet<DelaunayTriangulation.Edge>();

		// Create the vertex set from the edges
		HashSet<Vertex> vertices = CreateVertexSet(weightedEdges); // O(V * E)
		List<Vertex> visited = new List<Vertex>();

		// Use a priority queue to select the next edge
		PriorityQueue<Prim.Edge, float> edgeQueue = new PriorityQueue<Prim.Edge, float>();

		// Find and add the starting vertex to the visited list
		Vertex startingVertex = FindVertex(start, vertices); // O(V) or O(1)
		visited.Add(startingVertex); // O(1)

		// Add all edges of the starting vertex to the queue
		foreach (var edge in startingVertex.ConnectedEdges)
		{
			edgeQueue.Enqueue(edge, edge.Distance); // O(log(E))
		}

		// While loop processes edges until all vertices are included
		while (mst.Count < vertices.Count - 1 && edgeQueue.Count > 0) // Ensure you don't exceed vertex count
		{
			Prim.Edge edge = edgeQueue.Dequeue(); // Get the smallest edge, O(log(E))

			Vertex vertexA = FindVertex(edge.A, vertices); // O(V) or O(1)
			Vertex vertexB = FindVertex(edge.B, vertices); // O(V) or O(1)

			// Determine the next vertex that hasn't been visited
			Vertex nextVertex = visited.Contains(vertexA) ? vertexB : vertexA;

			// Only add the next vertex if it hasn't been visited
			if (!visited.Contains(nextVertex))
			{
				mst.Add(edge); // Add the edge to the MST, O(1)
				visited.Add(nextVertex); // Add the vertex to the visited list, O(1)

				// Add all edges of the next vertex to the queue
				foreach (Prim.Edge e in nextVertex.ConnectedEdges)
				{
					if (!mst.Contains(e)) // Avoid adding edges already in MST
					{
						edgeQueue.Enqueue(e, e.Distance); // O(log(E))
					}
				}
			}
		}

		return mst; // Convert HashSet to List, O(M)
	}

	/// <summary>
	/// Creates a set of vertices from the list of weighted edges.
	/// Time Complexity: O(V * E) where V is the number of vertices and E is the number of edges.
	/// </summary>
	/// <param name="weightedEdges">List of edges to create vertices from.</param>
	/// <returns>A HashSet of vertices created from the edges.</returns>
	private static HashSet<Vertex> CreateVertexSet(List<Edge> weightedEdges)
	{
		HashSet<DelaunayTriangulation.Point> points = new HashSet<DelaunayTriangulation.Point>();

		// Add unique points from edges to the points set
		foreach (var edge in weightedEdges)
		{
			points.Add(edge.A); // O(1) for HashSet
			points.Add(edge.B); // O(1) for HashSet
		}

		HashSet<Vertex> vertices = new HashSet<Vertex>();

		// Create vertices from unique points
		foreach (var point in points)
		{
			Vertex vertex = new Vertex(point);
			// Iterate over edges to add connected edges to the vertex
			foreach (var edge in weightedEdges) // O(E)
			{
				if (edge.IsVertexInEdge(vertex)) // O(1)
				{
					vertex.AddEdge(edge); // O(1)
				}
			}
			vertices.Add(vertex); // O(1)
		}

		// Establish connections between vertices
		foreach (var vertex in vertices)
		{
			foreach (var edges in vertex.ConnectedEdges) // O(k) where k is the average degree of vertices
			{
				bool isA = edges.A == vertex.Point;
				Vertex neighbor = FindVertex(isA ? edges.B : edges.A, vertices); // O(V) or O(1)
				vertex.AddNeighbor(neighbor); // O(1)
			}
		}

		return vertices; // O(1)
	}


}
