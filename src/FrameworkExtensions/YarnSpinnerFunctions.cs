using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;
using TMPro;

public static class YarnSpinnerFunctions
{
    const float DefaultMinDistance = 2f;

    private static int currentAnimationValidationId = 0;

    static float GetCurrentTime(Animator animator)
    {
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }


    private static GameObject FindGameObject(string name)
    {
        GameObject result = GameObject.Find(name);
        if (result == null)
        {
            Debug.LogError(string.Format("GameObject {0} can't be found", name));
        }
        return result;
    }

    private static T FindComponentOnObject<T>(string name) where T : Component
    {
        GameObject gameObject = FindGameObject(name);
        if (gameObject != null)
        {
            return gameObject.GetComponent<T>();
        }
        Debug.LogError(string.Format("GameObject {0} has no component of the type {1}", name, typeof(T).Name));
        return null;
    }


    [YarnCommand("play_animation")]
    public static IEnumerator PlayAnimation(string target, string name, bool shouldWait = true)
    {
        Animator animator = FindComponentOnObject<Animator>(target);
        Debug.Log($"Playing animation {name} on {target}");
        if (animator != null)
        {
            animator.Play(name);
            if (shouldWait)
            {
                currentAnimationValidationId++;
                int validationId = currentAnimationValidationId;
                while (GetCurrentTime(animator) >= 1.0f)
                {
                    if (currentAnimationValidationId != validationId) break;
                    yield return null;
                }
                while (GetCurrentTime(animator) < 1.0f)
                {
                    if (currentAnimationValidationId != validationId) break;
                    yield return null;
                }
            }
        }
    }

