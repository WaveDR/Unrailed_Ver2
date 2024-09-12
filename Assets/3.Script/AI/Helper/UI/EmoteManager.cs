using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmoteManager : MonoBehaviour
{
    [HideInInspector]
    public int DefaultEmote = 0;
    [HideInInspector]
    public int AxeEmote = 1;
    [HideInInspector]
    public int PickEmote = 2;
    [HideInInspector]
    public int BucketEmote = 3;


    [HideInInspector]
    public int WarningEmote = 4;
    [HideInInspector]
    public int SleepEmote = 5;
    [HideInInspector]
    public int HeartEmote = 6;
    [HideInInspector]
    public int HmmEmote = 7;
    [HideInInspector]
    public int SadEmote = 8;


    [HideInInspector]
    public int WoodEmote = 10;
    [HideInInspector]
    public int WaterEmote = 11;
    [HideInInspector]
    public int StoneEmote = 12;
    [HideInInspector]
    public int ResourceEmote = 13;


    private Dictionary<int, Sprite> _spriteData;
    [SerializeField] private Sprite[] _sprite;

    private void Awake()
    {
        _spriteData = new Dictionary<int, Sprite>();
        Init();
    }

    private void Init()
    {
        _spriteData.Add(AxeEmote, _sprite[0]);
        _spriteData.Add(PickEmote, _sprite[1]);
        _spriteData.Add(BucketEmote, _sprite[2]);
        _spriteData.Add(WarningEmote, _sprite[3]);
        _spriteData.Add(SleepEmote, _sprite[4]);

        _spriteData.Add(WoodEmote, _sprite[5]);
        _spriteData.Add(WaterEmote, _sprite[6]);
        _spriteData.Add(StoneEmote, _sprite[7]);
        _spriteData.Add(ResourceEmote, _sprite[8]);

        _spriteData.Add(HeartEmote, _sprite[9]);
        _spriteData.Add(HmmEmote, _sprite[10]);
        _spriteData.Add(SadEmote, _sprite[11]);
    }

    public Sprite GetEmote(int id)
    {
        return _spriteData[id];
    }
}
