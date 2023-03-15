using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

using System;
using System.Linq;
using System.Collections.Generic;
using Object = UnityEngine.Object;

/// <summary>
/// An editor for the <see cref="TabSystem"/>.
/// <br>Allows for dynamic modification &amp; generation on inspector.</br>
/// </summary>
[CustomEditor(typeof(TabSystem)), CanEditMultipleObjects]
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

        // Set Unity Stuff
        Undo.RegisterCreatedObjectUndo(TSystem, string.Format("create {0}", TSystem.name));
        Selection.activeObject = TSystem;
    }

    private readonly List<Object> undoRecord = new List<Object>();
    /// <summary>
    /// Automatically registers Generation based events (basically tab system objects) when <paramref name="undoableGenerateAction"/> is invoked.
    /// </summary>
    protected void UndoRecordTabGeneration(Action undoableGenerateAction, string undoMsg, TabSystem target = null)
    {
        if (target == null)
            target = (TabSystem)base.target;

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName(undoMsg);
        int undoID = Undo.GetCurrentGroup();

        // Record previous state of 'target'
        undoRecord.Add(target);
        // to be destroyed / created SpriteRenderers gameobjects
        foreach (TabButton btn in target.TabButtons)
        {
            if (btn == null)
                continue;

            undoRecord.Add(btn.gameObject);
        }

        Undo.RecordObjects(undoRecord.ToArray(), string.Empty);

        undoableGenerateAction();

        foreach (var undoRegister in target.TabButtons.Where(sr => !undoRecord.Contains(sr)))
        {
            if (undoRegister == null)
                continue;

            Undo.RegisterCreatedObjectUndo(undoRegister.gameObject, string.Empty);
        }

        Undo.CollapseUndoOperations(undoID);
        undoRecord.Clear();
    }

    public override void OnInspectorGUI()
    {
        // Support multiple object editing
        var targets = base.targets.Cast<TabSystem>().ToArray();

        // PropertyField's (using SerializedObject) are already handled by CanEditMultipleObjects attribute
        // For manual GUI, we need to compensate.
        var tabSO = serializedObject;
        var gEnabled = GUI.enabled;
        var showMixed = EditorGUI.showMixedValue;
        tabSO.Update();

        // Draw the 'm_Script' field that unity makes (with disabled gui)
        GUI.enabled = false;
        EditorGUILayout.PropertyField(tabSO.FindProperty("m_Script"));
        GUI.enabled = gEnabled;

        EditorGUI.BeginChangeCheck();
        // Setup variables
        EditorGUILayout.LabelField("Standard Settings", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal(); // TabButtonAmount
        int tBtnAmountTest = targets[0].TabButtonAmount; // Get a test variable, for showing mixed view.
        EditorGUI.showMixedValue = targets.Any(ts => ts.TabButtonAmount != tBtnAmountTest);

        var TBtnAmount = EditorGUILayout.IntField(nameof(TabSystem.TabButtonAmount), tBtnAmountTest);
        if (GUILayout.Button("+", GUILayout.Width(20f))) { TBtnAmount++; }
        if (GUILayout.Button("-", GUILayout.Width(20f))) { TBtnAmount--; }

        EditorGUI.showMixedValue = showMixed;
        GUILayout.EndHorizontal();
        // Show warning if TabButtonAmount is 0 or lower.
        if (targets.Any(tb => tb.TabButtonAmount <= 0))
        {
            string disabledMsg = targets.Length > 1 ? "(Some) TabSystem(s) are disabled. " : "TabSystem is disabled.";
            disabledMsg += "To enable it again set TabButtonAmount to 1 or more.";

            EditorGUILayout.HelpBox(disabledMsg, MessageType.Warning);
        }

        int tReferenceBtnTest = targets[0].TabButtonAmount;
        EditorGUI.showMixedValue = targets.Any(ts => ts.TabButtonAmount != tBtnAmountTest);
        var CRefTB = EditorGUILayout.IntField(nameof(TabSystem.CurrentReferenceTabButton), tReferenceBtnTest);
        EditorGUI.showMixedValue = showMixed;

        bool tInteractableTest = targets[0].Interactable;
        EditorGUI.showMixedValue = targets.Any(ts => ts.Interactable != tInteractableTest);
        var TInteractable = EditorGUILayout.Toggle(nameof(TabSystem.Interactable), tInteractableTest);
        EditorGUI.showMixedValue = showMixed;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Fade Settings", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(TabSystem.ButtonFadeType)));
        FadeType tFadeTypeTest = targets[0].ButtonFadeType;
        bool isFadeTypeMixedValue = targets.Any(ts => ts.ButtonFadeType != tFadeTypeTest);

        // Button fade
        // Hide any button fade options if the types are different, otherwise show.
        if (!isFadeTypeMixedValue)
        {
            switch (tFadeTypeTest)
            {
                case FadeType.ColorFade:
                    EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(TabSystem.FadeSpeed)));
                    // Set the default color dynamically on the editor
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(TabSystem.FadeColorTargetDefault)));
                    if (EditorGUI.EndChangeCheck())
                    {
                        foreach (var target in targets)
                            target.UpdateButtonAppearances();
                    }
                    EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(TabSystem.FadeColorTargetHover)));
                    EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(TabSystem.FadeColorTargetClick)));
                    EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(TabSystem.FadeColorTargetDisabled)));
                    EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(TabSystem.FadeSubtractFromCurrentColor)));
                    break;
                case FadeType.SpriteSwap:
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(TabSystem.DefaultSpriteToSwap)));
                    if (EditorGUI.EndChangeCheck())
                    {
                        foreach (var target in targets)
                            target.UpdateButtonAppearances();
                    }
                    EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(TabSystem.HoverSpriteToSwap)));
                    EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(TabSystem.TargetSpriteToSwap)));
                    EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(TabSystem.DisabledSpriteToSwap)));
                    break;
                case FadeType.CustomUnityEvent:
                    EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(TabSystem.ButtonCustomEventOnReset)));
                    EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(TabSystem.ButtonCustomEventOnHover)));
                    EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(TabSystem.ButtonCustomEventOnClick)));
                    break;

                default:
                case FadeType.None:
                    break;
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tab Event", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(tabSO.FindProperty(nameof(TabSystem.OnTabButtonsClicked)));

        if (EditorGUI.EndChangeCheck())
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("change variable on TabSystem");
            int undoID = Undo.GetCurrentGroup();
            foreach (var target in targets)
            {
                // In TabSystem's case, we can just record undoing the tab system as the
                // target value is updated + objects are generated by the always executing (AlwaysExecute) TabSystem anyways
                Undo.RecordObject(target, string.Empty);

                // Apply properties
                if (target.TabButtonAmount != TBtnAmount)
                {
                    target.TabButtonAmount = TBtnAmount;
                }
                if (target.Interactable != TInteractable)
                {
                    target.Interactable = TInteractable;
                }

                target.CurrentReferenceTabButton = CRefTB;
            }

            // Apply serializedObject
            Undo.CollapseUndoOperations(undoID);
            tabSO.ApplyModifiedProperties();
        }

        // -- Tab List Actions
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tab List", EditorStyles.boldLabel);

        EditorGUI.indentLevel++; // indentLevel = normal + 1
        GUI.enabled = false;
        // Apparently CanEditMultipleObjects ReorderableList has issues while drawing properties (keeps spamming you should stop calling next)
        // Most likely it uses serializedProperty.arraySize instead of iterating properly so we have to ditch the view if there's more than 2 views
        if (targets.Length <= 1)
            EditorGUILayout.PropertyField(tabSO.FindProperty("tabButtons"));

        GUI.enabled = true;

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear Tabs"))
        {
            foreach (var target in targets)
                target.ClearTabs();
        }
        if (GUILayout.Button("Generate Tabs"))
        {
            foreach (var target in targets)
                target.GenerateTabs();
        }
        if (GUILayout.Button("Reset Tabs"))
        {
            foreach (var target in targets)
                target.ResetTabs();
        }
        GUILayout.EndHorizontal();

        EditorGUI.indentLevel--; // indentLevel = normal
    }
}
