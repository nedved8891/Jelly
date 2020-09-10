using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody2D))]
public class TapController : MonoBehaviour {

	public delegate void PlayerDelegate();
	public static event PlayerDelegate OnPlayerDied;
	public static event PlayerDelegate OnPlayerScored;
	public delegate void PlayerDelegate2(int index);
	public static event PlayerDelegate2 OnPlayerCoin;
	public static event PlayerDelegate2 OnPlayerEat;

	public Image filled;

	public float energy;

	public float tapForceUp = 3;
	public float tapForceLeft = 4;
	public float tiltSmooth = 5;
	public Vector3 startPos;
	public AudioSource tapSound;
	public AudioSource scoreSound;
	public AudioSource dieSound;

	public SkeletonAnimation fish;

	Rigidbody2D rigidBody;
	Quaternion downRotation;
	Quaternion forwardRotation;

	GameManager game;
	TrailRenderer trail;

	public enum SkinsName
	{
		Fish1,
		Fish2,
		Fish3,
		Fish4,
		Fish5
	}

	string currentAnimation;


	private int hitTimeOut;
	private float hitTimeOutPositionX;
	private float hitTimeOutPositionY;
	public int ballSpeed = 250;  // Скорость шарика

	void Start() {
		rigidBody = GetComponent<Rigidbody2D>();

		hitTimeOut = 0;
		hitTimeOutPositionX = 0;
		hitTimeOutPositionY = 0;

		game = GameManager.Instance;
		rigidBody.simulated = false;

		initSkins ();

		fish.state.SetAnimation (0, "Idle", true);
	}

	public void initSkins(){
		fish.skeleton.SetSkin (((SkinsName)Random.Range (0, 4)).ToString());
	}

	void SetAnimation(string name = "Idle", bool repeat = false){
		//print ("SetAnimation: " + name + " " + repeat);
		currentAnimation = name;
		fish.state.SetAnimation (0, name, repeat);
	}

	void OnEnable() {
		GameManager.OnGameStarted += OnGameStarted;
		GameManager.OnGameOverConfirmed += OnGameOverConfirmed;

		EasyTouch.On_TouchStart += OnTouchDown;

		fish.state.Complete += OnCompleteAnimation;
	}

	void OnDisable() {
		GameManager.OnGameStarted -= OnGameStarted;
		GameManager.OnGameOverConfirmed -= OnGameOverConfirmed;

		EasyTouch.On_TouchStart -= OnTouchDown;

		fish.state.Complete -= OnCompleteAnimation;
	}

	void OnGameStarted() {
		rigidBody.velocity = Vector3.zero;
		rigidBody.simulated = true;

		SetEnergy (100);

		filled.color = Color.white;

		fish.state.SetAnimation (0, "Idle", true);
	}

	void OnGameOverConfirmed() {
		transform.localPosition = startPos;
		transform.rotation = Quaternion.identity;
		tapForceLeft = Mathf.Abs (tapForceLeft);

		hitTimeOut = 0;
		hitTimeOutPositionX = 0;
		hitTimeOutPositionY = 0;

		transform.localScale = transform.localScale = new Vector3 (Mathf.Abs(transform.localScale.x)*-1, transform.localScale.y, transform.localScale.z);

		initSkins ();

		SetAnimation ("Idle", true);
	}

	void OnTouchDown(Gesture gesture) {
		if (game.GameOver) return;

		if (rigidBody.simulated) {
			rigidBody.velocity = Vector2.zero;

			GetComponent<Rigidbody2D> ().AddForce (new Vector2(tapForceLeft, tapForceUp * SetEnergy (-3) + 50)); 

			tapSound.Play ();

			SetAnimation ("Go", false);
		}
	}

	public void OnCompleteAnimation(TrackEntry trackEntry){
		print ("OnCompleteAnimation: " + trackEntry.animation.name);
		if (trackEntry.animation.name == "Go") {
			if (currentAnimation != "Death")
			SetAnimation ("Idle", true);
		}else if (trackEntry.animation.name == "Death") {
			SetAnimation ("DeathIdle", true);
		}
	}

