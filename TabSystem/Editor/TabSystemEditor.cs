using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

[CustomEditor(typeof(TabSystem))]
public class TabSystemEditor : Editor
{
    //////////// Object Creation
    [MenuItem("GameObject/UI/Tab System")]
    public static void CreateTabSystem(MenuCommand Command)
    {
        // Create primary gameobject.
        GameObject TSystem = new GameObject("Tab System");

        // Align stuff
        GameObjectUtility.SetParentAndAlign(TSystem, (GameObject)Command.context);

        #region Creation
        // Add components here... (Also create tab button)

        // TabSystem on empty object.
        TabSystem CreatedTabSystem = TSystem.AddComponent<TabSystem>();
        // Layout group
        HorizontalLayoutGroup TabSystemLayoutGroup = TSystem.AddComponent<HorizontalLayoutGroup>();
        TabSystemLayoutGroup.childControlHeight = true;
        TabSystemLayoutGroup.childControlWidth = true;
        TabSystemLayoutGroup.spacing = 10f;
        // Tab Button
        _ = CreatedTabSystem.CreateTab();

        // Resize stuff accordingly.
        // Width -- Height
        RectTransform TSystemTransform = TSystem.GetComponent<RectTransform>();
        TSystemTransform.sizeDelta = new Vector2(200, 100);
        #endregion

        // Set Unity Stuff
        Undo.RegisterCreatedObjectUndo(TSystem, "Create " + TSystem.name);
        Selection.activeObject = TSystem;
    }

    public override void OnInspectorGUI()
    {
        // Standard
        var Target = (TabSystem)target;
        var Tso = serializedObject;

        Tso.Update();
        EditorGUI.BeginChangeCheck();
        
        // Setup variables
        EditorGUILayout.LabelField("Standard Settings", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal(); // TabButtonAmount
        var TBtnAmount = EditorGUILayout.IntField(nameof(Target.TabButtonAmount), Target.TabButtonAmount);
        if (GUILayout.Button("+", GUILayout.Width(20f))) { TBtnAmount++; }
        if (GUILayout.Button("-", GUILayout.Width(20f))) { TBtnAmount--; }
        GUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(Tso.FindProperty(nameof(Target.ButtonFadeType)));
        var CRefTB = EditorGUILayout.IntField(nameof(Target.CurrentReferenceTabButton), Target.CurrentReferenceTabButton);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Fade Settings", EditorStyles.boldLabel);
        // Button fade
        switch (Target.ButtonFadeType)
        {
            case FadeType.ColorFade:
                EditorGUILayout.PropertyField(Tso.FindProperty(nameof(Target.TabButtonFadeSpeed)));
                EditorGUILayout.PropertyField(Tso.FindProperty(nameof(Target.TabButtonFadeColorTargetHover)));
                EditorGUILayout.PropertyField(Tso.FindProperty(nameof(Target.TabButtonFadeColorTargetClick)));
                EditorGUILayout.PropertyField(Tso.FindProperty(nameof(Target.TabButtonSubtractFromCurrentColor)));
                break;
            case FadeType.SpriteSwap:
                EditorGUILayout.LabelField(
                    "Note : Default sprite is the image that is currently inside the Image component.",
                    EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.PropertyField(Tso.FindProperty(nameof(Target.HoverSpriteToSwap)));
                EditorGUILayout.PropertyField(Tso.FindProperty(nameof(Target.TargetSpriteToSwap)));
                break;
            case FadeType.CustomUnityEvent:
                EditorGUILayout.PropertyField(Tso.FindProperty(nameof(Target.TabButtonCustomEventOnReset)));
                EditorGUILayout.PropertyField(Tso.FindProperty(nameof(Target.TabButtonCustomEventHover)));
                EditorGUILayout.PropertyField(Tso.FindProperty(nameof(Target.TabButtonCustomEventClick)));
                break;

            default:
            case FadeType.None:
                break;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tab Event", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(Tso.FindProperty(nameof(Target.OnTabButtonsClicked)));

        if (EditorGUI.EndChangeCheck())
        {
            // Apply properties
            if (Target.TabButtonAmount != TBtnAmount)
            {
                Target.TabButtonAmount = TBtnAmount;
            }

            Target.CurrentReferenceTabButton = CRefTB;

            // Apply serializedObject
            Tso.ApplyModifiedProperties();
            Undo.RegisterCompleteObjectUndo(Target, $"Change variable on TabSystem {Target.name}");
        }

        // -- Tab List Actions
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tab List", EditorStyles.boldLabel);
        
        EditorGUI.indentLevel++; // indentLevel = normal + 1
        GUI.enabled = false;
        EditorGUILayout.PropertyField(Tso.FindProperty("TabButtons"));
        GUI.enabled = true;

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear Tabs"))
        {
            Target.ClearTabs();
        }
        if (GUILayout.Button("Generate Tabs"))
        {
            Target.GenerateTabs();
        }
        if (GUILayout.Button("Reset Tabs"))
        {
            Target.ResetTabs();
        }
        GUILayout.EndHorizontal();

        EditorGUI.indentLevel--; // indentLevel = normal
    }
}
