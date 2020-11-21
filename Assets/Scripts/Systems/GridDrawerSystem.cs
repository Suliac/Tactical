using System.Numerics;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

public class GridDrawerSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities
            .WithName("GridDrawerSystem")
            .ForEach((in GridCell cell) =>
            {
                float3 leftDownPoint = new float3(cell.GridPosition.x, 1, cell.GridPosition.y) - 0.5f;
                float3 leftUpPoint = new float3(cell.GridPosition.x, 1, cell.GridPosition.y + 1) - 0.5f;
                float3 rightDownPoint = new float3(cell.GridPosition.x + 1, 1, cell.GridPosition.y) - 0.5f;
                float3 rightUpPoint = new float3(cell.GridPosition.x + 1, 1, cell.GridPosition.y + 1) - 0.5f;

                Debug.DrawLine(leftDownPoint, leftUpPoint);
                Debug.DrawLine(leftDownPoint, rightDownPoint);
                Debug.DrawLine(leftUpPoint, rightUpPoint);
                Debug.DrawLine(rightDownPoint, rightUpPoint);
            }).ScheduleParallel();
    }
}
