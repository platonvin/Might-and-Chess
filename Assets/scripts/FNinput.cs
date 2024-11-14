using UnityEngine;

public interface IInputHandler {
    bool IsInputDown();
    bool IsInputUp();
    Vector3 GetMousePosition();
    Ray GetMouseRay();
}

// Concrete implementation of IInputHandler for mouse input
public class MouseInputHandler : IInputHandler {
    public bool IsInputDown() => Input.GetMouseButtonDown(0);
    public bool IsInputUp() => Input.GetMouseButtonUp(0);
    public Vector3 GetMousePosition() => Input.mousePosition;
    public Ray GetMouseRay() => Camera.main.ScreenPointToRay(Input.mousePosition);
}