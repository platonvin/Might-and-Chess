using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// also has sprite rendered
public class HandOfCards {
    List<SingleCard> cards = new List<SingleCard>();

    void removeCard(int id) {
        cards[id] = null;
    }

    void addCard(SingleCard card){
        cards.Add(card);
    }

    SingleCard getCard(int id) {
        return cards[id];
    }

    // returns ptr to card and removes from hand
    SingleCard drawCard(int id) {
        SingleCard card = cards[id];
        cards[id] = null; // ownership is moved
        return card;
    }
}
