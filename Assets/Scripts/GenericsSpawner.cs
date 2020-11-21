using Unity.Entities;
using UnityEngine;

public class GenericsSpawner : MonoBehaviour
{
    public GameObject GameManagerPrefab;

    // Start is called before the first frame update
    void Start()
    {
        Instantiate(GameManagerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        //var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        //// 1 - Create archetypes
        //EntityArchetype archetypeGameManager = entityManager.CreateArchetype(
        //    typeof(PlayerMouseInputsComponent)
        //);

        //// 2- Create the entities
        //Entity gameManager = entityManager.CreateEntity(archetypeGameManager);

        //entityManager.AddComponentData(gameManager, new PlayerMouseInputsComponent());
    }

}
