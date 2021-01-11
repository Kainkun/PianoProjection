using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lightTest : MonoBehaviour
{

    private void Start()
    {
        StartCoroutine(test());
    }
    IEnumerator test()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1, 3));
            transform.position = new Vector3(-1.25f + Random.Range(-10, 11) * 0.4938f, 10.73f, 9.3f);
            yield return null;
        }
    }
}
