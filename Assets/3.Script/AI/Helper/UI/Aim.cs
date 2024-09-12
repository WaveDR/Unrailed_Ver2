using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Aim : MonoBehaviour
{
    Image _aim;
    private Dictionary<WorldResource.EType, Transform> _order;
    [SerializeField] private Transform[] _orderTransform;

    private void Awake()
    {
        _order = new Dictionary<WorldResource.EType, Transform>();
        _aim = GetComponent<Image>();
        _aim.enabled = false;
        Init();
    }

    private void Init()
    {
        _order.Add(WorldResource.EType.Stone, _orderTransform[0]);
        _order.Add(WorldResource.EType.Wood, _orderTransform[1]);
        _order.Add(WorldResource.EType.Water, _orderTransform[2]);
        _order.Add(WorldResource.EType.Resource, _orderTransform[3]);

    }

    public void MoveAim(WorldResource.EType Order)
    {
        _aim.enabled = true;
        Transform order = _order[Order].transform;
        transform.position = order.position;
    }

    public void AimOff()
    {
        _aim.enabled = false;
    }
}
