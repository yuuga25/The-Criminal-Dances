using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName ="BGM_Data", fileName ="BGM_Data")]
public class BGMSetManager : ScriptableObject
{
    public List<BGMData> sheet;

    [Serializable]
    public class BGMData
    {
        public string bgmName;
        public AudioClip bgmClip;
    }
}
