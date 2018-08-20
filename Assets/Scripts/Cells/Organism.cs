using System.Collections.Generic;
using UnityEngine;
using System;
using Plankton;

public class Organism : MonoBehaviour
{

    private PlanktonMesh P = new PlanktonMesh();
    private List<Cell> cells = new List<Cell>();
    public GameObject cellPrefab;
    public Mesh InitialMesh;
    public PointOctree<Cell> pointTree = new PointOctree<Cell>(15, new Vector3(), 2);

    private MeshFilter meshFilter;

    // Use this for initialization
    void Start()
    {
        CreateHalfEdgeMesh();
        this.gameObject.AddComponent<MeshFilter>();
        meshFilter = this.gameObject.GetComponent<MeshFilter>();
    }

    private void CreateHalfEdgeMesh()
    {
        P = UnitySupport.ToPlanktonMesh(InitialMesh);

        for (int i = 0; i < InitialMesh.vertexCount; i++)
        {
            var pos = InitialMesh.vertices[i];
            GameObject go = CreateCell(pos);
            var c = go.GetComponent<Cell>();
            cells.Add(c);
        }

        Debug.Log("Initial number of cells:" + cells.Count);

    }

    private GameObject CreateCell(Vector3 pos)
    {
        GameObject go = (GameObject)Instantiate(cellPrefab);
        go.hideFlags = HideFlags.HideInHierarchy;
        go.transform.parent = transform;
        go.transform.position = pos;
        return go;
    }

    private List<Cell> GetLinkedCells(int cellIndex)
    {
        int[] Neighbours = P.Vertices.GetVertexNeighbours(cellIndex);
        List<Cell> linkedCells = new List<Cell>();

        foreach (var index in Neighbours)
            linkedCells.Add(cells[index]);

        return linkedCells;
    }


    void Update()
    {
        UpdateCells();
        SplitCells();
    }

    void UpdateCells()
    {

        pointTree = new PointOctree<Cell>(15, new Vector3(), 3);

        for (int i = 0; i < cells.Count; i++)
        {
            var c = cells[i];
            pointTree.Add(c, c.GetPosition());
        }

        for (int i = 0; i < cells.Count; i++)
        {
            var c = cells[i];
            var linkedCells = GetLinkedCells(i);
            var normal = Normal(P, i);
            c.ComputeNeighbourForces(linkedCells, normal);
            var nearby = new List<Cell>(pointTree.GetNearby(c.GetPosition(), 1));
            c.ComputeCollisionForces(nearby);
        }

        for (int i = 0; i < cells.Count; i++)
        {
            var c = cells[i];
            c.ApplyDisplacement();
            var pos = c.GetPosition();

            P.Vertices.SetVertex(i, pos.x, pos.y, pos.z);
        }
    }

    private void SplitCells()
    {
        var newCells = new List<Cell>();

        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].CellSplits())
            {
                var linkedEdges = P.Vertices.GetHalfedges(i);

                int edgeToSplit = linkedEdges[UnityEngine.Random.Range(0, linkedEdges.Length)];

                int SplitHEdge = P.Halfedges.TriangleSplitEdge(edgeToSplit);

                if (SplitHEdge != -1)
                {
                    int SplitCenter = P.Halfedges[SplitHEdge].StartVertex;
                    var pt = MidPt(P, i);
                    P.Vertices.SetVertex(SplitCenter, pt.x, pt.y, pt.z);
                    var newCell = CreateCell(pt);
                    var c = newCell.GetComponent<Cell>();
                    newCells.Add(c);
                    pointTree.Add(c, c.GetPosition());
                }
                Debug.Log("Split");
            }
        }

        cells.AddRange(newCells);

    }

    private Vector3 MidPt(PlanktonMesh P, int E)

    {
        var pV1 = P.Vertices[P.Halfedges[2 * E].StartVertex];
        var pV2 = P.Vertices[P.Halfedges[2 * E + 1].StartVertex];

        Vector3 Pos1 = new Vector3(pV1.X, pV1.Y, pV1.Z);
        Vector3 Pos2 = new Vector3(pV2.X, pV2.Y, pV2.Z);

        return (Pos1 + Pos2) * 0.5f;

    }

    private Vector3 Normal(PlanktonMesh P, int V)

    {
        var planktonVertex = P.Vertices[V];
        Vector3 Vertex = new Vector3(planktonVertex.X, planktonVertex.Y, planktonVertex.Z);

        Vector3 Norm = new Vector3();



        int[] OutEdges = P.Vertices.GetHalfedges(V);

        int[] Neighbours = P.Vertices.GetVertexNeighbours(V);

        Vector3[] OutVectors = new Vector3[Neighbours.Length];

        int Valence = P.Vertices.GetValence(V);



        for (int j = 0; j < Valence; j++)
        {
            var planktonV = P.Vertices[Neighbours[j]];
            var v = new Vector3(planktonV.X, planktonV.Y, planktonV.Z);
            OutVectors[j] = v - Vertex;
        }



        for (int j = 0; j < Valence; j++)

        {

            if (P.Halfedges[OutEdges[(j + 1) % Valence]].AdjacentFace != -1)

            {

                Norm += (Vector3.Cross(OutVectors[(j + 1) % Valence], OutVectors[j]));

            }

        }



        Norm.Normalize();

        return Norm;

    }

    void OnDrawGizmos()
    {
        pointTree.DrawAllBounds(); // Draw node boundaries
        pointTree.DrawAllObjects(); // Mark object positions
    }
}