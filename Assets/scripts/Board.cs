using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


// nullptr chess piece is literally no chess piece
// manages chess pieces on a board and some random pieces of logic completely unrelated with physical chess board if you do not have human common sense 
public class Board {
    private ChessPiece[,] board     = new ChessPiece[8, 8];
    private ChessPiece[,] graveyard = new ChessPiece[8, 8];
    private Vector3 boardLeftBottomPos;
    private float singleCellScale;
    private Dictionary<(PieceType, PieceColor), GameObject> piecePrefabs;
    private Dictionary<CardAbility, GameObject> iconPrefabs;
    private Dictionary<CardAbility, GameObject> animationPrefabs;

    // highlight possible moves
    public GameObject A_SelectorPrefab;
    private List<GameObject> activeSelectors = new List<GameObject>();


    public Board(Vector3 boardLeftBottomPos, float singleCellScale, 
                 Dictionary<(PieceType, PieceColor), GameObject> piecePrefabs, 
                 Dictionary<CardAbility, GameObject> iconPrefabs, 
                 Dictionary<CardAbility, GameObject> animationPrefabs,
                 GameObject A_Selector) {
        this.boardLeftBottomPos = boardLeftBottomPos;
        this.singleCellScale = singleCellScale;
        this.piecePrefabs = piecePrefabs;
        this.iconPrefabs = iconPrefabs;
        this.animationPrefabs = animationPrefabs;
        this.A_SelectorPrefab = A_Selector;
        InitializeBoard();
    }

    public void Turn() {
        for (int x = 0; x < 8; x++) {
            for (int y = 0; y < 8; y++) {
                ChessPiece piece = board[x, y];
                if (piece == null) continue;

                piece.sleepLeft -= 1;
                piece.antiMagicLeft -= 1;
                piece.timeInTurnsLeft -= 1;
                piece.speedleft -= 1;

                piece.sleepLeft = Math.Clamp(piece.sleepLeft, 0, 10);
                piece.antiMagicLeft = Math.Clamp(piece.antiMagicLeft, 0, 10);
                piece.timeInTurnsLeft = Math.Clamp(piece.timeInTurnsLeft, 0, 10);
                piece.speedleft = Math.Clamp(piece.speedleft, 0, 10);
                if (piece.speedleft == 0) { piece.speedLevel = 0; }

                // if()
                piece.updateEffectsDisplay();
                // if(piece.SleepLeft == 0) {}
                // if(piece.antiMagicLeft == 0) {}
                if (piece.timeInTurnsLeft == 0) {
                    if (piece.isClone) {
                        destroyPiece(x, y);
                    }
                }

            }
        }
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

    public bool doShiftPiece(ivec2 from, ivec2 to) {
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
        return doShiftPiece(new ivec2(from_x, from_y), new ivec2(to_x, to_y));
    }

    public bool tryMovePiece(ivec2 from, ivec2 to) {
        if (!IsPosInBoardBounds(from) || !IsPosInBoardBounds(to)) return false;
        if (from == to) return false;

        ChessPiece piece = board[from.x, from.y];
        if (piece == null) return false;
        if (piece.sleepLeft > 0) return false;

        if (ValidateMove(piece.Type, piece.speedLevel, from, to)) {
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
            iconPrefabs,
            // animationPrefabs,
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

    public void killPiece(int x, int y) {
        if (board[x, y] != null) {
            if(graveyard[x, y] != null) graveyard[x, y].Destroy(); // destroy previous one
            graveyard[x, y] = board[x, y]; // Move the new piece to the graveyard
            graveyard[x, y].setDeadVisually();
            // graveyard[x, y]
            board[x, y] = null; // Clear the board cell
        }
    }
    public void destroyPiece(int x, int y) {
        if (board[x, y] != null) {
            board[x, y].Destroy();
            board[x, y] = null; // Clear the board cell
        }
    }
    public void killPiece(ivec2 xy) {
        killPiece(xy.x, xy.y);
    }

    public ChessPiece getPiece(int x, int y) {
        return board[x, y];
    }
    public ChessPiece getPiece(ivec2 xy) {
        return board[xy.x, xy.y];
    }

    public bool hasGraveAt(int x, int y) {
        return graveyard[x, y] != null;
    }

    // Resurrect a piece from the graveyard and place it on the board at the specified coordinates
    public bool resurrectPieceAt(int x, int y) {
        if (!hasGraveAt(x, y) || board[x, y] != null) return false; // Check if a piece can be resurrected

        board[x, y] = graveyard[x, y]; // Move the piece back to the board
        graveyard[x, y] = null; // Clear the graveyard cell
        board[x, y].SetVisualPosition(getCellPosition(x, y)); // Update the visual position
        board[x, y].setAliveVisually();
        return true;
    }

    bool ValidateMove(PieceType type, int speed, ivec2 from, ivec2 to) {
        // return true;
        PieceColor notAllowedToEatColor = getPiece(from).Color;
        // PieceColor allowedToEatColor = (notAllowedToEatColor == PieceColor.Black) ? PieceColor.White : PieceColor.Black;
        if (checkPiece(to)) {
            if (notAllowedToEatColor == getPiece(to).Color) return false;
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
                if (ValidateMove(selectedPiece.Type, selectedPiece.speedLevel, selectedPosition, targetPosition)) {
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

    public void playAnimation(CardAbility ability, Vector3 position) {
        // Check if an animation prefab exists for the given ability
        if (animationPrefabs.TryGetValue(ability, out GameObject animationPrefab)) {
            Debug.Log("playing anim for" + ability);
            // Instantiate the animation at the specified position
            GameObject animationInstance = GameObject.Instantiate(animationPrefab, position, Quaternion.Euler(90,0,0));

            // Play the animation (assuming it has an Animator or similar component)
            Animator animator = animationInstance.GetComponent<Animator>();
            // if (animator != null) {
                animator.Play(0); // Play the default animation clip
            // }

            // destroy the animation object after it finishes playing
            float animationDuration = animator.GetCurrentAnimatorStateInfo(0).length;
            GameObject.Destroy(animationInstance, animationDuration);
        } else {
            Debug.LogWarning($"No animation prefab found for ability: {ability}");
        }
    }
}
