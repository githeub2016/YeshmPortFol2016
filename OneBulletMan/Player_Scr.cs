using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class Player_Scr : MonoBehaviour {

	[SerializeField]
	private GameObject Rotated_To_Target_prefab;//(as a reference for smooth turning)
	private GameObject Rotated_To_Target;//this snaps to the current tagets rotation then this object slowly turns toward it for smoother turns (as a reference for smooth turning)


	private GameObject ClickedObject;
	//movement===========
	private int MoveFront = 0;
	public float MoveSpeed = 0.0f;
	//private float topSpeed = 160;
	private	float throttle = 0;
	private float JumpSpeed = 0;
	private float MoveForce_And_JumpSpeed;
	
	//steering=========
	private float turnSpeed = 1;//3
	private float steer = 0;
	//==================
	//the target to track
	private int CurrentTargetType;
	private int Current_Target_Change;//to detect each time an new target has been selected
	private string CurrentTarget_Name_ID;//to get the name from its code if it has code attached
	private string CurrentTarget_Name_ID_OLD;
	private GameObject CurrentTarget;
	private Vector3 Nav_To_CurrentTarget;//the result guidance way point position to the current target
	private Vector3 MousePosVector3;//the result guidance way point position to the current target
	[SerializeField]
	private Transform MouselookPoint;
	private int Energy = 0;
	private int SlowMo = 0;

	//get the interaction status display object
	private Slider Energy_Status_slider;
	private GameObject Game_Canvas;
	private Slider SlowMo_Status_slider;
	private int Fire_Bullet = 0;

	private float Energy_Inc = 0;
	private float SlowMo_Inc = 0;

	[SerializeField]
	private Text playerText = null;

	private GameObject The_Bullet_Capsule;
	private int Score = 0;
	private bool Boss_Is_Dead = false;
	private Text Score_Status_Text;
	private int Fire = 0;

	public bool You_Win = false;
	public bool You_Lose = false;
	private int Startchecking_Win_Lose = 0;

	private int Minimum_Score = 0;//accounting for boss kill points (150)

	private string Level_Summary_Text_Out;
	private int Enemies_Killed = 0;
	private bool Show_Main_Targets = false;


	//=====for sound stuff
	private AudioSource audiosource; //to get audio source component attached to this object
	private float Volume_Normal = 1;// to set volume (0 to 1)
	[SerializeField]
	private AudioClip FireBulletSound;
	[SerializeField]
	private AudioClip Ricochet01;
	[SerializeField]
	private AudioClip Ricochet02;
	[SerializeField]
	private AudioClip Ricochet03;
	//a second audio source just For_whirring_wind_sound
	private AudioSource audiosource_002; //to get audio source component attached to child of this object
	[SerializeField]
	private AudioClip Whirring_wind_sound;
	private float Whirring_wind_sound_Volume = 0.5f;// to set volume (0 to 1)
	private int Play_whirring_wind_sound = 0;
	private Animator Pop_Up_Score_Animator;
	private Text Pop_Up_Score_Text;
	private int Enable_Return_Pop_Up_Anim_To_Idle = 0;

	//===============


	//Use this for initialization
	void Start () {
		//spawn your own rotation smoothing ref object
		Rotated_To_Target = Instantiate (Rotated_To_Target_prefab, transform.position + transform.forward, transform.rotation) as GameObject;

		//get the game canvas
		Game_Canvas = GameObject.FindWithTag("Game_Canvas_Tag");

		//get the energy slider
		Energy_Status_slider = Game_Canvas.transform.Find("Game_Panel").Find("Bullet_Energy_Slider").transform.GetComponent<Slider>();
		//get the SlowMo_Status_slider
		SlowMo_Status_slider = Game_Canvas.transform.Find("Game_Panel").Find("Bullet_SlowMo_Slider").transform.GetComponent<Slider>();

		//get the bulet object (child in this case)
		The_Bullet_Capsule = transform.Find("Bullet_Head_Obj").gameObject;
		//get the score ouput text
		Score_Status_Text = Game_Canvas.transform.Find("Game_Panel").Find("Score_Status_Text").transform.GetComponent<Text>();

		//get the AudioSource component
		audiosource = GetComponent<AudioSource>();
		//get the For_whirring_wind_sound AudioSource component
		audiosource_002 = transform.Find("For_whirring_wind_sound").GetComponent<AudioSource>();
		//get the pop_up_Score animator
		Pop_Up_Score_Animator = Game_Canvas.transform.Find("Game_Panel").Find("Pop_Up_Score_Panel").transform.GetComponent<Animator>();
		//get the Pop_Up_Score_Text ouput text
		Pop_Up_Score_Text = Game_Canvas.transform.Find("Game_Panel").Find("Pop_Up_Score_Panel").Find("Image").Find("Pop_Up_Text").transform.GetComponent<Text>();

	}

	public void Play_Pop_Up_Score_Animation (int Value_In) {
		Pop_Up_Score_Animator.SetInteger ("Pop_up_Act_Select", Value_In);
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

	
	//Used to play single sound clips on second audio source
	public void PlaySingle_Second_Audio_Source(AudioClip clip, float volume_In)
	{
		//Set the clip of our efxSource audio source to the clip passed in as a parameter.
		audiosource_002.clip = clip;
		audiosource_002.volume = volume_In;
		//Play the clip.
		audiosource_002.Play ();
	}



	//function to Set_Minimum_Score_Func
	public void Set_Minimum_Score_Func (int Amount_In) {
		Minimum_Score = Amount_In;
	}
	//==================
	



	//function to add Energy
	public void Move_To_Position_Func (Vector3 Position_IN) {
		GetComponent<Rigidbody> ().position = Position_IN;
	}
	//==================

	//show or not show bulet
	public void Enable_Render_Func (bool Render_True_False_IN) {
		The_Bullet_Capsule.transform.GetComponent<Renderer>().enabled = Render_True_False_IN;
	}
	//==================
	


	//function to add Energy
	public void Add_Energy (int Amount_In) {
		Energy = Energy + Amount_In;
		if (Energy > 100) {
			Energy = 100;
		}
		//update energy meter
		Energy_Status_slider.value = Energy;
	}
	//==================
	//function to Subtract_Energy
	public void Subtract_Energy (int Amount_In) {
		Energy = Energy - Amount_In;
		if (Energy < 0) {
			Energy = 0;
		}
		//update energy meter
		Energy_Status_slider.value = Energy;
	}
	//==================




	//function to Add_SlowMo
	public void Add_SlowMo (int Amount_In) {
		SlowMo = SlowMo + Amount_In;
		if (SlowMo > 100) {
			SlowMo = 100;
		}
		//update SlowMo_Status_slider meter
		SlowMo_Status_slider.value = SlowMo;
	}
	//==================
	//function to Subtract_SlowMo
	public void Subtract_SlowMo (int Amount_In) {
		SlowMo = SlowMo - Amount_In;
		if (SlowMo < 0) {
			SlowMo = 0;
		}
		//update SlowMo_Status_slider meter
		SlowMo_Status_slider.value = SlowMo;
	}
	//==================

	//function to 
	public void Add_Score (int Amount_In) {
		Score = Score + Amount_In;
		//set the score text to the amount gotten
		Pop_Up_Score_Text.text = "+" + Amount_In;
		//play the popup score animation
		Play_Pop_Up_Score_Animation(1);
		//enable to ruturn back to idle animation
		if (Enable_Return_Pop_Up_Anim_To_Idle == 0) {
			//start coroutine to set the animation back to idle state
			StartCoroutine (Return_Pop_Up_Animation_To_Idle ());
		}

		//update the score text
		Score_Status_Text.text = "Score: " + Score;
	}
	//==================



	//================================================
	IEnumerator Return_Pop_Up_Animation_To_Idle ()
	{
		//the loop condition variable
		int Listen_for = 1;
		float gesture_INC = 0.0f;

		while(Listen_for == 1)
		{
			//====return to idle after each movement
			if (Pop_Up_Score_Animator.GetCurrentAnimatorStateInfo (0).IsName ("Score_Pop_Up_Anim")) { 
				if (Pop_Up_Score_Animator.GetCurrentAnimatorStateInfo (0).normalizedTime >= 0.88f) {
					Pop_Up_Score_Animator.SetInteger ("Pop_up_Act_Select", 0);
					Listen_for = 0;
					//print ("pop up animation done");
					break;
				}
			}
			//================

			//====leave loop if animation does not stop in 20 seconds
			gesture_INC += Time.deltaTime;
			if(gesture_INC > 20.2f) {
				Pop_Up_Score_Animator.SetInteger ("Pop_up_Act_Select", 0);
				Listen_for = 0;
				break;
				//==============
			}
			//======================

			yield return null;
		}

		Enable_Return_Pop_Up_Anim_To_Idle = 0;
	}
	//============================



	//function to 
	public void Add_Enemies_Kill_Count_Func (int Sound_Type_IN) {
		//called once by each enemy killed
		Enemies_Killed = Enemies_Killed + 1;

		//play the sound (based on which type of enemy)
		if (Sound_Type_IN == 1) {
			PlaySingle (Ricochet01, Volume_Normal);
		}
		//play the sound (based on which type of enemy)
		if (Sound_Type_IN == 2) {
			PlaySingle (Ricochet02, Volume_Normal);
		}
		//play the sound (based on which type of enemy)
		if (Sound_Type_IN == 3) {
			PlaySingle (Ricochet03, Volume_Normal);
		}

	}
	//==================
	//function to se how many enemies killed
	public int Show_Enemies_Killed_Func () {
		return Enemies_Killed;
	}
	//==================
	//function to enable show all main targets
	public void Show_Main_Targets_Func (bool Show_Main_Targets_IN) {
		Show_Main_Targets = Show_Main_Targets_IN;
	}
	//==================
	//function to show all main targets output
	public bool Show_Main_Targets_Output_Func () {
		return  Show_Main_Targets;
	}
	//==================

	
	//set if boss is dead
	public void Boss_Is_Dead_In_Func (bool Boss_Is_Dead_IN) {
		Boss_Is_Dead = Boss_Is_Dead_IN;
	}
	//==================
	//see you win
	public bool You_Win_Out_Func () {
		return You_Win;
	}
	//==================
	//see you lose
	public bool You_Lose_Out_Func () {
		return You_Lose;
	}

	//==================
	public void Fire_This_Bullet_Func () {
		//allow firing only once
		if (Fire_Bullet == 0) {
			//enable bullet to move front
			MoveFront = 1;
			Fire_Bullet = 1;
			//add energy to the bullet (100%)
			Add_Energy (100);

			//play the fire sound (happens only once)
			PlaySingle (FireBulletSound, Volume_Normal);

		}
	}
	//=======================
	
	public void Stop_This_Bullet_Func () {
		//disable bullet movement
		MoveFront = 0;
		GetComponent<Rigidbody> ().velocity = Vector3.zero;
	}

	
	//to be used by enemies to know when bullet is fired
	public int Bullet_Fired_Out_Func () {
		return Fire_Bullet;
	}

	//function to show score
	public int Score_OutPut_Func () {
		return Score;
	}
	//==================
	public string Level_Summary_Text_OutPut_Func () {
		return Level_Summary_Text_Out;
	}
	//==================

	
	//if using mouse instead of touch
	void Mouse_input_Func(){
		//raycast general vars
		RaycastHit hit;
		Ray ray;


		//float mouseX = Input.mousePosition.x;
		//float mouseY = Input.mousePosition.y;
		float screenX = Screen.width;
		float screenY = Screen.height;
		//if (mouseX > 0 || mouseX < screenX || mouseY > 0 || mouseY < screenY) {
		//}


		//get the mouse position
		MousePosVector3 = Input.mousePosition;

		//=========limit the max and min values that will be used to steer the bullet
		if (MousePosVector3.x < 20) {
			MousePosVector3.x = 20;
		}
		if (MousePosVector3.y < 20) {
			MousePosVector3.y = 20;
		}
		if (MousePosVector3.x > screenX - 20) {
			MousePosVector3.x = screenX - 20;
		}
		if (MousePosVector3.y > screenY - 20) {
			MousePosVector3.y = screenY - 20;
		}
		//==================================================================

		//playerText.text = "mouse x" + MousePosVector3.x;

		// Bit shift the index of the layer (5) to get a bit mask
		int layerMask = 1 << 5;// This would cast rays only against colliders in layer 5.
		// But instead we want to collide against everything except layer 5 UI. The ~ operator does this, it inverts a bitmask.
		//layerMask = ~layerMask;
		
		
		
		//====raycast from cam eye view point to world plane point (project down and use the hit point as world point)============
		ray = Camera.main.ScreenPointToRay (MousePosVector3);//Input.mousePosition);
		if (Physics.Raycast (ray, out hit, Mathf.Infinity, layerMask)) {//Physics.Raycast(ray, out hit)
			
		
			//Debug.DrawLine (transform.position, hit.point, Color.red);

			//Nav_To_CurrentTarget = hit.point;

			
			//just to update mouse point in unity world
			MouselookPoint.position = hit.point;
			Nav_To_CurrentTarget = MouselookPoint.position;

			//========get inputs
			if (Input.GetMouseButtonDown (0)) {// 0=left click, 1=right click
				//get the object we are pointing to with racast
				ClickedObject = hit.collider.gameObject;
				
			}

		}

		

	}
	//=================

	void Update () {

		Mouse_input_Func ();




		//====check win lose conditions
		if (Fire_Bullet == 1) {

			if (Startchecking_Win_Lose < 30) {
				Startchecking_Win_Lose = Startchecking_Win_Lose + 1;
			}

				//wait to start checking win or lose conditions cause bullet doesnt have energy at begining of game
			if (Startchecking_Win_Lose == 30) {

				//start playing the whirring wind sound (only once and it is set to loop in the inspector)
				if (Play_whirring_wind_sound == 0) {
					PlaySingle_Second_Audio_Source (Whirring_wind_sound, Whirring_wind_sound_Volume);
					Play_whirring_wind_sound = 1;
				}




				//check wether player has at least minimum score after boss death
				if (Boss_Is_Dead == true) {

					if (Score >= Minimum_Score) {
						//check wining condition
						if (Energy > 0) {
							You_Win = true;
							You_Lose = false;
							//stop player movement
							MoveFront = 0;
							Level_Summary_Text_Out = "Target eliminated, good job!";

						}
					}else{//if player has not gotten enough score
						You_Win = false;
						You_Lose = true;
						//stop player movement
						MoveFront = 0;
						Level_Summary_Text_Out = "Minimum score objective failed";
					}



				}


				//check fail condition
				if (Energy <= 0) {
					You_Win = false;
					You_Lose = true;
					//stop player movement
					MoveFront = 0;
					Level_Summary_Text_Out = "Mission failed, you are out of energy";
				}

			}
		}
		//===========================


	// decrement the bullet energy with time seconds (as long as game is not finished)
		if (You_Win == false && You_Lose == false) {
			if (Energy > 0 && Fire_Bullet == 1) {
				Energy_Inc += 2 * Time.deltaTime;
				if (Energy_Inc > 1) {
					Energy_Inc = 0;
					Subtract_Energy (1);
				}
			}
		}


		//normal speed
		if (MoveFront == 1) {// 0=left click, 1=right click
			MoveSpeed = 3000;
			throttle = 1;

			//slow motion
			if (Input.GetKey ("space")) {
				//if there is slow mo then use it
				if (SlowMo > 0) {
					MoveSpeed = 700;
					throttle = 1;
					//=====decrement while in use

					SlowMo_Inc += 4 * Time.deltaTime;
					if (SlowMo_Inc > 1) {
						SlowMo_Inc = 0;
						Subtract_SlowMo (1);
					}
					//=====================
				}

			}
		} else {//stop the bullet if move fron is zero
			MoveSpeed = 0;
			throttle = 0;
			GetComponent<Rigidbody> ().velocity = Vector3.zero;
		}

		//just to update mouse point in unity world
		//MouselookPoint.position = Nav_To_CurrentTarget;
		
		//MouselookPoint.position = Vector3.Lerp(MouselookPoint.position, Nav_To_CurrentTarget, Time.deltaTime * 6);
	}

	// Update is called once per frame
	void FixedUpdate () {//Update () {



		
		//Now rotation and movement are independent of each other (they can do thier own thing or be synced
		Vector3 Target_To_Look_At = Vector3.zero;
			Target_To_Look_At = Nav_To_CurrentTarget;
		
		//----============new smoot rotation Look at and dampen the rotation
			//move to this current position
			Vector3 pos = transform.position;
			//pos.x = transform.position.x;
			//pos.y = transform.position.y;// CurrentTarget.transform.position.y;//
			//pos.z = transform.position.z;
			//always put target and tracker on the same level always
			//Target_To_Look_At.y = transform.position.y;
			
			Rotated_To_Target.transform.position = pos;
			//rotate snap to curent target
			Rotated_To_Target.transform.LookAt (Target_To_Look_At);//(CurrentTarget.transform.position);//
			//smoothly rotate this object to current target rotation tracking
			float Rotationdamping = 5.2f;//6f;//
			Quaternion rotation = Rotated_To_Target.transform.rotation;// Quaternion.LookRotation(target.position - transform.position);
			transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * Rotationdamping);

		//================limit the X (up down) rotation of the bulet
		//float X_Rot_Limit = transform.eulerAngles.x;
		//Vector3 rot = Vector3.zero;
		//limit uper half x rotation
		//if (transform.eulerAngles.x > 320 && transform.eulerAngles.x < 359){
		//	if (transform.eulerAngles.x < 325){
		//		X_Rot_Limit = 325;
		//	}
		//	rot = new Vector3(X_Rot_Limit,transform.eulerAngles.y,transform.eulerAngles.z);
		//	transform.eulerAngles = rot;
		//}
		//limit lower half x rotation
		//if (transform.eulerAngles.x > 0 && transform.eulerAngles.x < 40){
		//	if (transform.eulerAngles.x > 35){
		//	X_Rot_Limit = 35;
		//	}
		//	rot = new Vector3(X_Rot_Limit,transform.eulerAngles.y,transform.eulerAngles.z);
		//	transform.eulerAngles = rot;
		//}
		//============================

		playerText.text = "move sp" + MoveSpeed;
		//-----===================
		



		
		//move rigid body in any direction regardless (independent) of rotation (this allows side stepping)
		MoveForce_And_JumpSpeed = MoveSpeed;// + JumpSpeed;
		float MoveForce = Mathf.Sign(throttle) * MoveForce_And_JumpSpeed * GetComponent<Rigidbody>().mass;
		Vector3 direction;
		
		direction = (Nav_To_CurrentTarget - transform.position).normalized;
		
		//not using but could use GetComponent<Rigidbody>().MovePosition(transform.position + direction * movementSpeed * Time.deltaTime);
		GetComponent<Rigidbody>().AddForce(transform.position + direction * MoveForce * Time.deltaTime);
		GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		//===============================================



		//add gravity again or not
		//GetComponent<Rigidbody>().velocity += Vector3.down * Time.deltaTime * 10;


	}










	
	//=========
	void OnTriggerEnter(Collider other)
	{
		//if hit reduce energy
		if (other.tag == "Enemy_Projectile_01_Tag") {
			
			Subtract_Energy(10);
		}
		if (other.tag == "SpikeTrap_Tag") {
			
			Subtract_Energy(10);
		}

		

		
	}
	//==========









}






