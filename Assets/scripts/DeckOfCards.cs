using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DeckDef {
    public Dictionary<CardAbility, (int, int)> defs = new Dictionary<CardAbility, (int, int)>();

    private static DeckDef _instance;

    // Property to access the singleton instance
    public static DeckDef Instance {
        get {
            if (_instance == null) {
                _instance = new DeckDef();
                _instance.DefineDeck(); // Initialize the deck definitions
            }
            return _instance;
        }
    }

    // Private constructor to prevent external instantiation
    private DeckDef() { }

    public void DefineDeck() {
        defs[CardAbility.Shift] = (4, 1);
        defs[CardAbility.Haste] = (2, 2);
        defs[CardAbility.Slow] = (3, 2);
        defs[CardAbility.Clone] = (3, 3);
        defs[CardAbility.Blind] = (2, 2);
        defs[CardAbility.Dispel] = (3, 1);
        defs[CardAbility.Armageddon] = (3, 7);
        defs[CardAbility.AntiMagic] = (3, 5);
        defs[CardAbility.MagicArrow] = (3, 2);
        // defs[CardAbility.Barricade] = 2;
        // defs[CardAbility.Wrap] = 1;
        // defs[CardAbility.Wind] = 1;
        // defs[CardAbility.ExtraMove] = 2;
        // defs[CardAbility.ExtendedCastling] = 1;
        // defs[CardAbility.RandomEffect] = 1;
        // defs[CardAbility.Resurrect] = 1;
    }
}

public class DeckOfCards {
    private List<Card> cards = new List<Card>();

    public void initDeck(Dictionary<CardAbility, GameObject> cardPrefabs, Vector3 initialPosition, GameObject manaPrefab, GameObject textPrefab) {
        DeckDef deckDef = DeckDef.Instance;

        foreach (var (ability, (count, manacost)) in deckDef.defs) {
            for (int i = 0; i < count; i++) {
                // Create unique instances of cards for each ability
                var card = new Card(ability, CardCastType.None, cardPrefabs[ability], initialPosition,
                                    manacost, "some fucking description", manaPrefab, textPrefab);
                card.flip();
                cards.Add(card);
            }
        }
    }

    public Card drawCard() {
        if (cards.Count > 0) {
            Card card = cards[0];
            cards.RemoveAt(0);
            return card;
        }
        return null;
    }

    public void shuffle() {
        for (int i = 0; i < cards.Count; i++) {
            Card temp = cards[i];
            int randomIndex = Random.Range(i, cards.Count);
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }
    }
}
