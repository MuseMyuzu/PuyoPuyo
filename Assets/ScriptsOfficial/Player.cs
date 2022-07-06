using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/* 変更した部分 */
//2021/03/xx
//moveFlag、rotateFlagの導入
//"moving","rotating"のモードを廃止し、PlayMoveRotate関数にまとめた
//2021/03/12
//playing()のfixを返す部分を取り出して、moveFlagやrotateFlagの前に持ってきた
//2021/03/13
//actionStartFrameをmoveStartFrameとrotateStartFrameに分けた
//かべに挟まれているときに、回転ボタンをダブルクリックすると180度回転する仕組みの作成開始
//2021/05/07
//ぷよを13段積めるようにしたので、新しいぷよの作成場所を、一段下にずらした
//2021/09/10
//ぷよを固定するときに中途半端な位置にあれば、ぷよをマスに沿った正しい位置にセットするように修正
//2021/09/11
//PuyoStatus.yは1段下げる必要はない？ということで手をつけない
//2021/09/12
//ぷよの初期配置としてpuyoStatus.yを-1から0にした。
//接地してない場合、落下しているときに軸ぷよの右下・左下にぷよがあるか確認するのを排除...(*)
//軸ぷよが回転によって14段目に行かないための処理を追加（横移動では行けてしまう）
//2021/09/13
//09/12の(*)はやめた
//下に0.5マス以上の空きがあるときに回転して上に上がるとき、ぷよが1.5マス上の部分まで上がるようにした。（改善の余地あり）
//2021/09/27
//クイックターン時に少し上に上昇するようにした。
//2021/11/30
//フレームレートを60fpsに固定
//2021/12/02
//Playing内の横移動判定と回転判定を逆にした
//PlayMoveRotate関数で落ちる、回転、横移動、操作から、落ちる、操作、回転、横移動という順にした
//（Playing内の横移動判定と回転判定を逆にしたから、これはしなくていいかも）
//接地状態から回転によってぷよが持ち上がっても、接地状態が継続するようにした
//まわしは接地時間が長いとうまくいくっぽい。
//2021/12/12
//下ボタンを押していたら、固定時間が短くなるようにした。

//中心ぷよ=軸ぷよ　　　動くぷよ=子ぷよ

class PuyoStatus
{
    public int x;        //stage.boardの列成分に対応
    public int y;        //stage.boardの行成分に対応
    public float sceneX; //ゲームシーン上でのぷよの基準からのx座標
    public float sceneY; //ゲームシーン上でのぷよの基準からのy座標（ただし20210911現在、絶対値（実際の座標とは符号が逆））
    public int dx;       //-1か1の値を取る。子ぷよの相対的なx座標
    public int dy;       //-1か1の値を取る。子ぷよの相対的なy座標
    public int rotation; //子ぷよの回転角度
}

class KeyStatus
{
    public bool right;
    public bool left;
    public bool up;
    public bool down;
}

public class Player : MonoBehaviour
{
    private GameObject centerPuyoObj; //軸ぷよオブジェクト
    private GameObject movablePuyoObj;//子ぷよオブジェクト
    private int centerPuyo;           //軸ぷよの色（1:赤、2:緑、3:青、4:黄）
    private int movablePuyo;          //子ぷよの色

    private GameObject redPrefab;
    private GameObject greenPrefab;
    private GameObject bluePrefab;
    private GameObject yellowPrefab;

    private PuyoStatus puyoStatus = new PuyoStatus();
    private int groundFrame;          //接地している時間
    private KeyStatus keyStatus = new KeyStatus();
    private int moveStartFrame;
    private int rotateStartFrame;
    private int moveSource;
    private int moveDestination;
    private int rotateBeforeLeft;
    private int rotateAfterLeft;
    private int rotateFromRotation; //回転前の角度
    private int rotateDirection; //右回転なら-1、左回転なら1
    private bool moveFlag = false; //ぷよが左右移動をしている最中ならtrue
    private bool rotateFlag = false; //ぷよが回転をしている最中ならtrue
    private bool quickTurnFlag = false; //ぷよが180度回転をしている最中ならtrue
    private int firstClickFrame = 0; //ダブルクリックの際、一回目にクリックしたフレーム
    private int fixingFrame = 0;
    private int fixStartFrame; 

