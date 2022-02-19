using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    string mode;          // ゲームの現在の「モード」
    string previous_mode; //一つ前のモード
    int frame;   // ゲームの現在フレーム（1/60秒ごとに1追加される）
    public int combinationCount = 0; // 何連鎖かどうか
    bool loopFrag = false;    //ループを開始させるフラグ
    //連鎖数を表示させるためのテキスト
    public GameObject rensaObj;
    Text rensaText;

    //public voidの関数を呼ぶために必要
    Player playerScript;
    Next nextScript;
    PuyoImage puyoImageScript;
    Stage stageScript;
    Score scoreScript;

    void initialize()
    {
        // 画像を準備する
        PuyoImage.initialize();
        // ステージを準備する
        Stage.initialize();
        // ユーザー操作の準備をする
        Player.initialize();
        // シーンを初期状態にセットする
        scoreScript.initialize();
        // スコア表示の準備をする
        mode = "start";
        previous_mode = "";
        // フレームを初期化する
        frame = 0;
    }

    void loop()
    {
        //フレームレートを60FPSに固定
        Application.targetFrameRate = 60;
        //現在のモードと1フレーム前のモードが違ったら出力（モードが変化したとき）
        if(previous_mode != mode)
        {
            Debug.Log("mode=" + mode);
        }
        //一つ前のモードに、現在のモードを割り当てる
        previous_mode = mode;

        switch (mode)
        {
            case "start":
                //ネクスト、ネクネクを表示
                nextScript.OpeningDecidePuyoColor();
                nextScript.ShowNextPuyo();
                // 最初は、もしかしたら空中にあるかもしれないぷよを自由落下させるところからスタート
                mode = "checkFall";
                break;
            case "checkFall":
                // 落ちるかどうか判定する
                if (Stage.checkFall())
                {
                    //ステージに落ちるぷよがある
                    mode = "fall";
                }
                else
                {
                    // 落ちないならば、ぷよを消せるかどうか判定する
                    mode = "checkErase";
                }
                break;
            case "fall":
                if (!Stage.fall())
                {
                    // すべて落ちきったら、ぷよを消せるかどうか判定する
                    mode = "checkErase";
                }
                break;
            case "checkErase":
                // 消せるかどうか判定する
                var eraseInfo = Stage.checkErase(frame);
                //eraseInfoには、消せるぷよの個数と色の情報が入る
                if (eraseInfo != null)
                {
                    //消せるぷよがある
                    mode = "erasing";
                    //連鎖数を1増やす
                    combinationCount++;
                    // 得点を計算する
                    scoreScript.calculateScore(combinationCount, eraseInfo.piece, eraseInfo.color);
                    //表示させる連鎖数の変更
                    rensaText = rensaObj.GetComponent<Text>();
                    rensaText.text = combinationCount.ToString();

                    stageScript.hideZenkeshi();
                }
                else
                {
                    //消せるぷよがない
                    if (Stage.puyoCount == 0 && combinationCount > 0)
                    {
                        // 全消しの処理をする
                        stageScript.showZenkeshi();
                        scoreScript.addScore(3600);
                    }
                    combinationCount = 0;
                    // 消せなかったら、新しいぷよを登場させる
                    mode = "newPuyo";
                }
                break;
            case "erasing":
                if (!stageScript.erasing(frame))
                {
                    // 消し終わったら、再度落ちるかどうか判定する
                    mode = "checkFall";
                }
                break;
            case "newPuyo":
                if (!playerScript.createNewPuyo())
                {
                    // 新しい操作用ぷよを作成出来なかったら、ゲームオーバー
                    mode = "gameOver";
                }
                else
                {
                    // プレイヤーが操作可能
                    mode = "playing";
                }
                break;
            case "playing":
                // プレイヤーが操作する
                string action = playerScript.PlayMoveRotate(frame);
                mode = action; // 'playing' 'fix' のどれかが帰ってくる
                break;
            /*
            case "moving":
                if (!Player.moving(frame))
                {
                    // 移動が終わったので操作可能にする
                    mode = "playing";
                }
                break;
            case "rotating":
                if (!Player.rotating(frame))
                {
                    // 回転が終わったので操作可能にする
                    mode = "playing";
                }
                break;
            */
            case "fix":
                // 現在の位置でぷよを固定する
                // fixの状態でしばらく待つ
                if(Player.fix()){
                    // 固定したら、まず自由落下を確認する
                    mode = "checkFall";
                }
                break;
            case "gameOver":
                // ばたんきゅーの準備をする
                puyoImageScript.prepareBatankyu(frame);
                mode = "batankyu";
                break;
            case "batankyu":
                puyoImageScript.batankyu(frame);
                Player.batankyu();
                break;
        }
        frame++;
    }

    // Start is called before the first frame update
    //起動されたときに呼ばれる関数を登録する
    void Start()
    {
        // まずステージを整える
        playerScript = gameObject.GetComponent<Player>();
        nextScript = gameObject.GetComponent<Next>();
        puyoImageScript = gameObject.GetComponent<PuyoImage>();
        stageScript = gameObject.GetComponent<Stage>();
        scoreScript = gameObject.GetComponent<Score>();
        initialize();
        // ゲームを開始する
        this.loopFrag = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (loopFrag == false) return; //loopFragがfalseのときは何もしない
        this.loop();
    }
}
