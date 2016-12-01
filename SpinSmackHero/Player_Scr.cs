using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Player_Scr : MonoBehaviour {
	private float m_MaxSpeed = 6f;//6                  // The fastest the player can travel in the x axis.
	private float m_JumpForce = 800f;//600                // Amount of force added when the player jumps.
	[Range(0, 1)] [SerializeField] private float m_CrouchSpeed = 0.36f;  // Amount of maxSpeed applied to crouching movement. 1 = 100%
	[SerializeField] private bool m_AirControl = false;                 // Whether or not a player can steer while jumping;
	[SerializeField] private LayerMask m_WhatIsGround;                  // A mask determining what is ground to the character
	[SerializeField]
	private Text playerText = null;

	private Transform m_GroundCheck;    // A position marking where to check if the player is grounded.
	const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
	private bool m_Grounded;            // Whether or not the player is grounded.
	private Transform m_CeilingCheck;   // A position marking where to check for ceilings
	const float k_CeilingRadius = .01f; // Radius of the overlap circle to determine if the player can stand up
	private Animator m_Anim;            // Reference to the player's animator component.
	private Rigidbody2D m_Rigidbody2D;


	private bool m_FacingRight = true;  // For determining which way the player is currently facing.
	private int Start_Going_Down_Once = 0;

	private int jump_Steps = 0; 
	private float jumpresetlINC = 0; 
	private float velocitylINC = 0; 
	private float velocityThe = 0; 

	private int Health = 3; 
	private int hit_Blink = 0; 
	private float hit_INC = 0; 
	private float number_Blinks = 0; 

	private int DieOnce = 0;
	private SpriteRenderer sprite_renderer;
	private GameObject Game_UI_Panel;
	private Text Health_Heart_Text;
	private Text Game_Over_Text;
	private Text Chickens_Killed_Text;
	private Text Blood_Mode_Kill_Text;

	private int Halt_Processes = 1;
	private int Falling_Status_OUT = 1;

	[SerializeField]
	private AudioClip jumpSound;
	[SerializeField]
	private AudioClip damageSound;
	[SerializeField]
	private AudioClip HitSound;
	[SerializeField] 
	private AudioClip GameOverMusic; 
	[SerializeField] 
	private AudioClip EnemyDeathSFX1; 
	[SerializeField] 
	private AudioClip EnemyDeathSFX2; 
	[SerializeField] 
	private AudioClip LevelUpSound; 
	private AudioSource audiosource;
	private AudioSource audiosourceSecond;



	//for scoring
	[SerializeField]
	private Text PlayerScoreText = null;
	//for level
	[SerializeField]
	private Text PlayerLevelText = null;
	private int LevelAvanceScore = 100;
	private int Player_Score = 0;
	private int PlayerLevel = 0;




	private float Volume_Normal = 0.5f;
	private float Volume_BloodModeRock = 0.4f;
	private int Curentlevel = 0;
	//move input
	private bool crouchin = false;
	private float horizontalin = 0;
	private bool m_Jumpin = false;
	private float InVinsible_Mode = 0;

	//maybe use this incase (but maybe not)
	private bool TrytoFixStuck_INJump = false;

	private Slider Weapon_Energy_Meter;
	private Text Weapon_Energy_Text = null;
	private int Weapon_Energy_value = 0; 
	private float Weapon_Energy_DecrementINC = 0; 
	private int Enable_Timed_Energy_Drain = 0; 



	private GameObject ClickedObject;
	private int newClick;
	private int Clicked_Now;
	private int Clicked_HoldDownNow;
	private int Initial_Energy_Detected = 0;
	private Animator Weapon_Anim;// Reference to the weapon's animator component.
	private int Run_Now = 0;


	private int Enemy_Killed_Count = 0;
	private int Player_Enter_StumpPoint = 0;//of enemy
	private float PushMeBack = 0; 
	private int playDamageonce = 0; 
	int max_Energy_Allowed = 100;
	private Transform Current_Enemy_Transform;

	private int JumptrapJumpEnbale = 0;

	// Use this for initialization
	private void Awake()//void Start () 
	{
		// Setting up references.
		m_GroundCheck = transform.Find("GroundCheck");
		m_CeilingCheck = transform.Find("CeilingCheck");
		m_Anim = GetComponent<Animator>();
		m_Rigidbody2D = GetComponent<Rigidbody2D>();
		//get the AudioSource component
		audiosource = GetComponent<AudioSource>();
		audiosourceSecond = transform.Find("SecondAudioObject").GetComponent<AudioSource>();

		//get the weapons anim component
		Weapon_Anim = transform.Find("Idle_Weapon_0").GetComponent<Animator>();



		//get the weapon slider
		Weapon_Energy_Meter = transform.Find("Canvas_Player").transform.Find("Slider_Weapon_Energy").GetComponent<Slider>();
		//get the meter text
		//Weapon_Energy_Text = BloodMeter_Object.transform.Find("Blood Text").GetComponent<Text> ();

	}



	//Used to play single sound clips.
	public void PlaySingle(AudioClip clip, float volume_In)
	{
		//Set the clip of our efxSource audio source to the clip passed in as a parameter.
		audiosource.clip = clip;
		audiosource.volume = volume_In;
		//Play the clip.
		audiosource.Play ();
	}

	//Used to play single sound clips.
	public void PlaySingleSecondAudio(AudioClip clip, float volume_In)
	{
		//Set the clip of our efxSource audio source to the clip passed in as a parameter.
		audiosourceSecond.clip = clip;
		audiosourceSecond.volume = volume_In;
		//Play the clip.
		audiosourceSecond.Play ();
	}


	private void FixedUpdate()
	{


		Halt_Processes = 0;

		//==========Halt_Processes
		if (Halt_Processes == 0) {






			m_Grounded = false;

			// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
			// This can be done using layers instead but Sample Assets will not overwrite your project settings.
			Collider2D[] colliders = Physics2D.OverlapCircleAll (m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
			for (int i = 0; i < colliders.Length; i++) {
				if (colliders [i].gameObject != gameObject)
					m_Grounded = true;
			}


	



			// Pass all parameters to the character control script.
			Move (horizontalin, crouchin, m_Jumpin);
			m_Jumpin = false;


			//do running if you have energy level you are on the ground and not in the process of jumping
			if (m_Grounded == true && jump_Steps == 0 && Initial_Energy_Detected == 1)
			{
				if (Run_Now > 0)
				{

					//move foward
					if (Run_Now == 1) { 
						horizontalin = 1;// (-1= back 1= front)
					}
					//move backward
					if (Run_Now == 2) { 
						horizontalin = -1;// (-1= back 1= front)
					}
					//play the run animation
					m_Anim.SetInteger ("Activity_Select", 3);

				}
				//if player is not running the stop and play idle animation
				if (Run_Now == 0) 
				{
					//stop moving
					horizontalin = 0;
					//play ide animation
					m_Anim.SetInteger ("Activity_Select", 0);
				}
			}
			//=========================================================

		}


	}








	//if using mouse instead of touch
	void Mouse_input_Func(){
		//raycast general vars
		RaycastHit hit;
		Ray ray;








		//============new add
		// Bit shift the index of the layer (5) to get a bit mask
		int layerMask = 1 << 5;// This would cast rays only against colliders in layer 5. (UI is layer 5)
		// But instead we want to collide against everything except layer 5. The ~ operator does this, it inverts a bitmask.
		layerMask = ~layerMask;
		//============new add



		//====raycast from cam eye view point to world plane point (project down and use the hit point as world point)============
		ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if(Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))//Physics.Raycast(ray, out hit)
		{



			//Debug.DrawLine (transform.position, hit.point, Color.red);
			//update cursor object position
			//CursorObject.transform.position = hit.point;
			//spin the cursor object
			//CursorObject.transform.Rotate(0, 4, 0);

			//get the object we are pointing to with racast
			//ClickedObject = hit.collider.gameObject;


			//========get inputs
			if(Input.GetMouseButtonDown (0))// 0=left click, 1=right click
			{


				//get the object we are pointing to with racast
				ClickedObject = hit.collider.gameObject;


				//dont move cursor object if we clicked on a button (UI layer 11)
				if(ClickedObject.layer != 11){
					//Destination_Change = 1;
					if(ClickedObject.tag == "Enemy_01_Select_Tag"){

						//get the enemy select objects script

					}



				}//if(ClickedObject.layer != 11){
				//======================================


			}
			//================




			if(Input.GetMouseButtonDown (1))// 0=left click, 1=right click
			{

			}


		}





	}
	//=============








	// Update is called once per frame
	void Update () {
		//move character continiously (1= back 1= front)
		//horizontalin = 1;


		//game flow:


		//if the player is in the stump zone and has no energy then knock them back
		PushMeBack = 0;
		if (Weapon_Energy_value == 0 && Player_Enter_StumpPoint == 1) {
			PushMeBack = 600;
		} else {
			playDamageonce = 0;
		}



		//if there is no enemy then reset Player_Enter_StumpPoint (which is set by enemies) just in case to let player be able to move again
		if (!Current_Enemy_Transform) {
			Player_Enter_StumpPoint = 0;
		}
		//===============================

		//as a bonus if the energy meter is more than 40 then player moves faster
		m_MaxSpeed = 5f;
		if (Weapon_Energy_value > 40) {
			m_MaxSpeed = 7f;
		}
		if (Weapon_Energy_value > 60) {
			m_MaxSpeed = 8f;
		}
		if (Weapon_Energy_value > 80) {
			m_MaxSpeed = 9f;
		}
		if (Weapon_Energy_value == 100) {
			m_MaxSpeed = 12f;
		}
		//================================


		//move the character forward as long as there is energy and if there is an enemy to kill
		if (Weapon_Energy_value > 0 && Player_Enter_StumpPoint == 0 && PushMeBack == 0)
		{
			//select direction to run based on enemy direction
			if (Current_Enemy_Transform)
			{
				//run to left
				if (transform.position.x > Current_Enemy_Transform.position.x)
				{
					Run_Now = 2;
				}
				//run to right if your x is less than enemy
				if (transform.position.x <= Current_Enemy_Transform.position.x)
				{
					Run_Now = 1;
				}
			}
		}
		else
		{
			Run_Now = 0;
		}
		//also stop the player in place if the left button is held down at a stop location
		if (Input.GetMouseButton (0)) {//Input.GetMouseButton (0) is for mouse button hold
			Clicked_HoldDownNow = 1;
			//stop player if they are in a stopable zone
			//Run_Now = 0;
		} 
		else
		{
			Clicked_HoldDownNow = 0;
		}
		//==========================================

		//m_Jumpin = false;
		//if(Input.GetKeyDown(KeyCode.Space) == true){
		//	m_Jumpin = true;
		//}
		//========================


		//check mouse with checking objects clicked
		//Mouse_input_Func ();

		//see if player is clicking (dont respond to clicking if button is held down)
		Clicked_Now = 0;
		if (Clicked_Now == 0) //Clicked_HoldDownNow
		{
			if (Input.GetMouseButtonDown (0)) { // 0=left click, 1=right click
				Clicked_Now = 1;
			}
		}
		//=============================================


		//if the JumptrapJumpEnbale then jump if player is clicking
		m_Jumpin = false;
		if (JumptrapJumpEnbale == 1)
		{
			if (Clicked_Now == 1)
			{
				m_Jumpin = true;
			}
		}
		//==============================


		//reset weapon spin animation before each use (or it will be stuck on spin)
		Weapon_Anim.SetInteger ("Activity_Select", 0);

		//add weapon energy if player is clicking
		if (Weapon_Energy_value <= 100 && Clicked_Now == 1)
		{
			//if the player is in the enemy Player_Enter_StumpPoint == 0 then dont allow more than a certain amount of energy to be generated (to make the player enter zone with plenty energy)
			if (Player_Enter_StumpPoint == 1)
			{
				if (Weapon_Energy_value < 10)
				{
					max_Energy_Allowed = 5;
				}
			}
			else
			{
				max_Energy_Allowed = 100;
			}

			Weapon_Energy_value = Weapon_Energy_value + 5;
			if (Weapon_Energy_value >= max_Energy_Allowed)//100
			{
				Weapon_Energy_value = max_Energy_Allowed;
			}
			//set Enable player to start running if energy level is detected 
			if (Weapon_Energy_value >= 20) {
				Initial_Energy_Detected = 1;
			}
			//===========================
			//play the weapon spin animation as long as there is energy and player is in stump zone range of enemy
			if (Weapon_Energy_value > 0 && Player_Enter_StumpPoint == 1)
			{
				Weapon_Anim.SetInteger ("Activity_Select", 1);//it has exit time
			}

		}
		//==================================

		Enable_Timed_Energy_Drain = 1;
		//Drain Weapon Energy if there is no clicking going on and you are in an enemy stump zone
		if (Enable_Timed_Energy_Drain > 0 && Player_Enter_StumpPoint == 1) {// && Clicked_Now == 0
			if (Weapon_Energy_value > 0) {// && Clicked_Now == 0
				Weapon_Energy_DecrementINC += 2 * Time.deltaTime;
				if (Weapon_Energy_DecrementINC > 2) {
					Weapon_Energy_DecrementINC = 0;
					Weapon_Energy_value = Weapon_Energy_value - 5;
					if (Weapon_Energy_value < 0) {
						hit_INC = 0;
						Weapon_Energy_value = 0;
						//turn off blood mode
						//Blood_Mode = 0;
						//sprite_renderer.enabled = true;
						//set white color
						//sprite_renderer.color = Color.white;
					}
				}
			}
		}
		//====================



		//update energy status
		Weapon_Energy_Meter.value = Weapon_Energy_value;
		//Blood_Meter_Text.text = "Blood level: " + Blood_Meter_value;

		playerText.text = "horizontalin: " + horizontalin + " kill " + Enemy_Killed_Count + " stump " + Player_Enter_StumpPoint;

		//show player score
		PlayerScoreText.text = "Score: " + Player_Score;
		//show player Level
		PlayerLevelText.text = "Level: " + PlayerLevel;


	}//update


	public void Move(float move, bool crouch, bool jump)
	{
		if (Health > 0) {

			// If crouching, check to see if the character can stand up
			if (!crouch) {
				// If the character has a ceiling preventing them from standing up, keep them crouching
				if (Physics2D.OverlapCircle (m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround)) {
					crouch = true;
				}
			}


			//only control the player if grounded or airControl is turned on
			if (m_Grounded || m_AirControl) {
				// Reduce the speed if crouching by the crouchSpeed multiplier
				move = (crouch ? move * m_CrouchSpeed : move);


				// Move the character
				m_Rigidbody2D.velocity = new Vector2 (move * m_MaxSpeed, m_Rigidbody2D.velocity.y);

				// If the input is moving the player right and the player is facing left...
				if (move > 0 && !m_FacingRight) {
					// ... flip the player.
					Flip ();
				}
				// Otherwise if the input is moving the player left and the player is facing right...
				else if (move < 0 && m_FacingRight) {
					// ... flip the player.
					Flip ();
				}
			}
			// If the player should jump...
			if (m_Grounded && jump) {
				// Add a vertical force to the player.
				if (jump_Steps == 0) {
					jump_Steps = 1;
					jumpresetlINC = 0;
					//start the jump first animation
					m_Anim.SetInteger ("Activity_Select", 1);

				}
				print ("Jumping");
				m_Rigidbody2D.AddForce (new Vector2 (0f, m_JumpForce));
				Start_Going_Down_Once = 1;
				//play the jump sound
				PlaySingle (jumpSound, Volume_Normal);

			}




			//start the second jump fall animation make sure we are actually off the ground
			if (m_Grounded == false && jump_Steps == 1 && m_Rigidbody2D.velocity.y < 0) {
				jump_Steps = 2;
				m_Anim.SetInteger ("Activity_Select", 2);
			}
			//when we land then go to idle anim and it will take care of the rest of the transitions
			if (jump_Steps == 2 && m_Grounded == true) {
				jump_Steps = 0;
				m_Anim.SetInteger ("Activity_Select", 0);
			}

			//fix to reset jump steps if its stuck on mid jump anim while being on the ground
			if (TrytoFixStuck_INJump == true) {
				if (jump_Steps > 0 && m_Grounded == true) {
					jumpresetlINC += 2 * Time.deltaTime;
					if (jumpresetlINC > 1) {
						jumpresetlINC = 0;
						jump_Steps = 0;
						m_Anim.SetInteger ("Activity_Select", 0);
						print ("Jump Fixed");
					}
				}
			}
			//===========================

			// If the player looses a battle the will be pushed back
			if (Current_Enemy_Transform)
			{
				//select direction to run based on enemy direction
				if (PushMeBack != 0 && m_Rigidbody2D.velocity.x <= 0.01f)
				{
					//push to left
					if (transform.position.x > Current_Enemy_Transform.position.x) {
						// Add a vertical force to the player.
						m_Rigidbody2D.AddForce (new Vector2 (PushMeBack, 100));//100
					}
					//push to right
					if (transform.position.x <= Current_Enemy_Transform.position.x) {
						// Add a vertical force to the player.
						m_Rigidbody2D.AddForce (new Vector2 (-PushMeBack, 100));//100
					}
					if (playDamageonce == 0) {
						playDamageonce = 1;
						//play the damage sound
						PlaySingle (damageSound, Volume_Normal);
					}
				}
			}
			//===========================================================

		}
	}//move


	//to send your position
	public Vector3 Send_Position()
	{
		//send your position only to the enemy
		return transform.position;
	}
	//to recieve current enemy position for tracking (use only by enemies)
	public void Recieve_Enemy_Transform(Transform Current_Enemy_Transform_IN)
	{
		//if you dont have a current enemy then the next available enemy will send thier transform to you
		if (!Current_Enemy_Transform) {
			//recieve the curent enemy transform
			Current_Enemy_Transform = Current_Enemy_Transform_IN;
		}
	}
	//to send weather player has a current enemy
	public bool Player_Has_Current_Enemy()
	{
		//if you have enemy
		if (Current_Enemy_Transform) {
			//send 
			return true;
		}
		//send 
		return false;
	}


	public void Hit_Damage_IN_Func(){
		Damage_Destroy(1);
	}



	private void Damage_Destroy(int damage_amount_IN){

		//only do damage to player if hes not in blood mode
		if (InVinsible_Mode == 0) {
			Health = Health - damage_amount_IN;
			hit_Blink = 1;
			if (Health <= 0) {
				Health = 0;
				//Destroy(gameObject);

			}
		}

	}


	//to send weapon energy status to the enemy
	public int Weapon_Energy_OUT_Func()
	{
		//if the player is clicking then send energy damage data to the enemy
		if (Clicked_Now == 1)
		{
			return Weapon_Energy_value;
		}
		return 0;
	}

	//output the player's damage level
	public int Player_Damage_Level_out_Func()
	{
		int Damage_Tm = 10;
		if (PlayerLevel > 5){ Damage_Tm = 20;}
		if (PlayerLevel > 10){ Damage_Tm = 25;}
		if (PlayerLevel > 15){ Damage_Tm = 30;}
		if (PlayerLevel > 25){ Damage_Tm = 40;}
		if (PlayerLevel > 35){ Damage_Tm = 50;}
		if (PlayerLevel > 45){ Damage_Tm = 55;}
		if (PlayerLevel > 65){ Damage_Tm = 60;}
		return Damage_Tm;
	}
	//output the player's level
	public int Player_Level_out_Func()
	{
		return PlayerLevel;
	}


	//to jump when in rang or jump trap
	public void JumptrapJumpEnbaleINFunc(int JumptrapJumpEnbaleIN)
	{
		JumptrapJumpEnbale = JumptrapJumpEnbaleIN;
	}
	//to zap set player energy directly
	public void SetPlayerEnergyDirectly(int EnergyValueIN)
	{
		Weapon_Energy_value = EnergyValueIN;
	}

	public void Player_Enter_StumpPoint_IN(int StumpPoint_Entered_IN)
	{
		Player_Enter_StumpPoint = StumpPoint_Entered_IN;
	}

	//for enemy use to reduce player weapon energy when player hits enemy
	public void Subtract_Weapon_Energy(int amount_IN)
	{
		//only do damage to player if hes not in blood mode
		if (InVinsible_Mode == 0) {
			Weapon_Energy_value = Weapon_Energy_value - amount_IN;
			if (Weapon_Energy_value <= 0) {
				Weapon_Energy_value = 0;
				//Destroy(gameObject);
			}
		}
	}
	//=============================

	//for adding score (use by enemies)
	public void AddScore_For_keealingEnemy(int AddScore_IN)
	{
		int playingLevelUp = 0;
		//only do damage to player if hes not in blood mode
		if (Player_Score < 9000000 && PlayerLevel < 9000) {//9000000
			Player_Score = Player_Score + AddScore_IN;
			if (Player_Score > LevelAvanceScore) {
				LevelAvanceScore = Player_Score + 200;//level up every 1000 points 45000 max
				PlayerLevel = PlayerLevel + 1;
				//play the level up
				PlaySingleSecondAudio (LevelUpSound, Volume_Normal);
				playingLevelUp = 1;
			}
			//play the hit sound
			if (playingLevelUp == 0) {
				PlaySingle (HitSound, Volume_Normal);
			}
		}
	}
	//=============================



	public void Enemy_Killed_Counter_Func()
	{

		int DeathRandomSoundSelect = 0;
		Enemy_Killed_Count = Enemy_Killed_Count + 1;

			//=====play the random between EnemyDeathSFX2 and EnemyDeathSFX when its a normal stomp kill
			DeathRandomSoundSelect = UnityEngine.Random.Range(1, 10);
			if (DeathRandomSoundSelect >= 4){
				PlaySingle (EnemyDeathSFX1, Volume_Normal);
			}else{
				PlaySingle (EnemyDeathSFX2, Volume_Normal);
			}
			//=======================

	}



	private void Flip()
	{
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}


}//MonoBehavior
