using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//2022/04/30
//ScoreObjectsをpubScoreObjectにし、staticなscoreObjectを作った
//2022/06/27
//ScoreObjectsに戻した

public class Score : MonoBehaviour
{
    public static Sprite[] fontTemplateList = new Sprite[10];
    public GameObject scoreObject;
    static SpriteRenderer spRenderer;
    // static fontLength;
    public static int score;
    static int[] rensaBonus = new int[] { 0, 8, 16, 32, 64, 96, 128, 160, 192, 224, 256, 288, 320, 352, 384, 416, 448, 480, 512, 544, 576, 608, 640, 672 };
    static int[] pieceBonus = new int[] { 0, 0, 0, 0, 2, 3, 4, 5, 6, 7, 10, 10 };
    static int[] colorBonus = new int[] { 0, 0, 3, 6, 12, 24 };

    public void initialize()
    {
        score = 0;
        showScore();
    }

    public void showScore()
    {
        int scoreTemp = score;
        // スコアを下の桁から埋めていく
        for (int i = 0; i < 8; i++)
        {
            // 10で割ったあまりを求めて、一番下の桁を取り出す
            int number = scoreTemp % 10;
            // 一番うしろに追加するのではなく、一番前に追加することで、スコアの並びを数字と同じようにする
            //scoreElement.insertBefore(this.fontTemplateList[number].cloneNode(true), scoreElement.firstChild);

            GameObject child = scoreObject.transform.GetChild(i).gameObject;
            spRenderer = child.gameObject.GetComponent<SpriteRenderer>();
            spRenderer.sprite = fontTemplateList[number];

            // 10 で割って次の桁の準備をしておく
            scoreTemp = Mathf.FloorToInt(scoreTemp / 10);
        }

        
    }

    public void calculateScore(int rensa, int piece, int color)
    {
        //rensa...連鎖数, piece...消したぷよの個数, color...色の種類？
        rensa = Mathf.Min(rensa, Score.rensaBonus.Length - 1);
        piece = Mathf.Min(piece, Score.pieceBonus.Length - 1);
        color = Mathf.Min(color, Score.colorBonus.Length - 1);
        int scale = Score.rensaBonus[rensa-1] + Score.pieceBonus[piece-1] + Score.colorBonus[color-1];
        if (scale == 0)
        {
            scale = 1;
        }
        addScore(scale * piece * 10);
        Debug.Log("rensa=" + rensa + " piece=" + piece + " color=" + color + " score=" + score);
    }

    public void addScore(int addScore)
    {
        score += addScore;
        showScore();
    }

    void Start()
    {

    }
}
