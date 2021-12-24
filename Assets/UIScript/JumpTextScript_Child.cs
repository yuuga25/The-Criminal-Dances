using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpTextScript_Child : MonoBehaviour
{
    void Start()
    {
        int r = Random.Range(0, 1761);
        var pos = this.gameObject.transform.localPosition;
        pos.x = r - 880;
        pos.y = 580;
        pos.z = 0;
        gameObject.transform.localPosition = pos;

        StartCoroutine(DestroyObject());
    }
    IEnumerator DestroyObject()
    {
        yield return new WaitForSeconds(2f);
        Destroy(this.gameObject);
    }
}
