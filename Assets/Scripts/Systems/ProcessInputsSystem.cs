using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class ProcessInputsSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        // Find the ECB system once and store it for later usage
        m_EndSimulationEcbSystem = World
            .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        Vector2 screenViewMousePos = Mouse.current.position.ReadValue();

        if (!(0 > screenViewMousePos.x || 0 > screenViewMousePos.y || Screen.width < screenViewMousePos.x || Screen.height < screenViewMousePos.y))
        {
            Ray ray = Camera.main.ScreenPointToRay(screenViewMousePos);
            RaycastHit hit = new RaycastHit();

            // at each frame for now, not sure about that
            if (Physics.Raycast(ray, out hit, LayerMask.GetMask("FloorMap")))
            {
                // Acquire an ECB and convert it to a concurrent one to be able
                // to use it from a parallel job.
                var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();
                Entities
                    .ForEach((ref PlayerMouseInputsComponent mouseInputs) =>
                    {
                        // mouse pos info
                        mouseInputs.CurrentMousePosWorldView = hit.point;
                    }).Run();

                float worldFlooredX = Mathf.Floor(hit.point.x);
                float worldFlooredZ = Mathf.Floor(hit.point.z);

                // Process left click
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    Entities
                       .ForEach((ref GridCellOnHoverComponent onHover, in GridCellComponent cell) =>
                       {
                           // As we are deleting the OnHoverComponent at the end of the frame we may come here when we are the "old" hovered cell
                           if (worldFlooredX == cell.GridPosition.x
                              && worldFlooredZ == cell.GridPosition.y)
                           {
                               onHover.LeftClickOnCellThisFrame = true;
                           }
                       }).ScheduleParallel();
                }

                // Process on hover
                JobHandle jobHandle = Entities
                .ForEach((Entity entity, int entityInQueryIndex, in GridCellComponent cell) =>
                {
                    bool isCurrentCellOnHover = cell.GridPosition.x == worldFlooredX && cell.GridPosition.y == worldFlooredZ;
                    bool hasOnHoverComponent = HasComponent<GridCellOnHoverComponent>(entity);

                    if (hasOnHoverComponent && !isCurrentCellOnHover)
                    {
                        ecb.RemoveComponent<GridCellOnHoverComponent>(entityInQueryIndex, entity);
                    }

                    // Add component if the mouse is over a cell without an already existing OnHover component
                    if (!hasOnHoverComponent && isCurrentCellOnHover)
                    {
                        ecb.AddComponent<GridCellOnHoverComponent>(entityInQueryIndex, entity);
                    }

                }).ScheduleParallel(Dependency);

                jobHandle.Complete();

                m_EndSimulationEcbSystem.AddJobHandleForProducer(Dependency);
            }
        }
    }
}