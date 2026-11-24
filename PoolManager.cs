using System;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    // 프리팹 보관 변수 2 
    // 풀 담당 리스트 2
    public GameObject[] prefabs;
    List<GameObject>[] pools;

    private void Awake()
    {
        pools = new List<GameObject>[prefabs.Length];
        
        for (int i = 0; i < pools.Length; i++)
        {
            pools[i] = new List<GameObject>();
        }
    }

    public GameObject Get(int index)
    {
        GameObject select = null;
        foreach (GameObject g in pools[index])
        {
            if (!g.activeSelf)
            {
                select = g;
                select.SetActive(true);
                break;
            }
        }
        if (!select)
        {
                select = Instantiate(prefabs[index], transform); 
                pools[index].Add(select);
                select.SetActive(true);
        }
        return select;
    }
}
