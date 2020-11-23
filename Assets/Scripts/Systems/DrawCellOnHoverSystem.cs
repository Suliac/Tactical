using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class DrawCellOnHoverSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float3 gridWorldPos = new float3(0f, -1f, 0f); ;

        Entities
            .WithoutBurst()
            .WithName("Grid")
            .ForEach((in GridCellComponent cell, in GridCellOnHoverComponent onHover) =>
            {
                gridWorldPos.x = cell.GridPosition.x;
                gridWorldPos.y = 0.1f;
                gridWorldPos.z = cell.GridPosition.y;
            }).Run();

        Entities
            .WithoutBurst()
            .WithName("Highlighter")
            .ForEach((ref Translation trans, in CellHighlighterComponent highlight) =>
            {
                trans.Value = new float3(gridWorldPos.x + 0.5f, gridWorldPos.y, gridWorldPos.z + 0.5f);
            }).Run();
    }
}