    private Next next;
    private Score score;
    private Stage stage;

    public void initialize()
    {
        keyStatus.right = false;
        keyStatus.left = false;
        keyStatus.up = false;
        keyStatus.down = false;
    }

    //ぷよ設置確認
    public bool createNewPuyo()
    {
        // ぷよぷよが置けるかどうか、上から数えて二番目の段の左から3つ目を確認する
        if (stage.board[1][2] != 0)
        {
            // 空白でない場合は新しいぷよを置けない（ばたんきゅー）
            return false;
        }

        // 新しいぷよの色にネクストの色を代入
        centerPuyo = next.nextCenterPuyo;
        movablePuyo = next.nextMovablePuyo;
        //次のネクスト、ネクネクの色を決めて表示
        next.decidePuyoColor();
        next.showNextPuyo();

        //軸ぷよ、子ぷよを生成（位置はまだ決まっていない）
        switch (centerPuyo)
        {
            case 1: centerPuyoObj = Instantiate(redPrefab) as GameObject; break;
            case 2: centerPuyoObj = Instantiate(greenPrefab) as GameObject; break;
            case 3: centerPuyoObj = Instantiate(bluePrefab) as GameObject; break;
            case 4: centerPuyoObj = Instantiate(yellowPrefab) as GameObject; break;
            default: centerPuyoObj = Instantiate(redPrefab) as GameObject; break;
        }
        switch (movablePuyo)
        {
            case 1: movablePuyoObj = Instantiate(redPrefab) as GameObject; break;
            case 2: movablePuyoObj = Instantiate(greenPrefab) as GameObject; break;
            case 3: movablePuyoObj = Instantiate(bluePrefab) as GameObject; break;
            case 4: movablePuyoObj = Instantiate(yellowPrefab) as GameObject; break;
            default: movablePuyoObj = Instantiate(redPrefab) as GameObject; break;
        }

        // ぷよの初期配置を定める
        puyoStatus.x = 2;          // 中心ぷよのx位置: 左から3列目
        puyoStatus.y = 0;         // 中心ぷよのy位置: 天井から一つ上（画面外）
        puyoStatus.sceneX = 80.0f; //中心ぷよのゲームシーン上でのx座標
        puyoStatus.sceneY = 0.0f;  //中心ぷよのゲームシーン上でのy座標
        puyoStatus.dx = 0;// 動くぷよの相対位置: 動くぷよは上方向にある
        puyoStatus.dy = -1;
        puyoStatus.rotation = 90;// 動くぷよの角度は90度（上向き）
        
        // 接地時間はゼロ
        groundFrame = 0;
        // ぷよを描画
        this.setPuyoPosition();
        return true;
    }

    /// <summary>
    /// sceneX, sceneYを使ってぷよを描画（ゲームシーン上に配置）
    /// </summary>
    public void setPuyoPosition()
    {
        //軸ぷよを描画（移動してくる）
        centerPuyoObj.gameObject.transform.position = new Vector3(puyoStatus.sceneX, -puyoStatus.sceneY, 0);
        //子ぷよを描画（移動してくる）
        float x = puyoStatus.sceneX + Mathf.Cos(puyoStatus.rotation * Mathf.PI / 180) * Config.puyoImgWidth;
        float y = puyoStatus.sceneY - Mathf.Sin(puyoStatus.rotation * Mathf.PI / 180) * Config.puyoImgHeight;
        movablePuyoObj.gameObject.transform.position = new Vector3(x, -y, 0);
        
    }

