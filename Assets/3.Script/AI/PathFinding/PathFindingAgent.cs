using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFindingAgent : MonoBehaviour
{
    public float moveSpeed;
    public float rotateSpeed;
    public float randomRange;

    [SerializeField] private LayerMask throwLayer;  // 물, empty
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

    // 해당 위치로 이동하는 메소드
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

    // 랜덤 위치로 이동하는 메소드
    public void MoveToRandomPosition()
    {
        Vector2 randomPos = Random.insideUnitCircle;
        Vector3 targetPos = transform.position + new Vector3(randomPos.x, 0f, randomPos.y) * randomRange;

        MoveTo(targetPos);
    }

    // 가장 가까운 버릴 수 있는 위치 찾아서 이동 (회전까지 덤으로)
    public Vector3 MoveToClosestEndPosition()
    {
        Vector3 cloestEndPosition = FindCloestEndPosition();
        Vector3 targetPos = FindCloestAroundEndPosition(cloestEndPosition);
        MoveTo(targetPos, true);
        return targetPos;
    }

    // 갈 수 없는 가장 가까운 곳 반환
    private Vector3 FindCloestEndPosition()
    {
        // overlapSphere 반지름을 조금씩 늘리면서 가장 먼저 나온 곳을 찾아서 이동
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
                            Debug.Log("으아아아ㅏ아아아 "+ hitColliders[i].transform.position);
                            return hitColliders[i].transform.position;
                        }
                    }
                }
            }

        }

        return hitColliders[0].transform.position;
    }

    // 갈 수 없는 가장 가까운 곳을 받으면 그 주위에 가장 가까운 갈 수 있는 지점을 반환
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

    // 경로찾기
    private bool PathFinding(Vector3 targetPos)
    {
        // 크기 정해주고 isWall, x, y대입
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

        // 범위 밖 타겟이라면 false반환
        if (targetX >= sizeX || targetX < 0 || targetY >= sizeY || targetY < 0)
            return false;

        _startNode = _nodeArray[startY, startX];
        _targetNode = _nodeArray[targetY, targetX];

        // 시작과 끝 노드, 열린리스트와 닫힌리스트, 마지막리스트 초기화
        _openList = new List<Node>() { _startNode };
        _cloesdList = new List<Node>();
        finalNodeList = new List<Node>();

        while (_openList.Count > 0)
        {
            // 열린리스트 중 가장 F가 작고 F가 같다면 H가 작은 걸 현재노드로 하고 열린 리스트에서 닫힌리스트로 옮기기
            _currentNode = _openList[0];
            for (int i = 1; i < _openList.Count; i++)
                if (_openList[i].F <= _currentNode.F && _openList[i].H < _currentNode.H)
                    _currentNode = _openList[i];

            _openList.Remove(_currentNode);
            _cloesdList.Add(_currentNode);

            // 마지막
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

            // ↗↖↙↘
            OpenListAdd(_currentNode.x + 1, _currentNode.y + 1);
            OpenListAdd(_currentNode.x - 1, _currentNode.y + 1);
            OpenListAdd(_currentNode.x - 1, _currentNode.y - 1);
            OpenListAdd(_currentNode.x + 1, _currentNode.y - 1);

            // ↑ → ↓ ←
            OpenListAdd(_currentNode.x, _currentNode.y + 1);
            OpenListAdd(_currentNode.x + 1, _currentNode.y);
            OpenListAdd(_currentNode.x, _currentNode.y - 1);
            OpenListAdd(_currentNode.x - 1, _currentNode.y);
        }

        return false;
    }


    private void OpenListAdd(int checkX, int checkY)
    {
        // 상하좌우 범위를 벗어나지 않고, 벽이 아니면서, 닫힌리스트에 없다면
        if(checkX >= _bottomLeft.x && checkX < _topRight.x + 1 &&
             checkY >= _bottomLeft.y && checkY < _topRight.y + 1 &&
            !_nodeArray[checkY - _bottomLeft.y, checkX - _bottomLeft.x].isWall &&
            !_cloesdList.Contains(_nodeArray[checkY - _bottomLeft.y, checkX - _bottomLeft.x]))
        {
            // 대각선 허용시, 벽 사이로 통과 안됨
            if (_nodeArray[_currentNode.y - _bottomLeft.y, checkX - _bottomLeft.x].isWall && _nodeArray[checkY - _bottomLeft.y, _currentNode.x - _bottomLeft.x].isWall)
                return;

            // 코너를 가로질러 가지 않을시, 이동 중에 수직수평 장애물이 있으면 안됨
            if (_nodeArray[_currentNode.y - _bottomLeft.y, checkX - _bottomLeft.x].isWall || _nodeArray[checkY - _bottomLeft.y, _currentNode.x - _bottomLeft.x].isWall)
                return;

            // 이웃노드에 넣고, 직선은 10, 대각선은 14비용
            Node neighborNode = _nodeArray[checkY - _bottomLeft.y, checkX - _bottomLeft.x];
            int moveCost = _currentNode.G + (_currentNode.x - checkX == 0 || _currentNode.y - checkY == 0 ? 10 : 14);

            // 이동비용이 이웃노드G보다 작거나 또는 열린리스트에 이웃노드가 없다면, G, H, ParentNode를 설정 후 열린리스트에 추가
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
                    // 돌고
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
