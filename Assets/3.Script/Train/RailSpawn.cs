using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailSpawn : MonoBehaviour
{
    public List<GameObject> obj=  new List<GameObject>();

    float timeSpawn;
    [SerializeField] int countUp;
    [SerializeField] RailPooling poolRail;
    
    private void Awake()
    {
        poolRail = FindObjectOfType<RailPooling>();
    }
    public void Update()
    {
        timeSpawn += Time.deltaTime;
        if(timeSpawn > .1f)
        {
            TimeUp();
            timeSpawn = 0;
        }
    }

    void TimeUp()
    {

        if (countUp >= obj.Count)
        {
            return;
        }
        SoundManager.Instance.StopSoundEffect("Rail_Down");
        SoundManager.Instance.PlaySoundEffect("Rail_Down");
        poolRail.TransformRail(obj[countUp].transform, false);
        //obj[countUp].transform.position = poolRail.
        countUp++;

     
    }
}
