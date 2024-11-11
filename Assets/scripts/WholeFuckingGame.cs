using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Unity.VisualScripting;
using UnityEngine;

enum Dragged {
    None,
    ChessPiece,
    Card,
    FML,
}

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
        public GameObject C_Clone;
        public GameObject C_Blind;
        public GameObject C_Dispel;
        public GameObject C_Armageddon;
        public GameObject C_AntiMagic;
        public GameObject C_MagicArrow;
    public GameObject A_Selector;
    public GameObject A_Mana;
    public GameObject T_CardDescription;

    Dictionary<(PieceType, PieceColor), GameObject> piecePrefabs;
    Dictionary<CardAbility, GameObject> cardPrefabs;



    [SerializeField]
    public Vector3 boardLeftBottomPos = new Vector3(-34.72f, 0, -29.43f);
    [SerializeField]
    public float singleCellScale = (81.0f * ((142 - 7.0f * 2.0f) / (142.0f))) / 8.0f;
    [SerializeField]
    public Vector3 deckPosition = new Vector3(0, +2.5f, 0);


    ivec2 selectedBoardCell = new ivec2(0, 0);
    Vector3 draggingMousePos;

    // bool isPieceDragged = false;
    // bool isCardDragged = false;
    Dragged dragged = Dragged.None;
    bool isHandHovered = false;
    Card draggedCard = null;  // ptr of the dragged card

    // public ChessPiece[,] board = new ChessPiece[8, 8];
    public Board board;
    public DeckOfCards deck = new DeckOfCards();
    public HandOfCards hand = new HandOfCards();
    public PieceColor WhoseTurn = PieceColor.White;

    int movesLeft = 1;

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

            {CardAbility.Clone, C_Clone},
            {CardAbility.Blind, C_Blind},
            {CardAbility.Dispel, C_Dispel},
            {CardAbility.Armageddon, C_Armageddon},
            {CardAbility.AntiMagic, C_AntiMagic},
            {CardAbility.MagicArrow, C_MagicArrow},

            {CardAbility.Barricade, C_Slow},
            {CardAbility.Wrap, C_Slow},
            {CardAbility.Wind, C_Slow},
            {CardAbility.ExtraMove, C_Slow},
            {CardAbility.ExtendedCastling, C_Slow},
            {CardAbility.RandomEffect, C_Slow},
            {CardAbility.Resurrect, C_Slow},
        };

        board = new Board(boardLeftBottomPos, singleCellScale, piecePrefabs, A_Selector);
    }

    void Start() {
        deck.InitializeDeck(cardPrefabs, deckPosition, 
                            A_Mana, T_CardDescription);
        deck.Shuffle();

        Card card;
        card = deck.DrawCard(); if (card != null) hand.AddCard(card);
        card = deck.DrawCard(); if (card != null) hand.AddCard(card);
        card = deck.DrawCard(); if (card != null) hand.AddCard(card);
        card = deck.DrawCard(); if (card != null) hand.AddCard(card);
        card = deck.DrawCard(); if (card != null) hand.AddCard(card);
        card = deck.DrawCard(); if (card != null) hand.AddCard(card);
    }

    void Update() {
        // Handle input and update board
        bool didSomething = HandleInput();
        if (dragged != Dragged.None) {
            UpdateDraggingPosition();
        }
        if (dragged == Dragged.ChessPiece) {
            if (board.getPiece(selectedBoardCell) != null) {
                board.getPiece(selectedBoardCell).
                    SetVisualPosition(new Vector3(draggingMousePos.x, 2.5f, draggingMousePos.z));
            }
        } else if (dragged == Dragged.Card) {
            draggedCard.SetPosition(draggingMousePos);
        }
        hand.wholeHandHovered = isHandHovered;
        hand.UpdateHandDisplay();

        int selected_card_from_hand = hand.findSelected();

        if (didSomething) Turn();
    }

    void Turn() {
        movesLeft -= 1;
        if(movesLeft == 0){
            switchWhoseTurn();
        }
        board.Turn();

        // reset
        movesLeft = 1;
    }

    bool HandleInput() {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        LayerMask layerMask = LayerMask.GetMask("Cards");


        // WHY cards somewhat separate? They are not grid-snapped
        // i would also write collideres myself for it if it was an option
        isHandHovered = false;
        if (Physics.Raycast(ray.origin, new Vector3(0, -1, 0), out hit, Mathf.Infinity, layerMask)) {
            isHandHovered = true;
        }
        if (dragged == Dragged.Card) { isHandHovered = true; }

        // Handle left mouse button down (LMB click)
        if (Input.GetMouseButtonDown(0)) {
            if (Physics.Raycast(ray, out hit)) {
                // dragged = Dragged.None;

                // Check if clicked on a card
                var cardSelected = hand.findSelected();
                if (cardSelected != -1) {
                    // play cardSelected
                    // so now ref is in our mouse, and not updated by hand anymore
                    draggedCard = hand.DrawCard(cardSelected);
                    dragged = Dragged.Card;
                } else {
                    // Handle board cell clicks
                    Vector3Int clickedBoardCell = ScreenToBoardPosition(hit.point);
                    bool validTargetCell = 
                        board.IsPosInBoardBounds(new ivec2(clickedBoardCell.x, clickedBoardCell.z)) &&
                        board.getPiece(clickedBoardCell.x, clickedBoardCell.z) != null
                    ;

                    // Enable dragging if a valid cell is clicked
                    if (validTargetCell) {
                        if(board.getPiece(clickedBoardCell.x, clickedBoardCell.z).Color == WhoseTurn) {
                            selectedBoardCell = new ivec2(clickedBoardCell.x, clickedBoardCell.z);
                            dragged = Dragged.ChessPiece;
                            //show possible moves when a piece dragged
                            board.ShowMoveSelectors(new ivec2(clickedBoardCell.x, clickedBoardCell.z));
                        }
                    }
                }
            }
        }

        // Handle left mouse button release (LMB up)
        bool didSomethingThatEndsTurn = false;
        if (Input.GetMouseButtonUp(0)) {
            if (dragged == Dragged.ChessPiece) {
                if (Physics.Raycast(ray, out hit)) {
                    Vector3Int clickedBoardCell = ScreenToBoardPosition(hit.point);
                    bool validTargetCell = board.IsPosInBoardBounds(new ivec2(clickedBoardCell.x, clickedBoardCell.z));

                    if (validTargetCell) {
                        if(board.getPiece(selectedBoardCell).Color == WhoseTurn) {
                            Debug.Log($"Valid move to {clickedBoardCell}");
                            bool moved = board.tryMovePiece(selectedBoardCell, new ivec2(clickedBoardCell.x, clickedBoardCell.z));
                            if (moved) {
                                board.getPiece(clickedBoardCell.x, clickedBoardCell.z).SetVisualPosition(board.getCellPosition(clickedBoardCell.x, clickedBoardCell.z));
                                didSomethingThatEndsTurn = true;
                            }
                        }
                    }
                }

                // Reset visual position
                if (board.getPiece(selectedBoardCell.x, selectedBoardCell.y) != null) {
                    board.getPiece(selectedBoardCell.x, selectedBoardCell.y).SetVisualPosition(board.getCellPosition(selectedBoardCell.x, selectedBoardCell.y));
                }

                //do not show possible moves when no piece dragged anymore
                board.ClearSelectors();

                dragged = Dragged.None;
            } else if (dragged == Dragged.Card) {
                // for now, return card back
                ivec2 clickedBoardCell = new ivec2(ScreenToBoardPosition(hit.point).x, ScreenToBoardPosition(hit.point).z);
                bool validTargetCell =
                    board.IsPosInBoardBounds(clickedBoardCell) &&
                    board.checkPiece(clickedBoardCell)
                ;


                if (validTargetCell) {
                    Debug.Log("card casted on" + clickedBoardCell);
                    bool applied = draggedCard.tryApplyEffect(board, clickedBoardCell.x, clickedBoardCell.y, this);
                    if (applied) {
                        draggedCard.Destroy();
                        draggedCard = null; // free and remove the card
                        // dragged = Dragged.None;
                    } else {
                        hand.AddCard(draggedCard);
                    }
                } else {
                    // return back to the hand
                    hand.AddCard(draggedCard);
                }

                dragged = Dragged.None;
            }
        }

        return didSomethingThatEndsTurn;
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

    void switchWhoseTurn() {
        if (WhoseTurn == PieceColor.Black) {
            WhoseTurn = PieceColor.White;
        } else if (WhoseTurn == PieceColor.White) {
            WhoseTurn = PieceColor.Black;
        }
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