using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
    private FlockSettings _settings;


    [HideInInspector]
    public Vector3 Position;
    [HideInInspector]
    public Vector3 Forward;

    //무리 중심 위치를 나타내는 벡터
    [HideInInspector]
    public Vector3 CenterOfFlockmates;
    //무리의 수
    [HideInInspector]
    public int NumPerceivedFlockmates;
    //무리의 진행방향
    [HideInInspector]
    public Vector3 AvgFlockHeading;


    [HideInInspector]
    public Vector3 Center;
    [HideInInspector]
    public Vector3 TargetPosition;
    [HideInInspector]
    public Vector3 AlignmentPosition;




    private Transform _transform;

    private void Awake()
    {
        _transform = transform;

    }
    public void Init(FlockSettings _settings)
    {
        this._settings = _settings;
        Position = _transform.position;
        Forward = _transform.forward;
    }


    public void UpdateFlock()
    {
        Center = Vector3.zero;
        TargetPosition = Vector3.zero;

        //Cohesion
        if (NumPerceivedFlockmates != 0)
        {
            //무리의 중심 위치 / 주위의 무리들 수나누기 
            CenterOfFlockmates /= NumPerceivedFlockmates;
            Center = CenterOfFlockmates;

            //Alignment
            //주위 무리의 방향 / 무리들 수 나누기
            AvgFlockHeading /= NumPerceivedFlockmates;
            AlignmentPosition = AvgFlockHeading;
        }

        if(_transform!=null)
        {
            Position = _transform.position;
            Forward = _transform.forward;

        }

    }

    public bool IsHeadingForCollision()
    {
        //현재 진행 방향이 충돌 가능성이 있는지 검사
        //충돌 가능성이 있다면 회피 동작을 실행할 수 있도록 
        RaycastHit hit;
        if (Physics.SphereCast(Position, _settings.BoundsRadius, Forward, out hit, _settings.CollisionAvoidDst, _settings.ObstacleMask))
        {
            //현재 위치에서 앞쪽으로 구체 발사
            //충돌이 발생하면 true 반환
            return true;
        }
        // 충돌하지 않았다면 false 반환
        else return false;
    }

    public Vector3 ObstacleRays()
    {
        //장애물 회피 
        //여러가지 방향
        Vector3[] rayDirections = BoidHelper.Directions;
        for (int i = 0; i < rayDirections.Length; i++)
        {
            Vector3 dir = _transform.TransformDirection(rayDirections[i]);
            Ray ray = new Ray(Position, dir);
            if (!Physics.SphereCast(ray, _settings.BoundsRadius, _settings.CollisionAvoidDst, _settings.ObstacleMask))
            {
                // 충돌이 발생하지 않는다면 해당 방향 Vector 반환
                return dir;
            }
        }

        //유효한 방향이 없다면 전방 방향 반환 
        return Forward;
    }







}
