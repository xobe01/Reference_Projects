using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class EnemyController : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] protected int attackRange;
    [SerializeField] protected bool turnBeforeAttack;
    [SerializeField] protected int ID;

    [Header("SpawnPoints")]
    [SerializeField] bool isTower;
    [SerializeField] bool isSecondFloor;
    [SerializeField] bool isOutOfScreen;

    AudioClip fireAbilitySound;
    List<AudioClip> escapeScreams;
    protected float minPushingForce = 0;
    protected float secondFloorHeight;
    float maxCollisionPower;
    float runningSpeed;
    protected GameController gameCont;
    protected AudioController audioCont;
    protected GameObject player;
    protected Rigidbody2D rig;
    protected Animator anim;
    protected Collider2D col;
    Collider2D bodyCol;
    float levelBoundary;
    protected SpawnController spawnCont;
    protected List<HingeJoint2D> joints;
    List<Rigidbody2D> rigs;
    protected List<AudioClip> hitSounds;
    GameObject hitEffect;
    GameObject fireAbilityMarker;
    protected bool isImmortal;
    protected bool isRunning;
    protected bool isHit;
    protected bool toLeft = true;
    protected float timeFactor;
    protected float otherDestination;
    protected bool isEscaping;
    protected bool toSomewhereElse;
    protected bool toLadder;
    protected bool hasAttacked;
    protected bool gameOver;
    protected bool isAmbient;
    protected bool canRun;
    bool isFireAbilityActive;
    bool isElectricAbilityActive;
    bool isDead;
    float freezeTime;
    protected int abilityIndex;
    protected bool canAttack;

    protected virtual void Start()
    {
        levelBoundary = 270;
        canRun = true;
        rigs = new List<Rigidbody2D>();
        hitSounds = new List<AudioClip>();
        hitEffect = Resources.Load("HitEffect") as GameObject;
        escapeScreams = new List<AudioClip>();
        audioCont = FindObjectOfType<AudioController>();
        runningSpeed = 20 * Random.Range(0.75f, 1.25f);
        runningSpeed = 20 * Random.Range(0.75f, 1.25f);
        bodyCol = transform.Find("TestBody_Body").GetComponent<Collider2D>();
        foreach(Rigidbody2D r in GetComponentsInChildren<Rigidbody2D>())
        {
            if(r.GetComponent<WeaponController>()==null && r.GetComponent<GadgetController>() == null) { rigs.Add(r); }
        }
        joints = new List<HingeJoint2D>();
        foreach(HingeJoint2D h in GetComponentsInChildren<HingeJoint2D>()) { joints.Add(h); }
        gameCont = FindObjectOfType<GameController>();
        spawnCont = FindObjectOfType<SpawnController>();
        if (!isAmbient)
        {
            player = FindObjectOfType<PlayerController>().gameObject;
        }        
        col = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        rig = GetComponent<Rigidbody2D>();
        fireAbilitySound = Resources.Load("FireBall") as AudioClip; 
        TasksBeforeStart();
        CheckAbility(false);
        isElectricAbilityActive = abilityIndex == 10;
        freezeTime = 0;
    }

    void TasksBeforeStart()
    {
        escapeScreams.Add(Resources.Load("EscapeScream_01") as AudioClip);
        escapeScreams.Add(Resources.Load("EscapeScream_02") as AudioClip);
        escapeScreams.Add(Resources.Load("EscapeScream_03") as AudioClip);
        escapeScreams.Add(Resources.Load("EscapeScream_04") as AudioClip);
        hitSounds.Add(Resources.Load("Punch01") as AudioClip);
        hitSounds.Add(Resources.Load("Punch02") as AudioClip);
        hitSounds.Add(Resources.Load("Punch03") as AudioClip);
        Vanish(true);
        isRunning = true;
        maxCollisionPower = 300;
        secondFloorHeight = spawnCont.GetSecondFloorHeight();
        RunSomewhere();
        StartCoroutine(StartWait());
    }

    IEnumerator StartWait()
    {
        yield return new WaitForSeconds(1);
        canAttack = true;
    }

    protected virtual void CheckAbility(bool onEscape)
    {
        if (!onEscape)
        {
            if (abilityIndex == 8)
            {
                if (Random.Range(0, 100) >=85) { FireAbility(); }
            }
        }
    }

    public void RunSomewhere()
    {
        if(canRun)
        {
            anim.SetTrigger("Run");
            isRunning = false;
            isRunning = true;
            if (toLadder)
            {
                if (transform.position.y < secondFloorHeight - 10)
                {
                    StartCoroutine(RunToLadder());
                    return;
                }
            }
            else if (isAmbient)
            {
                if (isEscaping) { Escape(true); }
                else
                {
                    otherDestination = transform.position.x * -5;
                    StartCoroutine(RunSomewhereElse(false));
                }
            }
            else if (isEscaping)
            {
                Escape(false);
                return;
            }
            else
            if (toSomewhereElse) { StartCoroutine(RunSomewhereElse(true)); }
            else { StartCoroutine(RunToPlayer()); }
        }        
    }

    public void Ragdoll()
    {
        anim.enabled = false;
        rig.freezeRotation = false;        
        bodyCol.isTrigger = false;
        foreach (Rigidbody2D r in rigs) { if (r != null) { r.isKinematic = false; } }
        foreach (HingeJoint2D h in joints) { h.enabled = true; }
    }

    IEnumerator RunToPlayer()
    {
        Vector2 diff = player.transform.position - transform.position;
        while (!isHit && (Mathf.Abs(diff.x) > attackRange || Mathf.Abs(transform.position.x)>levelBoundary) && isRunning)
        {
            if (player == null) { break; }
            diff = player.transform.position - transform.position;
            Run(Mathf.Clamp(player.transform.position.x,-levelBoundary,levelBoundary));
            yield return null;
        }
        if (!isHit && Mathf.Abs(diff.x) <= attackRange)
        {
            TriggerAttack();
            if (turnBeforeAttack) { StartCoroutine(TurnAtPlayer()); }
        }
    }

    IEnumerator RunSomewhereElse(bool attackWhenReached)
    {
        float diff = otherDestination - transform.position.x;
        while (Mathf.Abs(diff) > 1 && !isHit && isRunning)
        {
            diff = otherDestination - transform.position.x;
            Run(otherDestination);
            yield return null;
        }
        if (!isHit && Mathf.Abs(diff) <= 1)
        {
            Stop();
            if (attackWhenReached) { TriggerAttack(); }            
        }
        yield return null;
    }

    IEnumerator RunToLadder()
    {
        float closestLadder = spawnCont.GetClosestLadder(transform.position.x);
        float diff = Mathf.Abs(closestLadder - transform.position.x);
        while (diff > 1 && !isHit && isRunning)
        {
            Run(closestLadder);
            diff = Mathf.Abs(closestLadder - transform.position.x);
            yield return null;
        }
        if (!isHit && diff <= 1)
        {
            Stop();
            transform.position = new Vector2(closestLadder, transform.position.y);
            StartCoroutine(ClimbLadder());            
        }
    }

    protected void Escape(bool isScared)
    {
        gameObject.layer = 10;
        otherDestination = spawnCont.GetNearestExit(this);        
        if (isScared)
        {
            CheckAbility(true);
            Drop();
            anim.ResetTrigger("Run");
            anim.SetTrigger("Escape");
            Transform rotateHelper = transform.Find("TestBody_Body").Find("RotateHelper");
            if (rotateHelper != null)
            {
                rotateHelper.rotation = Quaternion.identity;
                rotateHelper.localPosition = Vector3.zero;
            }
            if (FindObjectOfType<PlayerController>() != null) { player = FindObjectOfType<PlayerController>().gameObject; }
        }
        else
        {
            anim.SetTrigger("Run");
            anim.speed = 1;
        }
        StartCoroutine(RunSomewhereElse(false));
        StartCoroutine(WaitToEscape(otherDestination,isScared));
    }

    IEnumerator WaitToEscape(float destination, bool isScared)
    {
        if (isScared)
        {
            yield return new WaitForSeconds(Random.Range(0, 0.5f));
            audioCont.PlaySound(escapeScreams[Random.Range(0,escapeScreams.Count)]);
        }        
        while (Mathf.Abs(transform.position.x-destination)>1 && !isHit)
        {
            yield return null;
        }        
        if (!isHit)
        {
            Stop();
            transform.position = new Vector2(destination, transform.position.y);
            isImmortal = true;
            rig.gravityScale = 0;
            col.isTrigger = true;
            Vanish(false);            
            yield return new WaitForSeconds(1);
            Destroy(gameObject);
         }
    }

    IEnumerator TurnAtPlayer()
    {
        hasAttacked = false;
        while (!isEscaping && !isHit && !hasAttacked)
        {
            if (player == null) { break; }
            if( player.transform.position.x > transform.position.x && toLeft)
            {
                toLeft = false;
                transform.Rotate(0, 180, 0);
                transform.position = new Vector3(transform.position.x, transform.position.y, 50);
            }
            else if (player.transform.position.x < transform.position.x && !toLeft)
            {
                toLeft = true;
                transform.Rotate(0, 180, 0);
            }
            yield return null;
        }
    }

    void Run(float value)
    {        
        bool leftDirection;
        if (transform.position.x > value)
        {
            leftDirection=true;
        }
        else
        {
            leftDirection=false;
        }
        float runDirection;
        runDirection = leftDirection ? -1 : 1;
        rig.velocity = new Vector2(runDirection * runningSpeed, 0);
        if (toLeft && !leftDirection)
        {
            toLeft = false;
            transform.Rotate(0, 180, 0);
            transform.position = new Vector3(transform.position.x, transform.position.y, 50);
        }
        else if(!toLeft && leftDirection)
        {
            toLeft = true;
            transform.Rotate(0, 180, 0);
            transform.position = new Vector3(transform.position.x, transform.position.y, 50);
        }
    }

    void TriggerAttack()
    {
        if (canAttack)
        {
            anim.ResetTrigger("Run");
            anim.SetTrigger("Attack");
            Attack();
        }
        else
        {
            StartCoroutine(CantAttack());
        }
    }

    IEnumerator CantAttack()
    {
        while (!canAttack) { yield return null; }
        TriggerAttack();
    }

    protected abstract void Attack();

    protected void JumpOnPlayer(float jumpFactor, bool passesPlayer, bool differentSpeed)
    {
        foreach (SpriteRenderer s in GetComponentsInChildren<SpriteRenderer>()) { s.sortingOrder += 10; }
        Vector2 diff = player.transform.position - transform.position;
        isRunning = false;
        float force;
        if (differentSpeed)
        {
            force = diff.y <= 0 ? 1 : 4;
            if (diff.y < 0) { rig.velocity = Vector3.zero; }
        }
        else
        {
            force = 4;
        }
        force *= jumpFactor;
        gameObject.layer = passesPlayer? 18 : 14;
        rig.AddForce(diff.normalized * force);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player" && collision.gameObject.GetComponent<Rigidbody2D>().velocity.magnitude > minPushingForce && !isImmortal)
        {
            PlayerCollision(collision.gameObject.GetComponent<Rigidbody2D>().velocity.normalized);            
        }

        else if(collision.gameObject.tag == "Ground")
        {
            GroundCollision();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            if (rig != null) { rig.drag = 0; }            
        }
    }

    protected virtual void GroundCollision()
    {
        if (isHit && rig!=null) { rig.drag = 10; }
    }

    protected virtual void PlayerCollision(Vector3 collisionDirection)
    {
        if (!isHit)
        {
            isHit = true;
            if (hitEffect != null) { Instantiate(hitEffect, new Vector3(transform.position.x, transform.position.y, -50), transform.rotation); }
            if (isElectricAbilityActive && Random.Range(0,10)<10) { ElectricDeath(); }
            if (isFireAbilityActive) { FireExplosion(collisionDirection); }
            StartCoroutine(Die());               
        }        
    }

    public void TriggerDie()
    {
        StartCoroutine(Die());
    }

    protected virtual IEnumerator Die()
    {
        if(!isDead)
        {
            isDead = true;
            gameCont.EnemyKilled(ID, isHit, isEscaping, transform.position);
            isHit = true;   
            foreach (SpriteRenderer s in GetComponentsInChildren<SpriteRenderer>()) { s.sortingOrder += 10; }
            Ragdoll();
            Drop();
            audioCont.PlaySound(hitSounds[Random.Range(0, hitSounds.Count)]);
            foreach (SoundSourceController s in GetComponentsInChildren<SoundSourceController>()) { s.SetVolume(false); }
            foreach (Transform t in GetComponentInChildren<Transform>())
            {
                t.gameObject.layer = 10;
            }
            yield return new WaitForSeconds(0.5f);
            foreach (Transform t in GetComponentInChildren<Transform>())
            {
                t.gameObject.layer = 13;
            }
            while (rig.velocity.magnitude > 0.5f)
            {
                yield return null;
            }
            Vanish(false);
            yield return new WaitForSeconds(1);
            Destroy(gameObject);
        }        
    }

    void Drop()
    {
        foreach (GadgetController g in GetComponentsInChildren<GadgetController>())
        {
            g.Drop(true);
        }
        foreach (WeaponController w in GetComponentsInChildren<WeaponController>())
        {
            w.Drop(true);
        }
    }

    protected void Stop()
    {
        isRunning = false;
        rig.velocity = Vector2.zero;
    }

    protected void Vanish(bool inverse)
    {
        foreach (SpriteRenderer s in GetComponentsInChildren<SpriteRenderer>())
        {
            StartCoroutine(FadeColor(s, inverse));
        }
    }

    IEnumerator FadeColor(SpriteRenderer spriteRenderer, bool inverse)
    {
        if(spriteRenderer!=null)
        {
            Color defaultColor = spriteRenderer.material.color;
            if (!inverse)
            {
                for (float i = 0; i < 0.5f; i += Time.deltaTime)
                {
                    if (spriteRenderer != null) { spriteRenderer.material.color = new Color(defaultColor.r, defaultColor.g, defaultColor.b, 1 - i*2); }
                    yield return null;
                }
                if (spriteRenderer != null)
                { spriteRenderer.material.color = new Color(defaultColor.r, defaultColor.g, defaultColor.b, 0); }
            }
            else
            {
                for (float i = 0; i < 0.5f; i += Time.deltaTime)
                {
                    if (spriteRenderer != null)
                    { spriteRenderer.material.color = new Color(defaultColor.r, defaultColor.g, defaultColor.b, i*2); }
                    yield return null;
                }
                if (spriteRenderer != null)
                { spriteRenderer.material.color = new Color(defaultColor.r, defaultColor.g, defaultColor.b, 1); }
            }
        }        
    }

    IEnumerator ClimbLadder()
    {
        anim.ResetTrigger("Run");
        anim.SetTrigger("Climb");
        gameObject.layer = 14;
        float secondFloorHeight = spawnCont.GetSecondFloorHeight();
        while (!isHit && transform.position.y < secondFloorHeight)
        {
            rig.velocity = new Vector2(0, 25);
            yield return null;
        }
        if (!isHit)
        {
            anim.ResetTrigger("Climb");
            anim.SetTrigger("Run");
            Stop();
            gameObject.layer = 10;
            toLadder = false;
            RunSomewhere();
        }
    }

    public bool GetToLeft()
    {
        return toLeft;
    }

    public bool GetIsTower()
    {
        return isTower;
    }

    public bool GetIsOutOfScreen()
    {
        return isOutOfScreen;
    }

    public bool GetIsSecondFloor()
    {
        return isSecondFloor;
    }

    public float GetMaxCollisionPower()
    {
        return maxCollisionPower;
    }

    public void SetTimeFactor(float timeFactor)
    {
        this.timeFactor = timeFactor;
    }

    public void SetIsImmortal(bool newValue)
    {
        isImmortal = newValue;
    }    

    public bool GetIsHit()
    {
        return isHit;
    }

    public int GetID()
    {
        return ID;
    }

    public void GameOver()
    {
        gameOver = true;
        isEscaping = true;
        RunSomewhere();
    }

    public void SetIsAmbient(bool newValue)
    {
        isAmbient = newValue;
    }

    public void TriggerEscape()
    {
        isEscaping = true;
        RunSomewhere();
    }

    public IEnumerator Illuminate(float waitTime,float currentTime)
    {
        List<Color> defaultColors = new List<Color>();
        List<SpriteRenderer> renderers = new List<SpriteRenderer>();
        
        foreach (SpriteRenderer s in GetComponentsInChildren<SpriteRenderer>())
        {
            Color defaultColor = s.color;
            defaultColors.Add(defaultColor);
            s.color = currentTime <= 1? new Color(defaultColor.r * (currentTime / waitTime), defaultColor.g * (currentTime / waitTime), defaultColor.b * (currentTime / waitTime)):
                Color.black;
            renderers.Add(s);
        }
        if (currentTime <= 1)
        {
            yield return new WaitForSeconds(1);
        }
        for (float i = 0; i < waitTime - currentTime; i += Time.deltaTime)
        {
            for (int j = 0; j < renderers.Count; j++)
            {
                float currentShade = (currentTime + i) / waitTime;
                Color defaultColor = defaultColors[j];
                renderers[j].color = new Color(defaultColor.r * currentShade, defaultColor.g * currentShade, defaultColor.b * currentShade, 1);
            }
            yield return null;
        }
        for (int j = 0; j < renderers.Count; j++)
        {
            renderers[j].color = defaultColors[j];
        }
    }

    public void HitByShockWave(Vector2 diff)
    {
        TriggerDie();
        foreach(Rigidbody2D r in rigs)
        {
            if (r != null) { r.velocity = diff * 10 / (diff.magnitude / 10); }
        }
    }

    public void SetAbilityIndex(int ability)
    {
        abilityIndex = ability;
    }

    public void Freeze()
    {
        StartCoroutine(FreezeEnumerator());
    }

    IEnumerator FreezeEnumerator()
    {
        if(freezeTime == 0)
        {
            freezeTime = 2;
            Stop();
            Instantiate(Resources.Load("SnowEffect") as GameObject, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
            anim.speed = 0;
            canAttack = false;
            foreach (SpriteRenderer s in GetComponentsInChildren<SpriteRenderer>()) { s.color = new Color(0, 0, 0.5f, 1); }
            foreach (RotateTowards r in GetComponentsInChildren<RotateTowards>()) { r.SetIsRotating(false); }
            foreach (WeaponController w in GetComponentsInChildren<WeaponController>()) { w.SetCanAttack(false); }
            while (freezeTime > 0) 
            {
                freezeTime -= Time.deltaTime;
                yield return null;
            }
            freezeTime = 0;
            anim.speed = 1;
            RunSomewhere();
            canAttack = true;
            foreach (SpriteRenderer s in GetComponentsInChildren<SpriteRenderer>()) { s.color = Color.white; }
            foreach (RotateTowards r in GetComponentsInChildren<RotateTowards>()) { r.SetIsRotating(true); }
            foreach (WeaponController w in GetComponentsInChildren<WeaponController>()) { w.SetCanAttack(true); }
        }
        else
        {
            freezeTime = 2;
        }
    }

    public bool GetIsDead()
    {
        return isDead;
    }

    void FireAbility()
    {
        foreach (SpriteRenderer s in GetComponentsInChildren<SpriteRenderer>()) { s.color = new Color(1, 0, 0, 1); }
        isFireAbilityActive = true;
        fireAbilityMarker = Instantiate(Resources.Load("FireAbility") as GameObject, transform.position + Vector3.up * 15, Quaternion.identity, transform);
    }

    void FireExplosion(Vector3 direction)
    {
        audioCont.PlaySound(fireAbilitySound);
        spawnCont.Explosion(transform.position,25,this);
        int rnd = Random.Range(10,15);
        for (int i = 0; i < rnd; i++)
        {
            GameObject instance = Instantiate(Resources.Load("FireAbilityDebris") as GameObject, transform.position, Quaternion.identity);
            instance.GetComponent<Rigidbody2D>().AddForce((direction + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)))
                * 10000);
        }
        Destroy(fireAbilityMarker);
        Destroy(gameObject);
    }

    public void ElectricDeath()
    {
        Instantiate(Resources.Load("Electricity") as GameObject, transform.position, Quaternion.identity, transform.Find("TestBody_Body")   );
        audioCont.PlaySound(Resources.Load("ElectricShock") as AudioClip);
        foreach (SpriteRenderer s in GetComponentsInChildren<SpriteRenderer>()) { s.color = new Color(0, 1, 1, 1); }
        foreach (EnemyController e in spawnCont.GetCurrentEnemies())
        {
            if (!e.GetIsDead() && (transform.position-e.transform.position).magnitude<50)
            {
                e.TriggerDie();
                e.ElectricDeath();
            }
        }
    }
}
