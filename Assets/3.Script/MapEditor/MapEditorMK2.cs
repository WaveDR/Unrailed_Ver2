using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// public enum EBlock { empty, grass, water, tree1, tree2, iron, blackRock, size }
public enum EBlock { empty, grass, water, tree1, tree2, iron,
    blackRock, station, fence,
    rail, treeItem, ironItem, axe,
    pick, bucket, bolt, duck, flamingo, size}

public enum ECreature { player, helper, enemy, cow}

public class MapEditorMK2 : MonoBehaviour
{
    [Header("Blocks")]
    [SerializeField] private BlockMK2 _blockPrefab;
    [SerializeField] private Material[] _blocksMaterial; 

    [Header("Transform")]
    [SerializeField] private Transform _blockParent;
    [SerializeField] private Transform _borderParent;

    [Header("Info")]
    [SerializeField] private const int _defaultX = 40;
    [SerializeField] private const int _defaultY = 20;

    [Header("Data")]
    [SerializeField] private ItemPrefabData _itemPrefabData;
    [SerializeField] private BlockTransformerData[] _blockTransformerData;

    [Header("Prefabs")]
    [SerializeField] private Transform _waterFallUp;
    [SerializeField] private Transform _waterFallDown;
    [SerializeField] private GameObject _barricade;

    [Header("ETC")]
    [SerializeField] private Transform _target;
    [SerializeField] private Transform[] _creatures;

    [HideInInspector] public int materialIndex;
    [HideInInspector] public int creatureIndex;
    [HideInInspector] public bool isCreatureMode = false;
    // true�� ��ȣ�ۿ��� �� ����
    [HideInInspector] public bool isInteract = true;

    // ��� �����ϴϱ� ����Ʈ��?
    private List<List<BlockMK2>> groundList = new List<List<BlockMK2>>();

    private Vector2Int[] _creaturePos = new Vector2Int[4];

    private int _minX;
    private int _minY;
    private Camera _mainCam;
    private Vector3 _mainCamOriginPos;

    public void Awake()
    {
        isInteract = true;

        _mainCam = Camera.main;
        _mainCamOriginPos = _mainCam.transform.position;
        _target.gameObject.SetActive(true);

        InitMap();

        FileManager.LoadGame();
    }

    // �� ������Ʈ ����
    private void ClearMap()
    {
        // ���� ������Ʈ ����
        _blockParent.DestroyAllChild();
        _borderParent.DestroyAllChild();

        // ũ���� ���ֱ�
        for(int i = 0; i < _creatures.Length; i++)
        {
            _creaturePos[i] = Vector2Int.zero;
            _creatures[i].gameObject.SetActive(false);
        }
    }

    // �� �ʱ�ȭ
    public void InitMap()
    {
        ClearMap();

        // ī�޶� ����ġ
        _mainCam.transform.position = _mainCamOriginPos;

        groundList = new List<List<BlockMK2>>();

        _minX = (_defaultX / 2) - 1;
        _minY = (_defaultY / 2) - 1;

        for (int i = 0; i < _defaultY; i++)
        {
            groundList.Add(new List<BlockMK2>());
            for (int j = 0; j < _defaultX; j++)
            {
                BlockMK2 go = Instantiate(_blockPrefab, _blockParent);
                InitBlock(go, (int)EBlock.grass);
                go.transform.localPosition = new Vector3(j - _minX, 0, i - _minY);
                groundList[i].Add(go);
            }
        }
    }


    // x�� ��ȭ
    public void ResizeXMap(int value)
    {
        if(value > 0)
        {
            int x = groundList[0].Count;
            for(int i = 0; i < groundList.Count; i++)
            {
                BlockMK2 go = Instantiate(_blockPrefab, _blockParent);
                InitBlock(go, (int)EBlock.grass);
                go.transform.localPosition = new Vector3(x - _minX, 0, i - _minY);
                groundList[i].Add(go);
            }
        }
        else if(value < 0)
        {
            for (int i = 0; i < groundList.Count; i++)
            {
                BlockMK2 go = groundList[i][groundList[i].Count - 1];
                groundList[i].RemoveAt(groundList[i].Count - 1);
                Destroy(go.gameObject);
            }
        }
        _mainCam.transform.position = new Vector3((groundList[0].Count - _defaultX) / 2, Camera.main.transform.position.y, Camera.main.transform.position.z);
    }

