using System;
using System.Collections.Generic;
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
    public GameObject N_Barricade;
    // blacks
    public GameObject B_Bishop,
                      B_King,
                      B_Knight,
                      B_Pawn,
                      B_Queen,
                      B_Rook;
    // whites
    public GameObject W_Bishop,
                      W_King,
                      W_Knight,
                      W_Pawn,
                      W_Queen,
                      W_Rook;
    //cards
    public GameObject C_AntiMagic,
                      C_Armageddon,
                      C_Barrier,
                      C_Blind,
                      C_Clone,
                      C_Dispel,
                      C_Flight,
                      C_Haste,
                      C_LightingBolt,
                      C_Push,
                      C_Resurrect,
                      C_Sacrifice,
                      C_Slow,
                      C_Weakness;
    //possible moves
    public GameObject A_Selector;
    //mana
    public GameObject A_Mana;
    //text
    public GameObject T_CardDescription;
    //icons
    public GameObject I_Haste,
                      I_Slow,
                      I_Clone,
                      I_AntiMagic,
                      I_Blind,
                      I_Hypnotize,
                      I_Weakness;

    // on cast animations
    public GameObject AA_Armagedon,
                      AA_Dispel,
                      AA_Haste,
                      AA_Slow,
                      AA_LightingBolt,
                      AA_Blind,
                      AA_AntiMagic,
                      AA_Resurrect,
                      AA_Hypnotize,
                      AA_Berserk,
                      AA_Weakness,
                      AA_Flight;

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
    Transform manaContainer;
    List<GameObject> manaIcons = new();
    [SerializeField]
    public Vector3 manaContainerPos;

    IInputHandler inputHandler;

    ChessAS bot; //

    void Awake() {
        // input handler based on the platform
        #if UNITY_EDITOR || UNITY_STANDALONE
            inputHandler = new MouseInputHandler();
        #elif UNITY_IOS || UNITY_ANDROID
            inputHandler = new TouchInputHandler();
        #endif

        // init prefab dictionaries
        piecePrefabs = new Dictionary<(PieceType, PieceColor), GameObject> {
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

        cardPrefabs = new Dictionary<CardAbility, GameObject> {
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

        iconPrefabs = new Dictionary<CardAbility, GameObject> {
            {CardAbility.Haste, I_Haste},
            {CardAbility.Slow, I_Slow},
            {CardAbility.Clone, I_Clone},
            {CardAbility.Blind, I_Blind},
            {CardAbility.AntiMagic, I_AntiMagic},
            {CardAbility.Hypnotize, I_Hypnotize},
            {CardAbility.Weakness, I_Weakness},
        };

        animationPrefabs = new Dictionary<CardAbility, GameObject> {
            {CardAbility.Armageddon, AA_Armagedon},
            {CardAbility.Dispel, AA_Dispel},
            {CardAbility.Haste, AA_Haste},
            {CardAbility.Slow, AA_Slow},
            {CardAbility.LightingBolt, AA_LightingBolt},
            {CardAbility.Blind, AA_Blind},
            {CardAbility.AntiMagic, AA_AntiMagic},
            {CardAbility.Resurrect, AA_Resurrect},
            {CardAbility.Hypnotize, AA_Hypnotize},
            {CardAbility.Berserk, AA_Berserk},
            {CardAbility.Weakness, AA_Weakness},
            {CardAbility.Flight, AA_Flight},
        };

        board = new Board(boardLeftBottomPos, singleCellScale, piecePrefabs, iconPrefabs, animationPrefabs, A_Selector);
        bot = new ChessAS(board);

        board.Turn();

        //play anim of haste for every pawn
        for (int x = 0; x < 8; x++) {
            for (int y = 0; y < 8; y++) {
                ChessPiece piece = board.getPiece(x, y);
                if (piece == null) continue;

                if ((piece.type == PieceType.Pawn)) {
                    board.playAnimation(CardAbility.Haste, board.getCellPosition(x, y));
                }

            }
        }
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

    // init mana display thing to the right
    void CreateManaDisplay(int startingMana) {
        whiteManaLeft = startingMana;

        // container for mana icons
        GameObject manaContainerObject = new GameObject("ManaContainer");
        manaContainerObject.transform.SetParent(this.transform);
        manaContainer = manaContainerObject.transform;

        manaContainer.localPosition = manaContainerPos;
        manaContainer.localRotation = Quaternion.Euler(new(90, 0, 0));
        manaContainer.localScale = new(4, 4, 4);

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
                    setTargetPosition(new Vector3(draggingMousePos.x, 2.5f, draggingMousePos.z));
            }
        } else if (dragged == Dragged.Card) {
            // if(draggedCard != null)
            draggedCard.SetPosition(draggingMousePos);
        }
        whiteHand.wholeHandHovered = isHandHovered;
        whiteHand.updateHowHandDisplayed();

        UpdateManaDisplay();
        // int selected_card_from_hand = whiteHand.findSelected();
        if (didSomething) Turn();
        if (WhoseTurn == PieceColor.Black) {
            var (from, to) = bot.FindBestMove(PieceColor.Black);
            Debug.LogError("from " + from.x + " " + from.y);
            Debug.LogError("to " + to.x + " " + to.y);
            bool moved = board.tryMovePiece(from, to);
            Debug.Assert(moved);
            Turn();
        }

        board.visualUpdate();

        // if not dragged reset target 
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
        } else if (WhoseTurn == PieceColor.Black) {
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

        // WHY cards somewhat separate? They are not grid-snapped
        // i would also write collideres myself for it if it was an option
        bool handRayHit = Physics.Raycast(ray.origin, new Vector3(0, -1, 0), out hit, Mathf.Infinity, LayerMask.GetMask("Cards"));
        if (handRayHit || (dragged == Dragged.Card)) { isHandHovered = true; } else { isHandHovered = false; }

        bool didSomethingThatEndsTurn = false;
        bool anyHit = Physics.Raycast(ray, out hit);
        ivec2 clickedBoardCell = ScreenToBoardPosition(hit.point);
        if (inputHandler.IsInputDown()) { didSomethingThatEndsTurn = HandleLMBDown(ray, anyHit, clickedBoardCell); }
        if (inputHandler.IsInputUp()) { didSomethingThatEndsTurn = HandleLMBUp(ray, anyHit, clickedBoardCell); }

        return didSomethingThatEndsTurn;
    }

    bool HandleLMBDown(Ray ray, bool anyHit, ivec2 clickedBoardCell) {
        if (anyHit) {
            // Check if clicked on a card
            var cardSelected = whiteHand.findSelected();
            if (cardSelected != -1) {
                // play cardSelected
                // so now ref is in our mouse, and not updated by hand anymore
                draggedCard = whiteHand.drawCard(cardSelected);
                dragged = Dragged.Card;
            } else {
                // Handle board cell clicks
                bool validTargetCell =
                    board.IsPosInBoardBounds(clickedBoardCell) &&
                    board.getPiece(clickedBoardCell) != null
                ;
                selectedBoardCell = clickedBoardCell;

                // Enable dragging if a valid cell is clicked
                if (validTargetCell) {
                    if (board.getPiece(clickedBoardCell).color == WhoseTurn) {
                        // selectedBoardCell = clickedBoardCell;
                        dragged = Dragged.ChessPiece;
                        //show possible moves when a piece dragged
                        board.ShowMoveSelectors(clickedBoardCell);
                    }
                }
            }
        }
        return false;
    }
    bool HandleLMBUp(Ray ray, bool anyHit, ivec2 clickedBoardCell) {
        bool didSomethingThatEndsTurn = false;
        if (dragged == Dragged.ChessPiece) {
            if (anyHit) {
                bool validTargetCell = board.IsPosInBoardBounds(clickedBoardCell);

                if (validTargetCell) {
                    ChessPiece piece = board.getPiece(selectedBoardCell);
                    PieceColor effective_color =
                        (piece.controlledByOpponentTurnsLeft > 0) ?
                        switchColor(piece.color) : piece.color;

                    if (effective_color == WhoseTurn) {
                        Debug.Log($"Valid move to {clickedBoardCell}");
                        bool moved = board.tryMovePiece(selectedBoardCell, clickedBoardCell);
                        if (moved) {
                            board.getPiece(clickedBoardCell).setTargetPosition(board.getCellPosition(clickedBoardCell));
                            didSomethingThatEndsTurn = true;
                        }
                    }
                }
            }
            // Reset visual position
            if (board.getPiece(selectedBoardCell.x, selectedBoardCell.y) != null) {
                board.getPiece(selectedBoardCell.x, selectedBoardCell.y).setTargetPosition(board.getCellPosition(selectedBoardCell.x, selectedBoardCell.y));
            }
            //do not show possible moves when no piece dragged anymore
            board.ClearSelectors();
        } else if (dragged == Dragged.Card) {
            // if dropped into card graveyard
            if (ifGraveyardHovered()) {
                if (whiteManaLeft < MAX_MANA) {
                    draggedCard.Destroy();
                    draggedCard = null; // free and remove the card
                    whiteManaLeft += 1;
                    UpdateManaDisplay();
                } else {
                    whiteHand.addCard(draggedCard); // return to the hand
                }
            } else {
                // try to apply
                bool validTargetCell = board.IsPosInBoardBounds(clickedBoardCell);

                if (validTargetCell) {
                    Debug.Log("card casted on" + clickedBoardCell);
                    DeckDef deckDef = DeckDef.Instance;
                    int appliedManaCost = deckDef.defs[draggedCard.Ability].Item2;

                    if (whiteManaLeft >= appliedManaCost) {
                        bool applied = draggedCard.tryApplyEffect(board, clickedBoardCell.x, clickedBoardCell.y, this);
                        if (applied) {
                            //only if applied
                            board.playAnimation(draggedCard.Ability, board.getCellPosition(clickedBoardCell));

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

    ivec2 ScreenToBoardPosition(Vector3 screenPosition) {
        Debug.Log($"BoardPosition:" + screenPosition.x + " " + screenPosition.y + " " + screenPosition.z);
        int x = Mathf.FloorToInt((screenPosition.x - boardLeftBottomPos.x) / singleCellScale);
        int y = Mathf.FloorToInt((screenPosition.z - boardLeftBottomPos.z) / singleCellScale);
        return new ivec2(x, y);
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
