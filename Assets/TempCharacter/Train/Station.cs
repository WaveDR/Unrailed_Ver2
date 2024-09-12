using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Station : MonoBehaviour
{
    [SerializeField] private LayerMask blockLayer;
    [SerializeField] private LayerMask railLayer;

    private Transform _parentBlock;
    private Vector3[] _dir = new Vector3[4] { Vector3.forward, Vector3.right, Vector3.left, Vector3.back };

    public void InitStation(bool isStart = false, GameObject trainPrefab = null)
    {
        _parentBlock = transform.parent;

        if (isStart)
            InitFirstStation(trainPrefab);
        else
            InitNonFirstStation();
    }

    private void InitFirstStation(GameObject trainPrefab)
    {
        gameObject.AddComponent<GoalManager>();

        // �� ó�� ������ �տ� ���� ���� �ִ� ���ϸ� �α�
        Transform railTransform = null;
        RailController rail = null;

        if (Physics.Raycast(_parentBlock.position, Vector3.back, out RaycastHit hit, 1f, blockLayer))
        {
            if(hit.transform.childCount != 0)
            {
                railTransform = hit.transform.GetChild(0);
            }
        }

        while(true)
        {
            rail = railTransform.GetComponent<RailController>();
            rail.isInstance = true;

            if(Physics.Raycast(railTransform.position, Vector3.right, out RaycastHit railHit, 1f, railLayer))
            {
                railTransform = railHit.transform;
            }
            else
            {
                break;
            }
        }

        if (rail != null)
        {
            if(trainPrefab != null)
            {
                Debug.LogWarning("���⼭ ���� �ʱ�ȭ���ָ� ��");
                Transform trainTransform = Instantiate(trainPrefab, rail.transform).transform;
                trainTransform.localPosition = Vector3.left;
                trainTransform.localRotation = Quaternion.identity;
            }

            rail.PutRail();
        }
    }

    private void InitNonFirstStation()
    {
        // �տ� ���ϵ��� endRail ���ֱ�! ������
        Transform railTransform = null;
        RailController rail = null;

        if (Physics.Raycast(_parentBlock.position, Vector3.back, out RaycastHit hit, 1f, blockLayer))
        {
            if (hit.transform.childCount != 0)
            {
                railTransform = hit.transform.GetChild(0);
            }
        }

        while (true)
        {
            rail = railTransform.GetComponent<RailController>();
            rail.isEndRail = true;
            rail.isInstance = true;

            if (Physics.Raycast(railTransform.position, Vector3.right, out RaycastHit railHit, 1f, railLayer))
            {
                railTransform = railHit.transform;
            }
            else
            {
                break;
            }
        }
    }
}
