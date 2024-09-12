using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Axe : AI_Item
{
    public override int ID { get; protected set; } = 1;
    public override bool IsOn { get; protected set; } = false;
    public override void PickUp()
    {
        IsOn = !IsOn;
        //Debug.Log($"도끼는 지금 {(IsOn ? "들려있어용":"바닥이에용")}");
    }

}
