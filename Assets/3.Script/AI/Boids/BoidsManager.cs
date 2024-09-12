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

            //CPU의 작업을 GPU로 전환
            var boidBuffer = new ComputeBuffer(numFlock, FlockData.Size);
            boidBuffer.SetData(flockData);

            //메모리 공간 설정 (커널 0번에 boids 라는 이름으로 boidBuffer을 넣는다)
            Compute.SetBuffer(0, "boids", boidBuffer);

            //boid의 수
            Compute.SetInt("numBoids", _flock.Length);

            //무리를 인식하는 범위
            Compute.SetFloat("viewRadius", Settings.PerceptionRadius);

            // 충돌을 피하기 위해 인식하는 범위
            Compute.SetFloat("avoidRadius", Settings.AvoidanceRadius);

            //boid의 수 / 스레드 그룹 사이즈
            //numBoids * numBoids 한 값을 스레드 수로 나눠서 병렬처리 할 수 있게 함
            //나눈 값을 올림한 값 = threadGroups

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

            //버퍼 해제
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
