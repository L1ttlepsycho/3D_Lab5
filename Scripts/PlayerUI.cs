using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interfaces;

public class PlayerUI : MonoBehaviour
{
    private IPlayerAction action;
    private GameJudge gameJudge;
    public GameObject cam;
    public int life = 6;                   //Ѫ��
    //ÿ��GUI��style
    GUIStyle bold_style = new GUIStyle();
    GUIStyle score_style = new GUIStyle();
    GUIStyle text_style = new GUIStyle();
    GUIStyle over_style = new GUIStyle();        
    private bool game_start = false;       //��Ϸ��ʼ

    void Start()
    {
        action = SceneController.getInstance() as IPlayerAction;
        if(action == null)
        {
            Debug.LogError("action null!");
        }
        gameJudge = GameJudge.getInstance();
    }

    void OnGUI()
    {
        bold_style.normal.textColor = new Color(1, 0, 0);
        bold_style.fontSize = 16;
        text_style.normal.textColor = new Color(0, 0, 0, 1);
        text_style.fontSize = 16;
        score_style.normal.textColor = new Color(1, 0, 1, 1);
        score_style.fontSize = 16;
        over_style.normal.textColor = new Color(1, 0, 0);
        over_style.fontSize = 25;

        if (game_start)
        {
            //�û����
            if (Input.GetButtonDown("Fire1"))
            {
                action.shoot(cam);
            }

            GUI.Label(new Rect(10, 5, 200, 50), "����:", text_style);
            GUI.Label(new Rect(55, 5, 200, 50), action.getScore().ToString(), score_style);
            //��Ϸ����
            if (life == 0 || gameJudge.over())
            {
                GUI.Label(new Rect(Screen.width / 2 - 20, Screen.height / 2 - 25, 100, 100), "��Ϸ����", over_style);
                GUI.Label(new Rect(Screen.width / 2 - 10, Screen.height / 2 , 50, 50), "Score:", text_style);
                GUI.Label(new Rect(Screen.width / 2 + 50, Screen.height / 2 , 50, 50), gameJudge.score.ToString(), text_style);
            }
        }
        else
        {
            GUI.Label(new Rect(Screen.width / 2 - 30, Screen.height / 2 - 50, 100, 100), "Hit UFO", over_style);

            if (GUI.Button(new Rect(Screen.width / 2 - 20, Screen.height / 2 + 50, 100, 50), "��Ϸ��ʼ"))
            {
                game_start = true;
                action.gameStart();
            }
        }
    }
}
