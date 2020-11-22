using System.Numerics;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

public class GridDrawerSystem : SystemBase
{
    protected override void OnUpdate()
    {
        uint gridCellSize = 0;
        float3 gridStartPos = float3.zero;

        Entities
            .WithName("Grid")
            .ForEach((in GridComponent grid) =>
            {
                if (gridCellSize == 0)
                    gridCellSize = grid.CellSize;
                else
                    throw new System.Exception("You should have only 1 Grid");
            }).Run();

        Entities
            .WithName("GridCells")
            .ForEach((in GridCellComponent cell) =>
            {
                float3 leftDownPoint = new float3(cell.WorldPosition.x, cell.WorldPosition.y, cell.WorldPosition.z);
                float3 leftUpPoint = new float3(cell.WorldPosition.x, cell.WorldPosition.y, cell.WorldPosition.z + gridCellSize);
                float3 rightDownPoint = new float3(cell.WorldPosition.x + gridCellSize, cell.WorldPosition.y, cell.WorldPosition.z);
                float3 rightUpPoint = new float3(cell.WorldPosition.x + gridCellSize, cell.WorldPosition.y, cell.WorldPosition.z + gridCellSize);

                Debug.DrawLine(leftDownPoint, leftUpPoint);
                Debug.DrawLine(leftDownPoint, rightDownPoint);
                Debug.DrawLine(leftUpPoint, rightUpPoint);
                Debug.DrawLine(rightDownPoint, rightUpPoint); 
            }).ScheduleParallel();
    }
}
