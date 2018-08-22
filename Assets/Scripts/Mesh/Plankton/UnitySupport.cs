using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Plankton;

public static class UnitySupport
{

    /// <summary>

    /// Creates a Plankton halfedge mesh from a Rhino mesh.

    /// Uses the topology of the Rhino mesh directly.

    /// </summary>

    /// <returns>A <see cref="PlanktonMesh"/> which represents the topology and geometry of the source mesh.</returns>

    /// <param name="source">A Rhino mesh to convert from.</param>

    public static PlanktonMesh ToPlanktonMesh(this Mesh source)

    {


        PlanktonMesh pMesh = new PlanktonMesh();


        foreach (Vector3 v in source.vertices)

        {

            pMesh.Vertices.Add(v.x, v.y, v.z);

        }



        for (int i = 0; i < source.triangles.Length/3; i++)

        {

            pMesh.Faces.Add(new PlanktonFace());

        }

        // Build a edge list for all unique edges in the mesh
        var TopologyEdges = MeshTools.BuildEdges(source.vertexCount, source.triangles);


        for (int i = 0; i < TopologyEdges.Length; i++)

        {

            PlanktonHalfedge HalfA = new PlanktonHalfedge
            {
                //StartVertex = source.TopologyEdges.GetTopologyVertices(i).I
                StartVertex = TopologyEdges[i].vertexIndex[0]
            };


            if (pMesh.Vertices[HalfA.StartVertex].OutgoingHalfedge == -1)
            {

                pMesh.Vertices[HalfA.StartVertex].OutgoingHalfedge = pMesh.Halfedges.Count;

            }



            PlanktonHalfedge HalfB = new PlanktonHalfedge
            {
                StartVertex = TopologyEdges[i].vertexIndex[1]
            };



            if (pMesh.Vertices[HalfB.StartVertex].OutgoingHalfedge == -1)
            {

                pMesh.Vertices[HalfB.StartVertex].OutgoingHalfedge = pMesh.Halfedges.Count + 1;

            }


            int[] ConnectedFaces = TopologyEdges[i].faceIndex;



            //int VertA = (3 * ConnectedFaces[0]) + 0;

            //int VertB = (3 * ConnectedFaces[0]) + 1;

            //int VertC = (3 * ConnectedFaces[0]) + 2;




            HalfA.AdjacentFace = ConnectedFaces[0];

            if (pMesh.Faces[HalfA.AdjacentFace].FirstHalfedge == -1)
            {

                pMesh.Faces[HalfA.AdjacentFace].FirstHalfedge = pMesh.Halfedges.Count;

            }

            if (ConnectedFaces[0] != ConnectedFaces[1])

            {

                HalfB.AdjacentFace = ConnectedFaces[1];

                if (pMesh.Faces[HalfB.AdjacentFace].FirstHalfedge == -1)
                {

                    pMesh.Faces[HalfB.AdjacentFace].FirstHalfedge = pMesh.Halfedges.Count + 1;

                }

            }

            else

            {

                HalfB.AdjacentFace = -1;

                pMesh.Vertices[HalfB.StartVertex].OutgoingHalfedge = pMesh.Halfedges.Count + 1;

            }


            pMesh.Halfedges.Add(HalfA);

            pMesh.Halfedges.Add(HalfB);

        }


        var connetedVertices = MeshTools.ConnetedVerices(TopologyEdges, source, true);

        for (int i = 0; i < (pMesh.Halfedges.Count); i += 2)

        {

            int[] EndNeighbours = connetedVertices[pMesh.Halfedges[i + 1].StartVertex].ToArray();

            for (int j = 0; j < EndNeighbours.Length; j++)

            {

                if (EndNeighbours[j] == pMesh.Halfedges[i].StartVertex)

                {

                    int EndOfNextHalfedge = EndNeighbours[(j - 1 + EndNeighbours.Length) % EndNeighbours.Length];

                    int StartOfPrevOfPairHalfedge = EndNeighbours[(j + 1) % EndNeighbours.Length];



                    int NextEdge = MeshTools.GetEdgeIndex(TopologyEdges, pMesh.Halfedges[i + 1].StartVertex, EndOfNextHalfedge);

                    int PrevPairEdge = MeshTools.GetEdgeIndex(TopologyEdges,pMesh.Halfedges[i + 1].StartVertex, StartOfPrevOfPairHalfedge);



                    if (TopologyEdges[NextEdge].vertexIndex[0] == pMesh.Halfedges[i + 1].StartVertex)
                    {

                        pMesh.Halfedges[i].NextHalfedge = NextEdge * 2;

                    }
                    else
                    {

                        pMesh.Halfedges[i].NextHalfedge = NextEdge * 2 + 1;

                    }



                    if (TopologyEdges[PrevPairEdge].vertexIndex[1] == pMesh.Halfedges[i + 1].StartVertex)
                    {

                        pMesh.Halfedges[i + 1].PrevHalfedge = PrevPairEdge * 2;

                    }
                    else
                    {

                        pMesh.Halfedges[i + 1].PrevHalfedge = PrevPairEdge * 2 + 1;

                    }

                    break;

                }

            }



            int[] StartNeighbours = connetedVertices[pMesh.Halfedges[i].StartVertex].ToArray();

            for (int j = 0; j < StartNeighbours.Length; j++)

            {

                if (StartNeighbours[j] == pMesh.Halfedges[i + 1].StartVertex)

                {

                    int EndOfNextOfPairHalfedge = StartNeighbours[(j - 1 + StartNeighbours.Length) % StartNeighbours.Length];

                    int StartOfPrevHalfedge = StartNeighbours[(j + 1) % StartNeighbours.Length];



                    int NextPairEdge = MeshTools.GetEdgeIndex(TopologyEdges, pMesh.Halfedges[i].StartVertex, EndOfNextOfPairHalfedge);

                    int PrevEdge = MeshTools.GetEdgeIndex(TopologyEdges, pMesh.Halfedges[i].StartVertex, StartOfPrevHalfedge);



                    if (TopologyEdges[NextPairEdge].vertexIndex[0] == pMesh.Halfedges[i].StartVertex)
                    {

                        pMesh.Halfedges[i + 1].NextHalfedge = NextPairEdge * 2;

                    }
                    else
                    {

                        pMesh.Halfedges[i + 1].NextHalfedge = NextPairEdge * 2 + 1;

                    }



                    if (TopologyEdges[PrevEdge].vertexIndex[1] == pMesh.Halfedges[i].StartVertex)
                    {

                        pMesh.Halfedges[i].PrevHalfedge = PrevEdge * 2;

                    }
                    else
                    {

                        pMesh.Halfedges[i].PrevHalfedge = PrevEdge * 2 + 1;

                    }

                    break;

                }

            }

        }



        return pMesh;

    }



