using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailPooling : MonoBehaviour
{
    public GameObject prefabs;
    public GameObject[] rails;
    public Vector3 poolingVec;

    public int countPlus;
    public int prefabsCount;
    // Start is called before the first frame update
    void Awake()
    {
        rails = new GameObject[prefabsCount];

        for (int i = 0; i < rails.Length; i++)
        {
            rails[i] = Instantiate(prefabs, poolingVec, Quaternion.identity, transform);
            rails[i].SetActive(false);
        }
    }
    public void TransformRail(Transform moveVec, bool MakeRail)
    {
        rails[countPlus].transform.position = moveVec.position;
        rails[countPlus].SetActive(true);

        if (MakeRail)
        {
            rails[countPlus].transform.parent = moveVec;
        }

        countPlus++;
        if (countPlus >= rails.Length)
        {
            countPlus = 0;
        }
    }
}
