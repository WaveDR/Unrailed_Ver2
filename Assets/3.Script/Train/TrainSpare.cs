using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainSpare : TrainMovement
{
    public TrainMovement[] trainSpare;
    public List<TrainMovement> spawnTrainList = new List<TrainMovement>();

    TrainEngine engine;
    TrainMovement train;
    public int trainIndex;
    private void Awake()
    {
        trainIndex = -1;
        GetMesh();
        engine = FindObjectOfType<TrainEngine>();
        fireEffect = GetComponentInChildren<ParticleSystem>();
        fireEffect.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        TrainMovePos();
    }

    public void ChangeTrain(int _trainIdx)
    {
        GameObject trainObj = Instantiate(trainSpare[_trainIdx].gameObject, transform.position, transform.rotation);
        train = trainObj.GetComponent<TrainMovement>();
        trainIndex = _trainIdx;
        for (int i = 0; i < rails.Count; i++)
        {
            train.rails.AddLast(listToQue[i]);
        }
        spawnTrainList.Add(train);
        engine.trains.Add(train);

        train.TrainUpgrade();

        train.trainUpgradeLevel++;
        ShopManager.Instance.ResetTrains();

    }

    public void ResetTrain(int trainIdx) // 위치값 이동
    {
        Destroy(spawnTrainList[0].gameObject);
        engine.trains.RemoveAt(engine.trains.Count - 1);
        spawnTrainList.Clear();
       
        train.trainUpgradeLevel--;
        ShopManager.Instance.newCarList[trainIdx].gameObject.SetActive(false);
        ShopManager.Instance.newCarList[trainIdx].gameObject.SetActive(true);

    }
}
