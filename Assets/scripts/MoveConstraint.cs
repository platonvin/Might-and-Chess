using System;

public enum WeirdRotation {
    NoRotation = 0,
    Rotate90 = 90,
    Rotate180 = 180,
    Rotate270 = 270
}

public class MoveConstraint {
    public ivec2[] shifts;          // Array of direction vectors for movement
    public bool isRepeated;         // Can the movement be repeated?
    public bool repeatInsteadOfSpeed;         // Can the movement be repeated?
    public WeirdRotation[] rotations;    // Possible rotations for the movement pattern
    public bool doesHasteWork; // for pawns attack basically

    public MoveConstraint(ivec2[] shifts, bool isRepeated, WeirdRotation[] rotations, bool doesHasteWork = true, bool repeatInsteadOfSpeed = false) {
        this.shifts = shifts;
        this.isRepeated = isRepeated;
        this.rotations = rotations;
        this.repeatInsteadOfSpeed = repeatInsteadOfSpeed;
        this.doesHasteWork = doesHasteWork;
    }

    public bool CheckMove(Board board, ivec2 startPos, ivec2 endPos, int speed = 0) {
        bool valid = CheckMovePattern(board, startPos, endPos, speed);
        return valid;
    }

    // Check if a move from startPos to endPos is valid based on the constraint
    public bool CheckMovePattern(Board board, ivec2 startPos, ivec2 endPos, int speed) {
        ivec2 moveDifference = endPos - startPos;
        if (moveDifference == new ivec2(0, 0)) return false; // No movement

        foreach (var originShift in shifts) {
            foreach (var rotation in rotations) {
                ivec2 rotatedShift = originShift.Rotate(rotation);

                // Calculate unit direction based on rotated shift values
                ivec2 shiftDirection = new ivec2(
                    rotatedShift.x < 0 ? -1 : (rotatedShift.x > 0 ? 1 : 0),
                    rotatedShift.y < 0 ? -1 : (rotatedShift.y > 0 ? 1 : 0)
                );

                // Adjust speed only for non-repeated moves
                ivec2 currentShift = rotatedShift;

                // e.g. pawn attack 
                if (!doesHasteWork) speed = 0;

                if (!isRepeated) {
                    if (repeatInsteadOfSpeed) {
                        // basically speed will be achived via iterative approach
                    } else {
                        currentShift += shiftDirection * speed;
                        currentShift.clamp(new ivec2(0, 0), new ivec2(10, 10));
                    }
                } else { // is repeated
                    // Clamp speed for repeated moves as intended
                    speed = Math.Clamp(speed, -1, 0);
                }

                if ((currentShift.x != 0) || (currentShift.y != 0)) {
                    // Calculate number of steps required to match moveDifference
                    int stepsX = currentShift.x == 0 ? 0 : (moveDifference.x / currentShift.x);
                    int stepsY = currentShift.y == 0 ? 0 : (moveDifference.y / currentShift.y);

                    bool validSteps = (currentShift.x == 0 || moveDifference.x % currentShift.x == 0) &&
                                    (currentShift.y == 0 || moveDifference.y % currentShift.y == 0);

                    if (isRepeated && validSteps) {
                        int distance = Math.Max(Math.Abs(moveDifference.x), Math.Abs(moveDifference.y));
                        if (distance > 0 && CheckPathClear(board, startPos, endPos, currentShift)) {
                            // Repeated move patterns (e.g., Rook, Queen)
                            if (stepsX == stepsY) return true;
                            if (currentShift.x == 0 && stepsY > 0) return true;
                            if (currentShift.y == 0 && stepsX > 0) return true;
                        }
                    } else if (!isRepeated && validSteps) {
                        // Non-repeating move patterns (e.g., King, Knight)
                        // this also allows pawn to move both 1 and 2
                        if (moveDifference == currentShift) return true;

                        // basically pawn only
                        if (repeatInsteadOfSpeed) {
                            // basically speed will be achived via iterative approach
                            int distance = Math.Max(Math.Abs(moveDifference.x), Math.Abs(moveDifference.y));
                            if (distance > 0 && CheckPathClear(board, startPos, endPos, currentShift)) {
                                if (stepsX == stepsY && stepsY == (speed + 1)) return true;
                                if (currentShift.x == 0 && stepsY > 0 && stepsY == (speed + 1)) return true;
                                if (currentShift.y == 0 && stepsX > 0 && stepsX == (speed + 1)) return true;
                            }
                        } else {
                        }
                    }
                }
            }
        }
        return false; // No valid move pattern found
    }


