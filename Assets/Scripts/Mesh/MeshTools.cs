using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshTools {

    /// Builds an array of edges that connect to only one triangle.
	/// In other words, the outline of the mesh	
	public static Edge[] BuildManifoldEdges(Mesh mesh)
    {
        // Build a edge list for all unique edges in the mesh
        Edge[] edges = BuildEdges(mesh.vertexCount, mesh.triangles);

        // We only want edges that connect to a single triangle
        ArrayList culledEdges = new ArrayList();
        foreach (Edge edge in edges)
        {
            if (edge.faceIndex[0] == edge.faceIndex[1])
            {
                culledEdges.Add(edge);
            }
        }

        return culledEdges.ToArray(typeof(Edge)) as Edge[];
    }

    /// Builds an array of unique edges
    /// This requires that your mesh has all vertices welded. However on import, Unity has to split
    /// vertices at uv seams and normal seams. Thus for a mesh with seams in your mesh you
    /// will get two edges adjoining one triangle.
    /// Often this is not a problem but you can fix it by welding vertices 
    /// and passing in the triangle array of the welded vertices.
    public static Edge[] BuildEdges(int vertexCount, int[] triangleArray)
    {
        int maxEdgeCount = triangleArray.Length;
        int[] firstEdge = new int[vertexCount + maxEdgeCount];
        int nextEdge = vertexCount;
        int triangleCount = triangleArray.Length / 3;

        for (int a = 0; a < vertexCount; a++)
            firstEdge[a] = -1;

        // First pass over all triangles. This finds all the edges satisfying the
        // condition that the first vertex index is less than the second vertex index
        // when the direction from the first vertex to the second vertex represents
        // a counterclockwise winding around the triangle to which the edge belongs.
        // For each edge found, the edge index is stored in a linked list of edges
        // belonging to the lower-numbered vertex index i. This allows us to quickly
        // find an edge in the second pass whose higher-numbered vertex index is i.
        Edge[] edgeArray = new Edge[maxEdgeCount];

        int edgeCount = 0;
        for (int a = 0; a < triangleCount; a++)
        {
            int i1 = triangleArray[a * 3 + 2];
            for (int b = 0; b < 3; b++)
            {
                int i2 = triangleArray[a * 3 + b];
                if (i1 < i2)
                {
                    Edge newEdge = new Edge();
                    newEdge.vertexIndex[0] = i1;
                    newEdge.vertexIndex[1] = i2;
                    newEdge.faceIndex[0] = a;
                    newEdge.faceIndex[1] = a;
                    edgeArray[edgeCount] = newEdge;

                    int edgeIndex = firstEdge[i1];
                    if (edgeIndex == -1)
                    {
                        firstEdge[i1] = edgeCount;
                    }
                    else
                    {
                        while (true)
                        {
                            int index = firstEdge[nextEdge + edgeIndex];
                            if (index == -1)
                            {
                                firstEdge[nextEdge + edgeIndex] = edgeCount;
                                break;
                            }

                            edgeIndex = index;
                        }
                    }

                    firstEdge[nextEdge + edgeCount] = -1;
                    edgeCount++;
                }

                i1 = i2;
            }
        }

        // Second pass over all triangles. This finds all the edges satisfying the
        // condition that the first vertex index is greater than the second vertex index
        // when the direction from the first vertex to the second vertex represents
        // a counterclockwise winding around the triangle to which the edge belongs.
        // For each of these edges, the same edge should have already been found in
        // the first pass for a different triangle. Of course we might have edges with only one triangle
        // in that case we just add the edge here
        // So we search the list of edges
        // for the higher-numbered vertex index for the matching edge and fill in the
        // second triangle index. The maximum number of comparisons in this search for
        // any vertex is the number of edges having that vertex as an endpoint.

        for (int a = 0; a < triangleCount; a++)
        {
            int i1 = triangleArray[a * 3 + 2];
            for (int b = 0; b < 3; b++)
            {
                int i2 = triangleArray[a * 3 + b];
                if (i1 > i2)
                {
                    bool foundEdge = false;
                    for (int edgeIndex = firstEdge[i2]; edgeIndex != -1; edgeIndex = firstEdge[nextEdge + edgeIndex])
                    {
                        Edge edge = edgeArray[edgeIndex];
                        if ((edge.vertexIndex[1] == i1) && (edge.faceIndex[0] == edge.faceIndex[1]))
                        {
                            edgeArray[edgeIndex].faceIndex[1] = a;
                            foundEdge = true;
                            break;
                        }
                    }

                    if (!foundEdge)
                    {
                        Edge newEdge = new Edge();
                        newEdge.vertexIndex[0] = i1;
                        newEdge.vertexIndex[1] = i2;
                        newEdge.faceIndex[0] = a;
                        newEdge.faceIndex[1] = a;
                        edgeArray[edgeCount] = newEdge;
                        edgeCount++;
                    }
                }

                i1 = i2;
            }
        }

        Edge[] compactedEdges = new Edge[edgeCount];
        for (int e = 0; e < edgeCount; e++)
            compactedEdges[e] = edgeArray[e];

        return compactedEdges;
    }

    public class Edge
    {
        // The indiex to each vertex
        public int[] vertexIndex = new int[2];
        // The index into the face.
        // (faceindex[0] == faceindex[1] means the edge connects to only one triangle)
        public int[] faceIndex = new int[2];
    }

    /// <summary>
    /// Returns index of edge that connects topological vertices
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static int GetEdgeIndex(Edge[] edges, int v1, int v2)
    {
        for (int i = 0; i < edges.Length; i++)
        {
            var edge = edges[i];
            if (v1 == edge.vertexIndex[0] && v2 == edge.vertexIndex[1])
            {
                return i;
            }

            if (v1 == edge.vertexIndex[1] && v2 == edge.vertexIndex[0])
            {
                return i;
            }
        }

        return -1;
    }

 
    /// <summary>
    /// 
    /// </summary>
    /// <param name="mesh"></param>
    /// <returns></returns>
    public static Dictionary<int, List<int> > ConnetedVerices(Edge [] edges, Mesh mesh, bool sort)
    {

        
        var connections = new Dictionary<int, List<int> >();

        for (int i = 0; i < edges.Length; i++)
        {
            int cPt = edges[i].vertexIndex[0];
            int ePt = edges[i].vertexIndex[1];
            UpdteConnections(connections, cPt, ePt);
            UpdteConnections(connections, ePt, cPt);
        }

        if (sort)
        {

            foreach (KeyValuePair<int, List<int>> entry in connections)
            {
                int i = entry.Key;
                var cList = entry.Value;

                var normal = mesh.normals[i];
                //https://answers.unity.com/questions/532297/rotate-a-vector-around-a-certain-point.html
                var vecX = Quaternion.Euler(90, 0, 0) * normal;
                var vecY = Quaternion.Euler(0, 90, 0) * normal;

                var atan2 = new float[cList.Count];
                var conn = cList.ToArray();


                for (int j = 0; j < conn.Length; j++)
                {
                    var index = cList[j];
                    var projectX = Vector3.Dot(vecX, mesh.vertices[index]);
                    var projectY = Vector3.Dot(vecY, mesh.vertices[index]);
                    atan2[j] = Mathf.Atan2(projectY, projectX);
                }

                Array.Sort(atan2, conn);
                cList = new List<int>(conn);


            }
        }

        return connections;
    }

    private static void UpdteConnections(Dictionary<int, List<int>> connections, int cPt, int ePt)
    {
        if (!connections.ContainsKey(cPt))
        {
            var c = new List<int>
                {
                    ePt
                };
            connections.Add(cPt, c);
        }
        else
        {
            if (connections[cPt].IndexOf(ePt) == -1) connections[cPt].Add(ePt);
        }
    }

    public class VertexConnection
    {
        public List<int> connections = new List<int>();
    }

}
