using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Axe_Interaction : SimpleInteraction
{
    protected Item_Axe _axe;

    private void Awake()
    {
        _axe = GetComponent<Item_Axe>();
        _axe.Type = WorldResource.EType.Wood;
    }

    public override bool CanPerform()
    {
        return base.CanPerform() && !_axe.IsOn;
    }

    public override bool Perform()
    {
        _axe.PickUp();
        return base.CanPerform() && !_axe.IsOn;
    }
}
