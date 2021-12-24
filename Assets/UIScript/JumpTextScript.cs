using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class JumpTextScript : MonoBehaviour
{
    public GameObject ParentObject;
    public GameObject JumpTextObject;

    private Color textColor;
    
    private void Start()
    {
        textColor = Color.white;
    }
    void Update()
    {
        DownKeyCheck();
    }

    private void DownKeyCheck()
    {
        if (Input.anyKeyDown)
        {
            foreach (KeyCode code in Enum.GetValues(typeof(KeyCode)))
            {

                if (Input.GetKeyDown(code))
                {
                    if (code == KeyCode.Alpha0 || code == KeyCode.Alpha1 || code == KeyCode.Alpha2 || code == KeyCode.Alpha3 || code == KeyCode.Alpha4 ||
                    code == KeyCode.Alpha5 || code == KeyCode.Alpha6 || code == KeyCode.Alpha7 || code == KeyCode.Alpha8 || code == KeyCode.Alpha9 ||
                    code == KeyCode.Mouse0 || code == KeyCode.Mouse1 || code == KeyCode.Mouse2 || code == KeyCode.Mouse3 || code == KeyCode.Mouse4 || 
                    code == KeyCode.Mouse5 || code == KeyCode.Mouse6 || code == KeyCode.LeftArrow || code == KeyCode.RightArrow)
                    {
                        switch (code)
                        {
                            case KeyCode.Alpha1:
                                textColor = Color.white;
                                break;
                            case KeyCode.Alpha2:
                                textColor = Color.black;
                                break;
                            case KeyCode.Alpha3:
                                textColor = Color.magenta;
                                break;
                            case KeyCode.Alpha4:
                                textColor = Color.cyan;
                                break;
                            case KeyCode.Alpha5:
                                textColor = Color.yellow;
                                break;
                            case KeyCode.Alpha6:
                                textColor = Color.red;
                                break;
                            case KeyCode.Alpha7:
                                textColor = Color.blue;
                                break;
                            case KeyCode.Alpha8:
                                textColor = Color.green;
                                break;
                            case KeyCode.Alpha9:
                                textColor = Color.grey;
                                break;
                            case KeyCode.Alpha0:
                                textColor = Color.clear;
                                break;
                        }
                        break;
                    }

                    var obj = Instantiate(JumpTextObject, ParentObject.transform);
                    obj.transform.Find("Text").GetComponent<Text>().text = $"{code}";
                    obj.transform.Find("Text").GetComponent<Text>().color = textColor;
                    break;
                }
            }
        }
    }
}
