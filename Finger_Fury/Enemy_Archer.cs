using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Archer : EnemyController
{
    [Header("Extra Properties")]
    [SerializeField] GameObject arrow;
    [SerializeField] bool isNoble;
    [SerializeField] AudioClip shootSound;

    Transform arrowParent;
    Transform rotateHelper;
    GameObject arrowInstance;
    int abilityArrowCount;

    protected override void Start()
    {
        rotateHelper = transform.Find("TestBody_Body").Find("RotateHelper");
        base.Start();
        arrowParent = rotateHelper.Find("TestBody_Righthand");
        if (isNoble)
        {
            attackRange = 50;
        }
        minPushingForce = 0;
        abilityArrowCount = 0;
    }

    protected override void Attack()
    {
        isRunning = false;
    }

    void CreateArrow()
    {
        arrowInstance = Instantiate(arrow, new Vector3(transform.position.x, transform.position.y, 10), arrow.transform.rotation, arrowParent);
        arrowInstance.transform.localPosition = arrow.transform.localPosition;
        arrowInstance.transform.localRotation = Quaternion.Euler(0, 0, 90);
    }

    void ShootArrow()
    {
        if (!isHit)
        {
            audioCont.PlaySound(shootSound);
            ArrowController arrowCont = arrowInstance.GetComponent<ArrowController>();
            if (abilityIndex == 4 && !isNoble) { arrowCont.SetRicochetAbility(1, this); }
            arrowCont.Shoot((200 + (50 * timeFactor)), isNoble, true, player.transform.position);
        }
    }

    IEnumerator WaitToShoot()
    {
        yield return new WaitForSeconds(1 - timeFactor);
        anim.SetTrigger("Attack");
    }

    public void AbilityArrowPlus()
    {
        abilityArrowCount++;
        if (abilityArrowCount >= 5) { TriggerDie(); }
    }
}
