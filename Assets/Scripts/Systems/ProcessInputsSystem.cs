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
        var mouse = Mouse.current;
        Vector2 screenViewMousePos = mouse.position.ReadValue();

        if (!(0 > screenViewMousePos.x || 0 > screenViewMousePos.y || Screen.width < screenViewMousePos.x || Screen.height < screenViewMousePos.y))
        {
            bool rbIsPressed = mouse.rightButton.isPressed;
            bool lbIsPressed = mouse.leftButton.isPressed;
            bool rbPressedThisFrame = mouse.rightButton.wasPressedThisFrame;
            bool lbPressedThisFrame = mouse.leftButton.wasPressedThisFrame;

            Ray ray = Camera.main.ScreenPointToRay(screenViewMousePos);
            RaycastHit hit = new RaycastHit();

            // at each frame for now, not sure about that
            if (Physics.Raycast(ray, out hit, LayerMask.GetMask("FloorMap")))
            {
                Entities
                    .ForEach((ref PlayerMouseInputsComponent mouseInputs) =>
                    {
                        // mouse pos info
                        mouseInputs.CurrentMousePosWorldView = hit.point;
                    }).Run();


                // Acquire an ECB and convert it to a concurrent one to be able
                // to use it from a parallel job.
                var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer().AsParallelWriter();

                JobHandle jobHandle = Entities
                    .ForEach((Entity entity, int entityInQueryIndex, in GridCellComponent cell) =>
                    {
                        float worldFlooredX = Mathf.Floor(hit.point.x);
                        float worldFlooredZ = Mathf.Floor(hit.point.z);

                        bool isCurrentCellOnHover = cell.GridPosition.x == worldFlooredX && cell.GridPosition.y == worldFlooredZ;

                        // Manage OnHover
                        bool hasOnHoverComponent = HasComponent<GridCellOnHoverComponent>(entity);

                        if (hasOnHoverComponent && !isCurrentCellOnHover)
                        {
                            ecb.RemoveComponent<GridCellOnHoverComponent>(entityInQueryIndex, entity);
                        }
                        else if (isCurrentCellOnHover)
                        {
                            // Add component if the mouse is over a cell without an already existing OnHover component
                            if(!hasOnHoverComponent)
                                ecb.AddComponent<GridCellOnHoverComponent>(entityInQueryIndex, entity);

                            if (lbPressedThisFrame)
                                ecb.AddComponent<GridCellOnClickedComponent>(entityInQueryIndex, entity);
                        }

                    }).ScheduleParallel(Dependency);

                jobHandle.Complete();

                m_EndSimulationEcbSystem.AddJobHandleForProducer(Dependency);
            }
        }
    }
}