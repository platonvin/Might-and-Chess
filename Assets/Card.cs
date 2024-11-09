using UnityEngine;

public enum CardAbility {
    None,
    Shift,
    Haste,
    Slow,
    Clone,
    Barricade,
    Wrap,
    Wind,
    ExtraMove,
    ExtendedCastling,
    RandomEffect,
    Sleep,
    Heal,
    Resurrect,
}

public enum CardCastType {
    None,
    OnPiece,
    OnBoard,
    OnMove
}

// also has sprite rendered
public class Card {
    public CardAbility Ability { get; private set; }
    public CardCastType Cast { get; private set; }
    public int Duration { get; private set; } // Duration for temporary effects
    public GameObject Instance { get; private set; }
    public Vector3 Position { get; private set; }
    public Quaternion rotation = Quaternion.LookRotation(new Vector3(0f, -1f, 0f));

    public Card(CardAbility ability, CardCastType cast, int duration, GameObject prefab, Vector3 initialPosition) {
        Ability = ability;
        Cast = cast;
        Duration = duration;
        Position = initialPosition;
        Instance = GameObject.Instantiate(prefab, initialPosition, rotation);
        AdjustSpritePosition(Instance);
    }

    private void AdjustSpritePosition(GameObject cardInstance) {
        // I love unity for setting pos to random value and not letting me to mod it in editor
        // foreach (Transform child in cardInstance.transform) {
        //     if (child.TryGetComponent<SpriteRenderer>(out var spriteRenderer)) {
        //         child.localPosition = Vector3.zero; // or set to a specific offset if needed
        //     }
        //     child.tag = "Card";
        // }
    }

    public void flip (){
        rotation.y = -rotation.y;
    }

    public void SetPosition(Vector3 newPosition) {
        Position = newPosition;
        UpdateTransform();
    }

    private void UpdateTransform() {
        if (Instance != null) {
            Instance.transform.SetPositionAndRotation(Position, Quaternion.LookRotation(new Vector3(0f, -1f, 0f)));
        }
    }

    public void Destroy() {
        if (Instance != null) {
            GameObject.Destroy(Instance);
            Instance = null;
        }
    }

    public void ApplyEffectOnPiece(ChessPiece piece) {
        switch (Ability) {
            case CardAbility.Shift:
                // Shift pieces around specified cell
                break;
            case CardAbility.Wrap:
                // Teleport across board edges
                break;
            case CardAbility.Barricade:
                // Place a barricade on a cell
                break;
            case CardAbility.Wind:
                // Push pieces in a direction
                break;
            // Add additional ability logic here
            default:
                break;
        }
    }
}
