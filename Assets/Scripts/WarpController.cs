using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpController : MonoBehaviour
{
    Animator animator;

    public Transform target;

    public Transform sword;
    public Transform swordHand;
    private Vector3 swordOrigRot;
    private Vector3 swordOrigPos;

    public float warpDuration = 0.5f;

    public Material glowMaterial;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;

        animator = GetComponent<Animator>();

        swordOrigPos = sword.localPosition;
        swordOrigRot = sword.localEulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("Slash");
        }
    }

    void Warp()
    {
        GameObject clone = Instantiate(gameObject, transform.position, transform.rotation);
        Destroy(clone.GetComponent<WarpController>().sword.gameObject);
        Destroy(clone.GetComponent<Animator>());
        Destroy(clone.GetComponent<WarpController>());
        Destroy(clone.GetComponent<AnimationStateController>());

        SkinnedMeshRenderer[] skinnedMeshes = clone.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer smr in skinnedMeshes)
        {
            smr.material = glowMaterial;
            smr.material.DOFloat(2, "Vector1_2E9861A3", 5f).OnComplete(() => Destroy(clone));
        }

        ShowBody(false);
        animator.speed = 0;

        transform.DOMove(target.position, warpDuration).SetEase(Ease.InExpo).OnComplete(()=>FinishWarp());

        sword.parent = null;
        sword.DOMove(target.position, warpDuration / 1.2f);
        sword.DOLookAt(target.position, .2f, AxisConstraint.None);
    }

    void FinishWarp()
    {
        SkinnedMeshRenderer[] skinnedMeshes = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach(SkinnedMeshRenderer smr in skinnedMeshes)
        {
            GlowAmount(30);
            DOVirtual.Float(30, 0, 0.5f, GlowAmount);
        }

        ShowBody(true);
        animator.speed = 1;

        sword.parent = swordHand;
        sword.localPosition = swordOrigPos;
        sword.localEulerAngles = swordOrigRot;
    }

    void ShowBody(bool state)
    {
        SkinnedMeshRenderer[] skinnedMeshes = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer smr in skinnedMeshes)
        {
            smr.enabled = state;
        }
    }

    void GlowAmount(float x)
    {
        SkinnedMeshRenderer[] skinnedMeshes = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach(SkinnedMeshRenderer smr in skinnedMeshes)
        {
            smr.material.SetVector("_FresnelAmount", new Vector4(x, x, x, x));
        }
    }
}