    public bool falling(bool isDownPressed)
    {
        // 現状の場所の下にブロックがあるかどうか確認する
        bool isBlocked = false;
        int x = puyoStatus.x;
        int y = puyoStatus.y;
        int dx = puyoStatus.dx;
        int dy = puyoStatus.dy;
        Debug.Log("puyoStatus.y=" + puyoStatus.y + "puyoStatus.dy=" + puyoStatus.dy);
        if (y + 1 >= Config.stageRows || stage.board[y + 1][x] != 0 || (y + dy + 1 >= 0 && (y + dy + 1 >= Config.stageRows
        || stage.board[y + dy + 1][x + dx] != 0)))
        {
            isBlocked = true;
        }
        if (!isBlocked)
        {
            // 下にブロックがないなら自由落下してよい。プレイヤー操作中の自由落下処理をする
            puyoStatus.sceneY += Config.playerFallingSpeed;
            if (isDownPressed)
            {
                // 下キーが押されているならもっと加速する
                puyoStatus.sceneY += Config.playerDownSpeed;
            }
            if (Mathf.FloorToInt(puyoStatus.sceneY / Config.puyoImgHeight) != y)
            {
                // ブロックの境を超えたので、再チェックする
                // 下キーが押されていたら、得点を加算する
                if (isDownPressed)
                {
                    score.addScore(1);
                }
                //ブロックの境を超えたので、目的地となるマスの下にブロックがあるか確認
                y += 1;
                puyoStatus.y = y;
                if (y + 1 >= Config.stageRows || stage.board[y + 1][x] != 0 || (y + dy + 1 >= 0 && (y + dy + 1 >= Config.
                stageRows || stage.board[y + dy + 1][x + dx] != 0)))
                {
                    isBlocked = true;
                }
                if (!isBlocked)
                {
                    // 境を超えたが特に問題はなかった。次回も自由落下を続ける
                    //ぷよはブロックの境で降下中
                    groundFrame = 0;
                    return false;
                }
                else
                {
                    // 境を超えたらブロックにぶつかった。位置を調節して、接地を開始する
                    puyoStatus.sceneY = y * Config.puyoImgHeight;
                    groundFrame = 1;
                    return false;
                }
            }
            else
            {
                // 自由落下で特に問題がなかった。次回も自由落下を続ける
                //ブロックの境ではなく、ちょうどマスの中にいる
                groundFrame = 0;
                return false;
            }
        }
        if (groundFrame == 0)
        {
            // 初接地である。接地を開始する
            groundFrame = 1;
            return false;
        }
        else
        {
            //接地中はgroundFrameの値が増える
            groundFrame++;
            //下ボタンを押していたら、groundFrameを増やす（接地時間が短くなる）
            if(isDownPressed) groundFrame += Config.addgroundWhileDown;
            Debug.Log("groundFrame="+groundFrame);

            //groundFrameの値が一定の値を超えたら、trueを返す（ぷよを固定する処理へ）
            if (groundFrame > Config.playerGroundFrame)
            {
                return true;
            }
        }
        //地面についているが、一定時間は経過していないとき
        return false;
    }

