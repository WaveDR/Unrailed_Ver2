using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyPlayerInput : MonoBehaviour
{
    [SerializeField] private Vector3 _dir;
    [SerializeField] private bool _isSpace;
    [SerializeField] private bool _isShift;

    public Vector3 Dir => _dir.normalized;
    public bool IsSpace => _isSpace;
    public bool IsShift => _isShift;

    private void Awake()
    {
        _dir = Vector3.zero;
        _isSpace = false;
        _isShift = false;
    }

    private void Update()
    {
        _isSpace = Input.GetKeyDown(KeyCode.Space);
        _isShift = Input.GetKeyDown(KeyCode.LeftShift);

        if (!_isShift)
            _dir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
    }
}
