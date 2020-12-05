using Unity.Assertions;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ProcessMovableCellsSystem : SystemBase
{
    private EntityQuery m_SelectedActorQuery;
    private EntityQuery m_CellsQuery;

    private EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;

    protected override void OnCreate()
    {
        //////////////////////////////////////////////////////
        // get/create command buffer
        m_EndSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        //////////////////////////////////////////////////////
        // prepare query that retrieves the actors
        m_SelectedActorQuery = GetEntityQuery(
            ComponentType.ReadOnly<ActorSelectedComponent>(),
            ComponentType.ReadOnly<Translation>(),
            ComponentType.ReadOnly<MovePatternComponent>());

        m_CellsQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { ComponentType.ReadOnly<GridCellComponent>() },
            None = new ComponentType[] { ComponentType.ReadOnly<GridCellMovable>() }
        });
    }

    protected override void OnUpdate()
    {
        int actorSelectedNumber = m_SelectedActorQuery.CalculateEntityCount();
        int cellsMovableNumber = GetEntityQuery(typeof(GridCellMovable)).CalculateEntityCount();

        // most of the time, there are already cells movable displayed, or no actor is selected
        if (actorSelectedNumber <= 0 || cellsMovableNumber > 0)
        {
            return;
        }

        //////////////////////////////////////////////////////
        // 1 - Get the selected actor pos if relevant and the move info
        // & Loop through all gridcell to find if any has obstacle
        var findAndLinkMovableCells = new BatchedFindAndLinkMovableCells
        {
            CellComponentsAccessors = GetComponentTypeHandle<GridCellComponent>(true),
            CellEntitiesAccessors = GetEntityTypeHandle(),

            ActorTranslationArray = m_SelectedActorQuery.ToComponentDataArray<Translation>(Allocator.TempJob),
            ActorMovePatternArray = m_SelectedActorQuery.ToComponentDataArray<MovePatternComponent>(Allocator.TempJob),
            ActorEntitiesArray = m_SelectedActorQuery.ToEntityArray(Allocator.TempJob),

            CommandBuffer = m_EndSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter()
        };

        //////////////////////////////////////////////////////
        // 2 - Schedule the job
        JobHandle jobHandle = findAndLinkMovableCells.ScheduleParallel(m_CellsQuery, 4 /* batch per chunk */, Dependency);

        //////////////////////////////////////////////////////
        // 3 - Wait for the job to complete
        jobHandle.Complete();

        //////////////////////////////////////////////////////
        // 4 - Send commands from buffer
        m_EndSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }

    [BurstCompile]
    private struct BatchedFindAndLinkMovableCells : IJobEntityBatch
    {
        [ReadOnly] public ComponentTypeHandle<GridCellComponent> CellComponentsAccessors;
        [ReadOnly] public EntityTypeHandle CellEntitiesAccessors;

        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> ActorEntitiesArray;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Translation> ActorTranslationArray;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<MovePatternComponent> ActorMovePatternArray;
        public EntityCommandBuffer.ParallelWriter CommandBuffer;

        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            Assert.AreEqual(ActorEntitiesArray.Length, ActorTranslationArray.Length, "ActorEntitiesArray & ActorPositionsArray don't have the same length !");
            Assert.AreEqual(ActorEntitiesArray.Length, ActorMovePatternArray.Length, "ActorEntitiesArray & ActorMovePatternArray don't have the same length !");
            Assert.AreEqual(ActorEntitiesArray.Length, 1, "We shouldn't have more than 1 selected entity !");

            NativeArray<GridCellComponent> cellComponents = batchInChunk.GetNativeArray(CellComponentsAccessors);
            NativeArray<Entity> cellEntities = batchInChunk.GetNativeArray(CellEntitiesAccessors);

            for (int i = 0; i < cellEntities.Length; i++)
            {
                Entity cellEntity = cellEntities[i];
                GridCellComponent cellInfo = cellComponents[i];
                float2 actorGridPos = new float2(Mathf.Floor(ActorTranslationArray[0].Value.x), Mathf.Floor(ActorTranslationArray[0].Value.z));

                float2 vectorGridPos = actorGridPos - cellInfo.GridPosition;
                float xValue = Mathf.Abs(vectorGridPos.x); //FastAbs((uint)vectorGridPos.x);
                float yValue = Mathf.Abs(vectorGridPos.y); //FastAbs((uint)vectorGridPos.y);


                if (xValue + yValue <= ActorMovePatternArray[0].MaxRangeMove)
                {
                    //Debug.Log($"ADD CELL {cellInfo.GridPosition} MOVABLE | {xValue} + {yValue} <= {ActorMovePatternArray[0].MaxRangeMove}");
                    CommandBuffer.AddComponent<GridCellMovable>(batchIndex, cellEntity);
                }
            }
        }

        private uint FastAbs(uint number) // doesn't work
        {
            uint temp = number >> 31; // mask the sign bit
            number = number ^ temp;   // toggle the bits if value is negative
            number += temp & 1;

            return number;
        }
    }
}
