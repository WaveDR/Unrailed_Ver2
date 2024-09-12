using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCreator : MonoBehaviour
{
    [Header("프리팹 정보들")]
    [SerializeField] private BlockMK2 _blockPrefab;
    [SerializeField] private Transform _waterFallUp;
    [SerializeField] private Transform _waterFallDown;
    [SerializeField] private Transform _barricade;
    [SerializeField] private ItemPrefabData _itemPrefabData;
    [SerializeField] private BlockTransformerData[] _blockTransformerData;
    [SerializeField] private Material[] _blocksMaterial;

    [Header("AI")]
    [SerializeField] private Helper _robot;

    [Header("ETC")]
    [SerializeField] private float _intervalTimeToMoveBlockBundle = 0.1f;  // 블럭 번들들이 떨어지는 시간간격

    // 여기서 거리 UI 만들어??
    private List<List<BlockMK2>> _groundList;

    /// <summary>
    /// 맵을 생성해서 위치까지 초기화해줍니다.
    /// </summary>
    /// <param name="mapData">생성할 맵의 데이터</param>
    /// <param name="parent">맵의 부모</param>
    /// <returns>블럭들의 2차원 리스트(맵)</returns>
    public async UniTask<List<List<BlockMK2>>> CreateMapAsync(MapData mapData, Transform parent, bool isInit = false)
    {
        InitMap(mapData, parent);

        if(isInit)
            PathFindingField.Instance.InitData(parent);

        await UniTask.Yield();
        InitBlock(mapData);
        await UniTask.Yield();
        SetHeightMap();



        return _groundList;
    }

    /// <summary>
    /// blockBundle 단위로 맵이 재배치 됩니다.
    /// </summary>
    /// <param name="blockBundleList">재배치될 blockBundle의 리스트</param>
    /// <returns></returns>
    public async UniTask RePositionAsync(List<BlockBundle> blockBundleList)
    {
            SoundManager.Instance.PlaySoundEffect("Btn_MapCreate");
        foreach (BlockBundle blockBundle in blockBundleList)
        {
            blockBundle.RePositionAsync().Forget();
            await UniTask.Delay(System.TimeSpan.FromSeconds(_intervalTimeToMoveBlockBundle));
        }
    }

    // 맵 생성
    private void InitMap(MapData mapData, Transform parent)
    {
        int x = mapData.mapData[0].arr.Length;
        int y = mapData.mapData.Length;

        int minX = (x / 2) - 1;
        int minY = (y / 2) - 1;

        _groundList = new List<List<BlockMK2>>();

        for (int i = 0; i < y; i++)
        {
            _groundList.Add(new List<BlockMK2>());
            for (int j = 0; j < x; j++)
            {
                BlockMK2 go = Instantiate(_blockPrefab, parent);
                go.transform.localPosition = new Vector3(j - minX, 0, i - minY);
                _groundList[i].Add(go);
            }
        }
    }

    // 오브젝트 생성
    private void InitBlock(MapData mapData)
    {
        int x = mapData.mapData[0].arr.Length;
        int y = mapData.mapData.Length;

        for (int i = 0; i < y; i++)
        {
            for (int j = 0; j < x; j++)
            {
                int index = mapData.mapData[i].arr[j];
                InitBlock(_groundList[i][j], index);
            }
        }
    }

    // 높이 설정
    private void SetHeightMap()
    {
        int x = _groundList[0].Count;
        int y = _groundList.Count;

        for (int i = 0; i < y; i++)
        {
            for (int j = 0; j < x; j++)
            {
                if (_groundList[i][j].Index == (int)EBlock.blackRock)
                {
                    int height = _groundList[i][j].GetHeight();
                    Transform child = _groundList[i][j].transform.GetChild(0);
                    child.localScale = Vector3.one + Vector3.up * height * 0.3f;
                }
            }
        }
    }

    // 블럭 설정
    private void InitBlock(BlockMK2 block, int index)
    {
        BlockTransformerData blockTransformerData = null;
        if (index == (int)EBlock.iron)
            blockTransformerData = _blockTransformerData[0];
        else if (index == (int)EBlock.blackRock)
            blockTransformerData = _blockTransformerData[1];

        Material blockMaterial = null;

        if (index > (int)EBlock.blackRock && index < (int)EBlock.duck)
            blockMaterial = _blocksMaterial[(int)EBlock.grass];
        else if (index >= (int)EBlock.duck)
            blockMaterial = _blocksMaterial[(int)EBlock.water];
        else
            blockMaterial = _blocksMaterial[index];

        block.Init(index, blockMaterial, _itemPrefabData.itemPrefabs[index], blockTransformerData);

        if (index == (int)EBlock.water)
        {
            // 위쪽이면 _waterFallUp, 밑쪽이면 _waterFallDown 생성
            if (block.transform.localPosition.z == -9)
                Instantiate(_waterFallDown.gameObject, block.transform);
            else if (block.transform.localPosition.z == _groundList.Count - 10)
                Instantiate(_waterFallUp.gameObject, block.transform);
        }
    }

    // 가장자리 바리케이드 설정
    public List<Transform> SetBarricade(Transform startTransform, Transform endTransform)
    {
        List<Transform> betweenVerticalBarricadeList = new List<Transform>();
        Transform parent = new GameObject("BarricadeParent").transform;

        int minX = Mathf.RoundToInt(startTransform.position.x);
        int maxX = Mathf.RoundToInt(endTransform.position.x);

        int minY = Mathf.RoundToInt(startTransform.position.z);
        int maxY = Mathf.RoundToInt(endTransform.position.z);

        // 사이 수직선
        for(int i = minY; i <= maxY; i++)
        {
            Vector3 pos = new Vector3((minX + maxX) / 2 + 1, 0.5f, i);
            betweenVerticalBarricadeList.Add(Instantiate(_barricade, pos, Quaternion.identity, parent));
        }

        // 작은 수평선
        for (int i = minX; i <= maxX; i++)
        {
            Vector3 pos = new Vector3(i, 0.5f, minY - 1);
            Instantiate(_barricade, pos, Quaternion.identity, parent);
        }

        // 큰 수평선
        for (int i = minX; i <= maxX; i++)
        {
            Vector3 pos = new Vector3(i, 0.5f, maxY + 1);
            Instantiate(_barricade, pos, Quaternion.identity, parent);
        }

        // 작은 수직선
        for (int i = minY; i <= maxY; i++)
        {
            Vector3 pos = new Vector3(minX - 1, 0.5f, i);
            Instantiate(_barricade, pos, Quaternion.identity, parent);
        }

        // 큰 수평선
        for (int i = minY; i <= maxY; i++)
        {
            Vector3 pos = new Vector3(maxX + 1, 0.5f, i);
            Instantiate(_barricade, pos, Quaternion.identity, parent);
        }

        return betweenVerticalBarricadeList;
    }
}
