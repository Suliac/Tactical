using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class DrawCellSelectedSystem : SystemBase
{
    float3 m_LastSelectedPos = new float3(-1f, -1f, -1f);
    
    EntityQuery m_CellsSelectedQuery;

    protected override void OnCreate()
    {
        m_CellsSelectedQuery = GetEntityQuery(ComponentType.ReadOnly<GridCellComponent>(), ComponentType.ReadOnly<GridCellSelectedComponent>());
    }

    protected override void OnUpdate()
    {
        int nbSelected = m_CellsSelectedQuery.CalculateEntityCount();

        if (nbSelected == 1)
        {
            float3 gridSelectedPos = new float3(0f, -1f, 0f);

            Entities
                .WithoutBurst()
                .WithName("GridCellInfoSelected")
                .ForEach((in GridCellComponent cell, in GridCellSelectedComponent selected) =>
                {
                    gridSelectedPos.x = cell.GridPosition.x;
                    gridSelectedPos.y = 0.1f;
                    gridSelectedPos.z = cell.GridPosition.y;
                }).Run();

            if (gridSelectedPos.x != m_LastSelectedPos.x || gridSelectedPos.z != m_LastSelectedPos.z) // selected cell changed !
            {
                Entities
                    .WithoutBurst()
                    .WithName("HighlightSelected")
                    .ForEach((ref Translation trans, in GridCellHighlighterOnSelectComponent highlighter) =>
                    {
                        Debug.Log($"DRAW SELECT - {gridSelectedPos} - lastPos {m_LastSelectedPos}");
                        trans.Value = new float3(gridSelectedPos.x + 0.5f, gridSelectedPos.y, gridSelectedPos.z + 0.5f);
                    }).Run();
                m_LastSelectedPos = gridSelectedPos;
            }

        }
    }
}
