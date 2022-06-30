using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/* 変更した部分 */
//2021/05/07
//boardを6*12から6*13に変更
//2021/05/09
//アルルのキャラボイスを追加

/// <summary>
/// 落下中のぷよの状態を表すクラス
/// </summary>
public class fallingPuyoClass
{
    public GameObject puyoObject;
    public int position;
    public int destination;
    public bool falling;
}

public class CheckErase
{
    public int piece;
    public int color;
}

public class PuyoInfo
{
    public GameObject puyoObject;
    public int x;
    public int y;
    public int cell;
}

public class Stage : MonoBehaviour
{
    public int[][] board{get; set;}
    public int puyoCount{get; set;}
    public List<fallingPuyoClass> fallingPuyoList{get; set;}  //動的配列
    private int eraseStartFrame;
    public List<PuyoInfo> erasingPuyoInfoList{get; set;}

    private GameObject zenkeshiObject;
    private GameObject zenkeshiPrefab;
    private SpriteRenderer spRenderer;

    //キャラボイス用
    private AudioSource audioSource;
    private AudioClip[] voice;

    private Game game;    

    public void initialize()
    {
        // メモリを準備する
        board = new int[][]
        {
            new int[]{ 0, 0, 0, 0, 0, 0 },
            new int[]{ 0, 0, 0, 0, 0, 0 },
            new int[]{ 0, 0, 0, 0, 0, 0 },
            new int[]{ 0, 0, 0, 0, 0, 0 },
            new int[]{ 0, 0, 0, 0, 0, 0 },
            new int[]{ 0, 0, 0, 0, 0, 0 },
            new int[]{ 0, 0, 0, 0, 0, 0 },
            new int[]{ 0, 0, 0, 0, 0, 0 },
            new int[]{ 0, 0, 0, 0, 0, 0 },
            new int[]{ 0, 0, 0, 0, 0, 0 },
            new int[]{ 0, 0, 0, 0, 0, 0 },
            new int[]{ 0, 0, 0, 0, 0, 0 },
            new int[]{ 0, 0, 0, 0, 0, 0 }
        };
        
        int puyoCount = 0;
        for(int y = 0; y<Config.stageRows; y++) {
            int[] line;
            //board[y]のすべての要素が0以外ならばboardy=true、一つでも0があればfalse
            bool board_y = true;
            for (int i = 0; i < Config.stageCols; i++)
            {
                if (board[y][i] == 0)
                {
                    board_y = false;
                    break;
                }
            }

            if (board_y)
            {
                line = board[y];
            }
            else
            {
                line = (board[y] = new int[6]);
            }

            for(int x = 0; x<Config.stageCols; x++) {
                int puyo = line[x];
                if(puyo >= 1 && puyo <= 5) {
                    this.setPuyo(x, y, puyo);
                    puyoCount++;
                } else {
                    line[x] = 0;
                }
            }
        }
        this.puyoCount = puyoCount;
    }

    // （画面と）メモリ（両方）に puyo をセットする
    public void setPuyo(int x, int y, int puyo)
    {
        board[y][x] = puyo;
    }

    // 自由落下をチェックする
    public bool checkFall()
    {
        //fallingPuyoList.Length = 0;
        fallingPuyoList = new List<fallingPuyoClass>();
        bool isFalling = false;
        // 下の行から上の行を見ていく
        for (int y = Config.stageRows - 2; y >= 0; y--)
        {
            int[] line = board[y];
            for (int x = 0; x < line.Length; x++)
            {
                if (board[y][x]==0)
                {
                    // このマスにぷよがなければ次
                    continue;
                }

                if (board[y + 1][x]==0)
                {
                    // このぷよは落ちるので、取り除く
                    int cell = board[y][x];
                    board[y][x] = 0;
                    int dst = y;
                    while (dst + 1 < Config.stageRows && board[dst + 1][x] == 0)
                    {
                        dst++;
                    }
                    // 最終目的地に置く
                    board[dst][x] = cell;
                    // 落ちるリストに入れる
                    fallingPuyoClass fallingPuyo = new fallingPuyoClass();
                    GameObject[] puyos = GameObject.FindGameObjectsWithTag("puyo");
                    foreach(GameObject puyo in puyos)
                    {
                        if(Mathf.RoundToInt(puyo.transform.position.x) == x * Config.puyoImgWidth
                            && Mathf.RoundToInt(puyo.transform.position.y) == -y * Config.puyoImgHeight)
                        {
                            fallingPuyo.puyoObject = puyo;
                        }
                    }

                    fallingPuyo.position = y * Config.puyoImgHeight;
                    fallingPuyo.destination = dst * Config.puyoImgHeight;
                    fallingPuyo.falling = true;
                    fallingPuyoList.Add(fallingPuyo);
                    // 落ちるものがあったことを記録しておく
                    isFalling = true;
                }
            }
        }
        return isFalling;
    }

