using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldResource : MonoBehaviour
{
    public enum EType
    {
        Wood,
        Water,
        Stone,
        Resource
    }
    [SerializeField] private EType _Type;
    public EType Type => _Type;

    public AI_StackItem Stack;

    [SerializeField] private Transform _item;
    [SerializeField] private float _resourceHp = 2.7f;
    private float _scaleAmountOfChange = 0.2f;
    private float _resourceScale = Mathf.Clamp01(1);

    void Start()
    {
        Stack = GetComponent<AI_StackItem>();
        ResourceTracker.Instance.RegisterResource(this);
    }

    private void OnDestroy()
    {
        ResourceTracker.Instance.DeRegisterResource(this);
    }



    //자원 캐기
    public bool isDig()
    {
        if (_resourceHp > 0)
            return true;
        else
        {
            SpawnItem();
            return false;
        }
          
    }

    public IEnumerator isDigCo()
    {
        WaitForSeconds wait = new WaitForSeconds(0.7f);

            yield return wait;
        while(_resourceHp > 0)
        {
            switch (Type)
            {
                case WorldResource.EType.Wood:
                    SoundManager.Instance.PlaySoundEffect("Wood_Hit");
                    break;
                case WorldResource.EType.Stone:
                    SoundManager.Instance.PlaySoundEffect("Steel_Hit");
                    break;
            }

            _resourceScale -= 0.1f;
            _resourceHp--;
            transform.localScale -= Vector3.one * _scaleAmountOfChange;
            yield return wait;
        }
            yield return wait;
    }

    private void SpawnItem()
    {
        Transform NewItem = Instantiate(_item, new Vector3(0, 0, 0), Quaternion.identity);
        NewItem.SetParent(transform.parent);
        NewItem.localPosition = (new Vector3(0, 0.5f, 0));
        NewItem.localRotation = Quaternion.identity;
        DisableAllChildColliders(NewItem);
    }

    private void DisableAllChildColliders(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Collider childCollider = child.GetComponent<Collider>();
            if (childCollider != null)
            {
                childCollider.enabled = false;
            }

            // 자식 객체의 자식 객체들도 재귀적으로 탐색
            DisableAllChildColliders(child);
        }
    }

}
