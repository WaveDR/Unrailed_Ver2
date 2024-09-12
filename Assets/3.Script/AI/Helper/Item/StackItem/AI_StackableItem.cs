using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_StackableItem : AI_StackItem
{
    public override Pair<Stack<AI_StackItem>, Stack<AI_StackItem>> AutoGain(Stack<AI_StackItem> handItem, Stack<AI_StackItem> detectedItem)
    {
        Init();

        // 종류가 같다면 줍는다.
        if (handItem.Peek().CheckItemType(detectedItem.Peek()))
        {
            HelperCheckItemType = false;
            while (handItem.Count < 3)
            {
                if (detectedItem.Count == 0)
                    break;

                handItem.Push(detectedItem.Pop());
                handItem.Peek().RePosition(_aiHelper.TwoHandTransform, Vector3.up * (handItem.Count) * stackInterval);
            }
        }

        else
        {
            //아니라면 손에 있는걸 갖다놓고 다시 와야함
            detectedItem.Clear();
            HelperCheckItemType = true;
        }

        return new Pair<Stack<AI_StackItem>, Stack<AI_StackItem>>(handItem, detectedItem);
    }

    public override Pair<Stack<AI_StackItem>, Stack<AI_StackItem>> PickUp(Stack<AI_StackItem> handItem, Stack<AI_StackItem> detectedItem)
    {
        Init();

        for (int i = 0; i < 3; i++)
        {
            if (detectedItem.Count == 0)
                break;
            handItem.Push(detectedItem.Pop());
            handItem.Peek().RePosition(_aiHelper.TwoHandTransform, Vector3.up * stackInterval);
        }

        return new Pair<Stack<AI_StackItem>, Stack<AI_StackItem>>(handItem, detectedItem);
    }

    public override Pair<Stack<AI_StackItem>, Stack<AI_StackItem>> PutDown(Stack<AI_StackItem> handItem, Stack<AI_StackItem> detectedItem)
    {
        Init();

        while (handItem.Count != 0)
        {
            detectedItem.Push(handItem.Pop());
            detectedItem.Peek().PutDownResource(_aiHelper.CurrentBlockTransform, Vector3.up * 0.5f + Vector3.up * stackInterval*handItem.Count);
        }


        return new Pair<Stack<AI_StackItem>, Stack<AI_StackItem>>(handItem, detectedItem);
    }

    public override Pair<Stack<AI_StackItem>, Stack<AI_StackItem>> EnemyThrowResource(Stack<AI_StackItem> handItem, Stack<AI_StackItem> detectedItem)
    {
        Init();

        while (handItem.Count != 0)
        {
            detectedItem.Push(handItem.Pop());
            detectedItem.Peek().ThrowResource();
        }


        return new Pair<Stack<AI_StackItem>, Stack<AI_StackItem>>(handItem, detectedItem);
    }

    public override Pair<Stack<AI_StackItem>, Stack<AI_StackItem>> EnemyPickUp(Stack<AI_StackItem> handItem, Stack<AI_StackItem> detectedItem)
    {
        Init();

        for (int i = 0; i < 3; i++)
        {
            if (detectedItem.Count == 0)
                break;
            handItem.Push(detectedItem.Pop());
            handItem.Peek().RePosition(_aiEnemy.TwoHandTransform, Vector3.up * stackInterval);
        }

        return new Pair<Stack<AI_StackItem>, Stack<AI_StackItem>>(handItem, detectedItem);
    }

    public override Pair<Stack<AI_StackItem>, Stack<AI_StackItem>> EnemyAutoGain(Stack<AI_StackItem> handItem, Stack<AI_StackItem> detectedItem)
    {
        Init();

        // 종류가 같다면 줍는다.
        if (handItem.Peek().CheckItemType(detectedItem.Peek()))
        {
            EnemyCheckItemType = false;
            while (handItem.Count < 3)
            {
                if (detectedItem.Count == 0)
                    break;

                handItem.Push(detectedItem.Pop());
                handItem.Peek().RePosition(_aiEnemy.TwoHandTransform, Vector3.up * (handItem.Count) * stackInterval);
            }
        }

        else
        {
            detectedItem.Clear();
            EnemyCheckItemType = true;
        }

        return new Pair<Stack<AI_StackItem>, Stack<AI_StackItem>>(handItem, detectedItem);

    }

    public override Pair<Stack<AI_StackItem>, Stack<AI_StackItem>> EnemyPutDown(Stack<AI_StackItem> handItem, Stack<AI_StackItem> detectedItem)
    {
        Init();

        while (handItem.Count != 0)
        {
            detectedItem.Push(handItem.Pop());
            detectedItem.Peek().PutDownResource(_aiEnemy.CurrentBlockTransform, Vector3.up * 0.5f + Vector3.up * stackInterval * handItem.Count);
        }


        return new Pair<Stack<AI_StackItem>, Stack<AI_StackItem>>(handItem, detectedItem);
    }
}
