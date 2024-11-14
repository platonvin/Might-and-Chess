using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// also has sprite rendered
public class ChessPiece {
    public PieceType Type;
    public PieceColor Color;
    public GameObject Instance;
    private Vector3 visualPosition;
    private Transform statusContainer;
    Quaternion rotation = Quaternion.LookRotation(new Vector3(0f, -1f, 0f));

    public int speedLevel = 0; //[-1; +1]
    public int speedleft = 0; //[-1; +1]
    public int sleepLeft = 0;
    //-1 means forever. 0 means is removed right now
    public int timeInTurnsLeft = -1;
    public bool isClone = false;
    public int antiMagicLeft = 0;
    public int controlledByOpponentTurnsLeft = 0;
    public bool hasMovedAtAll = false;
    Dictionary<CardAbility, GameObject> iconPrefabs;
    // Dictionary<CardAbility, GameObject> animationPrefabs;

    public ChessPiece(PieceType type, PieceColor color, 
                      GameObject prefab, 
                      Dictionary<CardAbility, GameObject> iconPrefabs, 
                    //   Dictionary<CardAbility, GameObject> animationPrefabs, 
                      Vector3 initialPosition) {
        Type = type;
        Color = color;
        visualPosition = initialPosition;
        this.iconPrefabs = iconPrefabs;
        // this.animationPrefabs = animationPrefabs;
        Instance = GameObject.Instantiate(prefab, initialPosition, Quaternion.LookRotation(new Vector3(0f, -1f, 0f)));

        // Create a container for status effect icons
        GameObject statusContainerObject = new GameObject("StatusContainer");
        statusContainerObject.transform.SetParent(Instance.transform);
        statusContainer = statusContainerObject.transform;
        statusContainer.localPosition = new(-0.1f, 0.1f, -0.001f); // Adjust position as needed
        statusContainer.localRotation = Quaternion.identity;
    }

    public Vector3 GetVisualPosition() => visualPosition;

    public void SetVisualPosition(Vector3 newPosition) {
        visualPosition = newPosition;
        UpdateTransform();
    }

    public void UpdateTransform() {
        if (Instance != null) {
            Instance.transform.SetPositionAndRotation(visualPosition, rotation);
        }
    }

    public void Destroy() {
        if (Instance != null) {
            GameObject.Destroy(Instance);
        }
    }

    public void setDeadVisually(){
        Instance.transform.localScale = new (16,16,16);
        rotation = Quaternion.LookRotation(new Vector3(0f, -1f, 0f));
        rotation *= Quaternion.Euler(0,0,90); // rotate a little bit
        visualPosition.y -= 0.001f;
        UpdateTransform();
    }
    
    public void setAliveVisually(){
        Instance.transform.localScale = new (32,32,32);
        rotation = Quaternion.LookRotation(new Vector3(0f, -1f, 0f));
        visualPosition.y += 0.001f;
        UpdateTransform();
    }

    public void updateEffectsDisplay() {
        // Clear existing icons
        foreach (Transform child in statusContainer) {
            GameObject.Destroy(child.gameObject);
        }

        // list of active effects
        List<CardAbility> activeEffects = new();

        // ordered, yeah
        if (speedLevel < 0) activeEffects.Add(CardAbility.Slow);
        if (speedLevel > 0) activeEffects.Add(CardAbility.Haste);
        if (isClone) activeEffects.Add(CardAbility.Clone);
        if (antiMagicLeft > 0) activeEffects.Add(CardAbility.AntiMagic);
        if (sleepLeft > 0) activeEffects.Add(CardAbility.Blind);
        if (controlledByOpponentTurnsLeft > 0) activeEffects.Add(CardAbility.Hypnotize);

        // Display each active effect icon in the status container
        float iconSpacing = -3.0f;
        for (int i = 0; i < activeEffects.Count; i++) {
            CardAbility ability = activeEffects[i];
            if (iconPrefabs.TryGetValue(ability, out GameObject iconPrefab)) {
                GameObject iconInstance = GameObject.Instantiate(iconPrefab, statusContainer);
                iconInstance.transform.localPosition = new(0, i * iconSpacing, -0.001f);
            }
        }
    }
}

public enum PieceType {
    None,
    Pawn, Rook, Knight, Bishop, Queen, King,
    Barricade,
}

public enum PieceColor {
    None,
    White, Black,
    Neutral,
}