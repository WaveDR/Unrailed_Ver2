using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ������ ����, ö, ������ �̷���
// ������ ����, ���� �̷���
// ������ ���Ϸ� �Ǵ�����

public enum EImpassableObjectType { staticObject, dynamicObject, train, trainEnd };
public enum EImpassableDir { none, forward, back, right, left };

public class ImpassableObject : MonoBehaviour
{
    // PathFindingField�� awake���� �����ǹǷ� start���� ����
    [SerializeField] private EImpassableObjectType impassableType = EImpassableObjectType.staticObject;
    [SerializeField] private EImpassableDir impassableDir = EImpassableDir.none;
    [SerializeField] private int length = 1;

    private Vector3 prevPos;

    private void Start()
    {
        prevPos = transform.position;

        UpdateMapData(false);
    }

    // �� ��ü�� �ı��Ǹ� PathFindingField���� �ش� ��ǥ�� �� �� �ִ°����� ����
    private void OnDestroy()
    {
        UpdateMapData(true);
    }

    private void Update()
    {
        if (impassableType == EImpassableObjectType.dynamicObject)
        {
            if (!isEqualPos())
            {
                UpdateMapData(true);
                prevPos = transform.position;
                UpdateMapData(false);
            }
        }

        else if (impassableType == EImpassableObjectType.train)
        {
            if (!isEqualPos())
            {
                prevPos = transform.position;
                UpdateMapData(false);
            }
        }

        else if (impassableType == EImpassableObjectType.trainEnd)
        {
            if (!isEqualPos())
            {
                UpdateMapData(true);
                prevPos = transform.position;
            }
        }
    }

    // ��ġ�� ������ true, �ƴϸ� flase
    private bool isEqualPos()
    {
        Vector3Int pos1 = new Vector3Int(Mathf.RoundToInt(prevPos.x), 0, Mathf.RoundToInt(prevPos.z));
        Vector3Int pos2 = new Vector3Int(Mathf.RoundToInt(transform.position.x), 0, Mathf.RoundToInt(transform.position.z));

        return pos1.Equals(pos2);
    }

    // ����� ���̿� ���� mapData�� ����
    private void UpdateMapData(bool flag)
    {
        // ���� ���� �� PathFindingField�� ���� ������� ���⼭ ������ ���� ���ϱ� ����ó�� �ؾ� ��
        if (PathFindingField.Instance == null)
            return;

        switch (impassableDir)
        {
            case EImpassableDir.none:
                PathFindingField.Instance.UpdateMapData(prevPos.x, prevPos.z, flag);
                break;
            case EImpassableDir.forward:
                for (int i = 0; i < length; i++)
                {
                    Vector3 pos = prevPos + transform.forward * i;
                    PathFindingField.Instance.UpdateMapData(pos.x, pos.z, flag);
                }
                break;
            case EImpassableDir.back:
                for (int i = 0; i < length; i++)
                {
                    Vector3 pos = prevPos + -transform.forward * i;
                    PathFindingField.Instance.UpdateMapData(pos.x, pos.z, flag);
                }
                break;
            case EImpassableDir.right:
                for (int i = 0; i < length; i++)
                {
                    Vector3 pos = prevPos + transform.right * i;
                    PathFindingField.Instance.UpdateMapData(pos.x, pos.z, flag);
                }
                break;
            case EImpassableDir.left:
                for (int i = 0; i < length; i++)
                {
                    Vector3 pos = prevPos + -transform.right * i;
                    PathFindingField.Instance.UpdateMapData(pos.x, pos.z, flag);
                }
                break;
        }
    }
}
