using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Cysharp.Threading.Tasks;
using System.Linq;

public class ConnectingScript : MonoBehaviourPunCallbacks
{
    [Header("ゲームシーン名")] public string gameScene;

    [Header("部屋ID")] public string roomId;

    [Header("タイトル画面")]
    [Header("UI")] public GameObject titleObj;
    [Header("ロード画面")] public GameObject loadObj;
    [Header("名前テキスト")] public Text name_Text;
    [Header("部屋IDテキスト")] public Text roomId_Text;
    public InputField roomId_InputField;

    [Header("人数")]
    public Slider num_Slider;
    public Text num_Text;

    private bool connect;
    private bool IsMaster;

    public static int peopleStaticNum;

    private int IsClientLoadCompleted;

    private bool isLoad;

    private PhotonView view;
    private Coroutine multiCoroutine;
    private  AsyncOperation asyncOperation;

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();//Photonに接続
        }
        connect = false;
        IsMaster = false;
        view = GetComponent<PhotonView>();

        roomId_InputField.text = "room";
    }
    private void Update()
    {
        peopleStaticNum = (int)num_Slider.value;
        num_Text.text = peopleStaticNum.ToString();
    }
    public void LoadCancel()
    {
        StopCoroutine(multiCoroutine);
        titleObj.SetActive(true);
        loadObj.SetActive(false);
    }
    //マルチプレイのボタンを押された時の処理
    public async void LoadScene()
    {
        await UniTask.WaitUntil(() => PhotonNetwork.InRoom);
        titleObj.SetActive(false);
        loadObj.SetActive(true);
        multiCoroutine = StartCoroutine(Load());
    }
    //シーンロードと相手のロード状況の同期
    private IEnumerator Load()
    {
        print("読み込み中");
        yield return new WaitUntil(() => PhotonNetwork.IsConnected == true);
        print("読み込み完了");
        if (asyncOperation == null)
        {
            asyncOperation = SceneManager.LoadSceneAsync(gameScene);
            asyncOperation.allowSceneActivation = false;
        }
        yield return new WaitUntil(() => asyncOperation.progress >= 0.9f);
        print("ロード完了");
        IsClientLoadCompleted = 0;
        while (true)
        {
            if (!IsMaster)
            {
                print("ロード完了をマスター通知");
                view.RPC("ClientComplete", RpcTarget.MasterClient);
            }
            else
            {
                print("他プレイヤーロード待ち");
                yield return new WaitUntil(() => IsClientLoadCompleted == peopleStaticNum - 1);
                view.RPC("LoadTrue", RpcTarget.AllViaServer, string.Join(" ", PhotonNetwork.PlayerList.Select(player => player.NickName)));
            }
            yield return new WaitUntil(() => isLoad == true);

            peopleStaticNum = PhotonNetwork.PlayerList.Length;

            print(peopleStaticNum);

            asyncOperation.allowSceneActivation = true;
            break;
        }
    }
    [PunRPC]
    private void SaveMenber(int value)
    {
        peopleStaticNum = value;
    }

    [PunRPC]
    private void ClientComplete()
    {
        print("クライアントロード完了 : "+IsClientLoadCompleted);
        IsClientLoadCompleted++;
    }
    [PunRPC]
    private void ClientCancel()
    {
        print("クライアントキャンセル");
        IsClientLoadCompleted--;
    }
    [PunRPC]
    private void LoadTrue(string nameStr)
    {
        isLoad = true;
    }
    public void SoloDebug()
    {
        if (connect)
        {
            StartCoroutine(Load());
        }
        else
        {
            Debug.LogError("ルームに入室してください");
        }
        IEnumerator Load()
        {
            yield return new WaitUntil(() => PhotonNetwork.NetworkClientState.ToString() == "Joined");
            SceneManager.LoadScene("BattleScene");
        }
    }
    //接続
    public void Connect()
    {
        if (!connect)//接続していなければ
        {
            StartCoroutine(Connect());//マスターサーバーへのアクセスチェック
        }
        IEnumerator Connect()
        {
            yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);//マスターサーバーにアクセスするまで待機
            string roomName = roomId_Text.text;
            PhotonNetwork.JoinOrCreateRoom(roomName, new RoomOptions() { MaxPlayers = (byte)peopleStaticNum,}, TypedLobby.Default);//ルームがあれば接続なければ生成
            print("Connect");
            connect = true;//接続へ変更
        }
    }
    //切断
    public void DisConnect()
    {
        if (connect)//接続していたら
        {
            view.RPC("ClientCancel", RpcTarget.MasterClient);
            PhotonNetwork.LeaveRoom();
            connect = false;
            print("DisConect");
        }
    }
    public override void OnConnectedToMaster()
    {
        print("マスターサーバーにアクセス");
    }//マスターサーバーにアクセスしているか
    public override void OnJoinedRoom()
    {
        Room myRoom = PhotonNetwork.CurrentRoom;
        Player player = PhotonNetwork.LocalPlayer;
        print("join");
        print($"MyNumber : {player.ActorNumber}");
        print("ルームマスター : " + player.IsMasterClient);
        IsMaster = player.IsMasterClient;
        PhotonNetwork.NickName = name_Text.text;
    }
}
