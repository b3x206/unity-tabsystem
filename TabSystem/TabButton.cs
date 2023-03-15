using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

using System;
using System.Collections;

[RequireComponent(typeof(Image))]
public class TabButton : MonoBehaviour,
    IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Serializable]
    public class TabButtonUnityEvent : UnityEvent<Image, TabButton> { }

    // primary type
    public FadeType FadeType { get { return parentTabSystem.ButtonFadeType; } }
    // images
    private Image buttonBackgroundImage;
    public Image ButtonBackgroundImage
    {
        get
        {
            if (buttonBackgroundImage == null)
                buttonBackgroundImage = GetComponent<Image>();

            return buttonBackgroundImage;
        }
    }

    // color fade
    public Color PrevColor { get; private set; }
    public Color DisableColor { get { return parentTabSystem.FadeColorTargetDisabled; } }
    public Color HoverColor { get { return parentTabSystem.FadeColorTargetHover; } }
    // sprite swap
    private Sprite PrevSprite;

    [Header(":: Tab Button Content")]
    [Tooltip("Text Content of this button.\nSet this to update the text.")]
    [SerializeField, TextArea] private string buttonText = "Tab Button";
    [Tooltip("Text Content of this button.\nSet this to update the icon.")]
    [SerializeField] private Sprite buttonSprite;
    /// <summary>
    /// Mostly editor only, receive content (<see cref="buttonText"/> and <see cref="buttonSprite"/>) from the added button components.
    /// </summary>
    [SerializeField] private bool receiveContentFromComponents;

    public string ButtonText
    {
        get { return buttonText; }
        set
        {
            buttonText = value;
            GenerateButtonContent();
        }
    }
    public Sprite ButtonSprite
    {
        get { return buttonSprite; }
        set
        {
            buttonSprite = value;
            GenerateButtonContent();
        }
    }

    [SerializeField] private bool mInteractable = true;
    /// <summary>
    /// Whether if this button is interactable.
    /// <br>Note : The parent tab system's interactability overrides this buttons.</br>
    /// </summary>
    public bool Interactable
    {
        get { return parentTabSystem.Interactable && mInteractable; }
        set { mInteractable = value; }
    }

    [Header(":: Tab Button Reference")]
    [SerializeField] private TMP_Text buttonTMPText;
    [SerializeField] private Image buttonImage;
    public TMP_Text ButtonTMPText { get { return buttonTMPText; } internal set { buttonTMPText = value; } }
    public Image ButtonImage { get { return buttonImage; } internal set { buttonImage = value; } }

    [Header(":: Internal Reference (don't touch this unless necessary)")]
    [SerializeField, HideInInspector] private TabSystem parentTabSystem;
    public int ButtonIndex { get { return transform.GetSiblingIndex(); } }
    public TabSystem ParentTabSystem { get { return parentTabSystem; } }

    // -- Initilaze
    public bool IsInit => parentTabSystem != null;
    public void Initilaze(TabSystem parent)
    {
        if (IsInit)
            return;

        parentTabSystem = parent;
    }
    private void Start()
    {
        if (parentTabSystem == null)
        {
            Debug.LogWarning(string.Format("[TabButton (name -> '{0}')] The parent tab system is null. Will try to get it.", name));
            var parentTab = GetComponentInParent<TabSystem>();

            if (parentTab == null)
            {
                Debug.LogWarning(string.Format("[TabButton (name -> '{0}')] The parent tab system is null. Failed to get component.", name));
                return;
            }

            parentTabSystem = parentTab;
        }

        // Set Colors
        PrevColor = ButtonBackgroundImage.color;

        // Set Images
        PrevSprite = ButtonBackgroundImage.sprite;

        // If selected object.
        if (ButtonIndex == 0)
        {
            parentTabSystem.CurrentSelectedTab = this;

            // Set visuals.
            SetButtonAppearance(ButtonState.Click);
        }
    }

    /// <summary>
    /// <br>Generates content from <see cref="buttonContent"/>.</br>
    /// </summary>
    /// <param name="onValidateCall">
    /// This parameter specifies whether if this method was called from an 'OnValidate' method.
    /// <br>Do not touch this unless you are calling this from 'OnValidate' (Changes <see cref="Debug.Log"/> behaviour)</br>
    /// </param>
    internal void GenerateButtonContent(bool onValidateCall = false)
    {
        if (ButtonTMPText != null)
        {
            // Apply content if we have content
            if (!string.IsNullOrWhiteSpace(ButtonText))
            {
                ButtonTMPText.SetText(ButtonText);
                ButtonTMPText.gameObject.SetActive(true);
            }
            // Receive content if the 'image or sprite' does exist (& our content is null)
            else if (!string.IsNullOrWhiteSpace(ButtonTMPText.text) && receiveContentFromComponents)
            {
                ButtonText = ButtonTMPText.text;
                ButtonTMPText.gameObject.SetActive(true);
            }
            // No content, bail out
            else
            {
                ButtonTMPText.gameObject.SetActive(false);
            }
        }
        else if (Application.isPlaying && !onValidateCall && !string.IsNullOrWhiteSpace(ButtonText))
        {
            // Print only if tried to set content
            Debug.LogWarning(string.Format("[TabButton::GenerateButtonContent] ButtonTMPText field in button \"{0}\" is null.", name));
        }

        if (ButtonImage != null)
        {
            if (ButtonSprite != null)
            {
                ButtonImage.sprite = ButtonSprite;
                ButtonImage.gameObject.SetActive(true);
            }
            else if (ButtonImage.sprite != null && receiveContentFromComponents)
            {
                ButtonSprite = ButtonImage.sprite;
                ButtonImage.gameObject.SetActive(true);
            }
            else
            {
                ButtonImage.gameObject.SetActive(false);
            }
        }
        else if (Application.isPlaying && !onValidateCall && ButtonSprite != null)
        {
            Debug.LogWarning(string.Format("[TabButton::GenerateButtonContent] ButtonImage field in button \"{0}\" is null.", name));
        }
    }
    private void OnValidate()
    {
        GenerateButtonContent(true);
    }

    #region PointerClick Events
    // -- Invoke the actual click here.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Interactable)
            return;

        parentTabSystem.OnTabButtonsClicked?.Invoke(transform.GetSiblingIndex());

        parentTabSystem.CurrentSelectedTab = this;
        parentTabSystem.UpdateButtonAppearances();
    }

    // -- Visual Updates
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!Interactable)
            return;

        if (parentTabSystem.CurrentSelectedTab != this)
        {
            SetButtonAppearance(ButtonState.Click);
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!Interactable)
            return;

        if (parentTabSystem.CurrentSelectedTab != this)
        {
            SetButtonAppearance(ButtonState.Hover);
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!Interactable)
            return;

        if (parentTabSystem.CurrentSelectedTab != this)
        {
            SetButtonAppearance(ButtonState.Reset);
        }
        else // ParentTabSystem.CurrentSelectedTab == this
        {
            SetButtonAppearance(ButtonState.Click);
        }
    }

    /// <summary>
    /// State of the button to set the appearence into.
    /// <br>You can get the states of the button using <see cref="TabSystem"/> events.</br>
    /// </summary>
    internal enum ButtonState { Reset, Hover, Click, Disable }
    /// <summary>
    /// Sets the button appearence.
    /// <br>Do not call this method.</br>
    /// </summary>
    internal void SetButtonAppearance(ButtonState state)
    {
        switch (state)
        {
            case ButtonState.Reset:
                switch (FadeType)
                {
                    case FadeType.ColorFade:
                        TweenColorFade(parentTabSystem.FadeColorTargetDefault, parentTabSystem.FadeSpeed);
                        break;
                    case FadeType.SpriteSwap:
                        if (PrevSprite != null) { ButtonBackgroundImage.sprite = parentTabSystem.DefaultSpriteToSwap; }
                        else { ButtonBackgroundImage.sprite = null; }
                        break;
                    case FadeType.CustomUnityEvent:
                        parentTabSystem.ButtonCustomEventOnReset?.Invoke(ButtonBackgroundImage, this);
                        break;
                }
                break;
            case ButtonState.Hover:
                switch (FadeType)
                {
                    case FadeType.ColorFade:
                        TweenColorFade(parentTabSystem.FadeColorTargetHover, parentTabSystem.FadeSpeed);
                        break;
                    case FadeType.SpriteSwap:
                        ButtonBackgroundImage.sprite = parentTabSystem.HoverSpriteToSwap;
                        break;
                    case FadeType.CustomUnityEvent:
                        parentTabSystem.ButtonCustomEventOnHover?.Invoke(ButtonBackgroundImage, this);
                        break;
                }
                break;
            case ButtonState.Click:
                switch (FadeType)
                {
                    case FadeType.ColorFade:
                        TweenColorFade(parentTabSystem.FadeColorTargetClick, parentTabSystem.FadeSpeed);
                        break;
                    case FadeType.SpriteSwap:
                        ButtonBackgroundImage.sprite = parentTabSystem.TargetSpriteToSwap;
                        break;
                    case FadeType.CustomUnityEvent:
                        parentTabSystem.ButtonCustomEventOnClick?.Invoke(ButtonBackgroundImage, this);
                        break;
                }
                break;
            case ButtonState.Disable:
                switch (FadeType)
                {
                    case FadeType.ColorFade:
                        TweenColorFade(DisableColor, parentTabSystem.FadeSpeed);
                        break;
                    case FadeType.SpriteSwap:
                        if (PrevSprite != null) { ButtonBackgroundImage.sprite = parentTabSystem.DisabledSpriteToSwap; }
                        else { ButtonBackgroundImage.sprite = null; }
                        break;
                    case FadeType.CustomUnityEvent:
                        parentTabSystem.ButtonCustomEventOnDisable?.Invoke(ButtonBackgroundImage, this);
                        break;
                }
                break;

            default:
                // Reset if no state was assigned.
                Debug.LogWarning($"[TabButton::SetButtonAppearance] No behaviour defined for state : \"{state}\". Reseting instead.");
                goto case ButtonState.Reset;
        }
    }
    #endregion

    #region Color Fading
    private void TweenColorFade(Color Target, float Duration)
    {
        if (!gameObject.activeInHierarchy) return; // Do not start coroutines if the object isn't active.

        StartCoroutine(CoroutineTweenColorFade(Target, Duration));
    }
    private IEnumerator CoroutineTweenColorFade(Color Target, float Duration)
    {
        // Color manipulation
        Color CurrentPrevColor = ButtonBackgroundImage.color;
        bool TargetIsPrevColor = Target == PrevColor;

        if (parentTabSystem.FadeSubtractFromCurrentColor)
            Target = TargetIsPrevColor ? Target : CurrentPrevColor - Target;
        // else, leave it unchanged

        if (!Application.isPlaying)
        {
            // Set the color instantly as the 'UnityEditor' doesn't support tween.
            ButtonBackgroundImage.color = Target;

            yield break;
        }

        if (Duration <= 0f)
        {
            ButtonBackgroundImage.color = Target;

            yield break;
        }

        // Fade
        float T = 0f;

        while (T <= 1.0f)
        {
            T += Time.deltaTime / Duration;
            ButtonBackgroundImage.color = Color.Lerp(CurrentPrevColor, Target, Mathf.SmoothStep(0, 1, T));
            yield return null;
        }

        // Set end value.
        ButtonBackgroundImage.color = Target;
    }
    #endregion
}
