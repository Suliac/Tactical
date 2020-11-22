using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class GenericsSpawner : MonoBehaviour
{
    public GameObject TextPrefab;

    [Header("Grid Params")]
    public uint ParamGridCellSize = 1;
    public uint ParamGridWidth = 10;
    public uint ParamGridLength = 10;
    public float3 ParamStartGridPosition = float3.zero;

    // Start is called before the first frame update
    void Start()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Create archetypes
        EntityArchetype archetypeGrid = entityManager.CreateArchetype(
            typeof(PlayerMouseInputsComponent),
            typeof(GridComponent)
        );

        EntityArchetype archetypeGridCell = entityManager.CreateArchetype(
            typeof(GridCellComponent)
        );

        // Create the entities
        Entity grid = entityManager.CreateEntity(archetypeGrid);
        entityManager.AddComponentData(grid, new PlayerMouseInputsComponent());
        entityManager.AddComponentData(grid, new GridComponent()
        {
            CellSize = ParamGridCellSize,
            GridLength = ParamGridLength,
            GridWidth = ParamGridWidth,
            StartGridPosition = ParamStartGridPosition
        });

        for (uint x = 0; x < ParamGridWidth; x++)
        {
            for (uint y = 0; y < ParamGridLength; y++)
            {
                Entity cell = entityManager.CreateEntity(archetypeGridCell);
                entityManager.AddComponentData(cell, new GridCellComponent()
                {
                    GridPosition = new uint2(x, y),
                    WorldPosition = new float3(x * ParamGridCellSize + ParamStartGridPosition.x, ParamStartGridPosition.y, y * ParamGridCellSize + ParamStartGridPosition.z)
            });
            }
        }
    }

}
