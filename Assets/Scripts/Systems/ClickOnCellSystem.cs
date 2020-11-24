using Unity.Entities;
using Unity.Mathematics;

public class ProcessClickOnCellSystem : SystemBase
{
    EntityManager entityManager;

    protected override void OnCreate()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    protected override void OnUpdate()
    {
        Entities
            .ForEach((ref GridCellComponent cell, ref GridCellOnHoverComponent onHover) =>
            {
                if (onHover.LeftClickOnCellThisFrame)
                {
                    cell.ClickCountCell++;
                    onHover.LeftClickOnCellThisFrame = false;
                }
            }).ScheduleParallel();

    }
}
