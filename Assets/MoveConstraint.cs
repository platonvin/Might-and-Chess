
using UnityEngine;

public enum Rotation {
    NoRotation = 0,
    Rotate90 = 90,
    Rotate180 = 180,
    Rotate270 = 270
}

//
public struct IVec2 {
    public int x;
    public int y;

    public IVec2(int x, int y) {
        this.x = x;
        this.y = y;
    }

    // For rotation, we could define a method that rotates the vector.
    public IVec2 Rotate(Rotation rotation) {
        switch (rotation) {
            case Rotation.NoRotation:
                return this;
            case Rotation.Rotate90:
                return new IVec2(-y, x); // 90-degree clockwise rotation
            case Rotation.Rotate180:
                return new IVec2(-x, -y); // 180-degree rotation
            case Rotation.Rotate270:
                return new IVec2(y, -x); // 270-degree clockwise rotation
            default:
                return this;
        }
    }
}

public class MoveConstraint {
    public IVec2[] shifts;          // Array of direction vectors for movement
    public bool isRepeated;         // Can the movement be repeated?
    public Rotation[] rotations;    // Possible rotations for the movement pattern

    public MoveConstraint(IVec2[] shifts, bool isRepeated, Rotation[] rotations) {
        this.shifts = shifts;
        this.isRepeated = isRepeated;
        this.rotations = rotations;
    }

    // Check if a move from startPos to endPos is valid based on the constraint
    public bool CheckMove(Vector2Int startPos, Vector2Int endPos)
    {
        // Calculate the difference between start and end position
        IVec2 moveDifference = new IVec2(endPos.x - startPos.x, endPos.y - startPos.y);

        // Check all possible rotations and shifts
        foreach (var shift in shifts) {
            foreach (var rotation in rotations) {
                IVec2 rotatedShift = shift.Rotate(rotation);

                // Prevent division by zero
                if (rotatedShift.x != 0 && rotatedShift.y != 0)
                {
                    if (isRepeated)
                    {
                        // If the move is repeated, check if moveDifference is a multiple of rotatedShift
                        if ((moveDifference.x % rotatedShift.x == 0) && (moveDifference.y % rotatedShift.y == 0))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        // If not repeated, check if moveDifference matches exactly
                        if (moveDifference.x == rotatedShift.x && moveDifference.y == rotatedShift.y)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    // If rotatedShift.x or rotatedShift.y is zero, handle separately
                    if (rotatedShift.x == 0 && moveDifference.x == 0 && (isRepeated || moveDifference.y == rotatedShift.y))
                    {
                        return true;
                    }
                    else if (rotatedShift.y == 0 && moveDifference.y == 0 && (isRepeated || moveDifference.x == rotatedShift.x))
                    {
                        return true;
                    }
                }
            }
        }

        return false; // No valid move found
    }
}

public class ChessPieceMovement {
    // Pawn: Moves 1 square up (or down with 180-degree rotation), cannot repeat the move
    public static MoveConstraint PawnMovement = new MoveConstraint(
        new IVec2[] { new IVec2(0, 1) },    // Pawn moves 1 square up (no horizontal movement)
        false,                             // Can’t repeat the move
        new Rotation[] { Rotation.NoRotation, Rotation.Rotate180 } // Can move up and down
    );

    // King: Moves 1 square in all 4 directions (horizontal, vertical, and diagonals), cannot repeat the move
    public static MoveConstraint KingMovement = new MoveConstraint(
        new IVec2[] { new IVec2(1, 0), new IVec2(0, 1) },  // King moves horizontally and vertically
        false,                                           // Can’t repeat the move
        new Rotation[] { Rotation.NoRotation, Rotation.Rotate90, Rotation.Rotate180, Rotation.Rotate270 } // All 4 directions
    );

    // Bishop: Moves diagonally (any diagonal direction), can repeat the move (multiple steps)
    public static MoveConstraint BishopMovement = new MoveConstraint(
        new IVec2[] { new IVec2(1, 1) },  // Bishop moves diagonally
        true,                             // Can repeat the move
        new Rotation[] { Rotation.NoRotation, Rotation.Rotate90, Rotation.Rotate180, Rotation.Rotate270 } // All 4 diagonals
    );

    // Rook: Moves horizontally or vertically, can repeat the move (multiple steps)
    public static MoveConstraint RookMovement = new MoveConstraint(
        new IVec2[] { new IVec2(1, 0), new IVec2(0, 1) },  // Rook moves horizontally or vertically
        true,                                              // Can repeat the move
        new Rotation[] { Rotation.NoRotation, Rotation.Rotate90, Rotation.Rotate180, Rotation.Rotate270 } // 0° and 90° (horizontal and vertical)
    );

    // Knight: Moves in an "L" shape (2 squares in one direction and 1 square perpendicular), cannot repeat the move
    public static MoveConstraint KnightMovement = new MoveConstraint(
        new IVec2[] { new IVec2(2, 1), new IVec2(1, 2) },  // Knight moves in an "L" shape
        false,                                            // Can’t repeat the move
        new Rotation[] { Rotation.NoRotation, Rotation.Rotate90, Rotation.Rotate180, Rotation.Rotate270 } // All "L" shapes
    );

    // Queen: Moves horizontally, vertically, or diagonally, can repeat the move (multiple steps)
    public static MoveConstraint QueenMovement = new MoveConstraint(
        new IVec2[] { new IVec2(1, 0), new IVec2(0, 1), new IVec2(1, 1) },  // Queen moves horizontally, vertically, and diagonally
        true,                                                            // Can repeat the move
        new Rotation[] { Rotation.NoRotation, Rotation.Rotate90, Rotation.Rotate180, Rotation.Rotate270 } // All 4 directions (horizontal, vertical, diagonal)
    );
}
