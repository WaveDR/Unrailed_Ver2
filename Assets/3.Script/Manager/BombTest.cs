using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombTest : MonoBehaviour
{
    [SerializeField] private LayerMask banglayer;
    [SerializeField] private float radius = 2f;
    [SerializeField] private ParticleSystem ExplosionEffect;
    [SerializeField] private ParticleSystem BombEffect;


    public void Setup()
    {
        StartCoroutine(ExplosionCo());
    }


    private IEnumerator ExplosionCo()
    {
        ExplosionEffect.Play();
        yield return new WaitForSeconds(3f);

        BombEffect.transform.parent = null;
        BombEffect.Play();

        SoundManager.Instance.PlaySoundEffect("Train_Broken");
        // ���� �� �ִ°� �����ؼ� ����
        Collider[] hitCollider = Physics.OverlapSphere(transform.position, radius, banglayer);
        for(int i = 0; i< hitCollider.Length; i++)
        {
            ReSource resource = hitCollider[i].GetComponent<ReSource>();
            AnimalHealth animal = hitCollider[i].GetComponent<AnimalHealth>();
            if (resource != null)
            {
                resource.Explosion();
            }
            else if(animal != null)
            {
                animal.Explosion();
            }
        }

        // �÷��̾� ����
        PlayerController player = FindObjectOfType<PlayerController>();
        if(player != null && Vector3.Distance(transform.position, player.transform.position) < radius)
        {
            SoundManager.Instance.PlaySoundEffect("Enemy_Hit");
            player.Respawn();
        }

        Destroy(gameObject);
    }
}
