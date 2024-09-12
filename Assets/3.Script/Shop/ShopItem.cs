using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItem : MonoBehaviour
{
    public TrainType itemType;
    public int itemCost;
    public int itemIdx;
    public bool _isSpawn = true;
    BoxCollider box;

    private Transform _originParent;
    private Vector3 _originPos;

    void Awake()
    {
        _originParent = transform.parent;
        _originPos = transform.localPosition;

        TryGetComponent(out box);
        if (itemType == TrainType.StationDir || itemType == TrainType.Dynamite)
            ShopManager.Instance.newCarList.Add(this);

    }
    private void OnEnable()
    {
        _isSpawn = true;
        box.enabled = true;
    }


    public void TradeItem(Collider col)
    {
        Debug.Log("거래성사");
        TrainMovement train = col.GetComponent<TrainMovement>();

        if (train.trainType != TrainType.Spare)
        {
            if (train.trainType != itemType) return;

            train.trainUpgradeLevel++;
            train.TrainUpgrade();
            box.enabled = false;
        }

        else
        {
            if (itemType == TrainType.Dynamite || itemType == TrainType.StationDir)
                {
                    TrainSpare spare = col.GetComponent<TrainSpare>();
                    spare.ChangeTrain(itemIdx);
                    box.enabled = false;
                }
                else return;
        }
        SoundManager.Instance.PlaySoundEffect("Train_Buy");
        ShopManager.Instance.trainCoin--;
        ShopManager.Instance.TrainCost();
        _isSpawn = false;
        transform.GetChild(0).gameObject.SetActive(false);
    }

    // 진열된 위치로 되돌아가기
    public void SetInitPosition()
    {
        // 위치 설정하고
        transform.SetParent(_originParent);
        transform.localPosition = _originPos;
        transform.localRotation = Quaternion.identity;

        // 코일 돌려주자
        ShopManager.Instance.trainCoin += itemCost;
    }

    public void SetPosition(Transform parent)
    {
        // 위치 설정하고
        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // 코일 돌려주자
        ShopManager.Instance.trainCoin -= itemCost;
    }

    public bool TryBuyItem()
    {
        return ShopManager.Instance.trainCoin >= itemCost;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Train"))
        {
            if (_isSpawn && (itemType == TrainType.StationDir || itemType == TrainType.Dynamite))
            {
                TrainSpare trains = other.GetComponent<TrainSpare>();

                if (trains != null && trains.trainIndex == 0)
                {
                    trains.ResetTrain(1);
                }
                else if (trains != null && trains.trainIndex == 1)
                {
                    trains.ResetTrain(0);
                }
            }

            TradeItem(other);
        }
    }

    /*private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Train"))
        {
            if (_isSpawn && (itemType == TrainType.StationDir || itemType == TrainType.Dynamite))
            {
                TrainSpare trains = other.GetComponent<TrainSpare>();

                if (trains != null && trains.trainIndex == 0)
                {
                    trains.ResetTrain(1);
                }
                else if (trains != null && trains.trainIndex == 1)
                {
                    trains.ResetTrain(0);
                }
            }

            TradeItem(other);
        }

        else if (other.CompareTag("ShopItem"))
        {
            ShopItem items = other.GetComponent<ShopItem>();

            if (items._isSpawn)
            {
                transform.localPosition = Vector3.zero;
            }
        }
        else
        {
            transform.localPosition = Vector3.zero;
        }
    }*/
}
