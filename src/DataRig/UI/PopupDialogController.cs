using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupDialogController : MonoBehaviour
{
    public static PopupDialogController Instance { get; private set; }

    public static PopupDialogController FindInScene()
    {
        if (Instance != null)
        {
            return Instance;
        }

        PopupDialogController[] dialogs = FindObjectsOfType<PopupDialogController>(true);
        return dialogs.Length > 0 ? dialogs[0] : null;
    }

    [Header("Panel")]
    public GameObject dialogRoot;
    public TMP_Text titleText;
    public TMP_Text messageText;
    public float defaultMessageFontSize = 0f;

    [Header("Buttons")]
    public Button okButton;
    public Button yesButton;
    public Button noButton;

    private Action pendingConfirm;
    private Action pendingCancel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dialogRoot == null)
        {
            dialogRoot = gameObject;
        }

        if (messageText != null && defaultMessageFontSize <= 0f)
        {
            defaultMessageFontSize = messageText.fontSize;
        }

        if (okButton != null)
        {
            okButton.onClick.AddListener(OnOkPressed);
        }

        if (yesButton != null)
        {
            yesButton.onClick.AddListener(OnYesPressed);
        }

        if (noButton != null)
        {
            noButton.onClick.AddListener(OnNoPressed);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void ShowInfo(string title, string message, Action onOk = null, float messageFontSize = -1f)
    {
        pendingConfirm = onOk;
        pendingCancel = null;
        SetContent(title, message, messageFontSize);
        SetMode(showOk: true, showYesNo: false);
        Show();
    }

    public void ShowConfirm(string title, string message, Action onYes, Action onNo = null, float messageFontSize = -1f)
    {
        pendingConfirm = onYes;
        pendingCancel = onNo;
        SetContent(title, message, messageFontSize);
        SetMode(showOk: false, showYesNo: true);
        Show();
    }

    public void Hide()
    {
        HideImmediate();
        pendingConfirm = null;
        pendingCancel = null;
    }

    private void Show()
    {
        if (dialogRoot == null) dialogRoot = gameObject;
        dialogRoot.SetActive(true);
    }

    private void HideImmediate()
    {
        if (dialogRoot != null)
        {
            dialogRoot.SetActive(false);
        }
    }

    private void SetContent(string title, string message, float messageFontSize = -1f)
    {
        if (titleText != null)
        {
            titleText.text = title;
        }

        if (messageText != null)
        {
            messageText.text = message;
            messageText.fontSize = messageFontSize > 0f
                ? messageFontSize
                : (defaultMessageFontSize > 0f ? defaultMessageFontSize : messageText.fontSize);
        }
    }

    private void SetMode(bool showOk, bool showYesNo)
    {
        if (okButton != null)
        {
            okButton.gameObject.SetActive(showOk);
        }

        if (yesButton != null)
        {
            yesButton.gameObject.SetActive(showYesNo);
        }

        if (noButton != null)
        {
            noButton.gameObject.SetActive(showYesNo);
        }
    }

    private void OnOkPressed()
    {
        Action callback = pendingConfirm;
        Hide();
        callback?.Invoke();
    }

    private void OnYesPressed()
    {
        Action callback = pendingConfirm;
        Hide();
        callback?.Invoke();
    }

    private void OnNoPressed()
    {
        Action callback = pendingCancel;
        Hide();
        callback?.Invoke();
    }
}
