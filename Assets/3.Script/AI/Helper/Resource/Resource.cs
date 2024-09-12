using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Resource : MonoBehaviour
{
    public bool NonethisResourceTypeHelper { get; private set; } = false;
    public bool NonethisResourceTypeEnemy { get; private set; } = false;

    private WorldResource.EType _defaultResource = WorldResource.EType.Wood;
    private Dictionary<WorldResource.EType, List<WorldResource>> _trackedResources = null;
    private float _range = 20;

    private void Start()
    {
        PopulateResources();
    }

    private void Update()
    {
        if(_trackedResources==null)
        {
            PopulateResources();
        }
    }

    public void PopulateResources()
    {
        //자원 세팅
        var resourceTypes = System.Enum.GetValues(typeof(WorldResource.EType));
        _trackedResources = new Dictionary<WorldResource.EType, List<WorldResource>>();
        foreach (var value in resourceTypes)
        {
            var type = (WorldResource.EType)value;
            _trackedResources[type] = ResourceTracker.Instance.GetResourcesInRange(type, transform.position, _range);
        }
    }

    public WorldResource GetGatherTarget(Helper brain)
    {
        //자원 업데이트
        PopulateResources();
        WorldResource.EType targetResource = _defaultResource;
        var resourceTypes = System.Enum.GetValues(typeof(WorldResource.EType));

        foreach (var typeValue in resourceTypes)
        {
            var resourceType = (WorldResource.EType)typeValue;
            //명령이랑 같은지 확인
            if(resourceType==brain.TargetResource)
            {
                targetResource = resourceType;
                break;
            }
        }
        //자원이 있는지 확인
        if (_trackedResources[targetResource].Count <1)
        {
            NonethisResourceTypeHelper = true;
            Debug.Log($"{targetResource} :  자원이 이제 없어요");
        }
        else
            NonethisResourceTypeHelper = false;

        var sortedResources = _trackedResources[targetResource]
            //갈 수 있는 곳에 있는 자원만 추리기
            .Where(resource => Vector3.Distance(resource.transform.position, brain.Agent.
                                                FindCloestAroundEndPosition(resource.transform.position)) <= 1.5f)
            //가까운 순으로 정렬
            .OrderBy(resource => Vector3.Distance(brain.transform.position, resource.transform.position))
            //가장 가까운 자원 반환
            .FirstOrDefault();

        return sortedResources;
    }


    public WorldResource TargettoSteal(PathFindingAgent brain)
    {
        //자원 업데이트
        PopulateResources();
        WorldResource.EType targetResource = WorldResource.EType.Resource;
        //자원이 있는지 확인
        if (_trackedResources[targetResource].Count < 1)
        {
            NonethisResourceTypeEnemy = true;
            Debug.Log($"{targetResource} :  훔칠 자원이 이제 없어요");
        }
        else
            NonethisResourceTypeEnemy = false;

        var sortedResources = _trackedResources[targetResource]
            //갈 수 있는 곳에 있는 자원만 추리기
            .Where(resource => Vector3.Distance(resource.transform.position, brain.
                                                FindCloestAroundEndPosition(resource.transform.position)) <= 1.5f)
            //가까운 순으로 정렬
            .OrderBy(resource => Vector3.Distance(brain.transform.position, resource.transform.position))
            //가장 가까운 자원 반환
            .FirstOrDefault();
        return sortedResources;
    }



    public WorldResource ResearchTarget(Helper brain)
    {
        //갈 수 있는 곳이 없다면 실행
        WorldResource.EType targetResource = _defaultResource;
        var resourceTypes = System.Enum.GetValues(typeof(WorldResource.EType));

        foreach (var typeValue in resourceTypes)
        {
            var resourceType = (WorldResource.EType)typeValue;
            //명령이랑 같은지 확인
            if (resourceType == brain.TargetResource)
            {
                targetResource = resourceType;
                break;
            }
        }
        //자원이 있는지 확인
        
        var sortedResources = _trackedResources[targetResource]
            .OrderBy(resource => Vector3.Distance(brain.transform.position, resource.transform.position))
            .Take(20)
            .Where(resource => Vector3.Distance
            (brain.Agent.FindCloestAroundEndPosition(resource.transform.position), resource.transform.position)<1.5f)
            .Where(resource=> brain.Agent.MoveTo(brain.Agent.FindCloestAroundEndPosition(resource.transform.position)))
            //가장 가까운 자원 반환
            .FirstOrDefault();

        return sortedResources;
    }

}