    [YarnCommand("set_enabled_in_unity")]
    public static void SetEnabledInUnity(string target, bool mode = true)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == target)
            {
                obj.SetActive(mode);
            }
        }
    }

    [YarnCommand("set_enabled")]
    public static void SetEnabled(string target, bool mode = true)
    {
        GameObject gameObject = FindGameObject(target);
        if (gameObject == null) return;
        bool hasActivator = false;
        //this is needed to prevent this function from enabling Activatables that are waiting for an inventory item
        foreach (PlayerActivatable playerActivatable in gameObject.GetComponents<PlayerActivatable>())
        {
            if (playerActivatable is UseInventoryItemBase)
            {
                hasActivator = true;
            }
        }
        foreach (PlayerActivatable playerActivatable in gameObject.GetComponents<PlayerActivatable>())
        {
            bool ignoreActivators = (playerActivatable is UseInventoryItemBase) || (hasActivator == false);
            playerActivatable.canBeActivated = ignoreActivators && mode;
        }
    }

    [YarnCommand("activate")]
    public static void Activate(string target)
    {
        GameObject gameObject = FindGameObject(target);
        if (gameObject == null) return;
        foreach (PlayerActivatable playerActivatable in gameObject.GetComponents<PlayerActivatable>())
        {
            playerActivatable.Activate();
        }
    }

    [YarnCommand("enable_controls")]
    public static void EnableControls(string target)
    {
        var togglePlayerControls = FindComponentOnObject<TogglePlayerControls>(target);
        if (togglePlayerControls == null) return;
        togglePlayerControls.EnableControls();
    }

    [YarnCommand("disable_controls")]
    public static void DisableControls(string target)
    {
        var togglePlayerControls = FindComponentOnObject<TogglePlayerControls>(target);
        if (togglePlayerControls == null) return;
        togglePlayerControls.DisableControls();
    }

    [YarnCommand("give_item")]
    public static void GiveItem(string target, string id)
    {
        var inventory = FindComponentOnObject<Inventory>(target);
        inventory.AddItem(id);
    }

    [YarnCommand("take_item")]
    public static void TakeItem(string target, string id)
    {
        var inventory = FindComponentOnObject<Inventory>(target);
        inventory.RemoveItem(id);
    }


    [YarnCommand("hide_path")]
    [System.Obsolete]
    public static void HidePath()
    {
        var pathVisualizer = GameObject.FindFirstObjectByType<PathVisualizer>();
        if (pathVisualizer != null)
        {
            pathVisualizer.ClearPath();
        }
    }

    [YarnCommand("show_path")]
    [System.Obsolete]
    public static System.Collections.IEnumerator ShowPath(string fromName, string toName, bool shouldWait = true, float minDistance = DefaultMinDistance)
    {
        var pathVisualizer = GameObject.FindFirstObjectByType<PathVisualizer>();
        if (pathVisualizer != null)
        {
            GameObject from = FindGameObject(fromName);
            GameObject to = FindGameObject(toName);
            if (from != null && to != null)
            {
                pathVisualizer.SetTargets(from.transform, to.transform);
                if (shouldWait)
                {
                    while (Vector3.Distance(from.transform.position, to.transform.position) > minDistance)
                    {
                        yield return null;
                    }
                    pathVisualizer.ClearPath();
                }
            }
        }
    }

    [YarnFunction("get_distance")]
    public static float GetDistanceByName(string from, string to)
    {
        GameObject object1 = FindGameObject(from);
        GameObject object2 = FindGameObject(to);
        return GetDistance(object1, object2);
    }

    private static float GetDistance(GameObject gameObject, GameObject other)
    {
        Vector3 delta = other.transform.position - gameObject.transform.position;
        return delta.magnitude;
    }

    [YarnCommand("wait_for_location")]
    public static IEnumerator WaitToReachTarget(GameObject gameObject, GameObject target, float treshold = 1.5f)
    {
        float distance = 0f;
        do
        {
            distance = GetDistance(gameObject, target);
            yield return null;
        } while (distance > treshold);
    }

    [YarnCommand("set_spawn")]
    public static void SetRespawnPoint(string targetName, string spawnPointName)
    {
        var respawnBehaviour = FindComponentOnObject<RespawnBehaviour>(targetName);
        if (respawnBehaviour == null) return;
        GameObject spawnPoint = FindGameObject(spawnPointName);
        if (spawnPoint != null)
        {
            respawnBehaviour.SetRespawnPoint(spawnPoint);
        }
    }

    [YarnCommand("respawn")]
    public static void Respawn(string targetName)
    {
        var respawnBehaviour = FindComponentOnObject<RespawnBehaviour>(targetName);
        if (respawnBehaviour == null) return;
        respawnBehaviour.Respawn();
    }

    [YarnCommand("teleport")]
    public static void Teleport(string targetName, string locationName)
    {
        var respawnBehaviour = FindComponentOnObject<RespawnBehaviour>(targetName);
        if (respawnBehaviour == null) return;
        GameObject location = FindGameObject(locationName);
        if (location == null) return;
        respawnBehaviour.TeleportTo(location.transform);
    }

    [YarnCommand("say")]
    [System.Obsolete]
    public static IEnumerator Say2(string targetName, string message, float offset_y = 0.0f, bool shouldWait = true)
    {
        GameObject target = FindGameObject(targetName);
        if (target == null)
        {
            return null;
        }
        else
        {
            Vector3 offset = new Vector3(0f, offset_y, 0f);
            return GameObject.FindFirstObjectByType<SpeechBubbleManager>().AddSpeechBubble(target, offset, message, shouldWait);
        }
    }

    [YarnCommand("toggle_animation")]
    public static void ToggleAnimationStates(string targetName)
    {
        AnimationToggle animationToggle = FindComponentOnObject<AnimationToggle>(targetName);
        if (animationToggle != null)
        {
            animationToggle.ToggleAnimationStates();
        }
    }

    [YarnCommand("animate")]
    public static void Animate(string targetName)
    {
        SimpleAnimationTrigger simpleAnimationTrigger = FindComponentOnObject<SimpleAnimationTrigger>(targetName);
        if (simpleAnimationTrigger != null)
        {
            simpleAnimationTrigger.Animate();
        }
    }

    [YarnCommand("walk_to")]
    public static IEnumerator WalkTo(string targetName, string destinationName, float distance = 0f, bool shouldWait = true)
    {
        WalkToBehaviour target = FindComponentOnObject<WalkToBehaviour>(targetName);
        if (target != null)
        {
            GameObject destination = FindGameObject(destinationName);
            return target.WalkTo(destination, distance, shouldWait);
        }
        else
        {
            return null;
        }
    }

    [YarnCommand("playerpref_save_string")]
    public static void PlayerPrefSaveString(string id, string value)
    {
        PlayerPrefs.SetString(id, value);
        PlayerPrefs.Save();
    }

    [YarnCommand("playerpref_save_int")]
    public static void PlayerPrefSaveInt(string id, int value)
    {
        PlayerPrefs.SetInt(id, value);
        PlayerPrefs.Save();
    }

    [YarnCommand("playerpref_save_bool")]
    public static void PlayerPrefSaveBool(string id, bool value)
    {
        PlayerPrefs.SetInt(id, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    [YarnFunction("playerpref_load_string")]
    public static string PlayerPrefLoadString(string id)
    {
        if (PlayerPrefs.HasKey(id))
        {
            return PlayerPrefs.GetString(id);
        }
        else
        {
            Debug.LogWarning($"Key '{id}' not found in PlayerPrefs.");
            return string.Empty;
        }
    }

    [YarnFunction("playerpref_load_int")]
    public static int PlayerPrefLoadInt(string id)
    {
        if (PlayerPrefs.HasKey(id))
        {
            return PlayerPrefs.GetInt(id);
        }
        else
        {
            Debug.LogWarning($"Key '{id}' not found in PlayerPrefs.");
            return 0; // or another default value
        }
    }

    [YarnFunction("playerpref_load_bool")]
    public static bool PlayerPrefLoadBool(string id)
    {
        if (PlayerPrefs.HasKey(id))
        {
            return PlayerPrefs.GetInt(id) == 1;
        }
        else
        {
            Debug.LogWarning($"Key '{id}' not found in PlayerPrefs.");
            return false; // or another default value
        }
    }

    [YarnFunction("playerpref_exists")]
    public static bool PlayerPrefExists(string id)
    {
        return PlayerPrefs.HasKey(id);
    }

    [YarnFunction("get_scene")]
    public static string GetCurrentSceneName()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        return currentScene.name;
    }

    [YarnCommand("load_scene")]
    public static void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    [YarnCommand("log")]
    public static void ShowMessageInLog(string text)
    {
        Debug.Log(text);
    }

    [YarnCommand("wait_until_closed")]
    public static IEnumerator WaitUntilClosed(string targetName)
    {
        GameObject gameObject = null;
        do
        {
            gameObject = GameObject.Find(targetName);
            yield return new WaitForSeconds(0.1f);
        } while (gameObject == null);
        do
        {
            gameObject = GameObject.Find(targetName);
            yield return new WaitForSeconds(0.1f);
        } while (gameObject != null);
    }

    [YarnCommand("show_menu")]
    public static void ShowMenu(string menuName)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == menuName)
            {
                obj.SetActive(true);
                return;
            }
        }
        Debug.LogWarning("UI not found: " + menuName);
    }

    [YarnCommand("run_command")]
    public static void RunCommand(string objectName, string scriptName, string methodName)
    {
        // Find the GameObject by name
        GameObject targetObject = GameObject.Find(objectName);
        if (targetObject == null)
        {
            Debug.LogError($"GameObject '{objectName}' not found.");
            return;
        }

        // Get the component by name
        var component = targetObject.GetComponent(scriptName);
        if (component == null)
        {
            Debug.LogError($"Script '{scriptName}' not found on GameObject '{objectName}'.");
            return;
        }

        // Call the method using reflection
        var method = component.GetType().GetMethod(methodName);
        if (method == null)
        {
            Debug.LogError($"Method '{methodName}' not found in script '{scriptName}'.");
            return;
        }

        method.Invoke(component, null);
    }

    [YarnCommand("move_item_to")]
    public static void MoveItemTo(string itemToMoveName, string destinationItemName)
    {
        GameObject itemToMove = FindGameObject(itemToMoveName);
        GameObject destinationItem = FindGameObject(destinationItemName);
        if (itemToMove != null && destinationItem != null)
        {
            itemToMove.transform.position = destinationItem.transform.position;
            itemToMove.transform.rotation = destinationItem.transform.rotation;
        }
    }

    [YarnCommand("set_position")]
    public static void SetPosition(string itemToPositionName, float x, float y, float z, bool localPosition = false)
    {
        GameObject item = FindGameObject(itemToPositionName);
        if (item != null)
        {
            if (localPosition)
            {
                item.transform.localPosition = new Vector3(x, y, z);
            }
            else
            {
                item.transform.position = new Vector3(x, y, z);
            }
        }

    }

    [YarnCommand("set_rotation")]
    public static void SetRotation(string itemToPositionName, float x, float y, float z, bool localRotation = false)
    {
        GameObject item = FindGameObject(itemToPositionName);
        if (item != null)
        {
            if (localRotation)
            {
                item.transform.localEulerAngles = new Vector3(x, y, z);
            }
            else
            {
                item.transform.eulerAngles = new Vector3(x, y, z);
            }
        }

    }

    [YarnCommand("radio_toggle")]
    public static void ToggleRadio(string objectName)
    {
        GameObject obj = FindGameObject(objectName);
        if (obj == null) return;

        var audio = obj.GetComponent<ModifiedAudioTrigger>();
        if (audio != null)
        {
            audio.ToggleAudio();
        }
        else
        {
            Debug.LogWarning($"ModifiedAudioTrigger not found on {objectName}");
        }
    }

    [YarnCommand("camera")]
    public static void CameraCommand(string camName)
    {
        CameraDirector director = GameObject.FindFirstObjectByType<CameraDirector>();

        if (director == null)
        {
            Debug.LogError("CameraDirector not found in scene.");
            return;
        }

        director.ActivateCamera(camName);
    }

    [YarnCommand("camera_focus")]
    public static void CameraFocus(string objectName)
    {
        CameraDirector director = GameObject.FindFirstObjectByType<CameraDirector>();
        GameObject target = GameObject.Find(objectName);

        if (director != null && target != null)
            director.FocusTarget(target);
    }

    [YarnCommand("camera_return")]
    public static void CameraReturn()
    {
        CameraDirector director = GameObject.FindFirstObjectByType<CameraDirector>();

        if (director != null)
            director.ReturnPlayer();
    }

    [YarnCommand("look_at")]
    public static void LookAt(string objectName)
    {
        CameraDirector director = GameObject.FindFirstObjectByType<CameraDirector>();
        GameObject target = GameObject.Find(objectName);

        if (director != null && target != null)
            director.LookAtTarget(target);
    }

    [YarnCommand("look_reset")]
    public static void LookReset()
    {
        CameraDirector director = GameObject.FindFirstObjectByType<CameraDirector>();

        if (director != null)
            director.ResetLookAt();
    }

    [YarnCommand("face")]
    public static IEnumerator Face(string objectName, string targetName, float speed = 5f)
    {
        GameObject obj = FindGameObject(objectName);
        GameObject target = FindGameObject(targetName);

        if (obj == null || target == null)
            yield break;

        Vector3 direction = target.transform.position - obj.transform.position;
        direction.y = 0;

        if (direction.sqrMagnitude < 0.001f)
            yield break;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        while (Quaternion.Angle(obj.transform.rotation, targetRotation) > 1f)
        {
            obj.transform.rotation = Quaternion.Slerp(
                obj.transform.rotation,
                targetRotation,
                speed * Time.deltaTime
            );

            yield return null;
        }

        obj.transform.rotation = targetRotation;
    }

    [YarnCommand("show_text")]
    public static void ShowText(string objectName)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == objectName)
            {
                obj.SetActive(true);
            }
        }
    }

    [YarnCommand("hide_text")]
    public static void HideText(string objectName)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == objectName)
            {
                obj.SetActive(false);
            }
        }
    }

    [YarnCommand("complete_text")]
    public static void CompleteText(string objectName)
    {
        TMP_Text text = FindTMPText(objectName);

        if (text == null)
            return;

        text.text = "<s>" + text.text + "</s>";
    }

    [YarnCommand("uncomplete_text")]
    public static void UncompleteText(string objectName)
    {
        TMP_Text text = FindTMPText(objectName);

        if (text == null)
            return;

        text.text =
            text.text
                .Replace("<s>", "")
                .Replace("</s>", "");
    }

    [YarnCommand("set_text")]
    public static void SetText(
    string objectName,
    string newText)
    {
        TMP_Text text = FindTMPText(objectName);

        if (text != null)
            text.text = newText;
    }

    [YarnCommand("complete_and_hide")]
    public static IEnumerator CompleteAndHide(
    string objectName,
    float delay = 3f)
    {
        TMP_Text text = FindTMPText(objectName);

        if (text == null)
            yield break;

        text.text = "<s>" + text.text + "</s>";

        yield return new WaitForSeconds(delay);

        text.gameObject.SetActive(false);
    }

    private static TMP_Text FindTMPText(string objectName)
    {
        GameObject obj = FindGameObject(objectName);

        if (obj == null)
            return null;

        TMP_Text text = obj.GetComponent<TMP_Text>();

        if (text == null)
            Debug.LogError($"{objectName} has no TMP_Text component");

        return text;
    }

    [YarnCommand("debug_find")]
    public static void DebugFind(string objectName)
    {
        Debug.Log($"Looking for: {objectName}");

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == objectName)
            {
                Debug.Log("FOUND");
            }
        }

        Debug.Log("NOT FOUND");
    }

    [YarnCommand("play_audio")]
    public static void PlayAudio(string objectName)
    {
        GameObject obj = FindGameObject(objectName);

        if (obj == null)
            return;

        AudioSource audioSource = obj.GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError($"{objectName} has no AudioSource component");
            return;
        }

        audioSource.Play();
    }

    [YarnCommand("stop_audio")]
    public static void StopAudio(string objectName)
    {
        GameObject obj = FindGameObject(objectName);

        if (obj == null)
            return;

        AudioSource audioSource = obj.GetComponent<AudioSource>();

        if (audioSource != null)
            audioSource.Stop();
    }

    [YarnCommand("collect_evidence")]
    public static void CollectEvidence(string storyId, string markerId)
    {
        DataRigController dataRig = FindDataRigController();
        if (dataRig == null)
        {
            Debug.LogWarning("collect_evidence: DataRigController not found in scene.");
            return;
        }

        bool collected = false;
        bool hasSpecificStoryId = !string.IsNullOrWhiteSpace(storyId) &&
                                  !string.Equals(storyId, "storyId", System.StringComparison.OrdinalIgnoreCase);

        if (hasSpecificStoryId)
        {
            collected = dataRig.TryCollectEvidenceByStoryId(storyId, markerId, EvidenceSource.Dialogue);
        }

        if (!collected)
        {
            // Fallback: find the marker in any currently loaded Data-Rig story.
            collected = dataRig.TryCollectEvidenceByMarkerId(markerId, EvidenceSource.Dialogue);
        }

        if (!collected)
        {
            Debug.LogWarning($"collect_evidence: Could not collect '{markerId}' for story '{storyId}'.");
        }
    }

    [YarnCommand("set_patrol")]
    public static void SetPatrol(string targetName, bool enabled)
    {
        NpcPatrolBehaviour patrol = FindComponentOnObject<NpcPatrolBehaviour>(targetName);
        if (patrol != null)
        {
            patrol.SetPatrolEnabled(enabled);
        }
    }

    private static DataRigController FindDataRigController()
    {
        DataRigController activeController = GameObject.FindFirstObjectByType<DataRigController>();
        if (activeController != null)
        {
            return activeController;
        }

        DataRigController[] allControllers = Resources.FindObjectsOfTypeAll<DataRigController>();
        foreach (DataRigController controller in allControllers)
        {
            if (controller == null || !controller.gameObject.scene.IsValid())
            {
                continue;
            }

            return controller;
        }

        return null;
    }
}
