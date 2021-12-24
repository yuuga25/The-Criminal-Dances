using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSpectrumScript : MonoBehaviour
{
    private AudioSpectrum spectrum;
    private Transform[] images;

    public GameObject SpectrumParent;
    public float scale;

    private void Start()
    {
        int imagePos = 0;
        spectrum = GetComponent<AudioSpectrum>();
        images = new Transform[SpectrumParent.transform.childCount];
        for(int i = 0; i < SpectrumParent.transform.childCount; i++)
        {
            images[i] = SpectrumParent.transform.GetChild(i).gameObject.transform;
            var pos = images[i].transform.localPosition;
            pos.x = imagePos;
            images[i].transform.localPosition = pos;
            imagePos += 45;
        }
    }

    private void Update()
    {
        for(int i = 0; i < images.Length; i++)
        {
            var image = images[i];
            var localScale = image.localScale;
            localScale.y = spectrum.Levels[i] * scale;
            image.localScale = localScale;
        }
    }
}
