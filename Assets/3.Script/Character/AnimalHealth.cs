using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum EnemyType {Cow,Enemy }

public class AnimalHealth : MonoBehaviour
{

    [SerializeField] private EnemyType enemyType;
    [SerializeField] private int _animalHp = 8;

    [SerializeField] private GameObject[] hitEffects;
    [SerializeField] private GameObject bloodEffect;
    [SerializeField] private GameObject destroyEffect;

    private int _countPlus;

    [SerializeField] private int _currentHp;
    public int CurrentHp
    {
        get { return _currentHp; }
        set
        {
            _currentHp = value;

            if(_currentHp == 0)
            {
                Debug.Log("¼Ò Á×À½");
                StartCoroutine(Death());
            }
            else if (_currentHp <= 4)
            {
                bloodEffect.SetActive(true);
            }
        }
    }

    private void Awake()
    {
        CurrentHp = _animalHp;

        bloodEffect.SetActive(false);
        destroyEffect.SetActive(false);

        for (int i = 0; i < hitEffects.Length; i++)
        {
            hitEffects[i].SetActive(false);
        }
    }

    public void Hit()
    {
        if (CurrentHp <= 0) return;
        CurrentHp--;

        if (enemyType == EnemyType.Cow)
        {
           // SoundManager.Instance.StopSoundEffect("Enemy_CowHit");
            SoundManager.Instance.PlaySoundEffect("Enemy_CowHit");
        }

        else if (enemyType == EnemyType.Enemy)
        {
            SoundManager.Instance.PlaySoundEffect("Enemy_Hit");
        }

        if (_countPlus >= hitEffects.Length)
        {
            for (int i = 0; i < hitEffects.Length; i++)
            {
                hitEffects[i].SetActive(false);
            }
            _countPlus = 0;
        }
        hitEffects[_countPlus].SetActive(false);
        hitEffects[_countPlus].SetActive(true);
        _countPlus++;
    }

    public void Explosion()
    {
        CurrentHp = 0;
    }

    private IEnumerator Death()
    {
        bloodEffect.SetActive(false);
        destroyEffect.SetActive(true);
        transform.GetChild(0).gameObject.SetActive(false);
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}
