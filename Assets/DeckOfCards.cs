using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DeckDef {
    public Dictionary<CardAbility, int> defs = new Dictionary<CardAbility, int>();

    public void Define() {
        DefineDeck();
    }

    public void DefineDeck() {
        defs[CardAbility.Shift] = 2;
        defs[CardAbility.Haste] = 1;
        defs[CardAbility.Slow] = 1;
        // defs[CardAbility.Clone] = 1;
        // defs[CardAbility.Barricade] = 2;
        // defs[CardAbility.Wrap] = 1;
        // defs[CardAbility.Wind] = 1;
        // defs[CardAbility.ExtraMove] = 2;
        // defs[CardAbility.ExtendedCastling] = 1;
        // defs[CardAbility.RandomEffect] = 1;
        // defs[CardAbility.Sleep] = 2;
        // defs[CardAbility.Heal] = 3;
        // defs[CardAbility.Resurrect] = 1;
    }
}

public class DeckOfCards {
    private List<Card> cards = new List<Card>();

    public void InitializeDeck(Dictionary<CardAbility, GameObject> cardPrefabs, Vector3 initialPosition) {
        DeckDef deckDef = new DeckDef();
        deckDef.Define();

        foreach (var (ability, count) in deckDef.defs) {
            for (int i = 0; i < count; i++) {
                // Create unique instances of cards for each ability
                var card = new Card(ability, CardCastType.None, 0, cardPrefabs[ability], initialPosition);
                cards.Add(card);
                card.flip();
            }
        }
    }

    public Card DrawCard() {
        if (cards.Count > 0) {
            Card card = cards[0];
            cards.RemoveAt(0);
            return card;
        }
        return null;
    }

    public void Shuffle() {
        for (int i = 0; i < cards.Count; i++) {
            Card temp = cards[i];
            int randomIndex = Random.Range(i, cards.Count);
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }
    }
}
