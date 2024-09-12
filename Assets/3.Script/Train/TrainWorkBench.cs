using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainWorkBench : TrainMovement
{
    // Start is called before the first frame update

    TrainBox box;

    public Animator anim;
    [SerializeField] GameObject prefabsRail;
    public Transform railSpawnPos;
    public RailPooling railPool;
    [SerializeField] float madeTime;

    public int spawnIndex;
    public int spawnMaxIndex;
    public float spawnSpeed;

    bool _isAnim;

    private void Awake()
    {
        railPool = FindObjectOfType<RailPooling>();
        box = FindObjectOfType<TrainBox>();
        anim = GetComponent<Animator>();
        GetMesh();
        fireEffect.gameObject.SetActive(false);
        TrainUpgrade();

    }
    // Update is called once per frame
    void Update()
    {
        TrainMovePos();
        anim.SetBool("isBurn", isBurn);

        MakingRail();
    }

    /*public void MakeRail(ref List<BoxItem> wood, ref List<BoxItem> steel)
    {

        //if (wood.Count <= 0 || steel.Count <= 0 || spawnIndex >= spawnMaxIndex) return;
        if (wood.Count <= 0 || steel.Count <= 0 || spawnIndex >= spawnMaxIndex) return;

        else
        {
            madeTime += Time.deltaTime;

            switch (spawnIndex - 1)
            {
                case -1:
                    StartCoroutine(AnimDelay());
                    break;
                case 0:
                    anim.SetInteger("GetRails", 2);
                    break;
                case 1:
                    anim.SetInteger("GetRails", 3);
                    break;
                default: break;
            }
            if (madeTime >= spawnSpeed)
            {
                spawnIndex++;
                railPool.TransformRail(railSpawnPos, true);
                wood.RemoveAt(0);
                steel.RemoveAt(0);
                _isAnim = false;
                madeTime = 0;
            }
        }
    }*/

    public void MakeRail(ref Stack<MyItem> wood, ref Stack<MyItem> steel)
    {

        //if (wood.Count <= 0 || steel.Count <= 0 || spawnIndex >= spawnMaxIndex) return;
        if (wood.Count <= 0 || steel.Count <= 0 || spawnIndex >= spawnMaxIndex) return;

        else
        {
            madeTime += Time.deltaTime;

            switch (spawnIndex - 1)
            {
                case -1:
                    StartCoroutine(AnimDelay());
                    break;
                case 0:
                    anim.SetInteger("GetRails", 2);
                    break;
                case 1:
                    anim.SetInteger("GetRails", 3);
                    break;
                default: break;
            }
            if (madeTime >= spawnSpeed)
            {
                spawnIndex++;
                railPool.TransformRail(railSpawnPos, true);

                Destroy(wood.Pop().gameObject);
                Destroy(steel.Pop().gameObject);
                _isAnim = false;
                madeTime = 0;
                SoundManager.Instance.StopSoundEffect("Rail_Create");
                SoundManager.Instance.PlaySoundEffect("Rail_Create");
            }
        }
    }

    IEnumerator AnimDelay()
    {
        if (!_isAnim)
        {
            anim.SetInteger("GetRails", 0);
            yield return new WaitForSeconds(0.001f);
            anim.SetInteger("GetRails", 1);
            _isAnim = true;
        }
    }
    public void MakingRail()
    {
        if (!isBurn)
        {
            // MakeRail(ref box.woods, ref box.steels);
            MakeRail(ref box.woodStack, ref box.steelStack);
        }
        //스폰되는 레일 로직 쓰기

        if (spawnIndex > 0 && !_isPlay && !isReady && !isGoal && !isBurn && !isOver)
        {
            StartCoroutine(Warning());
        }
    }
    public override void TrainUpgrade()
    {
        base.TrainUpgrade();
        //업그레이드 메서드
        switch (trainUpgradeLevel)
        {
            case 1:
                spawnSpeed = 2.5f;
                spawnMaxIndex = 3;
                break;
            case 2:
                spawnSpeed = 1.8f;
                spawnMaxIndex = 5;
                break;
            default:
                spawnSpeed = 0;
                spawnMaxIndex = 6;
                break;
        }
    }
}