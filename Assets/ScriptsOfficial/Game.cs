using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//2022/06/27
//Config以外のstaticを基本的に排除する。

/// <summary>
/// ゲームの進行を制御するクラス
/// </summary>
public class Game : MonoBehaviour
{
    private string mode;          // ゲームの現在の「モード」
    private string previous_mode; //一つ前のモード
    private int frame;   // ゲームの現在フレーム（1/60秒ごとに1追加される）
    private bool loopFrag = false;    //ループを開始させるフラグ
    private int _combinationCount = 0; // 何連鎖かどうか
    //combinationCountのsetter, getter
    public int combinationCount 
    { 
        get { return _combinationCount; } 
        set { if(value >= 0) _combinationCount = value; }
    } 
    //連鎖数を表示させるためのテキスト
    private GameObject rensaObj;
    private Text rensaText;

    private Next next;
    private Player player;
    private PuyoImage puyoImage;
    private Score score;
    private Stage stage;

    void initialize()
    {
        // 画像を準備する
        puyoImage.initialize();
        // ステージを準備する
        stage.initialize();
        // ユーザー操作の準備をする
        player.initialize();
        // シーンを初期状態にセットする
        score.initialize();
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
                next.OpeningDecidePuyoColor();
                next.ShowNextPuyo();
                // 最初は、もしかしたら空中にあるかもしれないぷよを自由落下させるところからスタート
                mode = "checkFall";
                break;
            case "checkFall":
                // 落ちるかどうか判定する
                if (stage.checkFall())
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
                //ぷよを落とす
                if (!stage.fall())
                {
                    // すべて落ちきったら、ぷよを消せるかどうか判定する
                    mode = "checkErase";
                }
                break;
            case "checkErase":
                // 消せるかどうか判定する
                var eraseInfo = stage.checkErase(frame);
                //eraseInfoには、消せるぷよの個数と色の情報が入る
                if (eraseInfo != null)
                {
                    //消せるぷよがある
                    mode = "erasing";
                    //連鎖数を1増やす
                    combinationCount += 1;
                    // 得点を計算する
                    score.calculateScore(combinationCount, eraseInfo.piece, eraseInfo.color);
                    //表示させる連鎖数の変更
                    rensaText = rensaObj.GetComponent<Text>();
                    rensaText.text = combinationCount.ToString();

                    stage.hideZenkeshi();
                }
                else
                {
                    //消せるぷよがない
                    if (stage.puyoCount == 0 && combinationCount > 0)
                    {
                        // 全消しの処理をする
                        stage.showZenkeshi();
                        score.addScore(3600);
                    }
                    combinationCount = 0;
                    // 消せなかったら、新しいぷよを登場させる
                    mode = "newPuyo";
                }
                break;
            case "erasing":
                //ぷよを消す
                if (!stage.erasing(frame))
                {
                    // 消し終わったら、再度落ちるかどうか判定する
                    mode = "checkFall";
                }
                break;
            case "newPuyo":
                if (!player.createNewPuyo())
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
                string action = player.PlayMoveRotate(frame);
                mode = action; // 'playing' 'fix' のどれかが帰ってくる
                break;
            case "fix":
                // 現在の位置でぷよを固定する
                // fixの状態でしばらく待つ
                if(player.fix()){
                    // 固定したら、まず自由落下を確認する
                    mode = "checkFall";
                }
                break;
            case "gameOver":
                // ばたんきゅーの準備をする
                puyoImage.prepareBatankyu(frame);
                mode = "batankyu";
                break;
            case "batankyu":
                //ばたんきゅー状態
                puyoImage.batankyu(frame);
                player.batankyu();
                break;
        }
        frame++;
    }

    //起動されたときに呼ばれる関数を登録する
    void Start()
    {
        this.rensaObj = GameObject.Find("Rensa");
        // まずステージを整える
        this.player    = gameObject.GetComponent<Player>();
        this.next      = gameObject.GetComponent<Next>();
        this.puyoImage = gameObject.GetComponent<PuyoImage>();
        this.score     = gameObject.GetComponent<Score>();
        this.stage     = gameObject.GetComponent<Stage>();
        this.initialize();
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
