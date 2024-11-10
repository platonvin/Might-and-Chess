using System;
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

// why c# is trying to be like c++ but worse in every aspect?
// compiler is so bad that we have il2cpp, lol

// stored in pice, changed by cast-on-piece cards
public struct PieceProps {
    bool isClone; // Clone
    bool isBarricade; // Barricade
    bool isSleeping; // sleep
    bool isDead;// grave?
}

// stored in field (single only), changed by cast-on-field cards
public struct FieldProps {
    // speed == 0 is default
    int speed; // Haste/Slow
    int extraMoves;
    bool healCasted; // Heal
}

[System.Serializable]
public class CardDef {
    CardCastType casrType;
    CardAbility ability;
    PieceProps pieceProps;
    FieldProps fieldProps;
}

// also has sprite rendered
public class Card {
    public CardAbility Ability { get; private set; }
    public CardCastType Cast { get; private set; }
    // public int Duration { get; private set; } // Duration for temporary effects
    public Func<WholeFuckingGame, ChessPiece, bool> ExecuteEffect { get; set; }

    public GameObject Instance { get; private set; }
    public Vector3 Position { get; private set; }
    bool flipped = false; // show frot or back i guess

    public Card(CardAbility ability, CardCastType cast, GameObject prefab, Vector3 initialPosition) {
        Quaternion rotation = Quaternion.LookRotation(new Vector3(0f, flipped ? -1f : +1f, 0f));

        Ability = ability;
        Cast = cast;
        // Duration = duration;
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

    public void flip() {
        // Debug.Log(rotation);
        // rotation.y = -rotation.y;
        // rotation = -rotation;
        flipped = !flipped;
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

    public void ApplyEffect(Board board, int x, int y, WholeFuckingGame game) {
        switch (Ability) {
            case CardAbility.Shift:
                ApplyShiftEffect(board, x, y);
                break;
            case CardAbility.Wrap:
                ApplyWrapEffect(board, x, y);
                break;
            // case CardAbility.Barricade:
            //     ApplyBarricadeEffect(board, x, y);
            //     break;
            // case CardAbility.Wind:
            //     ApplyWindEffect(board, x, y);
            //     break;
            case CardAbility.Haste:
                ApplyHasteEffect(board, x, y);
                break;
            case CardAbility.Slow:
                ApplySlowEffect(board, x, y);
                break;
            // case CardAbility.Clone:
            //     ApplyCloneEffect(board, x, y);
            //     break;
            // case CardAbility.ExtraMove:
            //     ApplyExtraMoveEffect(game);
            //     break;
            // case CardAbility.ExtendedCastling:
            //     ApplyExtendedCastlingEffect(game);
            //     break;
            // case CardAbility.RandomEffect:
            //     ApplyRandomEffect(board, x, y, game);
            //     break;
            // case CardAbility.Sleep:
            //     ApplySleepEffect(board[x, y]);
            //     break;
            // case CardAbility.Heal:
            //     ApplyHealEffect(board[x, y]);
            //     break;
            // case CardAbility.Resurrect:
            //     ApplyResurrectEffect(game);
            //     break;
            default:
                Debug.LogError("Wrong Card Ability");
                break;
        }
    }

    // Effect implementations (prototypes)

    private void ApplyShiftEffect(Board board, int x, int y) {
        ChessPiece start = board.GetChessPiece(x, y);
        ChessPiece end = board.GetChessPiece(x, y + 1);
        if ((end == null) && (start != null)) {
            // board.RemoveChessPiece(x,y);
            // board.SetChessPiece(x,y,start);
            board.DoShiftPiece(x,y, x,y+1);
        }
    }

    private void ApplyWrapEffect(Board board, int x, int y) {
    }


    private void ApplyHasteEffect(Board board, int x, int y) {
        ChessPiece piece = board.GetChessPiece(x, y);
        piece.Speed = +1;
    }

    private void ApplySlowEffect(Board board, int x, int y) {
        ChessPiece piece = board.GetChessPiece(x, y);
        piece.Speed = -1;

    }




}
