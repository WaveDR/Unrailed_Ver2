using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmoteUI : MonoBehaviour
{
    private Transform _helperTransform;
    private void Awake()
    {
        _helperTransform = FindObjectOfType<Helper>().UItransform;
    }

    void Update()
    {
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(_helperTransform.position);
        transform.position = screenPosition;
    }
}
