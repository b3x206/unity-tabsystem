using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

using System.Linq;
using System.Collections;
using System.Collections.Generic;

#region External Classes

/// <summary>
/// The fading type of TabButton.
/// </summary>
public enum FadeType
{
    None,
    ColorFade,
    SpriteSwap,
    CustomUnityEvent
}
#endregion

/// <summary>
/// The tab system. Only use constructs if you don't identify as a button.
/// TODO : Make names more understandable
/// </summary>
[ExecuteInEditMode()]
public class TabSystem : MonoBehaviour
{
    [System.Serializable]
    public class IntUnityEvent : UnityEvent<int> { }

    ///////////// Public
    public int TabButtonAmount 
    {
        get 
        {
            return _TabButtonAmount;
        } 
        set
        {
            _TabButtonAmount = Mathf.Clamp(value, 1, ushort.MaxValue);
            GenerateTabs();
        } 
    }
    [SerializeField] private int _TabButtonAmount = 1;

    public FadeType ButtonFadeType = FadeType.ColorFade;

    public int CurrentReferenceTabButton
    {
        get
        {
            return _CurrentReferenceTabButton;
        }
        set
        {
            if (_CurrentReferenceTabButton == value) return;

            _CurrentReferenceTabButton = Mathf.Clamp(value, 0, TabButtonAmount - 1);
        }
    }
    [SerializeField] private int _CurrentReferenceTabButton = 0;

    // -- Fade Styles
    // ButtonFadeType = ColorFade
    public float TabButtonFadeSpeed = .15f;
    public Color TabButtonFadeColorTargetHover = new Color(.95f, .95f, .95f);
    public Color TabButtonFadeColorTargetClick = new Color(.9f, .9f, .9f);
    public bool TabButtonSubtractFromCurrentColor = false;
    // ButtonFadeType = SpriteSwap
    public Sprite HoverSpriteToSwap;
    public Sprite TargetSpriteToSwap;
    // ButtonFadeType = CustomUnityEvent
    public TabButton.TabButtonUnityEvent TabButtonCustomEventOnReset;
    public TabButton.TabButtonUnityEvent TabButtonCustomEventHover;
    public TabButton.TabButtonUnityEvent TabButtonCustomEventClick;

    // Standard event
    public IntUnityEvent OnTabButtonsClicked;

    //////////// Public Constructs
    /// <summary>
    /// Returns the current selected tab. Make sure it's assigned properly.
    /// </summary>
    public TabButton CurrentSelectedTab { get; set; }
    public TabButton this[int index] 
    { 
        get
        {
            return TabButtons[index];
        }
    }

    // Private
    [SerializeField] private List<TabButton> TabButtons = new List<TabButton>();

    public void GenerateTabs()
    {
        while (TabButtons.Count > TabButtonAmount)
        {
            if (Application.isEditor)
            {
                DestroyImmediate(TabButtons[TabButtons.Count - 1].gameObject);
            }
            if (Application.isPlaying)
            {
                Destroy(TabButtons[TabButtons.Count - 1].gameObject);
            }

            CleanTabButtonsList();
        }
        while (TabButtons.Count < TabButtonAmount)
        {
            CreateTab();
        }
    }

    public void ResetTabs()
    {
        ClearTabs(true, true);

        // Destroy all childs
        if (TabButtons.Count <= 1 && transform.childCount > 1)
        {
            var tChild = transform.childCount;
            for (int i = 0; i < tChild; i++)
            {
                if (Application.isEditor)
                {
                    DestroyImmediate(transform.GetChild(0).gameObject);
                }
                if (Application.isPlaying)
                {
                    Destroy(transform.GetChild(0).gameObject);
                }
            }
        }

        // Create new tab and refresh 
        var tab = CreateTab(false);
        tab.ButtonIndex = 0;
        TabButtons.Clear();
        TabButtons.Add(tab);
    }

    public void ClearTabs(bool ResetTbBtnAmount = true, bool ClearAll = false)
    {
        CleanTabButtonsList();

        // Destroy array.
        foreach (TabButton button in TabButtons)
        {
            if (button.ButtonIndex == 0 && !ClearAll) continue;

            if (Application.isPlaying)
            {
                Destroy(button.gameObject);
            }

            if (Application.isEditor)
            {
                DestroyImmediate(button.gameObject);
            }
        }

        if (TabButtons.Count > 1)
        {
            TabButtons.RemoveRange(1, Mathf.Max(1, TabButtons.Count - 1));
        }

        if (!ClearAll)
        {
            var tempTabBtn = TabButtons[0];
            TabButtons.Clear();
            TabButtons.Add(tempTabBtn);
            tempTabBtn.ButtonIndex = 0;
        }

        if (ResetTbBtnAmount)
        {
            _TabButtonAmount = 1;
        }
    }

