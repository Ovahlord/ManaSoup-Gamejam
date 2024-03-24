using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHUDManager : MonoBehaviour
{
    [SerializeField] private TMP_Text pickupInfoText = null;
    [SerializeField] private CanvasGroup menuCanvasGroup = null;
    private static PlayerHUDManager instance = null;


    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(instance);

        instance = this;

    }

    public static void SetPickupValues(int required, int gatheredPickups)
    {
        instance.pickupInfoText.text = $"{ gatheredPickups } / { required }";
    }

    public static void ToggleMenu(bool show)
    {
        instance.menuCanvasGroup.alpha = show ? 1 : 0;
        instance.menuCanvasGroup.interactable = show;
    }

    public static void RestartSceneButtonPressed()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public static void ExitGameButtonPressed()
    {
        Application.Quit();
    }

    public static void AudioVolumeSliderChanged(System.Single vol)
    {
        AudioListener.volume = vol;
    }

}
