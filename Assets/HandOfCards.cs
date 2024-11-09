using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HandOfCards {
    private List<Card> cards = new List<Card>();
    private Vector3 HiddenPosition = new Vector3(0, 2.5f, -22); // offscreen, hidden position
    private Vector3 VisiblePosition = new Vector3(0, 2.5f, -18); // onscreen, visible position
    
    private Vector3 position = new Vector3(0, 2.5f, -22); // onscreen, visible position
    private float cardSpacing = 1.5f; // distance between cards in the hand
    private float radialAngle = 15f; // degree to spread cards radially
    private float hoverHeight = -16; // additional z height when card is hovered
    private float animationSpeed = 0.2f; // speed for smooth animations
    private int hoveredCardIndex = -1;

    public bool wholeHandHovered = false;

    public void AddCard(Card card) {
        Debug.Assert(card != null);
        cards.Add(card);
        UpdateHandDisplay();
    }

    public void RemoveCard(int id) {
        Debug.Assert(id >= 0 && id < cards.Count);
        cards[id].Destroy();
        cards.RemoveAt(id);
        UpdateHandDisplay();
    }

    public Card DrawCard(int id) {
        Debug.Assert(id >= 0 && id < cards.Count);
        Card card = cards[id];
        cards.RemoveAt(id);
        UpdateHandDisplay();
        return card;
    }

    public void UpdateHandDisplay() {
        float midIndex = (cards.Count - 1) / 2f;
        for (int i = 0; i < cards.Count; i++) {
            // Calculate radial spread
            float angleOffset = (i - midIndex) * radialAngle;
            Vector3 cardPosition = wholeHandHovered? VisiblePosition : HiddenPosition; 
            cardPosition += Quaternion.Euler(0, 0, angleOffset) * new Vector3(i * cardSpacing, 0, 0);

            // Set slightly forward position if card is hovered
            if (i == hoveredCardIndex) {
                cardPosition.z = hoverHeight;
            }

            Debug.Assert(cards[i] != null);
            cards[i].SetPosition(cardPosition);
            // StartCoroutine(MoveToPosition(cards[i], cardPosition));
        }
    }

    public void OnMouseEnter() {
        // StartCoroutine(SlideToPosition(VisiblePosition));
        SlideToPosition(VisiblePosition);
    }

    public void OnMouseExit() {
        // StartCoroutine(SlideToPosition(HiddenPosition));
        SlideToPosition(HiddenPosition);
    }

    private IEnumerator SlideToPosition(Vector3 targetPosition) {
        while (Vector3.Distance(position, targetPosition) > 0.01f) {
            position = Vector3.Lerp(position, targetPosition, animationSpeed);
            UpdateHandDisplay(); // update display as position changes
            yield return null;
        }
    }

    private void Update() {
        if (hoveredCardIndex != -1) {
            UpdateHandDisplay(); // Refresh positions to handle smooth hover effect
        }
    }

    private void OnMouseOver() {
        // Get the currently hovered card
        Vector3 mousePosition = Input.mousePosition;
        for (int i = 0; i < cards.Count; i++) {
            if (IsMouseOverCard(cards[i], mousePosition)) {
                hoveredCardIndex = i;
                UpdateHandDisplay();
                return;
            }
        }
        hoveredCardIndex = -1; // Reset hover if no card is hovered
    }

    private bool IsMouseOverCard(Card card, Vector3 mousePosition) {
        // Use raycasting or bounding box check to see if the mouse is over a card
        return card.Instance.GetComponent<Collider2D>().bounds.Contains(Camera.main.ScreenToWorldPoint(mousePosition));
    }
}
