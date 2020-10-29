using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using BSH_Prototype;

public class GameController : MonoBehaviour {

	public delegate void Delegate();
	public static event Delegate OnStartGame;
	public static event Delegate OnGameOver;
	public static event Delegate OnGameRestart;
	public delegate void DelegatePause(bool value);
	public static event DelegatePause OnPaused;

	public static GameController Instance;

	[Header("[ Налаштування Player'a ]")]
	public Transform playerObject;
	public int currentPlayer = 0;

	[Header ("Кількість створених columns при старті")]
	public int precreateColumns = 5;

	[Header ("Позиція наступного background")]
	public Vector2 nextBackgroundPosition = new Vector2(0, 0);

	[Header ("Позиція наступного column")]
	public Vector2 nextColumnPosition = new Vector2(-2, -2);

	[Header ("Список елементів, які можуть відображатися в стовпцях")]
	public Transform[] items;

	[Header ("Після того як кількість стовпчиків = itemRate відображатиме елемент?")]
	public int itemRate = 8;
	internal int itemRateCount = 0;

	[Header("Шанс отримати платформу, що рухається")]
	public float movingColumnChance = 0;

	[Header("Шанс отримати платформу, що руйнується")]
	public float destroyColumnChance = 0;

	[Header ("Рамки по осях для респавна column")]
	public Vector2 columnGapRange = new Vector2(3,7);
	public Vector2 columnHeightRange = new Vector2(-2.5f,-2.5f);

	[Header("Точка для видалення колон")]
	public Transform PointDestroy;

	[Header ("Очки")]
	public int score = 0;
	public Transform scoreText;
	internal int highScore = 0;
	internal int scoreMultiplier = 1;

	[Header ("Кі-сть points потрібних підняття левела")]
	public int levelUpEveryScore = 1000;

	[Header ("Прогрес отримання нового левела")]
	internal int levelProgress = 0;

	[Header("На ск-ки змінюватиметься висота стовпців, при отриманні новго левела")]
	public Vector2 columnHeightIncrease = new Vector2(-0.2f, 0.2f);

	[Header("Границі руху для стовпців, що рухаються")]
	public Vector2 columnHeightMax = new Vector2(-3.5f, 0);

	[Header("Підвищує шанс стовпця, який рухається, при піднятті левела")]
	public float increaseMovingColumnChance = 0.05f;

	[Header("Підвищує шанс стовпця, який руйнується, при піднятті левела")]
	public float increaseDestroyColumnChance = 0.05f;

	[Header("[ Налаштування Камери ]")]
	public Transform cameraObject;

	[Header("Швидкість каемери")]
	public float cameraSpeed = 10;

	[Header("Найменування кнопки рестарта гри після програшу")]
	public string confirmButton = "Submit";

	[Header("Найменування кнопки Паузи")]
	public string pauseButton = "Cancel";

	[Header("Чи гра на паузі")]
	public bool  isPaused = false;

	[Header("Скільки раз користувач може використати контінує")]
	public int continues = 1;

	[Header("Останній трансформ стовпця, на якому знаходився персонаж до програшу")]
	internal Transform lastLandedObject;

	[Header ("Канваси для UI")]
	public Transform gameCanvas;
	public Transform pauseCanvas;
	public Transform gameOverCanvas;
	public Text textGameOver;
	public Text textHighScore;
	public Text textScore;
	
	public Button bttnContinue;

	[Header("Нижня межа по (Y), щоб сказати чи помер персонаж")]
	public float deathLineHeight = -2;

	[Header ("Чи автоматично стрибатиме гравець дійшовши до максимальної сили")]
	public bool playerAutoJump = true;

	[Header("Список бонісів, які діятимуть на персонажа")]
	public Powerup[] powerups;

	[Header("Найменування кнопки, що буде виступати стрибком")]
	public string jumpButton = "Jump";

	[Header("Чи гра завершена")]
	public bool  isGameOver = false;

	[Header("Список очків, які гравець отримає, за попадання на платформу відповідно до відстані до центра")]
	public LandingBonus[] landingBonuses;

	[Header("Стрік поточний")]
	internal int currentStreak = 0;

	[Header ("Текстове поле для відображення очків")]
	public Transform bonusText;

	// A list of records that keep track of various game stats
	internal int currentDistance = 0;
	internal int longestDistance = 0;
	internal int longestStreak = 0;
	internal int totalPowerups = 0;
	internal int currentPowerUpStreak = 0;
	internal int longestPowerUpStreak = 0;


	void OnDisable(){
		InitPool.OnInitPoolEnd -= OnStart;
		InitPool.OnSpawnBG -= CreateBackground;
	}