    // 自由落下させる
    public bool fall()
    {
        bool isFalling = false;
        foreach (fallingPuyoClass fallingPuyo in fallingPuyoList) {
            if (!fallingPuyo.falling)
            {
                // すでに自由落下が終わっている
                continue;
            }
            int position = fallingPuyo.position;
            position += Config.freeFallingSpeed;
            if (position >= fallingPuyo.destination)
            {
                // 自由落下終了
                position = fallingPuyo.destination;
                fallingPuyo.falling = false;
            }
            else
            {
                // まだ落下しているぷよがあることを記録する
                isFalling = true;
            }
            // 新しい位置を保存する
            fallingPuyo.position = position;
            // ぷよを動かす
            fallingPuyo.puyoObject.transform.localPosition = new Vector3(fallingPuyo.puyoObject.transform.position.x, -position, 0);

        }
        return isFalling;
    }
    
    // 消せるかどうか判定する
    public CheckErase checkErase(int startFrame)
    {
        eraseStartFrame = startFrame;
        erasingPuyoInfoList.Clear();
        // 何色のぷよを消したかを記録する
        bool[] erasedPuyoColor = new bool[Config.puyoColors+1];
        // 隣接ぷよを確認する関数内関数を作成
        List<PuyoInfo> sequencePuyoInfoList = new List<PuyoInfo>();
        List<PuyoInfo> existingPuyoInfoList = new List<PuyoInfo>();
        void checkSequentialPuyo(int x, int y) {
            // ぷよがあるか確認する
            int orig = board[y][x];
            if (orig == 0)
            {
                // ないなら何もしない
                return;
            }
            // あるなら一旦退避して、メモリ上から消す
            int puyo = board[y][x];
            PuyoInfo puyoInfo = new PuyoInfo();
            GameObject[] puyos = GameObject.FindGameObjectsWithTag("puyo");
            foreach (GameObject puyopuyo in puyos)
            {
                if (Mathf.RoundToInt(puyopuyo.transform.position.x) == x * Config.puyoImgWidth
                    && Mathf.RoundToInt(puyopuyo.transform.position.y) == -y * Config.puyoImgHeight)
                {
                    puyoInfo.puyoObject = puyopuyo;
                }
            }
            puyoInfo.x = x;
            puyoInfo.y = y;
            puyoInfo.cell = board[y][x];
            sequencePuyoInfoList.Add(puyoInfo);
            board[y][x] = 0;

            // 四方向の周囲ぷよを確認する
            int[][] direction = new int[][]{ new int[]{ 0, 1 }, new int[]{ 1, 0 }, new int[]{ 0, -1 }, new int[]{ -1, 0 } };
            for (int i = 0; i < direction.Length; i++)
            {
                int dx = x + direction[i][0];
                int dy = y + direction[i][1];
                if (dx < 0 || dy < 0 || dx >= Config.stageCols || dy >= Config.stageRows)
                {
                    // ステージの外にはみ出た
                    continue;
                }
                int cell = board[dy][dx];
                if (cell == 0 || cell != puyo)
                {
                    // ぷよの色が違う
                    continue;
                }
                // そのぷよのまわりのぷよも消せるか確認する
                checkSequentialPuyo(dx, dy);
            };
        };

        // 実際に削除できるかの確認を行う
        for(int y = 0; y<Config.stageRows; y++) {
            for(int x = 0; x<Config.stageCols; x++) {
                sequencePuyoInfoList.Clear();
                int puyoColor = board[y][x]; //とりあえずこうしておくが怪しかったら変更

                checkSequentialPuyo(x, y);
                if(sequencePuyoInfoList.Count == 0 || sequencePuyoInfoList.Count < Config.erasePuyoCount) {
                    // 連続して並んでいる数が足りなかったので消さない
                    if(sequencePuyoInfoList.Count != 0) {
                        // 退避していたぷよを消さないリストに追加する
                        //配列をマージ（末尾にくっつける）
                        existingPuyoInfoList.AddRange(sequencePuyoInfoList);
                    }
                } else {
                    // これらは消して良いので消すリストに追加する
                    erasingPuyoInfoList.AddRange(sequencePuyoInfoList);
                    //erasedPuyoColor [puyoColor] = true;
                    erasedPuyoColor[puyoColor] = true;
                }
            }
        }
        puyoCount -= erasingPuyoInfoList.Count;
        // 消さないリストに入っていたぷよをメモリに復帰させる
        foreach(PuyoInfo info in existingPuyoInfoList) {
            board[info.y][info.x] = info.cell;
        }

        if(erasingPuyoInfoList.Count != 0) {
            // もし消せるならば、消えるぷよの個数と色の情報をまとめて返す
            CheckErase puyo = new CheckErase();
            puyo.piece = erasingPuyoInfoList.Count;
            if(erasedPuyoColor[0] == true)
            {
                puyo.color = 1;
            }
            else if(erasedPuyoColor[1] == true)
            {
                puyo.color = 2;
            }
            else if(erasedPuyoColor[2] == true)
            {
                puyo.color = 3;
            }
            else if (erasedPuyoColor[3] == true)
            {
                puyo.color = 4;
            }
            else if (erasedPuyoColor[4] == true)
            {
                puyo.color = 5;
            }
            return puyo;
        }
        return null;
    }
    // 消すアニメーションをする
    public bool erasing(int frame)
    {
        int elapsedFrame = frame - eraseStartFrame;
        float ratio = elapsedFrame / Config.eraseAnimationDuration;
        if (ratio > 1)
        {
            // アニメーションを終了する
            foreach (PuyoInfo info in erasingPuyoInfoList)
            {
                Destroy(info.puyoObject);
            }

            //キャラボイスを鳴らす
            //連鎖数は、Gameスクリプトで計算した値を用いる
            switch (game.combinationCount)
            {
                case 1: audioSource.PlayOneShot(voice[0]); break; //1連鎖
                case 2: audioSource.PlayOneShot(voice[1]); break; //2連鎖
                case 3: audioSource.PlayOneShot(voice[2]); break;
                case 4: audioSource.PlayOneShot(voice[3]); break;
                case 5: audioSource.PlayOneShot(voice[4]); break;
                case 6: audioSource.PlayOneShot(voice[5]); break;
                default: audioSource.PlayOneShot(voice[6]); break; //7連鎖以上
            }
            

            return false;
        }
        else if (ratio > 0.75f)
        {
            foreach(PuyoInfo info in erasingPuyoInfoList)
            {
                var renderComponent = info.puyoObject.GetComponent<Renderer>();
                renderComponent.enabled = true;
            }
            
            return true;
        }
        else if (ratio > 0.50f)
        {
            foreach (PuyoInfo info in erasingPuyoInfoList)
            {
                var renderComponent = info.puyoObject.GetComponent<Renderer>();
                renderComponent.enabled = false;
            }

            return true;
        }
        else if (ratio > 0.25f)
        {
            foreach (PuyoInfo info in erasingPuyoInfoList)
            {
                var renderComponent = info.puyoObject.GetComponent<Renderer>();
                renderComponent.enabled = true;
            }

            return true;
        }
        else
        {
            foreach (PuyoInfo info in erasingPuyoInfoList)
            {
                var renderComponent = info.puyoObject.GetComponent<Renderer>();
                renderComponent.enabled = false;
            }

            return true;
        }
    }

