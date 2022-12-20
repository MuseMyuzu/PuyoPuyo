using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//参考：https://puyo-camp.jp/posts/71019

/// <summary>
/// 設定値（定数）を定義するクラス
/// </summary>
public class Config : MonoBehaviour
{
    public static int puyoImgWidth = 40; // ぷよぷよ画像の幅
    public static int puyoImgHeight = 40; // ぷよぷよ画像の高さ

    public static int fontHeight = 33;

    public static int stageCols = 6; // ステージの横の個数
    public static int stageRows = 13; // ステージの縦の個数
    // フィールドサイズ追加
    // 高さが全部入るように調整
    public static string stageBackgroundColor = "#ffffff"; // ステージの背景色
    public static string scoreBackgroundColor = "#24c0bb"; // スコアの背景色

    public static int freeFallingSpeed = 25; // 自由落下のスピード（ちぎったとき）
    public static int erasePuyoCount = 4; // 何個以上揃ったら消えるか
    public static float eraseAnimationDuration = 47.0f; // 何フレームでぷよを消すか

    public static int puyoColors = 4; // 何色のぷよを使うか
    public static float playerFallingSpeed = 0.9f; // プレイ中の自然落下のスピード
    public static int playerDownSpeed = 10; // プレイ中の下キー押下時の落下スピード
    public static int playerGroundFrame = 36; // 何フレーム接地したらぷよを固定するか
    public static float playerMoveFrame = 10.0f; // 左右移動に消費するフレーム数
    public static float playerRotateFrame = 10.0f; // 回転に消費するフレーム数

    public static float zenkeshiDuration = 150.0f; // 全消し時のアニメーションミリセカンド
    public static int gameOverFrame = 3000; // ゲームオーバー演出のサイクルフレーム
    public static int fixFrame = 16; //ぷよを固定する時間（操作不能）
    public static int addgroundWhileDown = 5; //下ボタンを押しているとき、groundFrameに追加で加算する量
    public static int interval = 10; //ダブルクリックのクリック間の時間
    
    void Start()
    {
        
    }
}
