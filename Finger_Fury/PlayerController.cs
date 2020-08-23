using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] GameObject explosion;
    [SerializeField] GameObject shockWave;
    [SerializeField] GameObject comboExplosion;
    [SerializeField] GameObject[] abilityEffects;
    [SerializeField] AudioClip comboBomb;
    [SerializeField] GameObject arrow;
    [SerializeField] AudioClip healthSound;

    GameController gameCont;
    Rigidbody2D rig;
    Vector2 position;
    CameraController camera;
    HealthBarController healthBarCont;
    ParticleSystem[] fireParticles;
    UIController UICont;
    SpawnController spawnCont;
    ChallengeController challengeCont;
    AbilityController abilityCont;
    AudioController audioCont;
    //AudioSource audioSource;

    bool comboExplosionAbility;
    const int defaultLife = 40; //40
    float[] damageTaken;
    bool isBurning;
    float burnTime;
    float life;
    bool isFollowing;
    bool isMobileBuild;
    bool gameOver;
    bool hasQuit;
    bool isPaused;
    int comboCount;
    int comboSum;
    float comboWaitTime;
    bool comboBreaked;
    bool stoppingSound;
    bool oneMoreChance;
    GameObject effect;

    void Awake()
    {
        audioCont = FindObjectOfType<AudioController>();
        abilityCont = FindObjectOfType<AbilityController>();
        challengeCont = FindObjectOfType<ChallengeController>();
        spawnCont = FindObjectOfType<SpawnController>();
        UICont = FindObjectOfType<UIController>();
        isFollowing = true;
        fireParticles = GetComponentsInChildren<ParticleSystem>();
        healthBarCont = FindObjectOfType<HealthBarController>();
        float[] zeroDataFloat = new float[11];
        for (int i = 0; i < zeroDataFloat.Length; i++) { zeroDataFloat[i] = 0; }
        damageTaken = zeroDataFloat;
        gameCont = FindObjectOfType<GameController>();
        life = defaultLife;
        camera = FindObjectOfType<CameraController>();
        transform.position = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, 0);
        rig = GetComponent<Rigidbody2D>();
        position = transform.position;
        isMobileBuild = (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) ? true : false;
        StartCoroutine(RestartPosition());
        effect = Instantiate(abilityEffects[abilityCont.GetActiveAbility()], transform);
        effect.transform.localPosition = Vector3.zero;
        //audioSource = effect.GetComponentInChildren<AudioSource>();
        comboCount = 0;
        comboSum = 0;
    }

    void FixedUpdate()
    {
        if(isFollowing)
        {
            Vector2 deviceInput = isMobileBuild ? Input.GetTouch(0).position : new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            Vector2 touchPosition = Camera.main.ScreenToWorldPoint(deviceInput);
            Vector2 diff = touchPosition - position;
            rig.velocity = new Vector2(diff.x, diff.y) * 50;
            
            position = new Vector3(touchPosition.x, touchPosition.y, 10);
            if (Camera.main.ScreenToViewportPoint(deviceInput).x > 0.75f)
            {
                camera.MoveSight(false);
            }
            else if (Camera.main.ScreenToViewportPoint(deviceInput).x < 0.25f)
            {
                camera.MoveSight(true);
            }
            /*if (audioSource != null)
            {
                if (rig.velocity != Vector2.zero)
                {
                    if (!audioSource.isPlaying) { StartCoroutine(StartSound()); }
                }
                else
                {
                    if (audioSource.isPlaying && !stoppingSound)
                    {
                        stoppingSound = true;
                        StartCoroutine(StopSoundDelay());
                    }                    
                }
            }*/            
        }        
    }

    /*IEnumerator StartSound()
    {
        audioSource.Play();
        for (float i = 0; i < 0.2f; i += Time.deltaTime)
        {
            audioSource.volume = 5 * i;
            if (rig.velocity == Vector2.zero) { break; }
            yield return null;
        }
        audioSource.volume = 1;
    }*/

    /*IEnumerator StopSoundDelay()
    {
        float time = 0;
        while(rig.velocity == Vector2.zero && time<0.1f)
        {
            time += Time.deltaTime;
            yield return null;
        }
        if(rig.velocity == Vector2.zero)
        {
            for (float i = 0; i < 0.2f; i+=Time.deltaTime)
            {
                audioSource.volume = 1 - 5 * i;
                if(rig.velocity != Vector2.zero) { break; }
                yield return null;
            }
            if(rig.velocity==Vector2.zero)
            {
                audioSource.Stop();
            }            
        }
        stoppingSound = false;
    }*/

    public void LifeLoss(float damage, int ID)
    {
        if(life>0)
        {
            comboBreaked = true;
            float actualDamage = Random.Range(damage * 0.75f, damage * 1.25f);
            life -= actualDamage;
            damageTaken[ID] += actualDamage + Mathf.Min(0, life);
            healthBarCont.SetHealthBar(life/defaultLife);
            if (life <= 0)
            {
                Die(false);
            }
        }              
    }

    public void Die(bool isQuit)
    {        
        hasQuit = isQuit;
        isBurning = false;
        if (GetComponentInChildren<TrailRenderer>() != null) { GetComponentInChildren<TrailRenderer>().emitting = false; }
        healthBarCont.SetHealthBar(0);
        isFollowing = false;
        GetComponent<Collider2D>().enabled = false;        
        Vector2 cameraPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        gameCont.GameOver(damageTaken,cameraPos,isQuit);
        rig.velocity = new Vector2(0, 0);
        PlayerExplosionController explosionInstance = Instantiate(explosion, new Vector3(transform.position.x, transform.position.y, -20), Quaternion.identity).GetComponent<PlayerExplosionController>();
        explosionInstance.TriggerExplosion(isQuit);
        StartCoroutine(WaitToDestroy(isQuit ? 0 : (float)3/4 ,explosionInstance));
    }

    IEnumerator WaitToDestroy(float waitTime, PlayerExplosionController instance)
    {
        for (float i = 0; i < waitTime; i+=Time.deltaTime)
        {
            if (oneMoreChance) { break; }
            yield return null;
        }
        if (oneMoreChance) 
        {
            oneMoreChance = false;
            FillLife(false);
            oneMoreChance = false;
            GetComponentInChildren<TrailRenderer>().emitting = true;
            isFollowing = true;
            GetComponent<Collider2D>().enabled = true;
            Destroy(instance.gameObject);
            StartCoroutine(RestartPosition());
        }
        else
        {
            gameOver = true;
            foreach (WeaponController w in GetComponentsInChildren<WeaponController>()) { w.HitByShockWave(w.transform.position - transform.position); }
            Destroy(gameObject);
        }        
    }

    public void TriggerBurn(int ID)
    {
        StartCoroutine(Burn(ID));
    }

    public IEnumerator Burn(int ID)
    {
        foreach (ParticleSystem p in fireParticles)
        {
            if (!p.isEmitting) { p.Play(); }
        }
        if (!isBurning)
        {
            burnTime = 1;
            isBurning = true;
            while (burnTime>0)
            {
                burnTime -= Time.deltaTime;
                LifeLoss(Time.deltaTime,ID);
                yield return null;
            }
            isBurning = false;
        }
        else
        {
            burnTime += Time.deltaTime;
        }        
    }

    IEnumerator RestartPosition()
    {
        while (isFollowing)
        {
            Vector3 newPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(newPos.x, newPos.y, -30);
            yield return new WaitForSeconds(0.1f);
        }        
    }

    public void FillLife(bool isAbility)
    {
        life = defaultLife;
        StartCoroutine(healthBarCont.FillLife());
        if (isAbility) { StartCoroutine(UICont.FillLife()); }
        audioCont.PlaySound(healthSound);
    }

    public void AbilityExplosion()
    {
        spawnCont.Explosion(transform.position, 100, null);
        Instantiate(shockWave, transform.position, Quaternion.identity);
        Instantiate(comboExplosion, transform.position, Quaternion.identity);
        audioCont.PlaySound(comboBomb);
    }

    public void PauseGame()
    {
        isPaused = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
    }

    public void EnemyKilled(Vector3 position)
    {
        StartCoroutine(StartCombo(position));  //Demoltion nem hívja, ha a playeren robban
    }

    IEnumerator StartCombo(Vector3 position)
    {
        if (comboBreaked) { while (comboWaitTime != 0) { yield return null; } }
        comboBreaked = false;
        comboCount++;
        if (comboCount >= 3)
        {
            UICont.SetCombo(comboCount, position);
            comboSum++;
        }        
        comboWaitTime = 2;
        if (comboCount == 1)
        {
            while (comboWaitTime > 0 && !comboBreaked)
            {
                comboWaitTime -= Time.deltaTime;
                yield return null;
            }
            comboWaitTime = 0;
            comboCount = 0;
        }
        else if (comboCount > 3 && comboExplosionAbility)
        {
            AbilityExplosion();
        }
    }

    public void SetComboAbility()
    {
        comboExplosionAbility = true;
    }

    public void OneMoreChance()
    {
        oneMoreChance = true;
    }

    public void ArrowAbility()
    {
        float plusRot = Random.Range(-Mathf.PI / 8, Mathf.PI / 8);
        for (int i = 0; i < 8; i++)
        {
            GameObject arrowInstance = Instantiate(arrow,transform.position,Quaternion.Euler(0,0,0+i*45));
            arrowInstance.transform.localScale = arrow.transform.localScale * 2;
            ArrowController arrowCont = arrowInstance.GetComponentInChildren<ArrowController>();
            arrowCont.SetRicochetAbility(2, null);
            arrowCont.Shoot(5000, false, true, new Vector3(Mathf.Round(transform.position.x + Mathf.Sin(i * Mathf.PI / 4 + plusRot)), transform.position.y + Mathf.Sin((i - 2) * Mathf.PI / 4 + plusRot), 0));
        }
    }

    public IEnumerator ChallengeDontGetHurt(int challengeIndex, int time)
    {
        while(gameCont.GetCurrentTime()<time && life >= defaultLife && !gameOver) { yield return null; }
        if (life >= defaultLife && !gameOver) { challengeCont.ChallengeCompleted(challengeIndex); }
    }

    public IEnumerator ChallengeDontBurn(int challengeIndex)
    {
        while(!gameOver && !isBurning) { yield return null; }
        if(gameOver && !hasQuit) { challengeCont.ChallengeCompleted(challengeIndex); }
    }

    public IEnumerator ChallengeBurn(int challengeIndex, int time, bool oneGame)
    {
        float challengeTime = 0;
        if (oneGame) {
            while (!gameOver && challengeTime < time)
            {
                if (isBurning) { challengeTime += Time.deltaTime; }
                yield return null;
            }
        }
        else {
            while (!gameOver && !isPaused && challengeTime < time)
            {
                if (isBurning) { challengeTime += Time.deltaTime; }
                yield return null;
            }
        }
        if (challengeTime > time) { challengeCont.ChallengeCompleted(challengeIndex); }
        else if (!oneGame)
        {
            challengeCont.SetChallengeData(challengeIndex, (int)challengeTime);
        }
    }
    
    public IEnumerator ChallengeMakeCombo(int challengeIndex, int number, int comboMult, bool inOneGame)
    {
        int helperInt = 0;
        if (inOneGame)
        {
            while(!gameOver && helperInt < number)
            {
                if (comboCount >= comboMult)
                {
                    helperInt++;
                    if (helperInt < number)
                    {
                        while (comboCount >= comboMult && !gameOver) { yield return null; }
                    }
                }
                yield return null;
            }
        }
        else
        {
            while (comboCount >= comboMult && !gameOver && !isPaused) { yield return null; }
            while (!gameOver && !isPaused && helperInt < number)
            {
                if (comboCount >= comboMult)
                {
                    helperInt++;
                    if(helperInt < number) { while (comboCount >= comboMult && !isPaused && !gameOver) { yield return null; } }                    
                }
                yield return null;
            }
        }
        if (helperInt >= number)
        {
            challengeCont.ChallengeCompleted(challengeIndex);
        }
        else if (!inOneGame)
        {
            challengeCont.SetChallengeData(challengeIndex, helperInt);
        }
    }

    public IEnumerator ChallengeComboSum(int challengeIndex, int amount, bool inOneGame)
    {
        int helperInt = comboSum;
        if (inOneGame)
        {
            while(!gameOver && comboSum - helperInt < amount) { yield return null; }
        }
        else
        {
            while (!isPaused && !gameOver && comboSum - helperInt < amount) { yield return null; }
        }
        if (comboSum - helperInt >= amount) { challengeCont.ChallengeCompleted(challengeIndex); }
        else if (!inOneGame) { challengeCont.SetChallengeData(challengeIndex, comboSum - helperInt); }
    }

    public IEnumerator ChallengeDontGetHitBySpecifiyEnemy(int challengeIndex, int enemyIndex)
    {
        while (!gameOver) { yield return null; }
        if (!hasQuit && damageTaken[enemyIndex] == 0) { challengeCont.ChallengeCompleted(challengeIndex); }
    }
}
