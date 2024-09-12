using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PathFindingAgent))]
public class BehaviorTree : MonoBehaviour
{
    public enum ENodeStatus
    {
        Unknown,
        InProgress,
        Failed,
        Succeeded
    }

    public BTNodeBase RootNode { get; private set; } = new BTNodeBase("ROOT");
    void Start()
    {
        RootNode.Reset();
    }

    void Update()
    {
        RootNode.Tick(Time.deltaTime);
    }

    public string GetDebugText()
    {
        return RootNode.GetDebugText();
    }


}