    public void showZenkeshi()
    {
        zenkeshiObject = Instantiate(zenkeshiPrefab) as GameObject;
        float startTime = DateTime.Now.Hour * 60 * 60 * 1000 + DateTime.Now.Minute * 60 * 1000 
            +DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;
        float startTop = Config.puyoImgHeight * Config.stageRows;
        float endTop = Config.puyoImgHeight * Config.stageRows / 3;
        float ratio;
        while (true)
        {
            ratio = Mathf.Min((DateTime.Now.Hour * 60 * 60 * 1000 + DateTime.Now.Minute * 60 * 1000
                + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond - startTime) / Config.zenkeshiDuration, 1);
            zenkeshiObject.gameObject.transform.position = new Vector3(100.0f, (startTop - endTop) * ratio - startTop, 1.0f);
            if (ratio >= 1) break;
        }
    }

    public void hideZenkeshi()
    {
        //全消しの文字がないときは何もしない
        if (!zenkeshiObject) return;

        // 全消しを消去する
        float startTime = DateTime.Now.Hour * 60 * 60 * 1000 + DateTime.Now.Minute * 60 * 1000
            + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond;
        spRenderer = zenkeshiObject.GetComponent<SpriteRenderer>();

        while (true) { 
            float ratio = Mathf.Min((DateTime.Now.Hour * 60 * 60 * 1000 + DateTime.Now.Minute * 60 * 1000
            + DateTime.Now.Second * 1000 + DateTime.Now.Millisecond - startTime) / Config.zenkeshiDuration, 1);
            var color = spRenderer.color;
            color.a = 1 - ratio;
            spRenderer.color = color;

            if(ratio >= 1)
            {
                Destroy(zenkeshiObject);
                break;
            }
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        this.game = gameObject.GetComponent<Game>();

        this.zenkeshiPrefab = Resources.Load("zenkeshiPrefab") as GameObject;

        this.erasingPuyoInfoList = new List<PuyoInfo>();

        this.voice = Resources.LoadAll<AudioClip>("voice/");
    }
}