	void OnEnable(){
		InitPool.OnInitPoolEnd += OnStart;
		InitPool.OnSpawnBG += CreateBackground;
	}

	void Awake(){
		Instance = this;
	}

	void Start(){
		UpdateScore();

		playerObject.gameObject.GetComponent<Rigidbody2D> ().simulated = false;

		//Ховаємо канвас програшу
		if ( gameOverCanvas )    gameOverCanvas.gameObject.SetActive(false);

		//Отримати найкращий рахунок для гравця
		highScore = PlayerPrefs.GetInt(SceneManager.GetActiveScene().name + "HighScore", 0);

		//Отримуєм вибрану шапку
		currentPlayer = PlayerPrefs.GetInt("CurrentPlayer", currentPlayer);

		// Get the current number of powerups from PlayerPrefs
		totalPowerups = PlayerPrefs.GetInt("TotalPowerups", totalPowerups);

		//Тут встановлення шапок
		SetPlayer(currentPlayer);

		if ( cameraObject == null )    cameraObject = GameObject.FindGameObjectWithTag("MainCamera").transform;

		//Створення колон при старті гри відбувається в пулі
		//		OnStartGame ();

		//Go through all the powerups and reset their timers
		for (var index = 0 ; index < powerups.Length ; index++ )
		{
			//Set the maximum duration of the powerup
			powerups[index].durationMax = powerups[index].duration;

			//Reset the duration counter
			powerups[index].duration = 0;

			//Deactivate the icon of the powerup
			powerups[index].icon.gameObject.SetActive(false);
		}

		//Assign the sound source for easier access
		//if ( GameObject.FindGameObjectWithTag(soundSourceTag) )    soundSource = GameObject.FindGameObjectWithTag(soundSourceTag);

		//Пауза при старті гри
		Pause();
	}

	public void OnTouchDown(Gesture gesture) {
		StartJump();
	}

	public void OnTouchUp(Gesture gesture) {
		EndJump();
	}

	void  Update()
	{
		if (isGameOver == true) {
			if ( Input.GetButtonDown(confirmButton) )
			{
				Restart();
			}

			if ( Input.GetButtonDown(pauseButton) )
			{
				MainMenu();
			}
		} else {
			if ( Input.GetButtonDown(pauseButton) )
			{
				Pause();
			}

			//If there is a player object, you can make it jump, the background moves in a loop.
			if ( playerObject )
			{
				if ( cameraObject )
				{
					//Make the camera chase the player in all directions
					cameraObject.GetComponent<Rigidbody2D>().velocity = new Vector2((playerObject.position.x - cameraObject.position.x + 3) * cameraSpeed, cameraObject.GetComponent<Rigidbody2D>().velocity.y);
				}

				//If we press the jump buttons, start the jump sequence, charging up the jump power
//				if ( Input.GetButtonDown(jumpButton) )    StartJump();
//				if ( Input.GetMouseButtonDown(0) )    StartJump();

				//If we release the jump buttons, end the jump sequence, and make the player jump
//				if ( Input.GetMouseButtonUp(0)  )    EndJump();

				//If the player object moves below the death line, kill it.
				if ( playerObject.position.y < deathLineHeight )     playerObject.SendMessage("Die");
			}
		}

		if ( nextColumnPosition.x - cameraObject.position.x < precreateColumns * 3 )
		{ 
			CreateColumn(1, true);
		}
	}

	void  SetPlayer( int playerNumber )
	{
		Debug.Log ("Потім тут має бути встановлення шапок");
	}

	#region Jump
	//This function sends a start jump command to the current player
	public void StartJump()
	{
		if (playerObject)    playerObject.SendMessage("StartJump", playerAutoJump);
	}

	//This function sends an end jump command to the current player
	public void EndJump()
	{
		if (playerObject)    playerObject.SendMessage("EndJump");
	}
	#endregion //Jump

	void OnStart(){
		CreateColumn (precreateColumns, true);
//		CreateBackground (3);
	}

	void CreateBackground( int columnCount = 0 ){
		while (columnCount > 0) 
		{
			columnCount--;

			Transform newColumn;

			newColumn = InitPool.Instance.Spawn (GetType (4, 13), nextBackgroundPosition);

			nextBackgroundPosition.x += 20.44f;
		}
	}

