using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFindingAgent : MonoBehaviour
{
    public float moveSpeed;
    public float rotateSpeed;
    public float randomRange;

    public List<Node> finalNodeList;
    private Vector2Int _bottomLeft, _topRight;
    private Node[,] _nodeArray;
    private Node _startNode, _targetNode, _currentNode;
    private List<Node> _openList, _cloesdList;

    private Coroutine _moveCo = null;

    // �ش� ��ġ�� �̵��ϴ� �޼ҵ�
    public void MoveTo(Vector3 targetPos)
    {
        if(PathFinding(targetPos))
        {
            if (_moveCo != null)
                StopCoroutine(_moveCo);
            _moveCo = StartCoroutine(MoveCo());
        }
    }

    public void MoveToRandomPosition()
    {
        Vector2 randomPos = Random.insideUnitCircle;
        Vector3 targetPos = transform.position + new Vector3(randomPos.x, 0f, randomPos.y) * randomRange;

        if(PathFinding(targetPos))
        {
            if (_moveCo != null)
                StopCoroutine(_moveCo);
            _moveCo = StartCoroutine(MoveCo());
        }
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

        while(_openList.Count > 0)
        {
            // ��������Ʈ �� ���� F�� �۰� F�� ���ٸ� H�� ���� �� ������� �ϰ� ���� ����Ʈ���� ��������Ʈ�� �ű��
            _currentNode = _openList[0];
            for(int i = 1; i < _openList.Count; i++)
                if (_openList[i].F <= _currentNode.F && _openList[i].H < _currentNode.H)
                    _currentNode = _openList[i];

            _openList.Remove(_currentNode);
            _cloesdList.Add(_currentNode);

            // ������
            if(_currentNode == _targetNode)
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


    private IEnumerator MoveCo()
    {
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
    }

    private void OnDrawGizmos()
    {
        if (finalNodeList.Count != 0) for (int i = 0; i < finalNodeList.Count - 1; i++)
                Gizmos.DrawLine(new Vector3(finalNodeList[i].x, 0.5f , finalNodeList[i].y), new Vector3(finalNodeList[i + 1].x, 0.5f, finalNodeList[i + 1].y));
    }
}