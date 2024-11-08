using UnityEngine;

// also has sprite rendered
public class SingleCard {
    public CardAbility Ability { get; private set; }
    public CardCastType Cast { get; private set; }
    public GameObject Instance { get; private set; }

    private Vector3 position;

    public SingleCard(CardAbility ability, CardCastType cast, GameObject prefab, Vector3 initialPosition) {
        Ability = ability;
        Cast = cast;
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

public enum CardAbility {
    None, 
    type1, 
    type2, 
    Teleport,
    Clone,
}

public enum CardCastType {
    None, 
    CastOnPiece,
    OnBoard,
    OnMove, 
}