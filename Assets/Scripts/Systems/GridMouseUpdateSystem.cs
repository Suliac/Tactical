using Unity.Entities;
using Unity.Mathematics;

public class GridMouseUpdateSystem : SystemBase
{
    protected override void OnUpdate()
    {
        bool wasLeftClickPressedThisFrame = false;
        float2 mousePos = float2.zero;

        Entities
            .WithName("Grid")
            .ForEach((in PlayerMouseInputsComponent mouseInput) =>
            {
                wasLeftClickPressedThisFrame = mouseInput.WasLeftClickPressedThisFrame;
                mousePos = mouseInput.CurrentMousePosGridView;
            }).Run();

        Entities
            .WithName("GridCell")
            .ForEach((ref GridCellComponent cell) =>
            {
                if (wasLeftClickPressedThisFrame 
                        && (mousePos.x == cell.GridPosition.x && mousePos.y == cell.GridPosition.y))
                {
                    cell.ClickCountCell++;
                }
            }).ScheduleParallel();
    }
}
