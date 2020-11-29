using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class ProcessClickOnCellSystem : SystemBase
{
    protected override void OnUpdate()
    {
        bool alreadyHasSelectedCell = GetEntityQuery(typeof(GridCellSelectedComponent)).CalculateEntityCount() > 0;
        bool addedThisFrameNewSelectedCell = false;
        float2 newSelectedCellPos = float2.zero;

        Entities
            .WithName("GridCellOnHoverClicked")
            .WithStructuralChanges()
            .ForEach((Entity entity, int entityInQueryIndex, ref GridCellComponent cell, ref GridCellOnHoverComponent onHover) =>
            {
                if (onHover.LeftClickOnCellThisFrame)
                {
                    cell.ClickCountCell++;
                    newSelectedCellPos = cell.GridPosition;

                    if (!HasComponent<GridCellSelectedComponent>(entity))
                    {
                        //Debug.Log($"Add [SELECTED] to cell : {cell.GridPosition}");
                        EntityManager.AddComponent(entity, typeof(GridCellSelectedComponent));
                        addedThisFrameNewSelectedCell = true;
                    }

                    onHover.LeftClickOnCellThisFrame = false;
                }
            }).Run();

        if (addedThisFrameNewSelectedCell && alreadyHasSelectedCell)
        {
            Entities
                .WithName("SelectedCellClean")
                .WithStructuralChanges()
                .WithAll<GridCellSelectedComponent>()
                .ForEach((Entity entity, int entityInQueryIndex, in GridCellComponent cell, in GridCellSelectedComponent onHover) =>
                {
                    if(newSelectedCellPos.x != cell.GridPosition.x
                        || newSelectedCellPos.y != cell.GridPosition.y)
                    {
                        //Debug.Log($"Remove [SELECTED] to cell : {cell.GridPosition}");
                        EntityManager.RemoveComponent(entity, typeof(GridCellSelectedComponent));
                    }
                }).Run();
        }
    }
}
