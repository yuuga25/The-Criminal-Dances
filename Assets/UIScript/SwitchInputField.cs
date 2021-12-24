using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchInputField : MonoBehaviour
{
    public InputField inputField_Question;
    public InputField inputField_Answer1;
    public InputField inputField_Answer2;
    public InputField inputField_Answer3;

    private void Update()
    {
        if (inputField_Question.isFocused && Input.GetKeyDown(KeyCode.Tab))
        {
            if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                inputField_Answer3.Select();
            }
            else inputField_Answer1.Select();
        }

        if (inputField_Answer1.isFocused && Input.GetKeyDown(KeyCode.Tab))
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                inputField_Question.Select();
            }
            else inputField_Answer2.Select();
        }

        if (inputField_Answer2.isFocused && Input.GetKeyDown(KeyCode.Tab))
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                inputField_Answer1.Select();
            }
            else inputField_Answer3.Select();
        }

        if( inputField_Answer3.isFocused && Input.GetKeyDown(KeyCode.Tab))
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                inputField_Answer2.Select();
            }
            else inputField_Question.Select();
        }
    }
}
