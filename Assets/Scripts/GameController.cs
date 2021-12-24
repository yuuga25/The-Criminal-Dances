using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Cysharp.Threading.Tasks;

public class GameController : MonoBehaviour
{
    [Header("UI")]
    public Color[] color_Cards;
    public string[] name_CardImageObjs;
    public GameObject obj_CardParent;
    public GameObject obj_CardContent;
    public GameObject incidentInputObj;
    public Text incidentText;
    public Button incidentButton;
    public InputField incidentField;
    public GameObject playerSelecctParent;
    public GameObject playerSelectPrefab;
    public GameObject cardSelectParent;
    public GameObject cardSelectPrefab;

    public GameObject textObj_Content;
    public GameObject textObj_Parent;

    public GameObject game;
    public GameObject result;
    public GameObject wonPlayer_Parent;

    public Button reStartButton;
    public Button gameEndButton;

    private List<string> CriminalSide = new List<string>();

    private List<Button> playerSelectButton = new List<Button>();
    private List<GameObject> cardCover = new List<GameObject>();

    #region カードの詳細
    /* 0 : 第一発見者
     * 1 : 犯人
     * 2 : 探偵
     * 3 : アリバイ
     * 4 : たくらみ
     * 5 : いぬ
     * 6 : 少年
     * 7 : うわさ
     * 8 : 一般人
     * 9 : 目撃者
     * 10: 情報操作
     * 11: 取り引き*/
    #endregion


    #region プライベート変数
    private int turn;
    private int turnCount;
    private int myNumber;//0から
    private int playerCount;
    private int maxMyCardCount = 4;
    private int weekCount;
    private int Turn
    {
        get { return turn; }
        set
        {
            if (value == -1) value = playerCount - 1;
            this.turn = value;
        }
    }//0だったら自分のターン
    private int procesDone;

    private bool isMaster;
    private bool isTurn;
    private bool isStarter;

    private List<int> allCard = new List<int>()
    {
        1,5,0,6,
        8,8,
        4,4,
        9,9,9,
        10,10,10,
        2,2,2,2,
        7,7,7,7,
        3,3,3,3,3,
        11,11,11,11,11

    };
    private List<int> deck = new List<int>();//山札
    private List<int> myCard = new List<int>();
    private List<int> allPlayerCardCount = new List<int>();
    private List<(string, string)> cardInfo = new List<(string, string)>()
    {
        ("第一発見者","このカードを出してゲームを始める。今回起こった事件を考えて、全員に伝えよう。"),
        ("犯人","探偵にあてられてしまうと負け。最後の手札1枚のときだけ出せる。出せたなら勝ち。"),
        ("探偵","他のだれか1人に「あなたが犯人ですね?」と聞く。当たれば勝ち。\n2週目になるまでは使えない。"),
        ("アリバイ","手札にあれば、「犯人ではありません。」と答えられる。\n出しても何も起きない。"),
        ("たくらみ","出すと、犯人の味方になる。\n犯人が勝つと、同じく勝ち、犯人が負けると同じく負け。"),

        ("いぬ","他のだれか一人の手札を1枚選ぶ。\n選んだカードを全員に見せる。そのカードが犯人なら勝ち。犯人でないなら元にもどす。"),
        ("少年","他全員に指示して犯人を知る。①「はいみなさん、目を閉じて」②「犯人カードを持っている人は目を開けて」③「みなさん、目をあけて」"),
        ("うわさ","全員自分の右となりの人の手札からこっそり1枚ひく。"),
        ("一般人","出しても何も起きない。"),
        ("目撃者","他のだれか一人の手札をこっそり全部見せてもらう。"),
        ("情報操作","全員、自分の左隣の人に手札の1枚をこっそりわたす。"),
        ("取り引き","他のだれか一人と、手札の1枚をこっそり交換し合う。\n（手札がないなら交換しない）"),
    };

    private Dictionary<int, List<int>> startCard = new Dictionary<int, List<int>>()
    {
        {3,new List<int>(){0,1,2,3} },
        {4,new List<int>(){0,1,2,3} },
        {5,new List<int>(){0,1,2,3,3,4} },
        {6,new List<int>(){0,1,2,2,3,3,4,4} },
        {7,new List<int>(){0,1,2,3,3,4,4,4,5,5,} }
    };
    private Dictionary<int, int> startOtherCard = new Dictionary<int, int>()
    {
        {3,8 },
        {4,11 },
        {5,14 },
        {6,16 },
        {7,19 }
    };

