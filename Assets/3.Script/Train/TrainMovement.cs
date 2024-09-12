using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public enum TrainType { Engine, WaterBox, ChestBox, WorkBench, StationDir, Dynamite, Spare }
public class TrainMovement : MonoBehaviour
{
    public TrainType trainType;

    public int trainNum;
   
    public LinkedList<RailController> rails = new LinkedList<RailController>();
    public List<RailController> listToQue = new List<RailController>();
    public Transform trainMesh;
    public int trainUpgradeLevel;

    public Cinemachine.CinemachineVirtualCamera[] cameraTarget;
    [SerializeField] private int targetCount;
    [SerializeField] protected GameObject destroyParticle;
    public ParticleSystem fireEffect;

    public bool isGoal;
    public bool isBurn;
    public bool isReady;
    public bool isTrainOver;
    protected bool _isPlay;

    //게임오버
    public bool isOver;

    public float trainSpeed = 0.5f;
    public float _trainRotateSpeed;

    public float goalSpeed = 6f;
    public float overSpeed = 0.1f;
    public float _trainMoveSpeed;

    [SerializeField] protected GameObject warningIcon;
    public Animator overText;
    private InGameScene manager;

    // Start is called before the first frame update
    //Transform startRayPos;
    protected void GetMesh()
    {
        if (trainType != TrainType.Spare)
        {
            trainMesh = transform.GetChild(0).GetComponent<Transform>();
            destroyParticle.SetActive(false);
            trainMesh.gameObject.SetActive(true);
            isReady = true;
            trainUpgradeLevel = 1;
            fireEffect = GetComponentInChildren<ParticleSystem>();

            // 조건이 잘못된듯 나침반, 다이너마이트, spare에는 2번째 자식이 없음
            // if (trainType != TrainType.StationDir && trainType != TrainType.Dynamite) 
            if (trainType != TrainType.StationDir && trainType != TrainType.Dynamite && trainType != TrainType.Spare)
            {
                warningIcon = transform.GetChild(2).gameObject;
                warningIcon.SetActive(false);
            }
        }
        if (trainType != TrainType.StationDir || trainType != TrainType.Dynamite)
        {
            fireEffect = GetComponentInChildren<ParticleSystem>();
        }



        cameraTarget = GameObject.FindGameObjectWithTag("Cinemachine").GetComponentsInChildren<Cinemachine.CinemachineVirtualCamera>();
        overText = GameObject.FindGameObjectWithTag("Cinemachine").GetComponent<Animator>();
        manager = FindObjectOfType<InGameScene>();
        overSpeed = 1f;

        if (trainType == TrainType.Engine)
        {
            cameraTarget[0].Follow = transform;
        }
    }
    private void Start()
    {
        CameraSwitch(1);
    }
    protected void TrainMovePos()
    {

        listToQue = rails.ToList();

        if (!isReady)
        {
            if (isGoal)
            {
                _trainMoveSpeed = goalSpeed;
                _trainRotateSpeed = goalSpeed;
            }
            else if (isTrainOver)
            {
                _trainMoveSpeed = overSpeed;
                _trainRotateSpeed = overSpeed;
            }
            else
            {
                _trainMoveSpeed = trainSpeed;
                _trainRotateSpeed = trainSpeed * 2;
            }
        }
        else
        {
            _trainMoveSpeed = 0;
            return;
        }

        if (rails.Count != 0)
        {
            RotateTrain();
            transform.position = Vector3.MoveTowards(transform.position, rails.First().transform.position, _trainMoveSpeed * Time.deltaTime);


            if (transform.position == rails.First().transform.position)
            {
                rails.RemoveFirst();
            }

            // 큐에 들어온 레일 위치로 이동
            // transform.LookAt(rails.Peek().transform.position);
            //var trainRotate = rails.Peek().transform.position;
            //Debug.Log(trainRotate);
            //transform.rotation = Quaternion.Slerp(transform.rotation, trainRotate, trainRotateSpeed * Time.deltaTime);
        }

        else
        {
            if (isGoal)
            {
                //클리어 조건
                //제일 먼저 도착하는 엔진을 멈추게 하면 뒤 따라오는 애들 전원 정지로 구현

                if (trainType == TrainType.Engine)
                {
                    if(!isReady)
                    {
                        isReady = true;
                        // 역 도착하기
                        FindObjectOfType<InGameScene>().engine = gameObject.GetComponent<TrainEngine>();

                        FindObjectOfType<InGameScene>().ArriveStation();

                        // ShopManager.Instance.ShopOn();
                    }
                }
            }
            else
            {
                //오버 조건
                TrainOver();
            }

        }
    }
    public void CameraSwitch(int num)
    {

        if(num== 2)
        {
            cameraTarget[1].Follow = GoalManager.Instance.lastRail.transform;
        }
        for (int i = 0; i < cameraTarget.Length; i++)
        {
            cameraTarget[i].gameObject.SetActive(true);
            if (i != num -1)
            {
                cameraTarget[i].gameObject.SetActive(false);
            }
        }
    }
    public virtual void TrainUpgrade()
    {
        destroyParticle.SetActive(false);
        destroyParticle.SetActive(true);
        //상속해서 올리기
    }
    public void TrainOver()
    {
        SoundManager.Instance.StopAllSound();

        if (trainType != TrainType.Spare && !isOver)
        {
            ResourceTracker.Instance.gameObject.SetActive(false);
             isOver = true;
            StopCoroutine(Warning());
            SoundManager.Instance.PlaySoundEffect("Train_Broken");
            trainMesh.gameObject.SetActive(false);
            destroyParticle.SetActive(false);
            destroyParticle.SetActive(true);
        }
        if(trainType == TrainType.Spare)
        {
            StartCoroutine(OverAnim());
        }
    }
    public IEnumerator OverAnim()
    {
        CameraSwitch(3);
        yield return new WaitForSeconds(2.5f);
        overText.SetBool("GameOver", true);
        yield return new WaitForSeconds(1.5f);
        manager.backToLobbyUI.SetActive(true);
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("IntroScene");
    }

    public IEnumerator ClearAnim()
    {
        CameraSwitch(4);
        yield return new WaitForSeconds(2f);
        overText.SetBool("GameClear", true);
        yield return new WaitForSeconds(5f);
        manager.backToLobbyUI.SetActive(true);
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("IntroScene");
    }

    public void RotateTrain()
    {
        if(rails != null)
        {
            Vector3 dir = rails.First().transform.position - transform.position;
            if(trainMesh != null)
            trainMesh.transform.rotation = Quaternion.Lerp(trainMesh.transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * _trainRotateSpeed);
        }
    }
    public void EnqueueRailPos(RailController gameObject)
    {
        if (trainType != TrainType.Dynamite || trainType != TrainType.StationDir)
            //큐에 추가
            rails.AddLast(gameObject);
    }
    public void DequeueRailPos(RailController gameObject)
    {
        if (trainType != TrainType.Dynamite || trainType != TrainType.StationDir)
            //큐에 추가
            rails.RemoveLast();
    }

    public IEnumerator Warning()
    {
        _isPlay = true;
        warningIcon.SetActive(true);
    
        yield return new WaitForSeconds(1.5f);
        warningIcon.SetActive(false);
        _isPlay = false;
    }
}
