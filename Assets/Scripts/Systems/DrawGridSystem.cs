using System.Numerics;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

public class DrawGridSystem : SystemBase
{
    protected override void OnUpdate()
    {        
        Entities
            .WithName("GridCells")
            .ForEach((in GridCellComponent cell) =>
            {
                float3 leftDownPoint = new float3(cell.GridPosition.x, 0.1f, cell.GridPosition.y);
                float3 leftUpPoint = new float3(cell.GridPosition.x, 0.1f, cell.GridPosition.y + 1f);
                float3 rightDownPoint = new float3(cell.GridPosition.x + 1f, 0.1f, cell.GridPosition.y);
                float3 rightUpPoint = new float3(cell.GridPosition.x + 1f, 0.1f, cell.GridPosition.y + 1f);

                Debug.DrawLine(leftDownPoint, leftUpPoint);
                Debug.DrawLine(leftDownPoint, rightDownPoint);
                Debug.DrawLine(leftUpPoint, rightUpPoint);
                Debug.DrawLine(rightDownPoint, rightUpPoint); 
            }).ScheduleParallel();
    }
}
