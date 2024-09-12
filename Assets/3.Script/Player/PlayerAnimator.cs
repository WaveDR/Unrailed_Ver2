using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public Animator anim;
    private PlayerInput input;
    private PlayerController control;

    private void Awake()
    {
        TryGetComponent(out anim);
        TryGetComponent(out input);
        TryGetComponent(out control);
    }
    // Update is called once per frame
    void Update()
    {
        InputBase();

        if (ShopManager.Instance.isPlayerShop) return;

        PickUpAnim();
        AttackAnim();
    }
    void InputBase()
    {
        if (input.Dir != Vector3.zero)
        {
            anim.SetBool("isMove", true);
        }
        else anim.SetBool("isMove", false);
    }

    private void PickUpAnim()
    {
        if (control.CurrentHandItem == null)
        {
            anim.SetBool("isTwoHandsPickUp", false);
        }

        else
        {
            if (control.CurrentHandItem.EquipPart == EEquipPart.twoHand)
            {
                anim.SetBool("isTwoHandsPickUp", true);
            }
            else
            {
                anim.SetBool("isTwoHandsPickUp", false);
            }
        }
    }

    private void AttackAnim()
    {
        if (control.CurrentFrontObject == null)
        {
            Debug.Log("앞에 업서용");
            anim.SetBool("isAttack", false);
        }
        else
        {
            Debug.Log(control.CurrentFrontObject.gameObject.layer);

            if (control.CurrentFrontObject.gameObject.layer == LayerMask.NameToLayer("diggable")
                || control.CurrentFrontObject.gameObject.layer == LayerMask.NameToLayer("attackable"))
            {
                Debug.Log("때려요");
                anim.SetBool("isAttack", true);
            }

            else
            {
                Debug.Log("때리는게 아니네요");
                anim.SetBool("isAttack", false);
            }
        }
    }
}