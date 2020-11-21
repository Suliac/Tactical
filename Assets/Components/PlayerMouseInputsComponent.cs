using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct PlayerMouseInputsComponent : IComponentData
{
    // Mouse input
    public float3 m_currentMousePosWorldView;

    public bool m_isPressingRightClick;
    public bool m_isPressingLeftClick;

    public bool m_wasRightClickPressedThisFrame;
    public bool m_wasLeftClickPressedThisFrame;
}
