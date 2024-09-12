using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayerUI : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    private Transform followCam;

    [SerializeField] bool EmotUI;

    private void Awake()
    {
        if(EmotUI)
        {
            SoundManager.Instance.StopAllSound();
            SoundManager.Instance.PlaySoundBgm("Lobby_Bgm");
            followCam = GameObject.FindGameObjectWithTag("MainCamera").transform;
        }
    }

    void Update()
    {
        if(followTarget != null)
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(followTarget.position+Vector3.up*0.5f);
            transform.position = screenPosition;
        }

        if (EmotUI)
        {
            Vector3 orderUIVec = new Vector3(followTarget.position.x - -0.5f, followTarget.position.y + 3.87f, followTarget.position.z - 3.11f);
            transform.position = orderUIVec;
        }
    }
}
