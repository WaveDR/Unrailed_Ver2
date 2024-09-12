using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BoxItem
{
    public BoxItem(EItemType type) { itemType = type; }
    public EItemType itemType;
}

public class TrainBox : TrainMovement
{
    [SerializeField] private Transform woodPos;
    [SerializeField] private Transform steelPos;

    public List<BoxItem> woods = new List<BoxItem>();
    public List<BoxItem> steels = new List<BoxItem>();

    public Stack<MyItem> woodStack = new Stack<MyItem>();
    public Stack<MyItem> steelStack = new Stack<MyItem>();

    [SerializeField] private TrainWorkBench workBench;
    public Player player;

    public int maxItem;
    public bool madeReady;
    private bool tutorialDone;
    // Start is called before the first frame update
    void Awake()
    {
        GetMesh();
        trainUpgradeLevel = 1;
        workBench = FindObjectOfType<TrainWorkBench>();
        fireEffect.gameObject.SetActive(false);
        TrainUpgrade();

        StartCoroutine(Warning()); //첫 목재 석재 넣어라
    }

    // Update is called once per frame
    void Update()
    {
        TrainMovePos();
 
    }
    public override void TrainUpgrade()
    {
        base.TrainUpgrade();
        //업그레이드 메서드
        switch (trainUpgradeLevel)
        {
            case 1:
                maxItem = 3;
                break;
            case 2:
                maxItem = 4;

                break;
            default:
                maxItem = 6;
                break;
        }
    }
    private void RelayItems()
    {
        workBench.MakingRail();
    }
/*    public void GiveMeItem(EItemType itemtype, Stack<MyItem> itemCount)
    {
        switch (itemtype)
        {
            case EItemType.wood:
                woods.Add(new BoxItem(itemtype));
                itemCount.Peek();
                break;

            case EItemType.steel:
                steels.Add(new BoxItem(itemtype));
                itemCount.Peek();
                break;
        }
    }*/

    public Stack<MyItem> GiveMeItem(Stack<MyItem> handItem)
    {

        if (!tutorialDone)
        {
            warningIcon.SetActive(false);
            tutorialDone = true;
        }
        MyItem item;

        switch (handItem.Peek().ItemType)
        {
            case EItemType.wood:
                item = handItem.Pop();
                item.transform.SetParent(woodPos);
                item.transform.localPosition = Vector3.up * woodStack.Count * 0.15f;
                item.transform.localRotation = Quaternion.identity;
                // woods.Add(new BoxItem(item.ItemType));
                woodStack.Push(item);
                break;

            case EItemType.steel:
                item = handItem.Pop();
                item.transform.SetParent(steelPos);
                item.transform.localPosition = Vector3.up * steelStack.Count * 0.15f;
                item.transform.localRotation = Quaternion.identity;
                //steels.Add(new BoxItem(item.ItemType));
                steelStack.Push(item);
                break;
        }
        
        return handItem;
    }
}