	/// <summary>
	/// Функція створення columns
	/// </summary>
	void CreateColumn ( int columnCount = 0, bool giveBonus = false)
	{
		//Create a few columns at the start of the game
		while ( columnCount > 0 )
		{
			columnCount--;

			int randomColumn = 0;

			Transform newColumn;

			if ( Random.value < movingColumnChance )
			{
				newColumn = InitPool.Instance.Spawn (GetType(3, 3), nextColumnPosition);
			}
			else
			{
				newColumn = InitPool.Instance.Spawn (GetType(0, 2), nextColumnPosition);
			}

            if (Random.value < destroyColumnChance)
            {
                newColumn.SendMessage("IsDestroy");
            }

            ColumnController column = newColumn.GetComponent<ColumnController>();

            if (column.isDestroy)
            {
                if (column.skltn)
                    column.skltn.state.SetAnimation(0, "Idle", true);
            }

			// Record the first column we land on
			if ( giveBonus == false )    lastLandedObject = newColumn;

			//Go to the next column position, based on the gap of the current column
			nextColumnPosition.x += Random.Range(columnGapRange.x, columnGapRange.y);
			nextColumnPosition.y = Random.Range(columnHeightRange.x, columnHeightRange.y);

			// If the column is moving, give it a vertical range
			newColumn.SendMessage("SetMoveRange", new Vector2( columnHeightRange.x, columnHeightRange.y));

			//Should this column give bonus when landed upon?
			if ( giveBonus == false )    newColumn.SendMessage("NoBonus");

			//Count the rate for an item to appear on a column
			itemRateCount++;

			//Create a new item on the column
			if ( itemRateCount >= itemRate  )
			{
				//Create a new random item from the list of items
				Instantiate( items[Mathf.FloorToInt(Random.Range(0, items.Length))], newColumn.position + new Vector3(0, 0.75f, 0), Quaternion.identity);

				//Reset the item rate counter
				itemRateCount = 0;
			}
		}
	}

	/// <summary>
	/// Отримуєм тип, для створення колони
	/// </summary>
	PoolType GetType(int _min, int _max){
		int k = Random.Range (_min, _max + 1);
		return (PoolType)k;
	}

	/// <summary>
	/// Оновлення рахунку
	/// </summary>
	void UpdateScore()
	{
		if ( scoreText )    scoreText.GetComponent<Text>().text = score.ToString();

		if ( levelProgress >= levelUpEveryScore )
		{
			levelProgress -= levelUpEveryScore;

			LevelUp();
		}
	}

	/// <summary>
	/// Піднімаєм левел
	/// </summary>
	void LevelUp()
	{
		Debug.Log ("@@@ LevelUp");
		// Increase the height range of the columns
		columnHeightRange += columnHeightIncrease;

		// Limit the height range of columns
		if ( columnHeightRange.x < columnHeightMax.x )    columnHeightRange.x = columnHeightMax.x;
		if ( columnHeightRange.y > columnHeightMax.y )    columnHeightRange.y = columnHeightMax.y;

		// Increase the chance of a moving column appearing
		movingColumnChance += increaseMovingColumnChance;

		destroyColumnChance += increaseDestroyColumnChance;

		// If there is a source and a sound, play it from the source
		//if ( soundSource && soundLevelUp )    soundSource.GetComponent<AudioSource>().PlayOneShot(soundLevelUp);

	}

	/// <summary>
	/// Змінюєм множник очків x2
	/// </summary>
	/// <param name="setValue">Set value.</param>
	void SetScoreMultiplier( int setValue )
	{
		scoreMultiplier = setValue;
	}

	/// <summary>
	/// Рестарт
	/// </summary>
	public void  Restart ()
	{
		SceneLoader.Instance.SwitchToScene (Scenes.Game);
	}

	/// <summary>
	/// Перехід в меню
	/// </summary>
	public void  MainMenu()
	{
		SceneLoader.Instance.SwitchToScene (Scenes.MainMenu);
	}

	public void ContinueRewarded()
    {
        isShowingReward = true;

        AdsProvider.Instance.ShowRewardedAd (VideoComplete);
	}

	private void VideoComplete(bool completed, string advertiser)
	{
        isShowingReward = false;

        if (Advertisements.Instance.debug) {
			Debug.Log ("Closed rewarded from: " + advertiser + " -> Completed " + completed);
			GleyMobileAds.ScreenWriter.Write ("Closed rewarded from: " + advertiser + " -> Completed " + completed);
		}

		if (completed == true)
		{
            MetricaController.Instance.ContinueAdsViewComplete();
            //user watched the entire video,, he deserves a coin
            continues ++;
			Continue ();
		}
		else
		{
			//no reward for you
		}
	}

