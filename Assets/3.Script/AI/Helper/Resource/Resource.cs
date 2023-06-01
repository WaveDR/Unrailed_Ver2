using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Resource : MonoBehaviour
{
    public Transform spawnPoint;
    int _first = 0;
    Helper Robot;

    [SerializeField] WorldResource.EType DefaultResource = WorldResource.EType.Wood;
    Dictionary<WorldResource.EType, List<WorldResource>> TrackedResources = null;

    [SerializeField] float PerfectKnowledgeRange = 30f;
    [SerializeField] int GatherPickRange = 10;

    public int NumAvailableResources { get; private set; } = 0;

    void Awake()
    {
        //���߿� �κ� ���� ����� �ٲٱ�
        Robot = FindObjectOfType<Helper>();
        Robot.SetHome(this);
    }

    private void Start()
    {
        PopulateResources();
    }

    private void Update()
    {
        if (TrackedResources == null)
            PopulateResources();
    }



    void PopulateResources()
    {
        //�ڿ� ����
        var resourceTypes = System.Enum.GetValues(typeof(WorldResource.EType));
        TrackedResources = new Dictionary<WorldResource.EType, List<WorldResource>>();
        foreach (var value in resourceTypes)
        {
            var type = (WorldResource.EType)value;
            TrackedResources[type] = ResourceTracker.Instance.GetResourcesInRange(type, transform.position, PerfectKnowledgeRange);
            //�� �ڿ���
            NumAvailableResources += TrackedResources[type].Count;
        }
    }

    public WorldResource GetGatherTarget(Helper brain)
    {
        //�ڿ� ������Ʈ
        PopulateResources();
        WorldResource.EType targetResource = DefaultResource;
        var resourceTypes = System.Enum.GetValues(typeof(WorldResource.EType));

        foreach (var typeValue in resourceTypes)
        {
            var resourceType = (WorldResource.EType)typeValue;
            //�����̶� ������ Ȯ��
            if(resourceType==brain.TargetResource)
            {
                targetResource = resourceType;
                break;
            }
        }
        //�ڿ��� �ִ��� Ȯ��
        if (TrackedResources[targetResource].Count == 0)
        {
            Debug.Log($"{targetResource} :  �� ĺ���");
        }

        var sortedResources = TrackedResources[targetResource]
            //�� �� �ִ� ���� �߸���
            .Where(resource => Vector3.Distance(resource.transform.position, brain.GetComponent<PathFindingAgent>().
                                                FindCloestAroundEndPosition(resource.transform.position)) <= 1f)
            //����� ������ ����
            .OrderBy(resource => Vector3.Distance(brain.transform.position, resource.transform.position))
            .First();

        return sortedResources;

    }

}