    public string playing(int frame)
    {
        this.setPuyoPosition();

        //A・Bボタンがしばらく押されていなかったとき
        if (frame - firstClickFrame > Config.interval)
        {
            firstClickFrame = 0;
        }

        
        //ぷよが回転していないときに、Aボタンが押されたら
        if (Input.GetButtonDown("A") && !(rotateFlag || quickTurnFlag))
        {
            // 回転を確認する
            // 回せるかどうかは後で確認。まわすぞ
            int x = puyoStatus.x;
            int y = puyoStatus.y;
            int mx = x + puyoStatus.dx;
            int my = y + puyoStatus.dy;
            int rotation = puyoStatus.rotation;
            bool canRotate = true;

            int cx = 0;
            int cy = 0;
            //右回転
            rotateDirection = -1;
            if (rotation == 0)
            {
                // 右から下に回す時には、自分の下か右下にブロックがあれば1個上に引き上げる。まず下を確認する
                if (y + 2 < 0 || y + 2 >= Config.stageRows || stage.board[y + 2][x] != 0)
                {
                    if (y + 2 >= 0)
                    {
                        // ブロックがある。上に引き上げる
                        cy = -1;
                    }
                }
                // 右下も確認する
                if (y + 2 < 0 || y + 2 >= Config.stageRows || x + 1 < 0 || stage.board[y + 2][x + 1] != 0)
                {
                    if (y + 2 >= 0)
                    {
                        // ブロックがある。上に引き上げる
                        cy = -1;
                    }
                }
                
                if (cy == -1)
                {
                    //上に引き上げる必要がある時、軸ぷよが14段目に行くときは回転出来ないので確認する
                    if (y == 0)
                    {
                        //回転出来ない
                        canRotate = false;
                    }
                    
                    //落下中のぷよが、目的地のマスよりも出発地のマスの方に近いときは、1.5段くらい上げる
                    if (Mathf.FloorToInt(puyoStatus.sceneY / Config.puyoImgHeight * 10) % 10 == 0)
                    {
                        //ぷよがブロックの境を超えていないときは何もしない。
                        //ぷよがマスぴったりにいるかの座標の精度は、小数第一位まで。（if文では小数第一位が0か確認）
                    }
                    else if (Mathf.FloorToInt(puyoStatus.sceneY / Config.puyoImgHeight * 10) % 10 <= 5)
                    {
                        //小数第一位が1～5だったら1.5段くらい上げる
                        cy = -2; //-2という数字に特に意味はない。
                    }
                }

                
            }
            else if (rotation == 90)
            {
                // 上から右に回すときに、右にブロックがあれば左に移動する必要があるのでまず確認する
                if (y + 1 < 0 || x + 1 < 0 || x + 1 >= Config.stageCols || stage.board[y + 1][x + 1] != 0)
                {
                    if (y + 1 >= 0)
                    {
                        // ブロックがある。左に1個ずれる
                        cx = -1;
                    }
                }
                // 左にずれる必要がある時、左にもブロックがあれば回転出来ないので確認する
                if (cx == -1)
                {
                    if (y + 1 < 0 || x - 1 < 0 || y + 1 >= Config.stageRows || x - 1 >= Config.stageCols || stage.
                    board[y + 1][x - 1] != 0)
                    {
                        if (y + 1 >= 0)
                        {
                            // ブロックがある。回転出来なかった
                            canRotate = false;

                            this.checkDoubleClick(frame);
                            
                        }
                    }
                }
            }
            else if (rotation == 180)
            {
                // 左から上には100% 確実に回せる。何もしない
            }
            else if (rotation == 270)
            {
                // 下から左に回すときは、左にブロックがあれば右に移動する必要があるのでまず確認する
                if (y + 1 < 0 || x - 1 < 0 || x - 1 >= Config.stageCols || stage.board[y + 1][x - 1] != 0)
                {
                    if (y + 1 >= 0)
                    {
                        // ブロックがある。右に1個ずれる
                        cx = 1;
                    }
                }
                // 右にずれる必要がある時、右にもブロックがあれば回転出来ないので確認する
                if (cx == 1)
                {
                    if (y + 1 < 0 || x + 1 < 0 || x + 1 >= Config.stageCols || stage.board[y + 1][x + 1] != 0)
                    {
                        if (y + 1 >= 0)
                        {
                            // ブロックがある。回転出来なかった
                            canRotate = false;
                        }

                        this.checkDoubleClick(frame);
                    }
                }
            }
            if (canRotate)
            {
                // 上に移動する必要があるときは、一気にあげてしまう
                if (cy == -1)
                {
                    if (groundFrame > 0)
                    {
                        // 接地しているなら1段引き上げる
                        puyoStatus.y -= 1;
                        //groundFrame = 0;
                    }
                    puyoStatus.sceneY = puyoStatus.y * Config.puyoImgHeight;
                }
                else if (cy == -2)
                {
                    //上に1.5段くらい引き上げる必要があるとき
                    puyoStatus.y -= 1; //一つ上のマスに行く
                    puyoStatus.sceneY = (puyoStatus.y + 0.5f) * Config.puyoImgHeight;
                }

                // 回すことが出来るので、回転後の情報をセットして回転状態にする
                rotateStartFrame = frame;
                rotateBeforeLeft = x * Config.puyoImgHeight;
                rotateAfterLeft = (x + cx) * Config.puyoImgHeight;
                rotateFromRotation = puyoStatus.rotation;

                // 次の状態を先に設定しておく
                puyoStatus.x += cx;
                int distRotation = (puyoStatus.rotation + (-90 + 360)) % 360;
                var dCombiOrigin = new int[4][]{ new int[2]{ 1, 0 }, new int[2]{ 0, -1 }, new int[2]{ -1, 0 }, new int[2]{ 0, 1 } };
                int[] dCombi = dCombiOrigin[distRotation / 90];
                puyoStatus.dx = dCombi[0];
                puyoStatus.dy = dCombi[1];
                rotateFlag = true;
            }
        }
        else if (Input.GetButtonDown("B") && !(rotateFlag || quickTurnFlag))
        {
            // 回転を確認する
            // 回せるかどうかは後で確認。まわすぞ
            int x = puyoStatus.x;
            int y = puyoStatus.y;
            int mx = x + puyoStatus.dx;
            int my = y + puyoStatus.dy;
            int rotation = puyoStatus.rotation;
            bool canRotate = true;
            int cx = 0;
            int cy = 0;
            //左回転
            rotateDirection = 1;
            if (rotation == 0)
            {
                // 右から上には100% 確実に回せる。何もしない
            }
            else if (rotation == 90)
            {
                // 上から左に回すときに、左にブロックがあれば右に移動する必要があるのでまず確認する
                if (y + 1 < 0 || x - 1 < 0 || x - 1 >= Config.stageCols || stage.board[y + 1][x - 1] != 0)
                {
                    if (y + 1 >= 0)
                    {
                        // ブロックがある。右に1個ずれる
                        cx = 1;
                    }
                }
                // 右にずれる必要がある時、右にもブロックがあれば回転出来ないので確認する
                if (cx == 1)
                {
                    if (y + 1 < 0 || x + 1 < 0 || y + 1 >= Config.stageRows || x + 1 >= Config.stageCols || stage.
                    board[y + 1][x + 1] != 0)
                    {
                        if (y + 1 >= 0)
                        {
                            // ブロックがある。回転出来なかった
                            canRotate = false;
                        }

                        this.checkDoubleClick(frame);
                    }
                }
            }
            else if (rotation == 180)
            {
                // 左から下に回す時には、自分の下か左下にブロックがあれば1個上に引き上げる。まず下を確認する
                if (y + 2 < 0 || y + 2 >= Config.stageRows || stage.board[y + 2][x] != 0)
                {
                    if (y + 2 >= 0)
                    {
                        // ブロックがある。上に引き上げる
                        cy = -1;
                    }
                }
                // 左下も確認する
                if (y + 2 < 0 || y + 2 >= Config.stageRows || x - 1 < 0 || stage.board[y + 2][x - 1] != 0)
                {
                    if (y + 2 >= 0)
                    {
                        // ブロックがある。上に引き上げる
                        cy = -1;
                    }
                }
                //上に引き上げる必要がある時、軸ぷよが14段目に行くときは回転出来ないので確認する
                if (cy == -1)
                {
                    if (y == 0)
                    {
                        //回転出来ない
                        canRotate = false;
                    }

                    //落下中のぷよが、目的地のマスよりも出発地のマスの方に近いときは、1.5段くらい上げる
                    if (Mathf.FloorToInt(puyoStatus.sceneY / Config.puyoImgHeight * 10) % 10 == 0)
                    {
                        //ぷよがブロックの境を超えていないときは何もしない。
                        //ぷよがマスぴったりにいるかの座標の精度は、小数第一位まで。（if文では小数第一位が0か確認）
                    }
                    else if (Mathf.FloorToInt(puyoStatus.sceneY / Config.puyoImgHeight * 10) % 10 <= 5)
                    {
                        //小数第一位が1～5だったら1.5段くらい上げる
                        cy = -2; //-2という数字に特に意味はない。
                    }
                }
            }
            else if (rotation == 270)
            {
                // 下から右に回すときは、右にブロックがあれば左に移動する必要があるのでまず確認する
                if (y + 1 < 0 || x + 1 < 0 || x + 1 >= Config.stageCols || stage.board[y + 1][x + 1] != 0)
                {
                    if (y + 1 >= 0)
                    {
                        // ブロックがある。左に1個ずれる
                        cx = -1;
                    }
                }
                // 左にずれる必要がある時、左にもブロックがあれば回転出来ないので確認する
                if (cx == -1)
                {
                    if (y + 1 < 0 || x - 1 < 0 || x - 1 >= Config.stageCols || stage.board[y + 1][x - 1] != 0)
                    {
                        if (y + 1 >= 0)
                        {
                            // ブロックがある。回転出来なかった
                            canRotate = false;
                        }

                        this.checkDoubleClick(frame);
                        
                    }
                }
            }
            if (canRotate)
            {
                // 上に移動する必要があるときは、一気にあげてしまう
                if (cy == -1)
                {
                    if (groundFrame > 0)
                    {
                        // 接地しているなら1段引き上げる
                        puyoStatus.y -= 1;
                        //groundFrame = 0;
                    }
                    puyoStatus.sceneY = puyoStatus.y * Config.puyoImgHeight;
                }
                else if (cy == -2)
                {
                    //上に1.5段くらい引き上げる必要があるとき
                    puyoStatus.y -= 1; //一つ上のマスに行く
                    puyoStatus.sceneY = (puyoStatus.y + 0.5f) * Config.puyoImgHeight;
                }

                // 回すことが出来るので、回転後の情報をセットして回転状態にする
                rotateStartFrame = frame;
                rotateBeforeLeft = x * Config.puyoImgHeight;
                rotateAfterLeft = (x + cx) * Config.puyoImgHeight;
                rotateFromRotation = puyoStatus.rotation;

                // 次の状態を先に設定しておく
                puyoStatus.x += cx;
                int distRotation = (puyoStatus.rotation + 90) % 360;
                var dCombiOrigin = new int[4][] { new int[2] { 1, 0 }, new int[2] { 0, -1 }, new int[2] { -1, 0 }, new int[2] { 0, 1 } };
                int[] dCombi = dCombiOrigin[distRotation / 90];
                puyoStatus.dx = dCombi[0];
                puyoStatus.dy = dCombi[1];
                rotateFlag = true;
            }
        }


        //ぷよが左右移動していないときに、左右キーが押されたら
        if ((keyStatus.right || keyStatus.left) && !moveFlag)
        {
            // 左右の確認をする
            int cx = (keyStatus.right) ? 1 : -1;
            int x = puyoStatus.x;
            int y = puyoStatus.y;
            int mx = x + puyoStatus.dx;
            int my = y + puyoStatus.dy;
            // その方向にブロックがないことを確認する
            // まずは自分の左右を確認
            bool canMove = true;
            if (y < 0 || x + cx < 0 || x + cx >= Config.stageCols || stage.board[y][x + cx] != 0)
            {
                if (y >= 0)
                {
                    canMove = false;
                }
            }
            if (my < 0 || mx + cx < 0 || mx + cx >= Config.stageCols || stage.board[my][mx + cx] != 0)
            {
                if (my >= 0)
                {
                    canMove = false;
                }
            }
            // 接地していない場合は、さらに1個下のブロックの左右も確認する
            if (groundFrame == 0)
            {
                if (y + 1 < 0 || x + cx < 0 || x + cx >= Config.stageCols || stage.board[y + 1][x + cx] != 0)
                {
                    if (y + 1 >= 0)
                    {
                        canMove = false;
                    }
                }
                if (my + 1 < 0 || mx + cx < 0 || mx + cx >= Config.stageCols || stage.board[my + 1][mx + cx] != 0)
                {
                    if (my + 1 >= 0)
                    {
                        canMove = false;
                    }
                }
            }
            if (canMove)
            {
                // 動かすことが出来るので、移動先情報をセットして移動状態にする
                moveStartFrame = frame;
                moveSource = x * Config.puyoImgWidth;
                moveDestination = (x + cx) * Config.puyoImgWidth;
                puyoStatus.x += cx;
                moveFlag = true;
            }
        }

        return "playing";
    }

