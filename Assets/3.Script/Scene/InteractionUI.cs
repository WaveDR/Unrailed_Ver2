using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InteractionUI : MonoBehaviour
{
    public Image image;
    public Text text;
    public Sprite[] triggerImage;

    public bool isMapEditor = false;
    public bool Exit;
    private void Awake()
    {
        image.sprite = triggerImage[0];
        //text.color = 
    }
    public void GameStart()
    {
        if (!Exit && !isMapEditor)
        {
            Debug.Log("이거 함 1");

            //goto 후에 바꿀 것
            SoundManager.Instance.PlaySoundEffect("Btn_Click");
            SceneManager.LoadScene("InGame");
        }
       
  
    }
    public void GameExit()
    {
        if (Exit && !isMapEditor)
        {
            Debug.Log("이거 함 2");

            //goto 후에 바꿀 것
            SoundManager.Instance.PlaySoundEffect("Btn_Click");
            Application.Quit();
        }
    }

    public void GoMapEdit()
    {
        if(isMapEditor && !Exit)
        {
            Debug.Log("이거 함 3");

            SoundManager.Instance.PlaySoundEffect("Btn_Click");
            SceneManager.LoadScene("MapTool");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SoundManager.Instance.PlaySoundEffect("Btn_OnMouse");
            image.sprite = triggerImage[1];
            text.color = new Color32(255, 196, 118, 255);
        }

    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            image.sprite = triggerImage[0];
            text.color = Color.white;
        }
    }
}
