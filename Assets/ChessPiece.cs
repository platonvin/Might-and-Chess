using UnityEngine;

// also has sprite rendered
public class ChessPiece {
    public PieceType Type { get; private set; }
    public PieceColor Color { get; private set; }
    public GameObject Instance { get; private set; }

    private Vector3 position;

    public ChessPiece(PieceType type, PieceColor color, GameObject prefab, Vector3 initialPosition) {
        Type = type;
        Color = color;
        position = initialPosition;
        Instance = GameObject.Instantiate(prefab, initialPosition, Quaternion.LookRotation(new Vector3(0f, -1f, 0f)));
    }

    public Vector3 GetVisualPosition() => position;

    public void SetVisualPosition(Vector3 newPosition) {
        position = newPosition;
        UpdateTransform();
    }

    private void UpdateTransform() {
        if (Instance != null) {
            Instance.transform.SetPositionAndRotation(position, Quaternion.LookRotation(new Vector3(0f, -1f, 0f)));
        }
    }

    public void Destroy() {
        if (Instance != null) {
            GameObject.Destroy(Instance);
        }
    }
}

public enum PieceType {
    None, Pawn, Rook, Knight, Bishop, Queen, King
}

public enum PieceColor {
    None, White, Black
}