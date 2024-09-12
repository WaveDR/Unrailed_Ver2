using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroUI : MonoBehaviour
{

    [Header("Title")]
    [SerializeField] private GameObject titleTalk;

    [Header("Lobby")]
    [SerializeField] private GameObject lobbyAnim;
    [SerializeField] private GameObject lobbyTalk;
    [SerializeField] private GameObject playerTutorial;

    [SerializeField] private LobbyPlayer player;

    public float _introTime;
    private bool _introInput;

    // Update is called once per frame

    private void Awake()
    {
        player = FindObjectOfType<LobbyPlayer>();
        titleTalk.SetActive(false);
        lobbyAnim.SetActive(false);
        lobbyTalk.SetActive(false);
        playerTutorial.SetActive(false);
        player.gameObject.SetActive(false);
    }

    void StartGameIntro()
    {

        _introTime += Time.deltaTime;

        if (_introTime > 2.5f)
        {
            titleTalk.SetActive(true);
            _introInput = true;
            return;
        }

    }
    private void Update()
    {
        StartGameIntro();
        if (Input.GetKeyDown(KeyCode.Space) && _introInput)
        {
            lobbyAnim.SetActive(true);
            lobbyTalk.SetActive(true);
            player.gameObject.SetActive(true);
            playerTutorial.SetActive(true);


            titleTalk.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}