    /// <summary>

    /// Creates a Rhino mesh from a Plankton halfedge mesh.

    /// Uses the face-vertex information available in the halfedge data structure.

    /// </summary>

    /// <returns>A <see cref="Mesh"/> which represents the source mesh (as best it can).</returns>

    /// <param name="source">A Plankton mesh to convert from.</param>

    /// <remarks>Any faces with five sides or more will be triangulated.</remarks>

    public static Mesh ToUnityMesh(PlanktonMesh source)

    {

        // could add different options for triangulating ngons later

        Mesh unityMesh = new Mesh();
        var vertices = new List<Vector3>();
        var faces = new List<int>();
        foreach (PlanktonVertex v in source.Vertices)

        {

            vertices.Add(new Vector3(v.X, v.Y, v.Z));

        }

        for (int i = 0; i < source.Faces.Count; i++)

        {

            int[] fvs = source.Faces.GetFaceVertices(i);

            if (fvs.Length == 3)

            {

                faces.AddRange(new int[] { fvs[0], fvs[1], fvs[2] });

            }

            //else if (fvs.Length == 4)

            //{
            //    faces.AddRange(new int[] { fvs[0], fvs[1], fvs[2], fvs[3] });
            //}

            //else if (fvs.Length > 4)

            //{
            //    //currently not supporting this kind of meshes
            //    //// triangulate about face center (fan)

            //    //var fc = source.Faces.GetFaceCenter(i);

            //    //vertices.Add(new Vector3(fc.X, fc.Y, fc.Z);

            //    //for (int j = 0; j < fvs.Length; j++)

            //    //{

            //    //    rMesh.Faces.AddFace(fvs[j], fvs[(j + 1) % fvs.Length], rMesh.Vertices.Count - 1);

            //    //}

            //}

        }

        unityMesh.vertices = vertices.ToArray();
        unityMesh.triangles = faces.ToArray();

        return unityMesh;

    }



