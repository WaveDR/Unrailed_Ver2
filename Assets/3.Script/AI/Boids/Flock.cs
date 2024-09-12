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

    //���� �߽� ��ġ�� ��Ÿ���� ����
    [HideInInspector]
    public Vector3 CenterOfFlockmates;
    //������ ��
    [HideInInspector]
    public int NumPerceivedFlockmates;
    //������ �������
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
            //������ �߽� ��ġ / ������ ������ �������� 
            CenterOfFlockmates /= NumPerceivedFlockmates;
            Center = CenterOfFlockmates;

            //Alignment
            //���� ������ ���� / ������ �� ������
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
        //���� ���� ������ �浹 ���ɼ��� �ִ��� �˻�
        //�浹 ���ɼ��� �ִٸ� ȸ�� ������ ������ �� �ֵ��� 
        RaycastHit hit;
        if (Physics.SphereCast(Position, _settings.BoundsRadius, Forward, out hit, _settings.CollisionAvoidDst, _settings.ObstacleMask))
        {
            //���� ��ġ���� �������� ��ü �߻�
            //�浹�� �߻��ϸ� true ��ȯ
            return true;
        }
        // �浹���� �ʾҴٸ� false ��ȯ
        else return false;
    }

    public Vector3 ObstacleRays()
    {
        //��ֹ� ȸ�� 
        //�������� ����
        Vector3[] rayDirections = BoidHelper.Directions;
        for (int i = 0; i < rayDirections.Length; i++)
        {
            Vector3 dir = _transform.TransformDirection(rayDirections[i]);
            Ray ray = new Ray(Position, dir);
            if (!Physics.SphereCast(ray, _settings.BoundsRadius, _settings.CollisionAvoidDst, _settings.ObstacleMask))
            {
                // �浹�� �߻����� �ʴ´ٸ� �ش� ���� Vector ��ȯ
                return dir;
            }
        }

        //��ȿ�� ������ ���ٸ� ���� ���� ��ȯ 
        return Forward;
    }







}
