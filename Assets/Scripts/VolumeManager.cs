using UnityEngine;
using UnityEngine.UI;

/***
 * 
 * VolumeManager, responsible for adjusting the volume level of different audio sources
 * 
 * Written by aleu0007
 * Last Modified: 6/08/2025
 * 
 */
public class VolumeManager : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (PlayerPrefs.HasKey("soundVolume"))
            LoadVolume();
        else
        {
            PlayerPrefs.SetFloat("soundVolume", 1);
            LoadVolume();
        }
    }

    public void SetVolume()
    {
        AudioListener.volume = volumeSlider.value;
        SaveVolume();
    }

    public void SaveVolume()
    {
        PlayerPrefs.SetFloat("soundVolume", volumeSlider.value);
    }

    public void LoadVolume()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("soundVolume");
    }
}
