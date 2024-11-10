
using System;
using System.Diagnostics.Tracing;
using UnityEngine;

public enum Rotation {
    NoRotation = 0,
    Rotate90 = 90,
    Rotate180 = 180,
    Rotate270 = 270
}

// so much easier that Vector2Int lol

public class MoveConstraint {
    public ivec2[] shifts;          // Array of direction vectors for movement
    public bool isRepeated;         // Can the movement be repeated?
    public Rotation[] rotations;    // Possible rotations for the movement pattern

    public MoveConstraint(ivec2[] shifts, bool isRepeated, Rotation[] rotations) {
        this.shifts = shifts;
        this.isRepeated = isRepeated;
        this.rotations = rotations;
    }

    // Check if a move from startPos to endPos is valid based on the constraint
    public bool CheckMove(Vector2Int startPos, Vector2Int endPos, int speed = 0) {
        // Calculate the difference between start and end position
        ivec2 moveDifference = new ivec2(endPos.x - startPos.x, endPos.y - startPos.y);

        // Check all possible rotations and shifts
        foreach (var shift in shifts) {
            foreach (var rotation in rotations) {
                ivec2 rotatedShift = shift.Rotate(rotation);
                
                // Apply speed scaling to the shift vector
                ivec2 shift_direction = new ivec2(
                    (rotatedShift.x < 0) ? -1 : ((rotatedShift.x > 0)? +1 : 0), 
                    (rotatedShift.y < 0) ? -1 : ((rotatedShift.y > 0)? +1 : 0)
                );
                ivec2 scaledShift = rotatedShift + shift_direction*speed;
                scaledShift.clamp(new ivec2(0,0), new ivec2(10,10));

                // Handle the zero-case for scaledShift.x and scaledShift.y
                if (scaledShift.x != 0 && scaledShift.y != 0) {
                    if (isRepeated) {
                        if ((moveDifference.x % scaledShift.x == 0) && (moveDifference.y % scaledShift.y == 0)) {
                            return true;
                        }
                    } else {
                        if (moveDifference.x == scaledShift.x && moveDifference.y == scaledShift.y) {
                            return true;
                        }
                    }
                } else {
                    // If either component is zero, check separately to avoid division issues
                    if (scaledShift.x == 0 && moveDifference.x == 0 && (isRepeated || moveDifference.y == scaledShift.y)) {
                        return true;
                    } else if (scaledShift.y == 0 && moveDifference.y == 0 && (isRepeated || moveDifference.x == scaledShift.x)) {
                        return true;
                    }
                }
            }
        }

        // false is also returned for 0-0 shift-speed. Because you do not want to waste your turn on this
        return false; // No valid move found
    }
}

// POSITIVE VALUES ONLY
public class ChessPieceMovement {
    // Pawn: Moves 1 square up (or down with 180-degree rotation), cannot repeat the move
    public static MoveConstraint PawnMovement = new MoveConstraint(
        new ivec2[] { new ivec2(0, 1) },    // Pawn moves 1 square up (no horizontal movement)
        false,                             // Can’t repeat the move
        new Rotation[] { Rotation.NoRotation, Rotation.Rotate180 } // Can move up and down
    );

    // King: Moves 1 square in all 4 directions (horizontal, vertical, and diagonals), cannot repeat the move
    public static MoveConstraint KingMovement = new MoveConstraint(
        new ivec2[] { new ivec2(1, 0), new ivec2(0, 1) },  // King moves horizontally and vertically
        false,                                           // Can’t repeat the move
        new Rotation[] { Rotation.NoRotation, Rotation.Rotate90, Rotation.Rotate180, Rotation.Rotate270 } // All 4 directions
    );

    // Bishop: Moves diagonally (any diagonal direction), can repeat the move (multiple steps)
    public static MoveConstraint BishopMovement = new MoveConstraint(
        new ivec2[] { new ivec2(1, 1) },  // Bishop moves diagonally
        true,                             // Can repeat the move
        new Rotation[] { Rotation.NoRotation, Rotation.Rotate90, Rotation.Rotate180, Rotation.Rotate270 } // All 4 diagonals
    );

    // Rook: Moves horizontally or vertically, can repeat the move (multiple steps)
    public static MoveConstraint RookMovement = new MoveConstraint(
        new ivec2[] { new ivec2(1, 0), new ivec2(0, 1) },  // Rook moves horizontally or vertically
        true,                                              // Can repeat the move
        new Rotation[] { Rotation.NoRotation, Rotation.Rotate90, Rotation.Rotate180, Rotation.Rotate270 } // 0° and 90° (horizontal and vertical)
    );

    // Knight: Moves in an "L" shape (2 squares in one direction and 1 square perpendicular), cannot repeat the move
    public static MoveConstraint KnightMovement = new MoveConstraint(
        new ivec2[] { new ivec2(2, 1), new ivec2(1, 2) },  // Knight moves in an "L" shape
        false,                                            // Can’t repeat the move
        new Rotation[] { Rotation.NoRotation, Rotation.Rotate90, Rotation.Rotate180, Rotation.Rotate270 } // All "L" shapes
    );

    // Queen: Moves horizontally, vertically, or diagonally, can repeat the move (multiple steps)
    public static MoveConstraint QueenMovement = new MoveConstraint(
        new ivec2[] { new ivec2(1, 0), new ivec2(0, 1), new ivec2(1, 1) },  // Queen moves horizontally, vertically, and diagonally
        true,                                                            // Can repeat the move
        new Rotation[] { Rotation.NoRotation, Rotation.Rotate90, Rotation.Rotate180, Rotation.Rotate270 } // All 4 directions (horizontal, vertical, diagonal)
    );
}
