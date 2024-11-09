using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


// megaclass that does everything
// sometimes split into subclasses, sometimes not
// there is no complexity difference for me, and C# compiler is trash so why not
public class WholeFuckingGame : MonoBehaviour {
    // blacks
    public GameObject B_Bishop;
    public GameObject B_King;
    public GameObject B_Knight;
    public GameObject B_Pawn;
    public GameObject B_Queen;
    public GameObject B_Rook;
    // whites
    public GameObject W_Bishop;
    public GameObject W_King;
    public GameObject W_Knight;
    public GameObject W_Pawn;
    public GameObject W_Queen;
    public GameObject W_Rook;

    public GameObject C_Push;
    public GameObject C_Haste;
    public GameObject C_Slow;

    Dictionary<(PieceType, PieceColor), GameObject> piecePrefabs;
    Dictionary<CardAbility, GameObject> cardPrefabs;

    // private PieceData[,] board = new PieceData[8, 8]; // 8x8 chess board
    // private Vector3[,] pieceInstanceStoredPositions = new Vector3[8, 8]; // 8x8 chess board
    // private GameObject[,] pieceInstances = new GameObject[8, 8]; // Store instances
    ChessPiece[,] board = new ChessPiece[8, 8];

    [SerializeField]
    public Vector3 boardLeftBottomPos = new Vector3(-34.72f, 0, -29.43f);
    [SerializeField]
    public float singleCellScale = (80.0f * ((142 - 7.0f * 2.0f) / (142.0f))) / 8.0f;

    Vector2Int selectedBoardCell = new Vector2Int(0, 0);
    bool isPieceDragged = false;
    Vector3 draggingMousePos;

    bool isCardDragged = false;
    bool isHandHovered = false;
    Card draggedCard = null;  // ptr of the dragged card

    DeckOfCards deck = new DeckOfCards();
    HandOfCards hand = new HandOfCards();


    private void Awake() {
        // Initialize the dictionary with the appropriate prefab for each piece type and color
        piecePrefabs = new Dictionary<(PieceType, PieceColor), GameObject>
        {
            { (PieceType.Bishop, PieceColor.Black), B_Bishop },
            { (PieceType.King, PieceColor.Black), B_King },
            { (PieceType.Knight, PieceColor.Black), B_Knight },
            { (PieceType.Pawn, PieceColor.Black), B_Pawn },
            { (PieceType.Queen, PieceColor.Black), B_Queen },
            { (PieceType.Rook, PieceColor.Black), B_Rook },

            { (PieceType.Bishop, PieceColor.White), W_Bishop },
            { (PieceType.King, PieceColor.White), W_King },
            { (PieceType.Knight, PieceColor.White), W_Knight },
            { (PieceType.Pawn, PieceColor.White), W_Pawn },
            { (PieceType.Queen, PieceColor.White), W_Queen },
            { (PieceType.Rook, PieceColor.White), W_Rook }
        };

        cardPrefabs = new Dictionary<CardAbility, GameObject>
        {
            {CardAbility.Shift, C_Push},
            {CardAbility.Haste, C_Haste},
            {CardAbility.Slow, C_Slow},

            {CardAbility.Clone, C_Slow},
            {CardAbility.Barricade, C_Slow},
            {CardAbility.Wrap, C_Slow},
            {CardAbility.Wind, C_Slow},
            {CardAbility.ExtraMove, C_Slow},
            {CardAbility.ExtendedCastling, C_Slow},
            {CardAbility.RandomEffect, C_Slow},
            {CardAbility.Sleep, C_Slow},
            {CardAbility.Heal, C_Slow},
            {CardAbility.Resurrect, C_Slow},
        };
    }

    void Start() {
        InitializeBoard();
        deck.InitializeDeck(cardPrefabs, new Vector3(0, +2.5f, 0));
        hand.AddCard(deck.DrawCard());
        hand.AddCard(deck.DrawCard());
        hand.AddCard(deck.DrawCard());
        hand.OnMouseEnter();
    }

    void Update() {
        // Handle input and update board
        HandleInput();
        if (isPieceDragged) {
            UpdateDraggingPosition();
        }
        if (isPieceDragged) {
            if (board[selectedBoardCell.x, selectedBoardCell.y] != null) {
                board[selectedBoardCell.x, selectedBoardCell.y].
                    SetVisualPosition(draggingMousePos);
            }
        }
        hand.wholeHandHovered = isHandHovered;
        hand.UpdateHandDisplay();
    }

