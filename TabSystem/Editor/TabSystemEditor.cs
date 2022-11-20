using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(TabSystem))]
internal class TabSystemEditor : Editor
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
        CreatedTabSystem.CreateTab();

        // Resize stuff accordingly.
        // Width -- Height
        RectTransform TSystemTransform = TSystem.GetComponent<RectTransform>();
        TSystemTransform.sizeDelta = new Vector2(200, 100);
        #endregion

        // Set Unity Stuff
        Undo.RegisterCreatedObjectUndo(TSystem, string.Format("Create {0}", TSystem.name));
        Selection.activeObject = TSystem;
    }

    public override void OnInspectorGUI()
    {
        // Standard
        var Target = (TabSystem)target;
        var tabSO = serializedObject;
        tabSO.Update();

        // Draw the 'm_Script' field that monobehaviour makes (with disabled gui)
        var gEnabled = GUI.enabled;
        GUI.enabled = false;
        EditorGUILayout.PropertyField(tabSO.FindProperty("m_Script"));
        GUI.enabled = gEnabled;

        EditorGUI.BeginChangeCheck();

        // Setup variables
        EditorGUILayout.LabelField("Standard Settings", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal(); // TabButtonAmount
        var TBtnAmount = EditorGUILayout.IntField(nameof(Target.TabButtonAmount), Target.TabButtonAmount);
        if (GUILayout.Button("+", GUILayout.Width(20f))) { TBtnAmount++; }
        if (GUILayout.Button("-", GUILayout.Width(20f))) { TBtnAmount--; }
        GUILayout.EndHorizontal();
        // Show warning if TabButtonAmount is 0 or lower.
        if (TBtnAmount <= 0)
            EditorGUILayout.HelpBox("TabSystem is disabled. To enable it again set TabButtonAmount to 1 or more.", MessageType.Warning);

        EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(Target.ButtonFadeType)));
        var CRefTB = EditorGUILayout.IntField(nameof(Target.CurrentReferenceTabButton), Target.CurrentReferenceTabButton);

        var TInteractable = EditorGUILayout.Toggle("Interactable", Target.Interactable);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Fade Settings", EditorStyles.boldLabel);
        // Button fade
        switch (Target.ButtonFadeType)
        {
            case FadeType.ColorFade:
                EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(Target.FadeSpeed)));
                // Set the default color dynamically on the editor
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(Target.FadeColorTargetDefault)));
                if (EditorGUI.EndChangeCheck())
                {
                    Target.UpdateButtonAppearances();
                }
                EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(Target.FadeColorTargetHover)));
                EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(Target.FadeColorTargetClick)));
                EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(Target.FadeColorTargetDisabled)));
                EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(Target.FadeSubtractFromCurrentColor)));
                break;
            case FadeType.SpriteSwap:
                EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(Target.DefaultSpriteToSwap)));
                EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(Target.HoverSpriteToSwap)));
                EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(Target.TargetSpriteToSwap)));
                EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(Target.DisabledSpriteToSwap)));
                break;
            case FadeType.CustomUnityEvent:
                EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(Target.ButtonCustomEventOnReset)));
                EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(Target.ButtonCustomEventOnHover)));
                EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(Target.ButtonCustomEventOnClick)));
                break;

            default:
            case FadeType.None:
                break;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tab Event", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(Target.OnTabButtonsClicked)));

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(Target, string.Format("Change variable on TabSystem {0}", Target.name));

            // Apply properties
            if (Target.TabButtonAmount != TBtnAmount)
            {
                Target.TabButtonAmount = TBtnAmount;
            }
            if (Target.Interactable != TInteractable)
            {
                Target.Interactable = TInteractable;
            }

            Target.CurrentReferenceTabButton = CRefTB;

            // Apply serializedObject
            tabSO.ApplyModifiedProperties();
        }

        // -- Tab List Actions
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tab List", EditorStyles.boldLabel);

        EditorGUI.indentLevel++; // indentLevel = normal + 1
        GUI.enabled = false;
        EditorGUILayout.PropertyField(tabSO.FindProperty("TabButtons"));
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
