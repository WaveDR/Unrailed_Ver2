using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyPlayer : MonoBehaviour
{
    [Header("Object")]
    [SerializeField] private GameObject _runParticle;

    // ����  => �̰� ��������??
    private bool _isDash = false;

    // ������Ʈ
    private LobbyPlayerInput _playerInput;
    private PlayerStat _playerStat;

    // �÷��̾� ��ġ
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
        // �÷��̾� ������
        Move();
    }

    private void Move()
    {
        // ������, ȸ��, ��ñ���
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