    void InitializeBoard() {
        // Place pawns
        for (int i = 0; i < 8; i++) {
            PlacePiece(PieceType.Pawn, PieceColor.White, i, 1);
            PlacePiece(PieceType.Pawn, PieceColor.Black, i, 6);
        }

        // Place white pieces
        PlacePiece(PieceType.Rook, PieceColor.White, 0, 0);
        PlacePiece(PieceType.Knight, PieceColor.White, 1, 0);
        PlacePiece(PieceType.Bishop, PieceColor.White, 2, 0);
        PlacePiece(PieceType.Queen, PieceColor.White, 3, 0);
        PlacePiece(PieceType.King, PieceColor.White, 4, 0);
        PlacePiece(PieceType.Bishop, PieceColor.White, 5, 0);
        PlacePiece(PieceType.Knight, PieceColor.White, 6, 0);
        PlacePiece(PieceType.Rook, PieceColor.White, 7, 0);

        // Place black pieces
        PlacePiece(PieceType.Rook, PieceColor.Black, 0, 7);
        PlacePiece(PieceType.Knight, PieceColor.Black, 1, 7);
        PlacePiece(PieceType.Bishop, PieceColor.Black, 2, 7);
        PlacePiece(PieceType.Queen, PieceColor.Black, 3, 7);
        PlacePiece(PieceType.King, PieceColor.Black, 4, 7);
        PlacePiece(PieceType.Bishop, PieceColor.Black, 5, 7);
        PlacePiece(PieceType.Knight, PieceColor.Black, 6, 7);
        PlacePiece(PieceType.Rook, PieceColor.Black, 7, 7);
    }

    Vector3 getPos(int x, int y) {
        float x_offset_in_cells = ((float)(x) - 3.5f);
        float y_offset_in_cells = ((float)(y) - 3.5f);

        Vector3 vector3 = new Vector3(
            x_offset_in_cells * singleCellScale,
            2.5f,
            y_offset_in_cells * singleCellScale);
        return vector3;
    }
    Quaternion getRot() {
        var rot = Quaternion.LookRotation(new Vector3(0f, -1f, 0f));
        return rot;
    }

    void PlacePiece(PieceType type, PieceColor color, int x, int y) {
        board[x, y] = makeNewPiece(type, color, x, y);
    }

    void HandleInput() {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        LayerMask layerMask = LayerMask.GetMask("Cards");


        // WHY cards somewhat separate? They are not grid-snapped
        // i would also write collideres myself for it if it was an option
        isHandHovered = false;
        if (Physics.Raycast(ray.origin, new Vector3 (0, -1, 0), out hit, Mathf.Infinity, layerMask)) {
           isHandHovered = true;
            Debug.Log("Did Hit a card");
        } 
        
        // else {
        //     Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
        //     Debug.Log("Did not Hit");
        // }
        // Hover detection: update hover state continuously
        // isHandHovered = false;
        // if (Physics.Raycast(ray, out hit)) {
        //     Debug.Log("" + hit.collider.tag);
        // }
        // if (hit.collider.tag =="Card")
        // if (hit.collider.CompareTag("Card"))

        // isHandHovered = true;
        // Debug.Log("Card hovered");
        // }


        // RaycastHit2D hitInfo = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(mousePosition), Vector2.zero);

        // if(hitInfo)
        // {
        //     Debug.Log( hitInfo.rigidbody.gameObject.name );
        // }

        // Handle left mouse button down (LMB click)
        if (Input.GetMouseButtonDown(0)) {
            if (Physics.Raycast(ray, out hit)) {
                // Check if the clicked object is a card
                if (hit.collider.CompareTag("Card")) {
                    draggedCard = hit.collider.GetComponent<Card>();
                    if (draggedCard != null) {
                        isCardDragged = true;
                        Debug.Log("Card selected for drag");
                    }
                } else {
                    // Handle board cell clicks
                    Vector3Int clickedBoardCell = ScreenToBoardPosition(hit.point);
                    bool validTargetCell = IsPosInBoardBounds(new Vector2Int(clickedBoardCell.x, clickedBoardCell.z)) &&
                                        board[clickedBoardCell.x, clickedBoardCell.z] != null;

                    // Enable dragging if a valid cell is clicked
                    if (validTargetCell) {
                        selectedBoardCell = new Vector2Int(clickedBoardCell.x, clickedBoardCell.z);
                        isPieceDragged = true;
                    }
                }
            }
        }

        // Handle left mouse button release (LMB up)
        if (Input.GetMouseButtonUp(0)) {
            if (isPieceDragged) {
                if (Physics.Raycast(ray, out hit)) {
                    Vector3Int clickedBoardCell = ScreenToBoardPosition(hit.point);
                    bool validTargetCell = IsPosInBoardBounds(new Vector2Int(clickedBoardCell.x, clickedBoardCell.z));

                    if (validTargetCell) {
                        Debug.Log($"Valid move to {clickedBoardCell}");
                        bool moved = TryMovePiece(selectedBoardCell, new Vector2Int(clickedBoardCell.x, clickedBoardCell.z));
                        if (moved) {
                            board[clickedBoardCell.x, clickedBoardCell.z].SetVisualPosition(getPos(clickedBoardCell.x, clickedBoardCell.z));
                        }
                    }
                }

                // Reset visual position
                if (board[selectedBoardCell.x, selectedBoardCell.y] != null) {
                    board[selectedBoardCell.x, selectedBoardCell.y].SetVisualPosition(getPos(selectedBoardCell.x, selectedBoardCell.y));
                }

                isPieceDragged = false;
            }
        }
    }


