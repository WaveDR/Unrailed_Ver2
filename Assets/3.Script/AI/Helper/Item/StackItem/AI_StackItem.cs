using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AI_StackItem : MonoBehaviour
{
    [SerializeField] protected float stackInterval = 0.15f;
    [SerializeField] protected EItemType itemType;
    [SerializeField] protected LayerMask blockLayer;

    public bool HelperCheckItemType { get; protected set; } = false;
    public bool EnemyCheckItemType { get; protected set; } = false;

    protected BaseAI _ai;
    protected HelperBT _aiHelper;
    protected EnemyBT _aiEnemy;
    protected WorldResource _resource;
    protected AI_Stack _aiStack;

    protected virtual void Awake()
    {
        Init();
    }

    protected void Init()
    {
        if(_ai == null)
        {
            _aiHelper = FindObjectOfType<HelperBT>();
            _aiEnemy = FindObjectOfType<EnemyBT>();

            _aiStack = FindObjectOfType<AI_Stack>();
            _resource = GetComponent<WorldResource>();
        }
    }


    public EItemType ItemType => itemType;

    public abstract Pair<Stack<AI_StackItem>, Stack<AI_StackItem>> PickUp(Stack<AI_StackItem> handItem, Stack<AI_StackItem> detectedItem);  // 줍는 메소드
    public abstract Pair<Stack<AI_StackItem>, Stack<AI_StackItem>> PutDown(Stack<AI_StackItem> handItem, Stack<AI_StackItem> detectedItem);  // 버리는 메소드
    public abstract Pair<Stack<AI_StackItem>, Stack<AI_StackItem>> AutoGain(Stack<AI_StackItem> handItem, Stack<AI_StackItem> detectedItem);  // 자동으로 먹는 메소드

    public abstract Pair<Stack<AI_StackItem>, Stack<AI_StackItem>> EnemyThrowResource(Stack<AI_StackItem> handItem, Stack<AI_StackItem> detectedItem);  // 버리는 메소드
    public abstract Pair<Stack<AI_StackItem>, Stack<AI_StackItem>> EnemyPutDown(Stack<AI_StackItem> handItem, Stack<AI_StackItem> detectedItem);  // 버리는 메소드
    public abstract Pair<Stack<AI_StackItem>, Stack<AI_StackItem>> EnemyPickUp(Stack<AI_StackItem> handItem, Stack<AI_StackItem> detectedItem);  // 줍는 메소드
    public abstract Pair<Stack<AI_StackItem>, Stack<AI_StackItem>> EnemyAutoGain(Stack<AI_StackItem> handItem, Stack<AI_StackItem> detectedItem);  // 자동으로 먹는 메소드




    public virtual void RePosition(Transform parent, Vector3 pos)
    {
        transform.SetParent(parent);
        transform.localPosition = pos;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    public virtual void PutDownResource(Transform parent, Vector3 pos)
    {
        transform.SetParent(parent);
        transform.localPosition = pos;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }


    public virtual void ThrowResource()
    {
        transform.gameObject.AddComponent<Rigidbody>();
        transform.SetParent(null);
        Destroy(gameObject, 2);
    }

    public virtual bool CheckItemType(AI_StackItem item)
    {
        return itemType.Equals(item.ItemType);
    }

}