    public bool CheckPathClear(Board board, ivec2 startPos, ivec2 endPos, ivec2 step) {
        ivec2 currentPos = startPos + step;
        int maxObsitclesOnPath = 1;
        if (board.getPiece(startPos).flightLeft > 0) maxObsitclesOnPath = 2;
        int obsticlesMet = 0;

        while (currentPos != endPos) {
            // Check if within bounds
            if (!board.IsPosInBoardBounds(currentPos)) { return false; } // Out of bounds
            // Check for an obstacle
            if (board.checkPiece(currentPos)) {
                obsticlesMet++;
                if (obsticlesMet >= maxObsitclesOnPath) return false; // Path is blocked enough times
            }
            // Move to the next position
            currentPos += step;
        }

        // Reached the destination without obstacles
        return true;
    }
}

public class ChessPieceMovement {
    // Pawn: Moves 1 square up (or down with 180-degree rotation), cannot repeat the move
    public static MoveConstraint PawnWhiteMovement = new MoveConstraint(
        new ivec2[] { new ivec2(0, 1) },    // Pawn moves 1 square up (no horizontal movement)
        false,                             // Can’t repeat the move
        new WeirdRotation[] { WeirdRotation.NoRotation }, // Can move up and down
        true,// haste works on movement
        true // cause they do not jump
    );
    // Pawn is little weird and has diagonal attack
    public static MoveConstraint PawnWhiteAttack = new MoveConstraint(
        new ivec2[] { new ivec2(+1, 1), new ivec2(-1, 1) },    // Pawn moves 1 square up and +1/-1 to the side
        false,                              // Can’t repeat the move
        new WeirdRotation[] { WeirdRotation.NoRotation }, // Can move up and down
        false,// haste doesnt work on attack
        true // cause they do not jump
    );
    public static MoveConstraint PawnBlackMovement = new MoveConstraint(
        new ivec2[] { new ivec2(0, -1) },    // Pawn moves 1 square up (no horizontal movement)
        false,                             // Can’t repeat the move
        new WeirdRotation[] { WeirdRotation.NoRotation }, // Can move up and down
        true,// haste works on movement
        true // cause they do not jump
    );
    public static MoveConstraint PawnBlackAttack = new MoveConstraint(
        new ivec2[] { new ivec2(+1, -1), new ivec2(-1, -1) },    // Pawn moves 1 square up and +1/-1 to the side
        false,                              // Can’t repeat the move
        new WeirdRotation[] { WeirdRotation.NoRotation }, // Can move up and down
        false,// haste doesnt work on attack
        true // cause they do not jump
    );

    // King: Moves 1 square in all 4 directions (horizontal, vertical, and diagonals), cannot repeat the move
    public static MoveConstraint KingMovement = new MoveConstraint(
        new ivec2[] { new ivec2(1, 0), new ivec2(1, 1), },  // King moves horizontally and vertically
        false,                                           // Can’t repeat the move
        new WeirdRotation[] { WeirdRotation.NoRotation, WeirdRotation.Rotate90, WeirdRotation.Rotate180, WeirdRotation.Rotate270 } // All 4 directions
    );

    // Bishop: Moves diagonally (any diagonal direction), can repeat the move (multiple steps)
    public static MoveConstraint BishopMovement = new MoveConstraint(
        new ivec2[] { new ivec2(1, 1) },  // Bishop moves diagonally
        true,                             // Can repeat the move
        new WeirdRotation[] { WeirdRotation.NoRotation, WeirdRotation.Rotate90, WeirdRotation.Rotate180, WeirdRotation.Rotate270 } // All 4 diagonals
    );

    // Rook: Moves horizontally or vertically, can repeat the move (multiple steps)
    public static MoveConstraint RookMovement = new MoveConstraint(
        new ivec2[] { new ivec2(1, 0), new ivec2(0, 1) },  // Rook moves horizontally or vertically
        true,                                              // Can repeat the move
        new WeirdRotation[] { WeirdRotation.NoRotation, WeirdRotation.Rotate90, WeirdRotation.Rotate180, WeirdRotation.Rotate270 } // 0° and 90° (horizontal and vertical)
    );

    // Knight: Moves in an "L" shape (2 squares in one direction and 1 square perpendicular), cannot repeat the move
    public static MoveConstraint KnightMovement = new MoveConstraint(
        new ivec2[] { new ivec2(2, 1), new ivec2(1, 2) },  // Knight moves in an "L" shape
        false,                                            // Can’t repeat the move
        new WeirdRotation[] { WeirdRotation.NoRotation, WeirdRotation.Rotate90, WeirdRotation.Rotate180, WeirdRotation.Rotate270 } // All "L" shapes
    );

    // Queen: Moves horizontally, vertically, or diagonally, can repeat the move (multiple steps)
    public static MoveConstraint QueenMovement = new MoveConstraint(
        new ivec2[] { new ivec2(1, 1), new ivec2(1, 0), new ivec2(0, 1) },  // Queen moves horizontally, vertically, and diagonally
        true,                                                            // Can repeat the move
        new WeirdRotation[] { WeirdRotation.NoRotation, WeirdRotation.Rotate90, WeirdRotation.Rotate180, WeirdRotation.Rotate270 } // All 4 directions (horizontal, vertical, diagonal)
    );
}
