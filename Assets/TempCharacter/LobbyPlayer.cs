using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyPlayer : MonoBehaviour
{
    [Header("Object")]
    [SerializeField] private GameObject _runParticle;

    // 상태  => 이건 상태패턴??
    private bool _isDash = false;

    // 컴포넌트
    private LobbyPlayerInput _playerInput;
    private PlayerStat _playerStat;

    // 플레이어 수치
    private float _currentSpeed;


    private void Awake()
    {
        _playerInput = GetComponent<LobbyPlayerInput>();
        _playerStat = GetComponent<PlayerStat>();
        _runParticle.SetActive(false);

        _currentSpeed = _playerStat.moveSpeed;
    }

    private void FixedUpdate()
    {
        // 플레이어 움직임
        Move();
    }

    private void Move()
    {
        // 움직임, 회전, 대시까지
        if (_playerInput.IsShift && !_isDash)
        {
            SoundManager.Instance.PlaySoundEffect("Player_Dash");
            _isDash = true;
            _runParticle.SetActive(true);
            _currentSpeed = _playerStat.dashSpeed;
            Invoke("DashOff", _playerStat.dashDuration);
        }

        transform.position += _playerInput.Dir * _currentSpeed * Time.deltaTime;
        transform.LookAt(_playerInput.Dir + transform.position);
    }

    private void DashOff()
    {
        _currentSpeed = _playerStat.moveSpeed;
        _runParticle.SetActive(false);
        _isDash = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("IntroUI"))
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                InteractionUI startUI = other.GetComponent<InteractionUI>();

                startUI.GoMapEdit();
                startUI.GameStart();
                startUI.GameExit();
            }
        }
    }
}
