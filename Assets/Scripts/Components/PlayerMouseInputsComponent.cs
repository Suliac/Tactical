using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct PlayerMouseInputsComponent : IComponentData
{
    // Mouse input
    public float3 CurrentMousePosWorldView;

    public bool IsPressingRightClick;
    public bool IsPressingLeftClick;

    public bool WasRightClickPressedThisFrame;
    public bool WasLeftClickPressedThisFrame;
}
