using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum CardAbility {
    None,
    Shift, // controllable? +1 up for now
    Haste, // +1 speed
    Slow, // -1 speed
    Clone, // 2 turn
    Barricade, // force field
    Wrap, // teleport to other side
    Wind, // move all figures in selection in direction?
    ExtraMove, // .
    ExtendedCastling, // from anywhere i guess
    RandomEffect, // literally
    Blind,
    Dispel, // remove sleep / hypnotize / clone / barricade
    Resurrect, // graves needed?
    AntiMagic, // no cast on him i guess
    Weakness, // resurrect better one
    Sacrifice, // resurrect better one
    Hypnotize, // pawn
    MagicArrow, // kill pawn
    Armageddon, // damage all
    Jumper, // 1 piece skip allowed

    // one Visions per turn for easier gameplay? 
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

    public GameObject Instance { get; private set; }
    public Vector3 Position { get; private set; }
    bool flipped = false; // show frot or back i guess

    private TextMeshProUGUI descriptionText;
    private Transform manaContainer;
    private GameObject manaImagePrefab; // Assign this from WholeGame

    public Card(CardAbility ability, CardCastType cast, GameObject cardPrefab, Vector3 initialPosition,
                int manaCost, string description, GameObject manaImagePrefab, GameObject descriptionTextPrefab) {
        Ability = ability;
        Cast = cast;
        Position = initialPosition;
        this.manaImagePrefab = manaImagePrefab;

        // Instantiate the card GameObject
        Instance = GameObject.Instantiate(cardPrefab, initialPosition, Quaternion.LookRotation(new(0f, flipped ? -1f : +1f, 0f)));

        // Create UI elements as children of the card instance
        CreateDescriptionText(descriptionTextPrefab, description);
        CreateManaCostDisplay(manaCost);
    }

    private void CreateDescriptionText(GameObject descriptionTextPrefab, string description) {
        GameObject descriptionObject = GameObject.Instantiate(descriptionTextPrefab, Instance.transform);
        descriptionObject.name = "DescriptionText";
        descriptionText = descriptionObject.GetComponent<TextMeshProUGUI>();
        if (descriptionText != null) {
            descriptionText.text = description;
        }
    }

    private void CreateManaCostDisplay(int manaCost) {
        // Create a container for mana icons
        GameObject manaContainerObject = new GameObject("ManaContainer");
        manaContainerObject.transform.SetParent(Instance.transform);
        manaContainer = manaContainerObject.transform;

        manaContainer.localPosition = new(-0.327f, 0.5f, -0.001f); // z cause idk unity is bullshit
        manaContainer.localRotation = Quaternion.identity;
        // Adjust the spacing between icons based on mana cost

        float iconSpacing = 0.9f;  // Adjust as needed
        float totalWidth = (manaCost - 1) * iconSpacing;  // Calculate total width of icons

        // Center each icon within manaContainer
        for (int i = 0; i < manaCost; i++) {
            GameObject manaIcon = GameObject.Instantiate(manaImagePrefab, manaContainer);
            manaIcon.transform.localPosition = new(i * iconSpacing, 0, -0.001f);  // Centered position
        }
    }

    public void flip() {
        flipped = !flipped;
    }

    public void SetPosition(Vector3 newPosition) {
        Position = newPosition;
        UpdateTransform();
    }

    private void UpdateTransform() {
        if (Instance != null) {
            Instance.transform.SetPositionAndRotation(Position, Quaternion.LookRotation(new(0f, -1f, 0f)));
        }
    }

    public void Destroy() {
        if (Instance != null) {
            GameObject.Destroy(Instance);
            Instance = null;
        }
    }

    public bool tryApplyEffect(Board board, int x, int y, WholeFuckingGame game) {
        switch (Ability) {
            case CardAbility.Haste: return ApplyHasteEffect(board, x, y);
            case CardAbility.Slow: return ApplySlowEffect(board, x, y);
            case CardAbility.Shift: return ApplyShiftEffect(board, x, y);
            case CardAbility.Clone: return ApplyCloneEffect(board, x, y, game);
            case CardAbility.MagicArrow: return ApplyMagicArrowEffect(board, x, y);
            case CardAbility.AntiMagic: return ApplyAntimagicEffect(board, x, y);
            // case CardAbility.Barricade: //     return ApplyBarricadeEffect(board, x, y);
            // case CardAbility.Wrap: //     return ApplyWrapEffect(board, x, y);
            // case CardAbility.Wind: //     return ApplyWindEffect(board, game);
            case CardAbility.Blind: return ApplySleepEffect(board, x, y);
            case CardAbility.Dispel: return ApplyDispelEffect(board, x, y);
            // case CardAbility.Resurrect: //     return ApplyResurrectEffect(board, game, x, y);
            case CardAbility.Armageddon: return ApplyArmageddonEffect(board, game);
            default:
                Debug.LogError("Wrong Card Ability");
                return false;
        }
    }

    // Effect implementations (prototypes)

    private bool ApplyShiftEffect(Board board, int x, int y) {
        ChessPiece piece = board.getPiece(x, y);
        ChessPiece end = board.getPiece(x, y + 1);
        if (piece.antiMagicLeft > 0) return false;

        if ((end == null) && (piece != null)) {
            // board.RemoveChessPiece(x,y);
            // board.SetChessPiece(x,y,start);
            board.doShiftPiece(x, y, x, y + 1);
            return true;
        }
        return false;
    }

    private bool ApplyHasteEffect(Board board, int x, int y) {
        ChessPiece piece = board.getPiece(x, y);
        if (piece == null) return false;
        if (piece.antiMagicLeft > 0) return false;

        piece.SpeedLevel = +1;
        piece.Speedleft = 3;
        return true;
    }

    private bool ApplySlowEffect(Board board, int x, int y) {
        ChessPiece piece = board.getPiece(x, y);
        if (piece == null) return false;
        if (piece.antiMagicLeft > 0) return false;

        piece.SpeedLevel = -1;
        piece.Speedleft = 3;
        return true;
    }

    private bool ApplyCloneEffect(Board board, int x, int y, WholeFuckingGame game) {
        ChessPiece piece = board.getPiece(x, y);
        if (piece == null) return false;
        if (piece.antiMagicLeft > 0) return false;

        // Clone piece at x, y and place it in an empty adjacent spot if possible
        List<ivec2> emptyNeighbors = GetEmptyNeighbors(board, x, y);
        if (emptyNeighbors.Count == 0) return false;

        int rnd_id = UnityEngine.Random.Range(0, emptyNeighbors.Count - 1);
        ivec2 clonePos = emptyNeighbors[rnd_id];
        Debug.Log(clonePos.x + "" + clonePos.y);
        // clone lives 2 turns
        board.placeNewPiece(piece.Type, piece.Color, clonePos.x, clonePos.y);
        board.getPiece(clonePos.x, clonePos.y).isClone = true;
        board.getPiece(clonePos.x, clonePos.y).TimeInTurnsLeft = 3;
        return true;
    }

    private bool ApplyBarricadeEffect(Board board, int x, int y) {
        ChessPiece piece = board.getPiece(x, y);
        if (piece == null) return false;

        board.placeNewPiece(PieceType.Barricade, PieceColor.Neutral, x, y);
        return true;
    }

    private bool ApplyWrapEffect(Board board, int x, int y) {
        ChessPiece piece = board.getPiece(x, y);
        if (piece == null) return false;

        // Wrap to the opposite side of the board
        // int newX = (x + 4) % 8; // Wrap around horizontally for a simple example
        // int newY = (y + 4) % 8; // Wrap around vertically
        // if (board.checkPiece(newX, newY) == false) {
        //     board.doShiftPiece(x, y, newX, newY);
        //     return true;
        // }
        return false;
    }

    private bool ApplyWindEffect(Board board, WholeFuckingGame game) {
        // Example: Move all pieces in a direction (e.g., upward)
        for (int x = 0; x < 8; x++) {
            for (int y = 1; y < 8; y++) { // Start from row 1 to avoid boundary issues
                if (board.checkPiece(x, y)) {
                    board.doShiftPiece(x, y, x, y - 1); // Move up
                }
            }
        }
        return true;
    }

    private bool ApplyMagicArrowEffect(Board board, int x, int y) {
        ChessPiece piece = board.getPiece(x, y);
        if (piece == null) return false;
        if (piece.antiMagicLeft > 0) return false;

        if (piece.Type != PieceType.Pawn) return false;
        board.destroyPiece(x, y);
        return true;
    }

    private bool ApplySleepEffect(Board board, int x, int y) {
        ChessPiece piece = board.getPiece(x, y);
        if (piece == null) return false;
        if (piece.antiMagicLeft > 0) return false;

        piece.SleepLeft = 3;
        return true;
    }

    private bool ApplyAntimagicEffect(Board board, int x, int y) {
        ChessPiece piece = board.getPiece(x, y);
        if (piece == null) return false;

        piece.antiMagicLeft = 3;
        return true;
    }

    private bool ApplyDispelEffect(Board board, int x, int y) {
        ChessPiece piece = board.getPiece(x, y);
        if (piece == null) return false;
        if (piece.antiMagicLeft > 0) return false;

        piece.SpeedLevel = 0;
        piece.SleepLeft = 0;
        // does nothing if not clone. Kills clone basically
        piece.TimeInTurnsLeft = 0;

        // piece.RemoveStatusEffect("Sleep");
        // piece.RemoveStatusEffect("Hypnotize");
        // piece.RemoveStatusEffect("Clone");
        // piece.RemoveStatusEffect("Barricade");
        if (piece.Type == PieceType.Barricade) {
            board.destroyPiece(x, y);
            return true;
        }
        return false;
    }

    private bool ApplyResurrectEffect(Board board, WholeFuckingGame game, int x, int y) {
        // Check for a grave at position x, y and resurrect a piece if one is there
        // if (game.Graves.Contains((x, y))) {
        //     ChessPiece resurrectedPiece = game.Graves[(x, y)]; // Example resurrection logic
        //     board.setPiece(x, y, resurrectedPiece);
        //     game.Graves.Remove((x, y)); // Remove grave after resurrection
        //     return true;
        // }
        return false;
    }

    private bool ApplyArmageddonEffect(Board board, WholeFuckingGame game) {
        // Example: Apply damage to all pieces on the board
        for (int x = 0; x < 8; x++) {
            for (int y = 0; y < 8; y++) {
                ChessPiece piece = board.getPiece(x, y);
                if (piece != null) {
                    if (piece.Type == PieceType.Pawn) {
                        if (piece.antiMagicLeft == 0)
                            board.destroyPiece(x, y);
                    }
                }
            }
        }
        return true;
    }

    // Helper function to find empty neighboring positions around a piece
    private List<ivec2> GetEmptyNeighbors(Board board, int x, int y) {
        List<ivec2> emptyNeighbors = new List<ivec2>();
        for (int dx = -1; dx <= 1; dx++) {
            for (int dy = -1; dy <= 1; dy++) {
                if (dx != 0 && dy != 0) continue; // no into self-cell-clone
                if (Math.Abs(dx) == 1 && Math.Abs(dy) == 1) continue;

                int nx = x + dx;
                int ny = y + dy;
                if (nx >= 0 && nx < 8 && ny >= 0 && ny < 8 && board.getPiece(nx, ny) == null) {
                    emptyNeighbors.Add(new ivec2(nx, ny));
                }
            }
        }
        return emptyNeighbors;
    }
}
