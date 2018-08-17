using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class meshTest : MonoBehaviour {

	// Use this for initialization
	void Start () {

        var mesh = GetComponent<MeshFilter>().mesh;


        var edges = MeshTools.BuildEdges(mesh.vertices.Length, mesh.triangles);
        var connetions = MeshTools.ConnetedVerices(edges, mesh, true);

        var planktonMesh = UnitySupport.ToPlanktonMesh(mesh);
        mesh = UnitySupport.ToUnityMesh(planktonMesh);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
