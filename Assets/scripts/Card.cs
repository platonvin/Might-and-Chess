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
    ExtraMove, // this.
    ExtendedCastling, // from anywhere i guess
    RandomEffect, // literally
    Blind,
    Dispel, // remove sleep / hypnotize / clone / barricade
    Resurrect, // graves needed?
    AntiMagic, // no cast on him i guess
    Sacrifice, // resurrect better one
    Weakness,
    Hypnotize, // pawn
    LightingBolt, // kill pawn
    Armageddon, // damage all
    Flight, // 1 piece skip allowed
    Tornado, // pulls in 3x3
    ThunderStrike, // like magi arrow but stronger
    Berserk,
// Reneval - new cards?

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
            case CardAbility.LightingBolt: return ApplyMagicArrowEffect(board, x, y);
            case CardAbility.AntiMagic: return ApplyAntimagicEffect(board, x, y);
            case CardAbility.Barricade: return ApplyBarricadeEffect(board, x, y);
            case CardAbility.Weakness: return ApplyWeaknessEffect(board, x, y);
            // case CardAbility.Wrap: //     return ApplyWrapEffect(board, x, y);
            // case CardAbility.Wind: //     return ApplyWindEffect(board, game);
            case CardAbility.Flight: return ApplyFlightEffect(board, x, y);
            case CardAbility.Blind: return ApplySleepEffect(board, x, y);
            case CardAbility.Dispel: return ApplyDispelEffect(board, x, y);
            case CardAbility.Resurrect:  return ApplyResurrectEffect(board,  x, y);
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

        piece.speedLevel = +1;
        piece.speedleft = 3;
        return true;
    }

    private bool ApplySlowEffect(Board board, int x, int y) {
        ChessPiece piece = board.getPiece(x, y);
        if (piece == null) return false;
        if (piece.antiMagicLeft > 0) return false;

        piece.speedLevel = -1;
        piece.speedleft = 3;
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
        board.placeNewPiece(piece.type, piece.color, clonePos.x, clonePos.y);
        // setting up clone piece itself

        // set initial pos to parent of clone
        board.getPiece(clonePos.x, clonePos.y).setVisualPosition(board.getCellPosition(x,y));
        board.getPiece(clonePos.x, clonePos.y).isClone = true;
        board.getPiece(clonePos.x, clonePos.y).hasMovedAtAll = true;
        board.getPiece(clonePos.x, clonePos.y).timeInTurnsLeft = 2; // so attacks ~ones and defends ~ones 
        board.getPiece(clonePos.x, clonePos.y).updateEffectsDisplay();
        return true;
    }

    private bool ApplyBarricadeEffect(Board board, int x, int y) {
        ChessPiece piece = board.getPiece(x, y);
        // only empty allowed
        if (piece != null) return false;

        board.placeNewPiece(PieceType.Barricade, PieceColor.Neutral, x, y);
        return true;
    }

    private bool ApplyWrapEffect(Board board, int x, int y) {
        ChessPiece piece = board.getPiece(x, y);
        if (piece == null) return false;

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

        if (piece.type != PieceType.Pawn) return false;
        board.tryKillPiece(x, y);
        return true;
    }

    private bool ApplySleepEffect(Board board, int x, int y) {
        ChessPiece piece = board.getPiece(x, y);
        if (piece == null) return false;
        if (piece.antiMagicLeft > 0) return false;

        piece.sleepLeft = 3;
        return true;
    }

    private bool ApplyFlightEffect(Board board, int x, int y) {
        ChessPiece piece = board.getPiece(x, y);
        if (piece == null) return false;
        if (piece.antiMagicLeft > 0) return false;

        piece.flightLeft = 3;
        return true;
    }

    private bool ApplyAntimagicEffect(Board board, int x, int y) {
        ChessPiece piece = board.getPiece(x, y);
        if (piece == null) return false;
        if (piece.antiMagicLeft > 0) return false; // lol am prevents am

        piece.antiMagicLeft = 3;
        return true;
    }

    private bool ApplyWeaknessEffect(Board board, int x, int y) {
        ChessPiece piece = board.getPiece(x, y);
        if (piece == null) return false;
        if (piece.antiMagicLeft > 0) return false;

        piece.weaknessLeft = 3;
        return true;
    }

    private bool ApplyDispelEffect(Board board, int x, int y) {
        ChessPiece piece = board.getPiece(x, y);
        if (piece == null) return false;
        if (piece.antiMagicLeft > 0) return false;

        piece.speedLevel = 0;
        piece.sleepLeft = 0;
        // does nothing if not clone. Kills clone basically
        piece.timeInTurnsLeft = 0;
        piece.weaknessLeft = 0;
        piece.flightLeft = 0;
        piece.controlledByOpponentTurnsLeft = 0;

        // also kills barriers
        if (piece.type == PieceType.Barricade) {
            board.tryKillPiece(x, y);
            return true;
        }
        return true;
    }

    private bool ApplyArmageddonEffect(Board board, WholeFuckingGame game) {
        for (int x = 0; x < 8; x++) {
            for (int y = 0; y < 8; y++) {
                ChessPiece piece = board.getPiece(x, y);
                if (piece != null) {
                    if (piece.type == PieceType.Pawn) {
                        if (piece.antiMagicLeft == 0)
                            board.tryKillPiece(x, y);
                    }
                }
            }
        }
        return true;
    }

    private bool ApplyHypnotizeEffect(Board board, int x, int y) {
        ChessPiece piece = board.getPiece(x, y);
        if (piece == null || piece.antiMagicLeft > 0) return false;

        piece.controlledByOpponentTurnsLeft = 3;
        return true;
    }

    private bool ApplyTornadoEffect(Board board, int x, int y) {
        ivec2 center = new ivec2(x, y);

        // Check all cells in a 3x3 area centered at (x, y)
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                // Skip the center cell itself
                if (i == 0 && j == 0) continue;

                ivec2 currentPos = new ivec2(x + i, y + j);
                ivec2 targetPos = center;

                // If there is a piece in the current position and the target position is empty
                if (board.checkPiece(currentPos) && !board.checkPiece(targetPos)) {
                    // Move the piece one step closer to the center
                    board.doShiftPiece(currentPos, targetPos);
                }
            }
        }

        return true;
    }

    private bool ApplySacrificeEffect(Board board, int x, int y) {
        ChessPiece toSacrifice = board.getPiece(x, y);
        if (toSacrifice == null || toSacrifice.antiMagicLeft > 0) return false;

        board.tryKillPiece(x, y);

        // replace target piece with a better one (just replace its type)
        return false;
        // return true;
    }
    private bool ApplyResurrectEffect(Board board, int x, int y) {
        if (!board.hasGraveAt(x, y)) return false; // Assuming there's a system to track graves
        // only empty allowed
        ChessPiece piece = board.getPiece(x, y);
        if (piece != null) return false;

        bool resurrected = board.resurrectPieceAt(x, y); // Resurrects a piece from the graveyard
        return resurrected;
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
