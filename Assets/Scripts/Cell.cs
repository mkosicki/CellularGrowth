using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public class Cell : MonoBehaviour
{

    public float linkRestLength;
    public float springFactor;
    public float planarFactor;
    public float bulgeFactor;

    public float repulsionStrength;
    public float radiusOfInfluence;

    private List<Cell> linkedCells;

    /// <summary>
    ///  the set of all particles within the radius of influence of the current particle 
    ///  that aren't directly linked to the current particle. 
    ///  Directly linked particles are excluded from the repulsion calculations 
    ///  since they are considered directly attached to each other, 
    ///  and the influences between them are already controlled by the previously described other effects.
    /// </summary>
    private List<Cell> neighbourCells;


    private Vector3 N = new Vector3();

    void Awake()
    {
        linkedCells = new List<Cell>();
    }

    void FixedUpdate()
    {
        this.transform.position = this.transform.position + ComputeForces() + ComputeCollision();
    }

    private Vector3 ComputeCollision()
    {
        Vector3 collisionOffset = new Vector3();
        var P = this.transform.position;

        var collisionEffectPartial = new List<Vector3>();

        Parallel.ForEach(neighbourCells, (nCell) =>
        {
            var L = nCell.transform.position;
            var pDiff = P - L;
            var roi2 = Mathf.Pow(radiusOfInfluence, 2);

            var c = (roi2 - Mathf.Pow(pDiff.magnitude,2))/ roi2;
            var v = c * L;
            collisionEffectPartial.Add(v);
        });

        for (int i = 0; i < neighbourCells.Count; i++)
        {
            collisionOffset += collisionEffectPartial[i];
        }

        return collisionOffset = repulsionStrength * collisionOffset;

    }


    private Vector3 ComputeForces()
    {
        var P = this.transform.position;

        var springTargetPartial = new List<Vector3>();
        var planarTargetPartial = new List<Vector3>();
        var bulgeDistPartial = new List<float>();

        Parallel.ForEach(linkedCells, (currentCell) =>
        {
            var L = currentCell.transform.position;
            

            springTargetPartial.Add(L+linkRestLength*(P-L));
            planarTargetPartial.Add(L);

            var dotN = Vector3.Dot((L - P), N);
            bulgeDistPartial.Add( 
                Mathf.Sqrt(Mathf.Pow(linkRestLength,2) - Mathf.Pow(L.magnitude,2) + Mathf.Pow(dotN,2))
                + dotN);
        });



        var springTarget = new Vector3();
        var planarTarget = new Vector3();
        var bulgeTarget = new Vector3();

        float bulgeDist = 0.0f;

        for (int i = 0; i < linkedCells.Count; i++)
        {
            springTarget += springTargetPartial[i];
            bulgeDist += bulgeDistPartial[i];
            planarTarget += planarTargetPartial[i];
        }

        springTarget = springTarget/linkedCells.Count;
        bulgeDist = bulgeDist / linkedCells.Count;
        bulgeTarget = P + bulgeDist*N;


        return  springFactor * (springTarget - P)
              + planarFactor * (planarTarget - P)
              + bulgeFactor  * (bulgeTarget - P);
    }

}