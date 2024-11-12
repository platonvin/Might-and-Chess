using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HandOfCards {
    private List<Card> cards = new List<Card>();

    public Vector3 Center = new Vector3(0, 2.5f, -68); // center position of the hand
    public float ShownRadius = 22.0f; // radius when cards are visible
    public float HiddenRadius = 20.0f; // radius when cards are hidden
    public float CardSpacing = 10.66f; // base distance between cards
    public float RadialAngle = 15.0f; // base degree to spread cards radially
    public float HoverHeightOffset = 3.0f; // additional z height when card is hovered
    public float HoverRadiusOffset = 5.0f;
    public float AnimationSpeed = 2.0f; // smoothing factor for position interpolation

    private float[] targetAngles;
    private float[] targetRadii;
    private float[] visualAngles;
    private float[] visualRadii;

    private int hoveredCardIndex = -1;
    public bool wholeHandHovered = false;

    public void addCard(Card card) {
        Debug.Assert(card != null);
        cards.Add(card);
        initArrays();
        updateHowHandDisplayed();
    }

    private void initArrays() {
        int count = cards.Count;
        Array.Resize<float>(ref targetAngles, count);
        Array.Resize<float>(ref targetRadii, count);
        Array.Resize<float>(ref visualAngles, count);
        Array.Resize<float>(ref visualRadii, count);
    }

    public void removeCard(int id) {
        Debug.Assert(id >= 0 && id < cards.Count);
        cards[id].Destroy();
        cards.RemoveAt(id);
        updateHowHandDisplayed();
    }

    public Card drawCard(int id) {
        Debug.Assert(id >= 0 && id < cards.Count);
        Card card = cards[id];
        cards.RemoveAt(id);
        updateHowHandDisplayed();
        return card;
    }

    public void updateHowHandDisplayed() {
        float deltaTime = Time.deltaTime;

        int cardCount = cards.Count;
        float midIndex = (cardCount - 1) / 2f;

        // Determine the base radius based on whole-hand hover state
        float baseRadius = wholeHandHovered ? ShownRadius : HiddenRadius;

        for (int i = 0; i < cardCount; i++) {
            // Calculate the base angle offset for even distribution
            float baseAngleOffset = (i - midIndex - 0.5f) * RadialAngle - RadialAngle / 2.0f;

            // Set target radius for hovered card and adjust angles
            if (i == hoveredCardIndex) {
                // Apply hover radius for the focused card
                targetRadii[i] = baseRadius + HoverRadiusOffset;
                targetAngles[i] = baseAngleOffset;
            } else {
                // Cards near the hovered card are angularly "spread" away from the hovered card
                float spacingFactor = (hoveredCardIndex != -1) ? 1.5f : 1.0f; // Adjust angular spread factor if hovered

                // Adjust target radius and angle based on proximity to the hovered card
                targetRadii[i] = baseRadius;
                if (i < hoveredCardIndex) {
                    // Move cards to the left of the hovered card further left
                    targetAngles[i] = baseAngleOffset - RadialAngle * spacingFactor;
                } else if (i > hoveredCardIndex) {
                    // Move cards to the right of the hovered card further right
                    targetAngles[i] = baseAngleOffset + RadialAngle * spacingFactor;
                } else {
                    // No special adjustment if no card is hovered
                    targetAngles[i] = baseAngleOffset;
                }
            }

            // Smoothly interpolate visual positions for smooth animations
            float interpolation_speed = 1 - Mathf.Exp(-AnimationSpeed * deltaTime);
            interpolation_speed = Mathf.Clamp01(interpolation_speed + 0.015f);

            visualAngles[i] = Mathf.Lerp(visualAngles[i], targetAngles[i], interpolation_speed);
            visualRadii[i] = Mathf.Lerp(visualRadii[i], targetRadii[i], interpolation_speed);

            // Convert polar coordinates to Cartesian for positioning
            float angleInRadians = visualAngles[i] * Mathf.Deg2Rad;
            Vector3 cardPosition = Center + new Vector3(
                Mathf.Sin(angleInRadians) * visualRadii[i],
                0,
                Mathf.Cos(angleInRadians) * visualRadii[i]
            );

            // Slight vertical offset for card stacking effect
            cardPosition.y = 2.5f + i * 0.1f;

            // Update card's position smoothly
            cards[i].SetPosition(cardPosition);
        }
    }




    public int findSelected() {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        LayerMask layerMask = LayerMask.GetMask("Cards");

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)) {
            // Loop through cards to find the one that was hit
            for (int i = 0; i < cards.Count; i++) {
                GameObject cardObject = cards[i].Instance;

                // Check if the hit collider's game object is a child of this card
                if (hit.collider.transform.IsChildOf(cardObject.transform)) {
                    // Debug.Log($"Card {i} is SELECTED.");
                    hoveredCardIndex = i;
                    return hoveredCardIndex; // Return the index of the selected card
                }
            }
        }

        hoveredCardIndex = -1;
        return hoveredCardIndex; // Return -1 if no card was hit
    }

}