    private PhotonView view;
    #endregion

    private void Awake()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadSceneAsync("TitleScene");
        }
    }

    private async void Start()
    {
        incidentInputObj.SetActive(false);
        cardSelectParent.SetActive(false);
        playerSelecctParent.SetActive(false);

        playerCount = PhotonNetwork.PlayerList.Length;
        isMaster = PhotonNetwork.IsMasterClient;
        myNumber = PhotonNetwork.PlayerList.ToList().IndexOf(PhotonNetwork.LocalPlayer);

        //手持ちカード欄リセット
        foreach (Transform obj in obj_CardParent.transform)
        {
            Destroy(obj.gameObject);
        }

        //他プレイヤーの選択欄初期化
        foreach (Transform obj in playerSelecctParent.transform)
        {
            Destroy(obj.gameObject);
        }
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            var content = Instantiate(playerSelectPrefab, playerSelecctParent.transform).transform.Find("Card_Child");

            content.Find("Text_PlayerName").GetComponent<Text>().text = PhotonNetwork.PlayerList[i].NickName;//名前設定
            playerSelectButton.Add(content.GetComponent<Button>());
            if (i == myNumber)
                content.parent.gameObject.SetActive(false);
        }

        view = GetComponent<PhotonView>();
        if (isMaster)
        {

            CreateDeck();//山札生成

            for (int i = 0; i < playerCount; i++)
            {
                for (int j = 0; j < maxMyCardCount; j++)//4枚配る
                {
                    view.RPC("GiveCard", RpcTarget.AllViaServer, i, deck[0]);
                    deck.RemoveAt(0);
                    await UniTask.Delay(250);
                }
            }
            view.RPC("TurnStart", RpcTarget.AllViaServer);
        }
        for (int i = 0; i < playerCount; i++)
            allPlayerCardCount.Add(4);
    }

    private void CreateDeck()
    {
        for (int i = 0; i < startCard[playerCount].Count; i++)//初めに絶対必要なカードだけ追加
        {
            deck.Add(startCard[playerCount][i]);
            allCard.Remove(startCard[playerCount][i]);
        }
        allCard = allCard.Select(i => i).OrderBy(i => Guid.NewGuid()).ToList();//シャッフル
        for (int i = 0; i < startOtherCard[playerCount]; i++)
        {
            deck.Add(allCard[i]);
        }
        deck = deck.Select(i => i).OrderBy(i => Guid.NewGuid()).ToList();//シャッフル
    }

    [PunRPC]
    private void GiveCard(int playerNumber, int cardNum)
    {
        if (myNumber == playerNumber)
        {
            myCard.Add(cardNum);

            //カード生成_UI
            var content = Instantiate(obj_CardContent, obj_CardParent.transform).transform.Find("Card_Child");
            SetCardContent(content, cardNum);

            int ii = cardCover.Count;
            content.GetComponent<Button>().onClick.AddListener(() => UseCard(ii));

            cardCover.Add(content.parent.Find("Card_Cover").gameObject);

            Animator anim = content.parent.GetComponent<Animator>();
            anim.SetTrigger("setCard");

            //第一発見者ならば他のプレイヤーの順番を決める
            if (cardNum == 0)
            {
                print("第一発見者");
                isTurn = isStarter = true;
                List<int> turnLi = Enumerable.Range(0, playerCount).ToList().Select(i => i).OrderBy(i => Guid.NewGuid()).ToList();
                turnLi.Remove(myNumber);
                for (int i = 1; i < playerCount; i++)
                {
                    view.RPC("Ordering", RpcTarget.AllViaServer, turnLi[i - 1], i);//順番決め
                    print(i);
                }
            }
        }
    }

    [PunRPC]
    private void Ordering(int playerNumber, int turn)
    {
        if (this.myNumber == playerNumber)
        {
            print("ordering");
            this.Turn = turn;
            print("Turn" + turn);
        }
    }


    //シーンから呼び出す
    public async void UseCard(int index)
    {
        int useCardId = myCard[index];
        myCard.Remove(useCardId);
        CardDisplayUpdate();

        view.RPC("PutPlay", RpcTarget.All, useCardId, myNumber);//カードの使用を通知

        //すべてのカードを出せないようにする                                                                     
        foreach (var obj in cardCover)
        {
            obj.SetActive(true);
        }

        //これのなかにカード使用に関する処理全部突っ込んどいた
        List<Func<UniTask>> method = new List<Func<UniTask>>()
        {
            ()=>Card0(),
            ()=>Card1(),
            ()=>Card2(),
            ()=>Card3(),
            ()=>Card4(),
            ()=>Card5(),
            ()=>Card6(),
            ()=>Card7(),
            ()=>Card8(),
            ()=>Card9(),
            ()=>Card10(),
            ()=>Card11(),
        };
        await method[useCardId]();


        print("TurnEnd");
        view.RPC("TurnEnd", RpcTarget.AllViaServer);
        view.RPC("ProcessReset", RpcTarget.AllViaServer);
        view.RPC("CardDisplayUpdate", RpcTarget.AllViaServer);
    }


    #region 第一発見者
    private async UniTask Card0()
    {
        //事件が起こった
        incidentInputObj.SetActive(true);
        await incidentButton.OnClickAsync();//ボタンのされるまで待機
        view.RPC("GameStart", RpcTarget.AllViaServer, incidentField.text);
        view.RPC("AddTextObject", RpcTarget.All, $"事件内容：{incidentField.text}。\n犯人を見つけよう。");
        incidentInputObj.SetActive(false);
    }
    [PunRPC]
    private void GameStart(string incident)
    {
        print(incident);
        incidentText.text = incident;
    }
    #endregion

    #region 犯人
    private async UniTask Card1()
    {
        view.RPC("AddCriminalSide", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);
        //CriminalSide.Add(PhotonNetwork.PlayerList[myNumber].NickName);
        view.RPC("GameEnd", RpcTarget.AllViaServer, true);//犯人の勝ち
        await UniTask.Delay(0);
    }
    #endregion

    #region 探偵

    private async UniTask Card2()
    {
        //相手を選択
        AddTextObject("犯人だと思う方を指名してください");
        int target = await SelectPlayer();

        print(target);

        view.RPC("Card2Que", RpcTarget.AllViaServer, target, myNumber);
    }

    //選択した相手へ犯人かどうかの確認
    [PunRPC]
    private void Card2Que(int target, int from)
    {
        if (myNumber == target)
        {
            view.RPC("AddTextObject", RpcTarget.All, $"{PhotonNetwork.PlayerList[target].NickName}が犯人だと疑われた。");
            print("犯人だと疑われた");
            bool isCriminal = false;

            //犯人カードを持ってたら
            if (myCard.Contains(1))
            {
                //アリバイカードを持っていなかったら
                if (!myCard.Contains(3))
                {
                    isCriminal = true;
                    view.RPC("AddCriminalSide", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);
                    //CriminalSide.Add(PhotonNetwork.PlayerList[myNumber].NickName);
                    print("犯人だということがバレた");
                    view.RPC("AddTextObject", RpcTarget.All, "犯人だった。");
                }
                else
                {
                    print("アリバイカードを持っていたためバレなかった");
                    view.RPC("AddTextObject", RpcTarget.All, "アリバイを証明した。");
                }
            }
            else
            {
                print("犯人カード持ってないから問題なかった");
                view.RPC("AddTextObject", RpcTarget.All, "犯人ではなかった。");
            }


            view.RPC("Card2Answer", RpcTarget.Others, from, isCriminal, myNumber);
        }
    }

    //選択した相手からの返答
    [PunRPC]
    private void Card2Answer(int from, bool isCriminal, int to)
    {
        if (myNumber == from)
        {
            //指名した相手が犯人だった
            if (isCriminal)
            {
                print(PhotonNetwork.PlayerList[to].NickName + "が犯人だった");
                view.RPC("GameEnd", RpcTarget.AllViaServer, false);//探偵の勝ち
            }
            else
            {
                print(PhotonNetwork.PlayerList[to].NickName + "は犯人じゃなかった");
            }
        }
    }
    #endregion

    #region アリバイ
    private async UniTask Card3()
    {
        print("アリバイカードを使った");
        view.RPC("AddTextObject", RpcTarget.All, $"特に何も起きなかった。");
        await UniTask.Delay(0);
        //効果なし
    }
    #endregion

    #region たくらみ
    private async UniTask Card4()
    {
        print("犯人サイドになった");
        view.RPC("AddTextObject", RpcTarget.All, $"{PhotonNetwork.LocalPlayer.NickName}は犯人側になりました。");
        view.RPC("AddCriminalSide", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);
        await UniTask.Delay(0);
    }
    #endregion

    #region いぬ
    private async UniTask Card5()
    {
        AddTextObject("カードを開示する人を選択してください。");
        //誰か一人を選ぶ
        int target = await SelectPlayer();

        AddTextObject("どのカードを開示するか選択してください。");
        //どのカードを開示させるか選ぶ
        int targetIndex = await BackCardSelect(allPlayerCardCount[target]);

        view.RPC("OpenReq", RpcTarget.Others, target, targetIndex,myNumber);

    }
    [PunRPC]
    private void OpenReq(int target, int index,int from)
    {
        if (myNumber == target)
        {
            view.RPC("OpenCard", RpcTarget.AllViaServer, target, myCard[index],from);
        }
    }
    [PunRPC]
    private void OpenCard(int target, int cardId,int from)
    {
        print(PhotonNetwork.PlayerList[target].NickName + "のカード" + cardInfo[cardId].Item1);
        view.RPC("AddTextObject", RpcTarget.All, $"{PhotonNetwork.PlayerList[target].NickName}が指名され\n開示されたカードは\n「{cardInfo[cardId].Item1}」でした。");

        //犯人カードを当てれたら
        if (cardId == 1&&myNumber==from)
        {
            //CriminalSide.Add(PhotonNetwork.PlayerList[target].NickName);
            view.RPC("AddCriminalSide", RpcTarget.All, PhotonNetwork.PlayerList[target].NickName);
            view.RPC("GameEnd", RpcTarget.AllViaServer, false);
        }
    }
    #endregion

    #region 少年
    private async UniTask Card6()
    {
        view.RPC("IsCriminalReq", RpcTarget.AllViaServer, myNumber);
        await UniTask.Delay(0);
    }
    [PunRPC]
    private void IsCriminalReq(int from)
    {
        if (myCard.Contains(1))//犯人カードを持っていたら
        {
            view.RPC("CriminalAns", RpcTarget.AllViaServer, myNumber, from);
        }
    }
    [PunRPC]
    private void CriminalAns(int criminalNum, int from)
    {
        //カードを出した本人だったら
        if (myNumber == from)
        {
            print(PhotonNetwork.PlayerList[criminalNum].NickName + "が犯人カードを持っている");
            AddTextObject(PhotonNetwork.PlayerList[criminalNum].NickName + "が犯人カードを持っている。");
        }
        else
        {
            print(PhotonNetwork.PlayerList[from].NickName + "が犯人を知った");
            AddTextObject(PhotonNetwork.PlayerList[from].NickName + "が犯人を知った。");
        }
    }
    #endregion

    #region うわさ
    private async UniTask Card7()
    {
        print("全てのカードの枚数　：　" + allPlayerCardCount.Sum());
        if (allPlayerCardCount.Sum() <= 1)
        {
            print("カードの交換ができない");
            return;
        }
        view.RPC("Roumer", RpcTarget.All);
        int selectedPla = allPlayerCardCount.Where(cardId => cardId != 0).Count();
        print(selectedPla);
        procesDone = 0;
        await UniTask.WaitUntil(() => procesDone == selectedPla);
    }
    [PunRPC]
    private async void Roumer()
    {
        if (myCard.Count == 0) return;

        var selectedPlaIndex = new List<int>();
        for (int i = 0; i < playerCount; i++)
        {
            if (allPlayerCardCount[i] != 0)
            {
                selectedPlaIndex.Add(i);
            }
        }

        var playerList = new List<Player>();
        foreach (var index in selectedPlaIndex)
        {
            playerList.Add(PhotonNetwork.PlayerList[index]);
        }

        int targetIndex = playerList.IndexOf(PhotonNetwork.LocalPlayer) - 1;
        if (targetIndex == -1) targetIndex = selectedPlaIndex.Count - 1;

        if (playerList.Count < 0)
        {
            Console.WriteLine("交換相手いない");
            view.RPC("AddTextObject", RpcTarget.All, $"特に何も起きなかった。");
            return;
        }

        Player targetPlayer = playerList[targetIndex];
        print(targetPlayer.NickName + "からカードをこっそり引く");
        AddTextObject(targetPlayer.NickName + "からカードをこっそり引く");

        int cardIndex = await BackCardSelect(allPlayerCardCount[PhotonNetwork.PlayerList.ToList().IndexOf(targetPlayer)]);
        view.RPC("RoumerReq", RpcTarget.Others, PhotonNetwork.PlayerList.ToList().IndexOf(playerList[targetIndex]), myNumber, cardIndex);
    }
    [PunRPC]private void RoumerReq(int target,int from,int cardIndex)
    {
        if (myNumber == target)//引かれる側
        {
            print(PhotonNetwork.PlayerList[from] + "に" + cardInfo[myCard[cardIndex]].Item1 + "を取られた");
            AddTextObject(PhotonNetwork.PlayerList[from].NickName + "に「" + cardInfo[myCard[cardIndex]].Item1 + "」を取られた。");
            view.RPC("RoumerAns", RpcTarget.Others, from, myCard[cardIndex],myNumber);
            myCard.RemoveAt(cardIndex);
        }
    }
    [PunRPC]private void RoumerAns(int from,int cardId,int target)
    {
        if(myNumber == from)//引いた側
        {
            print(PhotonNetwork.PlayerList[target] + "から" + cardInfo[cardId].Item1 + "を奪った");
            AddTextObject(PhotonNetwork.PlayerList[target].NickName + "から「" + cardInfo[cardId].Item1 + "」を奪った。");
            myCard.Add(cardId);
            view.RPC("ProcessDone", RpcTarget.All);
        }
    }
    #endregion

    #region 一般人
    private async UniTask Card8()
    {
        print("一般人カードを出した");
        view.RPC("AddTextObject", RpcTarget.All, $"特に何も起きなかった。");
        await UniTask.Delay(0);
        //効果なし
    }
    #endregion

    #region 目撃者
    private async UniTask Card9()
    {
        AddTextObject("手札を見る相手を指定してください。");
        int target = await SelectPlayer();
        view.RPC("OpenAllCardReq", RpcTarget.Others, target, myNumber);
    }
    [PunRPC]
    private void OpenAllCardReq(int target, int from)
    {
        if (target == myNumber)
        {
            print($"{PhotonNetwork.PlayerList[from].NickName}に手札がバレた");
            AddTextObject($"{PhotonNetwork.PlayerList[from].NickName}に手札がバレた");
            view.RPC("OpenAllCardReq", RpcTarget.Others, from, target, myCard.ToArray());
        }
    }
    [PunRPC]
    private void OpenAllCardReq(int from, int target, Array arr)
    {
        if (from == myNumber)
        {
            List<int> deck = new List<int>();
            foreach (var v in arr)
            {
                deck.Add((int)v);
            }
            print(PhotonNetwork.PlayerList[target] + "の手札は" + String.Join(" ", deck));
            string cardlist = PhotonNetwork.PlayerList[target].NickName + "の手札は";
            foreach (var card in deck) cardlist += $"「{cardInfo[card].Item1}」";
            AddTextObject(cardlist);
        }
    }

    #endregion

    #region 情報操作
    private async UniTask Card10()
    {
        view.RPC("InformationManipulation", RpcTarget.AllViaServer);
        int waitCount = allPlayerCardCount.Where(count => 0 < count).Count();//1枚以上カードがある人の数
        await UniTask.WaitUntil(() => procesDone == waitCount);
        view.RPC("CardDisplayUpdate", RpcTarget.Others);
    }
    [PunRPC]
    private async void InformationManipulation()
    {
        if (myCard.Count < 0) return;

        int prevPlayerNum = myNumber + 1;//1人後
        if (prevPlayerNum == playerCount) prevPlayerNum = 0;

        //交換するカードのインデックス
        AddTextObject($"{PhotonNetwork.PlayerList[prevPlayerNum].NickName}に渡すカードを選択してください。");
        int changeCardIndex = await FrontCardSelect(myCard);

        view.RPC("PassCard", RpcTarget.Others, prevPlayerNum, myCard[changeCardIndex]);
        AddTextObject($"{cardInfo[myCard[changeCardIndex]]}を渡した。");
        myCard.Remove(myCard[changeCardIndex]);
    }
    [PunRPC]
    private void PassCard(int targetId, int cardId)
    {
        if (myNumber == targetId)
        {
            print(cardInfo[cardId].Item1 + "を渡された");
            myCard.Add(cardId);
            AddTextObject($"{cardInfo[cardId].Item1}を渡された。");
            view.RPC("ProcessDone", RpcTarget.AllViaServer);
        }
    }
    #endregion

    #region とりひき
    private async UniTask Card11()
    {
        if (myCard.Count <= 0)
        {
            print("カードがないため交換できません");
            view.RPC("AddTextObject", RpcTarget.All, $"カードがないため交換できませんでした。");
            return;
        }

        AddTextObject("取り引きする相手を選択してください。");
        int target = await SelectPlayer();//取引相手を選択
        view.RPC("TransactionReq", RpcTarget.Others, target, myNumber);//取引相手に通信

        AddTextObject("取り引きするカードを選択してください。");
        int cardIndex = await FrontCardSelect(myCard);
        print(cardInfo[myCard[cardIndex]].Item1 + "を渡した");
        AddTextObject($"{cardInfo[myCard[cardIndex]].Item1}を渡した。");
        view.RPC("TransactionAns", RpcTarget.Others, target, myCard[cardIndex]);

        await UniTask.WaitUntil(() => procesDone == 2);//カード追加の処理が終わるまで待機
        myCard.RemoveAt(cardIndex);
        print("今の手持ちのカード");
        foreach (int card in myCard)
            print(cardInfo[card].Item1);
    }
    [PunRPC]
    private async void TransactionReq(int target, int from)
    {
        if (myNumber == target)
        {
            print("取引相手に選ばれた");
            AddTextObject("取り引き相手に選ばれました。");
            int cardIndex = await FrontCardSelect(myCard);
            print(cardInfo[myCard[cardIndex]].Item1 + "を渡した");
            AddTextObject($"{cardInfo[myCard[cardIndex]].Item1}を渡した。");
            view.RPC("TransactionAns", RpcTarget.Others, from, myCard[cardIndex]);

            await UniTask.WaitUntil(() => procesDone == 2);//カード追加の処理が終わるまで待機

            myCard.RemoveAt(cardIndex);
            print("今の手持ちのカード");
            foreach (int card in myCard)
                print(cardInfo[card].Item1);
            CardDisplayUpdate();
        }
    }
    [PunRPC]
    private void TransactionAns(int target,int cardId)
    {
        if (myNumber == target)
        {
            print(cardInfo[cardId].Item1 + "を貰った");
            AddTextObject($"{cardInfo[cardId].Item1}を貰った。");
            myCard.Add(cardId);
            view.RPC("ProcessDone", RpcTarget.AllViaServer);
        }
    }
    #endregion

    private async UniTask<int> SelectPlayer()
    {
        var selectedPlaIndex = new List<int>();
        for (int i = 0; i < playerCount; i++)
        {
            if (i == myNumber) continue;
            if (allPlayerCardCount[i] != 0)
            {
                selectedPlaIndex.Add(i);
            }
        }
        var playerList = new List<Player>();
        foreach (var index in selectedPlaIndex)
        {
            playerList.Add(PhotonNetwork.PlayerList[index]);
        }

        List<UniTask> tasks = new List<UniTask>();

        for (int i = 0; i < playerSelectButton.Count; i++)
        {
            playerSelectButton[i].transform.parent.gameObject.SetActive(false);
            tasks.Add(playerSelectButton[i].OnClickAsync());
            if (selectedPlaIndex.Contains(i))
            {
                playerSelectButton[PhotonNetwork.PlayerList.ToList().IndexOf(playerList[selectedPlaIndex.IndexOf(i)])].transform.parent.gameObject.SetActive(true);
            }
        }

        playerSelecctParent.SetActive(true);
        int target = await UniTask.WhenAny(tasks);//入力待機
        playerSelecctParent.SetActive(false);
        return target;
    }

    private async UniTask<int> FrontCardSelect(List<int> cardList)
    {
        List<UniTask> tasks = new List<UniTask>();

        //カードのリストリセット
        foreach (Transform obj in cardSelectParent.transform)
        {
            Destroy(obj.gameObject);
        }

        //選択肢を追加
        for (int i = 0; i < cardList.Count; i++)
        {
            var content = Instantiate(obj_CardContent, cardSelectParent.transform).transform.Find("Card_Child");

            SetCardContent(content, cardList[i]);

            content.parent.Find("Card_Cover").gameObject.SetActive(false);

            tasks.Add(content.GetComponent<Button>().OnClickAsync());
        }

        cardSelectParent.SetActive(true);//表示
        int target = await UniTask.WhenAny(tasks);//プレイヤーの入力待ち
        cardSelectParent.SetActive(false);//非表示

        return target;
    }

    private async UniTask<int> BackCardSelect(int cardCount)
    {
        List<UniTask> tasks = new List<UniTask>();

        //カードのリストリセット
        foreach (Transform obj in cardSelectParent.transform)
        {
            Destroy(obj.gameObject);
        }
        //選択肢を追加
        for (int i = 0; i < cardCount; i++)
        {
            Button button = Instantiate(cardSelectPrefab, cardSelectParent.transform).transform.Find("Card_Child").GetComponent<Button>();
            tasks.Add(button.OnClickAsync());
        }

        cardSelectParent.SetActive(true);//表示
        int target = await UniTask.WhenAny(tasks);//プレイヤーの入力待ち
        cardSelectParent.SetActive(false);//非表示
        return target;
    }

    private void SetCardContent(Transform content, int cardId)
    {
        content.Find("Images").GetChild(cardId).gameObject.SetActive(true);

        content.Find("Text_CardTitle").GetComponent<Text>().text = cardInfo[cardId].Item1;
        content.Find("Text").GetComponent<Text>().text = cardInfo[cardId].Item2;

        content.GetComponent<Image>().color = color_Cards[cardId];
        content.Find("Text_CardTitle").GetComponent<Outline>().effectColor = color_Cards[cardId];
    }

    [PunRPC]
    private void CardDisplayUpdate()
    {
        cardCover = new List<GameObject>();
        foreach (Transform obj in obj_CardParent.transform)
        {
            Destroy(obj.gameObject);
        }
        for (int i = 0; i < myCard.Count; i++)
        {
            var content = Instantiate(obj_CardContent, obj_CardParent.transform).transform.Find("Card_Child");
            int cardId = myCard[i];

            SetCardContent(content, cardId);

            content.GetComponent<Animator>().enabled = false;

            int ii = i;
            content.GetComponent<Button>().onClick.AddListener(() => UseCard(ii));

            cardCover.Add(content.parent.Find("Card_Cover").gameObject);
        }
    }

    [PunRPC]
    private void ProcessDone()
    {
        procesDone++;
        print("procesDone" + procesDone);
    }

    [PunRPC]
    private void ProcessReset()
    {
        procesDone = 0;
    }

    [PunRPC]
    private void TurnEnd()
    {
        Turn--;
        if (Turn == 0)
        {
            isTurn = true;
            view.RPC("TurnStart", RpcTarget.AllViaServer);
        }
        else
        {
            isTurn = false;
        }
    }

    [PunRPC]
    private void TurnStart()
    {
        if (isTurn)//自分のターンだったら
        {
            //カードを選択可能にする
            foreach (var obj in cardCover)
            {
                obj.SetActive(false);
            }

            //自分がスターターだったら
            if (isStarter)
            {
                view.RPC("TurnPlus", RpcTarget.All);
                view.RPC("WeekPlus", RpcTarget.All);
                view.RPC("AddTextObject", RpcTarget.All, $"────{turnCount}ターン目────");
            }
            view.RPC("AddTextObject", RpcTarget.All, $"────────────");
            view.RPC("AddTextObject", RpcTarget.All, $"{PhotonNetwork.PlayerList[myNumber].NickName}のターンです。");


            //手札が使用可能かどうか的な処理
            //1週目だったら
            if (weekCount == 1)
            {
                if (isStarter)
                {
                    //すべてのカードを出せないようにする
                    foreach (var obj in cardCover)
                    {
                        obj.SetActive(true);
                    }
                    //第一発見者のみだせる
                    cardCover[myCard.IndexOf(0)].SetActive(false);
                    return;
                }
                else
                {
                    //探偵は出せない
                    if (myCard.Contains(2))
                    {
                        //探偵を出せないようにする
                        for (int i = 0; i < myCard.Count; i++)
                        {
                            if (myCard[i] == 2)
                            {
                                cardCover[i].SetActive(true);
                            }
                        }
                    }
                }
            }

            //最後のターンでなければ
            if (weekCount != 4)
            {
                if (myCard.Contains(1))
                {
                    //犯人を出せないようにする処理
                    for (int i = 0; i < myCard.Count; i++)
                    {
                        if (myCard[i] == 1)
                        {
                            cardCover[i].SetActive(true);
                        }
                    }
                }
            }
        }
        else
        {
            foreach (var obj in cardCover)
                obj.SetActive(true);
        }
    }

    [PunRPC]
    private void TurnPlus()
    {
        turnCount++;
    }
    [PunRPC]
    private void WeekPlus()
    {
        weekCount++;
    }

    [PunRPC]
    private void AddCriminalSide(string name)
    {
        CriminalSide.Add(name);
    }

    [PunRPC]
    private void GameEnd(bool isEvilWin)
    {
        foreach (Transform obj in wonPlayer_Parent.transform) Destroy(obj.gameObject);

        if (isEvilWin)
        {
            //犯人側の勝ち
            print("犯人側の勝ち");
            foreach(var name in CriminalSide)
            {
                var content = Instantiate(playerSelectPrefab, wonPlayer_Parent.transform);

                content.transform.Find("Card_Child").Find("Text_PlayerName").GetComponent<Text>().text = name;
                content.transform.Find("Card_Child").Find("Text").gameObject.SetActive(false);
            }
        }
        else
        {
            //探偵側の勝ち
            print("探偵側の勝ち");
            for(var i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if (!CriminalSide.Contains(PhotonNetwork.PlayerList[i].NickName))
                {
                    var content = Instantiate(playerSelectPrefab, wonPlayer_Parent.transform);

                    content.transform.Find("Card_Child").Find("Text_PlayerName").GetComponent<Text>().text = PhotonNetwork.PlayerList[i].NickName;
                    content.transform.Find("Card_Child").Find("Text").gameObject.SetActive(false);
                }
            }
        }
        foreach(var obj in cardCover)
        {
            obj.SetActive(true);
        }

        game.SetActive(false);
        result.SetActive(true);
        ResultButton();
    }

    //場に出した時に呼ぶ
    //全クライアントで呼ばれる

    [PunRPC]
    private void PutPlay(int value, int from)
    {
        print(PhotonNetwork.PlayerList[from].NickName + "が" + cardInfo[value].Item1 + "を使った");
        AddTextObject("効果\n"+cardInfo[value].Item2);
        var text = PhotonNetwork.PlayerList[from].NickName + "が" + cardInfo[value].Item1 + "を使った。";
        AddTextObject(text);
        AddTextObject("────────────");
        allPlayerCardCount[from]--;
    }

    [PunRPC]
    private async void AddTextObject(string text)
    {
        var content = Instantiate(textObj_Content, textObj_Parent.transform);

        content.GetComponent<Text>().text = text;
        content.transform.SetAsFirstSibling();

        textObj_Parent.GetComponent<VerticalLayoutGroup>().enabled = false;
        await UniTask.Delay(100);
        textObj_Parent.GetComponent<VerticalLayoutGroup>().enabled = true;
    }


    //ゲーム終了後の処理
    private int reStartCount;
    private int selectCount;
    private bool isReStart;
    private bool isGameEnd;

    private async void ResultButton()
    {
        List<UniTask> tasks = new List<UniTask>();
        tasks.Add(gameEndButton.OnClickAsync());
        tasks.Add(reStartButton.OnClickAsync());
        int button = await UniTask.WhenAny(tasks);//どちらか押されたら
        if (button == 1)
        {
            tasks = new List<UniTask>();
            tasks.Add(UniTask.WaitUntil(() => reStartCount == playerCount));//全員揃うまで待つ
            tasks.Add(UniTask.WaitUntil(() => isGameEnd == true));//誰かがゲーム終了を押すまで待つ
            int select = await UniTask.WhenAny(tasks);
            if (select == 0)//全員揃った
            {
               await SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
            }
        }
        /*
         * リスタートorゲーム終了を押す
         * 
         * リスタート
         * 全員揃うのを待つor誰かがゲーム終了を押すのを待つ
         * 
         * 全員揃う→リスタート
         * 誰かがゲーム終了を押す→リスタートボタンを消すandゲーム終了待ち
         * 
         * ゲーム終了
         * 他プレイヤーにゲーム終了を伝えてタイトルに戻る。
         */
    }
    public void GameEndClick()
    {
        reStartButton.gameObject.SetActive(false);//ボタンを消す
        gameEndButton.gameObject.SetActive(false);//ボタンを消す
        if (PhotonNetwork.InRoom)
        {
            view.RPC("GameEndSignal", RpcTarget.Others);
            PhotonNetwork.LeaveRoom();
        }
        SceneManager.LoadSceneAsync("TitleScene");
    }

    [PunRPC]
    private void GameEndSignal()
    {
        reStartButton.gameObject.SetActive(false);//ボタンを消す

        print("ゲーム終了が押された");
        if (isReStart)
        {
            print("リスタートを選択していたが誰かがゲーム終了を押した");
            gameEndButton.gameObject.SetActive(true);
        }
        isGameEnd = true;
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    public void ReStartClick()
    {
        reStartButton.gameObject.SetActive(false);//ボタンを消す
        gameEndButton.gameObject.SetActive(false);//ボタンを消す
        isReStart = true;
        view.RPC("ReStartSignal", RpcTarget.All);
    }

    [PunRPC]
    private void ReStartSignal()
    {
        reStartCount++;
    }
}