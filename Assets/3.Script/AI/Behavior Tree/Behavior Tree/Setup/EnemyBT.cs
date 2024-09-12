using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBT : BaseAI
{
    public class BlackBoardKey : BlackboardKeyBase
    {
        public static readonly BlackBoardKey CurrentTarget = new BlackBoardKey() { Name = "CurrentTarget" };
        public static readonly BlackBoardKey HP = new BlackBoardKey() { Name = "HP" };

        public string Name;
    }

    private Blackboard<BlackBoardKey> _localMemory;
    private AnimalHealth _health;
    protected readonly int isRoot = Animator.StringToHash("isRoot");


    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _tree = GetComponent<BehaviorTree>();
        _agent = GetComponent<PathFindingAgent>();
        _stack = GetComponent<AI_Stack>();
        _health = GetComponent<AnimalHealth>();
    }

    private void Start()
    {
        _localMemory = BlackboardManager.Instance.GetIndividualBlackboard<BlackBoardKey>(this);
        _localMemory.SetGeneric<WorldResource>(BlackBoardKey.CurrentTarget, null);
        _localMemory.SetGeneric(BlackBoardKey.HP, _health.CurrentHp);
        _animator.SetBool(isMove, true);

        var BTRoot = _tree.RootNode.Add<BTNode_Selector>("BT 시작");

        BTRoot.AddService<BTServiceBase>("훔칠 거 찾는 Service", (float deltaTime) =>
        {
            Home.TargettoSteal(_agent);
        });

        var HaveTarget = BTRoot.Add<BTNode_Sequence>("훔칠 거 있나요?");
        HaveTarget.AddDecorator<BTDecoratorBase>("훔칠 거 있는지 거르는 DECO", () =>
        {
            //타겟이 있으면 true 없으면 false
            var hp = _localMemory.GetGeneric<int>(BlackBoardKey.HP);
            return hp == _health.CurrentHp && !Home.NonethisResourceTypeEnemy;
        });

        var MainSequence = HaveTarget.Add<BTNode_Sequence>("있는 경우");

        MainSequence.Add<BTNode_Action>("타겟 정하기", () =>
         {
             return BehaviorTree.ENodeStatus.InProgress;
        }
         , () =>
         {
            _target = Home.TargettoSteal(_agent);
             return _target == null ? BehaviorTree.ENodeStatus.InProgress : BehaviorTree.ENodeStatus.Succeeded;

         });


        MainSequence.Add<BTNode_Action>("이동하기",
        () =>
        {
            return _agent.MoveTo(_target.transform.position) ? BehaviorTree.ENodeStatus.InProgress : BehaviorTree.ENodeStatus.Failed;

        },
            () =>
            {
                return _agent.AtDestination ? BehaviorTree.ENodeStatus.Succeeded : BehaviorTree.ENodeStatus.InProgress;
            });

        MainSequence.Add<BTNode_Action>("훔치기",
        () =>
        {
            if(_target!=null)
            {
                _animator.SetBool(isMove, false);
                _animator.SetBool(isRoot, true);
                _stack.EnemyDetectGroundBlock(_target);
                //처음 드는 거 
                if (_stack.HandItem.Count == 0)
                {
                    _stack.EnemyInteractiveItem();
                }
                //그 후 쌓기
                else
                {
                    if (!_stack.HandItem.Peek().EnemyCheckItemType)
                    {
                        _stack.EnemyInteractiveAuto();

                    }
                }
                return BehaviorTree.ENodeStatus.InProgress;

            }
            return BehaviorTree.ENodeStatus.Failed;

        },
         () =>
         {
             if (_target != null)
             {
                 return BehaviorTree.ENodeStatus.Succeeded;

             }
             else
                 return BehaviorTree.ENodeStatus.InProgress;
         });

        MainSequence.Add<BTNode_Action>("쌓기",
            ()=>
            {
                return BehaviorTree.ENodeStatus.InProgress;

            },()=>
            {
                //자원이 더 이상 없다면 
                if (Home.NonethisResourceTypeEnemy || _stack.HandItem.Peek().EnemyCheckItemType)
                {
                    return BehaviorTree.ENodeStatus.Succeeded;
                }
                else
                //세 개 들었으면 옮기기
                {
                    return _stack.HandItem.Count == 3? BehaviorTree.ENodeStatus.Succeeded : BehaviorTree.ENodeStatus.Failed;
                }

            });

        MainSequence.Add<BTNode_Action>("도망",
        () =>
        {
            Vector3 pos = _agent.MoveToClosestEndPosition();
            _animator.SetBool(isMove, true);
            return BehaviorTree.ENodeStatus.InProgress;

        },
        () =>
        {
            return _agent.AtDestination ?
            BehaviorTree.ENodeStatus.Succeeded : BehaviorTree.ENodeStatus.InProgress;
        });


        //내려놓기

        MainSequence.Add<BTNode_Action>("버리기",
            () =>
            {
                SoundManager.Instance.PlaySoundEffect("Enemy_Laugh");
                _animator.SetBool(isRoot, false);
                _target = null;
                _stack.EnemyThrowResource();
                return BehaviorTree.ENodeStatus.InProgress;
            },
            () =>
            {
                return _stack.HandItem.Count ==0 ? BehaviorTree.ENodeStatus.Succeeded : BehaviorTree.ENodeStatus.InProgress;
            }
            );


        var Attacked = BTRoot.Add<BTNode_Sequence>("맞음");
        Attacked.Add<BTNode_Action>("공격 당함", () =>
         {
             var hp = _localMemory.GetGeneric<int>(BlackBoardKey.HP);
             if (_stack.HandItem.Count!=0&&hp!=_health.CurrentHp)
             {
                _currentblock = _stack.BFS(this);

                _animator.SetBool(isMove, true);
                _animator.SetBool(isRoot, false);
                _target = null;
                _stack.EnemyPutDown();
                _localMemory.SetGeneric(BlackBoardKey.HP, _health.CurrentHp);
                return BehaviorTree.ENodeStatus.InProgress;
             }
             return BehaviorTree.ENodeStatus.Failed;

         }, () =>
         {
             return _stack.HandItem.Count ==0 ? BehaviorTree.ENodeStatus.Failed : BehaviorTree.ENodeStatus.InProgress;
         });



        var WanderRoot = BTRoot.Add<BTNode_Sequence>("목표 못 찾음");
        WanderRoot.Add<BTNode_Action>("무작위 이동중",
        () =>
        {
            _animator.SetBool(isRoot, false);
            _animator.SetBool(isMove, true);
            _agent.MoveToRandomPosition();
            return BehaviorTree.ENodeStatus.InProgress;
        },
        () =>
        {
            return _agent.AtDestination ? BehaviorTree.ENodeStatus.Succeeded : BehaviorTree.ENodeStatus.InProgress;
        });

        var IdleRoot = MainSequence.Add<BTNode_Sequence>("위에거 다 실패 했을 경우");
        IdleRoot.Add<BTNode_Action>("가만히 있기", () =>
        {
            _animator.SetBool(isMove, false);
            return BehaviorTree.ENodeStatus.InProgress;
        });
    }
}
