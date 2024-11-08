using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WholeFuckingGame : MonoBehaviour {
    public GameObject B_Bishop;
    public GameObject B_King;
    public GameObject B_Knight;
    public GameObject B_Pawn;
    public GameObject B_Queen;
    public GameObject B_Rook;

    public GameObject W_Bishop;
    public GameObject W_King;
    public GameObject W_Knight;
    public GameObject W_Pawn;
    public GameObject W_Queen;
    public GameObject W_Rook;
    
    private Dictionary<(PieceType, PieceColor), GameObject> piecePrefabs;

    // private PieceData[,] board = new PieceData[8, 8]; // 8x8 chess board
    // private Vector3[,] pieceInstanceStoredPositions = new Vector3[8, 8]; // 8x8 chess board
    // private GameObject[,] pieceInstances = new GameObject[8, 8]; // Store instances
    private ChessPiece[,] board = new ChessPiece[8, 8];

    [SerializeField]
    public Vector3 board_left_bottom = new Vector3(-34.72f, 0, -29.43f);
    [SerializeField]
    public float single_cell_scale = (80.0f * ((142 - 7.0f * 2.0f) / (142.0f))) / 8.0f;
    
    private Vector2Int selectedBoardCell = new Vector2Int(0,0);
    private bool is_piece_dragged = false;
    private Vector3 dragging_pos;
    // private bool is_anything_dragged = false;

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
    }

    void Start() {
        InitializeBoard();
    }

    void Update() {
        // Handle input and update board
        HandleInput();
        if (is_piece_dragged) {
            UpdateDraggingPosition();
        }
        if (is_piece_dragged) {
            if (board[selectedBoardCell.x, selectedBoardCell.y] != null) {
                board[selectedBoardCell.x, selectedBoardCell.y].
                    SetVisualPosition(dragging_pos);
            }
        }
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
            x_offset_in_cells * single_cell_scale,
            2.5f,
            y_offset_in_cells * single_cell_scale);
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
        // LBM click
        if (Input.GetMouseButtonDown(0)) {
            // If already in hand
            if (is_piece_dragged){
                // nothing
            } else {
                Vector3 mousePosition = Input.mousePosition;
                Ray ray = Camera.main.ScreenPointToRay(mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit)) {
                    Vector3Int clickedBoardCell = ScreenToBoardPosition(hit.point);
                    Debug.Log("down" + clickedBoardCell.x + " " + clickedBoardCell.z);

                    bool valid_target_cell =
                        IsValidPosition(new Vector2Int(clickedBoardCell.x, clickedBoardCell.z)) &&
                        (board[clickedBoardCell.x, clickedBoardCell.z] != null);

                    // so can drag the figure
                    if (valid_target_cell) {
                        Debug.Log("down" + "valid");
                        selectedBoardCell = new Vector2Int(clickedBoardCell.x, clickedBoardCell.z);
                        is_piece_dragged = true;
                    }
                }

            }

        }

        if (Input.GetMouseButtonUp(0)) {
            // LMB up
            if (is_piece_dragged){
                Vector3 mousePosition = Input.mousePosition;
                Ray ray = Camera.main.ScreenPointToRay(mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit)) {
                    Vector3Int clickedBoardCell = ScreenToBoardPosition(hit.point);
                    Debug.Log("up" + clickedBoardCell.x + " " + clickedBoardCell.z);
                    bool valid_target_cell = IsValidPosition(new Vector2Int(clickedBoardCell.x, clickedBoardCell.z));

                    if (valid_target_cell) {
                        Debug.Log("up" + "valid");
                        // Move the piece without validation for now
                        bool moved = TryMovePiece(selectedBoardCell, new Vector2Int(clickedBoardCell.x, clickedBoardCell.z));
                        if (moved) {
                            board[clickedBoardCell.x, clickedBoardCell.z].SetVisualPosition(getPos(clickedBoardCell.x, clickedBoardCell.z));
                        }
                    }
                }

                // reset visual position so 
                // unity is so fucked up, i dont even know. Same in cpp-vk is like 100 lines and 100mb of project files
                if (board[selectedBoardCell.x, selectedBoardCell.y] != null)
                    board[selectedBoardCell.x, selectedBoardCell.y].SetVisualPosition(getPos(selectedBoardCell.x, selectedBoardCell.y));
                
                is_piece_dragged = false;
            }
        }
    }

    void UpdateDraggingPosition() {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            dragging_pos = hit.point;
        }
    }

    Vector3Int ScreenToBoardPosition(Vector3 screenPosition) {
        Debug.Log($"BoardPosition:" + screenPosition.x + " " + screenPosition.y + " " + screenPosition.z);
        int x = Mathf.FloorToInt((screenPosition.x - board_left_bottom.x) / single_cell_scale);
        int z = Mathf.FloorToInt((screenPosition.z - board_left_bottom.z) / single_cell_scale);
        return new Vector3Int(x, 0, z);
    }

    bool TryMovePiece(Vector2Int from, Vector2Int to) {
        if (!IsValidPosition(from) || !IsValidPosition(to)) return false;
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

    bool ValidatePawnMove(Vector2Int from, Vector2Int to, PieceColor color) {
        int direction = (color == PieceColor.White) ? 1 : -1;
        int startRow = (color == PieceColor.White) ? 1 : 6;

        // Regular move forward
        if (from.x == to.x && to.y == from.y + direction && board[to.x, to.y].Type == PieceType.None)
            return true;

        // Double move from start position
        if (from.x == to.x && from.y == startRow && to.y == from.y + 2 * direction && board[to.x, to.y].Type == PieceType.None)
            return true;

        // Capture move
        if (Mathf.Abs(from.x - to.x) == 1 && to.y == from.y + direction && board[to.x, to.y].Color != color && board[to.x, to.y].Type != PieceType.None)
            return true;

        return false;
    }

    bool ValidateRookMove(Vector2Int from, Vector2Int to) {
        if (from.x != to.x && from.y != to.y) return false; // Must be a straight line

        // Check for obstacles in the way
        if (!IsPathClear(from, to)) return false;

        return true;
    }

    bool ValidateKnightMove(Vector2Int from, Vector2Int to) {
        int dx = Mathf.Abs(from.x - to.x);
        int dy = Mathf.Abs(from.y - to.y);
        return (dx == 2 && dy == 1) || (dx == 1 && dy == 2);
    }

    bool ValidateBishopMove(Vector2Int from, Vector2Int to) {
        if (Mathf.Abs(from.x - to.x) != Mathf.Abs(from.y - to.y)) return false; // Must move diagonally

        // Check for obstacles
        if (!IsPathClear(from, to)) return false;

        return true;
    }

    bool ValidateQueenMove(Vector2Int from, Vector2Int to) {
        return ValidateRookMove(from, to) || ValidateBishopMove(from, to);
    }

    bool ValidateKingMove(Vector2Int from, Vector2Int to) {
        return Mathf.Abs(from.x - to.x) <= 1 && Mathf.Abs(from.y - to.y) <= 1;
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

    bool IsValidPosition(Vector2Int pos) {
        return pos.x >= 0 && pos.x < 8 && pos.y >= 0 && pos.y < 8;
    }

    // Other useful functions

    // void PrintBoard() {
    //     for (int y = 7; y >= 0; y--) {
    //         string row = "";
    //         for (int x = 0; x < 8; x++) {
    //             row += $"{board[x, y].Type} ";
    //         }
    //         Debug.Log(row);
    //     }
    // }

    bool IsInCheck(PieceColor color) {
        // Logic to determine if the king of a given color is in check
        return false; // Placeholder
    }

    bool IsCheckmate(PieceColor color) {
        // Logic to determine if the king of a given color is in checkmate
        return false; // Placeholder
    }
}
