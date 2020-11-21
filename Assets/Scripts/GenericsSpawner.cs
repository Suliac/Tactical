using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class GenericsSpawner : MonoBehaviour
{
    public GameObject GameManagerPrefab;
    [Header("Grid")]
    public uint GridLength = 2;
    public uint GridWidth = 2;
    // Start is called before the first frame update
    void Start()
    {
        Instantiate(GameManagerPrefab, new Vector3(0, 0, 0), Quaternion.identity);

        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Create archetypes
        EntityArchetype archetypeGridCell = entityManager.CreateArchetype(
            typeof(GridCell)
        );

        // Create the entities
        for (uint x = 0; x < GridWidth; x++)
        {
            for (uint y = 0; y < GridLength; y++)
            {
                Entity gameManager = entityManager.CreateEntity(archetypeGridCell);
                entityManager.AddComponentData(gameManager, new GridCell() { GridPosition = new uint2(x, y)});
            }
        }

    }

}
