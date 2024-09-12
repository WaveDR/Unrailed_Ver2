using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyTrainItem : MyItem
{
    public override Pair<Stack<MyItem>, Stack<MyItem>> AutoGain(Stack<MyItem> handItem, Stack<MyItem> detectedItem)
    {
        return new Pair<Stack<MyItem>, Stack<MyItem>>(handItem, detectedItem);
    }

    public override Pair<Stack<MyItem>, Stack<MyItem>> Change(Stack<MyItem> handItem, Stack<MyItem> detectedItem)
    {
        return new Pair<Stack<MyItem>, Stack<MyItem>>(handItem, detectedItem);
    }

    public override Pair<Stack<MyItem>, Stack<MyItem>> PickUp(Stack<MyItem> handItem, Stack<MyItem> detectedItem)
    {
        // 그냥 주워
        handItem.Push(detectedItem.Pop());
        handItem.Peek().RePosition(handItem.Peek().equipment, Vector3.zero);
        return new Pair<Stack<MyItem>, Stack<MyItem>>(handItem, detectedItem);
    }

    public override Pair<Stack<MyItem>, Stack<MyItem>> PutDown(Stack<MyItem> handItem, Stack<MyItem> detectedItem)
    {
        return new Pair<Stack<MyItem>, Stack<MyItem>>(handItem, detectedItem);
    }
}