	public void Continue()
	{
		if ( continues > 0 )
		{
			// Reset the player to its last position
			playerObject.position = lastLandedObject.position + new Vector3(0, 10, 0);
			Debug.Log ("::::" +playerObject.position);

			// Reset the player's dead status
			playerObject.SendMessage("NotDead");

			// Continue the game
			isGameOver = false;

			// Show the game screen and hide the game over screen
			if ( gameCanvas )    gameCanvas.gameObject.SetActive(true);
			if ( gameOverCanvas )    gameOverCanvas.gameObject.SetActive(false);

			ChangeContinues(-1);
		}
	}

	// This function changes the number of continues we have
	public void ChangeContinues( int changeValue )
	{
		continues += changeValue;

		// Limit the minimum number of continues to 0
		if ( continues > 0 ) 
		{
			// Deactivate the continues object if we have no more continues
			if ( gameOverCanvas )
				if (bttnContinue != null)
					bttnContinue.gameObject.SetActive(true);
		}
		else
		{
			// Activate the continues object if we have no more continues
			if ( gameOverCanvas )
				if (bttnContinue != null)
					bttnContinue.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// Пауза перед стартом
	/// </summary>
	public void  Pause()
	{
		OnPaused (isPaused);
		isPaused = !isPaused;

		if (pauseCanvas)    
			pauseCanvas.gameObject.SetActive(isPaused);
		if (gameCanvas)    
			gameCanvas.gameObject.SetActive(!isPaused);
	}

	/// <summary>
	/// Встановлення очків
	/// </summary>
	/// <param name="landedObject">Landed object.</param>
	private void  ChangeScore( Transform landedObject )
	{
		// Record the last landed object, so we can reset the player position when continuing after game over
		lastLandedObject = landedObject;

		//Calculate the distance of the player from the center of the column when it landed on it
		float landingDistance = Mathf.Abs(landedObject.position.x - playerObject.position.x);

		//Has bonus been given yet? If so, don't give any more bonus
		bool  bonusGiven = false;

		//Go through all landing bonuses, and check which one should be given to the player
		for (var index = 0 ; index < landingBonuses.Length ; index++ )
		{
			//If no bonus has been given, check if the player is within the correct distance to get a bonus
			if ( bonusGiven == false && landingDistance <= landingBonuses[index].landDistance )    
			{
				//Increase the streak if we are closest to the center, or reset it if we're not
				if ( index == 0 )    
				{
					currentStreak++;

					//Add the bonus to the score
					score += landingBonuses[index].bonusValue * currentStreak * scoreMultiplier;

					// Increase level progress
					levelProgress += landingBonuses[index].bonusValue * currentStreak * scoreMultiplier;

					//Call the perfect landing function, which plays a sound and particle effect based on the player's streak
					playerObject.gameObject.SendMessage("PerfectLanding", currentStreak);
				}
				else    
				{
					// Get the longest streak value from PlayerPrefs
					longestStreak = PlayerPrefs.GetInt( "LongestStreak", longestStreak);

					// If the streak we passed is longer than the longest streak we passed, record it in the PlayerPrefs
					if ( currentStreak > longestStreak )    PlayerPrefs.SetInt( "LongestStreak", currentStreak);

					currentStreak = 0;

					//Add the bonus to the score
					score += landingBonuses[index].bonusValue * scoreMultiplier;

					// Increase level progress
					levelProgress += landingBonuses[index].bonusValue * scoreMultiplier;
				}

				//Update the bonus text
				if ( bonusText )    
				{
					//Set the position of the bonus text to the player
					bonusText.position = playerObject.position;

					//Play the bonus animation
					if ( bonusText.GetComponent<Animation>() )    bonusText.GetComponent<Animation>().Play();

					//Update the text of the bonus object. If we have a streak, display 2X 3X etc
					if ( currentStreak > 1 )    bonusText.Find("Text").GetComponent<Text>().text = "+" + (landingBonuses[index].bonusValue * currentStreak * scoreMultiplier).ToString() + " " + currentStreak.ToString() + "X";  
					else    bonusText.Find("Text").GetComponent<Text>().text = "+" + (landingBonuses[index].bonusValue * scoreMultiplier).ToString();
				}

				//The score has been given, no need to give any more bonus
				bonusGiven = true;
			}
		}

		//Update the score
		UpdateScore();

		// Add to the longest distance statistic
		currentDistance++;
	}

	/// <summary>
	/// Завершення гри
	/// </summary>
	/// <returns>The over.</returns>
	/// <param name="delay">Delay.</param>
	IEnumerator GameOver(float delay)
	{
		//Go through all the powerups and nullify their timers, making them end
		for (var index = 0 ; index < powerups.Length ; index++ )
		{
			//Set the duration of the powerup to 0
			powerups[index].duration = 0;
		}

		//Зберігаєм статистику
		SaveStats();

		yield return new WaitForSeconds(delay);

		//If there is a source and a sound, play it from the source
		//if ( soundSource && soundGameOver )    soundSource.GetComponent<AudioSource>().PlayOneShot(soundGameOver);

		isGameOver = true;

		//Ховаєм канвас паузи і канвас гри
		if (pauseCanvas)    pauseCanvas.gameObject.SetActive(false);
		if (gameCanvas)    gameCanvas.gameObject.SetActive(false);

		//Показуєм скрін завершення гри
		if ( gameOverCanvas )    
		{
			//Show the game over screen
			gameOverCanvas.gameObject.SetActive(true);

			//Записуєм результат
			textScore.text = "SCORE: " + score.ToString();

			//Перевіряєм чи результат найкращий
			if ( score > highScore )    
			{
				highScore = score;

				//Реєструєм новий кращий результат
				PlayerPrefs.SetInt(SceneManager.GetActiveScene().name + "HighScore", score);
			}

			//Записуєм найкращий результат в текстове поле
			textHighScore.text = "HIGH SCORE: " + highScore.ToString();
		}

	}

	/// <summary>
	/// Змінюєм розміри персонажа
	/// </summary>
	/// <param name="targetScale">Target scale.</param>
	private void RescalePlayer( float targetScale )
	{
		if (playerObject)    playerObject.SendMessage("Rescale", targetScale);
	}
		
	//Завершення гри
	IEnumerator ActivatePowerup( int powerupIndex )
	{
		//If there is already a similar powerup running, refill its duration timer
		if ( powerups[powerupIndex].duration > 0 )
		{
			//Refil the duration of the powerup to maximum
			powerups[powerupIndex].duration = powerups[powerupIndex].durationMax;

			// Add to the powerup count
			totalPowerups++;

			// Add to the current power up streak count
			currentPowerUpStreak++;
		}
		else //Otherwise, activate the power up functions
		{
			//Activate the powerup icon
			if ( powerups[powerupIndex].icon )    powerups[powerupIndex].icon.gameObject.SetActive(true);

			//Run up to two start functions from the gamecontroller
			if ( powerups[powerupIndex].startFunction != string.Empty )    SendMessage(powerups[powerupIndex].startFunction, powerups[powerupIndex].startParamater);

			//Fill the duration timer to maximum
			powerups[powerupIndex].duration = powerups[powerupIndex].durationMax;

			//Count down the duration of the powerup
			while ( powerups[powerupIndex].duration > 0 )
			{
				yield return new WaitForSeconds(Time.deltaTime);

				powerups[powerupIndex].duration -= Time.deltaTime;

				//Animate the powerup timer graphic using fill amount
				if ( powerups[powerupIndex].icon )    powerups[powerupIndex].icon.Find("FillAmount").GetComponent<Image>().fillAmount = powerups[powerupIndex].duration/powerups[powerupIndex].durationMax;
			}

			//Run up to two end functions from the gamecontroller
			if ( powerups[powerupIndex].endFunction != string.Empty )    SendMessage(powerups[powerupIndex].endFunction, powerups[powerupIndex].endParamater);

			//Deactivate the powerup icon
			if ( powerups[powerupIndex].icon )    powerups[powerupIndex].icon.gameObject.SetActive(false);

			// Add to the powerup count
			totalPowerups++;

			// If the current powerup streak if bigger than the longest powerup streak, record it in the PlayerPrefs
			if ( currentPowerUpStreak > longestPowerUpStreak )    PlayerPrefs.SetInt( "LongestPowerup", currentPowerUpStreak);

			// Reset the current power up streak
			currentPowerUpStreak = 0;
		}
	}

	/// <summary>
	/// Зберыгаэм статистику в префаб
	/// </summary>
	private void SaveStats()
	{
		// Get the longest distance value from PlayerPrefs
		longestDistance = PlayerPrefs.GetInt( "LongestDistance", longestDistance);

		// If the distance we passed is longer than the longest distance we passed, record it in the PlayerPrefs
		if ( currentDistance > longestDistance )    PlayerPrefs.SetInt( "LongestDistance", currentDistance);

		// Set the current number of powerups in PlayerPrefs
		PlayerPrefs.SetInt("TotalPowerups", totalPowerups);
	}

	private bool isShowingReward = false;

	private void OnApplicationFocus(bool focus)
    {
	    if (!focus)
	    {
		    if(isShowingReward)
				MetricaController.Instance.ContinueAdsExit();
		    
		    if(AdsProvider.Instance.isShowingInterstitial)
			    MetricaController.Instance.InterstitialAdsFail();
	    }
    }
}
