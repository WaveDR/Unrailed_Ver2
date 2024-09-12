using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Stack : MonoBehaviour
{
    public Stack<AI_StackItem> HandItem = new Stack<AI_StackItem>();
    public Stack<AI_StackItem> DetectedItem = new Stack<AI_StackItem>();
    [SerializeField] private LayerMask BlockLayer;

    private Transform _currentblock;


    private void Awake()
    {

        HandItem = new Stack<AI_StackItem>();
        DetectedItem = new Stack<AI_StackItem>();

    }

    //Helper
    public void InteractiveItem()
    {
        if (HandItem.Count == 0 && DetectedItem.Count != 0)  // 처음 줍기
        {

            Pair<Stack<AI_StackItem>, Stack<AI_StackItem>> p = DetectedItem.Peek().PickUp(HandItem, DetectedItem);
            HandItem = p.first;
            DetectedItem = p.second;
            Destroy(HandItem.Peek().GetComponent<WorldResource>());
        }
    }

    public void InteractiveItemAuto()
    {
        if (DetectedItem.Count != 0 && HandItem.Count != 0) //두 번째부터는 자동 줍기
        {
            Pair<Stack<AI_StackItem>, Stack<AI_StackItem>> p = HandItem.Peek().AutoGain(HandItem, DetectedItem);
            HandItem = p.first;
            DetectedItem = p.second;
            Destroy(HandItem.Peek().GetComponent<WorldResource>());
        }
    }

    public void PutDown()
    {
        if (HandItem.Count != 0 && DetectedItem.Count == 0) // 내려놓기
        {
            switch(HandItem.Peek().ItemType)
            {
                case EItemType.wood:
                    SoundManager.Instance.StopSoundEffect("Wood_Down");
                    SoundManager.Instance.PlaySoundEffect("Wood_Down");
                    break;

                case EItemType.steel:
                    SoundManager.Instance.StopSoundEffect("Steel_Down");
                    SoundManager.Instance.PlaySoundEffect("Steel_Down");
                    break;
            }

            Pair<Stack<AI_StackItem>, Stack<AI_StackItem>> p = HandItem.Peek().PutDown(HandItem, DetectedItem);
            HandItem = p.first;
            DetectedItem = p.second;

            DetectedItem.Clear();
        }

    }

    //Enemy
    public void EnemyThrowResource()
    {
        if (HandItem.Count != 0 && DetectedItem.Count == 0) // 버리기
        {

            Pair<Stack<AI_StackItem>, Stack<AI_StackItem>> p = HandItem.Peek().EnemyThrowResource(HandItem, DetectedItem);
            HandItem = p.first;
            DetectedItem = p.second;

            DetectedItem.Clear();
            HandItem.Clear();
        }

    }


    public void EnemyPutDown()
    {
        if (HandItem.Count != 0 && DetectedItem.Count == 0) // 내려놓기
        {
            Pair<Stack<AI_StackItem>, Stack<AI_StackItem>> p = HandItem.Peek().EnemyPutDown(HandItem, DetectedItem);
            HandItem = p.first;
            DetectedItem = p.second;

            DetectedItem.Clear();
        }

    }



    public void EnemyInteractiveItem()
    {
        if (HandItem.Count == 0 && DetectedItem.Count != 0)  // 처음 줍기
        {
            Pair<Stack<AI_StackItem>, Stack<AI_StackItem>> p = DetectedItem.Peek().EnemyPickUp(HandItem, DetectedItem);
            HandItem = p.first;
            DetectedItem = p.second;
            Destroy(HandItem.Peek().GetComponent<WorldResource>());

        }
    }

    public void EnemyInteractiveAuto()
    {
        if (DetectedItem.Count != 0 && HandItem.Count != 0) //두 번째부터는 자동 줍기
        {
            Pair<Stack<AI_StackItem>, Stack<AI_StackItem>> p = HandItem.Peek().EnemyAutoGain(HandItem, DetectedItem);
            HandItem = p.first;
            DetectedItem = p.second;
            Destroy(HandItem.Peek().GetComponent<WorldResource>());
        }
    }



    public void EnemyDetectGroundBlock(WorldResource resource)
    {
        DetectedItem.Push(resource.Stack);
    }


    public void DetectGroundBlock(WorldResource resource)
    {
        DetectedItem.Push(resource.Stack);
        //Destroy(resource);
    }


    public Transform BFS(BaseAI _ai)
    {
        if (Physics.Raycast(_ai.RayStartTransfrom.position, Vector3.down, out RaycastHit hit, 1, BlockLayer))
        {
            _currentblock = hit.transform;
        }

        Queue<Transform> queue = new Queue<Transform>();
        HashSet<Transform> hashSet = new HashSet<Transform>();

        if(_currentblock!=null)
        {
            queue.Enqueue(_currentblock);
            hashSet.Add(_currentblock);
        }

        Vector3[] dir = new Vector3[8] { Vector3.forward, Vector3.back, Vector3.right, Vector3.left,
        new Vector3(1, 0, 1), new Vector3(1, 0, -1), new Vector3(-1, 0, -1), new Vector3(-1, 0, 1)};

        while (queue.Count != 0)
        {
            Transform currentBlock = queue.Dequeue();

            if (currentBlock.childCount == 0)
                return currentBlock;

            for (int i = 0; i < 8; i++)
            {
                if (Physics.Raycast(currentBlock.position, dir[i], out RaycastHit hot, 1f, BlockLayer))
                {
                    if (hashSet.Add(hot.transform))
                    {
                        queue.Enqueue(hot.transform);
                    }
                }
            }
        }

        return null;
    }
}