    //ダブルクリックによってクイックターンを行うか確認
    public void checkDoubleClick(int frame)
    {
        //まず、回転中なら新たにクイックターンは行わない。
        if(rotateFlag) return;
        if(quickTurnFlag) return;
        
        if (firstClickFrame == 0)
        {
            firstClickFrame = frame;
        }
        else if (frame - firstClickFrame <= Config.interval)
        {
            //クイックターンの条件を満たした

            // 上に移動する必要があるときは、一気にあげてしまう
            if (puyoStatus.rotation == 90)
            {
                //軸ぷよが下のとき
                //上に1.5段引き上げる？（あまり確認していない）
                puyoStatus.y -= 2; //二つ上のマスに行く
                puyoStatus.sceneY = (puyoStatus.y + 0.5f) * Config.puyoImgHeight;
            }
            else if(puyoStatus.rotation == 270)
            {
                //軸ぷよが上のとき
                //1段下げる？（あまり確認していない）
                puyoStatus.y += 1;
                puyoStatus.sceneY = (puyoStatus.y + 0.5f) * Config.puyoImgHeight;
            }

            // 次の状態を先に設定しておく
            rotateStartFrame = frame;
            rotateFromRotation = puyoStatus.rotation;
            puyoStatus.dx = 0;
            //次の子ぷよの位置は、現在のrotationによって決まる
            switch (puyoStatus.rotation)
            {
                case 90: puyoStatus.dy = 1; break;
                case 270: puyoStatus.dy = -1; break;
            }
            quickTurnFlag = true;

            firstClickFrame = 0;
        }
    }

