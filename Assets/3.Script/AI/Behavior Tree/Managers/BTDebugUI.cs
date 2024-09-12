using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BTDebugUI : MonoBehaviour
{
    private BehaviorTree _tree;
    private Helper _helper;
    private EnemyBT _enemy;
    [SerializeField] private Text text;

    private void Start()
    {
        text.text = "";
    }


    private void Update()
    {
        if(_tree==null)
        {
            _helper = FindObjectOfType<Helper>();
            _enemy = FindObjectOfType<EnemyBT>();
            if(_enemy!=null)
            {
                _tree = _enemy.GetComponent<BehaviorTree>();

            }
        }
        else
        {
            text.text = _tree.GetDebugText();

        }
    }

}
