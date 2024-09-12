using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FlockBT : MonoBehaviour
{
    public class BlackBoardKey : BlackboardKeyBase
    {
        public static readonly BlackBoardKey Separation = new BlackBoardKey() { Name = "Separation" };
        public static readonly BlackBoardKey Alignment = new BlackBoardKey() { Name = "Alignment" };

        public string Name;
    }

    [Header("랜덤 이동 거리")]
    [SerializeField] private float _wanderRange = 3f;


    private BehaviorTree _tree;
    private PathFindingAgent _agent;
    private Blackboard<BlackBoardKey> _localMemory;

    private Flock _flock;

    private void Awake()
    {
        _tree = GetComponent<BehaviorTree>();
        _agent = GetComponent<PathFindingAgent>();
        _flock = GetComponent<Flock>();
    }

    private void Start()
    {
        //Boids

        _localMemory = BlackboardManager.Instance.GetIndividualBlackboard<BlackBoardKey>(this);
        _localMemory.SetGeneric<Vector3>(BlackBoardKey.Separation, Vector3.zero);
        _localMemory.SetGeneric<Vector3>(BlackBoardKey.Alignment, Vector3.zero);

        var BTRoot = _tree.RootNode.Add<BTNode_Selector>("BT START");
        var isHeadingCollison = BTRoot.Add<BTNode_Sequence>("충돌 가능성 검사");
        isHeadingCollison.AddDecorator<BTDecoratorBase>("충돌 가능성이 있나요?", () =>
         {
             return !_flock.IsHeadingForCollision();
         });

        //충돌 가능성이 없다면
        var wanderRoot = isHeadingCollison.Add <BTNode_Sequence>("충돌 가능성 없음");
        var wander = wanderRoot.Add<BTNode_Action>("돌아다니기",
            () =>
            {
                _agent.MoveToRandomPosition();
                return BehaviorTree.ENodeStatus.InProgress;
            }, () =>
            {
                return _agent.AtDestination ?
                BehaviorTree.ENodeStatus.Succeeded : BehaviorTree.ENodeStatus.InProgress;
            }
            );


        //무리 가운데로 모이기(Cohesion)
        var cohesionRoot = wanderRoot.Add<BTNode_Sequence>("Cohesion");
        var cohesion = cohesionRoot.Add<BTNode_Action>("무리 가운데로 모이기",
        () =>
        {
            //Cohesion
            _agent.MoveTo(_flock.Center);
            return BehaviorTree.ENodeStatus.InProgress;
        }, () =>
        {
            return _agent.AtDestination ?
            BehaviorTree.ENodeStatus.Succeeded : BehaviorTree.ENodeStatus.InProgress;

        });

        //무리의 방향을 따라가기(Alignment)
        var AlignmentRoot = wanderRoot.Add<BTNode_Sequence>("Alignment");
        AlignmentRoot.Add<BTNode_Action>("무리의 방향을 따라가기",
        () =>
        {
            //Alignment
            _agent.MoveTo(_flock.AlignmentPosition);
            return BehaviorTree.ENodeStatus.InProgress;
        }, () =>
        {
            return _agent.AtDestination ?
            BehaviorTree.ENodeStatus.Succeeded : BehaviorTree.ENodeStatus.InProgress;

        });

        //충돌 가능성이 있다면
        //자기들끼리는 피하기(Separation)
        var SeparationRoot = BTRoot.Add<BTNode_Sequence>("Separation");
        SeparationRoot.Add<BTNode_Action>("무리와 충돌하지 않기",
        () =>
        {
            //Separation
            _agent.MoveTo(_flock.ObstacleRays());
            return BehaviorTree.ENodeStatus.InProgress;
        }, () =>
        {
            return _agent.AtDestination ?
            BehaviorTree.ENodeStatus.Succeeded : BehaviorTree.ENodeStatus.InProgress;

        });

        var CollisionRoot = SeparationRoot.Add<BTNode_Sequence>("자기들끼리 박았을 때 ");
        CollisionRoot.Add<BTNode_Action>("아무데나 가세용", 
        () =>
        {
             _agent.MoveToRandomPosition();
             return BehaviorTree.ENodeStatus.InProgress;
        }, () =>
        {
             return _agent.AtDestination ?
             BehaviorTree.ENodeStatus.Succeeded : BehaviorTree.ENodeStatus.InProgress;

        }
        );




    }


}