    /// <summary>
    /// Creates Button for TabSystem.
    /// Info : This command already adds to the list <see cref="TabButtons"/>.
    /// </summary>
    /// <param name="Parent"></param>
    /// <returns>Creation button result.</returns>
    public TabButton CreateTab(bool UseRefTab = true)
    {
        TabButton TabButtonScript;

        if (TabButtons.Count <= 0 || !UseRefTab)
        {
            GameObject TButton = new GameObject("Tab");
            TButton.transform.SetParent(transform);
            TButton.transform.localScale = Vector3.one;

            TabButtonScript = TButton.AddComponent<TabButton>();

            GameObject TText = new GameObject("Tab Text");
            TText.transform.SetParent(TButton.transform);

            TextMeshProUGUI ButtonText = TText.AddComponent<TextMeshProUGUI>();
            TabButtonScript.ButtonText = ButtonText;
            // Set Text Options.
            ButtonText.SetText("Tab Button");
            ButtonText.color = Color.black;
            ButtonText.alignment = TextAlignmentOptions.Center;
            TText.transform.localScale = Vector3.one;

            // Set Text Anchor.
            // Replaces this command.
            // RectTransformExtensions.SetAnchor(TText.GetComponent<RectTransform>(), AnchorPresets.StretchAll);
            var TTextRect = ButtonText.rectTransform;
            // Stretch all.
            TTextRect.anchorMin = new Vector2(0, 0);
            TTextRect.anchorMax = new Vector2(1, 1);
            TTextRect.offsetMin = Vector2.zero;
            TTextRect.offsetMax = Vector2.zero;
        }
        else
        {
            var TabButtonInstTarget = TabButtons[CurrentReferenceTabButton];
            if (TabButtonInstTarget == null)
            {
                return CreateTab(false);
            }

            TabButtonScript = Instantiate(TabButtonInstTarget);

            TabButtonScript.transform.SetParent(TabButtonInstTarget.transform.parent);
            TabButtonScript.transform.localScale = TabButtonInstTarget.transform.localScale;
        }

        // Init button
        TabButtonScript.ButtonIndex = TabButtons.Count;
        TabButtonScript.ParentTabSystem = this;
        TabButtonScript.name = $"{TabButtonScript.name}_{TabButtons.Count}".Replace("(Clone)", "");

        TabButtons.Add(TabButtonScript);

        return TabButtonScript;
    }

    #region Button Organize
    /// <summary>
    /// Cleans the <see cref="TabButtons"/> list in case of null and other stuff.
    /// </summary>
    public void CleanTabButtonsList()
    {
        TabButtons.RemoveAll((x) => x == null);
    }
    /// <summary>
    /// Resets button appearances of unselected ones.
    /// </summary>
    public void CheckUnClickedButtons()
    {
        foreach (TabButton b in TabButtons)
        {
            if (b == null)
            { continue; }

            if (b != CurrentSelectedTab)
            {
                b.ResetButtonAppearance();
            }
        }
    }

    /// <summary>
    /// Selects a button if it's selectable.
    /// </summary>
    /// <param name="BtnSelect">Index to select. Clamped value.</param>
    /// <returns>If the button selection succeeded.</returns>
    public void SetSelectedButtonIndex(int BtnSelect)
    {
        var IndexSelect = Mathf.Clamp(BtnSelect, 0, TabButtons.Count - 1);
        TabButton ButtonToSelScript = TabButtons[IndexSelect];

        if (ButtonToSelScript != null)
        {
            CurrentSelectedTab = ButtonToSelScript;
            ButtonToSelScript.SelectButtonAppearance();
            OnTabButtonsClicked?.Invoke(IndexSelect);
            CheckUnClickedButtons();
        }
        else
        {
            Debug.LogError($"[TabSystem] The tab button to select is null. The index was {IndexSelect}.");
        }
    }
    public int GetSelectedButtonIndex()
    {
        return CurrentSelectedTab.ButtonIndex;
    }
    #endregion
}