using System.Collections;
using UnityEngine;

public class Sector7IntroPopup : MonoBehaviour
{
    [Header("Popup")]
    public PopupDialogController popupDialog;
    public string title = "City 17: Sector 7";
    [TextArea(3, 12)]
    public string message =
        "Sector 7 never sleeps. Neon lights cut through rain, state alerts flood every screen, " +
        "and nobody agrees on what is true anymore.\n\n" +
        "You are not a hero. You are a local journalist trying to stay human in a city poisoned by " +
        "misinformation, fear, and political control. Every rumor can redirect people. Every false " +
        "headline can cause real harm.\n\n" +
        "Your job is to verify claims in the field, collect evidence, and separate fact from manipulation " +
        "before the city sinks deeper into chaos.\n\n" +
        "Press TAB to open the Data-Rig and begin your first investigation.";
    public float messageFontSize = 28f;
    public bool showEveryLoad = true;

    private static bool hasShownThisSession;

    private IEnumerator Start()
    {
        if (!showEveryLoad && hasShownThisSession)
        {
            yield break;
        }

        yield return null;

        if (popupDialog == null)
        {
            popupDialog = PopupDialogController.FindInScene();
        }

        if (popupDialog == null)
        {
            Debug.LogWarning("Sector7IntroPopup: PopupDialogController not found in scene.");
            yield break;
        }

        popupDialog.ShowInfo(title, message, null, messageFontSize);
        hasShownThisSession = true;
    }
}