	void OnTriggerEnter2D(Collider2D coll) {
		if (coll.gameObject.tag == "DeadZone" || coll.gameObject.tag == "DeadZoneBottom") {
			rigidBody.simulated = false;
			OnPlayerDied();

			filled.color = Color.white;

			dieSound.Play();

			SetAnimation ("Death", false);
		} 
		else if (coll.gameObject.tag == "CoinZone") {
			OnPlayerCoin(coll.gameObject.transform.parent.GetComponent<CoinScript>().index);
			scoreSound.Play();
		} 
		else if (coll.transform.CompareTag ("EatZone")) {
			OnPlayerEat (coll.gameObject.transform.GetComponent<EatScript>().index);
			SetEnergy (15);
			scoreSound.Play();
		}
	}

	void ChangeDirectionOfMovement ()
	{
		rigidBody.velocity = Vector2.zero;

		tapForceLeft *= -1;

		transform.localScale = new Vector3 (transform.localScale.x*-1, transform.localScale.y, transform.localScale.z);

		rigidBody.AddForce (Vector2.right * tapForceLeft, ForceMode2D.Force);
	}

	void FixedUpdate () 
	{
		// Ограничение скорости по осям Х и Y
		if(Mathf.Abs(rigidBody.velocity.x) > ballSpeed/100f)
		{
			rigidBody.velocity = new Vector2(Mathf.Sign(rigidBody.velocity.x) * ballSpeed/100f, rigidBody.velocity.y);
		}
		if(Mathf.Abs(rigidBody.velocity.y) > ballSpeed/100f)
		{
			rigidBody.velocity = new Vector2(rigidBody.velocity.x, Mathf.Sign(rigidBody.velocity.y) * ballSpeed/100f);
		}
	}

	void OnCollisionEnter2D(Collision2D coll)
	{
		// Если шарик "залипнет" на оси Х, то будет добавлена сила, чтобы изменить траекторию
		if(hitTimeOutPositionX != transform.position.x)
		{
			hitTimeOutPositionX = transform.position.x;
		}
		else
		{
			hitTimeOut++;
			if(hitTimeOut == 2)
			{
				hitTimeOut = 0;
				if(hitTimeOutPositionX < 0)
				{
					rigidBody.AddForce(new Vector2(ballSpeed, Random.Range(-ballSpeed, ballSpeed)));
				}
				else
				{
					rigidBody.AddForce(new Vector2(-ballSpeed, Random.Range(-ballSpeed, ballSpeed)));
				}
			}
		}
		// Тоже самое для Y
		if(hitTimeOutPositionY != transform.position.y)
		{
			hitTimeOutPositionY = transform.position.y;
		}
		else
		{
			hitTimeOut++;
			if(hitTimeOut == 2)
			{
				hitTimeOut = 0;
				if(hitTimeOutPositionY < 0)
				{
					rigidBody.AddForce(new Vector2(Random.Range(-ballSpeed, ballSpeed), ballSpeed));
				}
				else
				{
					rigidBody.AddForce(new Vector2(Random.Range(-ballSpeed, ballSpeed), -ballSpeed));
				}
			}
		}

		if (coll.transform.CompareTag ("ScoreZone")) {
			OnPlayerScored ();
			scoreSound.Play ();
			ChangeDirectionOfMovement ();
		}
	}

	float SetEnergy(float value){
		if (energy + value > 100) {
			energy = 100;
		} else if (energy + value > 0) {
			energy += value;
		} else {
			energy = 0;
		}

		filled.fillAmount = energy/100;

		if (filled.fillAmount < 0.5f) {
			filled.DOColor (Color.red, 1.0f / filled.fillAmount);
		} else {
			filled.DOColor (Color.white, filled.fillAmount/1.0f);
		}

		return ((energy / 100) * 1.3f > 1) ? 1 : (energy / 100) * 1.3f;
	}
}
