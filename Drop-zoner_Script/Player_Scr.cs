using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Player_Scr : MonoBehaviour
{
	// The fastest that the player can travel.
	private float m_MaxSpeed = 6f;
	// Amount of force added when the player jumps.
	private float m_JumpForce = 800f;
	// Whether or not a player can steer while jumping;
	[SerializeField]
	private bool m_AirControl = false;
	// A mask determining what is ground to the character
	[SerializeField]
	private LayerMask m_WhatIsGround;
	// Player output text UI object
	[SerializeField]
	private Text playerText = null;
	// A position marking where to check if the player is grounded.
	private Transform m_GroundCheck; 
	// Radius of the overlap circle to determine if grounded
	const float k_GroundedRadius = 0.2f; 
	// Whether or not the player is grounded.
	private bool m_Grounded;    
	// A position marking where to check for ceilings
	private Transform m_CeilingCheck;
	// Radius of the overlap circle to determine if the player can stand up
	const float k_CeilingRadius = 0.01f; 
	// Reference to the player's animator component.
	private Animator m_Anim;  
	// To get the player's Rigidbody component
	private Rigidbody2D m_Rigidbody2D;
	// For determining which way the player is currently facing.
	private bool m_FacingRight = true;  

	//to break down the jumping stages of the player (going up or comming down etc)
	private int jump_Steps = 0; 
	// Player health
	private int Health = 100; 
	private int currentHealthMAX = 100;
	private int DieOnce = 0;

	// Execute code or not
	private int Halt_Processes = 0;

	// the sounds clips of the player
	[SerializeField]
	private AudioClip jumpSound;
	[SerializeField]
	private AudioClip damageSound;
	[SerializeField] 
	private AudioClip GameOverMusic; 
	[SerializeField] 
	private AudioClip EnemyDeathSFX1; 
	[SerializeField] 
	private AudioClip EnemyDeathSFX2; 
	[SerializeField] 
	private AudioClip LevelUpSound; 
	[SerializeField] 
	private AudioClip LowEnergyWarning; 
	[SerializeField] 
	private AudioClip WinMusicFX; 
	[SerializeField]
	private AudioClip FireSound;
	[SerializeField]
	private AudioClip RocketThrustSound;
	[SerializeField]
	private AudioClip PlayerDeathSoundFX;

	// Audio Source (audio player) 1
	private AudioSource audiosource;
	// Audio Source (audio player) 2
	private AudioSource audiosourceSecond;



	//for displaying player score
	[SerializeField]
	private Text PlayerScoreText = null;
	//for displaying level
	[SerializeField]
	private Text PlayerLevelText = null;
	[SerializeField]
	private Slider Player_Health_Slider = null;
	//increment to reach before leveling up to next level 
	private int LevelAvanceScore = 100;
	private int Player_Score = 0;
	private int PlayerLevel = 0;



	// sound clips volume
	private float Volume_Normal = 0.5f;

	// for move input
	private bool crouchin = false;
	private float horizontalin = 0;
	private bool m_Jumpin = false;
	private float InVinsible_Mode = 0;

	// for World space UI, player energy meter indicator
	private Slider Weapon_Energy_Meter;
	private int Weapon_Energy_value = 0; 

	private int Enable_Timed_Energy_Drain = 1; 

	private int max_Energy_Allowed = 100;
	private int Hovering_Energy_value = 80;
	private float Hovering_Energy_DecrementINC = 0;  
	private bool Hovering_InputEnabled = true;


	private GameObject ClickedObject;
	private int newClick;
	private int Clicked_Now;

	private Animator Weapon_Anim;// Reference to the weapon's animator component.
	private int Run_Now = 0;


	private int Enemy_Killed_Count = 0;
	private float PushMeBack = 0; 
	private int playDamageonce = 0; 

	private Transform Current_Enemy_Transform;



	private float hoverForce = 85;//65f
	private float hoverHeight = 3.8f;

	private bool FlyingInPut = false;
	private bool HoveringEngaged = false;
	private float OldHitDistance = 0;
	private bool GentleBringDown = false;


	[SerializeField]
	private GameObject Green_Food_01_Prefab;
	private GameObject The_Projectile_Spawn;
	private Rigidbody2D RigidbodyProjectile2D;
	private bool FireNow = false;
	private int Enable_A_Spawn = 1;

	private float SpawnCooldownINC = 0; 
	private SpriteRenderer sprite_renderer;
	private int GoalReached = 0;
	private bool PlayRocketSoundOnce = false;
	private float FallSpeed_y = 0; 
	private float FallRate_y = 4.2f; 



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
		sprite_renderer = GetComponent<SpriteRenderer> ();
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


	//using FixedUpdate for physics operations
	private void FixedUpdate()
	{



		//==========Halt_Processes
		if (Halt_Processes == 0 && Health > 0) {

			//set the grounded to false before each ground detection
			m_Grounded = false;

			// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
			// This can be done using layers instead but Sample Assets will not overwrite your project settings.
			Collider2D[] colliders = Physics2D.OverlapCircleAll (m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
			for (int i = 0; i < colliders.Length; i++) {
				if (colliders [i].gameObject != gameObject) {
					//make sure not touching the camera check object (has a large colider)
					if (colliders [i].gameObject.tag != "Cam_CheckPoint_Tag") {
						m_Grounded = true;
					}
				}
			}
			//=================================================================================================

			// Pass all parameters to the character control function
			Move (horizontalin, crouchin, m_Jumpin);
			m_Jumpin = false;









			//====Hovering stuff========================================
			int layerMask = 0;
			if (FlyingInPut == true) 
			{
				if (HoveringEngaged == false)
				{
					// Bit shift the index of the layer (8) to get a bit mask
					layerMask = 1 << 8;// This would cast rays only against colliders in layer 5. (UI is layer 5)
					// But instead we want to collide against everything except layer 5. The ~ operator does this, it inverts a bitmask.
					//layerMask = ~layerMask;
					RaycastHit2D hit = Physics2D.Raycast (transform.position, -Vector2.up, 1000, layerMask);
					if (hit.collider != null) {
						//if the distance to ground is withing hovering range the engage hovering
						if (hit.distance < 2) {
							OldHitDistance = hit.distance;
							HoveringEngaged = true;
						}
					}
				}

				//move up if FlyingInPut is engaged and you are going down (so you never go up only maintain a height)
				if (m_Rigidbody2D.velocity.y < 0)
				{
					float proportionalHeight00 = 0.58f;
					Vector3 appliedUpForce = Vector3.up * proportionalHeight00 * 80;
					m_Rigidbody2D.AddForce (appliedUpForce);
				}
				//===================================================
			} 
			//======================================================



			//Hovering will engage if player starts flying and the ground becomes close enough
			if (HoveringEngaged == true) 
			{
				// Bit shift the index of the layer (8) to get a bit mask
				layerMask = 1 << 8;// This would cast rays only against colliders in layer 5. (UI is layer 5)
				// But instead we want to collide against everything except layer 5. The ~ operator does this, it inverts a bitmask.
				//layerMask = ~layerMask;
				RaycastHit2D hit = Physics2D.Raycast (transform.position, -Vector2.up, 100, layerMask);
				if (hit.collider != null) 
				{
					float proportionalHeight = (hoverHeight - hit.distance) / hoverHeight;
					Vector3 appliedHoverForce = Vector3.up * proportionalHeight * hoverForce;
					//to avoid slaming down
					if (appliedHoverForce.y < -50) 
					{
						appliedHoverForce.y = -50;
					}
					//=====================
					m_Rigidbody2D.AddForce (appliedHoverForce);
				}
				//turn off hovering if a fall is detected (suden change in ground hit distance)
				if (Mathf.Abs (OldHitDistance - hit.distance) > 1)
				{
					HoveringEngaged = false;
				}
				//also turn off hoverint if you hit the ground comming downward
				if (m_Grounded == true && m_Rigidbody2D.velocity.y < 0)
				{
					HoveringEngaged = false;
				}
				OldHitDistance = hit.distance;


				//limit the up force so that it never goes up too fast when hovering
				if (m_Rigidbody2D.velocity.y > 1.1f && m_Grounded == false) 
				{
					Vector2 vposYmod = m_Rigidbody2D.velocity;
					vposYmod.y = 1.1f;
					m_Rigidbody2D.velocity = vposYmod;
				}
			}
			//================================================






			//======Instant Death If you comeDown too fast===========================
			if (m_Rigidbody2D.velocity.y < -40.1f)
			{
				if (m_Grounded == true) {
					Damage_Destroy (100);
				}
			} 
			else
			{
				if (m_Rigidbody2D.velocity.y < -25.1f)//-18.1f old value
				{
					int damageFrean = (int)Mathf.Abs (m_Rigidbody2D.velocity.y) - 10;
					if (m_Grounded == true)
					{
						Damage_Destroy (damageFrean);
					}
				}
			}
			//=======================================================






			//do running if you have energy level you are on the ground and not in the process of jumping
			if (Run_Now > 0) 
			{
				//move foward
				if (Run_Now == 1)
				{ 
					horizontalin = 1;// (-1= back 1= front)
				}
				//move backward
				if (Run_Now == 2)
				{ 
					horizontalin = -1;// (-1= back 1= front)
				}
				//play the run animation if we are on the ground (and no jump 
				if (jump_Steps == 0)
				{ 
					if (m_Grounded == true)
					{
						m_Anim.SetInteger ("Activity_Select", 3);
					}
				}
			}
			//if player is not running the stop and play idle animation
			else
			{
				//stop moving
				horizontalin = 0;
				//Play the idle animation if you are on the ground and not doing any thing
				if (m_Grounded == true && jump_Steps == 0)
				{
					//play ide animation
					m_Anim.SetInteger ("Activity_Select", 0);
				}
			}
			//=========================================================





			//===============for Shooting projectile
			if (FireNow == true)
			{
				The_Projectile_Spawn = Instantiate (Green_Food_01_Prefab, transform.position, transform.rotation) as GameObject;
				//destroy after 5 seconds
				Destroy (The_Projectile_Spawn.gameObject, 4);
			
				//=====shoot the projectile item in a direction
				RigidbodyProjectile2D = The_Projectile_Spawn.transform.GetComponent<Rigidbody2D> ();
				//===shoot in the direction facing=======
				float ShootiNSpeed = 800;
				if (m_FacingRight == true) 
				{
					RigidbodyProjectile2D.AddForce (new Vector2 (ShootiNSpeed, 0));//100
				} else {
					RigidbodyProjectile2D.AddForce (new Vector2 (-ShootiNSpeed, 0));//100
				}

			}
			//================================================
			FireNow = false;
			//====================================================



		}




		//if there is no health then you lose
		if (Health == 0)
		{
			//do the death stuff
			//======Stop movement===========================
			if (DieOnce < 2)
			{
				m_Rigidbody2D.velocity = Vector2.zero;
				DieOnce = DieOnce + 1;
			}
			//
			if (DieOnce == 2) 
			{
				//play the death fx
				PlaySingle(PlayerDeathSoundFX, Volume_Normal);
				DieOnce = 3;
				//dont show the player
				sprite_renderer.enabled = false;
				//play the lose sound
				PlaySingleSecondAudio (GameOverMusic, Volume_Normal);

			}
			PlayerLevelText.text = "Game Over!";

		} 
		else
		{
			//see if player reached goal
			if (GoalReached == 1)
			{
				GoalReached = 2;
				//play the win sound
				PlaySingleSecondAudio (WinMusicFX, Volume_Normal);
			}
			//see if player reached goal
			if (GoalReached == 2) 
			{
				PlayerLevelText.text = "You Win!";
			}
		}
		//========================

	}//Fixedupdate




	// Update is called once per frame
	void Update ()
	{

		//game flow:
		if (Halt_Processes == 0 && Health > 0)
		{
			
			//The movements inputs
			Run_Now = 0;
			if (Input.GetKey (KeyCode.LeftArrow) || Input.GetKey (KeyCode.A))
			{
				Run_Now = 2;
			}
			if (Input.GetKey (KeyCode.RightArrow) || Input.GetKey (KeyCode.D))
			{
				Run_Now = 1;
			}
			if (Input.GetKeyDown (KeyCode.Space))
			{
				m_Jumpin = true;
			}

			//Hovering and flying input
			FlyingInPut = false;
			if (Hovering_InputEnabled == true && Hovering_Energy_value > 0)
			{
				if (Input.GetKey (KeyCode.UpArrow) || Input.GetKey (KeyCode.W))
				{
					FlyingInPut = true;
					//reset fall rate each time the flying input is used
					FallRate_y = 4.2f;
				}
			}

			//===========play the rocket sound each initial time
			if (FlyingInPut == true && PlayRocketSoundOnce == true)
			{
				PlayRocketSoundOnce = false;
				PlaySingle (RocketThrustSound, Volume_Normal);
			}
			if (FlyingInPut == false)
			{
				PlayRocketSoundOnce = true;
			}
			//===================================================



			//turn off hovering
			if (Input.GetKey (KeyCode.DownArrow) || Input.GetKey (KeyCode.S))
			{
				HoveringEngaged = false;
				//increase fall rate (only afects GentleBringDown down fall rate) if down arrow is used
				if (FlyingInPut == false && m_Grounded == false)
				{
					FallRate_y += 15.2f * Time.deltaTime;
					if (FallRate_y > 50.2f)
					{
						FallRate_y = 50.2f;
					}
				}
			}

			//turn off hovering
			if (Input.GetKey (KeyCode.F) && Enable_A_Spawn == 1 && Hovering_Energy_value >= 40)
			{
				FireNow = true;
				Enable_A_Spawn = 0;
				//play shoot sound
				PlaySingle (FireSound, Volume_Normal);
				//decremet the hover energy
				Hovering_Energy_value = Hovering_Energy_value - 40;
				//if theres no enegy then turn off hovering and flying input
				if (Hovering_Energy_value < 0) {
					Hovering_Energy_value = 0;
					HoveringEngaged = false;
				}
			}
			//==============================

			//if fired then do a count down to enable the firing again
			if (Enable_A_Spawn == 0)
			{
				SpawnCooldownINC += 4 * Time.deltaTime;
				if (SpawnCooldownINC > 2)
				{
					Enable_A_Spawn = 1;
					SpawnCooldownINC = 0;
				}
			}
			//======================




			//if there is fuel and you are flying or hovering then depleat fuel
			float EnergyDepletionRate = 2;
			if (HoveringEngaged == true || FlyingInPut == true || GentleBringDown == true)
			{
				//Deplete energy faster if the player is using the boost to maintain their position or reduce decent
				if (FlyingInPut == true) 
				{
					EnergyDepletionRate = 8;
				}
				//Reduce energy
				Hovering_Energy_DecrementINC += EnergyDepletionRate * Time.deltaTime;
				if (Hovering_Energy_DecrementINC > 2) 
				{
					Hovering_Energy_DecrementINC = 0;
					//decremet the hover energy
					Hovering_Energy_value = Hovering_Energy_value - 5;
					//if theres no enegy then turn off hovering and flying input
					if (Hovering_Energy_value < 0) {
						Hovering_Energy_value = 0;
						HoveringEngaged = false;
					}
				}
			} 
			else
			{//start recharging the energy if it is not being used

				//dont exceed max energy 0f 100
				if (Hovering_Energy_value < max_Energy_Allowed)
				{
					EnergyDepletionRate = 5;
					//Reduce energy
					Hovering_Energy_DecrementINC += EnergyDepletionRate * Time.deltaTime;
					if (Hovering_Energy_DecrementINC > 2)
					{
						Hovering_Energy_DecrementINC = 0;
						//Increment the hover energy
						Hovering_Energy_value = Hovering_Energy_value + 10;
					}
				}

			}
			//=======================================








			//Allways if theres no enegy then turn off hovering and flying input
			if (Hovering_Energy_value < 0)
			{
				Hovering_Energy_value = 0;
				HoveringEngaged = false;
			}
			//================================================


		}

		//update energy status
		Weapon_Energy_Meter.value = Hovering_Energy_value;
		Player_Health_Slider.value = Health;
		//Blood_Meter_Text.text = "Blood level: " + Blood_Meter_value;

		playerText.text = "horizontalin: " + horizontalin + " kill " + Enemy_Killed_Count + " y " + m_Rigidbody2D.velocity.y + "Fall Rate: " + FallRate_y;
		//show player score
		PlayerScoreText.text = "Score: " + Player_Score;
		//show player Level
		//PlayerLevelText.text = "Level: " + PlayerLevel;


	}//update


	public void Move(float move, bool crouch, bool jump)
	{
		if (Health > 0)
		{

			// If crouching, check to see if the character can stand up
			if (!crouch)
			{
				// If the character has a ceiling preventing them from standing up, keep them crouching
				if (Physics2D.OverlapCircle (m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
				{
					crouch = true;
				}
			}


			//only control the player if grounded or airControl is turned on
			if (m_Grounded || m_AirControl)
			{

				//also turn off GentleBringDown if HoveringEngaged
				if (HoveringEngaged == true) 
				{
					GentleBringDown = false;
				}


				// Move the character if you are on the ground
				if (m_Grounded)
				{
					m_Rigidbody2D.velocity = new Vector2 (move * m_MaxSpeed, m_Rigidbody2D.velocity.y);

					//turn off GentleBringDown in you are on the ground
					GentleBringDown = false;

				} 
				else
				{

					//only turn on gentle bring down if flying was just used and you are already off the ground and not hovering
					if (FlyingInPut == true && HoveringEngaged == false) 
					{
						GentleBringDown = true;
					}

					// Add a Horizontal force to the player if you are in the air
					m_Rigidbody2D.AddForce (new Vector2 (24 * move, 0));//100

					//limit the forward and back force so that it never goes too fast when hovering
					Vector2 vposYmodR = m_Rigidbody2D.velocity;
					float ForwardHoverSpeedLimit = 8.2f;
					if (m_Rigidbody2D.velocity.x > ForwardHoverSpeedLimit)
					{
						vposYmodR.x = ForwardHoverSpeedLimit;
						m_Rigidbody2D.velocity = vposYmodR;
					}
					if (m_Rigidbody2D.velocity.x < -ForwardHoverSpeedLimit)
					{
						vposYmodR.x = -ForwardHoverSpeedLimit;
						m_Rigidbody2D.velocity = vposYmodR;
					}
					//also bring the player down to zero if there is no more forward movement
					if (move == 0)
					{
						float ZeroHoverSpeed = 0.48f;
						if (m_Rigidbody2D.velocity.x > 0)
						{
							m_Rigidbody2D.AddForce (new Vector2 (10 * -ZeroHoverSpeed, 0));//100
						}
						if (m_Rigidbody2D.velocity.x < 0)
						{
							m_Rigidbody2D.AddForce (new Vector2 (10 * ZeroHoverSpeed, 0));//100
						}
					}


					//Also bring the player slowly down before cutting the rocket up force (too abrupt) (all this when player is not using rockets anymore)
					if (HoveringEngaged == false && FlyingInPut == false)
					{
						//if we start falling then make sure the fall speed is slowly increased (not all of a suden)
						if (GentleBringDown == true)
						{
							vposYmodR = m_Rigidbody2D.velocity;
							if (m_Rigidbody2D.velocity.y < 0) 
							{
								//start increasing the fall speed allowed (falling is a negetive number
								FallSpeed_y -= FallRate_y * Time.deltaTime;
								//if the fall velocity reaches limit then turn off GentleBringDown
								if (FallSpeed_y < -15) {
									FallSpeed_y = -15;//dont ever fall faster than -15 while GentleBringDown is on
									GentleBringDown = false;
								}

								//limit fall velocity to fall speed
								if (m_Rigidbody2D.velocity.y < FallSpeed_y)
								{
									vposYmodR.y = FallSpeed_y;
									m_Rigidbody2D.velocity = vposYmodR;
								}
							}
						}
					} 
					else
					{
						//colect the current veleocity y while the player is hovering or flying
						FallSpeed_y = m_Rigidbody2D.velocity.y;
					}
					//==========================================================


				}
				//==============================================


				// If the input is moving the player right and the player is facing left...
				if (move > 0 && !m_FacingRight)
				{
					// ... flip the player.
					Flip ();
				}
				// Otherwise if the input is moving the player left and the player is facing right...
				else if (move < 0 && m_FacingRight) 
				{
					// ... flip the player.
					Flip ();
				}
			}
			// If the player should jump...
			if (m_Grounded && jump)
			{
				// Add a vertical force to the player.
				jump_Steps = 1;
				//start the jump first animation
				m_Anim.SetInteger ("Activity_Select", 1);
				//play the jump sound
				PlaySingle (jumpSound, Volume_Normal);

				print ("Jumping");
				m_Rigidbody2D.AddForce (new Vector2 (0f, m_JumpForce));

			}

			//if we just left the ground after a jump
			if (m_Grounded == false && jump_Steps == 1 && m_Rigidbody2D.velocity.y > 0)
			{
				jump_Steps = 2;
			}

			//start the second jump fall animation make sure we are actually off the ground
			if (m_Grounded == false && jump_Steps > 0 && m_Rigidbody2D.velocity.y < 0)
			{
				jump_Steps = 3;
			}
			//when we land then reset jump steps
			if (jump_Steps >= 2 && m_Grounded == true)
			{
				jump_Steps = 0;
			}






			//========Play up and down fall any time we just left the ground after a jump or simply falling off a clif
			if (m_Grounded == false)
			{
				if (m_Rigidbody2D.velocity.y >= 0)
				{
					//start the jump first animation
					m_Anim.SetInteger ("Activity_Select", 1);
				}
				if (m_Rigidbody2D.velocity.y < 0)
				{
					//start the jump first animation
					m_Anim.SetInteger ("Activity_Select", 2);
				}
			}
			//=================================================






			// If the player looses a battle the will be pushed back
			if (Current_Enemy_Transform)
			{
				//select direction to run based on enemy direction
				if (PushMeBack != 0 && m_Rigidbody2D.velocity.x <= 0.01f)
				{
					//push to left
					if (transform.position.x > Current_Enemy_Transform.position.x)
					{
						// Add a vertical force to the player.
						m_Rigidbody2D.AddForce (new Vector2 (PushMeBack, 100));//100
					}
					//push to right
					if (transform.position.x <= Current_Enemy_Transform.position.x)
					{
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


	//to send your position (Out put)
	public Vector3 Send_Position()
	{
		//send your position only to the enemy
		return transform.position;
	}

	//to send weather player has a current enemy
	public bool Player_Has_Current_Enemy()
	{
		//if you have enemy
		if (Current_Enemy_Transform)
		{
			//send 
			return true;
		}
		//send 
		return false;
	}


	public void Hit_Damage_IN_Func()
	{
		Damage_Destroy(1);
	}

	public void Set_Goal_Reached()
	{
		GoalReached = 1;
	}



	public void Damage_Destroy(int damage_amount_IN)
	{
		//only do damage to player if hes not in blood mode
		if (InVinsible_Mode == 0)
		{
			if (Health > 0)
			{
				//play the damage sound
				PlaySingle (damageSound, Volume_Normal);

				Health = Health - damage_amount_IN;
				if (Health <= 0) 
				{
					Health = 0;
					//Destroy(gameObject);
				}
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


	//to zap set player energy directly
	public void SetPlayerEnergyDirectly(int EnergyValueIN)
	{
		Weapon_Energy_value = EnergyValueIN;
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
		if (Player_Score < 900000 && PlayerLevel < 900)
		{
			Player_Score = Player_Score + AddScore_IN;
			if (Player_Score > LevelAvanceScore)
			{
				//add bunus points per each level up
				Player_Score = Player_Score + 250;

				LevelAvanceScore = Player_Score + 200;//level up every 1000 points 45000 max
				PlayerLevel = PlayerLevel + 1;
				//play the level up
				PlaySingleSecondAudio (LevelUpSound, Volume_Normal);
			}
		}
	}
	//=============================



	public void Enemy_Killed_Counter_Func()
	{

		int DeathRandomSoundSelect = 0;
		//count enemy kills
		Enemy_Killed_Count = Enemy_Killed_Count + 1;

			//=====play the random sound between EnemyDeathSFX2 and EnemyDeathSFX1
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
		// Switch the way the player is facing.
		m_FacingRight = !m_FacingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}


}//MonoBehavior
