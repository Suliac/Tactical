using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ProcessClickOnCellSystem : SystemBase
{
    EntityQuery m_SelectedEntitiesQuery;

    protected override void OnCreate()
    {
        m_SelectedEntitiesQuery = GetEntityQuery(typeof(ActorSelectedComponent));
    }

    protected override void OnUpdate()
    {
        bool hasSelectedActor = m_SelectedEntitiesQuery.CalculateEntityCount() > 0;
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

        if (addedThisFrameNewSelectedCell)
        {
            Entities
                .WithName("IsActorSelected")
                .WithStructuralChanges()
                .WithAll<ActorComponent>()
                .ForEach((Entity entity, int entityInQueryIndex, in Translation translation) =>
                {
                    if (Mathf.Floor(translation.Value.x) == newSelectedCellPos.x
                                && Mathf.Floor(translation.Value.z) == newSelectedCellPos.y)
                    {
                        Debug.Log($"Add [SELECTED] to [ACTOR]");
                        EntityManager.AddComponent(entity, typeof(ActorSelectedComponent));
                    }
                }).Run();


            if (alreadyHasSelectedCell)
            {
                bool removeCellSelected = false;
                float2 removedCellPos = float2.zero;

                Entities
                    .WithName("SelectedCellClean")
                    .WithStructuralChanges()
                    .WithAll<GridCellSelectedComponent>()
                    .ForEach((Entity entity, int entityInQueryIndex, in GridCellComponent cell, in GridCellSelectedComponent onHover) =>
                    {
                        if (newSelectedCellPos.x != cell.GridPosition.x
                            || newSelectedCellPos.y != cell.GridPosition.y)
                        {
                            //Debug.Log($"Remove [SELECTED] to [CELL] : {cell.GridPosition}");
                            EntityManager.RemoveComponent(entity, typeof(GridCellSelectedComponent));
                            removeCellSelected = true;
                            removedCellPos = cell.GridPosition;
                        }
                    }).Run();

                if (hasSelectedActor && removeCellSelected)
                {
                    Entities
                        .WithStructuralChanges()
                        .WithAll<ActorSelectedComponent>()
                        .ForEach((Entity entity, int entityInQueryIndex, in Translation translation) =>
                        {
                            if (Mathf.Floor(translation.Value.x) == removedCellPos.x
                                && Mathf.Floor(translation.Value.z) == removedCellPos.y)
                            {
                                Debug.Log($"Remove [SELECTED] to [ACTOR]");
                                EntityManager.RemoveComponent(entity, typeof(ActorSelectedComponent));
                            }
                        }).Run();
                } 
            }
        }
    }
}