    // y�� ��ȭ
    public void ResizeYMap(int value)
    {
        if(value > 0)
        {
            int y = groundList.Count;
            groundList.Add(new List<BlockMK2>());
            for(int i = 0; i < groundList[0].Count; i++)
            {
                BlockMK2 go = Instantiate(_blockPrefab, _blockParent);

                InitBlock(go, (int)EBlock.grass);

                go.transform.localPosition = new Vector3(i - _minX, 0, y - _minY);
                groundList[groundList.Count - 1].Add(go);
            }
        }
        else if(value < 0)
        {
            for (int i = 0; i < groundList[0].Count; i++)
            {
                BlockMK2 go = groundList[groundList.Count - 1][i];
                Destroy(go.gameObject);
            }
            groundList.RemoveAt(groundList.Count - 1);
        }
        _mainCam.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, (groundList.Count - _defaultY) / 2 + 2);
    }

    private void Update()
    {
        if(isInteract)
        {
            RaycastHit hit;
            if (Physics.Raycast(_mainCam.ScreenPointToRay(Input.mousePosition), out hit, 1000f, 1 << LayerMask.NameToLayer("Block")))
            {
                int x = Mathf.RoundToInt(hit.point.x);
                int z = Mathf.RoundToInt(hit.point.z);

                _target.position = new Vector3(x, 10f, z);


                if (Input.GetMouseButton(0))
                {
                    if (isCreatureMode)
                    {
                        Vector2Int pos = new Vector2Int(x, z);
                        if (_creaturePos[creatureIndex] != pos)
                        {
                            _creatures[creatureIndex].gameObject.SetActive(false);

                            if (groundList[z + _minY][x + _minX].Index != (int)EBlock.grass)
                                InitBlock(groundList[z + _minY][x + _minX], (int)EBlock.grass);

                            _creatures[creatureIndex].gameObject.SetActive(true);
                            _creatures[creatureIndex].position = new Vector3(pos.x, 0.6f, pos.y);
                            _creaturePos[creatureIndex] = pos;
                        }
                    }
                    else
                    {
                        // �ٸ� ���� �ٸ� ������ ĥ���ش�.
                        if (groundList[z + _minY][x + _minX].Index != materialIndex)
                            InitBlock(groundList[z + _minY][x + _minX], materialIndex);
                    }
                }
            }
        }
    }

    // ���� �����ϱ�
    public void SaveMap(string mapName)
    {
        MyArr<int>[] mapData = new MyArr<int>[groundList.Count];
        for(int i = 0; i < groundList.Count; i++)
        {
            mapData[i] = new MyArr<int>(groundList[0].Count);
            for(int j = 0; j < groundList[0].Count; j++)
            {
                mapData[i].arr[j] = groundList[i][j].Index;
            }
        }

        FileManager.MapsData.Add(new MapData(mapName, 0, mapData));

        FileManager.SaveGame();
    }

    // �����
    public void SaveMap(int index)
    {
        MyArr<int>[] mapData = new MyArr<int>[groundList.Count];
        for (int i = 0; i < groundList.Count; i++)
        {
            mapData[i] = new MyArr<int>(groundList[0].Count);
            for (int j = 0; j < groundList[0].Count; j++)
            {
                mapData[i].arr[j] = groundList[i][j].Index;
            }
        }

        // �� ����
        string mapName = FileManager.MapsData.mapsData[index].mapDataName;
        FileManager.MapsData.mapsData[index] = new MapData(mapName, 0, mapData);


        // ũ���� ����
        for(int i = 0; i < _creaturePos.Length; i++)
            FileManager.MapsData.mapsData[index].creaturePos[i] = _creaturePos[i];

        FileManager.SaveGame();
    }

    public void LoadMap(MapData mapData)
    {
        int x = mapData.mapData[0].arr.Length;
        int y = mapData.mapData.Length;

        // ���� ������Ʈ ����
        ClearMap();

        groundList = new List<List<BlockMK2>>();

        _minX = (_defaultX / 2) - 1;
        _minY = (_defaultY / 2) - 1;

        for(int i = 0; i < y; i++)
        {
            groundList.Add(new List<BlockMK2>());
            for (int j = 0; j < x; j++)
            {
                BlockMK2 go = Instantiate(_blockPrefab, _blockParent);
                go.transform.localPosition = new Vector3(j - _minX, 0, i - _minY);
                groundList[i].Add(go);
            }
        }

        for (int i = 0; i < y; i++)
        {
            for (int j = 0; j < x; j++)
            {
                int index = mapData.mapData[i].arr[j];
                InitBlock(groundList[i][j], index);
            }
        }

        // ũ���� �ε�
        for (int i = 0; i < _creatures.Length; i++)
        {
            _creatures[i].gameObject.SetActive(false);
            if (mapData.creaturePos == null)
                continue;
            _creaturePos[i] = mapData.creaturePos[i];
            if(_creaturePos[i] != Vector2Int.zero)
            {
                _creatures[i].gameObject.SetActive(true);
                _creatures[i].position = new Vector3(_creaturePos[i].x, 0.6f, _creaturePos[i].y);
            }
        }

        // ī�޶� ��ġ ����
        _mainCam.transform.position = new Vector3((groundList[0].Count - _defaultX) / 2, Camera.main.transform.position.y, (groundList.Count - _defaultY) / 2 + 2);

        SetHeightMap();
    }

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

        StartCoroutine(InitBlockCo(block, blockMaterial, index, blockTransformerData));
    }

    // �� ������ �Ŀ� �����ؾ� ����� �۵���
    private IEnumerator InitBlockCo(BlockMK2 block, Material blockMaterial, int index, BlockTransformerData blockTransformerData)
    {
        yield return null;
        block.Init(index, blockMaterial, _itemPrefabData.itemPrefabs[index], blockTransformerData);
        
        if(index == (int)EBlock.water)
        {
            // �����̸� _waterFallUp, �����̸� _waterFallDown ����
            if (block.transform.localPosition.z == -9)
                Instantiate(_waterFallDown.gameObject, block.transform);
            else if (block.transform.localPosition.z == groundList.Count - 10)
                Instantiate(_waterFallUp.gameObject, block.transform);
        }   
    }

    // ��ư���� �Ҳ���
    public void SetHeightMap()
    {
        int x = groundList[0].Count;
        int y = groundList.Count;
        for (int i = 0; i < y; i++)
        {
            for(int j = 0; j < x; j++)
            {
                if(groundList[i][j].Index == (int)EBlock.blackRock)
                {
                    int height = groundList[i][j].GetHeight();
                    Transform child = groundList[i][j].transform.GetChild(0);
                    child.localScale = Vector3.one + Vector3.up * height * 0.3f;
                }
            }
        }
    }

    // ��ư�� �޲���
    public void SetBorder()
    {
        _borderParent.DestroyAllChild();

        int width = groundList[0].Count;
        int height = groundList.Count;

        GameObject emptyPrefab = _barricade;

        int minX = Mathf.RoundToInt(groundList[0][0].transform.position.x);
        int maxX = Mathf.RoundToInt(groundList[height - 1][width - 1].transform.position.x);

        int minY = Mathf.RoundToInt(groundList[0][0].transform.position.z);
        int maxY = Mathf.RoundToInt(groundList[height - 1][width - 1].transform.position.z);

        // ���� ����
        for(int i = minX; i <= maxX; i++)
        {
            Vector3 pos = new Vector3(i, 0.5f, minY - 1);
            Instantiate(emptyPrefab, pos, Quaternion.identity, _borderParent);
        }

        // ū ����
        for(int i = minX; i <= maxX; i++)
        {
            Vector3 pos = new Vector3(i, 0.5f, maxY + 1);
            Instantiate(emptyPrefab, pos, Quaternion.identity, _borderParent);
        }

        // ���� ������
        for(int i = minY; i <= maxY; i++)
        {
            Vector3 pos = new Vector3(minX - 1, 0.5f, i);
            Instantiate(emptyPrefab, pos, Quaternion.identity, _borderParent);
        }

        // ū ����
        for(int i = minY; i <= maxY; i++)
        {
            Vector3 pos = new Vector3(maxX + 1, 0.5f, i);
            Instantiate(emptyPrefab, pos, Quaternion.identity, _borderParent);
        }
    }

    public void LoadSceneLobby()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