    void UpdateDraggingPosition() {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            draggingMousePos = hit.point;
        }
    }

    Vector3Int ScreenToBoardPosition(Vector3 screenPosition) {
        Debug.Log($"BoardPosition:" + screenPosition.x + " " + screenPosition.y + " " + screenPosition.z);
        int x = Mathf.FloorToInt((screenPosition.x - boardLeftBottomPos.x) / singleCellScale);
        int z = Mathf.FloorToInt((screenPosition.z - boardLeftBottomPos.z) / singleCellScale);
        return new Vector3Int(x, 0, z);
    }

    bool TryMovePiece(Vector2Int from, Vector2Int to) {
        if (!IsPosInBoardBounds(from) || !IsPosInBoardBounds(to)) return false;
        if (from == to) return false;

        ChessPiece piece = board[from.x, from.y];
        if (piece == null) return false;

        if (ValidateMove(piece.Type, from, to)) {
            // Capture the piece at the destination if present
            if (board[to.x, to.y] != null) {
                board[to.x, to.y].Destroy();
            }

            // Move the piece in the board array
            board[to.x, to.y] = piece;
            board[from.x, from.y] = null;

            // Update the position of the piece
            piece.SetVisualPosition(getPos(to.x, to.y));
            return true;
        }
        return false;
    }

    ChessPiece makeNewPiece(PieceType type, PieceColor color, int x, int y) {
        ChessPiece piece = new ChessPiece(
            type,
            color,
            getPrefab(type, color),
            getPos(x, y));
        return piece;
    }

    GameObject getPrefab(PieceType type, PieceColor color) {
        if (piecePrefabs.TryGetValue((type, color), out GameObject prefab)) {
            return prefab;
        } else {
            Debug.LogError($"Prefab not found for piece type: {type}, color: {color}");
            return W_King;
        }
    }

    // void CapturePiece(Vector2Int position) {
    //     // "Kill" the piece by removing it from the board
    //     board[position.x, position.y] = new ChessPiece(PieceType.None, PieceColor.None, getPos());
    // }

    bool ValidateMove(PieceType type, Vector2Int from, Vector2Int to) {
        // return true;

        // // Fuck rtti
        switch (type) {
            case PieceType.Pawn:
                return ChessPieceMovement.PawnMovement.CheckMove(from, to);
            case PieceType.King:
                return ChessPieceMovement.KingMovement.CheckMove(from, to);
            case PieceType.Bishop:
                return ChessPieceMovement.BishopMovement.CheckMove(from, to);
            case PieceType.Rook:
                return ChessPieceMovement.RookMovement.CheckMove(from, to);
            case PieceType.Knight:
                return ChessPieceMovement.KnightMovement.CheckMove(from, to);
            case PieceType.Queen:
                return ChessPieceMovement.QueenMovement.CheckMove(from, to);
        }

        return true;
    }

    bool IsPathClear(Vector2Int from, Vector2Int to) {
        int dx = (int)Mathf.Sign(to.x - from.x);
        int dy = (int)Mathf.Sign(to.y - from.y);

        int x = from.x + dx;
        int y = from.y + dy;

        while (x != to.x || y != to.y) {
            if (board[x, y].Type != PieceType.None) return false;
            x += dx;
            y += dy;
        }
        return true;
    }

    bool IsPosInBoardBounds(Vector2Int pos) {
        return pos.x >= 0 && pos.x < 8 && pos.y >= 0 && pos.y < 8;
    }

    void ApplyCardEffect(Card card, Vector3Int targetCell) {
        // Implement card effect logic, e.g., move pieces, apply abilities, etc.
        Debug.Log($"Applying {card.Ability} to cell {targetCell}");
        card.Destroy();  // Remove the card after use, if necessary
    }

    bool IsInCheck(PieceColor color) {
        // Logic to determine if the king of a given color is in check
        return false; // Placeholder
    }

    bool IsCheckmate(PieceColor color) {
        // Logic to determine if the king of a given color is in checkmate
        return false; // Placeholder
    }
}
