using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public class Cell : MonoBehaviour
{

    public float linkRestLength = 1.0f;
    public float springFactor = 0.1f;
    public float planarFactor = 0.5f;
    public float bulgeFactor = 0.4f;

    public float repulsionStrength = 0.1f;
    public float radiusOfInfluence = 0.5f;


    /// <summary>
    ///  the set of all particles within the radius of influence of the current particle 
    ///  that aren't directly linked to the current particle. 
    ///  Directly linked particles are excluded from the repulsion calculations 
    ///  since they are considered directly attached to each other, 
    ///  and the influences between them are already controlled by the previously described other effects.
    /// </summary>
    private Vector3 displacement;

    public Cell()
    {
        displacement = new Vector3();
    }

 
    public bool CellSplits()
    {
        float flipCoin = Random.Range(0.0f, 1.0f);
        bool result;
        if (flipCoin < 0.8) result = false;
        else result = true;

        return result;
    }
    public void ApplyDisplacement()
    {
        this.transform.position = this.transform.position + displacement;
    }
    public Vector3 GetPosition()
    {
        return this.transform.position;
    }
    public void ComputeDisplacement(List<Cell> linkedCells, Vector3 cellNormal, List<Cell> neighbourCells)
    {
        displacement = ComputeForces(linkedCells, cellNormal) + ComputeCollisionForces(neighbourCells);
    }
    private Vector3 ComputeCollisionForces(List<Cell> neighbourCells)
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
    private Vector3 ComputeForces(List<Cell> linkedCells, Vector3 Normal)
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

            var dotN = Vector3.Dot((L - P), Normal);
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
        bulgeTarget = P + bulgeDist*Normal;


        return  springFactor * (springTarget - P)
              + planarFactor * (planarTarget - P)
              + bulgeFactor  * (bulgeTarget - P);
    }

}