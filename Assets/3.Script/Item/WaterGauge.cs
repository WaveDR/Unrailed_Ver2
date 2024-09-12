using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaterGauge : MonoBehaviour 
{
    [SerializeField] private float _slideSpeed;
    [SerializeField] private Slider _waterSlide;

    private Transform _player;

    private float _currentGauge = 0f;
    private float CurrentGauge
    {
        get { return _currentGauge; }
        set
        {
            _currentGauge = value;

            _currentGauge = Mathf.Clamp(_currentGauge, 0, 1);

            if(Mathf.Abs(_currentGauge - 1) <= 0.01f)
            {
                gameObject.SetActive(false);
            }
            else if(_currentGauge <= 0.01f)
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void Awake()
    {
        _player = FindObjectOfType<PlayerController>().transform;
    }

    void Update()
    {
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(_player.position);
        transform.position = screenPosition;
    }

    public void ResetWater()
    {
        CurrentGauge = 0;
        _waterSlide.value = 0;
    }


    public void FillGauge()
    {
        CurrentGauge += _slideSpeed;
       _waterSlide.value = CurrentGauge;
    }

    public bool IsFillWater()
    {
        return _waterSlide.value == 1;
    }
}
    