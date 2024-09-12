using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WorldManager : MonoBehaviour
{
    [Header("Map 관련")]
    // 맵 뿐만 아니라 전체를 관리하는 클래스
    [SerializeField] private MapCreator _mapCreator;
    [SerializeField] private MapSlicer _mapSlicer;

    [Header("오브젝트 관련")]
    // 플레이어, 기차, ai
    [SerializeField] private Transform[] _creatures;

    [SerializeField] private GameObject _trainPrefab;
    // aiaiai

    // 총 월드의 개수
    private int _worldCount = 0;
    private Transform[] parentTransform;



    // 전체 맵 저장
    public List<List<BlockMK2>>[] entireMap { get; private set; } = new List<List<BlockMK2>>[2];
    // 전체 맵 번들 저장
    public List<BlockBundle>[] entireMapBlockBundle { get; private set; } = new List<BlockBundle>[2];
    // 전체 월드오브젝트 저장 (플레이어, 기차, ai)
    public List<List<Transform>> worldObject { get; private set; } = new List<List<Transform>>();
    // 역들을 보관할 리스트
    public List<Station> stations { get; private set; } = new List<Station>();
    // 맵 사이 바리케이드
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
        // 맵 싸그리 생성
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
            // 부모의 위치 설정
            float width = mapData[i].mapData[0].arr.Length;
            Vector3 parentPosition = Vector3.right * width * i;
            parentTransform[i] = new GameObject("World " + i).transform;
            parentTransform[i].position = parentPosition;

            entireMap[i] = await _mapCreator.CreateMapAsync(mapData[i], parentTransform[i], i == 0);
        }

        // 바리케이드 설정
        betweenBarricadeTransform = _mapCreator.SetBarricade(parentTransform[0].GetChild(0), 
            parentTransform[parentTransform.Length - 1].GetChild(parentTransform[parentTransform.Length - 1].childCount - 1));

        foreach (Transform barricade in betweenBarricadeTransform)
            barricade.gameObject.AddComponent<ImpassableObject>();

        // 맵을 제외한 오브젝트 초기화 (플레이어, 역, 기차, AI)
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

        // 맵 자르고 초기위치 이동
        for (int i = 0; i < mapData.Length; i++)
        {
            entireMapBlockBundle[i] = await _mapSlicer.SliceMap(entireMap[i]);
        }

        // 프레임 기다리기
        await UniTask.WaitForEndOfFrame(this);
    }

    // 맵 재위치 시키기
    public async UniTask RePositionAsync(int index)
    {
        await _mapCreator.RePositionAsync(entireMapBlockBundle[index]);
    }


    private async UniTask InitWorldObject()
    {
        // 역 초기화 하면서 기차까지 생성!! (역에 기차가 있으니까!!)
        stations = FindObjectsOfType<Station>().OrderBy(elem => elem.transform.position.x).ToList();
        for (int i = 0; i < stations.Count; i++)
        {
            if (i == 0)
                stations[i].InitStation(true, _trainPrefab);
            else
                stations[i].InitStation(false);
        }
        FindObjectOfType<ShopManager>().currentStation = stations[1].transform;




        // 플레이어 생성, 기차 생성, AI 생성

        await UniTask.Yield();
    }
}
