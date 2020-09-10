using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using Spine;

public class PlayerController : MonoBehaviour {

	[Header("Скелетон")]
	public SkeletonAnimation skltn;

	string currentAnimation;

	[Header ("Трансформ персонажа")]
	internal Transform thisTransform;

	[Header ("Ігровий контроллер")]
	internal GameObject gameController;

	[Header("Швидкість зміни speedBar")]
	public float jumpChargeSpeed = 30;

	[Header ("поточна сила стрибка")]
	internal float jumpPower = 2;

	[Header ("Максимальна сила стрибка вверх")]
	public float jumpChargeMax = 20;

	[Header ("Чи включити авто стрибок")]
	internal bool autoJump = true;

	[Header("Повер бар")]
	public Transform powerBar;

	//The *horizontal* movement speed of the player when it is jumping
	public float moveSpeed = 4;

	[Header ("Ефекти")]
	public ParticleSystem jumpEffect;
	public ParticleSystem landEffect;
	public ParticleSystem perfectEffect;

//	//Various animations for the player
//	public AnimationClip animationJumpStart;
//	public AnimationClip animationJumpEnd;
//	public AnimationClip animationFullPower;
//	public AnimationClip animationFalling;
//	public AnimationClip animationLanded;
//
	//Various sounds and their source
	public AudioClip soundStartJump;
	public AudioClip soundEndJump;
	public AudioClip soundLand;
	public AudioClip soundCrash;
	public AudioClip soundPerfect;
	public string soundSourceTag = "GameController";
	internal GameObject soundSource;

	[Header("Старт стрибка")]
	internal bool  startJump = false;

	[Header("Чи в стрибку гравець?")]
	internal bool  isJumping = false;

	[Header("Чи на платформі гравець?")]
	internal bool  isLanded = false;

	[Header("Чи фоллінг гравець?")]
	internal bool  isFalling = false;

	[Header("Чи помер гравець?")]
	internal bool isDead = false;

	public Rigidbody2D rd;

	void OnDisable(){
		GameController.OnPaused -= _OnPaused;
		HatsScript.SwitchHat -= GetHat;
	}

	void OnEnable(){
		GameController.OnPaused += _OnPaused;
		HatsScript.SwitchHat += GetHat;
	}

	void _OnPaused(bool value){
        if(rd)
		    rd.simulated = value;
	}

	void GetHat(){
		switch (PlayerPrefs.GetInt ("Hat", 0)) {
		case 1:
			skltn.skeleton.SetAttachment ("Hat", "Hat5");
			break;
		case 2: 
			skltn.skeleton.SetAttachment ("Hat", "Hat1");
			break;
		case 3:
			skltn.skeleton.SetAttachment ("Hat", "Hat2");
			break;
		case 4:
			skltn.skeleton.SetAttachment ("Hat", "Hat3");
			break;
		default:
			skltn.skeleton.SetAttachment ("Hat", "Hat4");
			break;
		}
	}

	void  Start()
	{
		PlayAnimation("IdleJump", true);

		GetHat ();

		skltn.state.Complete += OnCompleteAnimation;

		thisTransform = transform;

		rd = GetComponent<Rigidbody2D> ();

		gameController = GameController.Instance.gameObject;

		if ( jumpEffect )    jumpEffect.GetComponent<Renderer>().sortingLayerName = "Particle";
		if ( landEffect )    landEffect.GetComponent<Renderer>().sortingLayerName = "Particle";
		if ( perfectEffect )    perfectEffect.GetComponent<Renderer>().sortingLayerName = "Particle";

		//Assign the sound source for easier access
		if ( GameObject.FindGameObjectWithTag(soundSourceTag) )    soundSource = GameObject.FindGameObjectWithTag(soundSourceTag);
	}

	void  Update()
	{
		if ( isDead == false )
		{
			if ( startJump == true )
			{
				if ( jumpPower < jumpChargeMax )
				{
					jumpPower += Time.deltaTime * jumpChargeSpeed;

					powerBar.Find("Base/FillAmount").GetComponent<Image>().fillAmount = jumpPower/jumpChargeMax;

					if ( soundSource )    soundSource.GetComponent<AudioSource>().pitch = 0.3f + jumpPower * 0.1f;
				}
				else if ( autoJump == true )
				{
					EndJump();
				}
				else
				{
//					PlayAnimation (animationFullPower);
				}
			}

			if ( isFalling == false && GetComponent<Rigidbody2D>().velocity.y < 0 )
			{
				isFalling = true;

				//if ( GetComponent<Animation>() && animationFalling )
				//{
				//	GetComponent<Animation>().PlayQueued(animationFalling.name, QueueMode.CompleteOthers);
				//}
			}
		}
	}

	/// <summary>
	/// Добавляє очки, викликаючи метод в ігровому контроллері
	/// </summary>
	/// <param name="landedObject">Landed object.</param>
	void  ChangeScore(Transform landedObject)
	{
		gameController.SendMessage("ChangeScore", landedObject);
	}

