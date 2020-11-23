using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct PlayerMouseInputsComponent : IComponentData
{
    public float3 CurrentMousePosWorldView;
}
