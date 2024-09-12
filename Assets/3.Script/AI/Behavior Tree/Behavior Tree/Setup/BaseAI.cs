using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BehaviorTree))]
public class BaseAI : MonoBehaviour
{
    [Header("Transform")]
    [SerializeField] protected Transform _rayStartTransform;
    [SerializeField] protected Transform _rightHandTransform;
    [SerializeField] protected Transform _twoHandTransform;

    protected AI_Stack _stack;

    public Transform RayStartTransfrom => _rayStartTransform;
    public Transform CurrentBlockTransform => _currentblock;
    public Transform RightHandTransform => _rightHandTransform;
    public Transform TwoHandTransform => _twoHandTransform;

    protected BehaviorTree _tree;
    protected PathFindingAgent _agent;
    protected Transform _currentblock;
    protected float _rotateSpeed = 10;


    protected WorldResource _target;

    protected Animator _animator;
    protected readonly int isMove = Animator.StringToHash("isMove");

    [HideInInspector]
    public Resource Home;
    public void SetHome(Resource _Home)
    {
        Home = _Home;
    }

}
