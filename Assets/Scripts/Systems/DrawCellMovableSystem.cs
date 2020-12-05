using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class DrawCellMovableSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;

    EntityQuery m_CellsMovableQuery;
    EntityQuery m_UsedMovableCellHighlighterQuery;

    protected override void OnCreate()
    {
        // get/create command buffer
        m_EndSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        m_CellsMovableQuery = GetEntityQuery(ComponentType.ReadOnly<GridCellComponent>(), ComponentType.ReadOnly<GridCellMovable>());
        m_UsedMovableCellHighlighterQuery = GetEntityQuery(new ComponentType[] { typeof(GridCellHighlighterMovableComponent), typeof(GridCellHighlighterUsedComponent) });
    }
    protected override void OnUpdate()
    {
        // already displaying?
        if (m_UsedMovableCellHighlighterQuery.CalculateEntityCount() > 0
            || GetEntityQuery(ComponentType.ReadOnly<ActorSelectedComponent>()).CalculateEntityCount() == 0)
        {
            return;
        }

        NativeArray<GridCellComponent> posOfCellsToHiglight = m_CellsMovableQuery.ToComponentDataArray<GridCellComponent>(Allocator.TempJob);

        var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

        var moveHighlitherJob = Entities
            .WithName("CellMovableDrawer")
            .WithAll<GridCellHighlighterMovableComponent>()
            .WithNone<GridCellHighlighterUsedComponent>()
            .WithReadOnly(posOfCellsToHiglight)
            .WithDisposeOnCompletion(posOfCellsToHiglight)
            .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation) =>
            {
                if (entityInQueryIndex < posOfCellsToHiglight.Length)
                {
                    float2 cellPos = posOfCellsToHiglight[entityInQueryIndex].GridPosition;

                    translation.Value = new float3(cellPos.x + 0.5f, 0.1f, cellPos.y + 0.5f);
                    ecb.AddComponent<GridCellHighlighterUsedComponent>(entityInQueryIndex, entity);
                }
            }).ScheduleParallel(Dependency);

        moveHighlitherJob.Complete();

        m_EndSimulationEcbSystem.AddJobHandleForProducer(Dependency);
    }
}
