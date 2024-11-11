using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor.Experimental.GraphView;
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

    public void Turn(){
        for (int x = 0; x < 8; x++){
        for (int y = 0; y < 8; y++){
            ChessPiece piece = board[x,y];
            if(piece == null) continue;

            piece.SleepLeft -= 1;
            piece.antiMagicLeft -= 1;
            piece.TimeInTurnsLeft -= 1;
            piece.Speedleft -= 1;

            piece.SleepLeft = Math.Clamp(piece.SleepLeft, 0, 10);
            piece.antiMagicLeft = Math.Clamp(piece.antiMagicLeft, 0, 10);
            piece.TimeInTurnsLeft = Math.Clamp(piece.TimeInTurnsLeft, 0, 10);
            piece.Speedleft = Math.Clamp(piece.Speedleft, 0, 10);

            // if()
            // if(piece.SleepLeft == 0) {}
            // if(piece.antiMagicLeft == 0) {}
            if(piece.TimeInTurnsLeft == 0) {
                if(piece.isClone) {
                    destroyPiece(x,y);
                }
            }
            if(piece.Speedleft == 0) {piece.SpeedLevel = 0;}
        }}
    }

    public void InitializeBoard() {
        // Place pawns
        for (int i = 0; i < 8; i++) {
            placeNewPiece(PieceType.Pawn, PieceColor.White, i, 1);
            placeNewPiece(PieceType.Pawn, PieceColor.Black, i, 6);
        }

        // Place remaining white and black pieces
        placeNewPiece(PieceType.Rook, PieceColor.White, 0, 0);
        placeNewPiece(PieceType.Knight, PieceColor.White, 1, 0);
        placeNewPiece(PieceType.Bishop, PieceColor.White, 2, 0);
        placeNewPiece(PieceType.Queen, PieceColor.White, 3, 0);
        placeNewPiece(PieceType.King, PieceColor.White, 4, 0);
        placeNewPiece(PieceType.Bishop, PieceColor.White, 5, 0);
        placeNewPiece(PieceType.Knight, PieceColor.White, 6, 0);
        placeNewPiece(PieceType.Rook, PieceColor.White, 7, 0);

        placeNewPiece(PieceType.Rook, PieceColor.Black, 0, 7);
        placeNewPiece(PieceType.Knight, PieceColor.Black, 1, 7);
        placeNewPiece(PieceType.Bishop, PieceColor.Black, 2, 7);
        placeNewPiece(PieceType.Queen, PieceColor.Black, 3, 7);
        placeNewPiece(PieceType.King, PieceColor.Black, 4, 7);
        placeNewPiece(PieceType.Bishop, PieceColor.Black, 5, 7);
        placeNewPiece(PieceType.Knight, PieceColor.Black, 6, 7);
        placeNewPiece(PieceType.Rook, PieceColor.Black, 7, 7);
    }

    public bool IsPosInBoardBounds(ivec2 pos) {
        return pos.x >= 0 && pos.x < 8 && pos.y >= 0 && pos.y < 8;
    }
    public bool IsPosInBoardBounds(int _x, int _y) {
        return _x >= 0 && _x < 8 && _y >= 0 && _y < 8;
    }

    public bool DoShiftPiece(ivec2 from, ivec2 to) {
        if (!IsPosInBoardBounds(from) || !IsPosInBoardBounds(to)) return false;
        if (from == to) return false;

        // only move piece to empty
        ChessPiece piece = board[from.x, from.y];
        if (piece == null) return false;
        if (board[to.x, to.y] != null) return false;


        board[to.x, to.y] = piece;
        board[from.x, from.y] = null;
        piece.SetVisualPosition(getCellPosition(to.x, to.y));
        return true;
    }

    public bool doShiftPiece(int from_x, int from_y, int to_x, int to_y) {
        return DoShiftPiece(new ivec2(from_x, from_y), new ivec2(to_x, to_y));
    }

    public bool tryMovePiece(ivec2 from, ivec2 to) {
        if (!IsPosInBoardBounds(from) || !IsPosInBoardBounds(to)) return false;
        if (from == to) return false;

        ChessPiece piece = board[from.x, from.y];
        if (piece == null) return false;

        if (ValidateMove(piece.Type, piece.SpeedLevel, from, to)) {
            if (board[to.x, to.y] != null) {
                board[to.x, to.y].Destroy();
            }

            board[to.x, to.y] = piece;
            board[from.x, from.y] = null;
            piece.SetVisualPosition(getCellPosition(to.x, to.y));
            return true;
        }
        return false;
    }

    public bool checkPiece(int x, int y) {
        return getPiece(x, y) != null;
    }
    public bool checkPiece(ivec2 pos) {
        return getPiece(pos.x, pos.y) != null;
    }

    public Vector3 getCellPosition(int x, int y) {
        float x_offset_in_cells = x - 3.5f;
        float y_offset_in_cells = y - 3.5f;
        return new Vector3(x_offset_in_cells * singleCellScale, 2.5f, y_offset_in_cells * singleCellScale);
    }

    public void placeNewPiece(PieceType type, PieceColor color, int x, int y) {
        board[x, y] = makePiece(type, color, x, y);
    }

    public ChessPiece makePiece(PieceType type, PieceColor color, int x, int y) {
        ChessPiece piece = new ChessPiece(
            type,
            color,
            piecePrefabs[(type, color)],
            getCellPosition(x, y)
        );
        return piece;
    }

    public void setPiece(int x, int y, ChessPiece piece) {
        board[x, y] = piece;
    }
    public void setPiece(ivec2 xy, ChessPiece piece) {
        board[xy.x, xy.y] = piece;
    }

    public void destroyPiece(int x, int y) {
        board[x, y].Destroy();
        board[x, y] = null;
    }
    public void destroyPiece(ivec2 xy) {
        destroyPiece(xy.x, xy.y);
    }

    public ChessPiece getPiece(int x, int y) {
        return board[x, y];
    }
    public ChessPiece getPiece(ivec2 xy) {
        return board[xy.x, xy.y];
    }

    bool ValidateMove(PieceType type, int speed, ivec2 from, ivec2 to) {
        // return true;
        PieceColor notAllowedToEatColor = getPiece(from).Color; 
        // PieceColor allowedToEatColor = (notAllowedToEatColor == PieceColor.Black) ? PieceColor.White : PieceColor.Black;
        if(checkPiece(to)) {
            if(notAllowedToEatColor == getPiece(to).Color) return false;
        }
        // Fuck rtti
        switch (type) {
            case PieceType.Pawn:
                return ChessPieceMovement.PawnMovement.CheckMove(this, from, to, speed);
            case PieceType.King:
                return ChessPieceMovement.KingMovement.CheckMove(this, from, to, speed);
            case PieceType.Bishop:
                return ChessPieceMovement.BishopMovement.CheckMove(this, from, to, speed);
            case PieceType.Rook:
                return ChessPieceMovement.RookMovement.CheckMove(this, from, to, speed);
            case PieceType.Knight:
                return ChessPieceMovement.KnightMovement.CheckMove(this, from, to, speed);
            case PieceType.Queen:
                return ChessPieceMovement.QueenMovement.CheckMove(this, from, to, speed);
        }

        return true;
    }

    public void ShowMoveSelectors(ivec2 selectedPosition) {
        // Clear any previously instantiated selectors
        ClearSelectors();

        // Get the piece at the selected position
        ChessPiece selectedPiece = getPiece(selectedPosition);
        if (selectedPiece == null) return; // No piece selected

        // Iterate through all cells on the board
        for (int x = 0; x < 8; x++) {
            for (int y = 0; y < 8; y++) {
                ivec2 targetPosition = new ivec2(x, y);

                // Check if moving to this position is valid for the selected piece
                if (ValidateMove(selectedPiece.Type, selectedPiece.SpeedLevel, selectedPosition, targetPosition)) {
                    // Instantiate the selector and position it at the target cell
                    Vector3 cellPosition = getCellPosition(x, y);
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
