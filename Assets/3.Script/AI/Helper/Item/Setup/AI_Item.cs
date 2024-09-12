using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AI_Item : MonoBehaviour
{
    [HideInInspector]
    public WorldResource.EType Type = WorldResource.EType.Wood;
    [SerializeField] protected Transform _InteractionMarker;
    public Vector3 InteractionPoint => _InteractionMarker != null ? _InteractionMarker.position : transform.position;
    protected List<BaseInteraction> _cachedInteractions = null;

    public abstract bool IsOn { get; protected set; }
    public abstract int ID { get; protected set; }

    public List<BaseInteraction> Interactions
    {
        get
        {
            if (_cachedInteractions == null)
                _cachedInteractions = new List<BaseInteraction>(GetComponents<BaseInteraction>());

            return _cachedInteractions;
        }
    }
    void Start()
    {
        ItemManager.Instance.RegisterItem(this);
    }

    private void OnDestroy()
    {
        ItemManager.Instance.DeregisterItem(this);
    }


    public virtual void PickUp()
    {

    }
}
