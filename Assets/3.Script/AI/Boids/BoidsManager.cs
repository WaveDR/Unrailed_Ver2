using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidsManager : MonoBehaviour
{
    public FlockSettings Settings;
    public ComputeShader Compute;

    private const int threadGroupSize = 64;


    private Flock[] _flock;
    public void Init()
    {
        _flock = FindObjectsOfType<Flock>();
        foreach (Flock sheep in _flock)
        {
            sheep.Init(Settings);
        }
    }

    void Update()
    {

        if(_flock != null)
        {
            int numFlock = _flock.Length;
            var flockData = new FlockData[numFlock];

            for (int i = 0; i < _flock.Length; i++)
            {
                flockData[i].Position = _flock[i].Position;
                flockData[i].Direction = _flock[i].Forward;
            }

            //CPU�� �۾��� GPU�� ��ȯ
            var boidBuffer = new ComputeBuffer(numFlock, FlockData.Size);
            boidBuffer.SetData(flockData);

            //�޸� ���� ���� (Ŀ�� 0���� boids ��� �̸����� boidBuffer�� �ִ´�)
            Compute.SetBuffer(0, "boids", boidBuffer);

            //boid�� ��
            Compute.SetInt("numBoids", _flock.Length);

            //������ �ν��ϴ� ����
            Compute.SetFloat("viewRadius", Settings.PerceptionRadius);

            // �浹�� ���ϱ� ���� �ν��ϴ� ����
            Compute.SetFloat("avoidRadius", Settings.AvoidanceRadius);

            //boid�� �� / ������ �׷� ������
            //numBoids * numBoids �� ���� ������ ���� ������ ����ó�� �� �� �ְ� ��
            //���� ���� �ø��� �� = threadGroups

            int threadGroups = Mathf.CeilToInt(numFlock / (float)threadGroupSize);
            Compute.Dispatch(0, threadGroups, 1, 1);
            boidBuffer.GetData(flockData);

            for (int i = 0; i < _flock.Length; i++)
            {
                _flock[i].NumPerceivedFlockmates = flockData[i].NumFlockmates;
                _flock[i].CenterOfFlockmates = flockData[i].FlockCenter;
                _flock[i].AvgFlockHeading = flockData[i].Direction;
                _flock[i].NumPerceivedFlockmates = flockData[i].NumFlockmates;

                _flock[i].UpdateFlock();
            }

            //���� ����
            boidBuffer.Release();

        }
    }


    public struct FlockData
    {
        public Vector3 Position;
        public Vector3 Direction;

        public Vector3 FlockHeading;
        public Vector3 FlockCenter;
        public Vector3 AvoidanceHeading;
        public int NumFlockmates;

        public static int Size
        {
            get
            {
                return sizeof(float) * 3 * 5 + sizeof(int);
            }
        }
    }


}
