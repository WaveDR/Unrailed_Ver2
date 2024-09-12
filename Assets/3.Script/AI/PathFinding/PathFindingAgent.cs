using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFindingAgent : MonoBehaviour
{
    public float moveSpeed;
    public float rotateSpeed;
    public float randomRange;

    [SerializeField] private LayerMask throwLayer;  // ��, empty
    [SerializeField] private LayerMask impassableLayer; 

    private bool _reachedDestination = false;
    public bool AtDestination => _reachedDestination;


    public List<Node> finalNodeList;
    private Vector2Int _bottomLeft, _topRight;
    private Node[,] _nodeArray;
    private Node _startNode, _targetNode, _currentNode;
    private List<Node> _openList, _cloesdList;
    private Animator _anim;
    private Vector3[] dirs = new Vector3[4] {new Vector3(0, 0, 1), new Vector3(1, 0, 0),
                 new Vector3(0, 0, -1), new Vector3(-1, 0, 0)};

    private Coroutine _moveCo = null;

    private void Awake()
    {
        TryGetComponent(out _anim);
    }

    // �ش� ��ġ�� �̵��ϴ� �޼ҵ�
    public bool MoveTo(Vector3 targetPos, bool isLastRotate = false)
    {
        if(PathFinding(targetPos))
        {
            _anim.SetBool("isMove", !isLastRotate);
            if (_moveCo != null)
                StopCoroutine(_moveCo);
            _moveCo = StartCoroutine(MoveCo(isLastRotate));
            return true;
        }
        return false;
    }

    // ���� ��ġ�� �̵��ϴ� �޼ҵ�
    public void MoveToRandomPosition()
    {
        Vector2 randomPos = Random.insideUnitCircle;
        Vector3 targetPos = transform.position + new Vector3(randomPos.x, 0f, randomPos.y) * randomRange;

        MoveTo(targetPos);
    }

    // ���� ����� ���� �� �ִ� ��ġ ã�Ƽ� �̵� (ȸ������ ������)
    public Vector3 MoveToClosestEndPosition()
    {
        Vector3 cloestEndPosition = FindCloestEndPosition();
        Vector3 targetPos = FindCloestAroundEndPosition(cloestEndPosition);
        MoveTo(targetPos, true);
        return targetPos;
    }

    // �� �� ���� ���� ����� �� ��ȯ
    private Vector3 FindCloestEndPosition()
    {
        // overlapSphere �������� ���ݾ� �ø��鼭 ���� ���� ���� ���� ã�Ƽ� �̵�
        float detectRadius = 0;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectRadius, throwLayer);

        bool isFind = false;

        while (!isFind)
        {
            InfiniteLoopDetector.Run();

            detectRadius += 0.1f;
            hitColliders = Physics.OverlapSphere(transform.position, detectRadius, throwLayer);

            for(int i = 0; i < hitColliders.Length; i++)
            {
                for(int j = 0; j < dirs.Length; j++)
                {
                    Vector3 startPos = hitColliders[i].transform.position - Vector3.up * 0.5f;
                    if (Physics.Raycast(startPos, dirs[j], out RaycastHit hit, 1f,  1 << LayerMask.NameToLayer("Block")))
                    {
                        if(hit.transform.childCount == 0)
                        {
                            isFind = true;
                            Debug.Log("���ƾƾƤ��ƾƾ� "+ hitColliders[i].transform.position);
                            return hitColliders[i].transform.position;
                        }
                    }
                }
            }

        }

        return hitColliders[0].transform.position;
    }

    // �� �� ���� ���� ����� ���� ������ �� ������ ���� ����� �� �� �ִ� ������ ��ȯ
    public Vector3 FindCloestAroundEndPosition(Vector3 cloestEndPosition)
    {
        Vector3 result = Vector3.zero;
        float currentDistance = Mathf.Infinity;
        for (int i = 0; i < dirs.Length; i++)
        {
            if (!Physics.Raycast(cloestEndPosition, dirs[i], 1f, impassableLayer))
            {
                float distance = Vector3.Distance(transform.position, cloestEndPosition + dirs[i]);
                if (distance < currentDistance)
                {
                    currentDistance = distance;
                    result = cloestEndPosition + dirs[i];
                }
            }
        }
        return result;
    }

    // ���ã��
    private bool PathFinding(Vector3 targetPos)
    {
        // ũ�� �����ְ� isWall, x, y����
        int sizeX = PathFindingField.Instance.Width;
        int sizeY = PathFindingField.Instance.Height;
        int minX = PathFindingField.Instance.MinX;
        int minY = PathFindingField.Instance.MinY;

        _bottomLeft = new Vector2Int(minX, minY);
        _topRight = new Vector2Int(minX + sizeX - 1, minY + sizeY - 1);

        _nodeArray = new Node[sizeY, sizeX];

        for(int i = 0; i < sizeY; i++)
        {
            for(int j = 0; j < sizeX; j++)
            {
                bool isWall = !PathFindingField.Instance.GetMapData(j, i);
                _nodeArray[i, j] = new Node(isWall, j + minX, i + minY); 
            }
        }

        int startX = Mathf.RoundToInt(transform.position.x) - _bottomLeft.x;
        int startY = Mathf.RoundToInt(transform.position.z) - _bottomLeft.y;
        int targetX = Mathf.RoundToInt(targetPos.x) - _bottomLeft.x;
        int targetY = Mathf.RoundToInt(targetPos.z) - _bottomLeft.y;

        // ���� �� Ÿ���̶�� false��ȯ
        if (targetX >= sizeX || targetX < 0 || targetY >= sizeY || targetY < 0)
            return false;

        _startNode = _nodeArray[startY, startX];
        _targetNode = _nodeArray[targetY, targetX];

        // ���۰� �� ���, ��������Ʈ�� ��������Ʈ, ����������Ʈ �ʱ�ȭ
        _openList = new List<Node>() { _startNode };
        _cloesdList = new List<Node>();
        finalNodeList = new List<Node>();

        while (_openList.Count > 0)
        {
            // ��������Ʈ �� ���� F�� �۰� F�� ���ٸ� H�� ���� �� ������� �ϰ� ���� ����Ʈ���� ��������Ʈ�� �ű��
            _currentNode = _openList[0];
            for (int i = 1; i < _openList.Count; i++)
                if (_openList[i].F <= _currentNode.F && _openList[i].H < _currentNode.H)
                    _currentNode = _openList[i];

            _openList.Remove(_currentNode);
            _cloesdList.Add(_currentNode);

            // ������
            if (_currentNode == _targetNode)
            {
                Node targetCurrentNode = _targetNode;
                while(targetCurrentNode != _startNode)
                {
                    finalNodeList.Add(targetCurrentNode);
                    targetCurrentNode = targetCurrentNode.ParentNode;
                }
                finalNodeList.Add(_startNode);
                finalNodeList.Reverse();

                return true;
            }

            // �֢آע�
            OpenListAdd(_currentNode.x + 1, _currentNode.y + 1);
            OpenListAdd(_currentNode.x - 1, _currentNode.y + 1);
            OpenListAdd(_currentNode.x - 1, _currentNode.y - 1);
            OpenListAdd(_currentNode.x + 1, _currentNode.y - 1);

            // �� �� �� ��
            OpenListAdd(_currentNode.x, _currentNode.y + 1);
            OpenListAdd(_currentNode.x + 1, _currentNode.y);
            OpenListAdd(_currentNode.x, _currentNode.y - 1);
            OpenListAdd(_currentNode.x - 1, _currentNode.y);
        }

        return false;
    }


    private void OpenListAdd(int checkX, int checkY)
    {
        // �����¿� ������ ����� �ʰ�, ���� �ƴϸ鼭, ��������Ʈ�� ���ٸ�
        if(checkX >= _bottomLeft.x && checkX < _topRight.x + 1 &&
             checkY >= _bottomLeft.y && checkY < _topRight.y + 1 &&
            !_nodeArray[checkY - _bottomLeft.y, checkX - _bottomLeft.x].isWall &&
            !_cloesdList.Contains(_nodeArray[checkY - _bottomLeft.y, checkX - _bottomLeft.x]))
        {
            // �밢�� ����, �� ���̷� ��� �ȵ�
            if (_nodeArray[_currentNode.y - _bottomLeft.y, checkX - _bottomLeft.x].isWall && _nodeArray[checkY - _bottomLeft.y, _currentNode.x - _bottomLeft.x].isWall)
                return;

            // �ڳʸ� �������� ���� ������, �̵� �߿� �������� ��ֹ��� ������ �ȵ�
            if (_nodeArray[_currentNode.y - _bottomLeft.y, checkX - _bottomLeft.x].isWall || _nodeArray[checkY - _bottomLeft.y, _currentNode.x - _bottomLeft.x].isWall)
                return;

            // �̿���忡 �ְ�, ������ 10, �밢���� 14���
            Node neighborNode = _nodeArray[checkY - _bottomLeft.y, checkX - _bottomLeft.x];
            int moveCost = _currentNode.G + (_currentNode.x - checkX == 0 || _currentNode.y - checkY == 0 ? 10 : 14);

            // �̵������ �̿����G���� �۰ų� �Ǵ� ��������Ʈ�� �̿���尡 ���ٸ�, G, H, ParentNode�� ���� �� ��������Ʈ�� �߰�
            if(moveCost < neighborNode.G || !_openList.Contains(neighborNode))
            {
                neighborNode.G = moveCost;
                neighborNode.H = (Mathf.Abs(neighborNode.x - _targetNode.x) + Mathf.Abs(neighborNode.y - _targetNode.y)) * 10;
                neighborNode.ParentNode = _currentNode;

                _openList.Add(neighborNode);
            }
        }
    }


    private IEnumerator MoveCo(bool isLastRotate = false)
    {
        _reachedDestination = false;
        for(int i = 1; i < finalNodeList.Count; i++)
        {
            Node destNode = finalNodeList[i];
            Vector3 originPosition = transform.position;
            Vector3 destPosition = new Vector3(destNode.x, 0.5f, destNode.y);

            Quaternion originRotation = transform.rotation;
            Vector3 dir = (destPosition - originPosition).normalized;
            Quaternion destRotation = Quaternion.LookRotation(dir);

            float currentTime = 0;
            float totalTime = Vector3.Distance(originPosition, destPosition) <= 1.1f ? 1 : Mathf.Sqrt(2);

            while(Vector3.Distance(transform.position, destPosition) > 0.001f)
            {
                currentTime += Time.deltaTime;

                transform.position = Vector3.Lerp(originPosition, destPosition, moveSpeed * currentTime / totalTime);
                transform.rotation = Quaternion.Slerp(originRotation, destRotation, rotateSpeed * currentTime / totalTime);
                yield return null;
            }
            transform.position = destPosition;
            transform.rotation = destRotation;
        }

        if(isLastRotate)
        {
            Vector3[] dirs = new Vector3[4] { transform.forward, transform.right, -transform.forward, -transform.right };

            float currentTime = 0f;
            for(int i = 0; i < 4; i++)
            {
                if(Physics.Raycast(transform.position, dirs[i],out RaycastHit hit, 1f, throwLayer))
                {
                    Quaternion originRotation = transform.rotation;
                    Vector3 destPosition = new Vector3(hit.transform.position.x, 0.5f, hit.transform.position.z);
                    Vector3 dir = (destPosition - transform.position).normalized;
                    Quaternion destRotation = Quaternion.LookRotation(dir);
                    // ����
                    while (currentTime * moveSpeed < 1)
                    {
                        currentTime += Time.deltaTime;
                        transform.rotation = Quaternion.Slerp(originRotation, destRotation, rotateSpeed * currentTime);
                        yield return null;
                    }
                    break;
                }
            }
        }

        _reachedDestination = true;
    }

    private void OnDrawGizmos()
    {
        if (finalNodeList.Count != 0) for (int i = 0; i < finalNodeList.Count - 1; i++)
                Gizmos.DrawLine(new Vector3(finalNodeList[i].x, 0.5f , finalNodeList[i].y), new Vector3(finalNodeList[i + 1].x, 0.5f, finalNodeList[i + 1].y));
    }
}
