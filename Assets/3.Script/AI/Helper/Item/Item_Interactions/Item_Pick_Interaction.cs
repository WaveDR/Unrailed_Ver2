using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Pick_Interaction : SimpleInteraction
{
    protected Item_Pick _pick;

    private void Awake()
    {
        _pick = GetComponent<Item_Pick>();
        _pick.Type = WorldResource.EType.Stone;
    }

    public override bool CanPerform()
    {
        return base.CanPerform() && !_pick.IsOn;
    }

    public override bool Perform()
    {
        _pick.PickUp();
        return base.CanPerform() && !_pick.IsOn;
    }
}
