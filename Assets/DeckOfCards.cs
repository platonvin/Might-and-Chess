using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DeckDef {
    List<KeyValuePair<CardAbility, int>> defs = new List<KeyValuePair<CardAbility, int>>();

    void defineDeck() {
        
        defs.Add(new KeyValuePair<CardAbility, int>(CardAbility.type1, 6));
    }

    void initDeck() {
        
    }
}

// also has sprite rendered
public class DeckOfCards {
    List<SingleCard> cards = new List<SingleCard>();

    void defineDeck() {
        
    }

    void initDeck() {
        
    }
}
