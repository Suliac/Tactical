
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class UnlinkEntityToCellSystem : SystemBase
{
    private EntityQuery m_DirtyCellsQuery;
    private EntityQuery m_ActorsQuery;

    protected override void OnCreate()
    {
        m_DirtyCellsQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { ComponentType.ReadOnly<GridCellComponent>() }
        });
        m_ActorsQuery = GetEntityQuery(ComponentType.ReadOnly<ActorComponent>(), ComponentType.ReadOnly<Translation>());
    }

    protected override void OnUpdate()
    {
        if (m_DirtyCellsQuery.CalculateEntityCount() > 0)
        {
            NativeArray<Entity> actorEntity = m_ActorsQuery.ToEntityArray(Allocator.Temp);
            NativeArray<Translation> actorTranslation = m_ActorsQuery.ToComponentDataArray<Translation>(Allocator.Temp);

            Entities
                .WithName("CellsWithDirtyLink")
                .WithStructuralChanges()
                .WithAll<GridCellLinkDirty, GridCellLinkToEntity, GridCellComponent>()
                .ForEach((Entity entity, int entityInQueryIndex, in GridCellLinkToEntity cellLink, in GridCellComponent cellInfo) =>
                {
                    for (int i = 0; i < actorEntity.Length; i++)
                    {
                        if (actorEntity[i] == cellLink.LinkedActor)
                        {
                            float2 actorGridPos = new float2(Mathf.Floor(actorTranslation[i].Value.x), Mathf.Floor(actorTranslation[i].Value.z));

                        // linked actor isn't on this celle anymore ...
                        if (actorGridPos.x != cellInfo.GridPosition.x || actorGridPos.y != cellInfo.GridPosition.y)
                            {
                                EntityManager.RemoveComponent(entity, typeof(GridCellLinkToEntity));
                                EntityManager.RemoveComponent(entity, typeof(GridCellLinkDirty));
                                Debug.Log($"Unlink cell at pos {cellInfo.GridPosition} with its old entity");
                            }
                        }
                    }
                }).Run();

            actorEntity.Dispose();
            actorTranslation.Dispose(); 
        }
    }
}
