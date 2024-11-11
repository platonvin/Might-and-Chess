using UnityEditor;
using UnityEngine;

#if (UNITY_EDITOR) 
public class PiecesSetup : EditorWindow {
    public GameObject pawnPrefab; // Drag the correctly scaled Pawn GameObject here
    public Texture[] pieceTextures; // Drag all chess piece textures here

    [MenuItem("Tools/Chess Piece Setup")]
    public static void ShowWindow() {
        GetWindow<PiecesSetup>("Chess Piece Setup");
    }

    private void OnGUI() {
        GUILayout.Label("Chess Piece Setup", EditorStyles.boldLabel);

        pawnPrefab = (GameObject)EditorGUILayout.ObjectField("Pawn Prefab", pawnPrefab, typeof(GameObject), true);
        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty texturesProperty = serializedObject.FindProperty("pieceTextures");

        EditorGUILayout.PropertyField(texturesProperty, true);
        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Generate Chess Pieces")) {
            GenerateChessPieces();
        }
    }

    private void GenerateChessPieces() {
        if (pawnPrefab == null || pieceTextures == null || pieceTextures.Length == 0) {
            Debug.LogError("Please assign the Pawn prefab and all piece textures.");
            return;
        }

        foreach (Texture texture in pieceTextures) {
            // Create a new instance of the pawn prefab
            GameObject piece = Instantiate(pawnPrefab);

            // Rename it to match the texture name
            piece.name = texture.name;

            // Apply the texture to the piece's material
            Renderer renderer = piece.GetComponent<Renderer>();
            if (renderer != null) {
                Material material = new Material(renderer.sharedMaterial);
                material.mainTexture = texture;
                renderer.sharedMaterial = material;
            }

            // Save it as a prefab in the "Assets/Prefabs" folder
            string prefabPath = $"Assets/Prefabs/{texture.name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(piece, prefabPath);

            // Destroy the temporary piece in the scene
            DestroyImmediate(piece);
        }

        Debug.Log("Chess pieces generated successfully!");
    }
}
#endif