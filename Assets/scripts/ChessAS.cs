using System.Collections.Generic;
using UnityEngine;

// artificial stupidity
public class ChessAS {
    private Board board;

    public ChessAS(Board board) {
        this.board = board;
    }

    public (ivec2 from, ivec2 to) FindBestMove(PieceColor color) {
        int bestScore = int.MinValue;
        ivec2 bestFrom = new ivec2();
        ivec2 bestTo = new ivec2();

        // Iterate over all pieces on the board
        for (int x = 0; x < 8; x++) {
            for (int y = 0; y < 8; y++) {
                ivec2 from = new ivec2(x, y);
                if (!board.checkPiece(from)) continue;
                if (board.getPiece(from).color != color) continue; // only move own pieces

                // Find all possible moves for this piece
                List<ivec2> possibleMoves = GetPossibleMoves(from);

                foreach (ivec2 to in possibleMoves) {
                    // Simulate the move
                    bool moveAllowed = board.validateMovePiece(from, to);


                    if (moveAllowed) {
                        // Evaluate the board state after move
                        board.forAiApplyMove(from, to, out ChessPiece capturedPiece);
                        int score = EvaluateBoard(color);
                        board.forAiUndoMove(from, to, capturedPiece);

                        // Update the best move if this score is better
                        if (score > bestScore) {
                            bestScore = score;
                            bestFrom = from;
                            bestTo = to;
                        }
                    }
                }
            }
        }

        return (bestFrom, bestTo);
    }

    private List<ivec2> GetPossibleMoves(ivec2 from) {
        List<ivec2> possibleMoves = new List<ivec2>();

        for (int x = 0; x < 8; x++) {
            for (int y = 0; y < 8; y++) {
                ivec2 to = new ivec2(x, y);
                if (board.validateMovePiece(from, to)) {
                    possibleMoves.Add(to);
                }
            }
        }

        return possibleMoves;
    }

    private int EvaluateBoard(PieceColor color) {
        int score = 0;

        for (int x = 0; x < 8; x++) {
            for (int y = 0; y < 8; y++) {
                ivec2 pos = new ivec2(x, y);
                if (!board.checkPiece(pos)) continue;

                // Get piece value and position value
                ChessPiece piece = board.getPiece(pos);
                if (piece.color == color) {
                    score += GetPieceValue(piece);
                    score += GetPositionValue(piece, pos);
                } else {
                    score -= GetPieceValue(piece);
                    score -= GetPositionValue(piece, pos);
                }
            }
        }

        return score;
    }

    private int GetPieceValue(ChessPiece piece) {
        // Example piece values
        switch (piece.type) {
            case PieceType.Pawn: return 150;
            case PieceType.Knight: return 370;
            case PieceType.Bishop: return 375;
            case PieceType.Rook: return 550;
            case PieceType.Queen: return 1024;
            case PieceType.King: return 32767; // High value for king
            default: return 0;
        }
    }

    private static readonly short[] PawnTable = new short[] {
        0, 0, 0, 0, 0, 0, 0, 0,
        50, 50, 50, 50, 50, 50, 50, 50,
        10, 10, 20, 30, 30, 20, 10, 10,
        5, 5, 10, 27, 27, 10, 5, 5,
        0, 0, 0, 25, 25, 0, 0, 0,
        5, -5,-10, 0, 0,-10, -5, 5,
        5, 10, 10,-25,-25, 10, 10, 5,
        0, 0, 0, 0, 0, 0, 0, 0
    };
    private static readonly short[] KnightTable = new short[] {
        -50,-40,-30,-30,-30,-30,-40,-50,
        -40,-20, 0, 0, 0, 0,-20,-40,
        -30, 0, 10, 15, 15, 10, 0,-30,
        -30, 5, 15, 20, 20, 15, 5,-30,
        -30, 0, 15, 20, 20, 15, 0,-30,
        -30, 5, 10, 15, 15, 10, 5,-30,
        -40,-20, 0, 5, 5, 0,-20,-40,
        -50,-40,-20,-30,-30,-20,-40,-50,
    };
    private static readonly short[] BishopTable = new short[] {
        -20,-10,-10,-10,-10,-10,-10,-20,
        -10, 0, 0, 0, 0, 0, 0,-10,
        -10, 0, 5, 10, 10, 5, 0,-10,
        -10, 5, 5, 10, 10, 5, 5,-10,
        -10, 0, 10, 10, 10, 10, 0,-10,
        -10, 10, 10, 10, 10, 10, 10,-10,
        -10, 5, 0, 0, 0, 0, 5,-10,
        -20,-10,-40,-10,-10,-40,-10,-20,
    };
    private static readonly short[] KingTable = new short[] {
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -30, -40, -40, -50, -50, -40, -40, -30,
        -20, -30, -30, -40, -40, -30, -30, -20,
        -10, -20, -20, -20, -20, -20, -20, -10,
        20, 20, 0, 0, 0, 0, 20, 20,
        20, 30, 10, 0, 0, 10, 30, 20
    };
    private static readonly short[] KingTableEndGame = new short[] {
        -50,-40,-30,-20,-20,-30,-40,-50,
        -30,-20,-10, 0, 0,-10,-20,-30,
        -30,-10, 20, 30, 30, 20,-10,-30,
        -30,-10, 30, 40, 40, 30,-10,-30,
        -30,-10, 30, 40, 40, 30,-10,-30,
        -30,-10, 20, 30, 30, 20,-10,-30,
        -30,-30, 0, 0, 0, 0,-30,-30,
        -50,-30,-30,-30,-30,-30,-30,-50
    };
    private static readonly short[] RookTable = new short[] {
        0,  0,  0,  0,  0,  0,  0,  0,
        5, 10, 10, 10, 10, 10, 10,  5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        0,  0,  0,  5,  5,  0,  0,  0
    };
    private static readonly short[] QueenTable = new short[] {
        -20,-10,-10, -5, -5,-10,-10,-20,
        -10,  0,  0,  0,  0,  0,  0,-10,
        -10,  0,  5,  5,  5,  5,  0,-10,
        -5,  0,  5,  5,  5,  5,  0, -5,
        0,  0,  5,  5,  5,  5,  0, -5,
        -10,  5,  5,  5,  5,  5,  0,-10,
        -10,  0,  5,  0,  0,  0,  0,-10,
        -20,-10,-10, -5, -5,-10,-10,-20
    };
    int indexPox(ivec2 pos){
        int _x =     pos.x;
        int _y = 7 - pos.y;
        return (_x + _y*8);
    }
    private int GetPositionValue(ChessPiece piece, ivec2 position) {
        switch (piece.type) {
            case PieceType.Knight:
                return KnightTable[indexPox(position)];
            case PieceType.Pawn:
                return PawnTable[indexPox(position)];
            case PieceType.Bishop:
                return BishopTable[indexPox(position)];
            case PieceType.Rook:
                return RookTable[indexPox(position)];
            case PieceType.Queen:
                return QueenTable[indexPox(position)];
            case PieceType.King:
                return KingTable[indexPox(position)]; // Use KingTableEndGame for endgame scenarios
            default:
                Debug.LogError("Invalid piece type.");
                return 0;
        }
    }
}