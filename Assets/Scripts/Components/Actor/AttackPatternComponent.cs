using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct AttackPatternComponent : IComponentData
{
    public uint MaxRange;
    public uint MinRange;
    public uint AoeSize;
}
