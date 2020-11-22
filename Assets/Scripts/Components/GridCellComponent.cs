
using System;
using System.ComponentModel;
using Unity.Entities;
using Unity.Mathematics;

public struct GridCellComponent : IComponentData
{
    public float2 GridPosition;
    public float3 WorldPosition;
    public uint ClickCountCell;
}
