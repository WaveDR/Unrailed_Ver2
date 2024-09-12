using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InGameScene : MonoBehaviour
{
    [Header("�׽�Ʈ")]
    [SerializeField] private bool isTest = false;
    [SerializeField] private bool isAITest = false;

    [Header("UI")]
    public GameObject _loadingSceneUI;
    public GameObject backToLobbyUI;
    public GameObject _distanceUI;
    public GameObject _aiDebugUI;

    [Header("Color")]
    [SerializeField] private Color _originColor;
    [SerializeField] private Color _endingColor;

    [Header("Manager")]
    [SerializeField] private WorldManager _worldManager;
    [SerializeField] private ShopManager _shopManager;

    [SerializeField] private int worldCount = 0;

    public TrainEngine engine;

    // ��ȯ�� �� isStart�� true�� ���� player�� �۵��� �� �ְ� ����~~
    public bool isStart { get; private set; } = false;

    private bool _isEnding = false;

    private void Awake()
    {
        // ��ο� ��
        Camera.main.backgroundColor = _originColor;

        // ���� �ε�
        FileManager.LoadGame();

        // �ε�
        backToLobbyUI.SetActive(false);
        _loadingSceneUI.SetActive(true);
        isStart = false;

        if (isAITest)
            _aiDebugUI.SetActive(true);
        else
            _aiDebugUI.SetActive(false);

        // �ε� ����
        LoadingFirstGame(isTest, 
            () =>
            {
                _loadingSceneUI.SetActive(false);
                RePositionAsync().Forget();

                _shopManager.StartTrainMove();
            }).Forget();
    }


    /// <summary>
    /// �� �����ϸ� ����� �޼ҵ�
    /// </summary>
    public void ArriveStation()
    {
        // ���� ������ ���̶�� ����
        if(_isEnding)
        {
            Camera.main.backgroundColor = _endingColor;
            Debug.Log("�����Դϴ�~~~~");

            StartCoroutine(engine.ClearAnim());
        }
        else
        {
            if(_worldManager.enemyObject != null)
            {
                Destroy(_worldManager.enemyObject);
            }

            // �Ÿ� UI ��Ȱ��ȭ
            _distanceUI.SetActive(false);

            // �÷��̾� �տ� ��� ��������
            FindObjectOfType<PlayerController>().PutDownItem();

            Helper helper = FindObjectOfType<Helper>();
            helper.ArriveStation();

            // �ٸ����̵� �÷��ֱ�
            Transform barricadeparent = GameObject.Find("BarricadeParent").transform;
            barricadeparent.position += Vector3.up * 20f;

            // �ٸ����̵� �߰� �����ֱ�
            for (int i = 0; i < _worldManager.betweenBarricadeTransform.Count; i++)
            {
                Destroy(_worldManager.betweenBarricadeTransform[i].gameObject);
            }
            _worldManager.betweenBarricadeTransform.Clear();


            // ��Ʈ �ϳ� �߰� ���ְ�

            // �����ִٰ� ���������ֱ�

            _shopManager.ShopOn();
        }
    }

    /// <summary>
    /// �� ������ ����� �޼ҵ�
    /// </summary>
    public void LeaveStation()
    {
        // �Ÿ� UI Ȱ��ȭ
        _distanceUI.SetActive(true);

        // �ø� �ٸ����̵� �����ֱ�
        Transform barricadeparent = GameObject.Find("BarricadeParent").transform;
        barricadeparent.position -= Vector3.up * 20f;

        Helper helper = FindObjectOfType<Helper>();
        RespawnTool();

        // ������ ������ 100�Ǹ� ����� �޼ҵ�

        // ���ο� �� ����
        _shopManager.currentStation = _worldManager.stations[2].transform;

        // ���� ����ǰ�
        _shopManager.ShopOff();

        _worldManager.SpawnCow();

        // �� ����ġ �����ֱ�
        UnitaskInvoke(1.5f, () => { RePositionAsync().Forget(); helper.ArriveStation(); }).Forget();

        // ���� 2�� �ۿ� �����ϱ� �ѹ� ���� ������ �����غ� �Ϸ�
        _isEnding = true;
    }

    private void RespawnTool()
    {
        // ���� �� ��ó�� ������ ����
        Transform blockParent = _shopManager.currentStation.parent;
        List<MyItem> tools = new List<MyItem>();
        tools.Add(FindObjectOfType<MyBucketItem>());
        MyNonStackableItem[] items = FindObjectsOfType<MyNonStackableItem>();

        for (int i = 0; i < items.Length; i++)
        {
            tools.Add(items[i]);
        }

        for (int i = 0; i < tools.Count; i++)
        {
            tools[i].transform.SetParent(BFS(blockParent));
            tools[i].transform.localPosition = Vector3.up * 0.5f;
            tools[i].transform.localRotation = Quaternion.identity;
        }

    }

    private Transform BFS(Transform startTransform)
    {
        Transform currentTransform = startTransform;
        while (currentTransform.childCount != 0)
        {
            InfiniteLoopDetector.Run();

            if (Physics.Raycast(currentTransform.position, Vector3.back, out RaycastHit hit, 1f, 1 << LayerMask.NameToLayer("Block")))
            {
                currentTransform = hit.transform;
            }
        }
        return currentTransform;
    }


    private async UniTaskVoid RePositionAsync(System.Action onFinishedAsyncEvent = null)
    {
        await _worldManager.RePositionAsync(worldCount++);
        onFinishedAsyncEvent?.Invoke();
    }

    private async UniTaskVoid LoadingFirstGame(bool isTest, System.Action onCreateNextMapAsyncEvent = null)
    {
        // �� ����
        await _worldManager.GenerateWorld(isTest);
        onCreateNextMapAsyncEvent?.Invoke();
    }

    private async UniTaskVoid UnitaskInvoke(float time, System.Action action)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(time));

        action?.Invoke();
    }
}
