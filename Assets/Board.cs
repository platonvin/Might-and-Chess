using System.Collections.Generic;
using UnityEngine;

public class Board {
    private ChessPiece[,] board;
    private Vector3 boardLeftBottomPos;
    private float singleCellScale;
    private Dictionary<(PieceType, PieceColor), GameObject> piecePrefabs;

    // highlight possible moves
    public GameObject A_SelectorPrefab;
    private List<GameObject> activeSelectors = new List<GameObject>();

    public Board(Vector3 boardLeftBottomPos, float singleCellScale, Dictionary<(PieceType, PieceColor), GameObject> piecePrefabs, GameObject A_Selector) {
        this.board = new ChessPiece[8, 8];
        this.boardLeftBottomPos = boardLeftBottomPos;
        this.singleCellScale = singleCellScale;
        this.piecePrefabs = piecePrefabs;
        this.A_SelectorPrefab = A_Selector;
        InitializeBoard();
    }

    public void InitializeBoard() {
        // Place pawns
        for (int i = 0; i < 8; i++) {
            PlacePiece(PieceType.Pawn, PieceColor.White, i, 1);
            PlacePiece(PieceType.Pawn, PieceColor.Black, i, 6);
        }

        // Place remaining white and black pieces
        PlacePiece(PieceType.Rook, PieceColor.White, 0, 0);
        PlacePiece(PieceType.Knight, PieceColor.White, 1, 0);
        PlacePiece(PieceType.Bishop, PieceColor.White, 2, 0);
        PlacePiece(PieceType.Queen, PieceColor.White, 3, 0);
        PlacePiece(PieceType.King, PieceColor.White, 4, 0);
        PlacePiece(PieceType.Bishop, PieceColor.White, 5, 0);
        PlacePiece(PieceType.Knight, PieceColor.White, 6, 0);
        PlacePiece(PieceType.Rook, PieceColor.White, 7, 0);

        PlacePiece(PieceType.Rook, PieceColor.Black, 0, 7);
        PlacePiece(PieceType.Knight, PieceColor.Black, 1, 7);
        PlacePiece(PieceType.Bishop, PieceColor.Black, 2, 7);
        PlacePiece(PieceType.Queen, PieceColor.Black, 3, 7);
        PlacePiece(PieceType.King, PieceColor.Black, 4, 7);
        PlacePiece(PieceType.Bishop, PieceColor.Black, 5, 7);
        PlacePiece(PieceType.Knight, PieceColor.Black, 6, 7);
        PlacePiece(PieceType.Rook, PieceColor.Black, 7, 7);
    }

    public bool IsPosInBoardBounds(Vector2Int pos) {
        return pos.x >= 0 && pos.x < 8 && pos.y >= 0 && pos.y < 8;
    }

    public bool DoShiftPiece(Vector2Int from, Vector2Int to) {
        if (!IsPosInBoardBounds(from) || !IsPosInBoardBounds(to)) return false;
        if (from == to) return false;

        // only move piece to empty
        ChessPiece piece = board[from.x, from.y];
        if (piece == null) return false;
        if (board[to.x, to.y] != null) return false;


        board[to.x, to.y] = piece;
        board[from.x, from.y] = null;
        piece.SetVisualPosition(GetCellPosition(to.x, to.y));
        return true;
    }

    public bool DoShiftPiece(int from_x, int from_y, int to_x, int to_y) {
        return DoShiftPiece(new Vector2Int(from_x, from_y), new Vector2Int(to_x, to_y));
    }

    public bool TryMovePiece(Vector2Int from, Vector2Int to) {
        if (!IsPosInBoardBounds(from) || !IsPosInBoardBounds(to)) return false;
        if (from == to) return false;

        ChessPiece piece = board[from.x, from.y];
        if (piece == null) return false;

        if (ValidateMove(piece.Type, piece.Speed, from, to)) {
            if (board[to.x, to.y] != null) {
                board[to.x, to.y].Destroy();
            }

            board[to.x, to.y] = piece;
            board[from.x, from.y] = null;
            piece.SetVisualPosition(GetCellPosition(to.x, to.y));
            return true;
        }
        return false;
    }

    public Vector3 GetCellPosition(int x, int y) {
        float x_offset_in_cells = x - 3.5f;
        float y_offset_in_cells = y - 3.5f;
        return new Vector3(x_offset_in_cells * singleCellScale, 2.5f, y_offset_in_cells * singleCellScale);
    }

    private void PlacePiece(PieceType type, PieceColor color, int x, int y) {
        board[x, y] = MakeNewPiece(type, color, x, y);
    }

    private ChessPiece MakeNewPiece(PieceType type, PieceColor color, int x, int y) {
        ChessPiece piece = new ChessPiece(
            type,
            color,
            piecePrefabs[(type, color)],
            GetCellPosition(x, y)
        );
        return piece;
    }

    public void SetChessPiece(int x, int y, ChessPiece piece) {
        board[x, y] = piece;
    }
    public void SetChessPiece(Vector2Int xy, ChessPiece piece) {
        board[xy.x, xy.y] = piece;
    }

    public void RemoveChessPiece(int x, int y) {
        board[x, y] = null;
    }
    public void RemoveChessPiece(Vector2Int xy) {
        board[xy.x, xy.y] = null;
    }

    public ChessPiece GetChessPiece(int x, int y) {
        return board[x, y];
    }
    public ChessPiece GetChessPiece(Vector2Int xy) {
        return board[xy.x, xy.y];
    }

    bool ValidateMove(PieceType type, int speed, Vector2Int from, Vector2Int to) {
        // return true;
        // Fuck rtti
        switch (type) {
            case PieceType.Pawn:
                return ChessPieceMovement.PawnMovement.CheckMove(from, to, speed);
            case PieceType.King:
                return ChessPieceMovement.KingMovement.CheckMove(from, to, speed);
            case PieceType.Bishop:
                return ChessPieceMovement.BishopMovement.CheckMove(from, to, speed);
            case PieceType.Rook:
                return ChessPieceMovement.RookMovement.CheckMove(from, to, speed);
            case PieceType.Knight:
                return ChessPieceMovement.KnightMovement.CheckMove(from, to, speed);
            case PieceType.Queen:
                return ChessPieceMovement.QueenMovement.CheckMove(from, to, speed);
        }

        return true;
    }

    public void ShowMoveSelectors(Vector2Int selectedPosition) {
        // Clear any previously instantiated selectors
        ClearSelectors();

        // Get the piece at the selected position
        ChessPiece selectedPiece = GetChessPiece(selectedPosition);
        if (selectedPiece == null) return; // No piece selected

        // Iterate through all cells on the board
        for (int x = 0; x < 8; x++) {
            for (int y = 0; y < 8; y++) {
                Vector2Int targetPosition = new Vector2Int(x, y);

                // Check if moving to this position is valid for the selected piece
                if (ValidateMove(selectedPiece.Type, selectedPiece.Speed, selectedPosition, targetPosition)) {
                    // Instantiate the selector and position it at the target cell
                    Vector3 cellPosition = GetCellPosition(x, y);
                    GameObject selector = GameObject.Instantiate(A_SelectorPrefab, cellPosition, Quaternion.LookRotation(new Vector3(0f, -1f, 0f)));

                    // Add the selector to the active selectors list to manage later
                    activeSelectors.Add(selector);
                }
            }
        }
    }
    public void ClearSelectors() {
        // Destroy all active selector instances
        foreach (GameObject selector in activeSelectors) {
            GameObject.Destroy(selector);
        }
        activeSelectors.Clear();
    }

}
