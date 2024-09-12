using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InVisibleObject : MonoBehaviour
{
    // 블럭, 정적물체, 레일, 동적물체에 다 붙일거야

    public bool isEmpty = false;

    private enum EInvisibleType { block, staticObject, rail, dynamicObject, tool };
    [SerializeField] private EInvisibleType invisibleType;

    private Collider _collider;
    private Renderer _renderer;
    private RailController _railController;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _renderer = GetComponent<Renderer>();
        _railController = GetComponent<RailController>();
    }

    public void Show()
    {
        switch (invisibleType)
        {
            case EInvisibleType.block:
                if (!isEmpty)
                    _renderer.enabled = true;
                break;
            case EInvisibleType.staticObject:
                transform.GetChild(0).gameObject.SetActive(true);
                _collider.isTrigger = false;
                break;
            case EInvisibleType.dynamicObject:
                transform.GetChild(0).gameObject.SetActive(true);
                _collider.isTrigger = false;
                break;
            case EInvisibleType.rail:
                if (!_railController.isInstance)
                    transform.GetChild(0).gameObject.SetActive(true);
                break;
            case EInvisibleType.tool:
                transform.GetChild(0).gameObject.SetActive(true);
                break;
        }
    }

    public void UnShow()
    {
        switch (invisibleType)
        {
            case EInvisibleType.block:
                if (!isEmpty)
                    _renderer.enabled = false;
                break;
            case EInvisibleType.staticObject:
                transform.GetChild(0).gameObject.SetActive(false);
                _collider.isTrigger = true;
                break;
            case EInvisibleType.dynamicObject:
                transform.GetChild(0).gameObject.SetActive(false);
                _collider.isTrigger = true;
                break;
            case EInvisibleType.rail:
                if (!_railController.isInstance)
                    for (int i = 0; i < transform.childCount; i++)
                        transform.GetChild(i).gameObject.SetActive(false);
                break;
            case EInvisibleType.tool:
                transform.GetChild(0).gameObject.SetActive(false);
                break;
        }
    }
}
