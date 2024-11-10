using Unity.VisualScripting;
using UnityEngine;

// also has sprite rendered
public class ChessPiece {
    public PieceType Type { get; private set; }
    public PieceColor Color { get; private set; }
    public GameObject Instance { get; private set; }
    private Vector3 visualPosition;

    public int Speed { get; set; } = 0; //[-1; +1]
    public int SleepLeft { get; set; } = 0;
    //-1 means forever. 0 means is removed right now
    public int TimeInTurnsLeft { get; set; } = -1; 

    public ChessPiece(PieceType type, PieceColor color, GameObject prefab, Vector3 initialPosition) {
        Type = type;
        Color = color;
        visualPosition = initialPosition;
        Instance = GameObject.Instantiate(prefab, initialPosition, Quaternion.LookRotation(new Vector3(0f, -1f, 0f)));
    }

    public Vector3 GetVisualPosition() => visualPosition;

    public void SetVisualPosition(Vector3 newPosition) {
        visualPosition = newPosition;
        UpdateTransform();
    }

    private void UpdateTransform() {
        if (Instance != null) {
            Instance.transform.SetPositionAndRotation(visualPosition, Quaternion.LookRotation(new Vector3(0f, -1f, 0f)));
        }
    }

    public void Destroy() {
        if (Instance != null) {
            GameObject.Destroy(Instance);
        }
    }
}

public enum PieceType {
    None, 
    Pawn, Rook, Knight, Bishop, Queen, King,
    Barricade,
}

public enum PieceColor {
    None, 
    White, Black,
    Neutral,
}