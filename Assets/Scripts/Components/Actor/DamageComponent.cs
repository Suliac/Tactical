using Unity.Entities;

[GenerateAuthoringComponent]
public struct DamageComponent : IComponentData
{
    public uint BaseDamage;
}
