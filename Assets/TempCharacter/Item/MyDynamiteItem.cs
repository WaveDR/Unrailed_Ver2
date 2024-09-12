using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyDynamiteItem : MyItem
{
    private BombTest _bomb;
    private bool isFire = false;

    protected override void Awake()
    {
        base.Awake();
        isFire = false;
        _bomb = GetComponent<BombTest>();
    }

    public override Pair<Stack<MyItem>, Stack<MyItem>> AutoGain(Stack<MyItem> handItem, Stack<MyItem> detectedItem)
    {
        return new Pair<Stack<MyItem>, Stack<MyItem>>(handItem, detectedItem);
    }

    public override Pair<Stack<MyItem>, Stack<MyItem>> Change(Stack<MyItem> handItem, Stack<MyItem> detectedItem)
    {
        if (!isFire)
            _bomb.Setup();

        return new Pair<Stack<MyItem>, Stack<MyItem>>(handItem, detectedItem);
    }

    public override Pair<Stack<MyItem>, Stack<MyItem>> PickUp(Stack<MyItem> handItem, Stack<MyItem> detectedItem)
    {
        // 弊成 林况
        handItem.Push(detectedItem.Pop());
        handItem.Peek().RePosition(handItem.Peek().equipment, Vector3.zero);
        return new Pair<Stack<MyItem>, Stack<MyItem>>(handItem, detectedItem);
    }

    public override Pair<Stack<MyItem>, Stack<MyItem>> PutDown(Stack<MyItem> handItem, Stack<MyItem> detectedItem)
    {
        if (!isFire)
            _bomb.Setup();

        // 弊成 冻迸
        detectedItem.Push(handItem.Pop());
        detectedItem.Peek().RePosition(player.AroundEmptyBlockTranform, Vector3.up * 0.5f);
        return new Pair<Stack<MyItem>, Stack<MyItem>>(handItem, detectedItem);
    }
}
