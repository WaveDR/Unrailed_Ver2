using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class HelperBT : BaseAI
{
    public class BlackBoardKey : BlackboardKeyBase
    {
        public static readonly BlackBoardKey Order = new BlackBoardKey() { Name = "Order" };
        public static readonly BlackBoardKey Destination = new BlackBoardKey() { Name = "Destination" };
        public static readonly BlackBoardKey Shopping = new BlackBoardKey() { Name = "Shopping" };
        public static readonly BlackBoardKey ResourceType = new BlackBoardKey() { Name = "ResourceType" };
        public static readonly BlackBoardKey Arrive = new BlackBoardKey() { Name = "Arrive" };
        public static readonly BlackBoardKey Home = new BlackBoardKey() { Name = "Home" };
        public static readonly BlackBoardKey Item = new BlackBoardKey() { Name = "Item" };

        public string Name;

    }

    private Blackboard<BlackBoardKey> _localMemory;


    private Helper _helper;
    //도구
    private AI_Item _item;
    //명령
    private WorldResource.EType _order;

    public bool _arrive = false;

    public float currentTime = 0;

    //이모티콘
    private EmoteManager _emoteManager;
    [SerializeField] private Image _emoteImage;


    private TrainMovement _engine;
    private float _defaultSpeed;


    private readonly int isDig = Animator.StringToHash("isDig");

    private void Awake()
    {
        _emoteManager = FindObjectOfType<EmoteManager>();
        _stack = GetComponent<AI_Stack>();
        _helper = GetComponent<Helper>();
        _animator = GetComponent<Animator>();
        _tree = GetComponent<BehaviorTree>();
        _agent = GetComponent<PathFindingAgent>();
        _defaultSpeed = _agent.moveSpeed;
    }

    private void Start()
    {
        _engine = FindObjectOfType<TrainMovement>();
        _localMemory = BlackboardManager.Instance.GetIndividualBlackboard<BlackBoardKey>(this);
        _localMemory.SetGeneric<WorldResource.EType>(BlackBoardKey.Order, WorldResource.EType.Wood);
        _localMemory.SetGeneric<bool>(BlackBoardKey.Arrive, _arrive);

        Vector3 home = _agent.FindCloestAroundEndPosition(GoalManager.Instance.lastRail.transform.position);
        _localMemory.SetGeneric<Vector3>(BlackBoardKey.Home, home);

        var BTRoot = _tree.RootNode.Add<BTNode_Selector>("BT 시작");
        BTRoot.AddService<BTServiceBase>("명령을 기다리는 Service", (float deltaTime) =>
        {
            // 전
            var order = _localMemory.GetGeneric<WorldResource.EType>(BlackBoardKey.Order);
            _order = _helper.TargetResource;

        });

        var OrderRoot = BTRoot.Add<BTNode_Sequence>("명령이 있나요?");
        var CheckOrder = OrderRoot.AddDecorator<BTDecoratorBase>("명령이 바뀌었는지 확인", () =>
         {
             var order = _localMemory.GetGeneric<WorldResource.EType>(BlackBoardKey.Order);
             var arrive = _localMemory.GetGeneric<bool>(BlackBoardKey.Arrive);
             //명령이 바뀌었거나 도착했다면
             return order == _order && arrive == _helper.arrive;
         });

        var MainSequence = OrderRoot.Add<BTNode_Sequence>("명령이 있는 경우");
        #region 나무 캐기, 돌 캐기, 물 떠오기 명령


        #region 명령 내리기
        var FindTools = MainSequence.Add<BTNode_Sequence>("1. 도구 찾기");

        var MoveToItem = FindTools.Add<BTNode_Action>("도구 정하기, 이동하기", () =>
         {
             var order = _localMemory.GetGeneric<WorldResource.EType>(BlackBoardKey.Order);

             if(_item == null)
             {
                 foreach (var item in ItemManager.Instance.RegisteredObjects)
                 {
                     //아이템이 명령이랑 호환이 된다면
                     if (item.Type == order)
                     {
                         foreach (var interaction in item.Interactions)
                         {
                             //플레이어가 들고 있는지 확인하기
                             if (interaction.CanPerform())
                             {
                                 _localMemory.SetGeneric<AI_Item>(BlackBoardKey.Item, item);
                                 _emoteImage.sprite = _emoteManager.GetEmote(item.ID);
                                 _agent.MoveTo(item.InteractionPoint);
                                 _animator.SetBool(isMove, true);
                             }
                             else
                             {
                                 //누군가 사용중이거나
                                 //물이 떠져있는데 물을 또 떠오라하거나 등등
                                 _emoteImage.sprite = _emoteManager.GetEmote(_emoteManager.WarningEmote);
                                 Debug.Log($"{_item} : 사용할 수 없는 상황이에요.");
                                 return BehaviorTree.ENodeStatus.Failed;
                             }
                             break;
                         }
                     }
                 }
             }


            //도구로 이동하기
             return BehaviorTree.ENodeStatus.InProgress;

         }, () =>
         {
            return _agent.AtDestination ? BehaviorTree.ENodeStatus.Succeeded : BehaviorTree.ENodeStatus.InProgress;
         });

        var PickUpTool = FindTools.Add<BTNode_Action>("도구 들기", () =>
         {
             //도착
             var item = _localMemory.GetGeneric<AI_Item>(BlackBoardKey.Item);
             _item = item;

             if (_item != null)
             {
                 foreach (var interaction in item.Interactions)
                 {
                     //플레이어가 들고 있는지 확인하기
                     if (interaction.CanPerform())
                     {
                         item.PickUp();
                         SoundManager.Instance.PlaySoundEffect("Player_ToolsUp");
                         _emoteImage.sprite = _emoteManager.GetEmote(item.ID);
                         break;
                     }
                 }

                 switch (_item.Type)
                 {
                     //양동이면 양손
                     case WorldResource.EType.Water:
                         _item.transform.SetParent(TwoHandTransform);
                         break;
                     //곡괭이, 도끼면 한 손
                     default:
                         _item.transform.SetParent(RightHandTransform);
                         break;
                 }

                 _item.transform.localPosition = Vector3.zero;
                 _item.transform.localRotation = Quaternion.identity;

                 return BehaviorTree.ENodeStatus.InProgress;
             }
             else
                 return BehaviorTree.ENodeStatus.Succeeded;
         },
        () =>
        {
            return BehaviorTree.ENodeStatus.Succeeded;
        });


        var WorkRoot = MainSequence.Add<BTNode_Sequence>("2. 일하기", () =>
         {
            //타겟 자원 설정
            return BehaviorTree.ENodeStatus.InProgress;

         }, () =>
         {
             var order = _localMemory.GetGeneric<WorldResource.EType>(BlackBoardKey.Order);
             return order == _order ? BehaviorTree.ENodeStatus.InProgress : BehaviorTree.ENodeStatus.Failed;
         });

        var Target = WorkRoot.Add<BTNode_Action>("타겟 정하기", () =>
         {
             if (_target == null)
             {
                 //목표 자원
                 _target = Home.GetGatherTarget(_helper);

                 if (_target != null)       
                 {
                     _emoteImage.sprite = _emoteManager.GetEmote((int)_target.Type+10);
                     Vector3 position = _agent.FindCloestAroundEndPosition(_target.transform.position);
                     _localMemory.SetGeneric<Vector3>(BlackBoardKey.Destination, position);
                     return BehaviorTree.ENodeStatus.Succeeded;

                 }

             }
             return BehaviorTree.ENodeStatus.InProgress;

         });

        var PossibleToWork = WorkRoot.Add<BTNode_Selector>("일하기 셀렉터");
        var PossibleSequence = PossibleToWork.Add<BTNode_Sequence>("가능", () =>
         {
             if(_target!=null)
             {
                 Vector3 pos = _agent.FindCloestAroundEndPosition(_target.transform.position);
                 return _agent.MoveTo(pos) ? BehaviorTree.ENodeStatus.InProgress : BehaviorTree.ENodeStatus.Failed;
             }
             return BehaviorTree.ENodeStatus.InProgress;
         }
        );
        var MoveToResource = PossibleSequence.Add<BTNode_Action>("자원으로 이동", () =>
         {
             //자원으로 이동하기
             _animator.SetBool(isMove, true);
             return BehaviorTree.ENodeStatus.InProgress;

         }
        , () =>
         {
             return _agent.AtDestination ? BehaviorTree.ENodeStatus.Succeeded : BehaviorTree.ENodeStatus.InProgress;

         });

        var ImpossibleToWork = PossibleToWork.Add<BTNode_Sequence>("불가능", () =>
        {
            //자원이 물 건너에 있을 때
            if (_target != null)
            {
                Vector3 pos = _agent.FindCloestAroundEndPosition(_target.transform.position);
                return _agent.MoveTo(pos) ? BehaviorTree.ENodeStatus.Failed : BehaviorTree.ENodeStatus.InProgress;
            }
            else return BehaviorTree.ENodeStatus.InProgress;
        });

        ImpossibleToWork.Add<BTNode_Action>("타겟이 갈 수 없는 곳에 있어요", () =>
         {
             _target = Home.ResearchTarget(_helper);
             return BehaviorTree.ENodeStatus.Succeeded;
         });


        ImpossibleToWork.Add<BTNode_Action>("타겟이 갈 수 없는 곳에 있어요", () =>
        {
            if (_target != null)
            {
                  Vector3 position = _agent.FindCloestAroundEndPosition(_target.transform.position);
                 _agent.MoveTo(position);

            return BehaviorTree.ENodeStatus.InProgress;
            }

            return BehaviorTree.ENodeStatus.Failed;
        },
         () =>
         {
             return _agent.AtDestination ? BehaviorTree.ENodeStatus.Succeeded : BehaviorTree.ENodeStatus.InProgress;
         });



        #endregion

        #region 나무, 돌
        var CheckTargetType = WorkRoot.Add<BTNode_Selector>("자원 종류에 따라 다른 행동하기");

        var WoodOrStone = CheckTargetType.Add<BTNode_Sequence>("[나무, 돌]", () =>
         {

             if (_target != null)
             {
                 return _target.Type == WorldResource.EType.Wood || _target.Type == WorldResource.EType.Stone
                    ? BehaviorTree.ENodeStatus.InProgress : BehaviorTree.ENodeStatus.Failed;
             }

             else return BehaviorTree.ENodeStatus.Failed;

         });

        var CollectResource = WoodOrStone.Add<BTNode_Action>("계속 채집하기", () =>
         {
             _animator.SetBool(isDig, true);
             StartCoroutine(_target.isDigCo());

             return BehaviorTree.ENodeStatus.InProgress;

         }, () =>

         {
             if(_target==null)
             {
                 return BehaviorTree.ENodeStatus.Failed;
             }
             else
             {
                 Vector3 dir = _target.transform.position - transform.position;
                 transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * _rotateSpeed);
             }

             if (!_target.isDig())
             {
                 if (_target.Type == WorldResource.EType.Wood) SoundManager.Instance.PlaySoundEffect("Wood_Broken");
                 if (_target.Type == WorldResource.EType.Stone) SoundManager.Instance.PlaySoundEffect("Steel_Broken");
                 Destroy(_target.gameObject);
                 _animator.SetBool(isDig, false);
                 return BehaviorTree.ENodeStatus.Succeeded;
             }

             return BehaviorTree.ENodeStatus.InProgress;
         }
        );
        #endregion

        #region 물, 자원
        // 물 ==========================================================================
        var WaterOrResource = CheckTargetType.Add<BTNode_Sequence>("[물, 자원]", () =>
         {
             if (_target != null)
             {
                 return _target.Type == WorldResource.EType.Water || _target.Type == WorldResource.EType.Resource
                 ? BehaviorTree.ENodeStatus.InProgress : BehaviorTree.ENodeStatus.Failed;
             }

             else return BehaviorTree.ENodeStatus.Failed;
         });

        var Interaction = WaterOrResource.Add<BTNode_Action>("물 채우기 / 자원 들기", () =>
         {
             switch (_target.Type)
             {
                 case WorldResource.EType.Water:
                     SoundManager.Instance.PlaySoundEffect("Player_WaterImport");
                     break;

                 case WorldResource.EType.Resource:
                     _stack.DetectGroundBlock(_target);
                     if (_stack.HandItem.Count == 0)
                     {
                         SoundManager.Instance.PlaySoundEffect("Item_Up");
                         _stack.InteractiveItem();
                     }
                     //그 후 쌓기
                     else
                     {
                         if(!_stack.HandItem.Peek().HelperCheckItemType)
                         {
                             SoundManager.Instance.PlaySoundEffect("Item_Up");
                             _stack.InteractiveItemAuto();
                         }

                     }
                     //처음 드는 거 

                     break;
             }


             return BehaviorTree.ENodeStatus.InProgress;

       }
        , () =>
         {
             switch (_target.Type)
             {
                 case WorldResource.EType.Water:
                    //아이템 상호작용 중 가능한 상호작용 찾기
                    //물이라면 물 뜨기
                    foreach (var interaction in _item.Interactions)
                     {
                         if (interaction.CanPerform())
                         {
                             if (interaction.Perform())
                             {
                                 _animator.SetBool(isMove, false);
                                 return BehaviorTree.ENodeStatus.InProgress;
                             }
                             else
                                 return BehaviorTree.ENodeStatus.Succeeded;
                         }
                     }
                     break;

                 case WorldResource.EType.Resource:
                     return BehaviorTree.ENodeStatus.Succeeded;

             }


             return BehaviorTree.ENodeStatus.InProgress;

         }
        );

        var MoveToHome = WaterOrResource.Add<BTNode_Action>("물 갖다놓기 / 다음 자원으로 이동하기", () =>
         {
             switch (_target.Type)
             {
                 case WorldResource.EType.Water:
                     _animator.SetBool(isMove, true);
                     Vector3 home = _localMemory.GetGeneric<Vector3>(BlackBoardKey.Home);
                     _agent.MoveTo(home);
                     break;

                 case WorldResource.EType.Resource:
                     break;
             }
             return BehaviorTree.ENodeStatus.InProgress;
             //나중에 기차 좌표로 바꾸기

         }, () =>
          {
              return _agent.AtDestination ? BehaviorTree.ENodeStatus.Succeeded : BehaviorTree.ENodeStatus.InProgress;
          }
         );

        var SleepRoot = WaterOrResource.Add<BTNode_Action>("양동이 내려놓기, 세 개 모으기", () =>
         {
             switch (_target.Type)
             {
                 case WorldResource.EType.Water:
                     PutDown();
                     _emoteImage.sprite = _emoteManager.GetEmote(_emoteManager.SleepEmote);
                     break;

                 case WorldResource.EType.Resource:
                     break;
             }

             return BehaviorTree.ENodeStatus.InProgress;
         }
        , () =>
         {
             switch (_target.Type)
             {
                 case WorldResource.EType.Resource:

                     Home.GetGatherTarget(_helper);
                     //자원이 더 이상 없다면 
                     if (Home.NonethisResourceTypeHelper||_stack.HandItem.Peek().HelperCheckItemType)
                     {
                         return BehaviorTree.ENodeStatus.Succeeded;
                     }
                     else
                     {
                         return _stack.HandItem.Count == 3 ? BehaviorTree.ENodeStatus.Succeeded : BehaviorTree.ENodeStatus.Failed;

                     }
                         //세 개 들었으면 옮기기



                 case WorldResource.EType.Water:
                     return BehaviorTree.ENodeStatus.InProgress;
             }
             return BehaviorTree.ENodeStatus.InProgress;

         });

        var CarryingResource = WaterOrResource.Add<BTNode_Action>("자원 운반하기", () =>
        {
            Vector3 home = _agent.FindCloestAroundEndPosition(GoalManager.Instance.lastRail.transform.position);
            _agent.MoveTo(home);
            return BehaviorTree.ENodeStatus.InProgress;
        },
        () =>
        {
            return _agent.AtDestination ? BehaviorTree.ENodeStatus.Succeeded : BehaviorTree.ENodeStatus.InProgress;
        }
        );

        var PutDownResource = WaterOrResource.Add<BTNode_Action>("자원 내려놓기", () =>
         {
             _currentblock = _stack.BFS(this);
             _stack.PutDown();

             _target = null;
             _currentblock = null;
            return BehaviorTree.ENodeStatus.InProgress;
         },
        () =>
        {
            Home.GetGatherTarget(_helper);
            //더 이상 채집할 자원이 없는경우
            if (Home.NonethisResourceTypeHelper)
            {
                _emoteImage.sprite = _emoteManager.GetEmote(_emoteManager.WarningEmote);
                _animator.SetBool(isMove, false);
                return BehaviorTree.ENodeStatus.InProgress;
            }

            else
            {
                var order = _localMemory.GetGeneric<WorldResource.EType>(BlackBoardKey.Order);
                if (order == _order)
                {
                    return _stack.HandItem.Count == 0 ? BehaviorTree.ENodeStatus.Succeeded : BehaviorTree.ENodeStatus.InProgress;
                }
                else
                    _localMemory.SetGeneric<WorldResource.EType>(BlackBoardKey.Order, _order);
                    return BehaviorTree.ENodeStatus.Succeeded;
            }
        }
        );

        #endregion
        #endregion


        #region 역으로 이동하기

        var GotoStation = BTRoot.Add<BTNode_Sequence>("도착한경우");
        GotoStation.Add<BTNode_Action>("역으로 이동하기", () =>
        {
            PutDown();
            if (_helper.arrive)
            {
                _emoteImage.sprite = _emoteManager.GetEmote(_emoteManager.HeartEmote);
                SoundManager.Instance.PlaySoundEffect("Player_Dash");
                Vector3 position = ShopManager.Instance.nextGame.position;
                _agent.MoveTo(position);
                _agent.moveSpeed = 10;
                _animator.SetBool(isMove, true);
                return BehaviorTree.ENodeStatus.InProgress;
            }

            return BehaviorTree.ENodeStatus.Failed;
        }, () =>
        {
            if(_helper.arrive)
            {
            return _agent.AtDestination ? BehaviorTree.ENodeStatus.Succeeded : BehaviorTree.ENodeStatus.InProgress;
            }
            return BehaviorTree.ENodeStatus.Failed;
        });

        GotoStation.Add<BTNode_Action>("밟고 있다면 가만히 있기", () =>
         {
             Reset();
             _animator.SetBool(isMove, false);
             _agent.moveSpeed = _defaultSpeed;
                 return BehaviorTree.ENodeStatus.InProgress;
         },() =>
         {

             return !_helper.GotoPlayer ? BehaviorTree.ENodeStatus.Succeeded : BehaviorTree.ENodeStatus.InProgress;
         });


        var NeedToMove = GotoStation.Add<BTNode_Sequence>("플레이어가 밟았는지 확인하기");
        NeedToMove.AddDecorator<BTDecoratorBase>("플레이어가 밟은걸 확인하는 데코레이터", () =>
        {
            return !_helper.GotoPlayer;

        });

        var Shopping = NeedToMove.Add<BTNode_Sequence>("안 밟았다면");
        Shopping.Add<BTNode_Action>("가만히 있기", () =>
         {
             _animator.SetBool(isMove, false);
             _agent.moveSpeed = _defaultSpeed;
             return BehaviorTree.ENodeStatus.InProgress;
         },()=>
         {
           return _agent.moveSpeed == _defaultSpeed ? BehaviorTree.ENodeStatus.Succeeded : BehaviorTree.ENodeStatus.InProgress;
         });

        Shopping.Add<BTNode_Action>("쇼핑 하기", () =>
        {
            _emoteImage.sprite = _emoteManager.GetEmote(_emoteManager.HmmEmote);
            Transform shopping = ShopManager.Instance.shopUpgradeTrainPos
                                 [UnityEngine.Random.Range(0, ShopManager.Instance.shopUpgradeTrainPos.Length)];
            _localMemory.SetGeneric<Transform>(BlackBoardKey.Shopping, shopping);

            Vector3 position = _agent.FindCloestAroundEndPosition(shopping.position);
            _animator.SetBool(isMove, true);
            _agent.MoveTo(position);
            return BehaviorTree.ENodeStatus.InProgress;
        }, () =>
        {
            return _agent.AtDestination ? BehaviorTree.ENodeStatus.Succeeded : BehaviorTree.ENodeStatus.InProgress;
        });

        Shopping.Add<BTNode_Action>("고민하기", () =>
        {
            _animator.SetBool(isMove, false);
            return BehaviorTree.ENodeStatus.InProgress;
        }, () =>
        {
            Transform shopping = _localMemory.GetGeneric<Transform>(BlackBoardKey.Shopping);
            Vector3 dir = shopping.position - transform.position;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * _rotateSpeed);

            currentTime += Time.deltaTime;
            if(currentTime>3)
            {
                currentTime = 0;
                
                return BehaviorTree.ENodeStatus.Succeeded;
            }
            return BehaviorTree.ENodeStatus.InProgress;
        });

        Shopping.Add<BTNode_Action>("쇼핑 하기", () =>
        {
            Transform shopping = ShopManager.Instance.shopNewTrainPos[UnityEngine.Random.Range(0, ShopManager.Instance.shopNewTrainPos.Length)];
            _localMemory.SetGeneric<Transform>(BlackBoardKey.Shopping, shopping);
            Vector3 position = _agent.FindCloestAroundEndPosition(shopping.position);
            _agent.MoveTo(position);
            _animator.SetBool(isMove, true);
            return BehaviorTree.ENodeStatus.InProgress;
        }, () =>
        {
            return _agent.AtDestination ? BehaviorTree.ENodeStatus.Succeeded : BehaviorTree.ENodeStatus.InProgress;
        });

        Shopping.Add<BTNode_Action>("고민하기", () =>
        {
            _animator.SetBool(isMove, false);
            return BehaviorTree.ENodeStatus.InProgress;
        }, () =>
        {
            Transform shopping = _localMemory.GetGeneric<Transform>(BlackBoardKey.Shopping);
            Vector3 dir = shopping.position - transform.position;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * _rotateSpeed);
            return BehaviorTree.ENodeStatus.InProgress;
        });

        #endregion


        #region 명령이 바뀐경우
        var OrderChange = BTRoot.Add<BTNode_Sequence>("명령이 바뀐 경우");
        OrderChange.Add<BTNode_Action>("도구 내려놓고 초기화", () =>
         {
             PutDown();
             var reset = Reset();
             //타겟이 없다면 자기
             //있다면 다음 명령 수행하기
             return reset ? BehaviorTree.ENodeStatus.Succeeded : BehaviorTree.ENodeStatus.Failed;
         });

        var CantDoAnything = BTRoot.Add<BTNode_Sequence>("명령을 수행할 수 없는 경우");
        CantDoAnything.Add<BTNode_Action>("경고", () =>
        {
            _emoteImage.sprite = _emoteManager.GetEmote(_emoteManager.SadEmote);
            return BehaviorTree.ENodeStatus.InProgress;
        }, () =>
         {
             //명령이 바뀌었다면 명령 수행
             var order = _localMemory.GetGeneric<WorldResource.EType>(BlackBoardKey.Order);
             if(order!=_order)
             {
                _localMemory.SetGeneric<WorldResource.EType>(BlackBoardKey.Order, _order);
                 return BehaviorTree.ENodeStatus.Succeeded;
             }
             if(_helper.GotoPlayer)
             {
                 return BehaviorTree.ENodeStatus.Failed;
             }

             //역에 도착했다면
             //아니라면 자기
             return BehaviorTree.ENodeStatus.InProgress;
         });


    }


    private void PutDown()
    {
        if (_item != null)
        {
            Debug.Log("내려놓기");
            _item.PickUp();
            SoundManager.Instance.StopSoundEffect("Player_ToolsDown");
            SoundManager.Instance.PlaySoundEffect("Player_ToolsDown");

            _item.transform.parent = _stack.BFS(this);
            _item.transform.rotation = Quaternion.identity;
            _item.transform.localPosition = (Vector3.up * 0.5f) + (Vector3.up * 0.15f);
            _localMemory.SetGeneric<AI_Item>(BlackBoardKey.Item, null);
            _animator.SetBool(isDig, false);
            _animator.SetBool(isMove, false);
            _item = null;

        }
    }

    private bool Reset()
    {
        if(_target!=null)
        {
            _item = null;
            _localMemory.SetGeneric<WorldResource.EType>(BlackBoardKey.Order, _order);
            _target = null;
            return true;
        }

        return false;
    }

    #endregion

}
