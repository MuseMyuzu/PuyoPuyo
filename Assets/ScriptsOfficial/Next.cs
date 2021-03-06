using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Next : MonoBehaviour
{
    private GameObject red, green, blue, yellow;
    private GameObject nextCenterPuyoObj, nextMovablePuyoObj, nextnextCenterPuyoObj, nextnextMovablePuyoObj;
    public int nextCenterPuyo{get; set;}
    public int nextMovablePuyo{get; set;}
    public int nextnextCenterPuyo{get; set;}
    public int nextnextMovablePuyo{get; set;}

    /// <summary>
    /// 開幕でネクストとネクネクの色を決定する
    /// </summary>
    public void decidePuyoColorOpening()
    {
        //最初は赤、緑、青の3色からのみ選ばれる（のちに改良の余地あり）
        int puyoColors = Mathf.Max(1, Mathf.Min(5, Config.puyoColors));
        //ネクストとネクネクの色を決める
        nextCenterPuyo = Mathf.FloorToInt(UnityEngine.Random.value * (puyoColors - 1)) + 1;
        nextMovablePuyo = Mathf.FloorToInt(UnityEngine.Random.value * (puyoColors - 1)) + 1;
        nextnextCenterPuyo = Mathf.FloorToInt(UnityEngine.Random.value * (puyoColors-1)) + 1;
        nextnextMovablePuyo = Mathf.FloorToInt(UnityEngine.Random.value * (puyoColors-1)) + 1;
    }

    /// <summary>
    /// 通常時、ネクストとネクネクの色を決定する
    /// </summary>
    public void decidePuyoColor()
    {
        int puyoColors = Mathf.Max(1, Mathf.Min(5, Config.puyoColors));
        //ネクストは一つ前のネクネクが移ってくる
        nextCenterPuyo = nextnextCenterPuyo;
        nextMovablePuyo = nextnextMovablePuyo;
        //通常は1～4の数字が代入される
        nextnextCenterPuyo = Mathf.FloorToInt(UnityEngine.Random.value * puyoColors) + 1;
        nextnextMovablePuyo = Mathf.FloorToInt(UnityEngine.Random.value * puyoColors) + 1;
    }

    /// <summary>
    /// ネクスト、ネクネクの欄にぷよを表示する
    /// </summary>
    public void showNextPuyo()
    {
        //現在表示されているネクスト、ネクネクを消去
        GameObject[] nexts = GameObject.FindGameObjectsWithTag("next");
        foreach (GameObject next in nexts)
        {
            Destroy(next);
        }

        switch (nextCenterPuyo)
        {
            case 1: nextCenterPuyoObj = Instantiate(red) as GameObject; break;
            case 2: nextCenterPuyoObj = Instantiate(green) as GameObject; break;
            case 3: nextCenterPuyoObj = Instantiate(blue) as GameObject; break;
            case 4: nextCenterPuyoObj = Instantiate(yellow) as GameObject; break;
        }
        nextCenterPuyoObj.transform.position = new Vector3(320.0f, -75.0f, 0.0f);

        switch (nextMovablePuyo)
        {
            case 1: nextMovablePuyoObj = Instantiate(red) as GameObject; break;
            case 2: nextMovablePuyoObj = Instantiate(green) as GameObject; break;
            case 3: nextMovablePuyoObj = Instantiate(blue) as GameObject; break;
            case 4: nextMovablePuyoObj = Instantiate(yellow) as GameObject; break;
        }
        nextMovablePuyoObj.transform.position = new Vector3(320.0f, -20.0f, 0.0f);

        switch (nextnextCenterPuyo)
        {
            case 1: nextnextCenterPuyoObj = Instantiate(red) as GameObject; break;
            case 2: nextnextCenterPuyoObj = Instantiate(green) as GameObject; break;
            case 3: nextnextCenterPuyoObj = Instantiate(blue) as GameObject; break;
            case 4: nextnextCenterPuyoObj = Instantiate(yellow) as GameObject; break;
        }
        nextnextCenterPuyoObj.transform.position = new Vector3(320.0f, -200.0f, 0.0f);

        switch (nextnextMovablePuyo)
        {
            case 1: nextnextMovablePuyoObj = Instantiate(red) as GameObject; break;
            case 2: nextnextMovablePuyoObj = Instantiate(green) as GameObject; break;
            case 3: nextnextMovablePuyoObj = Instantiate(blue) as GameObject; break;
            case 4: nextnextMovablePuyoObj = Instantiate(yellow) as GameObject; break;
        }
        nextnextMovablePuyoObj.transform.position = new Vector3(320.0f, -150.0f, 0.0f);
    }

    void Start(){
        this.red = Resources.Load("next_red") as GameObject;
        this.green = Resources.Load("next_green") as GameObject;
        this.blue = Resources.Load("next_blue") as GameObject;
        this.yellow = Resources.Load("next_yellow") as GameObject;
    }
}
