using Unity.Entities;

[GenerateAuthoringComponent]
public struct HealthComponent : IComponentData
{
    public uint Health;
}
