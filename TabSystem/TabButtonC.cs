using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// A tab button component, handles the pointer events and the color/graphic transitions.
/// The parent <see cref="TabSystem"/> manages it's settings.
/// </summary>
public class TabButtonC : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler {
    /// <summary>
    /// Used with a tab button related transitioning event.
    /// <br>The <see cref="Image"/> parameter is for the background image of the button and the <see cref="TabButton"/> parameter is for the button itself.</br>
    /// </summary>
    [Serializable]
    public class ButtonTransitionEvent : UnityEvent<Image, TabButtonC> { }

    // primary type
    /// <summary>
    /// Primary fading type used that is from the parent tab system.
    /// </summary>
    public FadeType FadeType { get { return parentTabSystem.ButtonFadeType; } }
    // images
    private Image buttonBackgroundImage;
    /// <summary>
    /// Background image assigned to this tab button.
    /// <br>This is the image used for the fading (<see cref="FadeType"/>).</br>
    /// </summary>
    public Image ButtonBackgroundImage {
        get {
            if (buttonBackgroundImage == null) {
                buttonBackgroundImage = GetComponent<Image>();
            }

            return buttonBackgroundImage;
        }
    }

    // color fade
    public Color PrevColor { get; private set; }
    public Color DisableColor { get { return parentTabSystem.FadeColorTargetOnDisabled; } }
    public Color HoverColor { get { return parentTabSystem.FadeColorTargetOnHover; } }
    // sprite swap
    private Sprite PrevSprite;

    [Header(":: Tab Button Content")]
    [Tooltip("Text Content of this button.\nSet this to update the text.")]
    [SerializeField, TextArea] private string buttonText = "Tab Button";
    /// <summary>
    /// Text contained inside this tab button.
    /// <br>Setting this will change the <see cref="ButtonTMPText"/>'s text property value.</br>
    /// </summary>
    public string ButtonText {
        get { return buttonText; }
        set {
            buttonText = value;
            GenerateButtonContent();
        }
    }
    [Tooltip("Text Content of this button.\nSet this to update the icon.")]
    [SerializeField] private Sprite buttonSprite;
    /// <summary>
    /// Text contained inside this tab button.
    /// <br>Setting this will change the <see cref="ButtonImage"/>'s sprite property value.</br>
    /// <br>Note : This does not contain the background sprite. Instead this is an accompanying value.</br>
    /// </summary>
    public Sprite ButtonSprite {
        get { return buttonSprite; }
        set {
            buttonSprite = value;
            GenerateButtonContent();
        }
    }
    /// <summary>
    /// Mostly editor only, receive content (<see cref="buttonText"/> and <see cref="buttonSprite"/>) from the added button components.
    /// </summary>
    [SerializeField] private bool receiveContentFromComponents;

    [SerializeField, FormerlySerializedAs("mInteractable")] private bool m_Interactable = true;
    /// <summary>
    /// Whether if this button is interactable.
    /// <br>Note : The parent tab system's interactability overrides this buttons.</br>
    /// </summary>
    public bool Interactable {
        get { return parentTabSystem.Interactable && m_Interactable; }
        set { m_Interactable = value; }
    }

    [Header(":: Tab Button Reference")]
    [SerializeField] private TMP_Text buttonTMPText;
    [SerializeField] private Image buttonImage;
    /// <summary>
    /// Text attached to this button.
    /// </summary>
    public TMP_Text ButtonTMPText { get { return buttonTMPText; } internal set { buttonTMPText = value; } }
    /// <summary>
    /// Image (complimentary one, for the <i>background</i> use <see cref="ButtonBackgroundImage"/>) attached to this button.
    /// <br/>
    /// <br>This image is on the left side of the button (by default), but it can be at the "anywhere else" side of the button.</br>
    /// </summary>
    public Image ButtonImage { get { return buttonImage; } internal set { buttonImage = value; } }

    // Internal Reference (don't touch this unless necessary)
    [SerializeField, HideInInspector] private TabSystem parentTabSystem;
    /// yes, this is indeed janky.
    /// <summary>
    /// Returns the index of this button.
    /// </summary>
    public int ButtonIndex { get { return parentTabSystem == null ? transform.GetSiblingIndex() : parentTabSystem.GetButtonIndex(this); } }
    /// <summary>
    /// The parent tab system that this button was initialized with.
    /// </summary>
    public TabSystem ParentTabSystem { get { return parentTabSystem; } }