    public bool moving(int frame)
    {
        float ratio = Mathf.Min(1, (frame - moveStartFrame) / Config.playerMoveFrame);
        puyoStatus.sceneX = ratio * (moveDestination - moveSource) + moveSource;
        this.setPuyoPosition();
        if (ratio == 1)
        {
            //回転途中ならfalseを返す
            return false;
        }
        //回転が終了したらtrueを返す
        return true;
    }

    public bool rotating(int frame)
    {
        float ratio = Mathf.Min(1.0f, ((frame - rotateStartFrame) / Config.playerRotateFrame));
        puyoStatus.sceneX = (rotateAfterLeft - rotateBeforeLeft) * ratio + rotateBeforeLeft;

        if(rotateDirection == -1)
        {
            puyoStatus.rotation = rotateFromRotation + Mathf.FloorToInt(ratio * -90);
        }
        else if(rotateDirection == 1)
        {
            puyoStatus.rotation = rotateFromRotation + Mathf.FloorToInt(ratio * 90);
        }

        this.setPuyoPosition();

        if (ratio == 1)
        {
            if(rotateDirection == -1)
            {
                puyoStatus.rotation = (rotateFromRotation + (-90 + 360)) % 360;
            }
            else if(rotateDirection == 1)
            {
                puyoStatus.rotation = (rotateFromRotation + 90) % 360;
            }
            return false;
        }
        return true;
    }

