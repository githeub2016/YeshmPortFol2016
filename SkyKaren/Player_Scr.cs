using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Player_Scr : MonoBehaviour {


	private Animator animator;
	private float DirectionDampTime = .25f;
	[SerializeField]
	private Text playerText = null;



	
	//movement===========
	private int MoveFront = 0;
	private float MoveSpeed = 0.0f;
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
	private int Path_Status_Out;




	[SerializeField]
	private GameObject CursorObject_Prefab;
	private GameObject CursorObject;
	[SerializeField]
	private GameObject Rotated_To_Target_prefab;//get from prefab, this snaps to the current tagets rotation then this object slowly turns toward it for smoother turns (as a reference for smooth turning)
	private GameObject Rotated_To_Target;//this snaps to the current tagets rotation then this object slowly turns toward it for smoother turns (as a reference for smooth turning)

	private Transform player_target_chest_point;
	private Transform player_target_head_point;
	[SerializeField]
	private GameObject IK_Arm_Aim_Prefab;//get from prefab, for aiming at targets
	private GameObject IK_Arm_Aim_Obj;//get from prefab, for aiming at targets
	private Transform IK_Aim_Target;//what the ik tracks
	
	
	private GameObject Weapon_Object;
	private Player_Weapon_01_Scr Weapon_Object_Script_IN;
	//********For AI events and activities


	//Target Types selected
	private int TargetType_Enemy = 0;
	private int TargetType_Interactable = 0;
	private int TargetType_Destination = 0;


	//movement presets
	private int Preset_Walk_Front = 0;
	private int Preset_Run_Front = 0;
	private int Preset_Shoot_Target = 0;
	private int Preset_Idle_Standing = 0;
	private int Preset_Idle_Crouched = 0;
	private int Preset_Aim_Weapon = 0;
	private int Preset_TrackNavTo_Target_Path = 0;
	private int Preset_Face_NavTo_Target = 0;//turn body to face direction of navigatin to destinatin target as oposed to shooting target (target you are shooting at)
	private int Preset_Face_Shooting_Target = 0;
	private int Preset_Deselect_Target = 0;
	private int Preset_Die = 0;
	//============
	
	//Event (timed and caused or detected)=========
	private int Select_Initialization_Activity;
	//setup Event priority and arange Event responses acording to importance
	private int Event_Priority_Overide = 0;//corect
	private int Curent_Event_ID;
	private int Event_Taking_Damage;
	private int Event_You_Are_Destroyed;
	private int Event_Target_Is_Destroyed;
	private int Event_Patrol_Area;
	private int Event_No_Path_To_Target;//if you are trying to get some where and there is no path posible then stop moving and be idle
	private int Event_Target_In_Range;
	private int Event_Target_Out_Of_Range;
	private int Event_Target_Selected;
	private int Event_Disengage_Button_Press;
	private int Event_Current_Target_Change;
	private int Event_The_Door_Is_Locked;



	//private int Event_Taking_Damage;
	//Activities=========
	private int Curent_Activity_ID;
	private int Activity_Attacking_Object;
	private int Activity_Backing_Off;
	private int Activity_Patrolling;
	private int Activity_Right_Attack;
	private int Activity_Jump_Up;
	private int Activity_Jump_Down;
	private int Activity_Foot_Navigation;
	//=====================
	//Actions or movements=========
	private int Curent_Move_ID;
	private int Move_Atack_Forward;
	private int Move_Atack_Backward;
	private int Move_Left_Attack;
	private int Move_Right_Attack;
	private int Move_Jump_Up;
	private int Move_Jump_Down;
	private int Move_Celebrate;
	private int Move_Idle;
	private int Move_Crouch_And_Shoot;
	private int Move_Move_To_Target;
	private int Move_Die;
	private int Move_Deselect_Target;
	//=====================
	//to create and store all acivity ID numbers
	private int Max_Personality_ID_Name = 100;
	private int Num_Of_Personality_ID_Names;
	private string [] Personality_ID_Name;
	private int [] Personality_ID_Status;//to show if an activity event or move has started,failed,finnished(succeded)
	
	//get the enemy script for access here
	private Enemy_01_Scr Enemy_Script_IN;

	
	private Transform ATarget_Enemy;//the enemy target to engage (could be anything interactable, chose wisely)
	//[SerializeField]
	//private GameObject ExplosionFXa;
	[SerializeField]
	private GameObject Spawn_Point_Prefab;
	private GameObject Spawn_Point_Marker;
	

	private float TartgetDistanceToCenter;
	private int do_Once = 0;
	//for path way point smoother transitions
	private float DistanceToWayPoint_01;
	private int Next_Way_Point;

	//for IK tracking
	private bool ikActive = false;
	private Transform rightHandObj = null;
	private Transform right_Up_Arm_Obj = null;
	private Transform lookObj = null;
	private IK_Aim_AnimShoot_Scr IK_Arm_Aim_Obj_Script_IN;
	//==========
	
	
	//======Nav Mesh stuff
	//just to store and show path and use it for way point generation
	private NavMeshPath path;
	private float elapsed = 0.0f; 
	//================
	private GameObject ClickedObject;
	private int newClick;
	//health
	private int currentHealth = 100;
	private int currentHealthMAX = 100;


	//to colect info about existing enemies
	private GameObject [] Enemies_Spawned;
	private int NumberOfEnemies_Spawned = 0;
	private int MaxNumberOfEnemies_Spawned = 11;//11=10 cause zero not used
	private int Enemys_01_Destroyed_count = 0;
	private int [] Enemy_Destroyed_ID_Number;
	private GameObject Last_Enemy_01_Destroyed_Position;


	//for radar
	[SerializeField]
	private GameObject Radar_Point_Prefab;
	private GameObject Radar_Point_Object;
	//get the radar script
	private My_Radar_Scr My_Radar_Script_IN;
	// for health bar
	private GameObject Player_Slider_Object;
	private Slider Player_Slider_Slider;
	//for death
	private int DieOnce = 0;
	private int Halt_Processes = 0;//to stop executing this objects code
	private int Im_Dead = 0;


	//for UI target tracking
	private GameObject UI_Rotated_To_Target;

	//the UI targeting and information UI object
	private GameObject UI_Main_Canvas_Object;
	private GameObject UI_Info_Object;
	private Transform UI_Info_Object_Hide_Point;//just to hold the UI_Info_Object hide point out of view
	private Text UI_Info_Text;
	private int UI_Info_Object_Lerp_To = 0;
	//what to display and position
	private Transform UI_Info_Display_Position_IN;  
	private string UI_Info_Display_Text_IN;
	//get the interaction status display object
	private Slider Interaction_Status_Slider;

	//get the select enemy script
	private Enemy_01_Select_Scr Enemy_01_Select_Script_IN;
	//get the level manager code
	private Level_01_Manager_Scr Level_01_Manager_Script_IN;

	
	//get the InteriorSlidingDoor_Scr script
	private InteriorSlidingDoor_Scr InteriorSlidingDoor_Script_IN;
	private int Entered_Door_Space;
	private int Entered_Door_Space_React_Once = 1;

	private int Destination_Change = 0;
	private int MoveStartEnabled = 0;


	// Use this for initialization
	void Start () {
		//Get the animator
		animator = GetComponent<Animator>();
		//NavMesh, I will be using nave mesh with my AI for path finding
		path = new NavMeshPath();
		//=============

		//if(animator.layerCount >= 2)
			//animator.SetLayerWeight(1, 1);
		//spawn your own rotation smoothing ref object
		Rotated_To_Target = Instantiate (Rotated_To_Target_prefab, transform.position + transform.forward, transform.rotation) as GameObject;
		//spawn your own UI target tracking
		UI_Rotated_To_Target = Instantiate (Rotated_To_Target_prefab, transform.position + transform.forward, transform.rotation) as GameObject;



		//get the right uper arm object
		right_Up_Arm_Obj = transform.Find("Reference1").Find("Hips").Find("Spine").Find("Chest").Find("RightShoulder").Find("RightArm");
		
		//spawn your spawn point marker object
		Spawn_Point_Marker = Instantiate (Spawn_Point_Prefab, transform.position, transform.rotation) as GameObject;
		//spawn your own rotation smoothing ref object
		Rotated_To_Target = Instantiate (Rotated_To_Target_prefab, transform.position, transform.rotation) as GameObject;
		
		
		//IK spawn your spawn from prefab, for aiming at targets
		IK_Arm_Aim_Obj = Instantiate (IK_Arm_Aim_Prefab, transform.position, transform.rotation) as GameObject;
		//ik target
		IK_Aim_Target = IK_Arm_Aim_Obj.transform.Find("IK_Target_Cube");
		//get the IK_Arm_Aim_Objs code cause it has the animation for firing recoil
		IK_Arm_Aim_Obj_Script_IN = IK_Arm_Aim_Obj.transform.GetComponent<IK_Aim_AnimShoot_Scr>();
		
		
		//set max empty Max_Personality_ID slots
		Personality_ID_Name = new string[Max_Personality_ID_Name];
		Personality_ID_Status = new int[Max_Personality_ID_Name];
		//==
		Create_Personality_ID_References();


		//spawn your spawn CursorObject
		CursorObject = Instantiate (CursorObject_Prefab, transform.position, transform.rotation) as GameObject;


		
		//set max empty object slots for tracking spawned enemies
		Enemies_Spawned = new GameObject[MaxNumberOfEnemies_Spawned];
		Enemy_Destroyed_ID_Number = new int[MaxNumberOfEnemies_Spawned];


		//spawn your Radar_Point_Object
		Radar_Point_Object = Instantiate (Radar_Point_Prefab, transform.position, transform.rotation) as GameObject;
		//get the radar script
		My_Radar_Script_IN = GameObject.FindWithTag ("Radar_Camera_Tag").transform.GetComponent<My_Radar_Scr>();
		//set the radar to track you as the center object
		My_Radar_Script_IN.Set_Tracking_Target(gameObject);

		//get the health bar object
		Player_Slider_Object = GameObject.FindWithTag ("Player_Health_Slider_Tag");
		Player_Slider_Slider = Player_Slider_Object.transform.GetComponent<Slider>();
		//update the health bar
		if (Player_Slider_Slider){
			Player_Slider_Slider.value = currentHealth;
		}

		//get the UI canvas object
		UI_Main_Canvas_Object = GameObject.FindWithTag ("Main_Canvas_Tag");
		//get the UI target info panel object
		UI_Info_Object = UI_Main_Canvas_Object.transform.Find("UI_Target_Panel").gameObject;
		//get the UI Text object
		UI_Info_Text = UI_Info_Object.transform.Find("UI_Target_Image").Find("UI_Target_Text").transform.GetComponent<Text>();
		//get the UI object hide point
		UI_Info_Object_Hide_Point = UI_Main_Canvas_Object.transform.Find("UI_Target_Panel_Hide_Point_Image");

		//get the Level_01_Manager_Scr Level_01_Manager_Script_IN;
		Level_01_Manager_Script_IN = GameObject.FindWithTag ("MainCamera").transform.GetComponent<Level_01_Manager_Scr>();
		//get the Interaction_Status_Slider
		Interaction_Status_Slider = UI_Info_Object.transform.Find("Interaction_Status_Slider").transform.GetComponent<Slider>();

		
	}



	public void Pickup_Object_IN_Func (GameObject Pickup_Object_In){
		Sprite Object_image_Sprite = null;

		if (Pickup_Object_In){
			if (Pickup_Object_In.tag == "Player_Weapon_01_Tag"){
				//get the right hand object
				GameObject Right_Hand_Object = transform.Find("Reference1").Find("Hips").Find("Spine").Find("Chest").Find("RightShoulder").Find("RightArm").Find("RightForeArm").Find("RightHand").gameObject;
				//get the Player_Weapon_01 positioning point object
				GameObject Player_Weapon_01_Point = Right_Hand_Object.transform.Find("Player_Weapon_01_Point").gameObject;
				//get the weapon obect
				Weapon_Object = Pickup_Object_In;
				//rotate and position the weapon at its spawn point
				Weapon_Object.transform.position = Player_Weapon_01_Point.transform.position;
				Weapon_Object.transform.rotation = Player_Weapon_01_Point.transform.rotation;

				//attach the weapon to the right hand and to the corect position point empty GameObject point object
				Weapon_Object.transform.parent = Player_Weapon_01_Point.transform;

				//get the weapon obect code
				Weapon_Object_Script_IN = Weapon_Object.transform.GetComponent<Player_Weapon_01_Scr>();
				//get the weapon's image for display
				Object_image_Sprite = Weapon_Object_Script_IN.Object_Image_Sprite_Out_Func();
			}

			if (Pickup_Object_In.tag == "Karen_Drive_Tag"){
				//get the Karen_Drive_ code
				Karen_Drive_Scr Karen_Drive_Script_IN = Pickup_Object_In.transform.GetComponent<Karen_Drive_Scr>();
				//get the Karen_Drive_'s image for display
				Object_image_Sprite = Karen_Drive_Script_IN.Object_Image_Sprite_Out_Func();
			}

			//add this pickup contents name to the interacted_Objects_List of the Level_01_Manager
			Level_01_Manager_Script_IN.Add_interacted_Objects_List(Pickup_Object_In.tag, Object_image_Sprite);
		}

	}

	
	//function for enemies to call and anouce thier existance and give them unique ID numbers
	public int Add_Enemies_Function  (GameObject Enemy_Object_IN){
		
		if (NumberOfEnemies_Spawned < MaxNumberOfEnemies_Spawned) {
			//count waypoints spawned
			NumberOfEnemies_Spawned = NumberOfEnemies_Spawned + 1;
			
			//just give zero something
			Enemies_Spawned[0] = Enemy_Object_IN;
			//say not destroyed
			Enemy_Destroyed_ID_Number[0] = 0;
			//========
			
			//spawn a way point marker object
			Enemies_Spawned[NumberOfEnemies_Spawned] = Enemy_Object_IN;
			//say not destroyed
			Enemy_Destroyed_ID_Number[NumberOfEnemies_Spawned] = 0;
			
		}
		return NumberOfEnemies_Spawned;
	}
	
	//function to decrese enemy count
	public void Add_Enemy_01_Destroyed_count_Func  (GameObject Enemy_01_Destroyed_Position_IN){
		//count destroyed
		Enemys_01_Destroyed_count = Enemys_01_Destroyed_count + 1;
		//get position marker of death position of this enemy
		Last_Enemy_01_Destroyed_Position = Enemy_01_Destroyed_Position_IN;
	}

	//function to outtp enemy count
	public int NumberOfEnemies_Spawned_Out_Func  (){
		return NumberOfEnemies_Spawned;
	}
	//function to outtp enemy destroyed count
	public int Enemy_01_Destroyed_count_Out_Func  (){
		return Enemys_01_Destroyed_count;
	}
	//function to outtp enemy destroyed position (using a death point marker spawn object from the last enemy)
	public GameObject Last_Enemy_01_Destroyed_Position_Out_Func (){
		return Last_Enemy_01_Destroyed_Position;
	}





	//IK=Aiming==a callback for calculating IK
	void OnAnimatorIK()//void OnAnimatorIK()
	{
		if(animator) {

			
			//lookObj is the head object
			//lookObj;
			
			//if the IK is active, set the position and rotation directly to the goal. 
			if(ikActive) {

				// Set the look target position, if one has been assigned
				if(lookObj != null) {
					animator.SetLookAtWeight(1);
					animator.SetLookAtPosition(lookObj.position);
				}    
				
				// Set the right hand target position and rotation, if one has been assigned
				if(rightHandObj != null) {


					animator.SetIKPositionWeight(AvatarIKGoal.RightHand,1);
					animator.SetIKRotationWeight(AvatarIKGoal.RightHand,1);  
					animator.SetIKPosition(AvatarIKGoal.RightHand,rightHandObj.position);
					animator.SetIKRotation(AvatarIKGoal.RightHand,rightHandObj.rotation);
				}        
				
			}
			
			//if the IK is not active, set the position and rotation of the hand and head back to the original position
			else {          
				animator.SetIKPositionWeight(AvatarIKGoal.RightHand,0);
				animator.SetIKRotationWeight(AvatarIKGoal.RightHand,0); 
				animator.SetLookAtWeight(0);
			}
		}
	}
	//IK=Aiming=========================
	
	//======create Personality_ID
	private void Create_Personality_ID_References () {

		
		//Event_Taking_Damage;
		//Activities=========
		Activity_Attacking_Object = Add_Personality_ID("Activity_Attacking_Object");
		Activity_Backing_Off = Add_Personality_ID("Activity_Backing_Off");
		Activity_Patrolling = Add_Personality_ID("Activity_Patrolling");
		Activity_Right_Attack = Add_Personality_ID("Activity_Right_Attack");
		Activity_Jump_Up = Add_Personality_ID("Activity_Jump_Up");
		Activity_Jump_Down = Add_Personality_ID("Activity_Jump_Down");
		Activity_Foot_Navigation = Add_Personality_ID("Activity_Foot_Navigation");
		//=====================
		//Actions or movements=========
		Move_Atack_Forward = Add_Personality_ID("Move_Atack_Forward");
		Move_Atack_Backward = Add_Personality_ID("Move_Atack_Backward");
		Move_Left_Attack = Add_Personality_ID("Move_Left_Attack");
		Move_Right_Attack = Add_Personality_ID("Move_Right_Attack");
		Move_Jump_Up = Add_Personality_ID("Move_Jump_Up");
		Move_Jump_Down = Add_Personality_ID("Move_Jump_Down");
		Move_Celebrate = Add_Personality_ID("Move_Celebrate");
		Move_Idle = Add_Personality_ID("Move_Idle");
		Move_Crouch_And_Shoot = Add_Personality_ID("Move_Crouch_And_Shoot");
		Move_Move_To_Target = Add_Personality_ID("Move_Move_To_Target");
		Move_Die = Add_Personality_ID("Move_Die");
		Move_Deselect_Target = Add_Personality_ID("Move_Deselect_Target");
		//=====================

		//Target types
		TargetType_Enemy = Add_Personality_ID("TargetType_Enemy");
		TargetType_Interactable = Add_Personality_ID("TargetType_Interactable");
		TargetType_Destination = Add_Personality_ID("TargetType_Destination");
		//================

	}
	//===============
	
	//function to add Personality_ID name and number
	private int Add_Personality_ID (string Personality_ID_Name_IN) {
		Num_Of_Personality_ID_Names = Num_Of_Personality_ID_Names + 1;
		Personality_ID_Name[Num_Of_Personality_ID_Names] = Personality_ID_Name_IN;
		
		
		return Num_Of_Personality_ID_Names;
	}
	//==================

	//===function to change activity and or moves withing a curent activity
	private void Activity_AndOR_Move_Change_Func(int New_Activity_In, int New_Move_In){
		if(Event_Priority_Overide == 0){
		//if the new activity is diferentf from old one then this is also an activity change other wise its just a move change within the current activity
		if (Curent_Activity_ID != New_Activity_In){
			//say current Activity is finnished  = 0
			Personality_ID_Status[Curent_Activity_ID] = 0;
			//set the new Activity
			Curent_Activity_ID = New_Activity_In;
		}

		//say current move is finnished  = 0
		Personality_ID_Status[Curent_Move_ID] = 0;
		//set the new move
		Curent_Move_ID = New_Move_In;

		//clear all events raised in this activity so the dont interfear with the next activity or move
		Activity_Change_Reset_Events_Func ();
		//set overide to disable event changes below this one
		Event_Priority_Overide = 1;
		}
		//print ("mov" + Personality_ID_Name[Curent_Move_ID]);
	}
	//==================
	
	
	
	
	
	
	
	//live and perform your duties
	private void Execute_Activities () {
		
		//select the first activity upon initialization (do once only)
		if (Select_Initialization_Activity == 0){
			Select_Initialization_Activity = Activity_Foot_Navigation;
			Curent_Activity_ID = Select_Initialization_Activity;
			//Move step 1 start moving toward the CurrentTarget
			Curent_Move_ID = Move_Move_To_Target;
		}


		//Activity function for forward attack on a target (more agressive) (requires target to work)
		Activity_Attacking_Object_Func ();
		//Activity function for normal navigation on foot (exploration)
		Activity_Foot_Navigation_Func ();
		
		
	}
	//=============


	//each time there is an Activity change, all events from the current activity should be cleared
	private void Activity_Change_Reset_Events_Func () {
		 Event_Taking_Damage = 0;
		 Event_You_Are_Destroyed = 0;
		 Event_Target_Is_Destroyed = 0;
		 Event_Patrol_Area = 0;
		 Event_No_Path_To_Target = 0;//if you are trying to get some where and there is no path posible then stop moving and be idle
		 Event_Target_In_Range = 0;
		 Event_Target_Out_Of_Range = 0;
		 Event_Target_Selected = 0;
		 Event_Disengage_Button_Press = 0;
		 Event_Current_Target_Change = 0;
		 Event_The_Door_Is_Locked = 0;
	}









	
	//======Activity_Attacking_Object_Func
	private void Activity_Foot_Navigation_Func () {
		//setup Event priority and arange Event responses acording to importance
		Event_Priority_Overide = 0;


		//======Activity_Attacking_Object
		if (Curent_Activity_ID == Activity_Foot_Navigation){
			//(AAA) Start the activity (=1)
			if (Personality_ID_Status[Curent_Activity_ID] == 0){
				//say activity started (=1)
				Personality_ID_Status[Curent_Activity_ID] = 1;
			}
			







			
			//*************ULTIMATE OVERIDING EVENT=== if health is gone you die
			if (currentHealth == 0) {
				Event_You_Are_Destroyed = 1;
			}
			if(Event_You_Are_Destroyed == 1){
				Activity_AndOR_Move_Change_Func(Curent_Activity_ID, Move_Die);
			}
			//*************END ULTIMATE OVERIDING EVENT============
			

			//*************ULTIMATE OVERIDING EVENT===got to Event_Disengage_Button_Press===============
			if(Event_Disengage_Button_Press == 1){
				Activity_AndOR_Move_Change_Func(Curent_Activity_ID, Move_Deselect_Target);
			}
			//*************END ULTIMATE OVERIDING EVENT============
			
			//*************ULTIMATE OVERIDING EVENT===raise event if an enemy object has been selected then got to Activity_Attacking_Object
			if (Current_Target_Change == 1 && CurrentTargetType == TargetType_Enemy){
				Event_Target_Selected = 1;
			}
			//change activity to Activity_Attacking_Object and move to Move_Crouch_And_Shoot
			if(Event_Target_Selected == 1){
				//olny go to a target attack if you have a weapon
				if(Weapon_Object_Script_IN){
					Activity_AndOR_Move_Change_Func(Activity_Attacking_Object, Move_Crouch_And_Shoot);
				}else{//continue what you where doing

				}
			}
			//*************END ULTIMATE OVERIDING EVENT============





			
			//*************ULTIMATE OVERIDING EVENT=== raise event if you entered door space and its locked
			if (Entered_Door_Space == 1){
				if (Entered_Door_Space_React_Once == 1){
					Entered_Door_Space_React_Once = 0;
				if (InteriorSlidingDoor_Script_IN){
					//see if door is closed
					if (InteriorSlidingDoor_Script_IN.Door_LockStatus_Out_Func() == true){
						Event_The_Door_Is_Locked = 1;
					}
				}
				}
			}
			//go to Move_Deselect_Target if the door is locked
			if(Event_The_Door_Is_Locked == 1){
				Activity_AndOR_Move_Change_Func(Curent_Activity_ID, Move_Deselect_Target);
			}
			//*************END ULTIMATE OVERIDING EVENT============
			












			//(MMM) Move_Move_To_Target
			if (Curent_Move_ID == Move_Move_To_Target){
				//say the move has started (=1)
				if (Personality_ID_Status[Curent_Move_ID] == 0){
					Personality_ID_Status[Curent_Move_ID] = 1;
				}
				//Run to target if it is far and stop shooting
				if(TartgetDistanceToCenter > 4){//5
					Preset_Run_Front = 1;
					Preset_Walk_Front = 0;
					Preset_Idle_Crouched = 0;
					Preset_Shoot_Target = 0;
					Preset_Idle_Standing = 0;
					Preset_Aim_Weapon = 0;//turn of ik aiming to alow animatoin to take over arm
					Preset_TrackNavTo_Target_Path = 1;
					Preset_Face_NavTo_Target = 1;
					Preset_Face_Shooting_Target = 0;
					Preset_Deselect_Target = 0;
					Preset_Die = 0;
					
				}else{
					//walk to target if it is closer
					Preset_Walk_Front = 1;
					Preset_Run_Front = 0;
					Preset_Idle_Crouched = 0;
					Preset_Shoot_Target = 0;
					Preset_Idle_Standing = 0;
					Preset_Aim_Weapon = 0;
					Preset_TrackNavTo_Target_Path = 1;
					Preset_Face_NavTo_Target = 1;
					Preset_Face_Shooting_Target = 0;
					Preset_Deselect_Target = 0;
					Preset_Die = 0;

					//if target is reached then stop and go to idle standing
					if(TartgetDistanceToCenter <= 0.6){//&& CurrentTargetType == TargetType_Destination
						Event_Target_In_Range = 1;
					}
				}


				//EEEEEVVVV respond to events (to posibly change activity or move
				
				//if target is reached then go to Move_Idle
				if(Event_Target_In_Range == 1){
					Activity_AndOR_Move_Change_Func(Curent_Activity_ID, Move_Idle);
				}

				//If there is no path and you where moving then (path disapeared or) then stop moving and go to idle or patrol
				if(Event_No_Path_To_Target == 1){
					Activity_AndOR_Move_Change_Func(Curent_Activity_ID, Move_Idle);
				}

				//if target is destroyed then leave this movement
				if(Event_Target_Is_Destroyed == 1){
					Activity_AndOR_Move_Change_Func(Curent_Activity_ID, Move_Celebrate);
				}
				
				//EEEEEVVVV End respond to events (to posibly change activity or move
			}
			//==(MMM) End Move_Move_To_Target

			
			
			//(MMM) Move_Idle
			if (Curent_Move_ID == Move_Idle){
				//say the move has started (=1)
				if (Personality_ID_Status[Curent_Move_ID] == 0){
					Personality_ID_Status[Curent_Move_ID] = 1;
				}
				//go to idle standing position
				Preset_Run_Front = 0;
				Preset_Walk_Front = 0;
				Preset_Idle_Crouched = 0;
				Preset_Shoot_Target = 0;
				Preset_Idle_Standing = 1;
				Preset_Aim_Weapon = 0;//turn of ik aiming
				Preset_TrackNavTo_Target_Path = 1;//turn on path search to see if you can find the target
				Preset_Face_NavTo_Target = 1;
				Preset_Face_Shooting_Target = 0;
				Preset_Deselect_Target = 0;
				Preset_Die = 0;
				//============

				//raise Event_Target_Out_Of_Range if the target point moves 
				if(TartgetDistanceToCenter > 1.2 ){
					Event_Target_Out_Of_Range = 1;
				}

				//EEEEEVVVV respond to events (to posibly change activity or move

				//If the target is out of range the go back to Move_Move_To_Target
				if(Event_Target_Out_Of_Range == 1){
					Activity_AndOR_Move_Change_Func(Curent_Activity_ID, Move_Move_To_Target);
				}
				//if target is destroyed then Move_Celebrate
				if(Event_Target_Is_Destroyed == 1){
					Activity_AndOR_Move_Change_Func(Curent_Activity_ID, Move_Celebrate);
				}
				
				
				//EEEEEVVVV End respond to events (to posibly change activity or move
			}
			//(MMM) End Move_Idle


		
				
				//(MMM) Move_Deselect_Target
			if (Curent_Move_ID == Move_Deselect_Target){
				//say the move has started (=1)
				if (Personality_ID_Status[Curent_Move_ID] == 0){
					Personality_ID_Status[Curent_Move_ID] = 1;
				}
				//go to idle standing position
				Preset_Run_Front = 0;
				Preset_Walk_Front = 0;
				Preset_Idle_Crouched = 0;
				Preset_Shoot_Target = 0;
				Preset_Idle_Standing = 1;
				Preset_Aim_Weapon = 0;
				Preset_TrackNavTo_Target_Path = 0;
				Preset_Face_NavTo_Target = 0;
				Preset_Face_Shooting_Target = 0;
				Preset_Deselect_Target = 1;
				Preset_Die = 0;
				//============
				
				//raise Event_Target_In_Range automaticaly then go to Move_Idle
				Event_Target_In_Range = 1;
				
				//EEEEEVVVV respond to events (to posibly change activity or move
				
				//Move_Idle
				if(Event_Target_In_Range == 1){
					Activity_AndOR_Move_Change_Func(Curent_Activity_ID, Move_Idle);
				}
				//if target is destroyed then Move_Celebrate
				if(Event_Target_Is_Destroyed == 1){
					Activity_AndOR_Move_Change_Func(Curent_Activity_ID, Move_Celebrate);
				}
				
				
				//EEEEEVVVV End respond to events (to posibly change activity or move
			}
			//(MMM) End Move_Idle

			
			
			
						
			//(MMM) Move_Die
			if (Curent_Move_ID == Move_Die){
				//say the move has started (=1)
				if (Personality_ID_Status[Curent_Move_ID] == 0){
					Personality_ID_Status[Curent_Move_ID] = 1;
				}
				//go to idle standing position
				Preset_Run_Front = 0;
				Preset_Walk_Front = 0;
				Preset_Idle_Crouched = 0;
				Preset_Shoot_Target = 0;
				Preset_Idle_Standing = 0;
				Preset_Aim_Weapon = 0;
				Preset_TrackNavTo_Target_Path = 0;
				Preset_Face_NavTo_Target = 0;
				Preset_Face_Shooting_Target = 0;
				Preset_Deselect_Target = 0;
				Preset_Die = 1;
				//============
				//EEEEEVVVV respond to events (to posibly change activity or move
				
				//EEEEEVVVV End respond to events (to posibly change activity or move
			}
			//(MMM) End Move_Die
			
			
			
			//activity successful
			//Personality_ID_Status(Curent_Activity_ID) = 2;
		}
		//======end Activity_Attacking_Object
		
		
	}
	//======end Activity_Attacking_Object_Func





	//======Activity_Attacking_Object_Func
	private void Activity_Attacking_Object_Func () {
		//setup Event priority and arange Event responses acording to importance
		Event_Priority_Overide = 0;

		//======Activity_Attacking_Object
		if (Curent_Activity_ID == Activity_Attacking_Object){
			//(AAA) Start the activity (=1)
			if (Personality_ID_Status[Curent_Activity_ID] == 0){
				//say activity started (=1)
				Personality_ID_Status[Curent_Activity_ID] = 1;
			}




			
			//*************ULTIMATE OVERIDING EVENT=== if health is gone you die
			if (currentHealth == 0) {
				Event_You_Are_Destroyed = 1;
			}
			if(Event_You_Are_Destroyed == 1){
				Activity_AndOR_Move_Change_Func(Curent_Activity_ID, Move_Die);
			}
			//*************END ULTIMATE OVERIDING EVENT============
			

			//*************ULTIMATE OVERIDING EVENT===got to Event_Disengage_Button_Press===============
			if(Event_Disengage_Button_Press == 1){
				Activity_AndOR_Move_Change_Func(Activity_Foot_Navigation, Move_Deselect_Target);
			}
			//*************END ULTIMATE OVERIDING EVENT============

			//*************ULTIMATE OVERIDING EVENT===got to Move_Crouch_And_Shoot if enemy target is clicked again (could happen)===============
			if (Current_Target_Change == 1 && CurrentTargetType == TargetType_Enemy){
				Event_Current_Target_Change = 1;
			}
			if(Event_Current_Target_Change == 1){
				Activity_AndOR_Move_Change_Func(Curent_Activity_ID, Move_Crouch_And_Shoot);
			}
			//*************END ULTIMATE OVERIDING EVENT============



			//*************ULTIMATE OVERIDING EVENT===if interactable object selecten, go to navigation mode to move to the interactable object (like loot chest, main computer etc)
			if (CurrentTargetType == TargetType_Interactable){
				Event_Current_Target_Change = 1;
			}
			if(Event_Current_Target_Change == 1){
				Activity_AndOR_Move_Change_Func(Activity_Foot_Navigation, Move_Move_To_Target);
			}
			//*************END ULTIMATE OVERIDING EVENT============







			//you cant run while in this mode, you have to disengage and go to Nav mode to run then reengage 
			//als your cursor movments will be direct to target not using nav mesh
			
			//(MMM) Move_Atack_Forward
			if (Curent_Move_ID == Move_Atack_Forward){
				//say the move has started (=1)
				if (Personality_ID_Status[Curent_Move_ID] == 0){
					Personality_ID_Status[Curent_Move_ID] = 1;
				}

				//Run to target if it is far and stop shooting
				if(TartgetDistanceToCenter > 2){//5
					Preset_Run_Front = 1;
					Preset_Walk_Front = 0;
					Preset_Idle_Crouched = 0;
					Preset_Shoot_Target = 0;
					Preset_Idle_Standing = 0;
					Preset_Aim_Weapon = 0;//turn of ik aiming to alow animatoin to take over arm
					Preset_TrackNavTo_Target_Path = 1;
					Preset_Face_NavTo_Target = 1;
					Preset_Face_Shooting_Target = 0;
					Preset_Deselect_Target = 0;
					Preset_Die = 0;
					
				}else{
					//walk to target if it is closer
					Preset_Walk_Front = 1;
					Preset_Run_Front = 0;
					Preset_Idle_Crouched = 0;
					Preset_Shoot_Target = 0;
					Preset_Idle_Standing = 0;
					Preset_Aim_Weapon = 0;
					Preset_TrackNavTo_Target_Path = 1;
					Preset_Face_NavTo_Target = 1;
					Preset_Face_Shooting_Target = 0;
					Preset_Deselect_Target = 0;
					Preset_Die = 0;

					//if target is reached then stop and go to idle standing
					if(TartgetDistanceToCenter <= 0.6){
						Event_Target_In_Range = 1;
					}
				}

				
				
				
				//EEEEEVVVV respond to events (to posibly change activity or move
				
				if(Event_Target_Is_Destroyed == 1){//if target is destroyed then leave this movement
					Activity_AndOR_Move_Change_Func(Curent_Activity_ID, Move_Celebrate);
				}

				//if target is in shooting range and there clear line of sight then go to Move_Crouch_And_Shoot
				if(Event_Target_In_Range == 1){
					Activity_AndOR_Move_Change_Func(Curent_Activity_ID, Move_Crouch_And_Shoot);
				}
			
				//If there is no path and you where moving then (path disapeared or) then stop moving and go to idle or patrol
				if(Event_No_Path_To_Target == 1){
					Activity_AndOR_Move_Change_Func(Activity_Foot_Navigation, Move_Idle);
				}
				//EEEEEVVVV End respond to events (to posibly change activity or move
			}
			//==(MMM) End Move_Atack_Forward
			
			
			
			
			
			
			
			
			//(MMM) Move_Crouch_And_Shoot
			if (Curent_Move_ID == Move_Crouch_And_Shoot){
				//say the move has started (=1)
				if (Personality_ID_Status[Curent_Move_ID] == 0){
					Personality_ID_Status[Curent_Move_ID] = 1;
				}

					//if target is reached and thier is clear line of sight then stop and get into idle combate crouch mode and shoot them
					Preset_Walk_Front = 0;
					Preset_Run_Front = 0;
					Preset_Idle_Crouched = 1;
					Preset_Shoot_Target = 1;
					Preset_Idle_Standing = 0;
					Preset_Aim_Weapon = 1;//keep aiming weapon at target while shooting (up down aim)
					Preset_TrackNavTo_Target_Path = 0;//disable navigation tracking to allow direct target look at and facing for shooting
					Preset_Face_NavTo_Target = 0;//face shooting target 
					Preset_Face_Shooting_Target = 1;
					Preset_Deselect_Target = 0;
					Preset_Die = 0;
				

				//raise the out of range if a new destination is selected (to move around while you fir
				if(TartgetDistanceToCenter > 0.3 && CurrentTargetType == TargetType_Destination){
					Event_Target_Out_Of_Range = 1;
				}

				
				
				//EEEEEVVVV respond to events (to posibly change activity or move
				//raise event enemy is destroyed
				if (Enemy_Script_IN == null){//!ATarget_Enemy){
					Event_Target_Is_Destroyed = 1;
				}
				//If the target is destroyed then to to newtral idle (to stop shooting)
				if(Event_Target_Is_Destroyed == 1){
					Activity_AndOR_Move_Change_Func(Activity_Foot_Navigation, Move_Deselect_Target);
				}

				//If the target has moved out of range then go back to Move_Atack_Forward to track them
				if(Event_Target_Out_Of_Range == 1){
					Activity_AndOR_Move_Change_Func(Curent_Activity_ID, Move_Atack_Forward);
				}


				
				//EEEEEVVVV End respond to events (to posibly change activity or move
			}
			//(MMM) End Move_Crouch_And_Shoot
			
			
			
			//(MMM) Move_Die
			if (Curent_Move_ID == Move_Die){
				//say the move has started (=1)
				if (Personality_ID_Status[Curent_Move_ID] == 0){
					Personality_ID_Status[Curent_Move_ID] = 1;
				}
				//go to idle standing position
				Preset_Run_Front = 0;
				Preset_Walk_Front = 0;
				Preset_Idle_Crouched = 0;
				Preset_Shoot_Target = 0;
				Preset_Idle_Standing = 0;
				Preset_Aim_Weapon = 0;
				Preset_TrackNavTo_Target_Path = 0;
				Preset_Face_NavTo_Target = 0;
				Preset_Face_Shooting_Target = 0;
				Preset_Deselect_Target = 0;
				Preset_Die = 1;
				//============
				//EEEEEVVVV respond to events (to posibly change activity or move
				
				//EEEEEVVVV End respond to events (to posibly change activity or move
			}
			//(MMM) End Move_Die

			
			//activity successful
			//Personality_ID_Status(Curent_Activity_ID) = 2;
		}
		//======end Activity_Attacking_Object
		

	}
	//======end Activity_Attacking_Object_Func
	
	//function to stop player from moving at boundaries by placing the way point on the players position
	public void Stop_player_Movement (){
		//stop on a dime
		GetComponent<Rigidbody>().velocity = Vector3.zero;
		//update cursor object position
		CursorObject.transform.position = transform.position;
		//Give the current target a type description
		CurrentTargetType = TargetType_Destination;
		CurrentTarget = CursorObject;
		//just give name
		CurrentTarget_Name_ID = "cursorobject";

	}


	//===========
	public void Disengage_Enemy_Target_Func () {
		//raise the Event_Disengage_Button_Press
		Event_Disengage_Button_Press = 1;
	}
	//=============





	
	private int Clear_Line_Of_Sight_Func () {
		int Clear_Path_to_Target = 0;
		
		
		//====================================
		// Bit shift the index of the layer (9) to get a bit mask
		int layerMask = 1 << 9;
		
		// This would cast rays only against colliders in layer 8.
		// But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
		layerMask = ~layerMask;
		
		RaycastHit hit;
		if (right_Up_Arm_Obj && CurrentTarget){
			
			//send ray cast to target an see if there is no objects between you and them (clear line of sight)
			if (Physics.Raycast(IK_Arm_Aim_Obj.transform.position, IK_Arm_Aim_Obj.transform.TransformDirection (Vector3.forward) * 100, out hit, Mathf.Infinity, layerMask)) {
				//Debug.DrawRay(IK_Arm_Aim_Obj.transform.position, IK_Arm_Aim_Obj.transform.TransformDirection (Vector3.forward) * hit.distance, Color.yellow);
				//now see if this object is the target in we want
				if (hit.collider.gameObject.tag == "Enemy_01_Tag"){
					Clear_Path_to_Target = 1;
				}
			}
		}
		//===================================
		
		return Clear_Path_to_Target;
	}
	


	
	//=====for touch input to track the curso position
	void Touch_input_Func(){
		//============================
		//check number of input points
		int touch_points_count = Input.touchCount;
		int i;
		Touch touchOne;
		
		RaycastHit hit;
		Ray ray;
		//get the touch position vector
		Vector3 touchPoint1;
		
		//rotate the cursor
		//CursorObject.transform.Rotate(0, 50 * Time.deltaTime, 0);
		
		
		//use one finger
		if (touch_points_count == 1) {	//get user input
			touchOne = Input.GetTouch (0);
			
			//get the start point
			
			//to colect gesture points
			if(touchOne.phase == TouchPhase.Began){//
				
				
				
				//============new add
				// Bit shift the index of the layer (5) to get a bit mask
				int layerMask = 1 << 5;// This would cast rays only against colliders in layer 5.
				// But instead we want to collide against everything except layer 5. The ~ operator does this, it inverts a bitmask.
				layerMask = ~layerMask;
				//============new add
				
				
				
				//====raycast from cam eye view point to world plane point (project down and use the hit point as world point)============
				ray = Camera.main.ScreenPointToRay(touchOne.position);//Input.mousePosition);
				if(Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))//Physics.Raycast(ray, out hit)
				{
					

					//to keep the curso point a fixed y position
					//Vector3 hitPointFixed = hit.point;
					//hitPointFixed.y = transform.position.y;
					
					//update cursor object position
					//CursorObject.transform.position = hitPointFixed;//hit.point;
					
					
					//get the touch position vector in unity world not mobile screen
					//touchPoint1 = CursorObject.transform.position;


					
					//========get the object we are pointing to with racast
					ClickedObject = hit.collider.gameObject;
					
					
					
					//dont move cursor object if we clicked on a button (UI layer 11)
					if(ClickedObject.layer != 11){
						Destination_Change = 1;
						if(ClickedObject.tag == "Enemy_01_Select_Tag"){
							
							//get the enemy select objects script
							Enemy_01_Select_Script_IN = ClickedObject.transform.GetComponent<Enemy_01_Select_Scr>();
							//get the parent object of it (the enemy object itself)
							CurrentTarget = Enemy_01_Select_Script_IN.Parent_Object_Out_Func();
							//get the enemy script
							Enemy_Script_IN = CurrentTarget.transform.GetComponent<Enemy_01_Scr>();
							
							
							//get the proper player target position (not their feet) (to shoot at)
							player_target_chest_point = CurrentTarget.transform.Find("Hip").Find("Torso1").Find("Chest1");
							//get the players head position (to look at)
							player_target_head_point = CurrentTarget.transform.Find("Hip").Find("Torso1").Find("Chest1").Find("Head1");
							//set the tartget = player
							//CurrentTarget = player_target_chest_point;
							//set the tartget to shoot at player
							ATarget_Enemy = player_target_chest_point;
							//==================================
							
							//allow UI_Info_Object to lerp to target (smoother)
							UI_Info_Object_Lerp_To = 1;
							//get the position to display
							UI_Info_Display_Position_IN = ATarget_Enemy; 
							//get the info to display
							UI_Info_Display_Text_IN = Level_01_Manager_Script_IN.Clicked_Object_In_Func(CurrentTarget);;
							
							
							//Give the current target a type description
							CurrentTargetType = TargetType_Enemy;
							//get the current target name ID from its code
							CurrentTarget_Name_ID = UI_Info_Display_Text_IN;
							
						}
						
						
						//if its ground tag then its a destination target
						if(ClickedObject.tag == "Ground_Tag"){
							//update cursor object position
							CursorObject.transform.position = hit.point;
							//Give the current target a type description
							CurrentTargetType = TargetType_Destination;
							//get the current target name ID from code (ground has no code so just give name)
							CurrentTarget_Name_ID = "Ground";
							CurrentTarget = CursorObject;
						}
						
						
						
						
						
						
						//if its loot chest tag or the main computer then its a destination target
						if(ClickedObject.tag == "Loot_Chest_01_Tag" || ClickedObject.tag == "Main_Computer_Tag" || ClickedObject.tag == "Lab_02_Computer_Tag" || ClickedObject.tag == "Karen_Drive_Tag" || ClickedObject.tag == "Cargo_Crate_Tag"){
							
							//get the interaction position of the clicked object
							Transform Interaction_Position = ClickedObject.transform.Find("Interaction_Point_Sphere").transform;
							//update cursor object position
							CursorObject.transform.position = Interaction_Position.position;// hit.point;
							
							
							
							//==========----==allow UI_Info_Object to lerp to target (smoother)
							UI_Info_Object_Lerp_To = 1;
							//get the position to display
							UI_Info_Display_Position_IN = ClickedObject.transform; 
							//give the level manager the lootchest object clicked and get its display name
							UI_Info_Display_Text_IN = Level_01_Manager_Script_IN.Clicked_Object_In_Func(ClickedObject);
							//==========----================================================================
							
							//Give the current target a type description
							CurrentTargetType = TargetType_Interactable;
							//get the current target name ID from code (ground has no code so just give name)
							CurrentTarget_Name_ID = UI_Info_Display_Text_IN;
							CurrentTarget = CursorObject;
							
							
						}
						
						
						
						
						
						
					}//if(ClickedObject.layer != 11){
					//======================================



					
					
				}
			}
			
			
			
			
			
			
			
		}
		
	}
	//===============
	
	
	//if using mouse instead of touch
	void Mouse_input_Func(){
		//raycast general vars
		RaycastHit hit;
		Ray ray;








		//============new add
		// Bit shift the index of the layer (5) to get a bit mask
		int layerMask = 1 << 5;// This would cast rays only against colliders in layer 5.
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
					Destination_Change = 1;
					if(ClickedObject.tag == "Enemy_01_Select_Tag"){
						
						//get the enemy select objects script
						Enemy_01_Select_Script_IN = ClickedObject.transform.GetComponent<Enemy_01_Select_Scr>();
						//get the parent object of it (the enemy object itself)
						CurrentTarget = Enemy_01_Select_Script_IN.Parent_Object_Out_Func();
						//get the enemy script
						Enemy_Script_IN = CurrentTarget.transform.GetComponent<Enemy_01_Scr>();
						
						
						//get the proper player target position (not their feet) (to shoot at)
						player_target_chest_point = CurrentTarget.transform.Find("Hip").Find("Torso1").Find("Chest1");
						//get the players head position (to look at)
						player_target_head_point = CurrentTarget.transform.Find("Hip").Find("Torso1").Find("Chest1").Find("Head1");
						//set the tartget = player
						//CurrentTarget = player_target_chest_point;
						//set the tartget to shoot at player
						ATarget_Enemy = player_target_chest_point;
						//==================================
						
						//allow UI_Info_Object to lerp to target (smoother)
						UI_Info_Object_Lerp_To = 1;
						//get the position to display
						UI_Info_Display_Position_IN = ATarget_Enemy; 
						//get the info to display
						UI_Info_Display_Text_IN = Level_01_Manager_Script_IN.Clicked_Object_In_Func(CurrentTarget);;

						
						//Give the current target a type description
						CurrentTargetType = TargetType_Enemy;
						//get the current target name ID from its code
						CurrentTarget_Name_ID = UI_Info_Display_Text_IN;
						
					}
						
						
					//if its ground tag then its a destination target
					if(ClickedObject.tag == "Ground_Tag"){
						//update cursor object position
						CursorObject.transform.position = hit.point;
						//Give the current target a type description
						CurrentTargetType = TargetType_Destination;
						//get the current target name ID from code (ground has no code so just give name)
						CurrentTarget_Name_ID = "Ground";
						CurrentTarget = CursorObject;
					}
						




						
					//if its loot chest tag or the main computer then its a destination target
					if(ClickedObject.tag == "Loot_Chest_01_Tag" || ClickedObject.tag == "Main_Computer_Tag" || ClickedObject.tag == "Lab_02_Computer_Tag" || ClickedObject.tag == "Karen_Drive_Tag" || ClickedObject.tag == "Cargo_Crate_Tag"){

						//get the interaction position of the clicked object
						Transform Interaction_Position = ClickedObject.transform.Find("Interaction_Point_Sphere").transform;
						//update cursor object position
						CursorObject.transform.position = Interaction_Position.position;// hit.point;


						
						//==========----==allow UI_Info_Object to lerp to target (smoother)
						UI_Info_Object_Lerp_To = 1;
						//get the position to display
						UI_Info_Display_Position_IN = ClickedObject.transform; 
						//give the level manager the lootchest object clicked and get its display name
						UI_Info_Display_Text_IN = Level_01_Manager_Script_IN.Clicked_Object_In_Func(ClickedObject);
						//==========----================================================================

						//Give the current target a type description
						CurrentTargetType = TargetType_Interactable;
						//get the current target name ID from code (ground has no code so just give name)
						CurrentTarget_Name_ID = UI_Info_Display_Text_IN;
						CurrentTarget = CursorObject;
						

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





	private void Place_UI_Target_Func (Vector3 UI_Target_Position_IN, string UI_Target_Name_Info_IN, int Interaction_Slider_Value_IN) {


		//====================================
		// Bit shift the index of the layer (5) to get a bit mask
		int layerMask = 1 << 5;// This would cast rays only against colliders in layer 5.

		// But instead we want to collide against everything except layer 5. The ~ operator does this, it inverts a bitmask.
		//layerMask = ~layerMask;

		RaycastHit hit;
			
		//place tracking object at cam eye view point
		//UI_Rotated_To_Target.transform.position = Camera.main.transform.position;
		//rotate it to look in direction of target
		//UI_Rotated_To_Target.transform.LookAt (UI_Target_Position_IN);//(CurrentTarget.transform.position);//

		//place tracking object at cam eye view point
		UI_Rotated_To_Target.transform.position = Camera.main.transform.position;
		//rotate it to look in direction of target
		UI_Rotated_To_Target.transform.LookAt (UI_Target_Position_IN);//(CurrentTarget.transform.position);//



		//====raycast from cam eye view point to world plane point (project down and use the hit point as world point)============
		if (Physics.Raycast(UI_Rotated_To_Target.transform.position, UI_Rotated_To_Target.transform.TransformDirection (Vector3.forward) * 50, out hit, Mathf.Infinity, layerMask)) {

			//draw
			Debug.DrawRay(UI_Rotated_To_Target.transform.position, UI_Rotated_To_Target.transform.TransformDirection (Vector3.forward) * hit.distance, Color.yellow);
			


			// learp the movement to target position
			if (UI_Info_Object_Lerp_To == 1){
				Vector3 poss = UI_Info_Object.transform.position;
				float DampingSpeed = 12;
				poss.x = Mathf.Lerp (poss.x, hit.point.x, DampingSpeed * Time.deltaTime);
				poss.y = Mathf.Lerp (poss.y, hit.point.y, DampingSpeed * Time.deltaTime);
				poss.z = Mathf.Lerp (poss.z, hit.point.z, DampingSpeed * Time.deltaTime);

				UI_Info_Object.transform.position = poss;
				//stop the lerp if the target position is reached
				if (Vector3.Distance(poss,hit.point) < 0.1f){
					UI_Info_Object_Lerp_To = 0;
				}

			}else{
				//just place the UI object on the target position directly
				UI_Info_Object.transform.position = hit.point;

			}
				//show the info of the target
				UI_Info_Text.text = UI_Target_Name_Info_IN;



		}
		//===================================


		//if this object has a status slider then show its interaction status
		if (Interaction_Slider_Value_IN > 0){
			Interaction_Status_Slider.gameObject.SetActive (true);
			Interaction_Status_Slider.value = Interaction_Slider_Value_IN;
		}else{//hide the Interaction_Status_Slider
			Interaction_Status_Slider.gameObject.SetActive (false);
		}




	
	}
	
	
	private void Place_UI_Target_INWorld_Func (Vector3 UI_Target_Position_IN) {

		//place tracking object at target point
		UI_Rotated_To_Target.transform.position = UI_Target_Position_IN;
		//rotate it to look in direction of cam
		UI_Rotated_To_Target.transform.LookAt (Camera.main.transform.position);//(CurrentTarget.transform.position);//



		//distance from target
		float TartgetDistanceFrom = Vector3.Distance(Camera.main.transform.position,UI_Target_Position_IN);
		if(TartgetDistanceFrom <= 0){TartgetDistanceFrom = 0.1f;}
		//get half the distance between target and projectile origin
		float distance = TartgetDistanceFrom / 8;// / 8 to keep the UI object closer to the target to avoid shacking when player moves


		
		Vector3 pos;
		pos = UI_Rotated_To_Target.transform.position;
		//place the half way point object between target and projectile origin
		//pos += transform.rotation * Vector3.forward * distance;
		//pos += UI_Rotated_To_Target.transform.rotation * Vector3.forward * distance;
		pos += UI_Rotated_To_Target.transform.TransformDirection (Vector3.forward) * distance;

		//======place the UI object on the target
		UI_Info_Object.transform.rotation = Camera.main.transform.rotation;
		UI_Info_Object.transform.position = pos;
		//===================================






	}


	void FixedUpdate ()
	{
		if (MoveStartEnabled == 1)
		{
			//Now rotation and movement are independent of each other (they can do thier own thing or be synced
			Vector3 Target_To_Look_At = Vector3.zero;
			//Look in the of the target you are shooting at all times while in Activity_Attacking_Object mode
			if (Preset_Face_Shooting_Target == 1 && ATarget_Enemy){
				Target_To_Look_At = ATarget_Enemy.position;
			}
			//look in the direction of movement
			if(Preset_Face_NavTo_Target == 1){
				Target_To_Look_At = Nav_To_CurrentTarget;
			}

			//----============new smoot rotation Look at and dampen the rotation
			if (TartgetDistanceToCenter > 0.01f){
				//move to this current position
				Vector3 pos = transform.position;
				//pos.x = transform.position.x;
				//pos.y = transform.position.y;// CurrentTarget.transform.position.y;//
				//pos.z = transform.position.z;
				//always put target and tracker on the same level always
				Target_To_Look_At.y = transform.position.y;

				Rotated_To_Target.transform.position = pos;
				//rotate snap to curent target
				Rotated_To_Target.transform.LookAt (Target_To_Look_At);//(CurrentTarget.transform.position);//
				//smoothly rotate this object to current target rotation tracking
				float Rotationdamping = 6.2f;//6f;//
				Quaternion rotation = Rotated_To_Target.transform.rotation;// Quaternion.LookRotation(target.position - transform.position);
				transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * Rotationdamping);
			}
			//-----===================







			//New way to move rigid body in any direction regardless (independent) of rotation (this allows side stepping *****And It Auto Stops When targets exact point is reached)
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
	}//FixedUpdate


	// Update is called once per frame
	void Update ()
	{//FixedUpdate () {

		Halt_Processes = Level_01_Manager_Script_IN.Halt_Status_OUT_Func();
		
		//==========Halt_Processes
		if (Halt_Processes == 0){

		//to use mouse or touch
		Mouse_input_Func ();
		//Touch_input_Func ();
		

		//=======new AI 


		
		
		//if there is no current target then set it to the cursor object
		if (CurrentTarget == null) {
			//Give the current target a type description
			CurrentTargetType = TargetType_Destination;
			CurrentTarget = CursorObject;
			//just give name
			CurrentTarget_Name_ID = "cursorobject";
		}
		//===========
		

		
		
		if (CurrentTarget) {


				//====detect target selection changes by detecting target name id changes
				Current_Target_Change = 0;
				if (CurrentTarget_Name_ID != CurrentTarget_Name_ID_OLD) {
					Current_Target_Change = 1;
				}
				CurrentTarget_Name_ID_OLD = CurrentTarget_Name_ID;
				//===========================================================




			//place ui object on the enemy
				if (UI_Info_Display_Text_IN != "" && UI_Info_Display_Position_IN && Level_01_Manager_Script_IN){//ATarget_Enemy
					//Place_UI_Target_Func(ATarget_Enemy.position, Enemy_Script_IN.ID_Name_Out_Func());
					Place_UI_Target_Func(UI_Info_Display_Position_IN.position, UI_Info_Display_Text_IN,Level_01_Manager_Script_IN.Clicked_Object_Status_Out_Func());
				}else{
					//if there is no target for the ui then move it out of view
					UI_Info_Object.transform.position = UI_Info_Object_Hide_Point.position;
					//show null
					UI_Info_Text.text = "";
				}




			//==========Move your radar point object (to show up on the radar)
			if (Radar_Point_Object) {
				Radar_Point_Object.transform.position = transform.position;
			}
			//==================




			//==================use alot by AI
			TartgetDistanceToCenter = Vector3.Distance(CurrentTarget.transform.position,transform.position);
			//AI activities
			Execute_Activities ();
			
			
			//==Nav mesh Calculate and Update the path to the target every second.
			if (Preset_TrackNavTo_Target_Path == 1 && TartgetDistanceToCenter > 1.2){
				Transform target = CurrentTarget.transform;
				//elapsed += Time.deltaTime;//every second
				//do this every other frame
				//elapsed = elapsed + 1;
					if (Destination_Change == 1) {//elapsed > 30 || 
					elapsed = 0;
					Destination_Change = 0;

					//clear path status result
					Path_Status_Out = 0;
					NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, path);//from unity 5

					//if the path terminates at the destination. (its complete and reachable)
					if (path.status == NavMeshPathStatus.PathComplete) {
						if (path.corners.Length > 0){
							Path_Status_Out = 1;
							//reset way point to first way point after each successful calculation
							Next_Way_Point = 1;
							//just to show the path
							for (int i = 0; i < path.corners.Length-1; i++){
								Debug.DrawLine(path.corners[i], path.corners[i+1], Color.red);	
							}
						}

					}
				}
				
			}else{
				Path_Status_Out = 2;//= 2 when nav is not in use cause it will remain 1 or 0 if not set here
			}
			

			//=====if a way point is found then use it to get to the target
			if (Path_Status_Out == 1 && path.corners.Length > 0){//&& path.corners.Length > 0

				//if theres more than 2 way points then see if you are getting close to way point [1] then see if you start moving away from it then switch to 2 and so on
				if (path.corners.Length > 1){
					if (Next_Way_Point > path.corners.Length-1){
						Next_Way_Point = path.corners.Length-1;
					}
					DistanceToWayPoint_01 = Vector3.Distance(path.corners[Next_Way_Point],transform.position);
					Debug.DrawLine(path.corners[Next_Way_Point],transform.position, Color.green);
					//move to next waypoint if reached or close enough to the current one
				if (DistanceToWayPoint_01 < 0.5){
						Next_Way_Point = Next_Way_Point + 1;
						if (Next_Way_Point > path.corners.Length-1){
							Next_Way_Point = path.corners.Length-1;
						}
					}
				}
				//================
				Nav_To_CurrentTarget = path.corners[Next_Way_Point];//use [1] cause [0] is your own position
			}else{// if there is no posible path then set the way point to the target itself
				Nav_To_CurrentTarget = CurrentTarget.transform.position;
			}
			//=======End Navmesh=====================
			
			
	


			
			
			
			//dont move unless the waypoint is at leas a decent distance away (to avoid snapy turns back ward)
			//float waypoint_Distance = Vector3.Distance(Nav_To_CurrentTarget,transform.position);
			
			
			//Preset_Run_Front
			if (Preset_Run_Front == 1){//INPUT
				throttle = 1;
				MoveSpeed = 1800;//1200
				if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Run_Tag"))// .IsName("Run")
				{
					MoveSpeed = 5400;// 2200;//2200
				}
				//play the Run animation
				animator.SetInteger("Activity_Select", 1);
				animator.SetFloat("Run_Tilt_Blend", 0);
			}
			//======================
			
			//Preset_Walk_Front
			if (Preset_Walk_Front == 1){//INPUT
				throttle = 1;
				MoveSpeed = 900;//2800
				if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Walk_Tag"))// .IsName("Run")
				{
					MoveSpeed = 1200;//900;//2800
				}
				//play the walk animation
				animator.SetInteger("Activity_Select", 2);
			}
			//======================
			

			
			//Preset_Idle_Standing
			if (Preset_Idle_Standing == 1){//INPUT
				//play the idle animation
				animator.SetInteger("Activity_Select", 0);
				MoveSpeed = 0;
				throttle = 0;
			}
			//=======================
			
			//default stop movements when there are no movements
			if (Preset_Walk_Front == 0 && Preset_Run_Front == 0){//INPUT
				MoveSpeed = 0;
				throttle = 0;
			}

			//Preset_Deselect_Target
			if (Preset_Deselect_Target == 1){//INPUT
				Stop_player_Movement();
			}

			
			//Raise event if you are trying to get some where and there is no path posible then raise an event for the curent activity to respond to or not
			if (Path_Status_Out == 0){//INPUT
				//raise an event for the curent activity to respond to or not
				Event_No_Path_To_Target = 1;
			}else{
				Event_No_Path_To_Target = 0;
			}
			
			
			
			
			//=======================
			
			
			
			
			//there are two targets (one to track and chase and other is the one to shoot at) Could be the same
			if (Preset_Shoot_Target == 1){
				if (Weapon_Object_Script_IN){
				//===IK hand aiming, move the aim reference object to the uper arm position
					if (ATarget_Enemy){
						Vector3 IKpos = right_Up_Arm_Obj.position;
						IK_Arm_Aim_Obj.transform.position = IKpos;
						//now make this object look at the target shoot point
						IK_Arm_Aim_Obj.transform.LookAt (ATarget_Enemy.position);//(CurrentTarget.transform.position);
						//rightHandObj is the target where the right hand will track using ik code
						rightHandObj = IK_Aim_Target;
				//===End IK hand aiming


					//give the weapon the target
					Weapon_Object_Script_IN.Set_Fire_Target(ATarget_Enemy);
					//start firing the weapon
					Weapon_Object_Script_IN.Fire_Func();
				
					//play weapon fire recoil animation when a bulet is fired
					if (Weapon_Object_Script_IN.Weapon_Has_Fired_Out_Func() == 1){
						IK_Arm_Aim_Obj_Script_IN.Play_Weapon_Animation(1);
					}else{//go back to weapon idle if bulet not fired
						IK_Arm_Aim_Obj_Script_IN.Play_Weapon_Animation(0);
					}

					}
				}
			}
			if (Preset_Shoot_Target == 0){
				if (Weapon_Object_Script_IN){
				//stop firing the weapon
				Weapon_Object_Script_IN.Stop_Fire_Func();
				}
			}
			//====================


			//Aim weapon using IK or return arm to normal animation control
			if (Preset_Aim_Weapon == 1){
				ikActive = true;
			}else{
				ikActive = false;
			}
			//======================

			
			//for crouched idle
			if (Preset_Idle_Crouched == 1){
				//play the crouched animation
				animator.SetInteger("Activity_Select", 3);
			}
			if (Preset_Idle_Crouched == 0){
				//play the crouched animation
				//animator.SetInteger("Activity_Select", 0);
			}
			//=========
			
			
			
			
			
			//========
			if (Preset_Die == 1){//INPUT
				//stop the animator from repeating death animation
				if (DieOnce == 1){
					if (animator.GetCurrentAnimatorStateInfo(0).IsName("Dying"))
					{ 	//set the value to some none existing animation
						animator.SetInteger("Activity_Select", 505);
						DieOnce = 2;
					}
				}
				//play the death animation
				if (DieOnce == 0){
					DieOnce = 1;
					animator.SetInteger("Activity_Select", 555);
				}
				//=====if the death animation is done then disable Halt_Processes
				if (DieOnce == 2){
					if (animator.GetCurrentAnimatorStateInfo(0).IsName("Dying"))
					{ 	
						Im_Dead = 1;
						if(animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f){
							Halt_Processes = 1;
						}
					}
				}
				//===============
				
			}
			//=======

			
			
			
			

			
			//Old way==move rigid body in direction of rotation (where it is facing cause of transform.forward)================================
			//MoveForce_And_JumpSpeed = MoveSpeed + JumpSpeed;
			//float MoveForce = Mathf.Sign(throttle) * MoveSpeed * rigidbody.mass;
			//float MoveForce = Mathf.Sign(throttle) * MoveForce_And_JumpSpeed * GetComponent<Rigidbody>().mass;
			//add gravity again or not
			//rigidbody.velocity += Vector3.down * Time.deltaTime * 10;
			//GetComponent<Rigidbody>().AddForce(transform.forward * Time.deltaTime * (MoveForce));
			//always reset angular velocity cause when there is a bump in to an object it will go crazy in rotation
			//GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
			//================================





				MoveStartEnabled = 1;

		}//currenttarget

		


		}//if (Halt_Processes == 0){



		//playerText.text = "wayeer: " +  CurrentTargetType;
		//playerText.text = "Act: " + Direction_Animation_Selected;// Personality_ID_Name[Curent_Activity_ID] + ", Mov: " + Personality_ID_Name[Curent_Move_ID];
		//playerText.text = "Act: " + Personality_ID_Name[Curent_Activity_ID] + ", Mov: " + Personality_ID_Name[Curent_Move_ID];
		//playerText.text = "vel z: " +  GetComponent<Rigidbody>().velocity.z;
	
		
		}//Update




	
	//=========
	void OnTriggerEnter(Collider other)
	{
		//if hit
		if (other.tag == "Enemy_Projectile_01_Tag") {
			
			Do_Damage(15);
		}
		//if you enter a InteriorSlidingDoor_Tag
		if (other.tag == "InteriorSlidingDoor_Tag") {
			Entered_Door_Space = 1;
			//get the door script
			InteriorSlidingDoor_Script_IN = other.transform.GetComponent<InteriorSlidingDoor_Scr>();
		}

		
	}
	//==========
	
	void OnTriggerExit(Collider other) {
		//to change way point if the current on is touched
		if (other.tag == "Boundary_Tag") {
			
		}
		//if you leave a InteriorSlidingDoor_Tag
		if (other.tag == "InteriorSlidingDoor_Tag") {
			Entered_Door_Space = 0;
			Entered_Door_Space_React_Once = 1;
		}

		
	}
	//=========
	
	//====
	
	public void Do_Damage(int Damage_amount_In){
		if (currentHealth > 0){
			currentHealth = currentHealth - Damage_amount_In;
		}
		if (currentHealth < 0) {
			currentHealth = 0;
		}
		
		if (currentHealth >= 0 && currentHealth <= currentHealthMAX){
			
			float currentHealth_ff = currentHealth;
			float currentHealth_toPoint1 = currentHealth_ff / 100;
			//update the health bar
			if (Player_Slider_Slider){
			Player_Slider_Slider.value = currentHealth;
			}
		}
		
		//destroy this object if there is no more health
		if (currentHealth == 0) {
			//Destroy_me();
		}
		//======
		
		
	}
	
	public void Restore_Health(int Health_amount_In){
		if (currentHealth < currentHealthMAX){
			currentHealth = currentHealth + Health_amount_In;
		}
	}
	
	//====
	


}