    /// <summary>

    /// Replaces the vertices of a PlanktonMesh with a new list of points

    /// </summary>

    /// <returns>A list of closed polylines representing the boundary edges of each face.</returns>

    /// <param name="source">A Plankton mesh.</param>

    /// <param name="points">A list of points.</param>

    public static PlanktonMesh ReplaceVertices(PlanktonMesh source, List<Vector3> points)

    {

        PlanktonMesh pMesh = source;

        for (int i = 0; i < points.Count; i++)

        {

            pMesh.Vertices[i] = new PlanktonVertex(points[i].x, points[i].y, points[i].z);

        }

        return pMesh;

    }



    /// <summary>

    /// Converts each face to a closed polyline.

    /// </summary>

    /// <returns>A list of closed polylines representing the boundary edges of each face.</returns>

    /// <param name="source">A Plankton mesh.</param>

    //public static Polyline[] ToPolylines(this PlanktonMesh source)

    //{

    //    int n = source.Faces.Count;

    //    Polyline[] polylines = new Polyline[n];

    //    for (int i = 0; i < n; i++)

    //    {

    //        Polyline facePoly = new Polyline();

    //        int[] vs = source.Faces.GetFaceVertices(i);

    //        for (int j = 0; j <= vs.Length; j++)

    //        {

    //            var v = source.Vertices[vs[j % vs.Length]];

    //            facePoly.Add(v.X, v.Y, v.Z);

    //        }

    //        polylines[i] = facePoly;

    //    }



    //    return polylines;

    //}



    /// <summary>

    /// Creates a Rhino Point3f from a Plankton vertex.

    /// </summary>

    /// <param name="vertex">A Plankton vertex</param>

    /// <returns>A Point3f with the same coordinates as the vertex.</returns>

    //public static Point3f ToPoint3f(this PlanktonVertex vertex)

    //{

    //    return new Point3f(vertex.X, vertex.Y, vertex.Z);

    //}



    /// <summary>

    /// Creates a Rhino Point3d from a Plankton vertex.

    /// </summary>

    /// <param name="vertex">A Plankton vertex</param>

    /// <returns>A Point3d with the same coordinates as the vertex.</returns>

    //public static Point3d ToPoint3d(this PlanktonVertex vertex)

    //{

    //    return new Point3d(vertex.X, vertex.Y, vertex.Z);

    //}



    /// <summary>

    /// Creates a Rhino Point3f from a Plankton vector.

    /// </summary>

    /// <param name="vector">A Plankton vector.</param>

    /// <returns>A Point3f with the same XYZ components as the vector.</returns>

    //public static Point3f ToPoint3f(this PlanktonXYZ vector)

    //{

    //    return new Point3f(vector.X, vector.Y, vector.Z);

    //}



    /// <summary>

    /// Creates a Rhino Point3d from a Plankton vector.

    /// </summary>

    /// <param name="vector">A Plankton vector.</param>

    /// <returns>A Point3d with the same XYZ components as the vector.</returns>

