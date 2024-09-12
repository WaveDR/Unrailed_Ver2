using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EResource { tree, steel};

public class ReSource : MonoBehaviour
{
    [SerializeField] private EResource resourceType;
    [SerializeField] private int resourceHp = 3;
    [SerializeField] private float _scaleAmountOfChange = 0.2f;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private ParticleSystem destroyEffect;
    private int _currentHp;
    private int CurrentHp
    {
        get { return _currentHp; }
        set 
        {
            _currentHp = value;

            if (_currentHp == 0)
            {

                DestroyResource();
            }
             
        }
    }

    public EResource ResourceType => resourceType;

    private void Awake()
    {
        CurrentHp = 3;
    }

  

    public void Dig()
    {
        if (CurrentHp <= 0) return;
        if (resourceType == EResource.tree) SoundManager.Instance.PlaySoundEffect("Wood_Hit");
        if (resourceType == EResource.steel) SoundManager.Instance.PlaySoundEffect("Steel_Hit");
        CurrentHp--;
        transform.localScale -= Vector3.one * _scaleAmountOfChange;
        hitEffect.Stop();
        hitEffect.Play();
    }

    public void Explosion()
    {
        CurrentHp = 0;
    }

    private void DestroyResource()
    {
        if (resourceType == EResource.tree) SoundManager.Instance.PlaySoundEffect("Wood_Broken");
        if (resourceType == EResource.steel) SoundManager.Instance.PlaySoundEffect("Steel_Broken");
        Transform parent = transform.parent;
        GameObject itemObject = Instantiate(itemPrefab, parent);
        itemObject.transform.localPosition = Vector3.up * 0.5f;
        itemObject.transform.localRotation = Quaternion.identity;
        destroyEffect.transform.parent = null;
        destroyEffect.Play();
        Destroy(gameObject);
    }
}