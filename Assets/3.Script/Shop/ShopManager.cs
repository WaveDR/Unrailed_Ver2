using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance = null;

    public List<ShopItem> newCarList = new List<ShopItem>();

    public TrainMovement[] trains;

    [SerializeField] private Animator anim;
    [SerializeField] public Transform[] shopUpgradeTrainPos;
    [SerializeField] private Transform[] shopEnginePos;
    public Transform[] shopNewTrainPos;

    [SerializeField] private Text[] shopUpgradeText;
    [SerializeField] private Text[] shopEngineText;
    public Text[] shopNewTrainText;

    public ShopItem[] trainPrefabs;
    public ShopItem[] trainNewPrefabs;
    public ShopItem[] enginePrefabs;

    private TrainWater trainWater;
    private TrainEngine trainEngine;

    [SerializeField] private int readyCount;

    public int trainCoin;
    public int itemIdx;

    bool _isShop;
    public Image[] goToLoading;

    public Transform currentStation = null;
    public Transform nextGame;

    public bool isPlayerShop;
    private void Awake()
    {
        //싱글톤 패턴
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        anim = GetComponentInParent<Animator>();
        trains = FindObjectsOfType<TrainMovement>();
        trainWater = FindObjectOfType<TrainWater>();
        trainEngine = FindObjectOfType<TrainEngine>();
    }

    public void StartTrainMove()
    {
        trains = FindObjectsOfType<TrainMovement>();
        trainWater = FindObjectOfType<TrainWater>();
        trainEngine = FindObjectOfType<TrainEngine>();

        StartCoroutine(TrainStartMove());
    }

    public void ShopOn()
    {
        if (!_isShop)
        {
            RandItemSpawn();

            isPlayerShop = true;
            _isShop = true;
        }

        //Debug.Log("상점으로 위치 이동");
        anim.transform.position = currentStation.position;

        anim.SetBool("isReady", true);
        SoundManager.Instance.audioSourdEngine.Stop();
        SoundManager.Instance.StopAllSound();
        SoundManager.Instance.PlaySoundBgm("Shop_Bgm");
    }  // 상점 오픈 애니메이션

    public void ShopOff()
    {
        trainEngine.CameraSwitch(1);
        anim.SetBool("isReady", false);
        StartCoroutine(TrainStartMove());
    } //상점 클로즈 애니메이션

    public void ResetTrains()
    {
        trains = FindObjectsOfType<TrainMovement>();
    }

    private void RandItemSpawn()
    {
        for (int i = 0; i < shopUpgradeTrainPos.Length; i++)
        {
            GameObject shopUpObj = Instantiate(trainPrefabs[i].gameObject, shopUpgradeTrainPos[i].position, shopUpgradeTrainPos[i].rotation, shopUpgradeTrainPos[i]);
            shopUpObj.transform.GetChild(0).gameObject.SetActive(false);
        }
        for (int i = 0; i < shopNewTrainPos.Length; i++)
        {
            GameObject shopNewObj = Instantiate(trainNewPrefabs[i].gameObject, shopNewTrainPos[i].position, shopNewTrainPos[i].rotation, shopNewTrainPos[i]);
            shopNewObj.transform.GetChild(0).gameObject.SetActive(false);
        }

        GameObject shopEngineObj = Instantiate(enginePrefabs[0].gameObject, shopEnginePos[0].position, shopEnginePos[0].rotation, shopEnginePos[0]);
        shopEngineObj.transform.GetChild(0).gameObject.SetActive(false);

        TrainCost();
    } //위치에 스폰

    public void TrainCost()
    {
        foreach(TrainMovement train in trains)
        {
            switch (train.trainType)
            {
                    //==========================================Upgrade
                case TrainType.WaterBox:
                    trainPrefabs[0].itemCost = train.trainUpgradeLevel;
                    shopUpgradeText[0].text = $"{train.trainUpgradeLevel}";
                    break;
   
                case TrainType.WorkBench:
                    trainPrefabs[1].itemCost = train.trainUpgradeLevel;
                    shopUpgradeText[1].text = $"{train.trainUpgradeLevel}";

                    break;

                case TrainType.ChestBox:
                    trainPrefabs[2].itemCost = train.trainUpgradeLevel;
                    shopUpgradeText[2].text = $"{train.trainUpgradeLevel}";
                    break;

                    //==========================================New Car


                case TrainType.StationDir:
                    trainNewPrefabs[0].itemCost = train.trainUpgradeLevel;
                    shopNewTrainText[0].text = $"{train.trainUpgradeLevel}";
                    break;

                case TrainType.Dynamite:
                    trainNewPrefabs[1].itemCost = train.trainUpgradeLevel;
                    shopNewTrainText[1].text = $"{train.trainUpgradeLevel}";
                    break;

                    //==========================================Engine

                case TrainType.Engine:
                    enginePrefabs[0].itemCost = train.trainUpgradeLevel;
                    shopEngineText[0].text = $"{train.trainUpgradeLevel}";
                    break;
            }
        }
    } // 코스트 출력 

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Invisible"))
        {
            InVisibleObject invisibleObject = other.GetComponent<InVisibleObject>();
            if(invisibleObject != null)
                invisibleObject.UnShow();
        }

        if (other.CompareTag("ShopItem"))
        {
            InVisibleObject invisibleObject = other.GetComponent<InVisibleObject>();
            if (invisibleObject != null)
                invisibleObject.Show();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Invisible"))
        {
            InVisibleObject invisibleObject = other.GetComponent<InVisibleObject>();
            if (invisibleObject != null)
                invisibleObject.Show();
        }

        if (other.CompareTag("ShopItem"))
        {
            InVisibleObject invisibleObject = other.GetComponent<InVisibleObject>();
            if (invisibleObject != null)
                invisibleObject.UnShow();
        }
    }

    private IEnumerator TrainStartMove() // 열차 시작 카운트다운
    {
        SoundManager.Instance.StopAllSound();
        isPlayerShop = false;
        trainEngine.isReady = true;
        yield return new WaitForSeconds(10f);
        trainEngine.anim.SetBool("CountDown", true);

        yield return new WaitForSeconds(0.1f);

        trainEngine.anim.SetBool("CountDown", false);

        for (int i = 0; i < readyCount; i++)
        {
            SoundManager.Instance.PlaySoundEffect("Train_Count");
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(0.5f);
        SoundManager.Instance.PlaySoundEffect("Train_Start");
        trainEngine.isGoal = false;
        trainEngine.isReady = false;
        _isShop = false;
        trainWater.FireOff();
        yield return new WaitForSeconds(2f);
        SoundManager.Instance.audioSourdEngine.Play();
        SoundManager.Instance.PlaySoundBgm("InGame_Bgm");
        
    } 
}
