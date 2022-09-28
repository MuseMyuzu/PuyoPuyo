using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

/// <summary>
/// ぷよ状態の保存を行うクラス
/// </summary>
public class MemoryPlayer : MonoBehaviour
{
    private Stage stage;
    private StreamWriter sw;

    public void save(){

        sw = new StreamWriter(@"puyo.csv", false, Encoding.GetEncoding("UTF-8"));

        for(int i=0; i<Config.stageRows; i++)
        {
            string[] texts = new string[Config.stageCols];
            for(int j=0; j<Config.stageCols; j++)
            {
                texts[j] = stage.board[i][j].ToString();
            }
            string textLine = string.Join(",", texts);
            sw.WriteLine(textLine);
        }

        sw.Close();
    }

    // Start is called before the first frame update
    void Start()
    {
        this.stage     = gameObject.GetComponent<Stage>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
