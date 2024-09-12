using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopUI : MonoBehaviour
{
    [Header("File")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button saveAsButton;
    [SerializeField] private Button openButton;
    [SerializeField] private Button newButton;

    [SerializeField] private SaveAsUI saveAsUI;
    [SerializeField] private OpenUI openUI;

    [Header("MapSizeArea")]
    [SerializeField] private Text xSize;
    [SerializeField] private Text ySize;
    [SerializeField] private Button xSizeUpButton;
    [SerializeField] private Button xSizeDownButton;
    [SerializeField] private Button ySizeUpButton;
    [SerializeField] private Button ySizeDownButton;

    [Header("Toggle")]
    [SerializeField] private Toggle gridOnToggle;
    [SerializeField] private Toggle counterToggle;

    [Header("GroundArea")]
    [SerializeField] private Button[] groundButtons;

    [Header("CreatureArea")]
    [SerializeField] private Button[] creatureButtons;

    [Header("ETC")]
    [SerializeField] private MapEditorMK2 mapEditor;
    [SerializeField] private LineGrid lineGrid;

    public int currentMapIndex;

    private int _currentXSize;
    private int _currentYSize;
    public int CurrentXSize
    {
        get { return _currentXSize; }
        set
        {
            _currentXSize = value;
            xSize.text = _currentXSize.ToString();
            lineGrid.MakeGrid(-19.5f, -9.5f, _currentXSize, _currentYSize);
        }
    }
    public int CurrentYSize
    {
        get { return _currentYSize; }
        set
        {
            _currentYSize = value;
            ySize.text = _currentYSize.ToString();
            lineGrid.MakeGrid(-19.5f, -9.5f, _currentXSize, _currentYSize);
        }
    }


    private void Awake()
    {
        currentMapIndex = -1;

        CurrentXSize = 40;
        CurrentYSize = 20;
        lineGrid.MakeGrid(-19.5f, -9.5f, CurrentXSize, CurrentYSize);
        SetOnClickButtonEvent();
        SetOnCheckToggleEvent();
    }

    public void InteractiveSaveButton()
    {
        saveButton.interactable = true;
    }

    private void SetOnClickButtonEvent()
    {
        // File 버튼
        saveButton.onClick.AddListener(() => mapEditor.SaveMap(currentMapIndex));
        saveAsButton.onClick.AddListener(() => saveAsUI.gameObject.SetActive(true));
        openButton.onClick.AddListener(() => openUI.gameObject.SetActive(true));
        newButton.onClick.AddListener(() => mapEditor.InitMap());
        newButton.onClick.AddListener(() => CurrentXSize = 40);
        newButton.onClick.AddListener(() => CurrentYSize = 20);

        // 사이즈 조절 버튼
        xSizeUpButton.onClick.AddListener(() => CurrentXSize++);
        xSizeDownButton.onClick.AddListener(() => CurrentXSize--);
        ySizeUpButton.onClick.AddListener(() => CurrentYSize++);
        ySizeDownButton.onClick.AddListener(() => CurrentYSize--);

        xSizeUpButton.onClick.AddListener(() => mapEditor.ResizeXMap(1));
        xSizeDownButton.onClick.AddListener(() => mapEditor.ResizeXMap(-1));
        ySizeUpButton.onClick.AddListener(() => mapEditor.ResizeYMap(1));
        ySizeDownButton.onClick.AddListener(() => mapEditor.ResizeYMap(-1));

        // ground 버튼들
        for(int i = 0; i < groundButtons.Length; i++)
        {
            int index = i;
            groundButtons[i].onClick.AddListener(() => mapEditor.isCreatureMode = false);
            groundButtons[i].onClick.AddListener(() => mapEditor.materialIndex = index);
            groundButtons[i].onClick.AddListener(() => SetButtonOutline(index));
        }

        // creature 버튼들
        for(int i = 0; i < creatureButtons.Length; i++)
        {
            int index = i;
            creatureButtons[i].onClick.AddListener(() => mapEditor.isCreatureMode = true);
            creatureButtons[i].onClick.AddListener(() => mapEditor.creatureIndex = index);
            creatureButtons[i].onClick.AddListener(() => SetCreatureButtonOutline(index));
        }
    }

    private void SetOnCheckToggleEvent()
    {
        gridOnToggle.isOn = false;
        lineGrid.gameObject.SetActive(false);

        gridOnToggle.onValueChanged.AddListener((flag) => lineGrid.gameObject.SetActive(gridOnToggle.isOn));
    }

    private void SetButtonOutline(int index)
    {
        SetInitButtonOutline();

        for (int i = 0; i < groundButtons.Length; i++)
        {
            if (i == index)
                groundButtons[i].GetComponent<Outline>().enabled = true;
        }
    }

    private void SetCreatureButtonOutline(int index)
    {
        SetInitButtonOutline();

        for (int i = 0; i < creatureButtons.Length; i++)
        {
            if (i == index)
                creatureButtons[i].GetComponent<Outline>().enabled = true;
        }
    }

    private void SetInitButtonOutline()
    {
        for(int i = 0; i < groundButtons.Length; i++)
        {
            groundButtons[i].GetComponent<Outline>().enabled = false;
        }

        for(int i = 0; i < creatureButtons.Length; i++)
        {
            creatureButtons[i].GetComponent<Outline>().enabled = false;
        }
    }
}
