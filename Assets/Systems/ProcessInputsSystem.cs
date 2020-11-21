using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

public class ProcessInputsSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities
            .ForEach((ref PlayerMouseInputsComponent mouseInputs) =>
            {
                var mouse = Mouse.current;

                Vector2 screenViewMousePos = mouse.position.ReadValue();

                if (!(0 > screenViewMousePos.x || 0 > screenViewMousePos.y || Screen.width < screenViewMousePos.x || Screen.height < screenViewMousePos.y))
                {
                    mouseInputs.m_isPressingRightClick = mouse.rightButton.isPressed;
                    mouseInputs.m_isPressingLeftClick = mouse.leftButton.isPressed;
                    mouseInputs.m_wasRightClickPressedThisFrame = mouse.rightButton.wasPressedThisFrame;
                    mouseInputs.m_wasLeftClickPressedThisFrame = mouse.leftButton.wasPressedThisFrame;

                    Ray ray = Camera.main.ScreenPointToRay(screenViewMousePos);
                    RaycastHit hit = new RaycastHit();

                    if (Physics.Raycast(ray, out hit, LayerMask.GetMask("FloorMap"))) // at each frame for now, not sure about that
                    {
                            mouseInputs.m_currentMousePosWorldView = hit.point;
                    }
                }
            });
    }
}