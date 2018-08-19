using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public class Cell : MonoBehaviour
{

    public float linkRestLength = 1.5f;
    public float springFactor = 0.1f;
    public float planarFactor = 0.1f;
    public float bulgeFactor = 0.2f;

    public float repulsionStrength = 0.1f;
    public float radiusOfInfluence = 1f;


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
        if (flipCoin < 0.99) result = false;
        else result = true;

        return result;
    }
    public void ApplyDisplacement()
    {
        this.transform.position += displacement;
        displacement = new Vector3();
    }
    public Vector3 GetPosition()
    {
        return this.transform.position;
    }
    public void ComputeDisplacement(List<Cell> linkedCells, Vector3 cellNormal, List<Cell> neighbourCells)
    {
        displacement = ComputeForces(linkedCells, cellNormal);
        //+ ComputeCollisionForces(neighbourCells);
    }
    private Vector3 ComputeCollisionForces(List<Cell> neighbourCells)
    {
        Vector3 collisionOffset = new Vector3();
        var P = this.transform.position;

        var collisionEffectPartial = new List<Vector3>();

        foreach (var nCell in neighbourCells)
        {
            var L = nCell.transform.position;
            var pDiff = P - L;
            var roi2 = Mathf.Pow(radiusOfInfluence, 2);

            var c = (roi2 - Mathf.Pow(pDiff.magnitude,2))/ roi2;
            var v = c * L;
            collisionEffectPartial.Add(v);
        }

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

        foreach (var currentCell in linkedCells)
        {
            var L = currentCell.transform.position;
            Vector3 PL = P - L;

            springTargetPartial.Add(L+linkRestLength*(PL.normalized));
            planarTargetPartial.Add(L);

            Vector3 LP = L - P;
            float dotN = Vector3.Dot((LP), Normal);
            float a1 = Mathf.Pow(linkRestLength, 2);
            float a2 = Mathf.Pow(LP.magnitude, 2);
            float a3 = Mathf.Pow(dotN, 2);
            float bDPvalue = Mathf.Sqrt(a1 - a2 + a3) + dotN;

            bulgeDistPartial.Add(bDPvalue);
        }



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
        planarTarget = planarTarget/linkedCells.Count;
        bulgeDist = bulgeDist / linkedCells.Count;
        bulgeTarget = P + bulgeDist * Normal;

        var endValue = springFactor * (springTarget - P)
              + planarFactor * (planarTarget - P)
              + bulgeFactor  * (bulgeTarget - P);

        return endValue;
    }

}