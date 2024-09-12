using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Transform")]
    [SerializeField] private Transform _rayStartTransform;
    [SerializeField] private Transform _rightHandTransform;
    [SerializeField] private Transform _twoHandTransform;
    [SerializeField] private Transform _twoHandFarTransform;

    [Header("Object")]
    [SerializeField] private Transform _railPreview;
    [SerializeField] private GameObject _balloonObject;
    [SerializeField] private GameObject _runParticle;

    [Header("Prefabs")]
    [SerializeField] private GameObject _bridgePrefab;

    [Header("UI")]
    [SerializeField] private WaterGauge _waterGauge;

    // 상태  => 이건 상태패턴??
    private bool _isDash = false;
    private bool _isInteractive = false;
    private bool _isRespawn = false;
    private bool _isCharge = false;


    // 컴포넌트
    private Rigidbody _rigidbody;
    private PlayerInput _playerInput;
    private PlayerStat _playerStat;
    private PlayerAnimator _playerAnim;

    // 물건
    // 종류가 다르면 안됨
    private Stack<MyItem> _handItem = new Stack<MyItem>();
    private Stack<MyItem> _detectedItem = new Stack<MyItem>();

    // 플레이어가 현재 들고 있는 차량
    private ShopItem _currentHoldTrain;

    // 플레이어 수치
    private float _currentSpeed;
    // 현재 상호작용 쿨타임
    private float _currentInteractCoolTime;

    // 현재 서 있는 블럭
    private Transform _currentblock;
    // 전방에 있는 오브젝트
    private Transform _currentFrontObject;

    Vector3[] dir = new Vector3[8] { Vector3.forward, Vector3.back, Vector3.right, Vector3.left,
        new Vector3(1, 0, 1), new Vector3(1, 0, -1), new Vector3(-1, 0, -1), new Vector3(-1, 0, 1)};

    // 프로퍼티
    public Transform RightHandTransform => _rightHandTransform;
    public Transform TwoHandTransform => _twoHandTransform;
    public Transform CurrentBlockTransform => _currentblock;
    public Transform AroundEmptyBlockTranform => BFS();
    public MyItem CurrentHandItem => _handItem.Count == 0 ? null : _handItem.Peek();  // 현재 들고 있는 아이템
    public ShopItem CurrentHoldTrain => _currentHoldTrain;
    public Transform CurrentFrontObject => _currentFrontObject;

    public int BoltCount => _boltCount;
    private int _boltCount = 0;


    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _playerInput = GetComponent<PlayerInput>();
        _playerAnim = GetComponent<PlayerAnimator>();
        _playerStat = GetComponent<PlayerStat>();
        _runParticle.SetActive(false);
        InitPlayer();
    }

    private void InitPlayer()
    {
        _isDash = false;
        _isInteractive = false;
        _currentSpeed = _playerStat.moveSpeed;
    }

    #region Move

    private void FixedUpdate()
    {
        // 플레이어 움직임
        Move();
    }

    private void Move()
    {
        if (_isRespawn)
            return;

        // 움직임, 회전, 대시까지
        if (_playerInput.IsShift && !_isDash)
        {
            SoundManager.Instance.PlaySoundEffect("Player_Dash");
            _isDash = true;
            _runParticle.SetActive(true);
            _currentSpeed = _playerStat.dashSpeed;
            Invoke("DashOff", _playerStat.dashDuration);
        }

        transform.position += _playerInput.Dir * _currentSpeed * Time.deltaTime;
        transform.LookAt(_playerInput.Dir + transform.position);
    }

    private void DashOff()
    {
        _currentSpeed = _playerStat.moveSpeed;
        _runParticle.SetActive(false);
        _isDash = false;
    }
    #endregion

    private void Update()
    {
        // 현재 땅, 전방 물체 감지
        DetectGroundBlock();
        DetectFrontObject();

        // 레일 미리보기 확인
        CheckPutDownRail();

        // 아이템 상호작용
        if (_playerInput.IsSpace)
            InteractiveItemSpace();
        InteractivItem();

        // 환경 상호작용
        DetectWater();  // 물 뜨기
        DigUp();        // 캐기
        Attack();       // 공격
        OffFire();      // 불끄기
    }

    // spacebar 누를 때
    private void InteractiveItemSpace()
    {
        // 상점 기차와 상호작용 가능하면 return하여 밑에 코드 실행하지 않는다.
        if (InteractShopTrain())
            return;

        // 그냥 기차와 상호작용 가능하면 return하여 밑에 코드 실행하지 않는다. (workBench와 box)
        if (InteractTrain())
            return;

        if (_handItem.Count == 0 && _detectedItem.Count != 0)  // 줍기
        {
            Debug.Log("줍기");
            Pair<Stack<MyItem>, Stack<MyItem>> p = _detectedItem.Peek().PickUp(_handItem, _detectedItem);
            _handItem = p.first;
            _detectedItem = p.second;
            ItemIOSound(0);

            InteractionHighlight();

            if (_handItem.Peek().ItemType == EItemType.wood || _handItem.Peek().ItemType == EItemType.steel)
            {
                WorldResource resource = _handItem.Peek().GetComponent<WorldResource>();
                if (resource != null)
                    ResourceTracker.Instance.DeRegisterResource(resource);
            }
        }

        else if (_handItem.Count != 0 && _detectedItem.Count == 0) // 버리기
        {
            Debug.Log("버리기");
            ItemIOSound(1);
            Pair<Stack<MyItem>, Stack<MyItem>> p = _handItem.Peek().PutDown(_handItem, _detectedItem);
            _handItem = p.first;
            _detectedItem = p.second;

            InteractionHighlight();
        }
        else if (_handItem.Count != 0 && _detectedItem.Count != 0) // 교체
        {
            Debug.Log("교체");
            ItemIOSound(2);
            Pair<Stack<MyItem>, Stack<MyItem>> p = _handItem.Peek().Change(_handItem, _detectedItem);
            _handItem = p.first;
            _detectedItem = p.second;

            InteractionHighlight();


        }
    }

    // 안 누를 때
    private void InteractivItem()
    {
        if (_handItem.Count != 0 && _detectedItem.Count != 0 && _handItem.Count <= 2)
        {
            Debug.Log("와다닥 줍기");

            ItemIOSound(3);
            Pair<Stack<MyItem>, Stack<MyItem>> p = _handItem.Peek().AutoGain(_handItem, _detectedItem);
            _handItem = p.first;
            _detectedItem = p.second;
            if (_handItem.Peek().ItemType == EItemType.wood || _handItem.Peek().ItemType == EItemType.steel)
            {
                WorldResource resource = _handItem.Peek().GetComponent<WorldResource>();
                if(resource!=null)
                ResourceTracker.Instance.DeRegisterResource(resource);
            }

        }

    }

    // 상점 기차아이템과 상호작용하는 메소드
    private bool InteractShopTrain()
    {
        ShopItem shop = null;
        if (_currentFrontObject != null && _currentFrontObject.gameObject.layer == LayerMask.NameToLayer("ShopItem"))
            shop = _currentFrontObject.GetComponent<ShopItem>();
        bool isDetected = shop != null;  // shop이 null이 아니라면 상점의 차량이 감지가 됨
        if (ShopManager.Instance.trainCoin == 0)
        {
            Debug.Log("코일 100개를 충전해줄게");
            // ShopManager.Instance.trainCoin = 100;
        }

        // 감지된게 있고 가지고 있는 차량이 없을 때  => 주워
        if (isDetected && _currentHoldTrain == null)
        {
            // 금액을 지불할 수 있으면
            if (shop.TryBuyItem())
            {
                // 일단 집어
                _currentHoldTrain = shop;
                _currentHoldTrain.SetPosition(_twoHandFarTransform);
                _playerAnim.anim.SetBool("isTwoHandsPickUp", true);
            }
            return true;
        }

        // 감지된게 없고 가지고 있는 차량이 있을 때  => 버려
        else if (!isDetected && _currentHoldTrain != null)
        {
            _currentHoldTrain.SetInitPosition();
            _currentHoldTrain = null;
            _playerAnim.anim.SetBool("isTwoHandsPickUp", false);
            return true;
        }

        // 감지된게 있고 가지고 있는 차량이 있을 때  => 교환
        else if (isDetected && _currentHoldTrain != null)
        {
            // 일단 버리고
            _currentHoldTrain.SetInitPosition();
            _currentHoldTrain = null;
            _playerAnim.anim.SetBool("isTwoHandsPickUp", false);
            // 금액을 지불할 수 있으면
            if (shop.TryBuyItem())
            {
                // 일단 집어
                _currentHoldTrain = shop;
                _currentHoldTrain.SetPosition(_twoHandFarTransform);
                _playerAnim.anim.SetBool("isTwoHandsPickUp", true);
            }
            return true;
        }
        return false;
    }

    // 기차와 상호작용하는 메소드
    private bool InteractTrain()
    {
        if (_currentFrontObject != null)
        {
            // workBench와 상호작용
            if (_currentFrontObject.gameObject.layer == LayerMask.NameToLayer("WorkBench"))
            {
                TrainWorkBench bench = _currentFrontObject.GetComponent<TrainWorkBench>();

                MyItem[] rail = bench.GetComponentsInChildren<MyItem>();

                Transform aroundTransform = BFS();

                while (_handItem.Count != 0)
                {
                    if (_currentblock.childCount == 0)
                    {
                        _detectedItem.Push(_handItem.Pop());
                        _detectedItem.Peek().transform.SetParent(_currentblock);
                        _detectedItem.Peek().transform.localPosition = Vector3.up * 0.5f + Vector3.up * (_detectedItem.Count - 1) * 0.15f;
                        _detectedItem.Peek().transform.localRotation = Quaternion.identity;
                    }
                    else
                    {
                        _detectedItem.Push(_handItem.Pop());
                        _detectedItem.Peek().transform.SetParent(aroundTransform);
                        _detectedItem.Peek().transform.localPosition = Vector3.up * 0.5f + Vector3.up * (_detectedItem.Count - 1) * 0.15f;
                        _detectedItem.Peek().transform.localRotation = Quaternion.identity;
                    }
                }
                for (int i = 0; i < rail.Length; i++)
                {
                    SoundManager.Instance.PlaySoundEffect("Rail_Up");
                    //레일의 부모를 다시 정적상태인 풀링으로 이동
                    Debug.Log("빼내기");
                    _handItem.Push(rail[i]);
                    _handItem.Peek().transform.SetParent(_twoHandTransform);
                    _handItem.Peek().transform.localPosition = Vector3.up * (_handItem.Count - 1) * 0.15f;
                    _handItem.Peek().transform.localRotation = Quaternion.identity;
                    bench.spawnIndex--;
                    bench.anim.SetInteger("GetRails", 0);
                }

                return true;
            }
            // box와 상호작용
            else if (_currentFrontObject.gameObject.layer == LayerMask.NameToLayer("Box"))
            {
                TrainBox box = _currentFrontObject.GetComponent<TrainBox>();

                if (CurrentHandItem.ItemType == EItemType.wood)
                {
                    while (box.woodStack.Count < box.maxItem)
                    {
                        if (_handItem.Count == 0)
                            break;

                        Debug.Log("납품");

                        _handItem = box.GiveMeItem(_handItem);
                    }
                    SoundManager.Instance.PlaySoundEffect("Wood_InBox");
                }
                else if (CurrentHandItem.ItemType == EItemType.steel)
                {
                    while (box.steelStack.Count < box.maxItem)
                    {
                        if (_handItem.Count == 0)
                            break;

                        Debug.Log("납품");
                        _handItem = box.GiveMeItem(_handItem);
                    }
                    SoundManager.Instance.PlaySoundEffect("Steel_InBox");
                }
                return true;
            }
            else if (_currentFrontObject.gameObject.layer == LayerMask.NameToLayer("DynamiteTrain"))
            {
                TrainDynamite trainDynamite = _currentFrontObject.GetComponent<TrainDynamite>();

                MyItem item = trainDynamite.GetItem();

                if (item == null)
                    return false;

                Transform aroundTransform = BFS();

                while (_handItem.Count != 0)
                {
                    if (_currentblock.childCount == 0)
                    {
                        _detectedItem.Push(_handItem.Pop());
                        _detectedItem.Peek().transform.SetParent(_currentblock);
                        _detectedItem.Peek().transform.localPosition = Vector3.up * 0.5f + Vector3.up * (_detectedItem.Count - 1) * 0.15f;
                        _detectedItem.Peek().transform.localRotation = Quaternion.identity;

                    }
                    else
                    {
                        _detectedItem.Push(_handItem.Pop());
                        _detectedItem.Peek().transform.SetParent(aroundTransform);
                        _detectedItem.Peek().transform.localPosition = Vector3.up * 0.5f + Vector3.up * (_detectedItem.Count - 1) * 0.15f;
                        _detectedItem.Peek().transform.localRotation = Quaternion.identity;
                    }
                }

                _handItem.Push(item);
                _handItem.Peek().transform.SetParent(_twoHandTransform);
                _handItem.Peek().transform.localPosition = Vector3.zero;
                _handItem.Peek().transform.localRotation = Quaternion.identity;
                _handItem.Peek().transform.localScale = Vector3.one;

                return true;
            }
        }
        return false;
    }

    private void ItemIOSound(int i)
    {
        if (_handItem.Count == 0)
            return;

        switch (i)
        {
            //줍기
            case 0:
                if (_handItem.Peek().ItemType == EItemType.rail)
                    SoundManager.Instance.PlaySoundEffect("Rail_Up");

                else if (_handItem.Peek().ItemType == EItemType.axe || _handItem.Peek().ItemType == EItemType.pick)
                    SoundManager.Instance.PlaySoundEffect("Player_ToolsUp");


                else
                    SoundManager.Instance.PlaySoundEffect("Item_Up");

                break;

            //버리기
            case 1:
                if (_handItem.Peek().ItemType == EItemType.rail)
                {
                    SoundManager.Instance.StopSoundEffect("Rail_Down");
                    SoundManager.Instance.PlaySoundEffect("Rail_Down");
                }
                else if (_handItem.Peek().ItemType == EItemType.steel)
                {
                    SoundManager.Instance.StopSoundEffect("Steel_Down");
                    SoundManager.Instance.PlaySoundEffect("Steel_Down");
                }
                else if (_handItem.Peek().ItemType == EItemType.wood)
                {
                    SoundManager.Instance.StopSoundEffect("Wood_Down");
                    SoundManager.Instance.PlaySoundEffect("Wood_Down");
                }
                else if (_handItem.Peek().ItemType == EItemType.axe || _handItem.Peek().ItemType == EItemType.pick)
                {
                    SoundManager.Instance.StopSoundEffect("Player_ToolsDown");
                    SoundManager.Instance.PlaySoundEffect("Player_ToolsDown");
                }
                break;

            //교체
            case 2:
                if (_handItem.Peek().ItemType == EItemType.rail)
                    SoundManager.Instance.PlaySoundEffect("Rail_Up");

                else if (_handItem.Peek().ItemType == EItemType.axe || _handItem.Peek().ItemType == EItemType.pick)
                    SoundManager.Instance.PlaySoundEffect("PlayerToolsUp");

                else
                    SoundManager.Instance.PlaySoundEffect("Item_Up");
                break;
            //와다닥 줍기
            case 3:
                if (_handItem.Peek().ItemType == EItemType.rail)
                {
                    SoundManager.Instance.StopSoundEffect("Rail_Up");
                    SoundManager.Instance.PlaySoundEffect("Rail_Up");
                }
                else
                {
                    SoundManager.Instance.StopSoundEffect("Item_Up");
                    SoundManager.Instance.PlaySoundEffect("Item_Up");
                }
                break;
        }
    }

    // 손에 있는 물건을 떨구는 메소드
    public void PutDownItem()
    {
        Debug.Log(_handItem.Count);
        if (_handItem.Count != 0)
        {
            Pair<Stack<MyItem>, Stack<MyItem>> p = _handItem.Peek().PutDown(_handItem, _detectedItem);
            _handItem = p.first;
            _detectedItem = p.second;
        }

        _playerAnim.anim.SetBool("isTwoHandsPickUp", false);
    }

    #region 감지 메소드

    private void InteractionHighlight()
    {
        MyItem[] handItem = _handItem.ToArray();
        MyItem[] detectedItem = _detectedItem.ToArray();

        for (int i = 0; i < handItem.Length; i++)
        {
            ItemInteractionTest item = handItem[i].GetComponent<ItemInteractionTest>();
            if(item != null)
                item.Interaction(false);
        }
        for (int i = 0; i < detectedItem.Length; i++)
        {
            ItemInteractionTest item = detectedItem[i].GetComponent<ItemInteractionTest>();
            if (item != null)
                item.Interaction(true);
        }
    }

    private void DetectGroundBlock()
    {
        if (Physics.Raycast(_rayStartTransform.position, Vector3.down, out RaycastHit hit, _playerStat.detectRange, _playerStat.blockLayer))
        {
            // 캐싱
            if (_currentblock == hit.transform)
                return;

            if(_currentblock != null)
            {
                // 기존 하이라이트 꺼주기
                for (int i = 0; i < _currentblock.childCount; i++)
                {
                    MyItem item = _currentblock.GetChild(i).GetComponent<MyItem>();
                    if(item != null)
                    {
                        Debug.Log("이거 해?");
                        ItemInteractionTest interactionItem = _currentblock.GetChild(i).GetComponent<ItemInteractionTest>();
                        if (interactionItem != null)
                            interactionItem.Interaction(false);
                    }
                }
            }

            _currentblock = hit.transform;
            _detectedItem = new Stack<MyItem>();

            for (int i = 0; i < _currentblock.childCount; i++)
            {
                MyItem item = _currentblock.GetChild(i).GetComponent<MyItem>();

                if (item != null)
                {
                    // 전방에 있는 오브젝트가 우선순위 위
                    if (_currentFrontObject == null)
                    {
                        // 새로운 하이라이트 넣어주기
                        ItemInteractionTest interactionItem = item.GetComponent<ItemInteractionTest>();
                        if (interactionItem != null)
                            interactionItem.Interaction(true);
                    }

                    _detectedItem.Push(item);
                }
            }
        }
        else
        {
            _currentblock = null;
        }
    }

    // 레일을 이을 수 있는 상황이라면 preview 레일을 활성화시킨다.
    private void CheckPutDownRail()
    {
        if (_handItem.Count != 0 && _handItem.Peek().ItemType == EItemType.rail)
        {
            for (int i = 0; i < 4; i++)
            {
                if (Physics.Raycast(_currentblock.position, dir[i], out RaycastHit hit, _playerStat.detectRange, _playerStat.blockLayer))
                {
                    // 현재 땅 위에 아무것도 없고 주위에 마지막 레일이 있다면
                    if (_currentblock.childCount == 0 && hit.transform.childCount != 0 && hit.transform.GetChild(0).GetComponent<RailController>() == FindObjectOfType<GoalManager>().lastRail)
                    {
                        _railPreview.gameObject.SetActive(true);
                        _railPreview.SetParent(null);
                        _railPreview.position = _currentblock.position + Vector3.up * 0.5f;
                        _railPreview.rotation = Quaternion.identity;
                        return;
                    }
                }
            }
        }
        if (_railPreview.gameObject.activeSelf)
            _railPreview.gameObject.SetActive(false);
    }

    private void DetectFrontObject()
    {
        if (Physics.Raycast(_rayStartTransform.position, transform.forward, out RaycastHit hit, _playerStat.detectRange, _playerStat.detectableLayer))
        {
            if (!_isInteractive)
            {
                _currentInteractCoolTime += Time.deltaTime;

                if (_currentInteractCoolTime > _playerStat.interactiveCoolTime)
                {
                    _currentInteractCoolTime = 0;
                    _isInteractive = true;
                }
            }

            if (hit.transform.GetComponent<MyItem>() != null)
                return;

            // 캐싱
            if (_currentFrontObject == hit.transform)
                return;

            ItemInteractionTest interactionObject = null;

            // 이전 오브젝트 하이라이트 꺼주기
            if (_currentFrontObject != null)
            {
                Debug.Log("꺼줍니다.");
                interactionObject = _currentFrontObject.GetComponent<ItemInteractionTest>();
                if (interactionObject != null)
                    interactionObject.Interaction(false);
            }

            _currentFrontObject = hit.transform;

            // 새로운 오브젝트 하이라이트 켜주기
            Debug.Log("켜줍니다.");
            interactionObject = _currentFrontObject.GetComponent<ItemInteractionTest>();
            if (interactionObject != null)
                interactionObject.Interaction(true);
        }
        else
        {
            if (_currentFrontObject != null)
            {
                Debug.Log("꺼줍니다.");
                ItemInteractionTest interactionObject = _currentFrontObject.GetComponent<ItemInteractionTest>();
                if (interactionObject != null)
                    interactionObject.Interaction(false);
            }

            _currentInteractCoolTime = 0;
            _isInteractive = false;
            _currentFrontObject = null;
        }
    }

    private Transform BFS()
    {
        // CurrentBlockTransform => _currentblock
        // 현재 블럭에서 가장 가까운 자식이 없는 블럭을 반환하자

        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(_currentblock);

        HashSet<Transform> hashSet = new HashSet<Transform>();
        hashSet.Add(_currentblock);

        while (queue.Count != 0)
        {
            Transform currentBlock = queue.Dequeue();

            if (currentBlock.childCount == 0)
                return currentBlock;

            for (int i = 0; i < 8; i++)
                if (Physics.Raycast(currentBlock.position, dir[i], out RaycastHit hit, 1f, _playerStat.blockLayer))
                    if (hashSet.Add(hit.transform))
                        if (hit.transform.childCount == 0 || hit.transform.childCount != 0 &&
                            hit.transform.GetChild(0).gameObject.layer != LayerMask.NameToLayer("Water") &&
                            hit.transform.GetChild(0).gameObject.layer != LayerMask.NameToLayer("Empty"))
                            queue.Enqueue(hit.transform);
        }

        return null;
    }
    #endregion

    #region 민경이 형
    private void DetectWater()  // 물 감지
    {
        if (_currentFrontObject == null)
        {
            return;
        }

        if (_currentFrontObject.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            if (CurrentHandItem != null && CurrentHandItem.ItemType == EItemType.bucket)
            {
                if (!_waterGauge.IsFillWater())
                {

                    if (!_isCharge)
                    {
                        SoundManager.Instance.PlaySoundEffect("Player_WaterImport");
                        _isCharge = true;
                    }
                    // 물 채우기
                    _waterGauge.gameObject.SetActive(true);
                    _waterGauge.FillGauge();
                }
                else
                {
                    CurrentHandItem.ActiveWater(true);
                    _isCharge = false;
                }
            }
        }
    }

    public bool SetBridge() // 다리 놓기
    {
        if (_currentFrontObject == null)
            return false;

        if (_currentFrontObject.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            if (CurrentHandItem != null && CurrentHandItem.ItemType == EItemType.wood)
            {
                // 다리 놓기
                Transform bridge = Instantiate(_bridgePrefab, _currentFrontObject.parent.parent).transform;
                bridge.localPosition = _currentFrontObject.parent.localPosition + Vector3.up * -0.5f;
                bridge.localRotation = Quaternion.identity;

                // 기존 블럭도 없애고 (없애면 그 자식들도 없어지지요)
                Destroy(_currentFrontObject.parent.gameObject);
                Destroy(_handItem.Pop().gameObject);

                return true;
            }
        }

        return false;
    }

    private void DigUp() // 캐기
    {
        if (_currentFrontObject == null)
            return;

        if (!_isInteractive)
            return;

        // 여기 애니메이션
        if (_currentFrontObject.gameObject.layer == LayerMask.NameToLayer("diggable"))
        {
            if (CurrentHandItem == null)
                return;

            ReSource resource = _currentFrontObject.GetComponent<ReSource>();
            if (resource == null)
                return;

            if (CurrentHandItem.ItemType == EItemType.axe && resource.ResourceType == EResource.tree)
            {
                resource.Dig();
                _isInteractive = false;
            }
            else if (CurrentHandItem.ItemType == EItemType.pick && resource.ResourceType == EResource.steel)
            {
                resource.Dig();
                _isInteractive = false;
            }
        }
    }

    private void Attack()  // 공격
    {
        if (_currentFrontObject == null)
            return;

        if (!_isInteractive)
            return;

        if (_currentFrontObject.gameObject.layer == LayerMask.NameToLayer("attackable"))
        {
            if (CurrentHandItem != null)
            {
                if (CurrentHandItem.ItemType == EItemType.pick || CurrentHandItem.ItemType == EItemType.axe)
                {
                    AnimalHealth animal = _currentFrontObject.GetComponent<AnimalHealth>();
                    if (animal != null)
                    {
                        SoundManager.Instance.PlaySoundEffect("Player_Hit");
                        animal.Hit();
                        _isInteractive = false;
                    }
                }
            }
        }
    }

    private void AddBolt(GameObject bolt) // 코일 먹기
    {
        ShopManager.Instance.trainCoin++;
        // _boltCount++;
        Destroy(bolt);
    }

    public void Respawn()
    {
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        _rigidbody.useGravity = true;

        _isRespawn = true;

        InitPlayer();

        // 죽으면 들고 있는거 다 놓자
        PutDownItem();

        transform.position += Vector3.up * 10f;
        _balloonObject.SetActive(true);
    }

    private void OffFire()
    {
        if(_currentFrontObject != null && CurrentHandItem != null)
        {
            if (CurrentHandItem.ItemType == EItemType.bucket && _currentFrontObject.gameObject.layer == LayerMask.NameToLayer("WaterBox"))
            {
                if (_waterGauge.IsFillWater() || CurrentHandItem.GetComponent<Item_Bucket>().Full)
                {
                    TrainWater water = _currentFrontObject.GetComponent<TrainWater>();

                    // 불꺼주기 넣어야 해
                    water.FireOff();
                    CurrentHandItem.ActiveWater(false);
                    _waterGauge.ResetWater();
                    CurrentHandItem.GetComponent<Item_Bucket>().BucketisFull();
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Block"))
        {
            if (_isRespawn)
            {
                _rigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
                _rigidbody.useGravity = false;
                _balloonObject.SetActive(false);
                _isRespawn = false;
                transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("IntroUI"))
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                InteractionUI startUI = other.GetComponent<InteractionUI>();

                if (startUI.isMapEditor) startUI.GoMapEdit();
                if (!startUI.Exit) startUI.GameStart();
                if (startUI.Exit) startUI.GameExit();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "ItemBolt(Clone)")
        {
            AddBolt(other.gameObject);
        }
    }
    #endregion
}
