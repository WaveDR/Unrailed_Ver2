using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private Vector3 _dir;
    [SerializeField] private bool _isSpace;
    [SerializeField] private bool _isShift;

    private FollowPlayerUI _orderUI;

    public Vector3 Dir => _dir.normalized;
    public bool IsSpace => _isSpace;
    public bool IsShift => _isShift;

    private bool _e;
    private void Awake()
    {
        _orderUI = FindObjectOfType<FollowPlayerUI>();
        _dir = Vector3.zero;
        _isSpace = false;
        _isShift = false;
        _e = false;
    }

    private void Update()
    {
        _e = Input.GetKey(KeyCode.E);
        _isSpace = Input.GetKeyDown(KeyCode.Space);
        _isShift = Input.GetKeyDown(KeyCode.LeftShift);
        
        if(!_isShift)
            _dir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        if(_e)
        {
            _orderUI.gameObject.SetActive(true);
        }
        else
        {
            _orderUI.gameObject.SetActive(false);
        }
    }
}