    //180度一気に回転
    public bool quickTurn(int frame)
    {
        float ratio = Mathf.Min(1.0f, ((frame - rotateStartFrame) / Config.playerRotateFrame));
        if (rotateDirection == -1)
        {
            //右回転
            puyoStatus.rotation = rotateFromRotation + Mathf.FloorToInt(ratio * -180);
        }
        else if (rotateDirection == 1)
        {
            //左回転
            puyoStatus.rotation = rotateFromRotation + Mathf.FloorToInt(ratio * 180);
        }

        this.setPuyoPosition();
        
        if (ratio == 1)
        {
            //ぷよを180度回す（右・左は関係ない）
            puyoStatus.rotation = (rotateFromRotation + 180) % 360;
            return false;
        }
        return true;
    }

    public bool fix(int frame)
    {
        // 現在のぷよをステージ上に配置する
        int x = puyoStatus.x;
        int y = puyoStatus.y;
        int dx = puyoStatus.dx;
        int dy = puyoStatus.dy;

        //20210910追加
        //もしかしたら、ぷよオブジェクトがマスとマスの間にあるかもしれないので、
        //ぷよオブジェクトをマスの中に配置し直す
        puyoStatus.sceneX = x * Config.puyoImgWidth;
        puyoStatus.sceneY = y * Config.puyoImgHeight;

        //もしまだ回転中なのにぷよを固定しようとしたら、
        //回転後の情報をもとにぷよを固定する
        if (rotateFlag)
        {
            if (rotateDirection == -1)
            {
                puyoStatus.rotation = (rotateFromRotation + (-90 + 360)) % 360;
            }
            else if (rotateDirection == 1)
            {
                puyoStatus.rotation = (rotateFromRotation + 90) % 360;
            }
        }

        if(quickTurnFlag)
        {
            //ぷよを180度回す（右・左は関係ない）
            puyoStatus.rotation = (rotateFromRotation + 180) % 360;
        }
        
        this.setPuyoPosition();

        if (y >= 0)
        {
           // 画面外のぷよは消してしまう
            stage.setPuyo(x, y, centerPuyo);
            stage.puyoCount++;
        }
        if (y + dy >= 0)
        {
            // 画面外のぷよは消してしまう
            stage.setPuyo(x + dx, y + dy, movablePuyo);
            stage.puyoCount++;
        }
        
        //ぷよが設置するときのアニメーション（ぶよんとなる）（もう少し厳密なアニメーションが必要）
        /*
        float ratio = Mathf.Min(1, (frame - moveStartFrame) / Config.playerMoveFrame);
        puyoStatus.sceneX = ratio * (moveDestination - moveSource) + moveSource;
        this.setPuyoPosition();
        if (ratio == 1)
        {
            //回転途中ならfalseを返す
            return false;
        }
        //回転が終了したらtrueを返す
        return true;
        */

        //しばらくfix関数にとどまる
        fixingFrame++;
        if(fixingFrame > Config.fixFrame){
            fixingFrame = 0;
            return true;
        }
        return false;
    }

