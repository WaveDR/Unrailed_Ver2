using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Helper : MonoBehaviour
{
    [HideInInspector]
    public PathFindingAgent Agent;

    public bool arrive = false;
    public bool GotoPlayer = false;
    private Aim _orderUI;
    private ItemUI _itemUI;

    public Transform UItransform;
    [HideInInspector]
    public Dictionary<KeyCode, System.Action> Order;
    [HideInInspector]
    public WorldResource.EType TargetResource;
    [HideInInspector]
    public WorldResource.EType DefaultResource = WorldResource.EType.Wood;

    public void ArriveStation()
    {
        arrive = !arrive;
    }

    private void Awake()
    {
        Agent = GetComponent<PathFindingAgent>();
        TargetResource = DefaultResource;
        Order = new Dictionary<KeyCode, Action>();
        _orderUI = FindObjectOfType<Aim>();
        _itemUI = FindObjectOfType<ItemUI>();
        Init();
    }


    void Init()
    {
        Order[KeyCode.Alpha1] = () =>
        {
            TargetResource = WorldResource.EType.Stone;
        };

        Order[KeyCode.Alpha2] = () =>
        {
            TargetResource = WorldResource.EType.Wood;
        };

        Order[KeyCode.Alpha3] = () =>
        {
            TargetResource = WorldResource.EType.Water;
        };

        Order[KeyCode.Alpha4] = () =>
        {
            TargetResource = WorldResource.EType.Resource;
        };
    }


    private void Update()
    {
        if (Input.anyKeyDown)
        {
            foreach (var dic in Order)
            {
                if (Input.GetKeyDown(dic.Key))
                {
                    dic.Value();

                    if(Input.GetKey(KeyCode.E))
                    {
                        _orderUI.MoveAim(TargetResource);

                    }

                }

            }
        }

        if(Input.GetKeyUp(KeyCode.E))
        {
            _orderUI.AimOff();
        }
    }

    public void CheckPlayer()
    {
        GotoPlayer = !GotoPlayer;
    }

}
