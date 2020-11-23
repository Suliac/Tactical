using Unity.Entities;
using Unity.Mathematics;

public class ProcessClickOnCellSystem : SystemBase
{
    protected override void OnCreate()
    {
    }

    protected override void OnUpdate()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        Entities
            .WithoutBurst()
            .WithStructuralChanges()
            .ForEach((ref GridCellComponent cell, in GridCellOnClickedComponent clickedCell) =>
            {
                cell.ClickCountCell++;
                entityManager.RemoveComponent<GridCellOnClickedComponent>(GetEntityQuery(typeof(GridCellComponent), typeof(GridCellOnClickedComponent)));
            }).Run();

    }
}
