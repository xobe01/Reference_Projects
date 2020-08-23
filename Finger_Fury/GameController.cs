using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] bool isSerialized;
    [SerializeField] GameObject player;
    [SerializeField] DataController dataCont;
    [SerializeField] StatController statData;
    SerializableData data;
    SpawnController spawnCont;
    UIController UICont;
    CameraController cameraCont;
    BackGroundController backGroundCont;
    AudioController audioCont;
    PlayerController playerCont;
    AbilityController abilityCont;
    ChallengeController challengeCont;
    ShopController shopCont;
    AdController adCont;

    Challenge[] waitingNewChallenges;
    int waitingExp;

    int comboSum;
    int killSum;
    int killSumByEnemies;
    int starReward;
    float startTime;
    float pauseTime;
    float currentGameTime;
    int[] currentEnemiesKilled;
    int escapingEnemiesKilled;
    int[] currentEnemiesKilledByEnemies;
    float[] currentDamageTaken;
    int adCounter;
    int adLimit;
    int maxExp;
    bool isPaused;
    bool healthAbilityOn;
    bool gameOver;
    bool hasQuit;
    bool oneMoreChance;
    bool lifeFilled;

    void Awake()
    {
        adCont = GetComponent<AdController>();
        shopCont = FindObjectOfType<ShopController>();
        adCounter = 0;
        adLimit = Random.Range(2,4);        
        challengeCont = FindObjectOfType<ChallengeController>();
        abilityCont = GetComponent<AbilityController>();
        audioCont=FindObjectOfType<AudioController>();
        cameraCont = FindObjectOfType<CameraController>();
        backGroundCont = FindObjectOfType<BackGroundController>();
        UICont = FindObjectOfType<UIController>();
        spawnCont = FindObjectOfType<SpawnController>();

        startTime = 0;
        currentEnemiesKilled = new int[11];
        currentEnemiesKilledByEnemies = new int[11];        
        currentDamageTaken = new float[11];
        SetInitialValues();
        data = new SerializableData();  
        data = SaveAndLoad.LoadData(isSerialized); 
        maxExp = shopCont.GetMaxExp();
    }

    private void Start()
    {
        TasksBeforeStart();
        PrepareToStart();
    }

    public void TriggerAd(bool isQuit, bool isRewarded)
    {
        StartCoroutine(WaitForAd(isQuit, isRewarded));
    }

    IEnumerator WaitForAd(bool isQuit, bool isRewarded)
    {
        yield return new WaitForSeconds(isQuit ? 4 : 6);
        while (!adCont.IsReady(false)) { yield return null; }
        adCont.ShowAd(isRewarded, -1);
        adCont.ShowAd(isRewarded, -1);
    }

    void TasksBeforeStart()
    {
        if (data.isFirstPlay) { data.isFirstPlay = false; }
        Input.multiTouchEnabled = false;
        float waitTime = 5;
        StartCoroutine(spawnCont.Illuminate(waitTime + 1));
        StartCoroutine(UICont.Illuminate(waitTime));
        StartCoroutine(UICont.SetExpBar(data.exp,data.level,true));
        audioCont.SetInitialMute((data.isMusicMuted ? 0 : 1) + (data.isSoundMuted ? 0 : 2));
        challengeCont.SetInitialData(data.challenges,data.remainingEasyChallenges,data.remainingMediumChallenges,data.remainingHardChallenges);
        abilityCont.SetActiveAbility(data.activeAbility);        
        shopCont.SetData(data.stars, data.ownedAbilities, data.activeAbility, data.exp);
    }    

    public void PrepareToStart()
    {        
        StartCoroutine(cameraCont.ScanLevel());
        StartCoroutine(spawnCont.CreateAmbient());        
    }

    public void StartGame()
    {
        SetInitialValues();
        playerCont = Instantiate(player).GetComponent<PlayerController>();                
        UICont.StartGame();
        abilityCont.StartGame(); //spawnCont előtt kell lennie
        StartCoroutine(spawnCont.Create());
        audioCont.SetMusic(true);
        challengeCont.StartGame();
        shopCont.StartGame();
    }

    void SetInitialValues()
    {
        hasQuit = false;
        gameOver = false;
        starReward = 0;
        killSum = 0;
        comboSum = 0;
        killSumByEnemies = 0;
        escapingEnemiesKilled = 0;
        startTime = Time.realtimeSinceStartup;
        currentGameTime = 0;
        for (int i = 0; i < currentEnemiesKilled.Length; i++)
        {
            currentEnemiesKilled[i] = 0;
            currentEnemiesKilledByEnemies[i] = 0;
        }
        for (int i = 0; i < currentDamageTaken.Length; i++) { currentDamageTaken[i] = 0; }
        //abilityCont.SetActiveAbility(10); //REMOVE
    }

    public void PauseGame()
    {
        isPaused = true;
        challengeCont.PauseGame();
        pauseTime = Time.realtimeSinceStartup;        
        audioCont.SetVolume(true);        
        playerCont.PauseGame();
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        isPaused = false;
        startTime += Time.realtimeSinceStartup - pauseTime;
        Time.timeScale = 1;
        audioCont.SetVolume(false);
        UICont.ResumeGame();
        playerCont.ResumeGame();
        challengeCont.ResumeGame();        
    }

    public void ShowRewardedAd(int type)
    {
        if (type == 1) 
        {
            Time.timeScale = 0;
            pauseTime = Time.realtimeSinceStartup;
        }
        adCont.ShowAd(true, type);
    }

    public void RewardedAdFinished(bool hasFinished,int type)
    {
        if (type == 1) //One more chance
        {
            if (hasFinished)
            {
                startTime += Time.realtimeSinceStartup - pauseTime;
                playerCont.OneMoreChance();
                cameraCont.OneMoreChance();
                UICont.OneMoreChance();
                audioCont.OneMoreChance();
                oneMoreChance = true;                
            }
            else
            {
                Time.timeScale = 1;
            }
        }
        else if(type==2) //New challenge
        {
            challengeCont.AdCompleted(hasFinished);
        }
        else if(type==3 && hasFinished) //Double stars
        {
            shopCont.DoubleReward();
        }
        if (hasFinished)
        {
            adCounter = 0;
            adLimit = 5;
        }        
    }

    public void GameOver(float[] damageTaken,Vector2 cameraPos, bool isQuit)
    {
        if (isQuit) { currentGameTime = pauseTime - startTime; }
        else { currentGameTime = (int)(Time.realtimeSinceStartup - startTime); }
        if (!isQuit)
        {
            StartCoroutine(cameraCont.ZoomOnPlayer(0.8f, cameraPos, true));
        }        
        StartCoroutine(GameOverEnumerator(cameraPos, damageTaken,isQuit));               
        audioCont.GameOver(); 
        UICont.GameOver(isQuit);       
        hasQuit = isQuit;
    }

    IEnumerator GameOverEnumerator(Vector2 cameraPos,float[] damageTaken,bool isQuit)
    {
        if (!isQuit)
        {
            Time.timeScale = 0.25f;
            for (float i = 0; i < (float)3/4; i += Time.deltaTime)
            {
                if (oneMoreChance) { break; }
                yield return null;
            }
        }
        if (oneMoreChance)
        {
            oneMoreChance = false;
        }
        else 
        {
            Time.timeScale = 1;
            cameraCont.GameOver();
            starReward = (int)killSum / 5 + (int)currentGameTime / 6;
            gameOver = true;
            challengeCont.GameOver(isQuit);
            StartCoroutine(spawnCont.GameOver(isQuit));            
            healthAbilityOn = false;
            int recordValue = (data.highScoreTime < currentGameTime ? 1 : 0) + (data.highScoreKill < killSum ? 2 : 0);
            dataCont.SetData((int)currentGameTime, killSum, currentEnemiesKilled, damageTaken, recordValue);
            SaveCurrentGameData(damageTaken, isQuit);            
            yield return new WaitForSeconds(0.5f); //TO avoid lag
            SaveAndLoad.SaveData(data);
        }        
    }

    void SaveCurrentGameData(float[] damageTaken, bool isQuit)
    {
        data.killSum += killSum;
        if (killSum > data.highScoreKill) { data.highScoreKill = killSum; }
        if ((int)currentGameTime > (int)data.highScoreTime) { data.highScoreTime = currentGameTime; }
        data.sumTime += currentGameTime;
        data.gameNumber++;
        for (int i = 0; i < data.killSpecificEnemies.Length; i++) { data.killSpecificEnemies[i] += currentEnemiesKilled[i]; }
        for (int i = 0; i < data.damageFromEnemies.Length; i++) { data.damageFromEnemies[i] += damageTaken[i]; }                
        adCounter++;        
        if (adCounter >= adLimit)
        {
            TriggerAd(isQuit, false);
            adCounter = 0;
            adLimit = Random.Range(2, 4);
        }
        data.challenges = challengeCont.GetChallenges();
        data.comboSum += comboSum;
        waitingExp = starReward + Mathf.Min(comboSum,150);
        StartCoroutine(UICont.SetExpBar(Mathf.Min(data.exp + waitingExp, maxExp), data.level, false));
        if (shopCont.GetLevelWithExp(data.exp+waitingExp) > data.level)
        {
            for (int i = data.level+1; i < shopCont.GetLevelWithExp(data.exp+waitingExp)+1; i++)
            {
                data.ownedAbilities[i] = 1;
                shopCont.UnlockAbility(i);
            }
            data.level = shopCont.GetLevelWithExp(data.exp+waitingExp);         
        }
        shopCont.GiveStars(true, starReward, -1);
        StartCoroutine(shopCont.StartRewardStars());
    }
    public void EnemyKilled(int ID, bool byPlayer, bool isEscaping, Vector3 position)
    {
        if (!byPlayer)
        {
            killSumByEnemies++;
            currentEnemiesKilledByEnemies[ID]++;
        }
        if (isEscaping) { escapingEnemiesKilled++; }
        killSum++;
        if (killSum % 150 == 0 && healthAbilityOn) {FillLife(true); }
        currentEnemiesKilled[ID]++;
        playerCont.EnemyKilled(position);
        UICont.EnumerateKills();
    }    

    public void FillLife(bool isAbility)
    {
        playerCont.FillLife(isAbility);
        lifeFilled = true;
    }

    public void Mute(bool isMusic)
    {
        audioCont.Mute(isMusic);
        if (isMusic) { data.isMusicMuted = !data.isMusicMuted; }
        else { data.isSoundMuted = !data.isSoundMuted; }
        SaveAndLoad.SaveData(data);
    }

    public void SaveChallenges(Challenge[] challenges, int completedChallenge, List<int> easyChallenges,
        List<int> meduiumChallenges, List<int> hardChallenges, bool withAd)
    {
        if(completedChallenge != -1) 
        {
            if (!withAd) { data.completedChallengesSum++; }
            data.challenges[completedChallenge] = challenges[completedChallenge];
            SaveAndLoad.SaveData(data);
        }
        else
        {
            data.remainingEasyChallenges = easyChallenges;
            data.remainingMediumChallenges = meduiumChallenges;
            data.remainingHardChallenges = hardChallenges;
            waitingNewChallenges = challenges;
        }        
    }

    public void SetStats()
    {
        statData.SetData((int)data.sumTime,data.killSum,data.killSpecificEnemies,data.damageFromEnemies,-1);
        statData.SetExtraData(data.highScoreKill, (int)data.highScoreTime, data.gameNumber, data.completedChallengesSum,
            data.biggestCombo, data.comboSum, data.totalStars);
    }

    public float GetCurrentTime()
    {
        if (!isPaused) { return Time.realtimeSinceStartup - startTime; }
        else { return -1; }
    }

    public void TriggerSlowDown(float waitTime)
    {
        StartCoroutine(SlowDownTime(waitTime));
    }

    IEnumerator SlowDownTime(float waitTime)
    {
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(waitTime);
        Time.timeScale = 1;
    }

    public void ComboMade(int comboMult)
    {
        if (comboMult > data.biggestCombo) { data.biggestCombo = comboMult; }
        comboSum++;
    }

    public void SetHealthAbility()
    {
        healthAbilityOn = true;
    }

    public void SetAbility(int newAbility)
    {
        data.activeAbility = newAbility;
        abilityCont.SetActiveAbility(data.activeAbility);
        SaveAndLoad.SaveData(data);
    }

    public void BuyAbility(int abilityIndex)
    {
        data.ownedAbilities[abilityIndex] = 2;
        SaveAndLoad.SaveData(data);
    }

    public void SaveStars(int stars)
    {
        if (stars > data.stars) { data.totalStars += stars - data.stars; }
        data.stars = stars;
        data.exp += waitingExp;
        if (data.exp > maxExp) { data.exp = maxExp; }
        if (waitingNewChallenges != null) { data.challenges = waitingNewChallenges; }
        SaveAndLoad.SaveData(data);
        waitingNewChallenges = null;
        waitingExp = 0;
        challengeCont.ChallengesSaved();
    }

    public IEnumerator ChallengeKillSum(int challengeIndex, int killGoal, bool isOneGame)
    {
        int helperInt = killSum;
        if (isOneGame) { while (!gameOver && killSum - helperInt < killGoal) { yield return null; } }
        else { while (!gameOver && !isPaused && killSum - helperInt < killGoal) { yield return null; } }
        if (killSum - helperInt >= killGoal) { challengeCont.ChallengeCompleted(challengeIndex); }
        else if (!isOneGame)
        {
            if (gameOver) { challengeCont.SetChallengeData(challengeIndex, killSum - helperInt); }
            else if (isPaused) { challengeCont.SetChallengeData(challengeIndex, killSum - helperInt); }
        }
    }

    public IEnumerator ChallengeDontKill(int challengeIndex, int time)
    {
        while (killSum-killSumByEnemies <= 0 && GetCurrentTime() < time && !gameOver) { yield return null; }
        if (killSum - killSumByEnemies <= 0 && ((time>0 && GetCurrentTime() >= time) || !hasQuit)) { challengeCont.ChallengeCompleted(challengeIndex); }
    }

    public IEnumerator ChallengeSurvive(int challengeIndex, int time)
    {
        while(GetCurrentTime()<time && !gameOver) { yield return null; }
        if (!gameOver) { challengeCont.ChallengeCompleted(challengeIndex); }
    }

    public IEnumerator ChallengeDontKnockSpecificEnemy(int challengeIndex, int enemyIndex)
    {
        while(!gameOver && currentEnemiesKilled[enemyIndex] - currentEnemiesKilledByEnemies[enemyIndex] <= 0) { yield return null; }
        if(gameOver && !hasQuit) { challengeCont.ChallengeCompleted(challengeIndex); }
    }

    public IEnumerator ChallengeKillEscaping(int challengeIndex, int goal, bool oneGame)
    {
        int helperInt = escapingEnemiesKilled;
        if (oneGame) { while (!gameOver && escapingEnemiesKilled - helperInt < goal) { yield return null; } }
        else { while (!gameOver && !isPaused && escapingEnemiesKilled - helperInt < goal) { yield return null; } }
        if (escapingEnemiesKilled - helperInt >= goal) { challengeCont.ChallengeCompleted(challengeIndex); }
        else if (!oneGame)
        {
            if (gameOver) { challengeCont.SetChallengeData(challengeIndex, escapingEnemiesKilled - helperInt); }
            else { challengeCont.SetChallengeData(challengeIndex, escapingEnemiesKilled - helperInt); }
        }
    }

    public IEnumerator ChallengeKillEveryType(int challengeIndex)
    {
        bool isDone = false;
        while(!gameOver && !isDone)
        {
            isDone = true;
            foreach(int i in currentEnemiesKilled) { if (i == 0) { isDone = false; } }
            yield return null;
        }
        if (isDone) { challengeCont.ChallengeCompleted(challengeIndex); }
    }

    public IEnumerator ChallengeKillSpecificEnemy(int challengeIndex, int enemyIndex, int amount, bool oneGame)
    {
        int helperInt = currentEnemiesKilled[enemyIndex] - currentEnemiesKilledByEnemies[enemyIndex];
        if (oneGame) { while (!gameOver && currentEnemiesKilled[enemyIndex] - currentEnemiesKilledByEnemies[enemyIndex] - helperInt < amount) 
            { yield return null; } }
        else { while (!gameOver && !isPaused && currentEnemiesKilled[enemyIndex] - currentEnemiesKilledByEnemies[enemyIndex] - helperInt < amount)
            { yield return null; } }
        if (currentEnemiesKilled[enemyIndex] - currentEnemiesKilledByEnemies[enemyIndex] - helperInt >= amount)
            { challengeCont.ChallengeCompleted(challengeIndex); }
        else if (!oneGame)
        {   challengeCont.SetChallengeData(challengeIndex, currentEnemiesKilled[enemyIndex] - currentEnemiesKilledByEnemies[enemyIndex] - helperInt); }
    }

    public IEnumerator ChallengeFillLife(int challengeIndex, int amount, bool inOneGame)
    {
        int challengeValue=0;
        if (inOneGame)
        {
            while (!gameOver && challengeValue < amount) 
            {
                while (!gameOver && !lifeFilled) { yield return null; }
                if (!gameOver)
                {
                    challengeValue++;
                }
                lifeFilled = false;
            }
        }
        else
        {
            while (!gameOver && !isPaused && challengeValue < amount)
            {
                while (!gameOver && !lifeFilled && !isPaused) { yield return null; }
                if (!gameOver && !isPaused)
                {
                    challengeValue++;
                }
                lifeFilled = false;
            }
        }
        if (challengeValue >= amount) { challengeCont.ChallengeCompleted(challengeIndex); }
        else if(!inOneGame)
        {
            challengeCont.SetChallengeData(challengeIndex, challengeValue); 
        }
    }  
    
    public IEnumerator ChallengeEarnStars(int challengeIndex, int amount, bool inOneGame)
    {
        if (inOneGame)
        {
            while (!gameOver) { yield return null; }
        }
        else
        {
            while (!gameOver && !isPaused) { yield return null; }
        }
        if (starReward >= amount) { challengeCont.ChallengeCompleted(challengeIndex); }
        else if (!inOneGame) { challengeCont.SetChallengeData(challengeIndex, starReward);}
    }    
}