    //public static Point3d ToPoint3d(this PlanktonXYZ vector)

    //{

    //    return new Point3d(vector.X, vector.Y, vector.Z);

    //}



    /// <summary>

    /// Creates a Rhino Vector3f from a Plankton vector.

    /// </summary>

    /// <param name="vector">A Plankton vector.</param>

    /// <returns>A Vector3f with the same XYZ components as the vector.</returns>

    //public static Vector3f ToVector3f(this PlanktonXYZ vector)

    //{

    //    return new Vector3f(vector.X, vector.Y, vector.Z);

    //}



    /// <summary>

    /// <para>Sets or adds a vertex to the Vertex List.</para>

    /// <para>If [index] is less than [Count], the existing vertex at [index] will be modified.</para>

    /// <para>If [index] equals [Count], a new vertex is appended to the end of the vertex list.</para>

    /// <para>If [index] is larger than [Count], the function will return false.</para>

    /// </summary>

    /// <param name="index">Index of vertex to set.</param>

    /// <param name="vertex">Vertex location.</param>

    /// <returns><c>true</c> on success, <c>false</c> on failure.</returns>

    //public static bool SetVertex(this PlanktonVertexList vertexList, int index, Point3f vertex)

    //{

    //    return vertexList.SetVertex(index, vertex.X, vertex.Y, vertex.Z);

    //}



    /// <summary>

    /// <para>Sets or adds a vertex to the Vertex List.</para>

    /// <para>If [index] is less than [Count], the existing vertex at [index] will be modified.</para>

    /// <para>If [index] equals [Count], a new vertex is appended to the end of the vertex list.</para>

    /// <para>If [index] is larger than [Count], the function will return false.</para>

    /// </summary>

    /// <param name="index">Index of vertex to set.</param>

    /// <param name="vertex">Vertex location.</param>

    /// <returns><c>true</c> on success, <c>false</c> on failure.</returns>

    //public static bool SetVertex(this PlanktonVertexList vertexList, int index, Point3d vertex)

    //{

    //    return vertexList.SetVertex(index, vertex.X, vertex.Y, vertex.Z);

    //}



    /// <summary>

    /// <para>Moves a vertex by a vector.</para>       

    /// </summary>

    /// <param name="index">Index of vertex to move.</param>

    /// <param name="vector">Vector to move by.</param>

    /// <returns><c>true</c> on success, <c>false</c> on failure.</returns>

    //public static bool MoveVertex(this PlanktonVertexList vertexList, int index, Vector3d vector)

    //{

    //    return vertexList.SetVertex(index, vertexList[index].X + vector.X, vertexList[index].Y + vector.Y, vertexList[index].Z + vector.Z);

    //}



    /// <summary>

    /// Adds a new vertex to the end of the Vertex list.

    /// </summary>

    /// <param name="vertex">Location of new vertex.</param>

    /// <returns>The index of the newly added vertex.</returns>

    //public static int Add(this PlanktonVertexList vertexList, Point3f vertex)

    //{

    //    return vertexList.Add(vertex.X, vertex.Y, vertex.Z);

    //}



    /// <summary>

    /// Adds a new vertex to the end of the Vertex list.

    /// </summary>

    /// <param name="vertex">Location of new vertex.</param>

    /// <returns>The index of the newly added vertex.</returns>

    //public static int Add(this PlanktonVertexList vertexList, Point3d vertex)

    //{

    //    return vertexList.Add(vertex.X, vertex.Y, vertex.Z);

    //}



    /// <summary>

    /// Gets positions of vertices

    /// </summary>

    /// <returns>A list of Point3d</returns>

    /// <param name="source">A Plankton mesh.</param>

    //public static IEnumerable<Point3d> GetPositions(this PlanktonMesh source)

    //{

    //    return Enumerable.Range(0, source.Vertices.Count).Select(i => source.Vertices[i].ToPoint3d());

    //}

}