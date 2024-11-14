using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

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
    public GameObject N_Barricade;
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
    //cards
    public GameObject C_AntiMagic;
    public GameObject C_Armageddon;
    public GameObject C_Barrier;
    public GameObject C_Blind;
    public GameObject C_Clone;
    public GameObject C_Dispel;
    public GameObject C_Flight;
    public GameObject C_Haste;
    public GameObject C_LightingBolt;
    public GameObject C_Push;
    public GameObject C_Resurrect;
    public GameObject C_Sacrifice;
    public GameObject C_Slow;
    public GameObject C_Weakness;
    //possible moves
    public GameObject A_Selector;
    //mana
    public GameObject A_Mana;
    //text
    public GameObject T_CardDescription;
    //icons
    public GameObject I_Haste;
    public GameObject I_Slow;
    public GameObject I_Clone;
    public GameObject I_AntiMagic;
    public GameObject I_Blind;
    public GameObject I_Hypnotize;

    // on cast animations
    public GameObject AA_Armagedon;
    public GameObject AA_Dispel;
    public GameObject AA_Haste;
    public GameObject AA_Slow;
    public GameObject AA_LightingBolt;
    public GameObject AA_Blind;
    public GameObject AA_AntiMagic;
    public GameObject AA_Resurrect;
    public GameObject AA_Hypnotize;

    public GameObject CardGraveyardObject;

    Dictionary<(PieceType, PieceColor), GameObject> piecePrefabs;
    Dictionary<CardAbility, GameObject> animationPrefabs; 
    Dictionary<CardAbility, GameObject> cardPrefabs;
    Dictionary<CardAbility, GameObject> iconPrefabs;
    // C# made me love C++

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
    public HandOfCards whiteHand = new HandOfCards();
    public HandOfCards blackhand = new HandOfCards();
    public PieceColor WhoseTurn = PieceColor.White;

    // actual state that does something apart from fixing unity
    int whiteMovesLeft = 1;
    int blackMovesLeft = 1;
    const int MAX_MANA = 8;
    int whiteManaLeft = MAX_MANA;
    int blackManaLeft = MAX_MANA;
    private Transform manaContainer;
    private List<GameObject> manaIcons = new();
    [SerializeField]
    public Vector3 manaContainerPos;

    private IInputHandler inputHandler;

    private void Awake() {
        // Use the appropriate input handler based on the platform
#if UNITY_EDITOR || UNITY_STANDALONE
        inputHandler = new MouseInputHandler();
#elif UNITY_IOS || UNITY_ANDROID
            inputHandler = new TouchInputHandler();
#endif

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
            { (PieceType.Rook, PieceColor.White), W_Rook },

            { (PieceType.Barricade, PieceColor.Neutral), N_Barricade }
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
            {CardAbility.LightingBolt, C_LightingBolt},

            {CardAbility.Barricade, C_Barrier},
            {CardAbility.Resurrect, C_Resurrect},
            {CardAbility.Flight, C_Flight},
            {CardAbility.Sacrifice, C_Sacrifice},
            {CardAbility.Weakness, C_Weakness},

            {CardAbility.ExtraMove, C_Slow},
            {CardAbility.ExtendedCastling, C_Slow},
            {CardAbility.RandomEffect, C_Slow},
            {CardAbility.Wrap, C_Slow},
            {CardAbility.Wind, C_Slow},
        };

        iconPrefabs = new Dictionary<CardAbility, GameObject>
        {
            {CardAbility.Haste, I_Haste},
            {CardAbility.Slow, I_Slow},
            {CardAbility.Clone, I_Clone},
            {CardAbility.Blind, I_Blind},
            {CardAbility.AntiMagic, I_AntiMagic},
            {CardAbility.Hypnotize, I_Hypnotize},
        };
        
        animationPrefabs = new Dictionary<CardAbility, GameObject>
        {
            {CardAbility.Armageddon, AA_Armagedon},
            {CardAbility.Dispel, AA_Dispel},
            {CardAbility.Haste, AA_Haste},
            {CardAbility.Slow, AA_Slow},
            {CardAbility.LightingBolt, AA_LightingBolt},
            {CardAbility.Blind, AA_Blind},
            {CardAbility.AntiMagic, AA_AntiMagic},
            {CardAbility.Resurrect, AA_Resurrect},
            {CardAbility.Hypnotize, AA_Hypnotize},
        };

        board = new Board(boardLeftBottomPos, singleCellScale, piecePrefabs, iconPrefabs, animationPrefabs, A_Selector);
    }

    void Start() {
        deck.initDeck(cardPrefabs, deckPosition,
                            A_Mana, T_CardDescription);
        deck.shuffle();

        Card card;
        card = deck.drawCard(); if (card != null) whiteHand.addCard(card);
        card = deck.drawCard(); if (card != null) whiteHand.addCard(card);
        card = deck.drawCard(); if (card != null) whiteHand.addCard(card);
        card = deck.drawCard(); if (card != null) whiteHand.addCard(card);
        card = deck.drawCard(); if (card != null) whiteHand.addCard(card);
        card = deck.drawCard(); if (card != null) whiteHand.addCard(card);

        CreateManaDisplay(whiteManaLeft);
    }

    // Initializes mana display with a specified amount
    private void CreateManaDisplay(int startingMana) {
        whiteManaLeft = startingMana;

        // Create a container for the mana icons
        GameObject manaContainerObject = new GameObject("ManaContainer");
        manaContainerObject.transform.SetParent(this.transform);
        manaContainer = manaContainerObject.transform;

        // Position and rotation for the container
        manaContainer.localPosition = manaContainerPos;
        manaContainer.localRotation = Quaternion.Euler(new(90, 0, 0));
        manaContainer.localScale = new(4, 4, 4);

        // Set spacing between icons
        float iconSpacing = 0.89f;
        float totalWidth = (startingMana - 1) * iconSpacing;

        // Create and center each mana icon in the container
        for (int i = 0; i < startingMana; i++) {
            GameObject manaIcon = Instantiate(A_Mana, manaContainer);
            manaIcon.transform.localPosition = new Vector3(0, i * iconSpacing - totalWidth / 2, 0);
            manaIcons.Add(manaIcon);
        }
    }

    // Update mana display when mana changes
    public void UpdateManaDisplay() {
        // Activate or deactivate mana icons based on current mana
        for (int i = 0; i < manaIcons.Count; i++) {
            manaIcons[i].SetActive(i < whiteManaLeft);
        }
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
            // if(draggedCard != null)
            draggedCard.SetPosition(draggingMousePos);
        }
        whiteHand.wholeHandHovered = isHandHovered;
        whiteHand.updateHowHandDisplayed();

        UpdateManaDisplay();

        int selected_card_from_hand = whiteHand.findSelected();

        if (didSomething) Turn();
    }

    void Turn() {
        if (WhoseTurn == PieceColor.White) {
            whiteMovesLeft -= 1;
            if (whiteMovesLeft == 0) {
                switchWhoseTurn();
            }
            // give player 2 cards
            Card card;
            card = deck.drawCard(); if (card != null) whiteHand.addCard(card);
            card = deck.drawCard(); if (card != null) whiteHand.addCard(card);
        } else {
            blackMovesLeft -= 1;
            if (blackMovesLeft == 0) {
                switchWhoseTurn();
            }
        }

        board.Turn();

        // reset. Keep in mind that they already inverted, so White means "white started turn"
        if (WhoseTurn == PieceColor.White) {
            // whiteManaLeft += 3;
            whiteManaLeft = Math.Clamp(whiteManaLeft, 0, MAX_MANA);
            whiteMovesLeft = 1;
        } else {
            // blackManaLeft += 3;
            blackManaLeft = Math.Clamp(blackManaLeft, 0, MAX_MANA);
            blackMovesLeft = 1;
        }
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
        if (inputHandler.IsInputDown()) {
            if (Physics.Raycast(ray, out hit)) {
                // Check if clicked on a card
                var cardSelected = whiteHand.findSelected();
                if (cardSelected != -1) {
                    // play cardSelected
                    // so now ref is in our mouse, and not updated by hand anymore
                    draggedCard = whiteHand.drawCard(cardSelected);
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
                        if (board.getPiece(clickedBoardCell.x, clickedBoardCell.z).Color == WhoseTurn) {
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
        if (inputHandler.IsInputUp()) {
            if (dragged == Dragged.ChessPiece) {
                if (Physics.Raycast(ray, out hit)) {
                    Vector3Int clickedBoardCell = ScreenToBoardPosition(hit.point);
                    bool validTargetCell = board.IsPosInBoardBounds(new ivec2(clickedBoardCell.x, clickedBoardCell.z));

                    if (validTargetCell) {
                        ChessPiece piece = board.getPiece(selectedBoardCell);
                        PieceColor effective_color = 
                            (piece.controlledByOpponentTurnsLeft > 0) ? 
                            switchColor(piece.Color) : piece.Color;

                        if (effective_color == WhoseTurn) {
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
            } else if (dragged == Dragged.Card) {
                // if dropped into card graveyard
                if(ifGraveyardHovered()) {
                    if(whiteManaLeft < MAX_MANA) {
                        draggedCard.Destroy();
                        draggedCard = null; // free and remove the card
                        whiteManaLeft += 1;
                        UpdateManaDisplay();
                    } else {
                        whiteHand.addCard(draggedCard); // return to the hand
                    }
                } else {
                    // try to apply
                    ivec2 clickedBoardCell = new ivec2(ScreenToBoardPosition(hit.point).x, ScreenToBoardPosition(hit.point).z);
                    bool validTargetCell =
                        board.IsPosInBoardBounds(clickedBoardCell) 
                        // && board.checkPiece(clickedBoardCell)
                    ;
                    if (validTargetCell) {
                        Debug.Log("card casted on" + clickedBoardCell);
                        DeckDef deckDef = DeckDef.Instance;
                        int appliedManaCost = deckDef.defs[draggedCard.Ability].Item2;

                        if (whiteManaLeft >= appliedManaCost) {
                            bool applied = draggedCard.tryApplyEffect(board, clickedBoardCell.x, clickedBoardCell.y, this);
                            if (applied) {
                                //only if applied
                                board.playAnimation(draggedCard.Ability, board.getCellPosition(clickedBoardCell.x, clickedBoardCell.y));
                                whiteManaLeft -= appliedManaCost;
                                draggedCard.Destroy();
                                draggedCard = null; // free and remove the card
                                UpdateManaDisplay();
                                ChessPiece piece = board.getPiece(clickedBoardCell);
                                // because effects can destroy it (magic arrow)
                                if (piece != null) piece.updateEffectsDisplay(); // could be inside apply effect but moved here
                            } else {
                                whiteHand.addCard(draggedCard);
                            }
                        } else {
                            whiteHand.addCard(draggedCard);
                        }
                    } else {
                        // return back to the hand
                        whiteHand.addCard(draggedCard);
                    }
                }
            }

            // "reset the carret:
            dragged = Dragged.None;
        }

        return didSomethingThatEndsTurn;
    }

    public bool ifGraveyardHovered() {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        LayerMask layerMask = LayerMask.GetMask("Card Graveyard layer");

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)) {
            // Check if the hit collider's game object is a child of this card
            if (hit.collider.transform.IsChildOf(CardGraveyardObject.transform)) {
                Debug.Log("GraveYard hovered");
                return true;
            }
        }

        Debug.Log("GraveYard NOT hovered");
        return false; // Return -1 if no card was hit
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
        WhoseTurn = switchColor(WhoseTurn);
    }


    PieceColor switchColor(PieceColor color) {
        if (color == PieceColor.Black) {
            color = PieceColor.White;
        } else if (color == PieceColor.White) {
            color = PieceColor.Black;
        }
        return color;
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