	/// <summary>
	/// Метод видаляє гравця і спрацьовує трігер кінця гри
	/// </summary>
	void  Die()
	{
		if ( isDead == false )
		{
			//Call the game over function from the game controller
			gameController.SendMessage("GameOver", 0.5f);

			//Play the death sound
			if ( soundSource )
			{
				soundSource.GetComponent<AudioSource>().pitch = 1;

				//If there is a sound source and a sound assigned, play it from the source
				if ( soundCrash )    soundSource.GetComponent<AudioSource>().PlayOneShot(soundCrash);
			}

			// The player is dead
			isDead = true;
		}
	}

	/// <summary>
	/// Метод повертає статус в значення не мертвий
	/// </summary>
	public void NotDead()
	{
		isDead = false;
	}

	/// <summary>
	/// Метод, який запускається при початку набирання сили стрибка
	/// </summary>
	void StartJump( bool playerAutoJump ){
		if ( isDead == false )
		{
			autoJump = playerAutoJump;

			if ( isLanded == true )//You can only jump if you are on land
			{	
				startJump = true;

				jumpPower = 0;//Reset the jump power

				//Play the jump start animation ( charging up the jump power )
				PlayAnimation("StartJump");

				//Align the power bar to the player and activate it
				if ( powerBar )
				{
					powerBar.position = thisTransform.position;

					powerBar.gameObject.SetActive(true);
				}

				if ( soundSource )
				{
					//If there is a sound source and a sound assigned, play it from the source
					if ( soundStartJump )    soundSource.GetComponent<AudioSource>().PlayOneShot(soundStartJump);
				}
			}
		}
	}

	/// <summary>
	/// Метод, що запускається при завершенні вибору сили стрибка
	/// </summary>
	void  EndJump(){
		if ( isDead == false )
		{
			//You can only jump if you are on land, and you already charged up the jump power ( jump start )
			if ( isLanded == true && startJump == true )
			{
				thisTransform.parent = null;

				startJump = false;
				isJumping = true;
				isLanded = false;
				isFalling = false;

				//Give the player velocity based on jump power and move speed
				GetComponent<Rigidbody2D>().velocity = new Vector2( moveSpeed, jumpPower);

				//Play the jump ( launch ) animation
				PlayAnimation("IdleJump", true);

				//Deactivate the power bar
				if ( powerBar )    powerBar.gameObject.SetActive(false);

				//Play the jump particle effect
				if ( jumpEffect )   jumpEffect.Play(); 

				//Play the jump sound ( launch )
				if ( soundSource )
				{
					soundSource.GetComponent<AudioSource>().Stop();

					soundSource.GetComponent<AudioSource>().pitch = 0.6f + jumpPower * 0.05f;

					//If there is a sound source and a sound assigned, play it from the source
					if ( soundEndJump )    soundSource.GetComponent<AudioSource>().PlayOneShot(soundEndJump);
				}

			}
		}
	}

	/// <summary>
	/// Метод, який запускається при приземленні на платформу
	/// </summary>
	void  PlayerLanded()
	{
		isLanded = true;

		//Play the landing animation
		PlayAnimation("EndJump");

		//Play the landing particle effect
		if ( landEffect )    landEffect.Play();

		//Play the landing sound
		if ( soundSource )
		{
			soundSource.GetComponent<AudioSource>().pitch = 1;

			//If there is a sound source and a sound assigned, play it from the source
			if ( soundLand )    soundSource.GetComponent<AudioSource>().PlayOneShot(soundLand);
		}
	}

	/// <summary>
	/// Метод, який запускається при ілеальному приземленні на платформу
	/// </summary>
	void  PerfectLanding(int streak)
	{
		//Play the perfect landing particle effect
		if ( perfectEffect )    perfectEffect.Play();

		//If there is a sound source and a sound assigned, play it from the source
		if ( soundSource && soundPerfect )    soundSource.GetComponent<AudioSource>().PlayOneShot(soundPerfect);
	}

	//This function rescales this object over time
	IEnumerator Rescale( float targetScale )
	{
		//Perform the scaling action for 1 second
		float scaleTime = 1;

		while ( scaleTime > 0 )
		{
			//Count down the scaling time
			scaleTime -= Time.deltaTime;

			//Wait for the fixed update so we can animate the scaling
			yield return new WaitForFixedUpdate();

			float tempScale = thisTransform.localScale.x;

			//Scale the object up or down until we reach the target scale
			tempScale -= ( thisTransform.localScale.x - targetScale ) * 5 * Time.deltaTime;

			thisTransform.localScale = Vector3.one * tempScale;
		}

		//Rescale the object to the target scale instantly, so we make sure that we got the the target
		thisTransform.localScale = Vector3.one * targetScale;
	}

	/// <summary>
	/// Програвання анімації
	/// </summary>
	void PlayAnimation(string _animation, bool loop = false){
		if ( skltn && _animation != "" )
		{
			currentAnimation = _animation;
			skltn.state.SetAnimation (0, _animation, loop);
		}
	}

	public void OnCompleteAnimation(TrackEntry trackEntry){
		if (trackEntry.animation.name == "EndJump") {
			if (currentAnimation != "StartJump" && currentAnimation != "IdleJump")
				PlayAnimation ("Idle", true);
		}
	}
}
