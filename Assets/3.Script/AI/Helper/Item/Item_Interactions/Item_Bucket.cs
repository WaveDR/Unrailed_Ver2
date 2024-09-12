using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Bucket : AI_Item
{
    public override int ID { get; protected set; } = 3;
    public override bool IsOn { get; protected set; } = false;
    public bool Full { get; protected set; } = false;

    public override void PickUp()
    {
        IsOn = !IsOn;
        //Debug.Log($"�絿�̴� ���� {(IsOn ? "����־��" : "�ٴ��̿���")}");
    }

    public void BucketisFull()
    {
        Full = !Full;
        //Debug.Log($"�絿�̴� ���� {(Full ? "�� á���" : "����־��")}");
    }
}