    // -- Initilaze
    /// <summary>
    /// Whether if this button is initialized.
    /// </summary>
    public bool IsInit => parentTabSystem != null;
    /// <summary>
    /// Initializes and sets up the button.
    /// </summary>
    public void Initilaze(TabSystem parent) {
        if (IsInit) {
            return;
        }

        parentTabSystem = parent;
    }
    private void Start() {
        if (parentTabSystem == null) {
            Debug.LogWarning("[TabButton::Start] The parent tab system is null. Getting the parent component.", this);
            TabSystem parentTab = GetComponentInParent<TabSystem>();

            if (parentTab == null) {
                Debug.LogWarning("[TabButton::Start] The parent tab system is null. Failed to get component.", this);
                return;
            }

            parentTabSystem = parentTab;
        }

        // Set Colors + Images
        PrevColor = ButtonBackgroundImage.color;
        PrevSprite = ButtonBackgroundImage.sprite;

        // If current button is the selected object.
        if (ButtonIndex == parentTabSystem.SelectedTabIndex) {
            parentTabSystem.SelectedTab = this;

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
    internal void GenerateButtonContent(bool onValidateCall = false) {
        if (ButtonTMPText != null) {
            // Apply content if we have content
            if (!string.IsNullOrWhiteSpace(ButtonText)) {
                ButtonTMPText.SetText(ButtonText);
                ButtonTMPText.gameObject.SetActive(true);
            }
            // Receive content if the 'image or sprite' does exist (& our content is null)
            else if (!string.IsNullOrWhiteSpace(ButtonTMPText.text) && receiveContentFromComponents) {
                ButtonText = ButtonTMPText.text;
                ButtonTMPText.gameObject.SetActive(true);
            }
            // No content, bail out
            else {
                ButtonTMPText.gameObject.SetActive(false);
            }
        } else if (Application.isPlaying && !onValidateCall && !string.IsNullOrWhiteSpace(ButtonText)) {
            // Print only if tried to set content
            Debug.LogWarning(string.Format("[TabButton::GenerateButtonContent] ButtonTMPText field in button \"{0}\" is null.", name));
        }

        if (ButtonImage != null) {
            if (ButtonSprite != null) {
                ButtonImage.sprite = ButtonSprite;
                ButtonImage.gameObject.SetActive(true);
            } else if (ButtonImage.sprite != null && receiveContentFromComponents) {
                ButtonSprite = ButtonImage.sprite;
                ButtonImage.gameObject.SetActive(true);
            } else {
                ButtonImage.gameObject.SetActive(false);
            }
        } else if (Application.isPlaying && !onValidateCall && ButtonSprite != null) {
            Debug.LogWarning(string.Format("[TabButton::GenerateButtonContent] ButtonImage field in button \"{0}\" is null.", name));
        }
    }
    private void OnValidate() {
        GenerateButtonContent(true);
    }

    #region PointerClick Events
    // -- Invoke the actual click here.
    public void OnPointerClick(PointerEventData eventData) {
        if (!Interactable) {
            return;
        }

        parentTabSystem.OnTabButtonClicked?.Invoke(transform.GetSiblingIndex());

        parentTabSystem.SelectedTab = this;
        parentTabSystem.UpdateButtonAppearances();
    }

    // -- Visual Updates
    public void OnPointerDown(PointerEventData eventData) {
        if (!Interactable) {
            return;
        }

        if (parentTabSystem.SelectedTab != this) {
            SetButtonAppearance(ButtonState.Click);
        }
    }
    public void OnPointerEnter(PointerEventData eventData) {
        if (!Interactable) {
            return;
        }

        if (parentTabSystem.SelectedTab != this) {
            SetButtonAppearance(ButtonState.Hover);
        }
    }
    public void OnPointerExit(PointerEventData eventData) {
        if (!Interactable) {
            return;
        }

        if (parentTabSystem.SelectedTab != this) {
            SetButtonAppearance(ButtonState.Reset);
        } else // ParentTabSystem.CurrentSelectedTab == this
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
    internal void SetButtonAppearance(ButtonState state) {
        switch (state) {
            case ButtonState.Reset:
                switch (FadeType) {
                    case FadeType.ColorFade:
                        TweenColorFade(parentTabSystem.FadeColorTargetOnDefault, parentTabSystem.FadeColorDuration);
                        break;
                    case FadeType.SpriteSwap:
                        if (PrevSprite != null) { ButtonBackgroundImage.sprite = parentTabSystem.SpriteTargetOnDefault; } else { ButtonBackgroundImage.sprite = null; }
                        break;
                    case FadeType.CustomUnityEvent:
                        parentTabSystem.ButtonCustomEventOnReset?.Invoke(ButtonBackgroundImage, this);
                        break;
                }
                break;
            case ButtonState.Hover:
                switch (FadeType) {
                    case FadeType.ColorFade:
                        TweenColorFade(parentTabSystem.FadeColorTargetOnHover, parentTabSystem.FadeColorDuration);
                        break;
                    case FadeType.SpriteSwap:
                        ButtonBackgroundImage.sprite = parentTabSystem.SpriteTargetOnHover;
                        break;
                    case FadeType.CustomUnityEvent:
                        parentTabSystem.ButtonCustomEventOnHover?.Invoke(ButtonBackgroundImage, this);
                        break;
                }
                break;
            case ButtonState.Click:
                switch (FadeType) {
                    case FadeType.ColorFade:
                        TweenColorFade(parentTabSystem.FadeColorTargetOnClick, parentTabSystem.FadeColorDuration);
                        break;
                    case FadeType.SpriteSwap:
                        ButtonBackgroundImage.sprite = parentTabSystem.SpriteTargetOnClick;
                        break;
                    case FadeType.CustomUnityEvent:
                        parentTabSystem.ButtonCustomEventOnClick?.Invoke(ButtonBackgroundImage, this);
                        break;
                }
                break;
            case ButtonState.Disable:
                switch (FadeType) {
                    case FadeType.ColorFade:
                        TweenColorFade(DisableColor, parentTabSystem.FadeColorDuration);
                        break;
                    case FadeType.SpriteSwap:
                        if (PrevSprite != null) { ButtonBackgroundImage.sprite = parentTabSystem.SpriteTargetOnDisabled; } else { ButtonBackgroundImage.sprite = null; }
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
    /// <summary>
    /// Tweens the color of <see cref="ButtonBackgroundImage"/> to <paramref name="target"/> with the duration of given <paramref name="duration"/>.
    /// </summary>
    /// <param name="target">Target color to end the tween on.</param>
    /// <param name="duration">Duration of the tween.</param>
    private void TweenColorFade(Color target, float duration) {
        if (!gameObject.activeInHierarchy) {
            return; // Do not start coroutines if the object isn't active.
        }

        StartCoroutine(CoroutineTweenColorFade(target, duration));
    }
    /// <summary>
    /// <inheritdoc cref="TweenColorFade(Color, float)"/>
    /// <br>This is the coroutine, call the <see cref="TweenColorFade(Color, float)"/> to directly dispatch the coroutine.</br>
    /// </summary>
    /// <inheritdoc cref="TweenColorFade(Color, float)"/>
    private IEnumerator CoroutineTweenColorFade(Color target, float duration) {
        // Color manipulation
        Color currentPrevColor = ButtonBackgroundImage.color;
        bool targetIsPrevColor = target == PrevColor;

        if (parentTabSystem.FadeSubtractsFromCurrentColor && !targetIsPrevColor) {
            target = currentPrevColor - target;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying) {
            // Set the color instantly as the 'UnityEditor' doesn't support tween.
            ButtonBackgroundImage.color = target;

            yield break;
        }
#endif
        if (duration <= 0f) {
            ButtonBackgroundImage.color = target;

            yield break;
        }

        // Fade
        float t = 0f;

        while (t <= 1.0f) {
            t += Time.deltaTime / duration;
            ButtonBackgroundImage.color = Color.Lerp(currentPrevColor, target, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        // Set end value.
        ButtonBackgroundImage.color = target;
    }
    #endregion
}
