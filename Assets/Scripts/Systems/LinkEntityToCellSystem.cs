using Unity.Assertions;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class FindEntityOnCellSystem : SystemBase
{
    private struct EntityWithPosition
    {
        public Entity Entity;
        public float2 Position;
    }

    private EntityQuery m_ActorsQuery;
    private EntityQuery m_CellsQuery;

    private EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;

    protected override void OnCreate()
    {
        //////////////////////////////////////////////////////
        // get/create command buffer
        m_EndSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        //////////////////////////////////////////////////////
        // prepare query that retrieves the actors
        m_ActorsQuery = GetEntityQuery(ComponentType.ReadOnly<ActorComponent>(), ComponentType.ReadOnly<Translation>());
        m_CellsQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { ComponentType.ReadOnly<GridCellComponent>() },
            None = new ComponentType[] { typeof(GridCellLinkToEntity) }
        });
    }

    protected override void OnDestroy()
    {
    }

    protected override void OnUpdate()
    {
        // NB : to much NativeArray instantiation

        int actorsNumberWithLink = m_ActorsQuery.CalculateEntityCount();
        int cellsNumberWithLink = GetEntityQuery(typeof(GridCellLinkToEntity)).CalculateEntityCount();

        // as many linked cells as actors, we don't need to update anything
        if (cellsNumberWithLink >= actorsNumberWithLink)
        {
            //return;
        }
        
        //////////////////////////////////////////////////////
        // 2 - Then try to link actors with cells
        var linkToCellJob = new BatchedLinkActorToCellJob
        {
            CellComponentsAccessors = GetComponentTypeHandle<GridCellComponent>(true),
            CellEntitiesAccessors = GetEntityTypeHandle(),

            ActorTranslationArray = m_ActorsQuery.ToComponentDataArray<Translation>(Allocator.TempJob),
            ActorEntitiesArray = m_ActorsQuery.ToEntityArray(Allocator.TempJob),

            CommandBuffer = m_EndSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter()
        };

        //////////////////////////////////////////////////////
        // 3 - Schedule the job
        JobHandle jobHandle = linkToCellJob.ScheduleParallel(m_CellsQuery, 4 /* batch per chunk */, Dependency);

        //////////////////////////////////////////////////////
        // 4 - Wait for the job to complete
        jobHandle.Complete();

        //////////////////////////////////////////////////////
        // 5 - Send commands from buffer
        m_EndSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }

    [BurstCompile]
    private struct BatchedLinkActorToCellJob : IJobEntityBatch
    {
        [ReadOnly] public ComponentTypeHandle<GridCellComponent> CellComponentsAccessors;
        [ReadOnly] public EntityTypeHandle CellEntitiesAccessors;

        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> ActorEntitiesArray;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Translation> ActorTranslationArray;
        public EntityCommandBuffer.ParallelWriter CommandBuffer;

        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            Assert.AreEqual(ActorEntitiesArray.Length, ActorTranslationArray.Length, "ActorEntitiesArray & ActorPositionsArray don't have the same length !");

            NativeArray<GridCellComponent> cellComponents = batchInChunk.GetNativeArray(CellComponentsAccessors);
            NativeArray<Entity> cellEntities = batchInChunk.GetNativeArray(CellEntitiesAccessors);

            for (int i = 0; i < cellEntities.Length; i++)
            {
                Entity cellEntity = cellEntities[i];
                GridCellComponent cellInfo = cellComponents[i];

                // for each cell, we iterate over all actor to determine if the actor is inside the cell
                for (int j = 0; j < ActorEntitiesArray.Length; j++)
                {
                    Entity actorEntity = ActorEntitiesArray[j];
                    float2 actorPos = new float2(Mathf.Floor(ActorTranslationArray[j].Value.x), Mathf.Floor(ActorTranslationArray[j].Value.z));
                    
                    if (actorPos.x == cellInfo.GridPosition.x
                            && actorPos.y == cellInfo.GridPosition.y)
                    {
                        CommandBuffer.AddComponent(batchIndex, cellEntity, new GridCellLinkToEntity() { LinkedActor = actorEntity });
                    }
                }
            }
        }
    }
}
