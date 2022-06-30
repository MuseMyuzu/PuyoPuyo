using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuyoImage : MonoBehaviour
{
    private float gameOverFrame;
    private GameObject batankyuObject;
    private GameObject batankyuPrefab;

    public void initialize()
    {
        
    }


    public void prepareBatankyu(int frame)
    {
        gameOverFrame = Mathf.FloorToInt(frame);
        batankyuObject = Instantiate(batankyuPrefab) as GameObject;
    }

    public void batankyu(int frame)
    {
        float ratio = (frame - gameOverFrame) / Config.gameOverFrame;
        float x = Mathf.Cos(Mathf.PI / 2 + ratio * Mathf.PI * 2 * 10) * Config.puyoImgWidth;
        float y = Mathf.Cos(Mathf.PI + ratio * Mathf.PI * 2) * Config.puyoImgHeight * Config.stageRows / 4 + Config.
            puyoImgHeight * Config.stageRows / 2;
        batankyuObject.gameObject.transform.position = new Vector3(x+100, -y, -1);
    }

    private void Start() {
        //ばたんきゅーのPrefabを取得
        batankyuPrefab = Resources.Load("batankyuPrefab") as GameObject;
    }
}
