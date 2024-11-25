using System.Collections.Generic;
using UnityEngine;

// also has sprite rendered
public class ChessPiece {
    public PieceType type;
    public PieceColor color;
    public GameObject Instance;
    private Vector3 visualPosition;
    private Vector3 targetPosition;
    private Transform statusContainer;
    Quaternion rotation = Quaternion.LookRotation(new Vector3(0f, -1f, 0f));

    public int speedLevel = 0; //[-1; +1]
    public int speedleft = 0; //[-1; +1]
    public int sleepLeft = 0;
    //controls clone lifetime
    public int timeInTurnsLeft = -1;
    public bool isClone = false;
    public int antiMagicLeft = 0;
    public int weaknessLeft = 0;
    public int flightLeft = 0;
    public int controlledByOpponentTurnsLeft = 0;
    public bool hasMovedAtAll = false;
    Dictionary<CardAbility, GameObject> iconPrefabs;

    public ChessPiece(PieceType type, PieceColor color,
                      GameObject prefab,
                      Dictionary<CardAbility, GameObject> iconPrefabs,
                      Vector3 initialPosition) {
        this.type = type;
        this.color = color;
        visualPosition = initialPosition;
        targetPosition = initialPosition;
        this.iconPrefabs = iconPrefabs;
        Instance = GameObject.Instantiate(prefab, initialPosition, Quaternion.LookRotation(new Vector3(0f, -1f, 0f)));

        // Create a container for status effect icons
        GameObject statusContainerObject = new GameObject("StatusContainer");
        statusContainerObject.transform.SetParent(Instance.transform);
        statusContainer = statusContainerObject.transform;
        statusContainer.localPosition = new(-0.1f, 0.1f, -0.001f);
        statusContainer.localRotation = Quaternion.identity;
        updateTransform();
    }

    public Vector3 getVisualPosition() => visualPosition;

    // sets where it is seen immediately
    public void setVisualPosition(Vector3 newPosition) {
        visualPosition = newPosition;
        updateTransform();
    }

    // smooth
    public void setTargetPosition(Vector3 newPosition) {
        targetPosition = newPosition;
        updateTransform();
    }

    // call me often please
    public void updateTransform() {
        float deltaTime = Time.deltaTime;
        float AnimationSpeed = 5.0f;

        float interpolation_speed = 1 - Mathf.Exp(-AnimationSpeed * deltaTime);
        interpolation_speed = Mathf.Clamp01(interpolation_speed + 0.05f);

        visualPosition = Vector3.Lerp(visualPosition, targetPosition, interpolation_speed);

        if (Instance != null) {
            var adjustedPosition = visualPosition;
            if (type == PieceType.Barricade) {
                adjustedPosition += new Vector3(0, 0, 3);
            }
            Instance.transform.SetPositionAndRotation(adjustedPosition, rotation);
        }
    }

    public void destroy() {
        if (Instance != null) {
            GameObject.Destroy(Instance);
        }
    }

    public void setDeadVisually() {
        Instance.transform.localScale = new(16, 16, 16);
        rotation = Quaternion.LookRotation(new Vector3(0f, -1f, 0f));
        rotation *= Quaternion.Euler(0, 0, 90); // rotate a little bit
        visualPosition.y -= 0.001f;
        updateTransform();
    }

    public void setAliveVisually() {
        Instance.transform.localScale = new(32, 32, 32);
        rotation = Quaternion.LookRotation(new Vector3(0f, -1f, 0f));
        visualPosition.y += 0.001f;
        updateTransform();
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
        if (weaknessLeft > 0) activeEffects.Add(CardAbility.Weakness);
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