using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInteractionTest : MonoBehaviour
{
    [SerializeField] private bool _isHighlight;

    [SerializeField] private float _brightAmount = 50f;
    private List<Material> materials = new List<Material>();
    private List<Color> _originColors = new List<Color>();

    private void Awake()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            materials.Add(renderer.material);
            _originColors.Add(renderer.material.color);
        }
        _brightAmount /= 256;
    }


/*    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {

            Interaction();
        }
    }*/

    public void Interaction()
    {
        // 밝은 상태면 원래대로
        if (_isHighlight)
        {
            foreach (Material material in materials)
                material.color -= new Color(_brightAmount, _brightAmount, _brightAmount, 0);
        }
        else
        {
            foreach (Material material in materials)
                material.color += new Color(_brightAmount, _brightAmount, _brightAmount, 0);
        }
        _isHighlight = !_isHighlight;
    }

    public void Interaction(bool flag)
    {
        // 켜주기
        if(flag)
        {
            for(int i = 0; i < materials.Count; i++)
            {
                materials[i].color = new Color(_originColors[i].r + _brightAmount, _originColors[i].g + _brightAmount, _originColors[i].b + _brightAmount, 0);
            }
            _isHighlight = true;
        }
        // 꺼주기
        else
        {
            for(int i = 0; i < materials.Count; i++)
            {
                materials[i].color = new Color(_originColors[i].r , _originColors[i].g, _originColors[i].b, 0);
            }
            _isHighlight = false;
        }
    }
}
