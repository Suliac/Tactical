
using System;
using System.ComponentModel;
using Unity.Entities;
using Unity.Mathematics;

public struct GridCellComponent : IComponentData
{
    public float2 GridPosition;
    public uint ClickCountCell;
}
