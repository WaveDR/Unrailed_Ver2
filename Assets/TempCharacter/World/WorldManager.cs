using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WorldManager : MonoBehaviour
{
    [Header("Map ����")]
    // �� �Ӹ� �ƴ϶� ��ü�� �����ϴ� Ŭ����
    [SerializeField] private MapCreator _mapCreator;
    [SerializeField] private MapSlicer _mapSlicer;

    [Header("������Ʈ ����")]
    // �÷��̾�, ����, ai
    [SerializeField] private Transform[] _creatures;

    [SerializeField] private GameObject _trainPrefab;
    // aiaiai

    // �� ������ ����
    private int _worldCount = 0;
    private Transform[] parentTransform;



    // ��ü �� ����
    public List<List<BlockMK2>>[] entireMap { get; private set; } = new List<List<BlockMK2>>[2];
    // ��ü �� ���� ����
    public List<BlockBundle>[] entireMapBlockBundle { get; private set; } = new List<BlockBundle>[2];
    // ��ü ���������Ʈ ���� (�÷��̾�, ����, ai)
    public List<List<Transform>> worldObject { get; private set; } = new List<List<Transform>>();
    // ������ ������ ����Ʈ
    public List<Station> stations { get; private set; } = new List<Station>();
    // �� ���� �ٸ����̵�
    public List<Transform> betweenBarricadeTransform { get; private set; } = new List<Transform>();

    private MapData mapDataEnemy = null;

    public GameObject enemyObject = null;

    public void SpawnCow()
    {
        Vector3 pos = new Vector3(mapDataEnemy.creaturePos[(int)ECreature.cow].x, 0.5f, mapDataEnemy.creaturePos[(int)ECreature.cow].y);

        for (int cowCount = 0; cowCount < 4; cowCount++)
        {
            Vector3 cowPos = pos + Random.insideUnitSphere * 4;
            Transform obj = Instantiate(_creatures[(int)ECreature.cow], parentTransform[1]);
            obj.localPosition = new Vector3(cowPos.x, 0.5f, cowPos.z);
            obj.localRotation = Quaternion.identity;
            obj.parent = null;
        }

        FindObjectOfType<BoidsManager>().Init();
    }

    public async UniTask GenerateWorld(bool isTest)
    {
        // �� �α׸� ����
        MapData[] mapData = new MapData[2];
        _worldCount = mapData.Length;
        parentTransform = new Transform[_worldCount];

        if (isTest)
        {
            mapData[0] = FileManager.MapsData.mapsData[3];
            mapData[1] = FileManager.MapsData.mapsData[4];
        }
        else
        {
            mapData[0] = FileManager.MapsData.mapsData[5];
            mapData[1] = FileManager.MapsData.mapsData[7];
        }

        mapDataEnemy = mapData[1];

        for (int i = 0; i < _worldCount; i++)
        {
            // �θ��� ��ġ ����
            float width = mapData[i].mapData[0].arr.Length;
            Vector3 parentPosition = Vector3.right * width * i;
            parentTransform[i] = new GameObject("World " + i).transform;
            parentTransform[i].position = parentPosition;

            entireMap[i] = await _mapCreator.CreateMapAsync(mapData[i], parentTransform[i], i == 0);
        }

        // �ٸ����̵� ����
        betweenBarricadeTransform = _mapCreator.SetBarricade(parentTransform[0].GetChild(0), 
            parentTransform[parentTransform.Length - 1].GetChild(parentTransform[parentTransform.Length - 1].childCount - 1));

        foreach (Transform barricade in betweenBarricadeTransform)
            barricade.gameObject.AddComponent<ImpassableObject>();

        // ���� ������ ������Ʈ �ʱ�ȭ (�÷��̾�, ��, ����, AI)
        await InitWorldObject();


        for (int i = 0; i < mapData.Length; i++)
        {
            for(int j = 0; j < _creatures.Length; j++)
            {
                Vector3 pos = new Vector3(mapData[i].creaturePos[j].x, 0.5f, mapData[i].creaturePos[j].y);
                if (mapData[i].creaturePos[j] == Vector2Int.zero)
                    continue;

                if(j != (int)ECreature.cow)
                {
                    Transform obj = Instantiate(_creatures[j], parentTransform[i]);
                    obj.localPosition = pos;
                    obj.localRotation = Quaternion.identity;

                    BaseAI ai = obj.GetComponent<BaseAI>();
                    if (ai != null)
                        ai.SetHome(FindObjectOfType<Resource>());
                    obj.parent = null;

                    if (j == (int)ECreature.enemy)
                        enemyObject = obj.gameObject;
                }
            }
        }
        // FindObjectOfType<BoidsManager>().Init();

        // �� �ڸ��� �ʱ���ġ �̵�
        for (int i = 0; i < mapData.Length; i++)
        {
            entireMapBlockBundle[i] = await _mapSlicer.SliceMap(entireMap[i]);
        }

        // ������ ��ٸ���
        await UniTask.WaitForEndOfFrame(this);
    }

    // �� ����ġ ��Ű��
    public async UniTask RePositionAsync(int index)
    {
        await _mapCreator.RePositionAsync(entireMapBlockBundle[index]);
    }


    private async UniTask InitWorldObject()
    {
        // �� �ʱ�ȭ �ϸ鼭 �������� ����!! (���� ������ �����ϱ�!!)
        stations = FindObjectsOfType<Station>().OrderBy(elem => elem.transform.position.x).ToList();
        for (int i = 0; i < stations.Count; i++)
        {
            if (i == 0)
                stations[i].InitStation(true, _trainPrefab);
            else
                stations[i].InitStation(false);
        }
        FindObjectOfType<ShopManager>().currentStation = stations[1].transform;




        // �÷��̾� ����, ���� ����, AI ����

        await UniTask.Yield();
    }
}
