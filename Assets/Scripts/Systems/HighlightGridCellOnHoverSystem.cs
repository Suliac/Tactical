using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class HighlightGridCellOnHoverSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float3 worldPos = float3.zero;

        Entities
            .WithName("Grid")
            .ForEach((in PlayerMouseInputsComponent mouseInput, in GridComponent grid) =>
            {
                worldPos = grid.StartGridPosition + new float3(mouseInput.CurrentMousePosGridView.x + grid.CellSize/2f, 0, mouseInput.CurrentMousePosGridView.y + grid.CellSize / 2f) * grid.CellSize;
            }).Run();

        Entities
            .WithName("Highlighter")
            .ForEach((ref Translation trans, in CellHighlighterComponent highlight) =>
            {
                trans.Value = worldPos;
            }).Run();
    }
}
