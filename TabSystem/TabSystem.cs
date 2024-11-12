using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// The fading type of TabButton.
/// </summary>
/// <br>Instead of using an <see cref="Selectable"/>, this was the solution I resorted to like 1(?) year ago.</br>
/// However, newer solutions that i will do will break compat.
public enum FadeType {
    None,
    ColorFade,
    SpriteSwap,
    CustomUnityEvent
}

/// <summary>
/// The tab system itself.
/// <br>Allows to control the buttons and events, this is the main component that manages the of the <see cref="TabButton"/>s.</br>
/// </summary>
[ExecuteAlways, DisallowMultipleComponent]
public class TabSystem : UIBehaviour {
    /// <summary>
    /// An unity event that takes an int parameter.
    /// </summary>
    [Serializable]
    public class IntUnityEvent : UnityEvent<int> { }
    /// <summary>
    /// An unity event that takes a tab button parameter (with an index).
    /// </summary>
    [Serializable]
    public class TabButtonUnityEvent : UnityEvent<int, TabButtonC> { }

    ///////////// Public
    /// <summary>
    /// The amount of the tab buttons. 0 means disabled.
    /// </summary>
    public int TabButtonAmount {
        get {
            return _TabButtonAmount;
        }
        set {
            int prevValue = _TabButtonAmount;
            // The weird value is because that the 'TabButtonAmount' will kill your pc if not clampped.
            _TabButtonAmount = Mathf.Clamp(value, 0, ushort.MaxValue);
            if (prevValue != value) // Generate if value is changed
            {
                GenerateTabs(prevValue);
            }
        }
    }
    [SerializeField] private int _TabButtonAmount = 1;

    /// <summary>
    /// The index of the currently referenced tab button.
    /// </summary>
    public int ReferenceTabButtonIndex {
        get {
            // Also clamp the return as that's necessary to protect sanity
            // (Note : clamp with TabButtons.Count as that's the actual button amount).
            return Mathf.Clamp(_ReferenceTabButtonIndex, 0, tabButtons.Count - 1);
        }
        set {
            if (_ReferenceTabButtonIndex == value) {
                return;
            }

            _ReferenceTabButtonIndex = Mathf.Clamp(value, 0, tabButtons.Count - 1);
        }
    }
    [SerializeField, FormerlySerializedAs("_CurrentReferenceTabButton")] private int _ReferenceTabButtonIndex = 0;

    /// <summary>
    /// The index of the currently selected tab button.
    /// </summary>
    public int SelectedTabIndex {
        get {
            return GetSelectedButtonIndex();
        }
        set {
            SetSelectedButtonIndex(value);
        }
    }
    [SerializeField] private int _SelectedTabIndex = 0;

    // -- Fade Styles
    /// <summary>
    /// Manages the button fading type.
    /// <br/>
    /// <br><see cref="FadeType.None"/> : None of the '<see cref="Color"/>/<see cref="Sprite"/>/<see cref="TabButtonC.ButtonTransitionEvent"/>'s are used.</br>
    /// <br><see cref="FadeType.ColorFade"/> : Fades the <see cref="Color"/> using the variables prefixed with 'FadeColor'.</br>
    /// <br><see cref="FadeType.SpriteSwap"/> : Changes the <see cref="Sprite"/> of the tab buttons using the variables prefixed with 'SpriteTarget'.</br>
    /// <br><see cref="FadeType.CustomUnityEvent"/> : Calls the <see cref="TabButtonC.ButtonTransitionEvent"/> on each interacted button using the variables prefixed with 'ButtonCustomEvent'.</br>
    /// </summary>
    public FadeType ButtonFadeType = FadeType.ColorFade;
    // ButtonFadeType = ColorFade
    /// <summary>
    /// Time (in seconds) used to fade transition a button to another color.
    /// </summary>
    [FormerlySerializedAs("FadeSpeed"), Range(0f, 4f)] public float FadeColorDuration = 0.15f;
    [FormerlySerializedAs("FadeColorTargetDefault")] public Color FadeColorTargetOnDefault = new Color(1f, 1f, 1f);
    [FormerlySerializedAs("FadeColorTargetHover")] public Color FadeColorTargetOnHover = new Color(.95f, .95f, .95f);
    [FormerlySerializedAs("FadeColorTargetClick")] public Color FadeColorTargetOnClick = new Color(.9f, .9f, .9f);
    [FormerlySerializedAs("FadeColorTargetDisabled")] public Color FadeColorTargetOnDisabled = new Color(.5f, .5f, .5f, .5f);
    /// <summary>
    /// Whether if the target colors subtract from the previous color of the button.
    /// <br>Basically a <see cref="FadeType.ColorFade"/> transition with this being <see langword="false"/> is done 
    /// by directly making the background's color to the target state's <see cref="Color"/>.</br>
    /// <br>If this value is <see langword="true"/>, the transition color is calculated as : <c>previousButtonColor - targetStateColor</c></br>
    /// </summary>
    public bool FadeSubtractsFromCurrentColor = false;
    // ButtonFadeType = SpriteSwap
    [FormerlySerializedAs("DefaultSpriteToSwap")] public Sprite SpriteTargetOnDefault;
    [FormerlySerializedAs("HoverSpriteToSwap")] public Sprite SpriteTargetOnHover;
    [FormerlySerializedAs("TargetSpriteToSwap")] public Sprite SpriteTargetOnClick;
    [FormerlySerializedAs("DisabledSpriteToSwap")] public Sprite SpriteTargetOnDisabled;
    // ButtonFadeType = CustomUnityEvent
    public TabButtonC.ButtonTransitionEvent ButtonCustomEventOnReset;
    public TabButtonC.ButtonTransitionEvent ButtonCustomEventOnHover;
    public TabButtonC.ButtonTransitionEvent ButtonCustomEventOnClick;
    public TabButtonC.ButtonTransitionEvent ButtonCustomEventOnDisable;

