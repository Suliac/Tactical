
using Unity.Entities;
using Unity.Mathematics;

public struct GridCell : IComponentData
{
    public uint2 GridPosition;
    public float3 WorldPosition;
    public bool IsPressed;
    public uint ClickCountCell;
}
