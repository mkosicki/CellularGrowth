using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public class Cell : MonoBehaviour
{

    public float linkRestLength = 0.75f;
    public float springFactor = 0.1f;
    public float planarFactor = 0.5f;
    public float bulgeFactor = 0.5f;

    public float repulsionStrength = 0.99f;
    public float radiusOfInfluence = 3f;

    private int numOfDivisions;

    /// <summary>
    ///  the set of all particles within the radius of influence of the current particle 
    ///  that aren't directly linked to the current particle. 
    ///  Directly linked particles are excluded from the repulsion calculations 
    ///  since they are considered directly attached to each other, 
    ///  and the influences between them are already controlled by the previously described other effects.
    /// </summary>
    private Vector3 displacementF;
    private Vector3 collisonF;

    public Cell()
    {
        displacementF = new Vector3();
        collisonF = new Vector3();
        numOfDivisions = 0;
    }

 
    public bool CellSplits()
    {
        float flipCoin = Random.Range(0.0f, 1.0f);
        bool result;
        if (flipCoin < 0.99) result = false;
        else {
            if (numOfDivisions < 1)
            {
                result = true;
                numOfDivisions++;
            }
            else result = false;
        }

        return result;
    }
    public void ApplyDisplacement()
    {
        var totalDisplacement =  collisonF + displacementF;
        this.transform.position += totalDisplacement;
    }
    public Vector3 GetPosition()
    {
        return this.transform.position;
    }

    public void ComputeCollisionForces(List<Cell> neighbourCells)
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
            var v = c * pDiff.normalized;
            collisionEffectPartial.Add(v);
        }

        for (int i = 0; i < neighbourCells.Count; i++)
        {
            collisionOffset += collisionEffectPartial[i];
        }

        collisonF = repulsionStrength * collisionOffset;
    }
    public void ComputeNeighbourForces(List<Cell> linkedCells, Vector3 Normal)
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
            float sum = a1 - a2 + a3;
            if (sum < 0) sum = 0;
            float bDPvalue = Mathf.Sqrt(sum) + dotN;
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

        displacementF = endValue;
    }

}