using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Misc;

public class main : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {   
        //StartCoroutine("MoveNext");
        Debug.Log(Utils.IsSameDay(1556440858, 1559032858));
    }

    IEnumerator MoveNext()
    {
        while (true)
        {
            var rd = Utils.RandomFloat(1.1f, 10.0f);
            Debug.Log(rd);
            yield return new WaitForSecondsRealtime(0.2f);
        }
    }
}
