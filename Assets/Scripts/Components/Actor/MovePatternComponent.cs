using Unity.Entities;

[GenerateAuthoringComponent]
public struct MovePatternComponent : IComponentData
{
    public uint MaxRangeMove;
}
