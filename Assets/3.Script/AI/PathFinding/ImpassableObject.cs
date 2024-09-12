using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 정적은 나무, 철, 정류장 이런거
// 동적은 동물, 도적 이런거
// 기차는 레일로 판단하자

public enum EImpassableObjectType { staticObject, dynamicObject, train, trainEnd };
public enum EImpassableDir { none, forward, back, right, left };

public class ImpassableObject : MonoBehaviour
{
    // PathFindingField는 awake에서 생성되므로 start에서 실행
    [SerializeField] private EImpassableObjectType impassableType = EImpassableObjectType.staticObject;
    [SerializeField] private EImpassableDir impassableDir = EImpassableDir.none;
    [SerializeField] private int length = 1;

    private Vector3 prevPos;

    private void Start()
    {
        prevPos = transform.position;

        UpdateMapData(false);
    }

    // 이 물체가 파괴되면 PathFindingField에서 해당 좌표를 갈 수 있는곳으로 변경
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

    // 위치가 같으면 true, 아니면 flase
    private bool isEqualPos()
    {
        Vector3Int pos1 = new Vector3Int(Mathf.RoundToInt(prevPos.x), 0, Mathf.RoundToInt(prevPos.z));
        Vector3Int pos2 = new Vector3Int(Mathf.RoundToInt(transform.position.x), 0, Mathf.RoundToInt(transform.position.z));

        return pos1.Equals(pos2);
    }

    // 방향과 길이에 따라 mapData를 갱신
    private void UpdateMapData(bool flag)
    {
        // 게임 종료 시 PathFindingField가 먼저 사라지면 여기서 오류가 많이 나니까 예외처리 해야 함
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