    public string playMoveRotate(int frame)
    {

        // まず自由落下を確認する
        // 下キーが押されていた場合、それ込みで自由落下させる
        if (this.falling(keyStatus.down))
        {
            // 落下が終わっていたら、ぷよを固定する
            this.setPuyoPosition();
            return "fix";
        }

        this.playing(frame);

        //rotatingがfalseを返すまで、rotating関数を動作させる
        if (rotateFlag)
        {
            rotateFlag = this.rotating(frame);
        }
        else if (quickTurnFlag)
        {
            quickTurnFlag = this.quickTurn(frame);
        }

        //movingがfalseを返すまで、moving関数を動作させる
        if (moveFlag)
        {
            moveFlag = this.moving(frame);
        }

        //fixを返す以外は、playing（プレイヤーが操作可能を返す）
        return "playing";
    }


    public void batankyu()
    {
        //上ボタンを押したらリロード
        if (keyStatus.up)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void Start()
    {
        this.next = gameObject.GetComponent<Next>();
        this.score = gameObject.GetComponent<Score>();
        this.stage = gameObject.GetComponent<Stage>();

        this.redPrefab = Resources.Load("red_Prefab") as GameObject;
        this.greenPrefab = Resources.Load("green_Prefab") as GameObject;
        this.bluePrefab = Resources.Load("blue_Prefab") as GameObject;
        this.yellowPrefab = Resources.Load("yellow_Prefab") as GameObject;
    }

    void Update()
    {
        // キーボードの入力を確認する
        if (Input.GetAxisRaw("Button_H") < 0)
        {
            keyStatus.left = true;
        }
        else if (Input.GetAxisRaw("Button_H") > 0)
        {
            keyStatus.right = true;
        }
        else
        {
            keyStatus.left = false;
            keyStatus.right = false;
        }

        if (Input.GetAxisRaw("Button_V") < 0)
        {
            keyStatus.down = true;
        }
        else if (Input.GetAxisRaw("Button_V") > 0)
        {
            keyStatus.up = true;
        }
        else
        {
            keyStatus.down = false;
            keyStatus.up = false;
        }
    }
}
