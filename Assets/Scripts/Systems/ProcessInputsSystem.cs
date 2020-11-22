using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class ProcessInputsSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var mouse = Mouse.current;
        Vector2 screenViewMousePos = mouse.position.ReadValue();

        if (!(0 > screenViewMousePos.x || 0 > screenViewMousePos.y || Screen.width < screenViewMousePos.x || Screen.height < screenViewMousePos.y))
        {
            bool rbIsPressed = mouse.rightButton.isPressed;
            bool lbIsPressed = mouse.leftButton.isPressed;
            bool rbPressedThisFrame = mouse.rightButton.wasPressedThisFrame;
            bool lbPressedThisFrame = mouse.leftButton.wasPressedThisFrame;

            Ray ray = Camera.main.ScreenPointToRay(screenViewMousePos);
            RaycastHit hit = new RaycastHit();

            // at each frame for now, not sure about that
            if (Physics.Raycast(ray, out hit, LayerMask.GetMask("FloorMap")))
            {
                Entities
                    .ForEach((ref PlayerMouseInputsComponent mouseInputs, in GridComponent grid) =>
                    {
                        // mouse clicked event
                        mouseInputs.IsPressingRightClick = rbIsPressed;
                        mouseInputs.IsPressingLeftClick = lbIsPressed;
                        mouseInputs.WasRightClickPressedThisFrame = rbPressedThisFrame;
                        mouseInputs.WasLeftClickPressedThisFrame = lbPressedThisFrame;
                
                        // mouse pos info
                        mouseInputs.CurrentMousePosWorldView = hit.point;
                
                        mouseInputs.CurrentMousePosGridView.x = Mathf.FloorToInt(hit.point.x) / grid.CellSize - Mathf.FloorToInt(grid.StartGridPosition.x);
                        mouseInputs.CurrentMousePosGridView.y = Mathf.FloorToInt(hit.point.z) / grid.CellSize - Mathf.FloorToInt(grid.StartGridPosition.z);
                    }).Run();
            }
        }
    }
}