    // -- Standard event
    // This variable is added to take more control of the generation of the buttons.
    /// <summary>
    /// Called when a tab button is created.
    /// <br><see langword="int"/> parameter : Returns the index.</br> 
    /// <br><see cref="TabButtonC"/> parameter : Returns the created button.</br>
    /// </summary>
    [FormerlySerializedAs("OnTabButtonsClicked")] public TabButtonUnityEvent OnTabButtonCreated;
    /// <summary>
    /// Called when a tab button is clicked on the tab system.
    /// </summary>
    public IntUnityEvent OnTabButtonClicked;

    /// <inheritdoc cref="SelectedTab"/>
    private TabButtonC _SelectedTab;
    /// <summary>
    /// Returns the current selected tab.
    /// <br>This value could be null, but it can't be set to null.</br>
    /// </summary>
    public TabButtonC SelectedTab {
        get {
            return _SelectedTab;
        }
        internal set {
            _SelectedTabIndex = GetButtonIndex(value);
            _SelectedTab = value;
        }
    }
    /// <summary>
    /// Get a tab button by directly indexing a tab system.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public TabButtonC this[int index] {
        get {
            return TabButtons[index];
        }
    }

    [SerializeField] private List<TabButtonC> tabButtons = new List<TabButtonC>();
    /// <summary>
    /// List of the currently registered tab buttons.
    /// </summary>
    public IReadOnlyList<TabButtonC> TabButtons {
        get {
            return tabButtons;
        }
    }

    // UIBehaviour
    #region Interaction Status
    [Tooltip("Can the TabButton be interacted with?")]
    [SerializeField] private bool interactable = true;
    /// <summary>
    /// Whether if this element is interactable with.
    /// </summary>
    public bool Interactable {
        get { return IsInteractable(); }
        set {
            interactable = value;

            UpdateButtonAppearances();
        }
    }
    /// <summary>
    /// Runtime variable for whether if the object is allowed to be interacted with.
    /// </summary>
    private bool groupsAllowInteraction = true;
    /// <summary>
    /// Whether if the UI element is allowed to be interactable.
    /// </summary>
    internal virtual bool IsInteractable() {
        if (groupsAllowInteraction) {
            return interactable;
        }

        return false;
    }
    private readonly List<CanvasGroup> canvasGroupCache = new List<CanvasGroup>();
    protected override void OnCanvasGroupChanged() {
        // This event is part of Selectable (but i adapted it to this script).
        // Search for 'CanvasGroup' behaviours & apply preferences to this object.
        // 1: Search for transforms that contain 'CanvasGroup'
        // 2: Keep them in cache
        // 3: Update the interaction state accordingly
        bool groupAllowInteraction = true;
        Transform t = transform;

        while (t != null) {
            t.GetComponents(canvasGroupCache);
            bool shouldBreak = false;

            for (int i = 0; i < canvasGroupCache.Count; i++) {
                if (!canvasGroupCache[i].interactable) {
                    groupAllowInteraction = false;
                    shouldBreak = true;
                }
                if (canvasGroupCache[i].ignoreParentGroups) {
                    shouldBreak = true;
                }
            }
            if (shouldBreak) {
                break;
            }

            t = t.parent;
        }
        if (groupAllowInteraction != groupsAllowInteraction) {
            groupsAllowInteraction = groupAllowInteraction;
            UpdateButtonAppearances();
        }
    }
    #endregion

    /// <summary>
    /// Internal call of <see cref="GenerateTabs"/>
    /// <br>Required to check 0 / 1 tabs disable-enable state.</br>
    /// </summary>
    /// <param name="prevIndex">Previous index passed by the <see cref="TabButtonAmount"/>'s setter.</param>
    protected void GenerateTabs(int prevIndex) {
        if (tabButtons.Count <= 0) {
            // Generate tabs from scratch if there is none.
            GenerateTabs();
            return;
        }

        // Ignore if count is 0 or less
        // While this isn't a suitable place for tab management, i wanted to add an '0' state to it. 
        TabButtonC firstTBtn = tabButtons[0];

        if (TabButtonAmount <= 0) {
            // Make sure the first tab button exists as we need to call 'GenerateTabs' for first spawn.
            if (firstTBtn != null) {
                firstTBtn.gameObject.SetActive(false);

                // Clean the buttons as that's necessary. (otherwise there's stray buttons)
                for (int i = 1; i < tabButtons.Count; i++) {
                    if (Application.isPlaying) {
                        if (tabButtons[i] != null) {
                            DestroyImmediate(tabButtons[i].gameObject); // Have to use DestroyImmediate here as well, otherwise unity gets stuck.
                        } else {
                            // Tab button is null, call CleanTabButtonsList
                            CleanTabButtonsList();
                            continue;
                        }
                    }
#if UNITY_EDITOR
                    else {
                        if (tabButtons[i] != null) {
                            UnityEditor.Undo.DestroyObjectImmediate(tabButtons[i].gameObject);
                        } else {
                            // Tab button is null, call CleanTabButtonsList
                            CleanTabButtonsList();
                            continue;
                        }
                    }
#endif
                }

                CleanTabButtonsList();
                return;
            }
            // In this case of this if statement, it's not necessary as the button amount is already 0.
        } else if (TabButtonAmount == 1 && prevIndex <= 0) {
            // Make sure the first tab button exists as we need to call 'GenerateTabs' for first spawn.
            if (firstTBtn != null) {
                // This is bad, calling fake event here.
                // But the thing is : 0 tab button amount mean disabled
                firstTBtn.gameObject.SetActive(true);
                // Do status update - management
                // This should have been done all in 'CreateTab' method but yeah
                // firstTBtn.parentTabSystem = this;
                if (!firstTBtn.IsInit) {
                    firstTBtn.Initilaze(this);
                }

                OnTabButtonCreated?.Invoke(0, firstTBtn);
            } else {
                // List needs to be cleaned (has null member that we can't access, will throw exceptions)
                CleanTabButtonsList();
            }
        }

        // Generate tabs normally after dealing with the '0' stuff.
        GenerateTabs();
    }
    /// <summary>
    /// Generates tabs.
    /// </summary>
    public void GenerateTabs() {
        // Normal creation
        while (tabButtons.Count > TabButtonAmount) {
            if (Application.isPlaying) {
                if (tabButtons[tabButtons.Count - 1] != null) {
                    // We need to use DestroyImmediate here as there's no need for the reference
                    // Otherwise the script gets stuck at an infinite loop and dies.
                    // (this is because the while loop is on the main thread, but the 'Destroy' stuff is also done on the main thread after this method is done,
                    // basically not destroying the object, 'while' loop is just constantly calling 'Destroy' to the same object)
                    // (this is kinda similar to the godot's 'queue_free' and 'free' distinction, now comparing 'tabButtons[tabButtons.Count - 1]' to null will be always true)
                    DestroyImmediate(tabButtons[tabButtons.Count - 1].gameObject);
                } else {
                    // Tab button is null, call CleanTabButtonsList
                    CleanTabButtonsList();
                    continue;
                }
            }
#if UNITY_EDITOR
            else {
                if (tabButtons[tabButtons.Count - 1] != null) {
                    UnityEditor.Undo.DestroyObjectImmediate(tabButtons[tabButtons.Count - 1].gameObject);
                } else {
                    // Tab button is null, call CleanTabButtonsList
                    CleanTabButtonsList();
                    continue;
                }
            }
#endif
            CleanTabButtonsList();
        }
        while (tabButtons.Count < TabButtonAmount) {
            CreateTab();
        }
    }
    /// <summary>
    /// Reset tabs.
    /// <br>Call this method if you have an issue with your tabs.</br>
    /// </summary>
    public void ResetTabs() {
        ClearTabs(true, true);

        // Destroy all childs
        if (tabButtons.Count <= 1 && transform.childCount > 1) {
            int tChild = transform.childCount;
            for (int i = 0; i < tChild; i++) {
                if (Application.isPlaying) {
                    Destroy(transform.GetChild(0).gameObject);
                }
#if UNITY_EDITOR
                else {
                    UnityEditor.Undo.DestroyObjectImmediate(transform.GetChild(0).gameObject);
                }
#endif
            }
        }

        // Create new tab and refresh 
        TabButtonC tab = CreateTab(false);
        tab.Initilaze(this);     // this may be redundant
        tabButtons.Clear();
        tabButtons.Add(tab);
    }
    /// <summary>
    /// Clears tabs.
    /// </summary>
    /// <param name="resetTabBtnAmount">Sets internal variable of TabButtonAmount to be 1.</param>
    /// <param name="clearAll">Clears all of the buttons (hard reset parameter).</param>
    public void ClearTabs(bool resetTabBtnAmount = true, bool clearAll = false) {
        CleanTabButtonsList();

        // Destroy array.
        foreach (TabButtonC button in tabButtons) {
            if (button.ButtonIndex == 0 && !clearAll) {
                continue;
            }

            if (Application.isPlaying) {
                Destroy(button.gameObject);
            }
#if UNITY_EDITOR
            else {
                UnityEditor.Undo.DestroyObjectImmediate(button.gameObject);
            }
#endif
        }

        if (tabButtons.Count > 1) {
            tabButtons.RemoveRange(1, Mathf.Max(1, tabButtons.Count - 1));
        }

        if (!clearAll) {
            TabButtonC tempTabBtn = tabButtons[0];
            tabButtons.Clear();
            tabButtons.Add(tempTabBtn);
            tempTabBtn.Initilaze(this);
        }

        if (resetTabBtnAmount) {
            _TabButtonAmount = 1;
        }
    }

    /// <summary>
    /// Creates Button for TabSystem.
    /// Info : This command already adds to the list <see cref="tabButtons"/>.
    /// </summary>
    /// <param name="useReferenceTab">Whether to use the referenced tab from index <see cref="ReferenceTabButtonIndex"/>.</param>
    /// <returns>Creation button result.</returns>
    public TabButtonC CreateTab(bool useReferenceTab = true) {
        TabButtonC tabButtonScript;

        if (tabButtons.Count <= 0 || !useReferenceTab) {
            GameObject tabButton = new GameObject("Tab");
            tabButton.transform.SetParent(transform);
            tabButton.transform.localScale = Vector3.one;

            tabButtonScript = tabButton.AddComponent<TabButtonC>();
            tabButton.AddComponent<Image>();

            // -- Text
            GameObject tabText = new GameObject("Tab Text");
            tabText.transform.SetParent(tabButton.transform);
            TextMeshProUGUI ButtonTMPText = tabText.AddComponent<TextMeshProUGUI>();
            tabButtonScript.ButtonTMPText = ButtonTMPText;
            // Set Text Options.
            ButtonTMPText.SetText("Tab Button");
            ButtonTMPText.color = Color.black;
            ButtonTMPText.alignment = TextAlignmentOptions.Center;
            tabText.transform.localScale = Vector3.one;
            // Set Text Anchor. (Stretch all)
            ButtonTMPText.rectTransform.anchorMin = new Vector2(.33f, 0f);
            ButtonTMPText.rectTransform.anchorMax = new Vector2(1f, 1f);
            ButtonTMPText.rectTransform.offsetMin = Vector2.zero;
            ButtonTMPText.rectTransform.offsetMax = Vector2.zero;

            // -- Image
            GameObject tabImage = new GameObject("Tab Image");
            tabImage.transform.SetParent(tabButton.transform);
            Image ButtonImage = tabImage.AddComponent<Image>();
            tabButtonScript.ButtonImage = ButtonImage;
            // Image Options
            tabImage.transform.localScale = Vector3.one;
            ButtonImage.preserveAspect = true;
            // Set anchor to left & stretch along the anchor.
            ButtonImage.rectTransform.anchorMin = new Vector2(0f, 0f);
            ButtonImage.rectTransform.anchorMax = new Vector2(.33f, 1f);
            ButtonImage.rectTransform.offsetMin = Vector2.zero;
            ButtonImage.rectTransform.offsetMax = Vector2.zero;

            tabButtonScript.GenerateButtonContent();
        } else {
            TabButtonC tabButtonInstantiationTarget = tabButtons[ReferenceTabButtonIndex];
            if (tabButtonInstantiationTarget == null) {
                // No reference tab to create from, don't use a reference.
                return CreateTab(false);
            }

            tabButtonScript = Instantiate(tabButtonInstantiationTarget);

            tabButtonScript.transform.SetParent(tabButtonInstantiationTarget.transform.parent);
            tabButtonScript.transform.localScale = tabButtonInstantiationTarget.transform.localScale;
        }

        // Init button
        tabButtonScript.Initilaze(this);
        tabButtonScript.gameObject.name = tabButtonScript.gameObject.name.Replace("(Clone)", string.Empty);
        int objectNameIndexSplit = tabButtonScript.gameObject.name.LastIndexOf('_');
        if (objectNameIndexSplit != -1) {
            // If the previous name was prefixed with an underscore, remove the underscore
            tabButtonScript.gameObject.name = tabButtonScript.gameObject.name.Substring(0, objectNameIndexSplit);
        }
        // Prefix the name with underscore
        tabButtonScript.gameObject.name = string.Format("{0}_{1}", tabButtonScript.gameObject.name, tabButtons.Count);

        tabButtons.Add(tabButtonScript);
        OnTabButtonCreated?.Invoke(tabButtons.Count - 1, tabButtonScript);

        return tabButtonScript;
    }

    // Tab Cleanup
    /// <summary>
    /// Updates the appearances of the buttons.
    /// <br>Call this when you need to visually update the button.</br>
    /// </summary>
    public void UpdateButtonAppearances() {
        foreach (TabButtonC button in tabButtons) {
            if (button == null) {
                continue;
            }

            if (!Interactable) {
                button.SetButtonAppearance(TabButtonC.ButtonState.Disable);
                continue;
            }

            button.SetButtonAppearance(SelectedTab == button ? TabButtonC.ButtonState.Click : TabButtonC.ButtonState.Reset);
        }
    }
    /// <summary>
    /// Cleans the <see cref="tabButtons"/> list in case of null and other stuff.
    /// </summary>
    public void CleanTabButtonsList() {
        tabButtons.RemoveAll((x) => x == null);
    }

    /// <summary>
    /// Gather the index for the <paramref name="button"/>.
    /// </summary>
    /// <param name="button">The button parameter. This can't be null.</param>
    /// <returns>The index of <paramref name="button"/> in this tabsystem. If this tabsystem does not have the given <paramref name="button"/> this returns -1.</returns>
    /// <exception cref="ArgumentNullException"/>
    public int GetButtonIndex(TabButtonC button) {
        if (button == null) {
            throw new ArgumentNullException(nameof(button), "[TabSystem::GetButtonIndex] Given argument was null.");
        }

        return tabButtons.IndexOf(button);
    }

    /// <summary>
    /// Returns the currently selected buttons index.
    /// </summary>
    public int GetSelectedButtonIndex() {
        return Mathf.Clamp(_SelectedTabIndex, 0, tabButtons.Count - 1);
    }
    /// <summary>
    /// Selects a button if it's selectable.
    /// </summary>
    /// <param name="btnSelect">Index to select. Clamped value.</param>
    /// <param name="silentSelect">
    /// Whether if the <see cref="OnTabButtonClicked"/> event should invoke. 
    /// This is set to <see langword="true"/> by default.
    /// </param>
    public void SetSelectedButtonIndex(int btnSelect, bool silentSelect = false) {
        _SelectedTabIndex = Mathf.Clamp(btnSelect, 0, tabButtons.Count - 1);
#if UNITY_EDITOR
        if (!Application.isPlaying) {
            return;
        }
#endif
        TabButtonC ButtonToSelScript = tabButtons[_SelectedTabIndex];

        if (ButtonToSelScript != null) {
            SelectedTab = ButtonToSelScript;
            ButtonToSelScript.SetButtonAppearance(TabButtonC.ButtonState.Click);

            if (!silentSelect) {
                OnTabButtonClicked?.Invoke(_SelectedTabIndex);
            }

            UpdateButtonAppearances();
        } else {
            Debug.LogError($"[TabSystem] The tab button to select is null. The index was {_SelectedTabIndex}.", this);
        }
    }
}
