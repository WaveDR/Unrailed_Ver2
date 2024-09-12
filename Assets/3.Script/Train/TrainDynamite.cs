using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainDynamite : TrainMovement
{
    [SerializeField] private Transform spawnPos;
    [SerializeField] private MyDynamiteItem _dynamiteItemPrefab;

    [SerializeField] private float spawnSelect;
    [SerializeField] private float spawnTime;
    [SerializeField] private Text text;

    private MyDynamiteItem _dynamiteItem = null;

    // 만들었는가?
    public bool isMake;

    private void Awake()
    {
        GetMesh();
        spawnTime = spawnSelect;
    }

    private void Update()
    {
        TrainMovePos();
        //isMake 건들일 수 있게 만들 것 05 25
        if (!isBurn || !isReady)
        {
            if (isMake)
            {
                spawnTime -= Time.deltaTime;

                if (spawnTime <= 0)
                {
                    MakeDynamite();
                    isMake = false;
                    spawnTime = spawnSelect;
                }
            }
            int i = (int)spawnTime;
            text.text = i.ToString("D2");
        }
    
    }

    private void MakeDynamite()
    {
        _dynamiteItem = Instantiate(_dynamiteItemPrefab, spawnPos);
        _dynamiteItem.transform.localPosition = Vector3.zero;
        _dynamiteItem.transform.localRotation = Quaternion.identity;
        _dynamiteItem.transform.localScale = Vector3.one;
    }

    public void RequestDynamite()
    {
        isMake = true;
    }

    public MyDynamiteItem GetItem()
    {
        if(!isMake)
        {
            MyDynamiteItem temp = _dynamiteItem;
            _dynamiteItem = null;
            RequestDynamite();

            return temp;
        }
        else
        {
            return null;
        }
    }
}
