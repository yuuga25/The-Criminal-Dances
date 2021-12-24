using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class BGMController : MonoBehaviour
{
    [Header("ランダム化")] public bool isRandom = true;
    [Space(20)]
    public AudioSource audioSource_bgm;
    public BGMSetManager bgm_Data;
    public Text text_BGMName;
    public Slider slider_BGMTime;

    public AudioClip SecretData;

    public GameObject volumeSlider;

    public Image muteButton;
    public Image pauseButton;
    public Sprite[] audioImage = new Sprite[2];

    public Text text_BGMTrackName;
    public BGMTrack tracksData;
    public BGMSetManager SecretTrack;

    public Image repeatButton;
    public Sprite[] repeat_RandomImage;
    private int nowBGMClip;

    public Text timerText;

    private void Start()
    {
        timerText.gameObject.SetActive(false);

        if(BGMSettings.PlayingClip == null)
        {
            temporaryTrackID = 0;
            volumeSlider.GetComponent<Slider>().value = 0.2f;
            if(PlayerPrefs.GetFloat("VOLUME") != 0)
            {
                volumeSlider.GetComponent<Slider>().value = PlayerPrefs.GetFloat("VOLUME");
                print("ロード完了");
            }
            text_BGMTrackName.text = $"Track{tracksData.sheet[0].trackId}　{tracksData.sheet[0].trackName}";
            ChangeBGM();
        }
        else
        {
            SetBGM();
        }
    }
    private void Update()
    {
        slider_BGMTime.value = audioSource_bgm.time;

        int time = Mathf.FloorToInt(slider_BGMTime.maxValue - slider_BGMTime.value);

        int minute = time / 60;
        int seconds = time % 60;

        timerText.text = $"{minute}:{seconds.ToString("00")}";

        audioSource_bgm.volume = volumeSlider.GetComponent<Slider>().value;
        if (Input.GetKey(KeyCode.Escape))
        {
            if (Input.GetKey(KeyCode.V))
            {
                if (Input.GetKey(KeyCode.Space))
                {
                    if(audioSource_bgm.clip != SecretData)
                    {
                        audioSource_bgm.clip = SecretData;
                        audioSource_bgm.Play();
                        text_BGMName.text = "～ SecretData ～";
                        slider_BGMTime.minValue = 0;
                        slider_BGMTime.maxValue = audioSource_bgm.clip.length;
                    }
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if(audioSource_bgm.time + 5 > audioSource_bgm.clip.length)
            {
                audioSource_bgm.time = audioSource_bgm.clip.length - 0.1f;
            }
            else
            {
                audioSource_bgm.time += 5;
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if(audioSource_bgm.time - 5 < 0)
            {
                audioSource_bgm.time = audioSource_bgm.time + audioSource_bgm.clip.length - 5;
            }
            else
            {
                audioSource_bgm.time -= 5;
            }
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKey(KeyCode.RightControl))
            {
                if (Input.GetKey(KeyCode.Backspace))
                {
                    if(audioSource_bgm.clip.name == "greed")
                    {
                        bgm_Data = SecretTrack;
                        temporaryTrackID = 0;
                        text_BGMTrackName.text = $"Track ScretTrack";
                        ChangeBGM();
                    }
                }
            }
        }

        if (volumeSlider.GetComponent<Slider>().value == 0) muteButton.sprite = audioImage[1];
        else muteButton.sprite = audioImage[0];
    }
    public void ChangeBGM()
    {
        audioSource_bgm.time = 0;

        if (isRandom)
        {
            var r = Random.Range(0, bgm_Data.sheet.Count);
            audioSource_bgm.clip = bgm_Data.sheet[r].bgmClip;
            audioSource_bgm.Play();
            pauseButton.sprite = audioImage[2];

            text_BGMName.text = $"～ {bgm_Data.sheet[r].bgmName} ～";
            slider_BGMTime.minValue = 0;
            slider_BGMTime.maxValue = audioSource_bgm.clip.length;

            nowBGMClip = r;
        }
        else
        {
            var r = nowBGMClip + 1;

            if(r > bgm_Data.sheet.Count - 1)
            {
                r = 0;
            }

            audioSource_bgm.clip = bgm_Data.sheet[r].bgmClip;
            audioSource_bgm.Play();
            pauseButton.sprite = audioImage[2];

            text_BGMName.text = $"～ {bgm_Data.sheet[r].bgmName} ～";
            slider_BGMTime.minValue = 0;
            slider_BGMTime.maxValue = audioSource_bgm.clip.length;

            nowBGMClip = r;
        }

        print("BGMナンバー:"+nowBGMClip);
    }
    public void MuteBGM()
    {
        volumeSlider.SetActive(true);
    }
    public void PauseBGM()
    {
        if (audioSource_bgm.isPlaying)
        {
            audioSource_bgm.Pause();
            pauseButton.sprite = audioImage[3];
        }
        else
        {
            audioSource_bgm.Play();
            pauseButton.sprite = audioImage[2];
        }
    }
    public void ChangeMode()
    {
        if (isRandom)
        {
            isRandom = false;
            repeatButton.sprite = repeat_RandomImage[1];
        }
        else
        {
            isRandom = true;
            repeatButton.sprite = repeat_RandomImage[0];
        }
    }
    public void SliderClick()
    {
        audioSource_bgm.time = slider_BGMTime.value;
    }

    private int temporaryTrackID;
    public void ChangeTrack()
    {
        temporaryTrackID += 1;
        BGMTrack.TrackData nextTrack;
        print(temporaryTrackID);
        print(tracksData.sheet.Count);
        if (temporaryTrackID + 1 > tracksData.sheet.Count)
        {
            nextTrack = tracksData.sheet[0];
            bgm_Data = nextTrack.bgmSheet;
            temporaryTrackID = 0;
            print("初期化");
        }
        else
        {
            nextTrack = tracksData.sheet[temporaryTrackID];
            bgm_Data = nextTrack.bgmSheet;
            print("次のトラックへ");
        }
        text_BGMTrackName.text = $"Track{nextTrack.trackId}　{nextTrack.trackName}";
        ChangeBGM();
    }

    public void SetBGM()
    {
        bgm_Data = BGMSettings.setTrack;
        var clips = bgm_Data.sheet.Where((c) => c.bgmClip == BGMSettings.PlayingClip);
        foreach(var i in clips)
        {
            temporaryTrackID = BGMSettings.trackId;

            audioSource_bgm.clip = i.bgmClip;
            volumeSlider.GetComponent<Slider>().value = BGMSettings.AudioVolume;
            audioSource_bgm.time = BGMSettings.Playbacktime;

            text_BGMName.text = $"～ {i.bgmName} ～";
            slider_BGMTime.minValue = 0;
            slider_BGMTime.maxValue = audioSource_bgm.clip.length;

            if (BGMSettings.isPlaying)
            {
                audioSource_bgm.Play();
                pauseButton.sprite = audioImage[2];
            }
            else
            {
                audioSource_bgm.Pause();
                pauseButton.sprite = audioImage[3];
            }
        }
        text_BGMTrackName.text = $"Track{tracksData.sheet[temporaryTrackID].trackId}　{tracksData.sheet[temporaryTrackID].trackName}";
    }

    public void SetBGMData()
    {
        BGMSettings.setTrack = bgm_Data;
        BGMSettings.trackId = temporaryTrackID;
        BGMSettings.PlayingClip = audioSource_bgm.clip;
        BGMSettings.AudioVolume = audioSource_bgm.volume;
        BGMSettings.isPlaying = audioSource_bgm.isPlaying;
        BGMSettings.Playbacktime = audioSource_bgm.time;
    }
}

public class BGMSettings
{
    public static bool isRandom;            //ランダムかどうか
    public static AudioClip PlayingClip;    //再生中クリップ
    public static float AudioVolume;        //オーディオボリューム
    public static float Playbacktime;       //再生時間
    public static bool isPlaying;           //再生しているか
    public static BGMSetManager setTrack;   //再生しているトラック
    public static int trackId;              //再生しているトラックID
}
