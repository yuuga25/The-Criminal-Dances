using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "BGM_TrackData", fileName = "BGM_TrackData")]
public class BGMTrack : ScriptableObject
{
    public List<TrackData> sheet;

    [Serializable]
    public class TrackData
    {
        public string trackName;
        public int trackId;
        public BGMSetManager bgmSheet;
    }
}