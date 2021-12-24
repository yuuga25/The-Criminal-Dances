using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    public InputField nameSpace;
    public Slider volumeSlider;

    private void Awake()
    {
        if (PlayerPrefs.GetString("NAME") != null)
        {
            nameSpace.text = PlayerPrefs.GetString("NAME");
        }
    }

    public void Save()
    {
        PlayerPrefs.SetString("NAME", nameSpace.text);
        PlayerPrefs.SetFloat("VOLUME", volumeSlider.value);
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetFloat("VOLUME", volumeSlider.value);
        print("データ保存完了");
    }
}
