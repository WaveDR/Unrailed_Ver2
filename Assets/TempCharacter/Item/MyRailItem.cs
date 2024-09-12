using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyRailItem : MyItem
{
    private RailController railController;
    protected override void Awake()
    {
        base.Awake();
        railController = GetComponent<RailController>();
    }

    public override Pair<Stack<MyItem>, Stack<MyItem>> AutoGain(Stack<MyItem> handItem, Stack<MyItem> detectedItem)
    {
        // 종류가 같다면 줍는다.
        if (handItem.Peek().CheckItemType(detectedItem.Peek()))
        {
            if (IsRailInteractive(detectedItem.Peek().GetComponent<RailController>()))
            {
                while (handItem.Count < 3)
                {
                    if (detectedItem.Count == 0)
                        break;

                    handItem.Push(detectedItem.Pop());
                    handItem.Peek().RePosition(handItem.Peek().equipment, Vector3.up * (handItem.Count - 1) * stackInterval);
                }
            }
        }

        return new Pair<Stack<MyItem>, Stack<MyItem>>(handItem, detectedItem);
    }

    public override Pair<Stack<MyItem>, Stack<MyItem>> Change(Stack<MyItem> handItem, Stack<MyItem> detectedItem)
    {
        // handItem은 무조건 railItem

        EItemType detectedItemType = detectedItem.Peek().ItemType;
        if (detectedItemType == EItemType.axe || detectedItemType == EItemType.pick || detectedItemType == EItemType.bucket)
        {
            MyItem temp = detectedItem.Pop();
            while (handItem.Count != 0)
            {
                // 하나를 놓는데 연결할 수 있는 곳이라면 하나만 놓음
                detectedItem.Push(handItem.Pop());
                detectedItem.Peek().RePosition(player.CurrentBlockTransform, Vector3.up * 0.5f + Vector3.up * (detectedItem.Count - 1) * stackInterval);

                if (IsInstallable())
                    break;
            }

            // 손에 남았으면 빌 때까지 다른데에 놓음
            Transform aroundTransform = player.AroundEmptyBlockTranform;
            int count = 0;
            while(handItem.Count != 0)
            {
                handItem.Pop().RePosition(aroundTransform, Vector3.up * 0.5f + Vector3.up * count++ * stackInterval);
            }

            // 그리고 도구를 집는다.
            handItem.Push(temp);
            handItem.Peek().RePosition(handItem.Peek().equipment, Vector3.zero);
        }
        else if (detectedItemType == EItemType.wood || detectedItemType == EItemType.steel)
        {
            Stack<MyItem> temp = new Stack<MyItem>(handItem);
            handItem.Clear();

            // 한계까지 든다
            while (handItem.Count < 3)
            {
                if (detectedItem.Count == 0)
                    break;
                handItem.Push(detectedItem.Pop());
                handItem.Peek().RePosition(handItem.Peek().equipment, Vector3.up * (handItem.Count - 1) * stackInterval);
            }

            if (detectedItem.Count == 0)
            {
                while (temp.Count != 0)
                {
                    detectedItem.Push(temp.Pop());
                    detectedItem.Peek().RePosition(player.CurrentBlockTransform, Vector3.up * 0.5f + Vector3.up * (detectedItem.Count - 1) * stackInterval);
                }
            }
            else
            {
                Transform aroundTransform = player.AroundEmptyBlockTranform;
                int count = 0;
                while (temp.Count != 0)
                {
                    temp.Pop().RePosition(aroundTransform, Vector3.up * 0.5f + Vector3.up * count++ * stackInterval);
                }
            }
        }
        else if (detectedItemType == EItemType.rail)
        {
            if (IsRailInteractive(detectedItem.Peek().GetComponent<RailController>()))
            {
                while (handItem.Count != 0)
                {
                    detectedItem.Push(handItem.Pop());
                    detectedItem.Peek().RePosition(player.CurrentBlockTransform, Vector3.up * 0.5f + Vector3.up * (detectedItem.Count - 1) * stackInterval);
                }
            }
        }

        return new Pair<Stack<MyItem>, Stack<MyItem>>(handItem, detectedItem);
    }
    public override Pair<Stack<MyItem>, Stack<MyItem>> PickUp(Stack<MyItem> handItem, Stack<MyItem> detectedItem)
    {
        RailController detectedRail = detectedItem.Peek().GetComponent<RailController>();

        // 설치된 레일이 아니라면
        if (!IsRailInstance(detectedRail))
        {
            // 마지막 레일이라면 꺼내는 메소드 호출
            // 마지막 레일이라면 어차피 하나밖에 없으니까 기본 줍는 로직 써도 됨
            if (detectedRail.Equals(FindObjectOfType<GoalManager>().lastRail))
                detectedRail.PickUpRail();

            for (int i = 0; i < 3; i++)
            {
                if (detectedItem.Count == 0)
                    break;
                handItem.Push(detectedItem.Pop());
                handItem.Peek().RePosition(handItem.Peek().equipment, Vector3.up * (handItem.Count - 1) * stackInterval);
            }
        }

        return new Pair<Stack<MyItem>, Stack<MyItem>>(handItem, detectedItem);
    }


    public override Pair<Stack<MyItem>, Stack<MyItem>> PutDown(Stack<MyItem> handItem, Stack<MyItem> detectedItem)
    {
        // 내려놓기
        // 설치할 수 있는 곳이면 하나만 떨구고 설치
        // 아니면 그냥 다 떨굼
        if(IsInstallable())
        {
            detectedItem.Push(handItem.Pop());
            detectedItem.Peek().RePosition(player.CurrentBlockTransform, Vector3.up * 0.5f + Vector3.up * (detectedItem.Count - 1) * stackInterval);

            // 여기서 내려놓음
            detectedItem.Peek().GetComponent<RailController>().PutRail();
        }
        else
        {
            Transform aroundTransform = player.AroundEmptyBlockTranform;
            while (handItem.Count != 0)
            {
                detectedItem.Push(handItem.Pop());
                detectedItem.Peek().RePosition(aroundTransform, Vector3.up * 0.5f + Vector3.up * (detectedItem.Count - 1) * stackInterval);
            }
        }
        return new Pair<Stack<MyItem>, Stack<MyItem>>(handItem, detectedItem);
    }
}
