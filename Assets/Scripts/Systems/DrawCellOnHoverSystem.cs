using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class DrawCellOnHoverSystem : SystemBase
{
    float3 m_LastOnHoverPos = new float3(-1f, -1f, -1f);

    EntityQuery m_CellsOnHoverQuery;

    protected override void OnCreate()
    {
        m_CellsOnHoverQuery = GetEntityQuery(ComponentType.ReadOnly<GridCellComponent>(), ComponentType.ReadOnly<GridCellOnHoverComponent>());
    }

    protected override void OnUpdate()
    {
        int nbOnHover = m_CellsOnHoverQuery.CalculateEntityCount();

        if (nbOnHover == 1)
        {
            float3 gridWorldPos = new float3(0f, -1f, 0f);

            Entities
                .WithoutBurst()
                .WithName("GridCellInfoOnHover")
                .ForEach((in GridCellComponent cell, in GridCellOnHoverComponent onHover) =>
                {
                    gridWorldPos.x = cell.GridPosition.x;
                    gridWorldPos.y = 0.1f;
                    gridWorldPos.z = cell.GridPosition.y;
                }).Run();

            if (gridWorldPos.x != m_LastOnHoverPos.x || gridWorldPos.z != m_LastOnHoverPos.z) // onHoverChanged !
            {
                Entities
                    .WithoutBurst()
                    .WithName("HighlightOnHover")
                    .ForEach((ref Translation trans, in GridCellHighlighterOnHoverComponent highlighter) =>
                    {
                        Debug.Log($"DRAW ONHOVER | {gridWorldPos} - lastPos {m_LastOnHoverPos}");
                        trans.Value = new float3(gridWorldPos.x + 0.5f, gridWorldPos.y, gridWorldPos.z + 0.5f);
                    }).Run();

                m_LastOnHoverPos = gridWorldPos;
            }

        }
    }
}
