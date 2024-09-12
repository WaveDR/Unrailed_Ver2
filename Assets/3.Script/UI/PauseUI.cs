using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private GameObject setPauseObj;
    [SerializeField] private GameObject blurPost;

    [Header("UI Zip")]
    [SerializeField] private GameObject audioUI;
    [SerializeField] private GameObject defaultUI;
    [SerializeField] private Text title_Text;

    [SerializeField] private Text bgmValueText;
    [SerializeField] private Text sfxValueText;
    public AudioMixer mixer;

    public Slider bgmSlider;
    public Slider sfxSlider;

    PlayerController player;
    private bool _isEscape;
    private bool _isAudioSetting;

    private void Awake()
    {
        player = FindObjectOfType<PlayerController>();
        setPauseObj.SetActive(false);
        blurPost.SetActive(false);
        defaultUI.SetActive(true);
        audioUI.SetActive(false);

        bgmSlider.value = 0.5f;
        sfxSlider.value = 0.5f;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) )
        {
            _isEscape = !_isEscape;
        }
        if (!_isEscape)
        {
            Continue();
        }

        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _isAudioSetting = false;
            }

            if (_isAudioSetting) //����� ��ư �ѱ�
            {
                title_Text.text = "�����";

                bgmValueText.text = Mathf.FloorToInt(bgmSlider.value * 100).ToString();
                sfxValueText.text = Mathf.FloorToInt(sfxSlider.value * 100).ToString();
            }
            else
            {
                title_Text.text = "�Ͻ� ����";
                defaultUI.SetActive(true);
                audioUI.SetActive(false);
            }

            blurPost.SetActive(true); //����Ʈ���μ��� ��
            setPauseObj.SetActive(true); //ui esc �ѱ�

            Time.timeScale = 0;
        }

    }
    public void ClickOnContinue()
    {
        _isEscape = false;
    }
    public void GotoLobby()
    {
        SceneManager.LoadScene("IntroScene");
        Time.timeScale = 1;
    }
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1;
    }
  
    public void Respawn()
    {
        //������ ���߿� �ΰ澾 �ϼ��ϸ� ����
        _isEscape = false;
        Time.timeScale = 1;
        player.Respawn();
        setPauseObj.SetActive(false);
    }
    public void ExitGame()
    {
        Application.Quit();
    }
    public void AudioSetting()
    {
        //���ؾ� ����� �ϼ��� ����
        _isAudioSetting = true;

        defaultUI.SetActive(false);
        audioUI.SetActive(true);
    }
    public void Continue()
    {
        blurPost.SetActive(false);
        setPauseObj.SetActive(false);
        Time.timeScale = 1;
    }

    public void SetBgmVolme()
    {
        mixer.SetFloat("BGM", Mathf.Log10(bgmSlider.value) * 20);
    }

    public void SetSfxVolme()
    {
        mixer.SetFloat("SFX", Mathf.Log10(sfxSlider.value) * 20);
    }
}
