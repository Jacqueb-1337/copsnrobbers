using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Boo.Lang;
using Boo.Lang.Runtime;
using UnityEngine;
using UnityScript.Lang;

[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations)]
[assembly: AssemblyVersion("0.0.0.0")]
[Serializable]
public class Detonator Spray Helper : MonoBehaviour
{
	public float startTimeMin;

	public float startTimeMax;

	public float stopTimeMin;

	public float stopTimeMax;

	public Material firstMaterial;

	public Material secondMaterial;

	private float startTime;

	private float stopTime;

	private float spawnTime;

	private bool isReallyOn;

	public Detonator Spray Helper()
	{
		stopTimeMin = 10f;
		stopTimeMax = 10f;
	}

	public virtual void Start()
	{
		isReallyOn = particleEmitter.emit;
		particleEmitter.emit = false;
		spawnTime = Time.time;
		startTime = UnityEngine.Random.value * (startTimeMax - startTimeMin) + startTimeMin + Time.time;
		stopTime = UnityEngine.Random.value * (stopTimeMax - stopTimeMin) + stopTimeMin + Time.time;
		if (!(UnityEngine.Random.value <= 0.5f))
		{
			renderer.material = firstMaterial;
		}
		else
		{
			renderer.material = secondMaterial;
		}
	}

	public virtual void FixedUpdate()
	{
		if (!(Time.time <= startTime))
		{
			particleEmitter.emit = isReallyOn;
		}
		if (!(Time.time <= stopTime))
		{
			particleEmitter.emit = false;
		}
	}

	public virtual void Main()
	{
	}
}
[Serializable]
public class DetonatorTest : MonoBehaviour
{
	public GameObject currentDetonator;

	private int _currentExpIdx;

	private bool buttonClicked;

	public GameObject[] detonatorPrefabs;

	public float explosionLife;

	public float timeScale;

	public float detailLevel;

	public GameObject wall;

	private GameObject _currentWall;

	private int _spawnWallTime;

	private object _guiRect;

	private bool toggleBool;

	private Rect checkRect;

	public DetonatorTest()
	{
		_currentExpIdx = -1;
		explosionLife = 10f;
		timeScale = 1f;
		detailLevel = 1f;
		_spawnWallTime = -1000;
		checkRect = new Rect(0f, 0f, 260f, 180f);
	}

	public virtual void Start()
	{
		SpawnWall();
		if (!currentDetonator)
		{
			NextExplosion();
		}
		else
		{
			_currentExpIdx = 0;
		}
	}

	public virtual void OnGUI()
	{
		_guiRect = new Rect(7f, Screen.height - 180, 250f, 200f);
		GUILayout.BeginArea((Rect)_guiRect);
		GUILayout.BeginVertical();
		string lhs = currentDetonator.name;
		if (GUILayout.Button(lhs + " (Click For Next)"))
		{
			NextExplosion();
		}
		if (GUILayout.Button("Rebuild Wall"))
		{
			SpawnWall();
		}
		if (GUILayout.Button("Camera Far"))
		{
			Camera.main.transform.position = new Vector3(0f, 0f, -7f);
			Camera.main.transform.eulerAngles = new Vector3(13.5f, 0f, 0f);
		}
		if (GUILayout.Button("Camera Near"))
		{
			Camera.main.transform.position = new Vector3(0f, -8.664466f, 31.38269f);
			Camera.main.transform.eulerAngles = new Vector3(1.213462f, 0f, 0f);
		}
		GUILayout.Label("Time Scale");
		timeScale = GUILayout.HorizontalSlider(timeScale, 0f, 1f);
		GUILayout.Label("Detail Level (re-explode after change)");
		detailLevel = GUILayout.HorizontalSlider(detailLevel, 0f, 1f);
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}

	public virtual void NextExplosion()
	{
		if (_currentExpIdx >= detonatorPrefabs.Length - 1)
		{
			_currentExpIdx = 0;
		}
		else
		{
			_currentExpIdx++;
		}
		currentDetonator = detonatorPrefabs[_currentExpIdx];
	}

	public virtual void SpawnWall()
	{
		if ((bool)_currentWall)
		{
			UnityEngine.Object.Destroy(_currentWall);
		}
		_currentWall = (GameObject)UnityEngine.Object.Instantiate(wall, new Vector3(-7f, -12f, 48f), Quaternion.identity);
		_spawnWallTime = (int)Time.time;
	}

	public virtual void Update()
	{
		_guiRect = new Rect(7f, Screen.height - 150, 250f, 200f);
		if (!(Time.time + (float)_spawnWallTime <= 0.5f))
		{
			if (!checkRect.Contains(Input.mousePosition) && Input.GetMouseButtonDown(0))
			{
				SpawnExplosion();
			}
			Time.timeScale = timeScale;
		}
	}

	public virtual void SpawnExplosion()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hitInfo = default(RaycastHit);
		if (!Physics.Raycast(ray, out hitInfo, 1000f))
		{
		}
	}

	public virtual void Main()
	{
	}
}
[Serializable]
public enum Action
{
	Stand,
	Crouch,
	Prone
}
[Serializable]
public class animations
{
	public AnimationClip jumpPose;

	public AnimationClip stayIdle;

	public AnimationClip crouchIdle;

	public AnimationClip proneIdle;

	public AnimationClip walkFront;

	public AnimationClip walkBack;

	public AnimationClip walkLeft;

	public AnimationClip walkRight;

	public float walkAnimationsSpeed;

	public AnimationClip runFront;

	public float runAnimationsSpeed;

	public AnimationClip crouchFront;

	public AnimationClip crouchLeft;

	public AnimationClip crouchRight;

	public AnimationClip crouchBack;

	public float crouchAnimationsSpeed;

	public AnimationClip proneFront;

	public AnimationClip proneLeft;

	public AnimationClip proneRight;

	public AnimationClip proneBack;

	public float proneAnimationsSpeed;

	public AnimationClip pistolIdle;

	public AnimationClip knifeIdle;

	public AnimationClip gunIdle;

	public animations()
	{
		walkAnimationsSpeed = 1f;
		runAnimationsSpeed = 1f;
		crouchAnimationsSpeed = 1f;
		proneAnimationsSpeed = 1f;
	}
}
[Serializable]
public class CharacterAnimation : MonoBehaviour
{
	public GameObject animationSyncHelper;

	public GameObject animationForHands;

	public GameObject activeWeapon;

	public Action action;

	public animations Animations;

	public System.Collections.Generic.List<WeaponScript> twoHandedWeapons;

	public System.Collections.Generic.List<WeaponScript> pistols;

	public System.Collections.Generic.List<WeaponScript> knivesNades;

	private FPScontroller fpsController;

	private WeaponManager weapManager;

	public virtual void Start()
	{
		fpsController = GameObject.FindWithTag("Player").GetComponent<FPScontroller>();
		configureAnimations();
		weapManager = GameObject.FindWithTag("WeaponManager").GetComponent<WeaponManager>();
		if ((bool)weapManager)
		{
			ThirdPersonWeaponControl();
		}
	}

	public virtual void Update()
	{
		activeWeapon.name = weapManager.SelectedWeapon.weaponName;
		if (!fpsController.crouch && !fpsController.prone)
		{
			action = Action.Stand;
		}
		else if (fpsController.crouch && !fpsController.prone)
		{
			action = Action.Crouch;
		}
		else if (!fpsController.crouch && fpsController.prone)
		{
			action = Action.Prone;
		}
		if (action == Action.Stand)
		{
			if (fpsController.grounded)
			{
				if (fpsController.Walking && !fpsController.Running)
				{
					if (!Input.GetKey(KeyCode.W))
					{
						if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.S))
						{
							animation.CrossFade(Animations.walkLeft.name, 0.2f);
							animationSyncHelper.name = Animations.walkLeft.name;
						}
						else if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.S))
						{
							animation.CrossFade(Animations.walkRight.name, 0.2f);
							animationSyncHelper.name = Animations.walkRight.name;
						}
						else if (Input.GetKey(KeyCode.S))
						{
							animation.CrossFade(Animations.walkBack.name, 0.2f);
							animationSyncHelper.name = Animations.walkBack.name;
						}
					}
				}
				else if (fpsController.Walking && fpsController.Running && !Input.GetKey(KeyCode.W))
				{
				}
				if (!fpsController.Walking)
				{
					animation.CrossFade(Animations.stayIdle.name, 0.2f);
					animationSyncHelper.name = Animations.stayIdle.name;
				}
			}
			else
			{
				animation.CrossFade(Animations.jumpPose.name, 0.2f);
				animationSyncHelper.name = Animations.jumpPose.name;
			}
		}
		if (action == Action.Crouch)
		{
			if (fpsController.Walking)
			{
				if (!Input.GetKey(KeyCode.W))
				{
					if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.S))
					{
						animation.CrossFade(Animations.crouchLeft.name, 0.2f);
						animationSyncHelper.name = Animations.crouchLeft.name;
					}
					else if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.S))
					{
						animation.CrossFade(Animations.crouchRight.name, 0.2f);
						animationSyncHelper.name = Animations.crouchRight.name;
					}
					else if (Input.GetKey(KeyCode.S))
					{
						animation.CrossFade(Animations.crouchBack.name, 0.2f);
						animationSyncHelper.name = Animations.crouchBack.name;
					}
				}
			}
			else
			{
				animation.CrossFade(Animations.crouchIdle.name, 0.2f);
				animationSyncHelper.name = Animations.crouchIdle.name;
			}
		}
		if (action == Action.Prone)
		{
			if (fpsController.Walking)
			{
				if (!Input.GetKey(KeyCode.W))
				{
					if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.S))
					{
						animation.CrossFade(Animations.proneLeft.name, 0.2f);
						animationSyncHelper.name = Animations.proneLeft.name;
					}
					else if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.S))
					{
						animation.CrossFade(Animations.proneRight.name, 0.2f);
						animationSyncHelper.name = Animations.proneRight.name;
					}
					else if (Input.GetKey(KeyCode.S))
					{
						animation.CrossFade(Animations.proneBack.name, 0.2f);
						animationSyncHelper.name = Animations.proneBack.name;
					}
				}
			}
			else
			{
				animation.CrossFade(Animations.proneIdle.name, 0.2f);
				animationSyncHelper.name = Animations.proneIdle.name;
			}
		}
		ThirdPersonWeaponControl();
	}

	public virtual void ThirdPersonWeaponControl()
	{
		if (action != Action.Prone)
		{
			if (twoHandedWeapons.Contains(weapManager.SelectedWeapon))
			{
				animationForHands.name = Animations.gunIdle.name;
			}
			else if (pistols.Contains(weapManager.SelectedWeapon))
			{
				animationForHands.name = Animations.pistolIdle.name;
			}
			else if (knivesNades.Contains(weapManager.SelectedWeapon))
			{
				animationForHands.name = Animations.knifeIdle.name;
			}
		}
		else
		{
			animationForHands.name = "Null";
		}
	}

	public virtual void configureAnimations()
	{
		if ((bool)Animations.stayIdle)
		{
			animation[Animations.stayIdle.name].wrapMode = WrapMode.Loop;
		}
		if ((bool)Animations.crouchIdle)
		{
			animation[Animations.crouchIdle.name].wrapMode = WrapMode.Loop;
		}
		if ((bool)Animations.proneIdle)
		{
			animation[Animations.proneIdle.name].wrapMode = WrapMode.Loop;
		}
		if ((bool)Animations.walkFront)
		{
			animation[Animations.walkFront.name].wrapMode = WrapMode.Loop;
			animation[Animations.walkFront.name].speed = Animations.walkAnimationsSpeed;
		}
		if ((bool)Animations.walkBack)
		{
			animation[Animations.walkBack.name].wrapMode = WrapMode.Loop;
			animation[Animations.walkBack.name].speed = Animations.walkAnimationsSpeed;
		}
		if ((bool)Animations.walkLeft)
		{
			animation[Animations.walkLeft.name].wrapMode = WrapMode.Loop;
			animation[Animations.walkLeft.name].speed = Animations.walkAnimationsSpeed;
		}
		if ((bool)Animations.walkRight)
		{
			animation[Animations.walkRight.name].wrapMode = WrapMode.Loop;
			animation[Animations.walkRight.name].speed = Animations.walkAnimationsSpeed;
		}
		if ((bool)Animations.runFront)
		{
			animation[Animations.runFront.name].wrapMode = WrapMode.Loop;
			animation[Animations.runFront.name].speed = Animations.runAnimationsSpeed;
		}
		if ((bool)Animations.crouchFront)
		{
			animation[Animations.crouchFront.name].wrapMode = WrapMode.Loop;
			animation[Animations.crouchFront.name].speed = Animations.crouchAnimationsSpeed;
		}
		if ((bool)Animations.crouchLeft)
		{
			animation[Animations.crouchLeft.name].wrapMode = WrapMode.Loop;
			animation[Animations.crouchLeft.name].speed = Animations.crouchAnimationsSpeed;
		}
		if ((bool)Animations.crouchRight)
		{
			animation[Animations.crouchRight.name].wrapMode = WrapMode.Loop;
			animation[Animations.crouchRight.name].speed = Animations.crouchAnimationsSpeed;
		}
		if ((bool)Animations.crouchBack)
		{
			animation[Animations.crouchBack.name].wrapMode = WrapMode.Loop;
			animation[Animations.crouchBack.name].speed = Animations.crouchAnimationsSpeed;
		}
		if ((bool)Animations.proneFront)
		{
			animation[Animations.proneFront.name].wrapMode = WrapMode.Loop;
			animation[Animations.proneFront.name].speed = Animations.proneAnimationsSpeed;
		}
		if ((bool)Animations.proneLeft)
		{
			animation[Animations.proneLeft.name].wrapMode = WrapMode.Loop;
			animation[Animations.proneLeft.name].speed = Animations.proneAnimationsSpeed;
		}
		if ((bool)Animations.proneRight)
		{
			animation[Animations.proneRight.name].wrapMode = WrapMode.Loop;
			animation[Animations.proneRight.name].speed = Animations.proneAnimationsSpeed;
		}
		if ((bool)Animations.proneBack)
		{
			animation[Animations.proneBack.name].wrapMode = WrapMode.Loop;
			animation[Animations.proneBack.name].speed = Animations.proneAnimationsSpeed;
		}
	}

	public virtual void Main()
	{
	}
}
[Serializable]
public enum RotationAxes
{
	MouseXAndY,
	MouseX,
	MouseY
}
[Serializable]
[AddComponentMenu("FPS system/Character/FPS MouseLook")]
public class FPSMouseLook : MonoBehaviour
{
	public RotationAxes axes;

	public float sensitivity;

	public float aimSensitivity;

	[HideInInspector]
	public float sensitivityX;

	[HideInInspector]
	public float sensitivityY;

	private float minimumX;

	private float maximumX;

	public float minimumY;

	public float maximumY;

	private float rotationY;

	private WeaponManager weapManager;

	private WeaponScript weapScript;

	[HideInInspector]
	public float currentSensitivity;

	public FPSMouseLook()
	{
		axes = RotationAxes.MouseXAndY;
		sensitivity = 4f;
		aimSensitivity = 2f;
		sensitivityX = 15f;
		sensitivityY = 15f;
		minimumX = -360f;
		maximumX = 360f;
		minimumY = -80f;
		maximumY = 80f;
	}

	public virtual void Awake()
	{
	}

	public virtual void Update()
	{
		if (Time.timeScale >= 0.01f)
		{
			if (axes == RotationAxes.MouseXAndY)
			{
				float y = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;
				rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
				rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
				transform.localEulerAngles = new Vector3(0f - rotationY, y, 0f);
			}
			else if (axes == RotationAxes.MouseX)
			{
				transform.Rotate(0f, Input.GetAxis("Mouse X") * sensitivityX, 0f);
			}
			else
			{
				rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
				rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
				transform.localEulerAngles = new Vector3(0f - rotationY, transform.localEulerAngles.y, 0f);
			}
		}
	}

	public virtual void Recoil(float amount)
	{
		rotationY += amount;
	}

	public virtual void Main()
	{
	}
}
[Serializable]
[RequireComponent(typeof(AudioSource))]
public class FPSSoundController : MonoBehaviour
{
	public AudioClip[] walkSounds;

	public float walkStepLength;

	public float runStepLenght;

	public float crouchStepLenght;

	private CharacterController controller;

	private FPScontroller motor;

	private float lastStep;

	private float StepLenght;

	public FPSSoundController()
	{
		walkStepLength = 0.45f;
		runStepLenght = 0.38f;
		crouchStepLenght = 0.38f;
		lastStep = -10f;
	}

	public virtual void Awake()
	{
		StepLenght = walkStepLength;
		controller = (CharacterController)GetComponent(typeof(CharacterController));
		motor = (FPScontroller)GetComponent(typeof(FPScontroller));
	}

	public virtual void FixedUpdate()
	{
		if (!motor.prone)
		{
			if (motor.Walking && motor.grounded && !motor.crouch)
			{
				PlayStepSounds();
				StepLenght = walkStepLength;
			}
			if (motor.Running && motor.grounded)
			{
				PlayStepSounds();
				StepLenght = runStepLenght;
			}
			if (motor.Walking && motor.crouch && motor.grounded)
			{
				PlayStepSounds();
				StepLenght = crouchStepLenght;
			}
		}
	}

	public virtual void PlayStepSounds()
	{
		if (!(Time.time <= StepLenght + lastStep))
		{
			audio.clip = walkSounds[UnityEngine.Random.Range(0, Extensions.get_length((System.Array)walkSounds))];
			audio.Play();
			lastStep = Time.time;
		}
	}

	public virtual void Main()
	{
	}
}
[Serializable]
public class FPScontrollerMovement
{
	[HideInInspector]
	public float maxForwardSpeed;

	[HideInInspector]
	public float maxSidewaysSpeed;

	[HideInInspector]
	public float maxBackwardsSpeed;

	public float WalkSpeed;

	public float RunSpeed;

	public bool canCrouch;

	public float CrouchSpeed;

	public float crouchHeight;

	public float crouchSmooth;

	public bool canProne;

	public float ProneSpeed;

	public float proneHeight;

	public AnimationCurve slopeSpeedMultiplier;

	public float maxGroundAcceleration;

	public float maxAirAcceleration;

	public float gravity;

	public float maxFallSpeed;

	[HideInInspector]
	public bool enableGravity;

	[NonSerialized]
	public CollisionFlags collisionFlags;

	[NonSerialized]
	public Vector3 velocity;

	[NonSerialized]
	public Vector3 frameVelocity;

	[NonSerialized]
	public Vector3 hitPoint;

	[NonSerialized]
	public Vector3 lastHitPoint;

	public FPScontrollerMovement()
	{
		maxForwardSpeed = 10f;
		maxSidewaysSpeed = 10f;
		maxBackwardsSpeed = 10f;
		WalkSpeed = 6f;
		RunSpeed = 9f;
		canCrouch = true;
		CrouchSpeed = 3f;
		crouchHeight = 1.5f;
		crouchSmooth = 8f;
		canProne = true;
		ProneSpeed = 1.5f;
		proneHeight = 0.7f;
		slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90f, 1f), new Keyframe(0f, 1f), new Keyframe(90f, 0f));
		maxGroundAcceleration = 30f;
		maxAirAcceleration = 20f;
		gravity = 10f;
		maxFallSpeed = 20f;
		enableGravity = true;
		frameVelocity = Vector3.zero;
		hitPoint = Vector3.zero;
		lastHitPoint = new Vector3(float.PositiveInfinity, 0f, 0f);
	}
}
[Serializable]
public enum FPSMovementTransferOnJump
{
	None,
	InitTransfer,
	PermaTransfer,
	PermaLocked
}
[Serializable]
public class FPScontrollerJumping
{
	public bool enabled;

	public float baseHeight;

	public float extraHeight;

	public float perpAmount;

	public float steepPerpAmount;

	[NonSerialized]
	public bool jumping;

	[NonSerialized]
	public bool holdingJumpButton;

	[NonSerialized]
	public float lastStartTime;

	[NonSerialized]
	public float lastButtonDownTime;

	[NonSerialized]
	public Vector3 jumpDir;

	public FPScontrollerJumping()
	{
		enabled = true;
		baseHeight = 1f;
		extraHeight = 4.1f;
		steepPerpAmount = 0.5f;
		lastButtonDownTime = -100f;
		jumpDir = Vector3.up;
	}
}
[Serializable]
public class FPScontrollerMovingPlatform
{
	public bool enabled;

	public FPSMovementTransferOnJump movementTransfer;

	[NonSerialized]
	public Transform hitPlatform;

	[NonSerialized]
	public Transform activePlatform;

	[NonSerialized]
	public Vector3 activeLocalPoint;

	[NonSerialized]
	public Vector3 activeGlobalPoint;

	[NonSerialized]
	public Quaternion activeLocalRotation;

	[NonSerialized]
	public Quaternion activeGlobalRotation;

	[NonSerialized]
	public Matrix4x4 lastMatrix;

	[NonSerialized]
	public Vector3 platformVelocity;

	[NonSerialized]
	public bool newPlatform;

	public FPScontrollerMovingPlatform()
	{
		enabled = true;
		movementTransfer = FPSMovementTransferOnJump.PermaTransfer;
	}
}
[Serializable]
public class FPScontrollerSliding
{
	public bool enabled;

	public float slidingSpeed;

	public float sidewaysControl;

	public float speedControl;

	public FPScontrollerSliding()
	{
		enabled = true;
		slidingSpeed = 15f;
		sidewaysControl = 1f;
		speedControl = 0.4f;
	}
}
[Serializable]
public class FPScontrollerPushing
{
	public bool canPush;

	public float pushPower;

	public FPScontrollerPushing()
	{
		canPush = true;
		pushPower = 2f;
	}
}
[Serializable]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(FPSinput))]
[AddComponentMenu("FPS system/Character/FPS Controller")]
public class FPScontroller : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	internal sealed class $setupBools$154 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class $ : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal FPScontroller $self_$155;

			public $(FPScontroller self_)
			{
				$self_$155 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					if (!$self_$155.movement.canProne)
					{
						$self_$155.movement.canProne = true;
						result = (Yield(2, new WaitForSeconds(0.2f)) ? 1 : 0);
						break;
					}
					goto IL_0064;
				case 2:
					$self_$155.movement.canProne = false;
					goto IL_0064;
				case 1:
					{
						result = 0;
						break;
					}
					IL_0064:
					YieldDefault(1);
					goto case 1;
				}
				return (byte)result != 0;
			}
		}

		internal FPScontroller $self_$156;

		public $setupBools$154(FPScontroller self_)
		{
			$self_$156 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new $($self_$156);
		}
	}

	[Serializable]
	[CompilerGenerated]
	internal sealed class $SubtractNewPlatformVelocity$157 : GenericGenerator<object>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class $ : GenericGeneratorEnumerator<object>, IEnumerator
		{
			internal Transform $platform$158;

			internal FPScontroller $self_$159;

			public $(FPScontroller self_)
			{
				$self_$159 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					if ($self_$159.movingPlatform.enabled && ($self_$159.movingPlatform.movementTransfer == FPSMovementTransferOnJump.InitTransfer || $self_$159.movingPlatform.movementTransfer == FPSMovementTransferOnJump.PermaTransfer))
					{
						if ($self_$159.movingPlatform.newPlatform)
						{
							$platform$158 = $self_$159.movingPlatform.activePlatform;
							result = (Yield(2, new WaitForFixedUpdate()) ? 1 : 0);
							break;
						}
						goto case 4;
					}
					goto IL_0124;
				case 2:
					result = (Yield(3, new WaitForFixedUpdate()) ? 1 : 0);
					break;
				case 3:
					if ($self_$159.grounded && $platform$158 == $self_$159.movingPlatform.activePlatform)
					{
						result = (Yield(4, 1) ? 1 : 0);
						break;
					}
					goto case 4;
				case 4:
					$self_$159.movement.velocity = $self_$159.movement.velocity - $self_$159.movingPlatform.platformVelocity;
					goto IL_0124;
				case 1:
					{
						result = 0;
						break;
					}
					IL_0124:
					YieldDefault(1);
					goto case 1;
				}
				return (byte)result != 0;
			}
		}

		internal FPScontroller $self_$160;

		public $SubtractNewPlatformVelocity$157(FPScontroller self_)
		{
			$self_$160 = self_;
		}

		public override IEnumerator<object> GetEnumerator()
		{
			return new $($self_$160);
		}
	}

	public bool canControl;

	public bool useFixedUpdate;

	[HideInInspector]
	public bool Running;

	[HideInInspector]
	public bool Walking;

	[HideInInspector]
	public bool canRun;

	private GameObject mainCamera;

	[HideInInspector]
	public bool onLadder;

	private float ladderHopSpeed;

	[NonSerialized]
	public Vector3 inputMoveDirection;

	[NonSerialized]
	public bool inputJump;

	[HideInInspector]
	public bool inputRun;

	[HideInInspector]
	public bool inputCrouch;

	[HideInInspector]
	public bool inputProne;

	public FPScontrollerMovement movement;

	[HideInInspector]
	public bool crouch;

	private float standartHeight;

	private GameObject lookObj;

	private float centerY;

	private bool canStand;

	private bool canStandCrouch;

	[HideInInspector]
	public bool prone;

	public FPScontrollerJumping jumping;

	public FPScontrollerMovingPlatform movingPlatform;

	public FPScontrollerSliding sliding;

	public FPScontrollerPushing pushing;

	[NonSerialized]
	public bool grounded;

	[NonSerialized]
	public Vector3 groundNormal;

	private Vector3 lastGroundNormal;

	private Transform tr;

	private CharacterController controller;

	public FPScontroller()
	{
		canControl = true;
		useFixedUpdate = true;
		ladderHopSpeed = 6f;
		inputMoveDirection = Vector3.zero;
		movement = new FPScontrollerMovement();
		canStandCrouch = true;
		jumping = new FPScontrollerJumping();
		movingPlatform = new FPScontrollerMovingPlatform();
		sliding = new FPScontrollerSliding();
		grounded = true;
		groundNormal = Vector3.zero;
		lastGroundNormal = Vector3.zero;
	}

	public virtual void Awake()
	{
		controller = (CharacterController)GetComponent(typeof(CharacterController));
		standartHeight = controller.height;
		lookObj = GameObject.FindWithTag("LookObject");
		centerY = controller.center.y;
		tr = transform;
		mainCamera = GameObject.FindWithTag("MainCamera");
		canRun = true;
		canStand = true;
		StartCoroutine_Auto(setupBools());
	}

	public virtual IEnumerator setupBools()
	{
		return new $setupBools$154(this).GetEnumerator();
	}

	private void UpdateFunction()
	{
		Vector3 velocity = movement.velocity;
		velocity = ApplyInputVelocityChange(velocity);
		if (movement.enableGravity)
		{
			if ((prone || crouch) && inputJump)
			{
				return;
			}
			velocity = ApplyGravityAndJumping(velocity);
		}
		Vector3 zero = Vector3.zero;
		if (MoveWithPlatform())
		{
			Vector3 vector = movingPlatform.activePlatform.TransformPoint(movingPlatform.activeLocalPoint);
			zero = vector - movingPlatform.activeGlobalPoint;
			if (zero != Vector3.zero)
			{
				controller.Move(zero);
			}
			Quaternion quaternion = movingPlatform.activePlatform.rotation * movingPlatform.activeLocalRotation;
			float y = (quaternion * Quaternion.Inverse(movingPlatform.activeGlobalRotation)).eulerAngles.y;
			if (y != 0f)
			{
				tr.Rotate(0f, y, 0f);
			}
		}
		Vector3 position = tr.position;
		Vector3 motion = velocity * Time.deltaTime;
		float num = Mathf.Max(controller.stepOffset, new Vector3(motion.x, 0f, motion.z).magnitude);
		if (grounded)
		{
			motion -= num * Vector3.up;
		}
		movingPlatform.hitPlatform = null;
		groundNormal = Vector3.zero;
		movement.collisionFlags = controller.Move(motion);
		movement.lastHitPoint = movement.hitPoint;
		lastGroundNormal = groundNormal;
		if (movingPlatform.enabled && movingPlatform.activePlatform != movingPlatform.hitPlatform && movingPlatform.hitPlatform != null)
		{
			movingPlatform.activePlatform = movingPlatform.hitPlatform;
			movingPlatform.lastMatrix = movingPlatform.hitPlatform.localToWorldMatrix;
			movingPlatform.newPlatform = true;
		}
		Vector3 vector2 = new Vector3(velocity.x, 0f, velocity.z);
		movement.velocity = (tr.position - position) / Time.deltaTime;
		Vector3 lhs = new Vector3(movement.velocity.x, 0f, movement.velocity.z);
		if (vector2 == Vector3.zero)
		{
			movement.velocity = new Vector3(0f, movement.velocity.y, 0f);
		}
		else
		{
			float value = Vector3.Dot(lhs, vector2) / vector2.sqrMagnitude;
			movement.velocity = vector2 * Mathf.Clamp01(value) + movement.velocity.y * Vector3.up;
		}
		if (!(movement.velocity.y >= velocity.y - 0.001f))
		{
			if (!(movement.velocity.y >= 0f))
			{
				movement.velocity.y = velocity.y;
			}
			else
			{
				jumping.holdingJumpButton = false;
			}
		}
		if (grounded && !IsGroundedTest())
		{
			grounded = false;
			if (movingPlatform.enabled && (movingPlatform.movementTransfer == FPSMovementTransferOnJump.InitTransfer || movingPlatform.movementTransfer == FPSMovementTransferOnJump.PermaTransfer))
			{
				movement.frameVelocity = movingPlatform.platformVelocity;
				movement.velocity += movingPlatform.platformVelocity;
			}
			SendMessage("OnFall", SendMessageOptions.DontRequireReceiver);
			tr.position += num * Vector3.up;
		}
		else if (!grounded && IsGroundedTest())
		{
			grounded = true;
			jumping.jumping = false;
			StartCoroutine_Auto(SubtractNewPlatformVelocity());
			SendMessage("OnLand", SendMessageOptions.DontRequireReceiver);
		}
		if (MoveWithPlatform())
		{
			movingPlatform.activeGlobalPoint = tr.position + Vector3.up * (controller.center.y - controller.height * 0.5f + controller.radius);
			movingPlatform.activeLocalPoint = movingPlatform.activePlatform.InverseTransformPoint(movingPlatform.activeGlobalPoint);
			movingPlatform.activeGlobalRotation = tr.rotation;
			movingPlatform.activeLocalRotation = Quaternion.Inverse(movingPlatform.activePlatform.rotation) * movingPlatform.activeGlobalRotation;
		}
	}

	public virtual void FixedUpdate()
	{
		if (movingPlatform.enabled)
		{
			if (movingPlatform.activePlatform != null)
			{
				if (!movingPlatform.newPlatform)
				{
					Vector3 platformVelocity = movingPlatform.platformVelocity;
					movingPlatform.platformVelocity = (movingPlatform.activePlatform.localToWorldMatrix.MultiplyPoint3x4(movingPlatform.activeLocalPoint) - movingPlatform.lastMatrix.MultiplyPoint3x4(movingPlatform.activeLocalPoint)) / Time.deltaTime;
				}
				movingPlatform.lastMatrix = movingPlatform.activePlatform.localToWorldMatrix;
				movingPlatform.newPlatform = false;
			}
			else
			{
				movingPlatform.platformVelocity = Vector3.zero;
			}
		}
		if (useFixedUpdate)
		{
			UpdateFunction();
		}
	}

	public virtual void Update()
	{
		if (!useFixedUpdate)
		{
			UpdateFunction();
		}
		if (!(Input.GetAxis("Vertical") <= 0.1f) && inputRun && canRun && !onLadder && Walking)
		{
			if (canStand && canStandCrouch)
			{
				OnRunning();
			}
		}
		else
		{
			OffRunning();
		}
		float num = movement.velocity.x;
		if (num == 0f)
		{
			num = movement.velocity.z;
		}
		if (!(num > 0.01f))
		{
			float num2 = movement.velocity.x;
			if (num2 == 0f)
			{
				num2 = movement.velocity.z;
			}
			if (num2 >= -0.01f)
			{
				Walking = false;
				goto IL_00fb;
			}
		}
		Walking = true;
		goto IL_00fb;
		IL_00fb:
		if (canControl)
		{
			if (movement.canCrouch && !onLadder)
			{
				Crouch();
			}
			if (movement.canProne && !onLadder)
			{
				Prone();
			}
			if (onLadder)
			{
				grounded = false;
				crouch = false;
				prone = false;
			}
			if (!crouch && !prone && !(controller.height >= standartHeight - 0.01f))
			{
				controller.height = Mathf.Lerp(controller.height, standartHeight, Time.deltaTime / movement.crouchSmooth);
				float y = Mathf.Lerp(controller.center.y, centerY, Time.deltaTime / movement.crouchSmooth);
				Vector3 center = controller.center;
				float num3 = (center.y = y);
				Vector3 vector = (controller.center = center);
				float y2 = Mathf.Lerp(lookObj.transform.localPosition.y, standartHeight, Time.deltaTime / movement.crouchSmooth);
				Vector3 localPosition = lookObj.transform.localPosition;
				float num4 = (localPosition.y = y2);
				Vector3 vector3 = (lookObj.transform.localPosition = localPosition);
			}
		}
	}

	public virtual void Prone()
	{
		Vector3 vector = transform.TransformDirection(Vector3.up);
		RaycastHit hitInfo = default(RaycastHit);
		CharacterController characterController = (CharacterController)GetComponent(typeof(CharacterController));
		Vector3 position = transform.position;
		if (inputProne && !Running && !onLadder && (canStand || crouch))
		{
			crouch = false;
			prone = !prone;
			if (!prone)
			{
				crouch = true;
			}
			else
			{
				canStandCrouch = true;
			}
			if (canStandCrouch)
			{
				crouch = false;
			}
		}
		if (inputJump && prone && canStand)
		{
			prone = false;
			crouch = true;
			if (canStandCrouch)
			{
				crouch = false;
			}
			else
			{
				crouch = true;
			}
		}
		if (prone || Running)
		{
			if (!Physics.SphereCast(position, characterController.radius, transform.up, out hitInfo, movement.crouchHeight * 0.9f))
			{
				if (Running && prone)
				{
					prone = false;
					if (!prone)
					{
						crouch = true;
					}
					if (canStandCrouch)
					{
						crouch = false;
					}
				}
				if (prone)
				{
					canStand = true;
				}
			}
			else if (prone)
			{
				canStand = false;
			}
		}
		if (prone && !crouch && !(controller.height <= movement.proneHeight + 0.01f))
		{
			controller.height = Mathf.Lerp(controller.height, movement.proneHeight, Time.deltaTime / movement.crouchSmooth);
			float y = Mathf.Lerp(controller.center.y, movement.proneHeight / 2f, Time.deltaTime / movement.crouchSmooth);
			Vector3 center = controller.center;
			float num = (center.y = y);
			Vector3 vector2 = (controller.center = center);
			float y2 = Mathf.Lerp(lookObj.transform.localPosition.y, movement.proneHeight, Time.deltaTime / movement.crouchSmooth);
			Vector3 localPosition = lookObj.transform.localPosition;
			float num2 = (localPosition.y = y2);
			Vector3 vector4 = (lookObj.transform.localPosition = localPosition);
			movement.maxForwardSpeed = movement.ProneSpeed;
			movement.maxSidewaysSpeed = movement.ProneSpeed;
			movement.maxBackwardsSpeed = movement.ProneSpeed;
		}
	}

	public virtual void Crouch()
	{
		Vector3 vector = transform.TransformDirection(Vector3.up);
		RaycastHit hitInfo = default(RaycastHit);
		CharacterController characterController = (CharacterController)GetComponent(typeof(CharacterController));
		Vector3 position = transform.position;
		if (inputCrouch && !Running && !onLadder && canStand)
		{
			prone = false;
			crouch = !crouch;
		}
		if (!Physics.SphereCast(position, characterController.radius, transform.up, out hitInfo, standartHeight))
		{
			if (inputJump && crouch)
			{
				crouch = false;
			}
			if (Running && crouch)
			{
				crouch = false;
			}
			if (crouch)
			{
				canStand = true;
			}
			canStandCrouch = true;
		}
		else
		{
			if (crouch)
			{
				canStand = false;
			}
			canStandCrouch = false;
		}
		if (crouch && !prone && (controller.height >= movement.crouchHeight + 0.01f || controller.height <= movement.crouchHeight - 0.01f))
		{
			controller.height = Mathf.Lerp(controller.height, movement.crouchHeight, Time.deltaTime / movement.crouchSmooth);
			float y = Mathf.Lerp(controller.center.y, movement.crouchHeight / 2f, Time.deltaTime / movement.crouchSmooth);
			Vector3 center = controller.center;
			float num = (center.y = y);
			Vector3 vector2 = (controller.center = center);
			float y2 = Mathf.Lerp(lookObj.transform.localPosition.y, movement.crouchHeight, Time.deltaTime / movement.crouchSmooth);
			Vector3 localPosition = lookObj.transform.localPosition;
			float num2 = (localPosition.y = y2);
			Vector3 vector4 = (lookObj.transform.localPosition = localPosition);
			movement.maxForwardSpeed = movement.CrouchSpeed;
			movement.maxSidewaysSpeed = movement.CrouchSpeed;
			movement.maxBackwardsSpeed = movement.CrouchSpeed;
		}
	}

	public virtual void OnRunning()
	{
		Running = true;
		movement.maxForwardSpeed = movement.RunSpeed;
		movement.maxSidewaysSpeed = movement.RunSpeed;
		jumping.extraHeight = jumping.baseHeight + 0.15f;
	}

	public virtual void OffRunning()
	{
		Running = false;
		if (!crouch && !prone)
		{
			movement.maxForwardSpeed = movement.WalkSpeed;
			movement.maxSidewaysSpeed = movement.WalkSpeed;
			movement.maxBackwardsSpeed = movement.WalkSpeed / 2f;
			jumping.extraHeight = jumping.baseHeight;
		}
	}

	public virtual void OnLadder()
	{
		onLadder = true;
		inputMoveDirection = Vector3.zero;
		movement.enableGravity = false;
	}

	public virtual void OffLadder(object ladderMovement)
	{
		onLadder = false;
		Vector3 forward = mainCamera.transform.forward;
		forward = transform.TransformDirection(forward);
		movement.enableGravity = true;
	}

	private Vector3 ApplyInputVelocityChange(Vector3 velocity)
	{
		if (!canControl)
		{
			inputMoveDirection = Vector3.zero;
		}
		Vector3 vector = default(Vector3);
		if (grounded && TooSteep())
		{
			vector = new Vector3(groundNormal.x, 0f, groundNormal.z).normalized;
			Vector3 vector2 = Vector3.Project(inputMoveDirection, vector);
			vector = vector + vector2 * sliding.speedControl + (inputMoveDirection - vector2) * sliding.sidewaysControl;
			vector *= sliding.slidingSpeed;
		}
		else
		{
			vector = GetDesiredHorizontalVelocity();
		}
		if (movingPlatform.enabled && movingPlatform.movementTransfer == FPSMovementTransferOnJump.PermaTransfer)
		{
			vector += movement.frameVelocity;
			vector.y = 0f;
		}
		if (grounded)
		{
			vector = AdjustGroundVelocityToNormal(vector, groundNormal);
		}
		else
		{
			velocity.y = 0f;
		}
		float num = GetMaxAcceleration(grounded) * Time.deltaTime;
		Vector3 vector3 = vector - velocity;
		if (!(vector3.sqrMagnitude <= num * num))
		{
			vector3 = vector3.normalized * num;
		}
		if (grounded || canControl)
		{
			velocity += vector3;
		}
		if (grounded)
		{
			velocity.y = Mathf.Min(velocity.y, 0f);
		}
		return velocity;
	}

	private Vector3 ApplyGravityAndJumping(Vector3 velocity)
	{
		if (!inputJump || !canControl)
		{
			jumping.holdingJumpButton = false;
			jumping.lastButtonDownTime = -100f;
		}
		if (inputJump && !(jumping.lastButtonDownTime >= 0f) && canControl)
		{
			jumping.lastButtonDownTime = Time.time;
		}
		if (grounded)
		{
			velocity.y = Mathf.Min(0f, velocity.y) - movement.gravity * Time.deltaTime;
		}
		else
		{
			velocity.y = movement.velocity.y - movement.gravity * Time.deltaTime;
			if (jumping.jumping && jumping.holdingJumpButton && !(Time.time >= jumping.lastStartTime + jumping.extraHeight / CalculateJumpVerticalSpeed(jumping.baseHeight)))
			{
				velocity += jumping.jumpDir * movement.gravity * Time.deltaTime;
			}
			velocity.y = Mathf.Max(velocity.y, 0f - movement.maxFallSpeed);
		}
		if (grounded)
		{
			if (jumping.enabled && canControl && !(Time.time - jumping.lastButtonDownTime >= 0.2f))
			{
				grounded = false;
				jumping.jumping = true;
				jumping.lastStartTime = Time.time;
				jumping.lastButtonDownTime = -100f;
				jumping.holdingJumpButton = true;
				if (TooSteep())
				{
					jumping.jumpDir = Vector3.Slerp(Vector3.up, groundNormal, jumping.steepPerpAmount);
				}
				else
				{
					jumping.jumpDir = Vector3.Slerp(Vector3.up, groundNormal, jumping.perpAmount);
				}
				velocity.y = 0f;
				velocity += jumping.jumpDir * CalculateJumpVerticalSpeed(jumping.baseHeight);
				if (movingPlatform.enabled && (movingPlatform.movementTransfer == FPSMovementTransferOnJump.InitTransfer || movingPlatform.movementTransfer == FPSMovementTransferOnJump.PermaTransfer))
				{
					movement.frameVelocity = movingPlatform.platformVelocity;
					velocity += movingPlatform.platformVelocity;
				}
				SendMessage("OnJump", SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				jumping.holdingJumpButton = false;
			}
		}
		return velocity;
	}

	public virtual void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (!(hit.normal.y <= 0f) && !(hit.normal.y <= groundNormal.y) && !(hit.moveDirection.y >= 0f))
		{
			if ((hit.point - movement.lastHitPoint).sqrMagnitude > 0.001f || lastGroundNormal == Vector3.zero)
			{
				groundNormal = hit.normal;
			}
			else
			{
				groundNormal = lastGroundNormal;
			}
			movingPlatform.hitPlatform = hit.collider.transform;
			movement.hitPoint = hit.point;
			movement.frameVelocity = Vector3.zero;
		}
		if (pushing.canPush)
		{
			Rigidbody attachedRigidbody = hit.collider.attachedRigidbody;
			if (!(attachedRigidbody == null) && !attachedRigidbody.isKinematic && hit.moveDirection.y >= -0.3f)
			{
				Vector3 vector = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
				attachedRigidbody.velocity = vector * pushing.pushPower;
			}
		}
	}

	private IEnumerator SubtractNewPlatformVelocity()
	{
		return new $SubtractNewPlatformVelocity$157(this).GetEnumerator();
	}

	private bool MoveWithPlatform()
	{
		bool num = movingPlatform.enabled;
		if (num)
		{
			num = grounded;
			if (!num)
			{
				num = movingPlatform.movementTransfer == FPSMovementTransferOnJump.PermaLocked;
			}
		}
		if (num)
		{
			num = movingPlatform.activePlatform != null;
		}
		return num;
	}

	private Vector3 GetDesiredHorizontalVelocity()
	{
		Vector3 vector = tr.InverseTransformDirection(inputMoveDirection);
		float num = MaxSpeedInDirection(vector);
		if (grounded)
		{
			float time = Mathf.Asin(movement.velocity.normalized.y) * 57.29578f;
			num *= movement.slopeSpeedMultiplier.Evaluate(time);
		}
		return tr.TransformDirection(vector * num);
	}

	private Vector3 AdjustGroundVelocityToNormal(Vector3 hVelocity, Vector3 groundNormal)
	{
		Vector3 lhs = Vector3.Cross(Vector3.up, hVelocity);
		return Vector3.Cross(lhs, groundNormal).normalized * hVelocity.magnitude;
	}

	private bool IsGroundedTest()
	{
		return groundNormal.y > 0.01f;
	}

	public virtual float GetMaxAcceleration(bool grounded)
	{
		return (!grounded) ? movement.maxAirAcceleration : movement.maxGroundAcceleration;
	}

	public virtual float CalculateJumpVerticalSpeed(float targetJumpHeight)
	{
		return Mathf.Sqrt(2f * targetJumpHeight * movement.gravity);
	}

	public virtual bool IsJumping()
	{
		return jumping.jumping;
	}

	public virtual bool IsSliding()
	{
		bool num = grounded;
		if (num)
		{
			num = sliding.enabled;
		}
		if (num)
		{
			num = TooSteep();
		}
		return num;
	}

	public virtual bool IsTouchingCeiling()
	{
		return (movement.collisionFlags & CollisionFlags.Above) != 0;
	}

	public virtual bool IsGrounded()
	{
		return grounded;
	}

	public virtual bool TooSteep()
	{
		return !(groundNormal.y > Mathf.Cos(controller.slopeLimit * ((float)Math.PI / 180f)));
	}

	public virtual Vector3 GetDirection()
	{
		return inputMoveDirection;
	}

	public virtual void SetControllable(bool controllable)
	{
		canControl = controllable;
	}

	public virtual float MaxSpeedInDirection(Vector3 desiredMovementDirection)
	{
		float result;
		if (desiredMovementDirection == Vector3.zero)
		{
			result = 0f;
		}
		else
		{
			float num = ((desiredMovementDirection.z <= 0f) ? movement.maxBackwardsSpeed : movement.maxForwardSpeed) / movement.maxSidewaysSpeed;
			Vector3 normalized = new Vector3(desiredMovementDirection.x, 0f, desiredMovementDirection.z / num).normalized;
			float num2 = new Vector3(normalized.x, 0f, normalized.z * num).magnitude * movement.maxSidewaysSpeed;
			result = num2;
		}
		return result;
	}

	public virtual void SetVelocity(Vector3 velocity)
	{
		grounded = false;
		movement.velocity = velocity;
		movement.frameVelocity = Vector3.zero;
		SendMessage("OnExternalVelocity");
	}

	public virtual void Main()
	{
	}
}
[Serializable]
public class FPSinput : MonoBehaviour
{
	private FPScontroller motor;

	public virtual void Awake()
	{
		motor = (FPScontroller)GetComponent(typeof(FPScontroller));
	}

	public virtual void LateUpdate()
	{
		Vector3 vector = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
		if (vector != Vector3.zero)
		{
			float magnitude = vector.magnitude;
			vector /= magnitude;
			magnitude = Mathf.Min(1f, magnitude);
			magnitude *= magnitude;
			vector *= magnitude;
		}
		motor.inputMoveDirection = transform.rotation * vector;
		motor.inputJump = Input.GetKeyDown(KeyCode.Space);
		motor.inputRun = Input.GetKey(KeyCode.LeftShift);
		motor.inputCrouch = Input.GetKeyDown(KeyCode.C);
		motor.inputProne = Input.GetKeyDown(KeyCode.LeftControl);
	}

	public virtual void Main()
	{
	}
}
[Serializable]
[AddComponentMenu("FPS system/Ladder System/Attach to Ladder")]
public class Ladder : MonoBehaviour
{
	public GameObject ladderBottom;

	public GameObject ladderTop;

	private Vector3 climbDirection;

	public Ladder()
	{
		climbDirection = Vector3.zero;
	}

	public virtual void Start()
	{
		climbDirection = ladderTop.transform.position - ladderBottom.transform.position;
	}

	public virtual Vector3 ClimbDirection()
	{
		return climbDirection;
	}

	public virtual void Main()
	{
	}
}
[Serializable]
[RequireComponent(typeof(CharacterController))]
[AddComponentMenu("FPS system/Ladder System/Attach to Player")]
public class LadderPlayer : MonoBehaviour
{
	public float climbSpeed;

	public float climbDownThreshold;

	private Vector3 climbDirection;

	private Vector3 lateralMove;

	private Vector3 forwardMove;

	private Vector3 ladderMovement;

	private Ladder currentLadder;

	private bool latchedToLadder;

	private bool inLandingPad;

	private GameObject mainCamera;

	private CharacterController controller;

	private ArrayList landingPads;

	private bool trigger;

	public LadderPlayer()
	{
		climbSpeed = 6f;
		climbDownThreshold = -0.4f;
		climbDirection = Vector3.zero;
		lateralMove = Vector3.zero;
		forwardMove = Vector3.zero;
		ladderMovement = Vector3.zero;
	}

	public virtual void Start()
	{
		mainCamera = GameObject.FindWithTag("MainCamera");
		controller = (CharacterController)GetComponent(typeof(CharacterController));
		landingPads = new ArrayList();
	}

	public virtual void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Ladder")
		{
			LatchLadder(other.gameObject, other);
			trigger = true;
		}
	}

	public virtual void OnTriggerExit()
	{
	}

	public virtual void LatchLadder(GameObject latchedLadder, Collider collisionWaypoint)
	{
		currentLadder = (Ladder)latchedLadder.GetComponent(typeof(Ladder));
		latchedToLadder = true;
		climbDirection = currentLadder.ClimbDirection();
		gameObject.SendMessage("OnLadder", null, SendMessageOptions.RequireReceiver);
	}

	public virtual void UnlatchLadder()
	{
		latchedToLadder = false;
		currentLadder = null;
		gameObject.SendMessage("OffLadder", ladderMovement, SendMessageOptions.RequireReceiver);
	}

	public virtual void FixedUpdate()
	{
		if (!latchedToLadder)
		{
			return;
		}
		if (trigger)
		{
			RaycastCheck();
		}
		if (Input.GetButton("Jump"))
		{
			UnlatchLadder();
			return;
		}
		Vector3 normalized = climbDirection.normalized;
		normalized *= Input.GetAxis("Vertical");
		normalized *= (float)((!(mainCamera.transform.forward.y <= climbDownThreshold)) ? 1 : (-1));
		if (inLandingPad)
		{
			lateralMove = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
		}
		else
		{
			lateralMove = new Vector3(Input.GetAxis("Horizontal"), 0f, 0f);
		}
		lateralMove = transform.TransformDirection(lateralMove);
		ladderMovement = normalized + lateralMove;
		CollisionFlags collisionFlags = controller.Move(ladderMovement * climbSpeed * Time.deltaTime);
	}

	public virtual void RaycastCheck()
	{
		RaycastHit hitInfo = default(RaycastHit);
		CharacterController characterController = (CharacterController)GetComponent(typeof(CharacterController));
		Vector3 vector = transform.position + characterController.center + Vector3.up * ((0f - characterController.height) * 0.5f);
		Vector3 point = vector + Vector3.up * characterController.height;
		if (!Physics.CapsuleCast(vector, point, characterController.radius, transform.forward, out hitInfo, characterController.radius))
		{
			UnlatchLadder();
			trigger = false;
		}
	}

	public virtual void Main()
	{
	}
}
[Serializable]
public class WeaponSync(Catcher)JS : MonoBehaviour
{
	public GameObject thirdPersonWeapon;

	private WeaponScript weapScript;

	public virtual void Awake()
	{
		weapScript = gameObject.GetComponent<WeaponScript>();
	}

	public virtual void Fire()
	{
		if (!thirdPersonWeapon.active)
		{
			thirdPersonWeapon.active = true;
		}
		if (weapScript.GunType == WeaponScript.gunType.MACHINE_GUN)
		{
			thirdPersonWeapon.SendMessage("syncMachineGun", weapScript.errorAngle);
		}
		if (weapScript.GunType == WeaponScript.gunType.SHOTGUN)
		{
			thirdPersonWeapon.SendMessage("syncShotGun", weapScript.ShotGun.fractions);
		}
		if (weapScript.GunType == WeaponScript.gunType.GRENADE_LAUNCHER)
		{
			thirdPersonWeapon.SendMessage("syncGrenadeLauncher", weapScript.grenadeLauncher.initialSpeed);
		}
		if (weapScript.GunType == WeaponScript.gunType.KNIFE)
		{
			thirdPersonWeapon.SendMessage("syncKnife");
		}
	}

	public virtual void Main()
	{
	}
}
[Serializable]
public class EnableHelper : MonoBehaviour
{
	public FPScontroller fpsController;

	public WeaponManager weaponManager;

	public FPSMouseLook mouseLook1;

	public FPSMouseLook mouseLook2;

	private GameObject enablerReferenceObject;

	public virtual void Start()
	{
		enablerReferenceObject = GameObject.FindWithTag("EnableHelper").gameObject;
	}

	public virtual void Update()
	{
		if ((bool)enablerReferenceObject && enablerReferenceObject.active)
		{
			if (!fpsController.canControl)
			{
				fpsController.canControl = true;
			}
			if (!weaponManager.enabled && (bool)weaponManager.SelectedWeapon && !weaponManager.SelectedWeapon.enabled)
			{
				weaponManager.enabled = true;
				weaponManager.SelectedWeapon.enabled = true;
			}
			if (!mouseLook1.enabled && !mouseLook2.enabled)
			{
				mouseLook1.enabled = true;
				mouseLook2.enabled = true;
			}
		}
		if (!enablerReferenceObject || !enablerReferenceObject.active)
		{
			if (fpsController.canControl)
			{
				fpsController.canControl = false;
			}
			if (weaponManager.enabled && (bool)weaponManager.SelectedWeapon && weaponManager.SelectedWeapon.enabled)
			{
				weaponManager.SelectedWeapon.enabled = false;
				weaponManager.enabled = false;
			}
			if (mouseLook1.enabled && mouseLook2.enabled)
			{
				mouseLook1.enabled = false;
				mouseLook2.enabled = false;
			}
		}
		if (!enablerReferenceObject && GameObject.FindWithTag("EnableHelper").gameObject != null)
		{
			enablerReferenceObject = GameObject.FindWithTag("EnableHelper").gameObject;
		}
	}

	public virtual void Main()
	{
	}
}
[Serializable]
public class MainMenu : MonoBehaviour
{
	public GUISkin guiStyle;

	public string objective;

	public bool showTime;

	[HideInInspector]
	public bool finishedGame;

	private WeaponManager weaponManager;

	private bool startGame;

	private float timer;

	private bool mainMenu;

	private Resolution[] resolutions;

	private string[] QualityNames;

	private int resolutionIndex;

	private Vector2 scroll;

	private Vector2 scroll2;

	private Vector2 scroll3;

	private string niceTime;

	public MainMenu()
	{
		showTime = true;
		startGame = true;
		resolutionIndex = 3;
	}

	public virtual void Start()
	{
		weaponManager = GameObject.FindWithTag("WeaponManager").GetComponent<WeaponManager>();
		mainMenu = true;
		Invoke("Pause", 0.01f);
		resolutions = Screen.resolutions;
		resolutionIndex = (resolutions.Length - 1) / 2;
		QualityNames = QualitySettings.names;
	}

	public virtual void Update()
	{
		if (startGame && (bool)weaponManager.SelectedWeapon)
		{
			weaponManager.SelectedWeapon.gameObject.SetActiveRecursively(state: false);
		}
		if (!startGame)
		{
			if (!finishedGame)
			{
				timer += Time.deltaTime;
			}
			if (Input.GetKeyDown(KeyCode.Tab))
			{
				mainMenu = !mainMenu;
				Pause();
			}
			if (!mainMenu)
			{
				Screen.lockCursor = true;
			}
		}
		if (Input.GetKeyDown(KeyCode.P))
		{
			Screen.fullScreen = !Screen.fullScreen;
			if (!Screen.fullScreen)
			{
				Screen.SetResolution(resolutions[resolutionIndex].width, resolutions[resolutionIndex].height, fullscreen: true);
			}
		}
	}

	public virtual void OnGUI()
	{
		GUI.skin = guiStyle;
		float a = 0.7f;
		Color color = GUI.color;
		float num = (color.a = a);
		Color color2 = (GUI.color = color);
		int num2 = Mathf.FloorToInt(timer / 60f);
		int num3 = Mathf.FloorToInt(timer - (float)(num2 * 60));
		niceTime = $"{num2:0}:{num3:00}";
		if (showTime)
		{
			if (!finishedGame)
			{
				GUI.Box(new Rect(Screen.width / 2 - 50, 40f, 100f, 30f), niceTime);
			}
			else
			{
				GUI.Box(new Rect(Screen.width / 2 - 100, 40f, 200f, 30f), "Your Time | " + niceTime);
			}
		}
		if (startGame)
		{
			startGame = false;
			mainMenu = false;
			Pause();
			weaponManager.SelectedWeapon.gameObject.SetActiveRecursively(state: true);
			weaponManager.TakeFirstWeapon(weaponManager.SelectedWeapon.gameObject);
		}
	}

	public virtual void MainMenu(int windowID)
	{
		GUILayout.Space(10f);
		GUILayout.BeginHorizontal();
		GUILayout.Box(resolutions[resolutionIndex].width + " x " + resolutions[resolutionIndex].height, GUILayout.Width(150f), GUILayout.Height(20f));
		GUILayout.Box(QualityNames[QualitySettings.GetQualityLevel()], GUILayout.Width(150f), GUILayout.Height(20f));
		GUILayout.Space(15f);
		if (startGame)
		{
			if (GUILayout.Button("Start Game", GUILayout.Width(150f), GUILayout.Height(30f)))
			{
				startGame = false;
				mainMenu = false;
				Pause();
				weaponManager.SelectedWeapon.gameObject.SetActiveRecursively(state: true);
				weaponManager.TakeFirstWeapon(weaponManager.SelectedWeapon.gameObject);
			}
		}
		else
		{
			GUILayout.BeginVertical();
			if (GUILayout.Button("Restart Game", GUILayout.Width(150f), GUILayout.Height(30f)))
			{
				Time.timeScale = 1f;
				Application.LoadLevel(0);
			}
			GUILayout.EndVertical();
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(90f);
		GUI.color = new Color(0f, 20f, 0f, 0.6f);
		if (!finishedGame)
		{
			GUILayout.Label(objective);
		}
		else
		{
			GUILayout.Label("Objective: Completed with time: " + niceTime + " min");
		}
		GUILayout.Space(5f);
		GUI.color = Color.white;
		scroll3 = GUILayout.BeginScrollView(scroll3, GUILayout.Width(480f), GUILayout.Height(115f));
		GUI.color = new Color(20f, 20f, 0f, 0.6f);
		GUILayout.Label("Tab - Main Menu");
		GUILayout.Label("Q - slow motion");
		GUILayout.Label("P - Fullscreen");
		GUILayout.Label("C - crouch");
		GUILayout.Label("Left Ctrl - prone");
		GUILayout.Label("LMB - fire");
		GUILayout.Label("RMB - aim");
		GUILayout.Label("F - weapon pick up");
		GUILayout.Label("R - reload");
		GUILayout.Label("Left Shift - run");
		GUILayout.Label("Space - jump");
		GUILayout.Label("1/2 - weapon change");
		GUILayout.Label("While selected STW-25 press G for flashlight");
		GUILayout.EndScrollView();
	}

	public virtual void Resolutions(int windowID)
	{
		GUI.BringWindowToFront(windowID);
		scroll = GUILayout.BeginScrollView(scroll, GUILayout.Width(140f), GUILayout.Height(75f));
		GUILayout.BeginVertical();
		for (int i = default(int); i < resolutions.Length; i++)
		{
			if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
			{
				GUI.color = new Color(0f, 20f, 20f, 0.6f);
			}
			else
			{
				GUI.color = new Color(20f, 20f, 20f, 0.6f);
			}
			if (GUILayout.Button(resolutions[i].width + " x " + resolutions[i].height))
			{
				resolutionIndex = i;
				if (Screen.fullScreen)
				{
					Screen.SetResolution(resolutions[resolutionIndex].width, resolutions[resolutionIndex].height, fullscreen: true);
				}
			}
		}
		GUILayout.EndVertical();
		GUILayout.EndScrollView();
	}

	public virtual void QualityWindow(int windowID)
	{
		GUI.BringWindowToFront(windowID);
		scroll2 = GUILayout.BeginScrollView(scroll2, GUILayout.Width(140f), GUILayout.Height(75f));
		GUILayout.BeginVertical();
		for (int i = 0; i < QualityNames.Length; i++)
		{
			if (QualityNames[i] == QualityNames[QualitySettings.GetQualityLevel()])
			{
				GUI.color = new Color(0f, 20f, 20f, 0.6f);
			}
			else
			{
				GUI.color = new Color(20f, 20f, 20f, 0.6f);
			}
			if (GUILayout.Button(QualityNames[i]))
			{
				QualitySettings.SetQualityLevel(i, applyExpensiveChanges: true);
			}
		}
		GUILayout.EndVertical();
		GUILayout.EndScrollView();
	}

	public virtual void Pause()
	{
		if (mainMenu)
		{
			Time.timeScale = 0.0001f;
			Screen.lockCursor = false;
		}
		else
		{
			Time.timeScale = 1f;
			Screen.lockCursor = true;
		}
	}

	public virtual void Main()
	{
	}
}
[Serializable]
public class SlowMotionEffect : MonoBehaviour
{
	public bool slowMotion;

	public GUISkin guiSkin;

	public float slowTimeTo;

	[HideInInspector]
	public AudioSource[] audios;

	public SlowMotionEffect()
	{
		slowTimeTo = 0.5f;
	}

	public virtual void Start()
	{
	}

	public virtual void Update()
	{
		if (!(Time.timeScale >= 0.01f))
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.Q))
		{
			slowMotion = !slowMotion;
		}
		if (slowMotion)
		{
			audios = ((AudioSource[])UnityEngine.Object.FindObjectsOfType(typeof(AudioSource))) as AudioSource[];
			for (int i = 0; i < audios.Length; i++)
			{
				audios[i].pitch = slowTimeTo;
			}
			Time.timeScale = slowTimeTo;
			Time.fixedDeltaTime = 0.005f;
		}
		else if (!slowMotion && Time.deltaTime != 1f)
		{
			audios = ((AudioSource[])UnityEngine.Object.FindObjectsOfType(typeof(AudioSource))) as AudioSource[];
			for (int j = 0; j < audios.Length; j++)
			{
				audios[j].pitch = 1f;
			}
			Time.timeScale = 1f;
			Time.fixedDeltaTime = 0.02f;
		}
	}

	public virtual void OnGUI()
	{
	}

	public virtual void Main()
	{
	}
}
[Serializable]
public class TriggerActivate : MonoBehaviour
{
	public GUISkin guiStyle;

	public GameObject weaps;

	public Transform teleportPoint;

	public MainMenu mm;

	private bool inside;

	public virtual void OnTriggerEnter(Collider weapon)
	{
		if (weapon.gameObject.tag == "Player")
		{
			inside = true;
		}
	}

	public virtual void OnTriggerExit(Collider weapon)
	{
		if (weapon.gameObject.tag == "Player")
		{
			inside = false;
		}
	}

	public virtual void Update()
	{
		if (inside && Input.GetKeyDown(KeyCode.F))
		{
			GameObject.FindWithTag("Player").transform.position = teleportPoint.position;
			GameObject.FindWithTag("Player").transform.rotation = teleportPoint.rotation;
			inside = false;
			weaps.SetActiveRecursively(state: true);
			mm.finishedGame = true;
			RenderSettings.fog = false;
			UnityEngine.Object.Destroy(gameObject);
		}
	}

	public virtual void OnGUI()
	{
		if (inside)
		{
			GUI.skin = guiStyle;
			float a = 0.9f;
			Color color = GUI.color;
			float num = (color.a = a);
			Color color2 = (GUI.color = color);
			GUI.depth = -10;
			string text = "Press \u00b4F\u00b4 for MORE GUNS!        ";
			Rect position = new Rect(Screen.width / 2 - text.Length * 9 / 2, Screen.height / 2 - 25, text.Length * 9, 50f);
			GUI.Box(position, text);
		}
	}

	public virtual void Main()
	{
	}
}
[Serializable]
public class AmmoDeactivator : MonoBehaviour
{
	public System.Collections.Generic.List<GameObject> objectsToDeactivate;

	private WeaponScript weapScript;

	public virtual void Start()
	{
		weapScript = gameObject.GetComponent<WeaponScript>();
	}

	public virtual void Update()
	{
		if (weapScript.GunType != WeaponScript.gunType.GRENADE_LAUNCHER)
		{
			return;
		}
		for (int i = 0; i < objectsToDeactivate.Count; i++)
		{
			if (weapScript.grenadeLauncher.ammoCount == 0)
			{
				objectsToDeactivate[i].SetActiveRecursively(state: false);
			}
			else
			{
				objectsToDeactivate[i].SetActiveRecursively(state: true);
			}
		}
	}

	public virtual void Main()
	{
	}
}
[Serializable]
public class AmmoDisplay : MonoBehaviour
{
	public GUISkin guiStyle;

	public bool display;

	private int bulletsLeft;

	private int clips;

	private WeaponScript weaponscript;

	private WeaponManager weaponManager;

	private WeaponScript currentWeapon;

	private float color;

	private GameObject NG_UI;

	public AmmoDisplay()
	{
		display = true;
	}

	public virtual void Awake()
	{
		weaponManager = GameObject.FindWithTag("WeaponManager").GetComponent<WeaponManager>();
		NG_UI = GameObject.Find("UI Root (3D)");
	}

	public virtual void Update()
	{
		if ((bool)weaponManager.SelectedWeapon)
		{
			weaponscript = weaponManager.SelectedWeapon.GetComponent<WeaponScript>();
		}
		if (!weaponscript)
		{
			return;
		}
		if (weaponscript.GunType == WeaponScript.gunType.MACHINE_GUN)
		{
			bulletsLeft = weaponscript.machineGun.bulletsLeft;
			clips = weaponscript.machineGun.clips;
		}
		if (weaponscript.GunType == WeaponScript.gunType.SHOTGUN)
		{
			bulletsLeft = weaponscript.ShotGun.bulletsLeft;
			clips = weaponscript.ShotGun.clips;
		}
		if (weaponscript.GunType == WeaponScript.gunType.GRENADE_LAUNCHER)
		{
			clips = weaponscript.grenadeLauncher.ammoCount;
		}
		if (currentWeapon != weaponManager.SelectedWeapon)
		{
			color = Mathf.Lerp(color, 0.3f, Time.deltaTime * 20f);
			if (!(color >= 0.32f))
			{
				currentWeapon = weaponManager.SelectedWeapon;
			}
		}
		if (!weaponscript)
		{
			return;
		}
		if (weaponscript.GunType != WeaponScript.gunType.KNIFE)
		{
			if (weaponscript.GunType == WeaponScript.gunType.GRENADE_LAUNCHER)
			{
				NG_UI.SendMessage("receiveBullets", clips.ToString(), SendMessageOptions.DontRequireReceiver);
			}
			else if (weaponscript.weaponName == "Deagle" || weaponscript.weaponName == "GLOCK21")
			{
				NG_UI.SendMessage("receiveBullets", bulletsLeft + " | NA", SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				NG_UI.SendMessage("receiveBullets", bulletsLeft + " | " + clips, SendMessageOptions.DontRequireReceiver);
			}
		}
		else
		{
			NG_UI.SendMessage("receiveBullets", "NA", SendMessageOptions.DontRequireReceiver);
		}
	}

	public virtual void Main()
	{
	}
}
[Serializable]
[AddComponentMenu("FPS system/Weapon System/Bullet Controller")]
public class Bullet : MonoBehaviour
{
	public int speed;

	public float life;

	public int damage;

	public int impactForce;

	public bool impactHoles;

	public bool knifeHoles;

	public bool doDamage;

	public System.Collections.Generic.List<GameObject> impactObjects;

	private Vector3 velocity;

	private Vector3 newPos;

	private Vector3 oldPos;

	private bool hasHit;

	public Transform bloodParticleEffect;

	public string onlinePlayerTag;

	public float bulletDamage;

	public string shooter;

	public Bullet()
	{
		speed = 500;
		life = 3f;
		damage = 20;
		impactForce = 10;
		impactHoles = true;
		knifeHoles = true;
		onlinePlayerTag = "null";
		bulletDamage = 1f;
		shooter = string.Empty;
	}

	public virtual void SetMyTag(string id)
	{
		onlinePlayerTag = id;
	}

	public virtual void Start()
	{
		newPos = transform.position;
		oldPos = newPos;
		velocity = speed * transform.forward;
		UnityEngine.Object.Destroy(gameObject, life);
	}

	public virtual void Update()
	{
		if (hasHit)
		{
			return;
		}
		newPos += velocity * Time.deltaTime * 10f;
		Vector3 direction = newPos - oldPos;
		float magnitude = direction.magnitude;
		if (!(magnitude <= 0f))
		{
			RaycastHit hitInfo = default(RaycastHit);
			if (Physics.Raycast(oldPos, direction, out hitInfo, magnitude, 19))
			{
				newPos = hitInfo.point;
				hasHit = true;
				Quaternion quaternion = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
				if ((bool)hitInfo.rigidbody)
				{
					hitInfo.rigidbody.AddForce(transform.forward * impactForce, ForceMode.Impulse);
				}
				if (PlayerPrefs.GetInt("GameQualityLevel", 3) == 3 && impactHoles)
				{
					if (hitInfo.transform.tag == "City")
					{
						if (impactObjects.Count != 0)
						{
							UnityEngine.Object.Instantiate(impactObjects[0], hitInfo.point, quaternion);
						}
					}
					else if ((hitInfo.transform.tag == "EnemyTag" || hitInfo.transform.tag == "EnemyHeadTag" || hitInfo.transform.tag == "EnemyBodyTag" || hitInfo.transform.tag == "EnemyFootTag") && impactObjects.Count != 0)
					{
						UnityEngine.Object.Instantiate(impactObjects[1], hitInfo.point, quaternion);
					}
				}
				if (knifeHoles)
				{
					if (hitInfo.transform.tag == "City")
					{
						UnityEngine.Object.Instantiate(impactObjects[0], hitInfo.point, quaternion * Quaternion.Euler(0f, 90f, 0f));
					}
					else if (hitInfo.transform.tag == "EnemyTag" || hitInfo.transform.tag == "EnemyHeadTag" || hitInfo.transform.tag == "EnemyBodyTag" || hitInfo.transform.tag == "EnemyFootTag")
					{
						UnityEngine.Object.Instantiate(impactObjects[1], hitInfo.point, quaternion);
					}
				}
				if (hitInfo.transform.tag == "EnemyTag")
				{
					if (shooter == "player")
					{
						hitInfo.transform.SendMessageUpwards("decreaseBlood", bulletDamage, SendMessageOptions.DontRequireReceiver);
					}
					hitInfo.transform.SendMessageUpwards("setTargetIsPlayer", true, SendMessageOptions.DontRequireReceiver);
					if (onlinePlayerTag == string.Empty)
					{
						hitInfo.transform.SendMessage("OnDamaged", bulletDamage, SendMessageOptions.DontRequireReceiver);
					}
				}
				else if (hitInfo.transform.tag == "EnemyHeadTag")
				{
					if (onlinePlayerTag == string.Empty)
					{
						hitInfo.transform.SendMessageUpwards("OnDamaged", 1000, SendMessageOptions.DontRequireReceiver);
					}
				}
				else if (hitInfo.transform.tag == "EnemyBodyTag")
				{
					if (onlinePlayerTag == string.Empty)
					{
						hitInfo.transform.SendMessageUpwards("OnDamaged", bulletDamage, SendMessageOptions.DontRequireReceiver);
					}
				}
				else if (hitInfo.transform.tag == "EnemyFootTag")
				{
					if (onlinePlayerTag == string.Empty)
					{
						hitInfo.transform.SendMessageUpwards("OnDamaged", bulletDamage * 0.7f, SendMessageOptions.DontRequireReceiver);
					}
				}
				else if (hitInfo.transform.tag == "Player")
				{
					hitInfo.transform.SendMessage("PlayerDamage", bulletDamage, SendMessageOptions.DontRequireReceiver);
				}
				else
				{
					hitInfo.transform.SendMessageUpwards("setTargetIsPlayer", false, SendMessageOptions.DontRequireReceiver);
				}
				UnityEngine.Object.Destroy(gameObject, 1f);
			}
		}
		oldPos = transform.position;
		transform.position = newPos;
	}

	public virtual void Main()
	{
	}
}
[Serializable]
[AddComponentMenu("FPS system/Character/FPS CameraBob")]
public class CameraBob : MonoBehaviour
{
	public float walkBobbingSpeed;

	public float runBobbingSpeed;

	public float idleBobbingSpeed;

	public float bobbingAmount;

	public float smooth;

	private Vector3 midpoint;

	private GameObject player;

	private float timer;

	private float bobbingSpeed;

	private FPScontroller motor;

	private float BobbingAmount;

	public CameraBob()
	{
		walkBobbingSpeed = 0.21f;
		runBobbingSpeed = 0.35f;
		idleBobbingSpeed = 0.1f;
		bobbingAmount = 0.1f;
		smooth = 1f;
	}

	public virtual void Awake()
	{
		player = GameObject.FindWithTag("Player");
		motor = (FPScontroller)player.GetComponent(typeof(FPScontroller));
		midpoint = transform.localPosition;
	}

	public virtual void FixedUpdate()
	{
		if (motor.prone)
		{
			return;
		}
		float num = default(float);
		float num2 = default(float);
		float num3 = default(float);
		if (Time.timeScale == 1f)
		{
			if (num != walkBobbingSpeed || num2 != runBobbingSpeed || num3 != idleBobbingSpeed)
			{
				num = walkBobbingSpeed;
				num2 = runBobbingSpeed;
				num3 = idleBobbingSpeed;
			}
		}
		else
		{
			num = walkBobbingSpeed * (Time.fixedDeltaTime / 0.02f);
			num2 = runBobbingSpeed * (Time.fixedDeltaTime / 0.02f);
			num3 = idleBobbingSpeed * (Time.fixedDeltaTime / 0.02f);
		}
		float num4 = 0f;
		float num5 = 0f;
		Vector3 to = default(Vector3);
		num4 = Mathf.Sin(timer * 2f);
		num5 = Mathf.Sin(timer);
		timer += bobbingSpeed;
		if (!(timer <= (float)Math.PI * 2f))
		{
			timer -= (float)Math.PI * 2f;
		}
		if (num4 != 0f)
		{
			float num6 = num4 * BobbingAmount;
			float num7 = num5 * BobbingAmount;
			float num8 = Mathf.Clamp(1f, 0f, 1f);
			float num9 = num8 * num6;
			float num10 = num8 * num7;
			if (motor.grounded)
			{
				to.y = midpoint.y + num9;
				to.x = midpoint.x + num10;
			}
		}
		else
		{
			to = midpoint;
		}
		if (motor.Walking && !motor.Running)
		{
			bobbingSpeed = num;
			BobbingAmount = bobbingAmount;
		}
		else if (motor.Running)
		{
			bobbingSpeed = num2;
			BobbingAmount = bobbingAmount;
		}
		if (!motor.Running && !motor.Walking)
		{
			bobbingSpeed = num3;
			BobbingAmount = bobbingAmount * 0.3f;
		}
		float t = default(float) + Time.deltaTime * smooth;
		transform.localPosition = Vector3.Lerp(transform.localPosition, to, t);
	}

	public virtual void Main()
	{
	}
}
[Serializable]
public class ExplosionDamage : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	internal sealed class $Start$161 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class $ : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal Vector3 $explosionPosition$162;

			internal Collider[] $colliders$163;

			internal Collider $hit$164;

			internal Vector3 $closestPoint$165;

			internal float $distance$166;

			internal float $hitPoints$167;

			internal Collider $hit$168;

			internal int $$112$169;

			internal Collider[] $$113$170;

			internal int $$114$171;

			internal int $$116$172;

			internal Collider[] $$117$173;

			internal int $$118$174;

			internal ExplosionDamage $self_$175;

			public $(ExplosionDamage self_)
			{
				$self_$175 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					$explosionPosition$162 = $self_$175.transform.position;
					$colliders$163 = Physics.OverlapSphere($explosionPosition$162, $self_$175.explosionRadius);
					$$112$169 = 0;
					$$113$170 = $colliders$163;
					for ($$114$171 = $$113$170.Length; $$112$169 < $$114$171; $$112$169++)
					{
						$closestPoint$165 = $$113$170[$$112$169].ClosestPointOnBounds($explosionPosition$162);
						$distance$166 = Vector3.Distance($closestPoint$165, $explosionPosition$162);
						$hitPoints$167 = 1f - Mathf.Clamp01($distance$166 / $self_$175.explosionRadius);
						$hitPoints$167 *= $self_$175.explosionDamage;
						$$113$170[$$112$169].SendMessageUpwards("ApplyDamage", $hitPoints$167, SendMessageOptions.DontRequireReceiver);
					}
					$colliders$163 = Physics.OverlapSphere($explosionPosition$162, $self_$175.explosionRadius);
					$$116$172 = 0;
					$$117$173 = $colliders$163;
					for ($$118$174 = $$117$173.Length; $$116$172 < $$118$174; $$116$172++)
					{
						if ((bool)$$117$173[$$116$172].rigidbody)
						{
							$$117$173[$$116$172].rigidbody.AddExplosionForce($self_$175.explosionPower, $explosionPosition$162, $self_$175.explosionRadius, 3f);
						}
					}
					if ((bool)$self_$175.particleEmitter)
					{
						$self_$175.particleEmitter.emit = true;
						result = (Yield(2, new WaitForSeconds(0.5f)) ? 1 : 0);
						break;
					}
					goto IL_0229;
				case 2:
					$self_$175.particleEmitter.emit = false;
					goto IL_0229;
				case 1:
					{
						result = 0;
						break;
					}
					IL_0229:
					UnityEngine.Object.Destroy($self_$175.gameObject, $self_$175.explosionTimeout);
					YieldDefault(1);
					goto case 1;
				}
				return (byte)result != 0;
			}
		}

		internal ExplosionDamage $self_$176;

		public $Start$161(ExplosionDamage self_)
		{
			$self_$176 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new $($self_$176);
		}
	}

	public float explosionRadius;

	public float explosionPower;

	public float explosionDamage;

	public float explosionTimeout;

	public Type player1;

	public ExplosionDamage()
	{
		explosionRadius = 5f;
		explosionPower = 10f;
		explosionDamage = 100f;
		explosionTimeout = 2f;
		player1 = typeof(GameObject);
	}

	public virtual IEnumerator Start()
	{
		return new $Start$161(this).GetEnumerator();
	}

	public virtual void Main()
	{
	}
}
[Serializable]
public class Flashlight : MonoBehaviour
{
	public bool turnOn;

	public Light flashLight;

	public AudioClip OnOffAudio;

	public virtual void Start()
	{
		if (turnOn)
		{
			flashLight.enabled = true;
		}
		else
		{
			flashLight.enabled = false;
		}
	}

	public virtual void Update()
	{
		if (Input.GetKeyDown(KeyCode.G))
		{
			turnOn = !turnOn;
			flashLightOnOff();
		}
	}

	public virtual void flashLightOnOff()
	{
		audio.clip = OnOffAudio;
		audio.Play();
		if (turnOn)
		{
			flashLight.enabled = true;
		}
		else
		{
			flashLight.enabled = false;
		}
	}

	public virtual void Main()
	{
	}
}
[Serializable]
public class LaserEffectForPlayer : MonoBehaviour
{
	private int speed;

	private float life;

	private Vector3 velocity;

	private Vector3 newPos;

	private Vector3 oldPos;

	private bool hasHit;

	public LaserEffectForPlayer()
	{
		speed = 50;
		life = 0.1f;
	}

	public virtual void Start()
	{
		newPos = transform.position;
		oldPos = newPos;
		velocity = speed * transform.forward;
		UnityEngine.Object.Destroy(gameObject, life);
	}

	public virtual void Update()
	{
		if (!hasHit)
		{
			newPos += velocity * Time.deltaTime * 10f;
			float magnitude = (newPos - oldPos).magnitude;
			oldPos = transform.position;
			transform.position = newPos;
		}
	}

	public virtual void Main()
	{
	}
}
[Serializable]
public class PorcCollider : MonoBehaviour
{
	private int bulletsLeft;

	private int clips;

	private WeaponScript weaponscript;

	private WeaponManager weaponManager;

	private WeaponScript currentWeapon;

	private float color;

	private GameObject AudioProcX;

	public virtual void Awake()
	{
		weaponManager = GameObject.FindWithTag("WeaponManager").GetComponent<WeaponManager>();
		AudioProcX = transform.FindChild("Audio").gameObject;
	}

	public virtual void OnTriggerEnter(Collider other)
	{
		if ((bool)weaponManager.SelectedWeapon)
		{
			weaponscript = weaponManager.SelectedWeapon.GetComponent<WeaponScript>();
		}
		if ((bool)weaponscript && other.gameObject.tag == "Player")
		{
			if (AudioProcX != null)
			{
				AudioProcX.audio.Play();
			}
			switch (weaponscript.weaponName)
			{
			case "Deagle":
				weaponscript.machineGun.clips = weaponscript.machineGun.clips + 10;
				weaponscript.isReload = false;
				break;
			case "G36K":
				weaponscript.machineGun.clips = weaponscript.machineGun.clips + 40;
				weaponscript.isReload = false;
				break;
			case "GLOCK21":
				weaponscript.machineGun.clips = weaponscript.machineGun.clips + 20;
				weaponscript.isReload = false;
				break;
			case "M67":
				weaponscript.grenadeLauncher.ammoCount = weaponscript.grenadeLauncher.ammoCount + 5;
				weaponscript.isReload = false;
				break;
			case "M87T":
				weaponscript.ShotGun.clips = weaponscript.ShotGun.clips + 10;
				weaponscript.isReload = false;
				break;
			case "MP5KA4":
				weaponscript.machineGun.clips = weaponscript.machineGun.clips + 40;
				weaponscript.isReload = false;
				break;
			case "MP5KA5":
				weaponscript.machineGun.clips = weaponscript.machineGun.clips + 50;
				weaponscript.isReload = false;
				break;
			case "RPG":
				weaponscript.grenadeLauncher.ammoCount = weaponscript.grenadeLauncher.ammoCount + 5;
				weaponscript.isReload = false;
				break;
			case "Blaser R93":
				weaponscript.machineGun.clips = weaponscript.machineGun.clips + 5;
				weaponscript.isReload = false;
				break;
			case "STW-25":
				weaponscript.machineGun.clips = weaponscript.machineGun.clips + 50;
				weaponscript.isReload = false;
				break;
			case "UZI":
				weaponscript.machineGun.clips = weaponscript.machineGun.clips + 60;
				weaponscript.isReload = false;
				break;
			case "M249":
				weaponscript.machineGun.clips = weaponscript.machineGun.clips + 100;
				weaponscript.isReload = false;
				break;
			case "MilkBomb":
				weaponscript.grenadeLauncher.ammoCount = weaponscript.grenadeLauncher.ammoCount + 5;
				weaponscript.isReload = false;
				break;
			case "CandyRifle":
				weaponscript.machineGun.clips = weaponscript.machineGun.clips + 50;
				weaponscript.isReload = false;
				break;
			case "ChristmasSniper":
				weaponscript.machineGun.clips = weaponscript.machineGun.clips + 50;
				weaponscript.isReload = false;
				break;
			case "SantaGun":
				weaponscript.machineGun.clips = weaponscript.machineGun.clips + 50;
				weaponscript.isReload = false;
				break;
			case "GingerbreadBomb":
				weaponscript.grenadeLauncher.ammoCount = weaponscript.grenadeLauncher.ammoCount + 5;
				weaponscript.isReload = false;
				break;
			case "AUG":
				weaponscript.machineGun.clips = weaponscript.machineGun.clips + 40;
				weaponscript.isReload = false;
				break;
			case "M3":
				weaponscript.ShotGun.clips = weaponscript.ShotGun.clips + 7;
				weaponscript.isReload = false;
				break;
			}
			transform.position -= new Vector3(0f, -20f, 0f);
		}
	}

	public virtual void Update()
	{
	}

	public virtual void Main()
	{
	}
}
[Serializable]
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
	public GameObject explosion;

	public float destroyDelay;

	public float timeOut;

	public GameObject[] objectsToDestroy;

	public ContactPoint contact;

	private Quaternion rotation;

	public string onlinePlayerTag;

	public string shooter;

	private int killCreateCount;

	public Projectile()
	{
		timeOut = 3f;
		onlinePlayerTag = "null";
		shooter = string.Empty;
	}

	public virtual void SetMyTag(string id)
	{
		onlinePlayerTag = id;
	}

	public virtual void SetShooter(string id)
	{
		shooter = id;
	}

	public virtual void Start()
	{
		if (!(destroyDelay <= 0f))
		{
			Invoke("Kill", destroyDelay);
		}
		else
		{
			Invoke("Kill", timeOut);
		}
	}

	public virtual void FixedUpdate()
	{
		if (rigidbody.velocity != Vector3.zero)
		{
			transform.rotation = Quaternion.LookRotation(rigidbody.velocity);
		}
		else
		{
			transform.rotation = Quaternion.Euler(Vector3.zero);
		}
	}

	public virtual void OnCollisionEnter(Collision collision)
	{
		contact = collision.contacts[0];
		rotation = Quaternion.FromToRotation(Vector3.up, contact.normal);
		if (destroyDelay <= 0f && killCreateCount == 0)
		{
			Kill();
			killCreateCount = 1;
		}
	}

	public virtual void Kill()
	{
		GameObject gameObject = ((GameObject)UnityEngine.Object.Instantiate(explosion, transform.position, rotation)) as GameObject;
		gameObject.SendMessage("SetDetonatorShooter", shooter, SendMessageOptions.DontRequireReceiver);
		gameObject.SendMessage("SetMyTag", onlinePlayerTag, SendMessageOptions.DontRequireReceiver);
		ParticleEmitter particleEmitter = (ParticleEmitter)GetComponentInChildren(typeof(ParticleEmitter));
		if ((bool)particleEmitter)
		{
			particleEmitter.emit = false;
		}
		transform.DetachChildren();
		UnityEngine.Object.Destroy(this.gameObject);
		if (objectsToDestroy.Length > 0)
		{
			for (int i = 0; i < objectsToDestroy.Length; i++)
			{
				UnityEngine.Object.Destroy(objectsToDestroy[i]);
			}
		}
	}

	public virtual void Main()
	{
	}
}
[Serializable]
[AddComponentMenu("FPS system/Weapon System/SniperAnimation")]
public class SniperAnimation : MonoBehaviour
{
	public string Idle;

	public string ReloadBegin;

	public string ReloadMiddle;

	public string ReloadEnd;

	public string Shoot;

	public string TakeIn;

	public string TakeOut;

	public float FireAnimationSpeed;

	public float TakeInOutSpeed;

	public float ReloadMiddleRepeat;

	private string PlayThis;

	private FPScontroller motor;

	private GameObject player;

	public SniperAnimation()
	{
		Idle = "Idle";
		ReloadBegin = "Reload_1_3";
		ReloadMiddle = "Reload_2_3";
		ReloadEnd = "Reload_3_3";
		Shoot = "Fire";
		TakeIn = "TakeIn";
		TakeOut = "TakeOut";
		FireAnimationSpeed = 1f;
		TakeInOutSpeed = 1f;
		ReloadMiddleRepeat = 4f;
	}

	public virtual void Awake()
	{
		animation.Play(Idle);
		animation[Idle].wrapMode = WrapMode.Once;
		animation[ReloadBegin].wrapMode = WrapMode.Once;
		animation[ReloadMiddle].wrapMode = WrapMode.Once;
		animation[ReloadEnd].wrapMode = WrapMode.Once;
		animation[Shoot].wrapMode = WrapMode.Once;
		animation[TakeIn].wrapMode = WrapMode.Once;
		animation[TakeOut].wrapMode = WrapMode.Once;
	}

	public virtual void Fire()
	{
		animation.Rewind(Shoot);
		animation[Shoot].speed = FireAnimationSpeed;
		animation.Play(Shoot);
	}

	public virtual void Reloading(float reloadTime)
	{
		float num = animation[ReloadBegin].clip.length + animation[ReloadMiddle].clip.length * ReloadMiddleRepeat + animation[ReloadEnd].clip.length;
		AnimationState animationState = animation.CrossFadeQueued(ReloadBegin);
		animationState.speed = num / reloadTime / 2f;
		for (int i = 0; (float)i < ReloadMiddleRepeat; i++)
		{
			AnimationState animationState2 = animation.CrossFadeQueued(ReloadMiddle);
			animationState2.speed = num / reloadTime / 1.4f;
		}
		AnimationState animationState3 = animation.CrossFadeQueued(ReloadEnd);
		animationState3.speed = num / reloadTime / 2f;
	}

	public virtual void takeIn()
	{
		animation.Rewind(TakeIn);
		animation[TakeIn].speed = TakeInOutSpeed;
		animation[TakeIn].time = 0f;
		animation.Play(TakeIn);
	}

	public virtual void takeOut()
	{
		animation.Rewind(TakeOut);
		animation[TakeOut].speed = TakeInOutSpeed;
		animation[TakeOut].time = 0f;
		animation.Play(TakeOut);
	}

	public virtual void Main()
	{
	}
}
[Serializable]
[AddComponentMenu("FPS system/Weapon System/SniperScope")]
public class SniperScope : MonoBehaviour
{
	public Texture2D scopeTexture;

	public GameObject[] objectsToDeactivate;

	private WeaponScript weapScript;

	public virtual void Awake()
	{
		weapScript = gameObject.GetComponent<WeaponScript>();
	}

	public virtual void OnGUI()
	{
		if (weapScript.aimed)
		{
			GUI.DrawTexture(new Rect((float)(Screen.width / 2) - (float)Screen.height * 1.8f / 2f, Screen.height / 2 - Screen.height / 2, (float)Screen.height * 1.8f, Screen.height), scopeTexture);
			for (int i = 0; i < objectsToDeactivate.Length; i++)
			{
				objectsToDeactivate[i].SetActiveRecursively(state: false);
			}
		}
		else
		{
			for (int j = 0; j < objectsToDeactivate.Length; j++)
			{
				objectsToDeactivate[j].SetActiveRecursively(state: true);
			}
		}
	}

	public virtual void Main()
	{
	}
}
[Serializable]
public class WaitForDestroy : MonoBehaviour
{
	public float lifeTime;

	public WaitForDestroy()
	{
		lifeTime = 2f;
	}

	public virtual void Awake()
	{
		UnityEngine.Object.Destroy(gameObject, lifeTime);
	}

	public virtual void Main()
	{
	}
}
[Serializable]
[AddComponentMenu("FPS system/Character/FPS WalkSway")]
public class WalkSway : MonoBehaviour
{
	public float walkBobbingSpeed;

	public float runBobbingSpeed;

	public float idleBobbingSpeed;

	public float bobbingAmount;

	public float smooth;

	private Vector3 midpoint;

	private GameObject player;

	private float timer;

	private float bobbingSpeed;

	private float BobbingAmount;

	public WalkSway()
	{
		walkBobbingSpeed = 0.14f;
		runBobbingSpeed = 0.35f;
		idleBobbingSpeed = 0.1f;
		bobbingAmount = 0.06f;
		smooth = 1f;
	}

	public virtual void Awake()
	{
		player = GameObject.FindWithTag("Player");
		midpoint = transform.localPosition;
	}

	public virtual void FixedUpdate()
	{
		float num = 0f;
		float num2 = 0f;
		Vector3 to = default(Vector3);
		float num3 = default(float);
		float num4 = default(float);
		float num5 = default(float);
		if (Time.timeScale == 1f)
		{
			if (num3 != walkBobbingSpeed || num4 != runBobbingSpeed || num5 != idleBobbingSpeed)
			{
				num3 = walkBobbingSpeed;
				num4 = runBobbingSpeed;
				num5 = idleBobbingSpeed;
			}
		}
		else
		{
			num3 = walkBobbingSpeed * (Time.fixedDeltaTime / 0.02f);
			num4 = runBobbingSpeed * (Time.fixedDeltaTime / 0.02f);
			num5 = idleBobbingSpeed * (Time.fixedDeltaTime / 0.02f);
		}
		num = Mathf.Sin(timer * 2f);
		num2 = Mathf.Sin(timer);
		timer += bobbingSpeed;
		if (!(timer <= (float)Math.PI * 2f))
		{
			timer -= (float)Math.PI * 2f;
		}
		if (num != 0f)
		{
			float num6 = num * BobbingAmount;
			float num7 = num2 * BobbingAmount;
			float num8 = Mathf.Clamp(1f, 0f, 1f);
			float num9 = num8 * num6;
			float num10 = num8 * num7;
			to.y = midpoint.y + num9;
			to.x = midpoint.x + num10;
		}
		else
		{
			to = midpoint;
		}
		if (PlayerPrefs.GetInt("moveStatus", 0) == 1)
		{
			bobbingSpeed = num3;
			BobbingAmount = bobbingAmount * 0.7f;
		}
		if (PlayerPrefs.GetInt("moveStatus", 0) == 0)
		{
			bobbingSpeed = num5;
			BobbingAmount = bobbingAmount * 0.3f;
		}
		float t = default(float) + Time.deltaTime * smooth;
		transform.localPosition = Vector3.Lerp(transform.localPosition, to, t);
	}

	public virtual void Main()
	{
	}
}
[Serializable]
[AddComponentMenu("FPS system/Weapon System/WeaponAnimation")]
public class WeaponAnimation : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	internal sealed class $takeIn$177 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class $ : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal WeaponAnimation $self_$178;

			public $(WeaponAnimation self_)
			{
				$self_$178 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					$self_$178.animation.Rewind($self_$178.TakeIn);
					$self_$178.animation[$self_$178.TakeIn].speed = $self_$178.TakeInOutSpeed;
					$self_$178.animation[$self_$178.TakeIn].time = 0f;
					$self_$178.animation.Play($self_$178.TakeIn);
					$self_$178.isPlayingTakeInAnimation = true;
					result = (Yield(2, new WaitForSeconds(0.29f)) ? 1 : 0);
					break;
				case 2:
					$self_$178.isPlayingTakeInAnimation = false;
					YieldDefault(1);
					goto case 1;
				case 1:
					result = 0;
					break;
				}
				return (byte)result != 0;
			}
		}

		internal WeaponAnimation $self_$179;

		public $takeIn$177(WeaponAnimation self_)
		{
			$self_$179 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new $($self_$179);
		}
	}

	public string Idle;

	public string Reload;

	public string Shoot;

	public string TakeIn;

	public string TakeOut;

	public float FireAnimationSpeed;

	public float TakeInOutSpeed;

	private string PlayThis;

	private GameObject player;

	private bool isPlayingTakeInAnimation;

	public WeaponAnimation()
	{
		Idle = "Idle";
		Reload = "Reload";
		Shoot = "Fire";
		TakeIn = "TakeIn";
		TakeOut = "TakeOut";
		FireAnimationSpeed = 1f;
		TakeInOutSpeed = 1f;
	}

	public virtual void Awake()
	{
	}

	public virtual void Fire(float fireTime)
	{
		if (!isPlayingTakeInAnimation)
		{
			animation.Rewind(Shoot);
			animation[Shoot].speed = animation[Shoot].clip.length / fireTime;
			animation.Play(Shoot);
		}
	}

	public virtual void Reloading(float reloadTime)
	{
		if (!isPlayingTakeInAnimation)
		{
			animation.Stop(Reload);
			animation[Reload].speed = animation[Reload].clip.length / reloadTime;
			animation.Rewind(Reload);
			animation.Play(Reload);
		}
	}

	public virtual IEnumerator takeIn()
	{
		return new $takeIn$177(this).GetEnumerator();
	}

	public virtual void takeOut()
	{
		animation.Rewind(TakeOut);
		animation[TakeOut].speed = TakeInOutSpeed;
		animation[TakeOut].time = 0f;
		animation.Play(TakeOut);
	}

	public virtual void Main()
	{
	}
}
[Serializable]
public class WeaponCrosshair : MonoBehaviour
{
	public Texture2D crosshairTexture;

	public float length;

	private float width;

	public bool dynamicCrosshair;

	public float crosshairResponce;

	public float defaultDistance;

	public float smooth;

	private bool crosshair;

	private Texture textu;

	private GUIStyle lineStyle;

	private float distance;

	private float currentDistance;

	private WeaponManager weaponManager;

	private WeaponScript weaponScript;

	private GameObject CrosshairSprite;

	public WeaponCrosshair()
	{
		length = 15f;
		width = 2f;
		dynamicCrosshair = true;
		crosshairResponce = 60f;
		defaultDistance = 40f;
		smooth = 0.3f;
		crosshair = true;
	}

	public virtual void Awake()
	{
		lineStyle = new GUIStyle();
		lineStyle.normal.background = crosshairTexture;
		weaponManager = GameObject.FindWithTag("WeaponManager").GetComponent<WeaponManager>();
		CrosshairSprite = GameObject.Find("Sprite (Crosshair)");
	}

	public virtual void Update()
	{
		if ((bool)weaponManager && (bool)weaponManager.SelectedWeapon)
		{
			weaponScript = weaponManager.SelectedWeapon.GetComponent<WeaponScript>();
		}
		if (Time.timeScale >= 0.01f && (bool)weaponScript)
		{
			if (weaponScript.aimed)
			{
				CrosshairSprite.SetActive(value: false);
			}
			else
			{
				CrosshairSprite.SetActive(value: true);
			}
		}
	}

	public virtual void Main()
	{
	}
}
[Serializable]
[AddComponentMenu("FPS system/Weapon System/WeaponManager")]
[RequireComponent(typeof(AudioSource))]
public class WeaponManager : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	internal sealed class $SwitchWeapons$180 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class $ : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal GameObject $currentWeapon$181;

			internal GameObject $nextWeapon$182;

			internal WeaponManager $self_$183;

			public $(GameObject currentWeapon, GameObject nextWeapon, WeaponManager self_)
			{
				$currentWeapon$181 = currentWeapon;
				$nextWeapon$182 = nextWeapon;
				$self_$183 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					$self_$183.canSwitch = false;
					if ($currentWeapon$181.active)
					{
						$currentWeapon$181.SendMessage("deselectWeapon");
					}
					result = (Yield(2, new WaitForSeconds($self_$183.SwitchTime)) ? 1 : 0);
					break;
				case 2:
					$self_$183.audio.clip = $self_$183.takeInAudio;
					$self_$183.audio.Play();
					$currentWeapon$181.SetActiveRecursively(state: false);
					$nextWeapon$182.SetActiveRecursively(state: true);
					$nextWeapon$182.SendMessage("selectWeapon");
					$self_$183.canSwitch = true;
					YieldDefault(1);
					goto case 1;
				case 1:
					result = 0;
					break;
				}
				return (byte)result != 0;
			}
		}

		internal GameObject $currentWeapon$184;

		internal GameObject $nextWeapon$185;

		internal WeaponManager $self_$186;

		public $SwitchWeapons$180(GameObject currentWeapon, GameObject nextWeapon, WeaponManager self_)
		{
			$currentWeapon$184 = currentWeapon;
			$nextWeapon$185 = nextWeapon;
			$self_$186 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new $($currentWeapon$184, $nextWeapon$185, $self_$186);
		}
	}

	public System.Collections.Generic.List<WeaponScript> allWeapons;

	public System.Collections.Generic.List<WeaponScript> allWeaponsTotal;

	public float SwitchTime;

	public bool bPlayer;

	[HideInInspector]
	public WeaponScript SelectedWeapon;

	public int index;

	public AudioClip takeInAudio;

	private GameObject defaultPrimaryWeap;

	private GameObject defaultSecondaryWeap;

	private bool canSwitch;

	private GameObject NG_UI;

	public GameObject Player;

	public Transform synFirePoint;

	public string onlinePlayerTag;

	public int mGunLv_MP5KA4;

	public int mGunLv_STW25;

	public int mGunLv_Deagle;

	public int mGunLv_M87T;

	public int mGunLv_GLOCK21;

	public int mGunLv_MP5KA5;

	public int mGunLv_UZI;

	public int mGunLv_G36K;

	public int mGunLv_AUG;

	public int mGunLv_M3;

	public int mGunLv_BallisticKnife;

	public int mGunLv_M134;

	public int mGunLv_G36K1;

	public int mGunLv_RAZER;

	public int mGunLv_M1Carbine;

	public int mGunLv_TeslaP1;

	private GameObject UIMenuDirectorExt;

	public WeaponManager()
	{
		SwitchTime = 0.5f;
		onlinePlayerTag = "null";
		mGunLv_MP5KA4 = 1;
		mGunLv_STW25 = 1;
		mGunLv_Deagle = 1;
		mGunLv_M87T = 1;
		mGunLv_GLOCK21 = 1;
		mGunLv_MP5KA5 = 1;
		mGunLv_UZI = 1;
		mGunLv_G36K = 1;
		mGunLv_AUG = 1;
		mGunLv_M3 = 1;
		mGunLv_BallisticKnife = 1;
		mGunLv_M134 = 1;
		mGunLv_G36K1 = 1;
		mGunLv_RAZER = 1;
		mGunLv_M1Carbine = 1;
		mGunLv_TeslaP1 = 1;
	}

	public virtual void SetMyTag(string id)
	{
		onlinePlayerTag = id;
	}

	public virtual void SetGunLv(string param)
	{
		string[] array = param.Split("_"[0]);
		mGunLv_MP5KA4 = int.Parse(array[0]);
		mGunLv_STW25 = int.Parse(array[1]);
		mGunLv_Deagle = int.Parse(array[2]);
		mGunLv_M87T = int.Parse(array[3]);
		mGunLv_GLOCK21 = int.Parse(array[4]);
		mGunLv_MP5KA5 = int.Parse(array[5]);
		mGunLv_UZI = int.Parse(array[6]);
		mGunLv_G36K = int.Parse(array[7]);
		mGunLv_AUG = int.Parse(array[8]);
		mGunLv_M3 = int.Parse(array[9]);
		mGunLv_BallisticKnife = int.Parse(array[10]);
		mGunLv_M134 = int.Parse(array[11]);
		mGunLv_G36K1 = int.Parse(array[12]);
		mGunLv_RAZER = int.Parse(array[13]);
		mGunLv_M1Carbine = int.Parse(array[14]);
		mGunLv_TeslaP1 = int.Parse(array[15]);
	}

	public virtual void Awake()
	{
		NG_UI = GameObject.Find("UI Root (3D)");
		if (Application.loadedLevelName != "FreeRun7" && Application.loadedLevelName != "FreeRun7_1" && bPlayer)
		{
			int num = 0;
			for (int i = 1; i <= 8; i++)
			{
				string text = PlayerPrefs.GetString("CurWeaponEquiped_" + i.ToString());
				if (text != string.Empty)
				{
					num++;
					switch (text)
					{
					case "BallisticKnife":
						allWeapons.Add(allWeaponsTotal[6]);
						break;
					case "DesertEagle":
						allWeapons.Add(allWeaponsTotal[0]);
						break;
					case "AK47":
						allWeapons.Add(allWeaponsTotal[4]);
						break;
					case "M4":
						allWeapons.Add(allWeaponsTotal[2]);
						break;
					case "M87T":
						allWeapons.Add(allWeaponsTotal[3]);
						break;
					case "AWP":
						allWeapons.Add(allWeaponsTotal[7]);
						break;
					case "RPG":
						allWeapons.Add(allWeaponsTotal[5]);
						break;
					case "M67":
						allWeapons.Add(allWeaponsTotal[1]);
						break;
					case "GLOCK21":
						allWeapons.Add(allWeaponsTotal[9]);
						break;
					case "MP5KA5":
						allWeapons.Add(allWeaponsTotal[10]);
						break;
					case "UZI":
						allWeapons.Add(allWeaponsTotal[11]);
						break;
					case "G36K":
						allWeapons.Add(allWeaponsTotal[8]);
						break;
					case "M249":
						allWeapons.Add(allWeaponsTotal[12]);
						break;
					case "MilkBomb":
						allWeapons.Add(allWeaponsTotal[13]);
						break;
					case "CandyRifle":
						allWeapons.Add(allWeaponsTotal[14]);
						break;
					case "ChristmasSniper":
						allWeapons.Add(allWeaponsTotal[15]);
						break;
					case "GingerbreadBomb":
						allWeapons.Add(allWeaponsTotal[16]);
						break;
					case "GingerbreadKnife":
						allWeapons.Add(allWeaponsTotal[17]);
						break;
					case "SantaGun":
						allWeapons.Add(allWeaponsTotal[18]);
						break;
					case "AUG":
						allWeapons.Add(allWeaponsTotal[19]);
						break;
					case "M3":
						allWeapons.Add(allWeaponsTotal[20]);
						break;
					case "M134":
						allWeapons.Add(allWeaponsTotal[21]);
						break;
					case "G36K1":
						allWeapons.Add(allWeaponsTotal[22]);
						break;
					case "RAZER":
						allWeapons.Add(allWeaponsTotal[23]);
						break;
					case "FRF2":
						allWeapons.Add(allWeaponsTotal[24]);
						break;
					case "M1Carbine":
						allWeapons.Add(allWeaponsTotal[25]);
						break;
					case "MiniCannon":
						allWeapons.Add(allWeaponsTotal[26]);
						break;
					case "TeslaP1":
						allWeapons.Add(allWeaponsTotal[27]);
						break;
					}
					continue;
				}
				break;
			}
			if (num == 0)
			{
				allWeapons.Add(allWeaponsTotal[0]);
			}
		}
		if ((Application.loadedLevelName == "FreeRun7" || Application.loadedLevelName == "FreeRun7_1") && bPlayer)
		{
			if (PlayerPrefs.GetInt("GingerbreadKnife", 0) >= 1)
			{
				allWeapons.Add(allWeaponsTotal[17]);
			}
			allWeapons.Add(allWeaponsTotal[6]);
		}
		for (int j = 0; j < transform.childCount; j++)
		{
			transform.GetChild(j).gameObject.SetActiveRecursively(state: false);
		}
		for (int k = 0; k < allWeapons.Count; k++)
		{
			allWeapons[k].gameObject.SetActiveRecursively(state: false);
		}
		TakeFirstWeapon(allWeapons[index].gameObject);
		Player = GameObject.Find("ExampleCharacter");
		UIMenuDirectorExt = GameObject.Find("UIMenuDirectorExt");
	}

	public virtual void Update()
	{
		if (!(Time.timeScale >= 0.01f))
		{
			return;
		}
		SelectedWeapon = allWeapons[index];
		if (allWeapons.Count < 2)
		{
			return;
		}
		if (Input.GetKeyDown("2") && canSwitch)
		{
			if (index < allWeapons.Count - 1)
			{
				StartCoroutine_Auto(SwitchWeapons(allWeapons[index].gameObject, allWeapons[index + 1].gameObject));
				index++;
			}
			else
			{
				StartCoroutine_Auto(SwitchWeapons(allWeapons[allWeapons.Count - 1].gameObject, allWeapons[0].gameObject));
				index = 0;
			}
		}
		if (Input.GetKeyDown("1") && canSwitch)
		{
			if (index > 0)
			{
				StartCoroutine_Auto(SwitchWeapons(allWeapons[index].gameObject, allWeapons[index - 1].gameObject));
				index--;
			}
			else
			{
				StartCoroutine_Auto(SwitchWeapons(allWeapons[0].gameObject, allWeapons[allWeapons.Count - 1].gameObject));
				index = allWeapons.Count - 1;
			}
		}
	}

	public virtual void SwitchWeapon()
	{
		SelectedWeapon = allWeapons[index];
		WeaponScript weaponScript = null;
		if (allWeapons.Count >= 2)
		{
			if (index < allWeapons.Count - 1)
			{
				StartCoroutine_Auto(SwitchWeapons(allWeapons[index].gameObject, allWeapons[index + 1].gameObject));
				index++;
				weaponScript = allWeapons[index];
			}
			else
			{
				StartCoroutine_Auto(SwitchWeapons(allWeapons[allWeapons.Count - 1].gameObject, allWeapons[0].gameObject));
				index = 0;
				weaponScript = allWeapons[index];
			}
			if (bPlayer)
			{
				NG_UI.SendMessage("receiveGunName", weaponScript.weaponName, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public virtual void SwitchWeaponRight()
	{
		SelectedWeapon = allWeapons[index];
		WeaponScript weaponScript = null;
		if (allWeapons.Count >= 2)
		{
			if (index < allWeapons.Count - 1)
			{
				StartCoroutine_Auto(SwitchWeapons(allWeapons[index].gameObject, allWeapons[index + 1].gameObject));
				index++;
				weaponScript = allWeapons[index];
			}
			else
			{
				StartCoroutine_Auto(SwitchWeapons(allWeapons[allWeapons.Count - 1].gameObject, allWeapons[0].gameObject));
				index = 0;
				weaponScript = allWeapons[index];
			}
			weaponScript.dieTimeLimit = 0;
			if (weaponScript.weaponName.Equals("Blaser R93") || weaponScript.weaponName.Equals("ChristmasSniper") || weaponScript.weaponName.Equals("FRF2"))
			{
				UIMenuDirectorExt.SendMessage("EquipAWP", SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				UIMenuDirectorExt.SendMessage("NoEquipAWP", SendMessageOptions.DontRequireReceiver);
			}
			if (bPlayer)
			{
				NG_UI.SendMessage("receiveGunName", weaponScript.weaponName, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public virtual void SwitchWeaponLeft()
	{
		SelectedWeapon = allWeapons[index];
		WeaponScript weaponScript = null;
		if (allWeapons.Count >= 2)
		{
			if (index > 0)
			{
				StartCoroutine_Auto(SwitchWeapons(allWeapons[index].gameObject, allWeapons[index - 1].gameObject));
				index--;
				weaponScript = allWeapons[index];
			}
			else
			{
				StartCoroutine_Auto(SwitchWeapons(allWeapons[0].gameObject, allWeapons[allWeapons.Count - 1].gameObject));
				index = allWeapons.Count - 1;
				weaponScript = allWeapons[index];
			}
			weaponScript.dieTimeLimit = 0;
			if (weaponScript.weaponName.Equals("Blaser R93") || weaponScript.weaponName.Equals("ChristmasSniper") || weaponScript.weaponName.Equals("FRF2"))
			{
				UIMenuDirectorExt.SendMessage("EquipAWP", SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				UIMenuDirectorExt.SendMessage("NoEquipAWP", SendMessageOptions.DontRequireReceiver);
			}
			if (bPlayer)
			{
				NG_UI.SendMessage("receiveGunName", weaponScript.weaponName, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public virtual void SwitchWeaponOnline(int tmpIndex)
	{
		if (index == tmpIndex)
		{
			return;
		}
		SelectedWeapon = allWeapons[index];
		WeaponScript weaponScript = null;
		if (allWeapons.Count >= 2)
		{
			StartCoroutine_Auto(SwitchWeapons(allWeapons[index].gameObject, allWeapons[tmpIndex].gameObject));
			index = tmpIndex;
			weaponScript = allWeapons[tmpIndex];
			if (bPlayer)
			{
				NG_UI.SendMessage("receiveGunName", weaponScript.weaponName, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public virtual void TakeFirstWeapon(GameObject nextWeapon)
	{
		audio.clip = takeInAudio;
		audio.Play();
		nextWeapon.SetActiveRecursively(state: true);
		nextWeapon.SendMessage("selectWeapon");
		canSwitch = true;
	}

	public virtual IEnumerator SwitchWeapons(GameObject currentWeapon, GameObject nextWeapon)
	{
		return new $SwitchWeapons$180(currentWeapon, nextWeapon, this).GetEnumerator();
	}

	public virtual void Main()
	{
	}
}
[Serializable]
public enum PickUpStyle
{
	Replace,
	Add
}
[Serializable]
[AddComponentMenu("FPS system/Weapon System/WeaponPickUp")]
public class WeaponPickUp : MonoBehaviour
{
	public GUISkin guiStyle;

	public PickUpStyle pickUpStyle;

	public int pickAmmoMultiply;

	public int reserveAmmoLimit;

	public float throwForce;

	public Transform spawnObject;

	private int actionsToDisplay;

	private float messageTimeOut;

	public System.Collections.Generic.List<GameObject> weapons;

	public System.Collections.Generic.List<WeaponScript> playerWeapons;

	[HideInInspector]
	public System.Collections.Generic.List<string> actionsList;

	[HideInInspector]
	public System.Collections.Generic.List<float> timer;

	private string weapName;

	private GameObject weaponToThrow;

	private WeaponScript newWeapon;

	private GameObject WeaponToPick;

	private WeaponManager weapManager;

	private float color;

	private string text;

	private CharacterController controller;

	private float prevHeight;

	public WeaponPickUp()
	{
		pickAmmoMultiply = 1;
		reserveAmmoLimit = 3;
		throwForce = 500f;
		actionsToDisplay = 5;
		messageTimeOut = 5f;
	}

	public virtual void Awake()
	{
		weapManager = GameObject.FindWithTag("WeaponManager").GetComponent<WeaponManager>();
		controller = (CharacterController)GetComponent(typeof(CharacterController));
		prevHeight = controller.height;
	}

	public virtual void Update()
	{
		if (prevHeight != controller.height)
		{
			WeaponToPick = null;
			prevHeight = controller.height;
		}
		if ((bool)WeaponToPick)
		{
			for (int i = 0; i < playerWeapons.Count; i++)
			{
				if (playerWeapons[i].weaponName == WeaponToPick.name)
				{
					newWeapon = playerWeapons[i];
				}
			}
			for (int j = 0; j < weapons.Count; j++)
			{
				if (weapons[j].name == weapManager.SelectedWeapon.weaponName)
				{
					weaponToThrow = weapons[j];
				}
			}
			if (weapManager.allWeapons.Contains(newWeapon))
			{
				if (newWeapon.GunType == WeaponScript.gunType.MACHINE_GUN)
				{
					if (newWeapon.machineGun.clips < newWeapon.machineGun.bulletsPerClip * reserveAmmoLimit)
					{
						newWeapon.machineGun.clips = newWeapon.machineGun.clips + newWeapon.machineGun.bulletsPerClip * pickAmmoMultiply;
						UnityEngine.Object.Destroy(WeaponToPick);
						actionsList.Add(("Picked ammo for | " + newWeapon.weaponName).ToString());
						timer.Add(messageTimeOut);
					}
					else
					{
						text = "Full Ammo    ";
					}
				}
				if (newWeapon.GunType == WeaponScript.gunType.GRENADE_LAUNCHER)
				{
					if (newWeapon.grenadeLauncher.ammoCount < reserveAmmoLimit)
					{
						newWeapon.grenadeLauncher.ammoCount = newWeapon.grenadeLauncher.ammoCount + pickAmmoMultiply;
						UnityEngine.Object.Destroy(WeaponToPick);
						actionsList.Add(("Picked ammo for | " + newWeapon.weaponName).ToString());
						timer.Add(messageTimeOut);
					}
					else
					{
						text = "Full Ammo    ";
					}
				}
				if (newWeapon.GunType == WeaponScript.gunType.SHOTGUN)
				{
					if (newWeapon.ShotGun.clips < newWeapon.ShotGun.bulletsPerClip * reserveAmmoLimit)
					{
						newWeapon.ShotGun.clips = newWeapon.ShotGun.clips + newWeapon.ShotGun.bulletsPerClip * pickAmmoMultiply;
						UnityEngine.Object.Destroy(WeaponToPick);
						actionsList.Add(("Picked ammo for | " + newWeapon.weaponName).ToString());
						timer.Add(messageTimeOut);
					}
					else
					{
						text = "Full Ammo    ";
					}
				}
			}
			if (Input.GetKeyDown(KeyCode.F))
			{
				if (pickUpStyle == PickUpStyle.Replace)
				{
					if (weapManager.allWeapons.Contains(newWeapon))
					{
						return;
					}
					GameObject gameObject = null;
					gameObject = (GameObject)UnityEngine.Object.Instantiate(weaponToThrow, spawnObject.position, spawnObject.rotation);
					gameObject.name = weaponToThrow.name;
					gameObject.rigidbody.AddForce(-spawnObject.transform.up * throwForce);
					StartCoroutine_Auto(weapManager.SwitchWeapons(weapManager.allWeapons[weapManager.index].gameObject, newWeapon.gameObject));
					weapManager.allWeapons[weapManager.index] = newWeapon;
					UnityEngine.Object.Destroy(WeaponToPick);
					actionsList.Add(("Picked | " + newWeapon.weaponName).ToString());
					timer.Add(messageTimeOut);
				}
				if (pickUpStyle == PickUpStyle.Add)
				{
					if (weapManager.allWeapons.Contains(newWeapon))
					{
						return;
					}
					weapManager.allWeapons.Add(newWeapon);
					StartCoroutine_Auto(weapManager.SwitchWeapons(weapManager.SelectedWeapon.gameObject, weapManager.allWeapons[weapManager.allWeapons.Count - 1].gameObject));
					weapManager.index = weapManager.allWeapons.Count - 1;
					UnityEngine.Object.Destroy(WeaponToPick);
					actionsList.Add(("Picked | " + newWeapon.weaponName).ToString());
					timer.Add(messageTimeOut);
				}
			}
		}
		if (timer.Count <= 0)
		{
			return;
		}
		for (int k = 0; k < timer.Count; k++)
		{
			timer[k] -= Time.deltaTime;
			if (!(timer[k] >= 0f))
			{
				timer.Remove(timer[k]);
				actionsList.Remove(actionsList[k]);
			}
		}
		if (timer.Count > actionsToDisplay && actionsList.Count > actionsToDisplay)
		{
			timer.Remove(timer[0]);
			actionsList.Remove(actionsList[0]);
		}
	}

	public virtual void OnTriggerStay(Collider weapon)
	{
		if (weapon.gameObject.tag == "PickUp")
		{
			WeaponToPick = weapon.gameObject;
		}
	}

	public virtual void OnTriggerExit(Collider weapon)
	{
		if (weapon.gameObject.tag == "PickUp")
		{
			WeaponToPick = null;
		}
	}

	public virtual void OnGUI()
	{
		GUI.skin = guiStyle;
		if ((bool)WeaponToPick)
		{
			weapName = WeaponToPick.name;
			this.color = Mathf.Lerp(this.color, 0.9f, Time.deltaTime * 10f);
		}
		else
		{
			this.color = Mathf.Lerp(this.color, 0f, Time.deltaTime * 10f);
		}
		float a = this.color;
		Color color = GUI.color;
		float num = (color.a = a);
		Color color2 = (GUI.color = color);
		if (!weapManager.allWeapons.Contains(newWeapon))
		{
			text = "Press `F` to pick  |  " + weapName;
		}
		Rect position = new Rect(Screen.width / 2 - text.Length * 10 / 2, Screen.height - 105, text.Length * 10, 45f);
		GUI.Box(position, text);
		float a2 = 0.6f;
		Color color4 = GUI.color;
		float num2 = (color4.a = a2);
		Color color5 = (GUI.color = color4);
		GUILayout.BeginArea(new Rect(10f, Screen.height - actionsList.Count * 33 - 10, 300f, Screen.height));
		GUILayout.BeginVertical();
		for (int i = 0; i < actionsList.Count; i++)
		{
			GUILayout.Box(actionsList[i], GUILayout.Width(300f), GUILayout.Height(30f));
		}
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}

	public virtual void Main()
	{
	}
}
[Serializable]
[AddComponentMenu("FPS system/Weapon System/WeaponScript")]
[RequireComponent(typeof(AudioSource))]
public class WeaponScript : MonoBehaviour
{
	[Serializable]
	public enum gunType
	{
		MACHINE_GUN,
		GRENADE_LAUNCHER,
		SHOTGUN,
		KNIFE
	}

	[Serializable]
	public class AimVariables
	{
		public Vector3 aimPosition;

		public float smoothTime;

		public float toFov;

		public float aimBobbingAmount;

		public bool playAnimation;

		public AimVariables()
		{
			aimPosition = Vector3.zero;
			smoothTime = 5f;
			toFov = 45f;
		}
	}

	[Serializable]
	public class Textures
	{
		public Texture AKLV2Texture;

		public Texture AKLV3Texture;

		public Texture M4LV2Texture;

		public Texture M4LV3Texture;

		public Texture DeagleLV2Texture;

		public Texture DeagleLV3Texture;

		public Texture M87TLV2Texture;

		public Texture M87TLV3Texture;

		public Texture GLOCK21LV2Texture;

		public Texture GLOCK21LV3Texture;

		public Texture MP5KA5LV2Texture;

		public Texture MP5KA5LV3Texture;

		public Texture UZILV2Texture;

		public Texture UZILV3Texture;

		public Texture G36KLV2Texture;

		public Texture G36KLV3Texture;

		public Texture AUGLV2Texture;

		public Texture AUGLV3Texture;

		public Texture M3LV2Texture;

		public Texture M3LV3Texture;

		public Texture BallisticKnifeLV2Texture;

		public Texture BallisticKnifeLV3Texture;

		public Texture M134LV2Texture;

		public Texture M134LV3Texture;

		public Texture G36K1LV2Texture;

		public Texture G36K1LV3Texture;

		public Texture RAZERLV2Texture;

		public Texture RAZERLV3Texture;

		public Texture M1CarbineLV2Texture;

		public Texture M1CarbineLV3Texture;

		public Texture TeslaP1LV2Texture;

		public Texture TeslaP1LV3Texture;
	}

	[Serializable]
	public class Materials
	{
		public Material AKLV2Material;

		public Material AKLV3Material;

		public Material M4LV2Material;

		public Material M4LV3Material;

		public Material DeagleLV2Material;

		public Material DeagleLV3Material;

		public Material M87TLV2Material;

		public Material M87TLV3Material;

		public Material GLOCK21LV2Material;

		public Material GLOCK21LV3Material;

		public Material MP5KA5LV2Material;

		public Material MP5KA5LV3Material;

		public Material UZILV2Material;

		public Material UZILV3Material;

		public Material G36KLV2Material;

		public Material G36KLV3Material;

		public Material AUGLV2Material;

		public Material AUGLV3Material;

		public Material M3LV2Material;

		public Material M3LV3Material;

		public Material BallisticKnifeLV2Material;

		public Material BallisticKnifeLV3Material;

		public Material M134LV2Material;

		public Material M134LV3Material;

		public Material G36K1LV2Material;

		public Material G36K1LV3Material;

		public Material RAZERLV2Material;

		public Material RAZERLV3Material;

		public Material M1CarbineLV2Material;

		public Material M1CarbineLV3Material;

		public Material TeslaP1LV2Material;

		public Material TeslaP1LV3Material;
	}

	[Serializable]
	public class shotGun
	{
		public Transform bullet;

		public int fractions;

		public float errorAngle;

		public float fireRate;

		public float reloadTime;

		public AudioClip fireSound;

		public AudioClip reloadSound;

		public int bulletsPerClip;

		public int bulletsLeft;

		public int clips;

		public ParticleEmitter smoke;

		public shotGun()
		{
			fractions = 5;
			errorAngle = 1f;
			fireRate = 1f;
			bulletsPerClip = 40;
			clips = 15;
		}
	}

	[Serializable]
	public class GrenadeLauncher
	{
		public Rigidbody projectile;

		public AudioClip fireSound;

		public AudioClip reloadSound;

		public float initialSpeed;

		public float shotDelay;

		public float waitBeforeReload;

		public float reloadTime;

		public int ammoCount;

		public GrenadeLauncher()
		{
			initialSpeed = 20f;
			waitBeforeReload = 0.5f;
			reloadTime = 0.5f;
			ammoCount = 20;
		}
	}

	[Serializable]
	public class MachineGun
	{
		public Transform bullet;

		public GameObject muzzleFlash;

		public AudioClip fireSound;

		public AudioClip reloadSound;

		public Light pointLight;

		public float fireRate;

		public int bulletsPerClip;

		public int clips;

		public int bulletsLeft;

		public float reloadTime;

		public float NoAimErrorAngle;

		public float AimErrorAngle;

		public MachineGun()
		{
			fireRate = 0.05f;
			bulletsPerClip = 40;
			clips = 15;
			NoAimErrorAngle = 3f;
		}
	}

	[Serializable]
	public class Knife
	{
		public Transform bullet;

		public AudioClip fireSound;

		public float fireRate;

		public float delayTime;

		public Knife()
		{
			fireRate = 0.5f;
		}
	}

	[Serializable]
	public class RotationReal
	{
		public float RotationAmplitude;

		public float smooth;

		public RotationReal()
		{
			RotationAmplitude = 2f;
			smooth = 7f;
		}
	}

	[Serializable]
	public class SmoothMov
	{
		public float maxAmount;

		public float Smooth;

		public SmoothMov()
		{
			maxAmount = 0.5f;
			Smooth = 3f;
		}
	}

	[Serializable]
	public class cameraRecoil
	{
		public float recoilPower;

		public float shakeAmount;

		public float smooth;

		public cameraRecoil()
		{
			recoilPower = 0.5f;
			shakeAmount = 6f;
			smooth = 3f;
		}
	}

	[Serializable]
	[CompilerGenerated]
	internal sealed class $FireOnline$187 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class $ : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal WeaponScript $self_$188;

			public $(WeaponScript self_)
			{
				$self_$188 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					$self_$188.fire = true;
					if ($self_$188.GunType == gunType.MACHINE_GUN)
					{
						if ($self_$188.canFire && !$self_$188.isReload)
						{
							$self_$188.machineGunFire();
						}
						else
						{
							$self_$188.machineGunStopFire();
						}
					}
					if ($self_$188.GunType == gunType.SHOTGUN && $self_$188.canFire && !$self_$188.isReload && $self_$188.singleFire)
					{
						$self_$188.shotGunFire();
					}
					if ($self_$188.GunType == gunType.GRENADE_LAUNCHER && $self_$188.canFire && !$self_$188.isReload && $self_$188.singleFire)
					{
						$self_$188.grenadeLauncherFIre();
					}
					if ($self_$188.GunType == gunType.KNIFE && $self_$188.canFire && !$self_$188.isReload && $self_$188.singleFire)
					{
						$self_$188.knifeOneShot();
					}
					result = (Yield(2, new WaitForSeconds(0.04f)) ? 1 : 0);
					break;
				case 2:
					$self_$188.fire = false;
					YieldDefault(1);
					goto case 1;
				case 1:
					result = 0;
					break;
				}
				return (byte)result != 0;
			}
		}

		internal WeaponScript $self_$189;

		public $FireOnline$187(WeaponScript self_)
		{
			$self_$189 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new $($self_$189);
		}
	}

	[Serializable]
	[CompilerGenerated]
	internal sealed class $machineGunMuzzleFlash$190 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class $ : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal WeaponScript $self_$191;

			public $(WeaponScript self_)
			{
				$self_$191 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					if ((bool)$self_$191.machineGun.muzzleFlash)
					{
						$self_$191.machineGun.muzzleFlash.transform.localRotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0, 359), Vector3.left);
						$self_$191.machineGun.muzzleFlash.active = true;
					}
					if ((bool)$self_$191.machineGun.pointLight)
					{
						$self_$191.machineGun.pointLight.enabled = true;
					}
					result = (Yield(2, new WaitForSeconds(0.04f)) ? 1 : 0);
					break;
				case 2:
					if ((bool)$self_$191.machineGun.muzzleFlash)
					{
						$self_$191.machineGun.muzzleFlash.active = false;
					}
					if ((bool)$self_$191.machineGun.pointLight)
					{
						$self_$191.machineGun.pointLight.enabled = false;
					}
					YieldDefault(1);
					goto case 1;
				case 1:
					result = 0;
					break;
				}
				return (byte)result != 0;
			}
		}

		internal WeaponScript $self_$192;

		public $machineGunMuzzleFlash$190(WeaponScript self_)
		{
			$self_$192 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new $($self_$192);
		}
	}

	[Serializable]
	[CompilerGenerated]
	internal sealed class $machineGunReload$193 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class $ : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal int $difference$194;

			internal WeaponScript $self_$195;

			public $(WeaponScript self_)
			{
				$self_$195 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					if ($self_$195.machineGun.clips == 0)
					{
						goto case 1;
					}
					$self_$195.isReload = true;
					$self_$195.aimed = false;
					$self_$195.canAim = false;
					if ($self_$195.transform.root.tag == "Player")
					{
						$self_$195.BroadcastMessage("Reloading", $self_$195.machineGun.reloadTime, SendMessageOptions.DontRequireReceiver);
					}
					result = (Yield(2, new WaitForSeconds($self_$195.machineGun.reloadTime)) ? 1 : 0);
					break;
				case 2:
					if ($self_$195.machineGun.clips > 0)
					{
						$difference$194 = $self_$195.machineGun.bulletsPerClip - $self_$195.machineGun.bulletsLeft;
						if ($self_$195.machineGun.clips > $difference$194)
						{
							$self_$195.machineGun.clips = $self_$195.machineGun.clips - $difference$194;
							$self_$195.machineGun.bulletsLeft = $self_$195.machineGun.bulletsLeft + $difference$194;
						}
						else
						{
							$self_$195.machineGun.bulletsLeft = $self_$195.machineGun.bulletsLeft + $self_$195.machineGun.clips;
							$self_$195.machineGun.clips = 0;
						}
						$self_$195.noBullets = false;
						$self_$195.isReload = false;
						$self_$195.canAim = true;
						$self_$195.reloadFlag = true;
					}
					YieldDefault(1);
					goto case 1;
				case 1:
					result = 0;
					break;
				}
				return (byte)result != 0;
			}
		}

		internal WeaponScript $self_$196;

		public $machineGunReload$193(WeaponScript self_)
		{
			$self_$196 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new $($self_$196);
		}
	}

	[Serializable]
	[CompilerGenerated]
	internal sealed class $machineGunCameraRecoil$197 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class $ : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal WeaponScript $self_$198;

			public $(WeaponScript self_)
			{
				$self_$198 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					$self_$198.camPos = Quaternion.Euler(UnityEngine.Random.Range(0f, 0f - $self_$198.CameraRecoil.shakeAmount), UnityEngine.Random.Range(0f - $self_$198.CameraRecoil.shakeAmount, $self_$198.CameraRecoil.shakeAmount), 0f);
					result = (Yield(2, new WaitForSeconds(0.05f)) ? 1 : 0);
					break;
				case 2:
					$self_$198.camPos = $self_$198.camDefaultRotation;
					YieldDefault(1);
					goto case 1;
				case 1:
					result = 0;
					break;
				}
				return (byte)result != 0;
			}
		}

		internal WeaponScript $self_$199;

		public $machineGunCameraRecoil$197(WeaponScript self_)
		{
			$self_$199 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new $($self_$199);
		}
	}

	[Serializable]
	[CompilerGenerated]
	internal sealed class $grenadeLauncherOneShot$200 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class $ : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal Rigidbody $instantiatedProjectile$201;

			internal Collider $col$202;

			internal IEnumerator $$iterator$111$203;

			internal WeaponScript $self_$204;

			public $(WeaponScript self_)
			{
				$self_$204 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					if (!($self_$204.grenadeLauncher.shotDelay <= 0f))
					{
						if (!$self_$204.aimed || $self_$204.Aim.playAnimation)
						{
						}
						if (!$self_$204.aimed)
						{
						}
						if ($self_$204.Recoil)
						{
							$self_$204.mouseLook.Recoil($self_$204.CameraRecoil.recoilPower);
							$self_$204.StartCoroutine_Auto($self_$204.grenadeLauncherCameraRecoil());
						}
						$self_$204.StartCoroutine_Auto($self_$204.grenadeLauncherReload());
						result = (Yield(2, new WaitForSeconds($self_$204.grenadeLauncher.shotDelay)) ? 1 : 0);
						break;
					}
					goto case 2;
				case 2:
					$instantiatedProjectile$201 = ((Rigidbody)UnityEngine.Object.Instantiate($self_$204.grenadeLauncher.projectile, $self_$204.firePoint.position, $self_$204.firePoint.rotation)) as Rigidbody;
					if ($self_$204.transform.root.tag == "Player")
					{
						if ($self_$204.weaponName == "M67")
						{
							$self_$204.gameObjectM671.renderer.enabled = false;
						}
						else if ($self_$204.weaponName == "RPG")
						{
							$self_$204.gameObjectRPG2.renderer.enabled = false;
						}
						else if ($self_$204.weaponName == "MilkBomb")
						{
							$self_$204.gameObjectMilkBomb1.renderer.enabled = false;
						}
						else if ($self_$204.weaponName == "GingerbreadBomb")
						{
							$self_$204.gameObjectGingerbreadBomb1.renderer.enabled = false;
						}
					}
					if (Application.loadedLevelName == "FreeRun")
					{
						$$iterator$111$203 = UnityRuntimeServices.GetEnumerator($self_$204.cols);
						while ($$iterator$111$203.MoveNext())
						{
							object obj = $$iterator$111$203.Current;
							if (!(obj is Collider))
							{
								obj = RuntimeServices.Coerce(obj, typeof(Collider));
							}
							$col$202 = (Collider)obj;
							Physics.IgnoreCollision($col$202, $instantiatedProjectile$201.collider);
							UnityRuntimeServices.Update($$iterator$111$203, $col$202);
						}
					}
					if ($self_$204.transform.parent.tag == "WeaponManagerOnline")
					{
						if ($self_$204.weaponName == "M67" || $self_$204.weaponName == "MilkBomb" || $self_$204.weaponName == "GingerbreadBomb")
						{
							$instantiatedProjectile$201.velocity = $self_$204.transform.TransformDirection(new Vector3(0f, 0f, 0f - $self_$204.grenadeLauncher.initialSpeed));
						}
						else
						{
							$instantiatedProjectile$201.velocity = $self_$204.transform.TransformDirection(new Vector3(0f, 0f, $self_$204.grenadeLauncher.initialSpeed));
						}
					}
					else
					{
						$instantiatedProjectile$201.velocity = $self_$204.transform.TransformDirection(new Vector3(0f, 0f, $self_$204.grenadeLauncher.initialSpeed));
					}
					$instantiatedProjectile$201.transform.gameObject.SendMessage("SetMyTag", $self_$204.weaponManager.onlinePlayerTag, SendMessageOptions.DontRequireReceiver);
					if ($self_$204.transform.root.tag == "Player")
					{
						$instantiatedProjectile$201.transform.gameObject.SendMessage("SetShooter", "Player", SendMessageOptions.DontRequireReceiver);
					}
					$self_$204.lastShot = Time.time;
					if ($self_$204.transform.root.tag == "Player")
					{
						$self_$204.grenadeLauncher.ammoCount = $self_$204.grenadeLauncher.ammoCount - 1;
					}
					$self_$204.audio.clip = $self_$204.grenadeLauncher.fireSound;
					$self_$204.audio.Play();
					if ($self_$204.grenadeLauncher.shotDelay == 0f)
					{
						if (!$self_$204.aimed || $self_$204.Aim.playAnimation)
						{
						}
						if (!$self_$204.aimed)
						{
						}
						if ($self_$204.Recoil)
						{
							$self_$204.mouseLook.Recoil($self_$204.CameraRecoil.recoilPower);
							$self_$204.StartCoroutine_Auto($self_$204.grenadeLauncherCameraRecoil());
						}
						if ($self_$204.grenadeLauncher.ammoCount > 0)
						{
							$self_$204.StartCoroutine_Auto($self_$204.grenadeLauncherReload());
						}
					}
					YieldDefault(1);
					goto case 1;
				case 1:
					result = 0;
					break;
				}
				return (byte)result != 0;
			}
		}

		internal WeaponScript $self_$205;

		public $grenadeLauncherOneShot$200(WeaponScript self_)
		{
			$self_$205 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new $($self_$205);
		}
	}

	[Serializable]
	[CompilerGenerated]
	internal sealed class $grenadeLauncherReload$206 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class $ : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal WeaponScript $self_$207;

			public $(WeaponScript self_)
			{
				$self_$207 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					$self_$207.isReload = true;
					result = (Yield(2, new WaitForSeconds($self_$207.grenadeLauncher.waitBeforeReload)) ? 1 : 0);
					break;
				case 2:
					$self_$207.aimed = false;
					$self_$207.audio.clip = $self_$207.grenadeLauncher.reloadSound;
					$self_$207.audio.Play();
					if ($self_$207.transform.root.tag == "Player")
					{
						$self_$207.BroadcastMessage("Reloading", $self_$207.grenadeLauncher.reloadTime, SendMessageOptions.DontRequireReceiver);
					}
					result = (Yield(3, new WaitForSeconds($self_$207.grenadeLauncher.reloadTime)) ? 1 : 0);
					break;
				case 3:
					$self_$207.isReload = false;
					if ($self_$207.transform.root.tag == "Player")
					{
						if ($self_$207.weaponName == "M67")
						{
							$self_$207.gameObjectM671.renderer.enabled = true;
						}
						else if ($self_$207.weaponName == "RPG")
						{
							$self_$207.gameObjectRPG2.renderer.enabled = true;
						}
						else if ($self_$207.weaponName == "MilkBomb")
						{
							$self_$207.gameObjectMilkBomb1.renderer.enabled = true;
						}
						else if ($self_$207.weaponName == "GingerbreadBomb")
						{
							$self_$207.gameObjectGingerbreadBomb1.renderer.enabled = true;
						}
					}
					YieldDefault(1);
					goto case 1;
				case 1:
					result = 0;
					break;
				}
				return (byte)result != 0;
			}
		}

		internal WeaponScript $self_$208;

		public $grenadeLauncherReload$206(WeaponScript self_)
		{
			$self_$208 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new $($self_$208);
		}
	}

	[Serializable]
	[CompilerGenerated]
	internal sealed class $grenadeLauncherCameraRecoil$209 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class $ : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal WeaponScript $self_$210;

			public $(WeaponScript self_)
			{
				$self_$210 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					$self_$210.camPos = Quaternion.Euler(UnityEngine.Random.Range((0f - $self_$210.CameraRecoil.shakeAmount) * 1.5f, 0f - $self_$210.CameraRecoil.shakeAmount), UnityEngine.Random.Range($self_$210.CameraRecoil.shakeAmount / 3f, $self_$210.CameraRecoil.shakeAmount / 2f), 0f);
					result = (Yield(2, new WaitForSeconds(0.1f)) ? 1 : 0);
					break;
				case 2:
					$self_$210.camPos = $self_$210.camDefaultRotation;
					YieldDefault(1);
					goto case 1;
				case 1:
					result = 0;
					break;
				}
				return (byte)result != 0;
			}
		}

		internal WeaponScript $self_$211;

		public $grenadeLauncherCameraRecoil$209(WeaponScript self_)
		{
			$self_$211 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new $($self_$211);
		}
	}

	[Serializable]
	[CompilerGenerated]
	internal sealed class $shotGunReload$212 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class $ : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal int $difference$213;

			internal WeaponScript $self_$214;

			public $(WeaponScript self_)
			{
				$self_$214 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					if ($self_$214.ShotGun.clips == 0)
					{
						goto case 1;
					}
					$self_$214.isReload = true;
					$self_$214.aimed = false;
					if ($self_$214.transform.root.tag == "Player")
					{
						$self_$214.BroadcastMessage("Reloading", $self_$214.ShotGun.reloadTime, SendMessageOptions.DontRequireReceiver);
					}
					result = (Yield(2, new WaitForSeconds($self_$214.ShotGun.reloadTime)) ? 1 : 0);
					break;
				case 2:
					if ($self_$214.ShotGun.clips > 0)
					{
						$difference$213 = $self_$214.ShotGun.bulletsPerClip - $self_$214.ShotGun.bulletsLeft;
						if ($self_$214.ShotGun.clips > $difference$213)
						{
							$self_$214.ShotGun.clips = $self_$214.ShotGun.clips - $difference$213;
							$self_$214.ShotGun.bulletsLeft = $self_$214.ShotGun.bulletsLeft + $difference$213;
						}
						else
						{
							$self_$214.ShotGun.bulletsLeft = $self_$214.ShotGun.bulletsLeft + $self_$214.ShotGun.clips;
							$self_$214.ShotGun.clips = 0;
						}
						$self_$214.noBullets = false;
						$self_$214.isReload = false;
						$self_$214.canAim = true;
						$self_$214.reloadFlag = true;
					}
					YieldDefault(1);
					goto case 1;
				case 1:
					result = 0;
					break;
				}
				return (byte)result != 0;
			}
		}

		internal WeaponScript $self_$215;

		public $shotGunReload$212(WeaponScript self_)
		{
			$self_$215 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new $($self_$215);
		}
	}

	[Serializable]
	[CompilerGenerated]
	internal sealed class $shotGunSmokeEffect$216 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class $ : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal WeaponScript $self_$217;

			public $(WeaponScript self_)
			{
				$self_$217 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					if (!$self_$217.ShotGun.smoke)
					{
						goto case 1;
					}
					$self_$217.ShotGun.smoke.emit = true;
					result = (Yield(2, new WaitForSeconds(0.3f)) ? 1 : 0);
					break;
				case 2:
					$self_$217.ShotGun.smoke.emit = false;
					YieldDefault(1);
					goto case 1;
				case 1:
					result = 0;
					break;
				}
				return (byte)result != 0;
			}
		}

		internal WeaponScript $self_$218;

		public $shotGunSmokeEffect$216(WeaponScript self_)
		{
			$self_$218 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new $($self_$218);
		}
	}

	[Serializable]
	[CompilerGenerated]
	internal sealed class $shotGunCameraRecoil$219 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class $ : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal WeaponScript $self_$220;

			public $(WeaponScript self_)
			{
				$self_$220 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					$self_$220.camPos = Quaternion.Euler(UnityEngine.Random.Range((0f - $self_$220.CameraRecoil.shakeAmount) * 1.5f, 0f - $self_$220.CameraRecoil.shakeAmount), UnityEngine.Random.Range($self_$220.CameraRecoil.shakeAmount / 3f, $self_$220.CameraRecoil.shakeAmount / 2f), 0f);
					result = (Yield(2, new WaitForSeconds(0.1f)) ? 1 : 0);
					break;
				case 2:
					$self_$220.camPos = $self_$220.camDefaultRotation;
					YieldDefault(1);
					goto case 1;
				case 1:
					result = 0;
					break;
				}
				return (byte)result != 0;
			}
		}

		internal WeaponScript $self_$221;

		public $shotGunCameraRecoil$219(WeaponScript self_)
		{
			$self_$221 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new $($self_$221);
		}
	}

	[HideInInspector]
	public bool aimed;

	[HideInInspector]
	public bool fire;

	[HideInInspector]
	public bool canAim;

	[HideInInspector]
	public bool isReload;

	[HideInInspector]
	public bool noBullets;

	[HideInInspector]
	public bool Recoil;

	[HideInInspector]
	public bool canFire;

	[HideInInspector]
	public bool singleFire;

	[HideInInspector]
	public Camera cam;

	public bool reloadFlag;

	private GameObject player;

	private CharacterController controller;

	private FPSMouseLook mouseLook;

	private WalkSway walkSway;

	private WeaponManager weaponManager;

	private float defaultBobbingAmount;

	private GameObject managerObject;

	public gunType GunType;

	public bool bPlayer;

	public bool FlashLight;

	public string weaponName;

	private GameObject Send;

	private GameObject gameObjectIsPause;

	private GameObject gameObjectIsDied;

	public AimVariables Aim;

	private float defaultFov;

	private Vector3 defaultPosition;

	private float currentFov;

	private Vector3 currentPosition;

	public Transform firePoint;

	private Transform laserFirepoint;

	private GameObject gameObjectKnife1;

	private GameObject gameObjectKnife2;

	private GameObject gameObjectKnife3;

	private GameObject gameObjectDeagle1;

	private GameObject gameObjectDeagle2;

	private GameObject gameObjectDeagle3;

	private GameObject gameObjectM671;

	private GameObject gameObjectM672;

	private GameObject gameObjectM673;

	private GameObject gameObjectM87T1;

	private GameObject gameObjectM87T11;

	private GameObject gameObjectM87T12;

	private GameObject gameObjectM87T2;

	private GameObject gameObjectM87T3;

	private GameObject gameObjectMP5KA41;

	private GameObject gameObjectMP5KA42;

	private GameObject gameObjectMP5KA43;

	private GameObject gameObjectRPG1;

	private GameObject gameObjectRPG2;

	private GameObject gameObjectRPG3;

	private GameObject gameObjectRPG4;

	private GameObject gameObjectSTW_251;

	private GameObject gameObjectSTW_252;

	private GameObject gameObjectSTW_253;

	private GameObject gameObjectSniperRifle1;

	private GameObject gameObjectSniperRifle2;

	private GameObject gameObjectSniperRifle3;

	private GameObject gameObjectGlock211;

	private GameObject gameObjectGlock212;

	private GameObject gameObjectGlock213;

	private GameObject gameObjectG36K1;

	private GameObject gameObjectG36K2;

	private GameObject gameObjectG36K3;

	private GameObject gameObjectMP5KA51;

	private GameObject gameObjectMP5KA52;

	private GameObject gameObjectMP5KA53;

	private GameObject gameObjectUZI1;

	private GameObject gameObjectUZI2;

	private GameObject gameObjectUZI3;

	private GameObject gameObjectM2491;

	private GameObject gameObjectM2492;

	private GameObject gameObjectM2493;

	private GameObject gameObjectM2494;

	private Animation M249Animation;

	private GameObject gameObjectMilkBomb1;

	private GameObject gameObjectMilkBomb2;

	private GameObject gameObjectMilkBomb3;

	private GameObject gameObjectGingerbreadKnife1;

	private GameObject gameObjectGingerbreadKnife2;

	private GameObject gameObjectGingerbreadKnife3;

	private GameObject gameObjectGingerbreadBomb1;

	private GameObject gameObjectGingerbreadBomb2;

	private GameObject gameObjectGingerbreadBomb3;

	private GameObject gameObjectChristmasSniper1;

	private GameObject gameObjectChristmasSniper2;

	private GameObject gameObjectChristmasSniper3;

	private GameObject gameObjectCandyRifle1;

	private GameObject gameObjectCandyRifle2;

	private GameObject gameObjectCandyRifle3;

	private GameObject gameObjectSantaGun1;

	private GameObject gameObjectSantaGun2;

	private GameObject gameObjectSantaGun3;

	private GameObject gameObjectAUG1;

	private GameObject gameObjectAUG2;

	private GameObject gameObjectAUG3;

	private GameObject gameObjectM31;

	private GameObject gameObjectM32;

	private GameObject gameObjectM33;

	private GameObject gameObjectM34;

	private GameObject gameObjectM35;

	private GameObject gameObjectM1342;

	private GameObject gameObjectM1343;

	private GameObject gameObjectM1344;

	private GameObject gameObjectM1345;

	private GameObject gameObjectG36K11;

	private GameObject gameObjectG36K12;

	private GameObject gameObjectG36K13;

	private GameObject gameObjectRAZER1;

	private GameObject gameObjectRAZER2;

	private GameObject gameObjectRAZER3;

	private GameObject gameObjectFRF21;

	private GameObject gameObjectFRF22;

	private GameObject gameObjectFRF23;

	private GameObject gameObjectM1Carbine1;

	private GameObject gameObjectM1Carbine2;

	private GameObject gameObjectM1Carbine3;

	private GameObject gameObjectMiniCannon1;

	private GameObject gameObjectMiniCannon2;

	private GameObject gameObjectMiniCannon3;

	private GameObject gameObjectTeslaP11;

	private GameObject gameObjectTeslaP12;

	private GameObject gameObjectTeslaP13;

	public bool bLastDied;

	public int dieTimeLimit;

	private UnityScript.Lang.Array cols;

	public Textures textures;

	public Materials materials;

	public shotGun ShotGun;

	public GrenadeLauncher grenadeLauncher;

	private float lastShot;

	public MachineGun machineGun;

	[HideInInspector]
	public float errorAngle;

	private float nextFireTime;

	public Knife knife;

	public RotationReal RotRealism;

	private float currentAnglex;

	private float currentAngley;

	public SmoothMov SmoothMovement;

	private Vector3 DefaultPos;

	public cameraRecoil CameraRecoil;

	private Quaternion camDefaultRotation;

	private Quaternion camPos;

	private GameObject UIMenuDirectorExt;

	private float FireAnimationTime;

	private GameObject buyBulletPriceGO;

	public WeaponScript()
	{
		reloadFlag = true;
		weaponName = string.Empty;
		lastShot = -10f;
		FireAnimationTime = 0.1f;
	}

	public virtual void Awake()
	{
		UIMenuDirectorExt = GameObject.Find("UIMenuDirectorExt");
		player = GameObject.FindWithTag("Player");
		controller = (CharacterController)player.GetComponent(typeof(CharacterController));
		mouseLook = (FPSMouseLook)GameObject.FindWithTag("LookObject").GetComponent("FPSMouseLook");
		if (Application.loadedLevelName == "FreeRun3" || Application.loadedLevelName == "FreeRun4" || Application.loadedLevelName == "FreeRun5" || Application.loadedLevelName == "FreeRun6" || Application.loadedLevelName == "FreeRun7" || Application.loadedLevelName == "FreeRun8" || Application.loadedLevelName == "FreeRun9" || Application.loadedLevelName == "FreeRun10")
		{
			cam = GameObject.Find("InGameMenu-Local/Camera").camera;
		}
		if (Application.loadedLevelName == "FreeRun3_1" || Application.loadedLevelName == "FreeRun4_1" || Application.loadedLevelName == "FreeRun5_1" || Application.loadedLevelName == "FreeRun6_1" || Application.loadedLevelName == "FreeRun7_1" || Application.loadedLevelName == "FreeRun8_1" || Application.loadedLevelName == "FreeRun9_1" || Application.loadedLevelName == "FreeRun10_1")
		{
			cam = GameObject.Find("InGameMenu-Online/Camera").camera;
		}
		if (Application.loadedLevelName == "FreeRun" || Application.loadedLevelName == "FreeRun2")
		{
			cam = GameObject.Find("InGameMenu/Camera").camera;
		}
		Send = GameObject.Find("MainScene");
		gameObjectIsPause = GameObject.Find("IsPause");
		gameObjectIsDied = GameObject.Find("IsDied");
		if (transform.root.tag == "Player")
		{
			gameObjectKnife1 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/BallisticKnife/Hands+BallisticKnife/fps_hand_dao/handright/Dao_1");
			gameObjectKnife2 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/BallisticKnife/Hands+BallisticKnife/fps_hand_dao/handleft/handleft_new");
			gameObjectKnife3 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/BallisticKnife/Hands+BallisticKnife/fps_hand_dao/handright/handright_new");
			gameObjectDeagle1 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/Deagle/Hands+Deagle/fps_hand_Deagle/handright/Deagle_1");
			gameObjectDeagle2 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/Deagle/Hands+Deagle/fps_hand_Deagle/handleft/handleft_new");
			gameObjectDeagle3 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/Deagle/Hands+Deagle/fps_hand_Deagle/handright/handright_new");
			gameObjectM671 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/Grenade/Hands+M67/fps_hand_M67/handright/M67_1");
			gameObjectM672 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/Grenade/Hands+M67/fps_hand_M67/handleft/handleft_new");
			gameObjectM673 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/Grenade/Hands+M67/fps_hand_M67/handright/handright_new");
			gameObjectM87T1 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/M87T/Hands+M87T/fps_hand_M87T/handright/M87T_1");
			gameObjectM87T11 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/M87T/Hands+M87T/fps_hand_M87T/handright/M87T_1/Z");
			gameObjectM87T12 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/M87T/Hands+M87T/fps_hand_M87T/handright/M87T_1/Z1");
			gameObjectM87T2 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/M87T/Hands+M87T/fps_hand_M87T/handleft/handleft_new");
			gameObjectM87T3 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/M87T/Hands+M87T/fps_hand_M87T/handright/handright_new");
			gameObjectMP5KA41 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/MP5KA4/Hands+MP5KA4/fps_hand_MP5KA4/handright/AK47_1");
			gameObjectMP5KA42 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/MP5KA4/Hands+MP5KA4/fps_hand_MP5KA4/handleft/handleft_new");
			gameObjectMP5KA43 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/MP5KA4/Hands+MP5KA4/fps_hand_MP5KA4/handright/handright_new");
			gameObjectRPG1 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/RPG/Hands+RPG/fps_hand_RPG/handright/RPG_1");
			gameObjectRPG2 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/RPG/Hands+RPG/fps_hand_RPG/handright/RPG_1/RPG_Missle");
			gameObjectRPG3 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/RPG/Hands+RPG/fps_hand_RPG/handleft/handleft_new");
			gameObjectRPG4 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/RPG/Hands+RPG/fps_hand_RPG/handright/handright_new");
			gameObjectSTW_251 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/STW-25/Hands+STW-25/fps_hand_stw-25/handleft/handleft_new");
			gameObjectSTW_252 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/STW-25/Hands+STW-25/fps_hand_stw-25/handright/handright_new");
			gameObjectSTW_253 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/STW-25/Hands+STW-25/fps_hand_stw-25/handright/M4A1_1");
			gameObjectSniperRifle1 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/SniperRifle/Hands+Blaser R93 LRS2/fps_hand_AWP/handleft/handleft_new");
			gameObjectSniperRifle2 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/SniperRifle/Hands+Blaser R93 LRS2/fps_hand_AWP/handright/handright_new");
			gameObjectSniperRifle3 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/SniperRifle/Hands+Blaser R93 LRS2/fps_hand_AWP/handright/AWP_1");
			gameObjectGlock211 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/GLOCK21/Hands+Deagle/fps_hand_Deagle/handright/GLOCK21");
			gameObjectGlock212 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/GLOCK21/Hands+Deagle/fps_hand_Deagle/handright/handright_new");
			gameObjectGlock213 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/GLOCK21/Hands+Deagle/fps_hand_Deagle/handleft/handleft_new");
			gameObjectMP5KA51 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/MP5KA5/Hands+MP5KA4/fps_hand_MP5KA4/handright/MP5KA5");
			gameObjectMP5KA52 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/MP5KA5/Hands+MP5KA4/fps_hand_MP5KA4/handleft/handleft_new");
			gameObjectMP5KA53 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/MP5KA5/Hands+MP5KA4/fps_hand_MP5KA4/handright/handright_new");
			gameObjectG36K1 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/G36K/Hands+STW-25/fps_hand_stw-25/handleft/handleft_new");
			gameObjectG36K2 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/G36K/Hands+STW-25/fps_hand_stw-25/handright/handright_new");
			gameObjectG36K3 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/G36K/Hands+STW-25/fps_hand_stw-25/handright/G36K");
			gameObjectUZI1 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/UZI/Hands+Deagle/fps_hand_Deagle/handright/UZI");
			gameObjectUZI2 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/UZI/Hands+Deagle/fps_hand_Deagle/handright/handright_new");
			gameObjectUZI3 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/UZI/Hands+Deagle/fps_hand_Deagle/handleft/handleft_new");
			gameObjectM2491 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/M249/Hands+MP5KA4/fps_hand_MP5KA4/handright/M249");
			gameObjectM2492 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/M249/Hands+MP5KA4/fps_hand_MP5KA4/handright/M249/M249_1");
			gameObjectM2493 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/M249/Hands+MP5KA4/fps_hand_MP5KA4/handleft/handleft_new");
			gameObjectM2494 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/M249/Hands+MP5KA4/fps_hand_MP5KA4/handright/handright_new");
			gameObjectMilkBomb1 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/MilkBomb/Hands+M67/fps_hand_M67/handright/MilkBomb");
			gameObjectMilkBomb2 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/MilkBomb/Hands+M67/fps_hand_M67/handright/handright_new");
			gameObjectMilkBomb3 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/MilkBomb/Hands+M67/fps_hand_M67/handleft/handleft_new");
			gameObjectGingerbreadKnife1 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/GingerbreadKnife/Hands+BallisticKnife/fps_hand_dao/handright/GingerbreadKnife1");
			gameObjectGingerbreadKnife2 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/GingerbreadKnife/Hands+BallisticKnife/fps_hand_dao/handleft/handleft_new");
			gameObjectGingerbreadKnife3 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/GingerbreadKnife/Hands+BallisticKnife/fps_hand_dao/handright/handright_new");
			gameObjectGingerbreadBomb1 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/GingerbreadBomb/Hands+M67/fps_hand_M67/handright/GingerbreadBomb1");
			gameObjectGingerbreadBomb2 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/GingerbreadBomb/Hands+M67/fps_hand_M67/handleft/handleft_new");
			gameObjectGingerbreadBomb3 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/GingerbreadBomb/Hands+M67/fps_hand_M67/handright/handright_new");
			gameObjectChristmasSniper1 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/ChristmasSniper/Hands+Blaser R93 LRS2/fps_hand_AWP/handleft/handleft_new");
			gameObjectChristmasSniper2 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/ChristmasSniper/Hands+Blaser R93 LRS2/fps_hand_AWP/handright/handright_new");
			gameObjectChristmasSniper3 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/ChristmasSniper/Hands+Blaser R93 LRS2/fps_hand_AWP/handright/ChristmasSniper1");
			gameObjectCandyRifle1 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/CandyRifle/Hands+STW-25/fps_hand_stw-25/handleft/handleft_new");
			gameObjectCandyRifle2 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/CandyRifle/Hands+STW-25/fps_hand_stw-25/handright/handright_new");
			gameObjectCandyRifle3 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/CandyRifle/Hands+STW-25/fps_hand_stw-25/handright/CandyRifle1");
			gameObjectSantaGun1 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/SantaGun/Hands+MP5KA4/fps_hand_MP5KA4/handright/SantaGun1");
			gameObjectSantaGun2 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/SantaGun/Hands+MP5KA4/fps_hand_MP5KA4/handleft/handleft_new");
			gameObjectSantaGun3 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/SantaGun/Hands+MP5KA4/fps_hand_MP5KA4/handright/handright_new");
			gameObjectAUG1 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/AUG/Hands+STW-25/fps_hand_stw-25/handleft/handleft_new");
			gameObjectAUG2 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/AUG/Hands+STW-25/fps_hand_stw-25/handright/handright_new");
			gameObjectAUG3 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/AUG/Hands+STW-25/fps_hand_stw-25/handright/AUG_1");
			gameObjectM31 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/M3/Hands+M87T/fps_hand_M87T/handright/M3_1");
			gameObjectM32 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/M3/Hands+M87T/fps_hand_M87T/handright/M3_1/M3_2");
			gameObjectM33 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/M3/Hands+M87T/fps_hand_M87T/handright/M3_1/M3_3");
			gameObjectM34 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/M3/Hands+M87T/fps_hand_M87T/handleft/handleft_new");
			gameObjectM35 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/M3/Hands+M87T/fps_hand_M87T/handright/handright_new");
			GameObject gameObject = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/M134/Hands+M134/fps_hand_M134/handright/M134");
			gameObjectM1342 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/M134/Hands+M134/fps_hand_M134/handright/M134/M134_1");
			gameObjectM1343 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/M134/Hands+M134/fps_hand_M134/handright/M134/M134_2");
			gameObjectM1344 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/M134/Hands+M134/fps_hand_M134/handleft/handleft_new");
			gameObjectM1345 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/M134/Hands+M134/fps_hand_M134/handright/handright_new");
			gameObjectG36K11 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/G36K1/Hands+G36K1/fps_hand_G36K1/handleft/handleft_new");
			gameObjectG36K12 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/G36K1/Hands+G36K1/fps_hand_G36K1/handright/handright_new");
			gameObjectG36K13 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/G36K1/Hands+G36K1/fps_hand_G36K1/handright/G36K1");
			gameObjectRAZER1 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/RAZER/Hands+RAZER/fps_hand_RAZER/handleft/handleft_new");
			gameObjectRAZER2 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/RAZER/Hands+RAZER/fps_hand_RAZER/handright/handright_new");
			gameObjectRAZER3 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/RAZER/Hands+RAZER/fps_hand_RAZER/handright/RAZER");
			gameObjectFRF21 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/FRF2/Hands+FRF2/fps_hand_FRF2/handleft/handleft_new");
			gameObjectFRF22 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/FRF2/Hands+FRF2/fps_hand_FRF2/handright/handright_new");
			gameObjectFRF23 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/FRF2/Hands+FRF2/fps_hand_FRF2/handright/FRF2");
			gameObjectM1Carbine1 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/M1Carbine/Hands+M1Carbine/fps_hand_M1Carbine/handleft/handleft_new");
			gameObjectM1Carbine2 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/M1Carbine/Hands+M1Carbine/fps_hand_M1Carbine/handright/handright_new");
			gameObjectM1Carbine3 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/M1Carbine/Hands+M1Carbine/fps_hand_M1Carbine/handright/M1Carbine");
			gameObjectMiniCannon1 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/MiniCannon/Hands+MiniCannon/fps_hand_MiniCannon/handleft/handleft_new");
			gameObjectMiniCannon2 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/MiniCannon/Hands+MiniCannon/fps_hand_MiniCannon/handright/handright_new");
			gameObjectMiniCannon3 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/MiniCannon/Hands+MiniCannon/fps_hand_MiniCannon/handright/MiniCannon");
			gameObjectTeslaP11 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/TeslaP1/Hands+TeslaP1/fps_hand_TeslaP1/handleft/handleft_new");
			gameObjectTeslaP12 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/TeslaP1/Hands+TeslaP1/fps_hand_TeslaP1/handright/handright_new");
			gameObjectTeslaP13 = GameObject.Find("ExampleCharacter/LookObject/Main Camera/Weapon Camera/WeaponManager/TeslaP1/Hands+TeslaP1/fps_hand_TeslaP1/handright/TeslaP1");
		}
		bLastDied = false;
		if (Application.loadedLevelName == "FreeRun")
		{
			cols = GameObject.FindGameObjectWithTag("RiverCollider").GetComponentsInChildren(typeof(Collider));
		}
		laserFirepoint = firePoint;
	}

	public virtual void Start()
	{
		buyBulletPriceGO = GameObject.Find("UIMenuDirectorExt");
		if (transform.root.tag == "Player")
		{
			if (weaponName.Equals("Blaser R93") || weaponName.Equals("ChristmasSniper") || weaponName.Equals("FRF2"))
			{
				UIMenuDirectorExt.SendMessage("EquipAWP", SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				UIMenuDirectorExt.SendMessage("NoEquipAWP", SendMessageOptions.DontRequireReceiver);
			}
		}
		managerObject = transform.parent.gameObject;
		reloadFlag = true;
		if (transform.root.tag == "Player")
		{
			walkSway = (WalkSway)managerObject.GetComponent("WalkSway");
			defaultBobbingAmount = walkSway.bobbingAmount;
		}
		weaponManager = (WeaponManager)managerObject.GetComponent("WeaponManager");
		camDefaultRotation = Camera.main.transform.localRotation;
		defaultFov = Camera.main.fieldOfView;
		defaultPosition = transform.localPosition;
		if (weaponName.Equals("MP5KA4"))
		{
			int num = default(int);
			if ((weaponManager.onlinePlayerTag == "null" || weaponManager.onlinePlayerTag == string.Empty) && weaponManager.bPlayer)
			{
				switch (PlayerPrefs.GetInt("AK", 1))
				{
				case 1:
					machineGun.bulletsPerClip = 40;
					machineGun.clips = 120;
					break;
				case 2:
					machineGun.bulletsPerClip = 50;
					machineGun.clips = 150;
					gameObjectMP5KA41.renderer.material.mainTexture = textures.AKLV2Texture;
					break;
				case 3:
					machineGun.bulletsPerClip = 60;
					machineGun.clips = 240;
					gameObjectMP5KA41.renderer.material.mainTexture = textures.AKLV3Texture;
					break;
				}
			}
			else
			{
				num = weaponManager.mGunLv_MP5KA4;
				machineGun.clips = 999999;
				gameObjectMP5KA41 = transform.Find("Hands+MP5KA4/AK47").gameObject;
				switch (num)
				{
				case 2:
					gameObjectMP5KA41.renderer.material = materials.AKLV2Material;
					break;
				case 3:
					gameObjectMP5KA41.renderer.material = materials.AKLV3Material;
					break;
				}
			}
		}
		if (weaponName.Equals("STW-25"))
		{
			int num2 = default(int);
			if ((weaponManager.onlinePlayerTag == "null" || weaponManager.onlinePlayerTag == string.Empty) && weaponManager.bPlayer)
			{
				switch (PlayerPrefs.GetInt("M4", 1))
				{
				case 1:
					machineGun.bulletsPerClip = 50;
					machineGun.clips = 150;
					break;
				case 2:
					machineGun.bulletsPerClip = 60;
					machineGun.clips = 180;
					gameObjectSTW_253.renderer.material.mainTexture = textures.M4LV2Texture;
					break;
				case 3:
					machineGun.bulletsPerClip = 70;
					machineGun.clips = 210;
					gameObjectSTW_253.renderer.material.mainTexture = textures.M4LV3Texture;
					break;
				}
			}
			else
			{
				num2 = weaponManager.mGunLv_STW25;
				machineGun.clips = 999999;
				gameObjectSTW_253 = transform.Find("Hands+STW-25/M4A1").gameObject;
				switch (num2)
				{
				case 2:
					gameObjectSTW_253.renderer.material = materials.M4LV2Material;
					break;
				case 3:
					gameObjectSTW_253.renderer.material = materials.M4LV3Material;
					break;
				}
			}
		}
		if (weaponName.Equals("Deagle"))
		{
			int num3 = default(int);
			if ((weaponManager.onlinePlayerTag == "null" || weaponManager.onlinePlayerTag == string.Empty) && weaponManager.bPlayer)
			{
				switch (PlayerPrefs.GetInt("Deagle", 1))
				{
				case 1:
					machineGun.bulletsPerClip = 10;
					machineGun.clips = 60;
					break;
				case 2:
					machineGun.bulletsPerClip = 12;
					machineGun.clips = 96;
					gameObjectDeagle1.renderer.material.mainTexture = textures.DeagleLV2Texture;
					break;
				case 3:
					machineGun.bulletsPerClip = 15;
					machineGun.clips = 120;
					gameObjectDeagle1.renderer.material.mainTexture = textures.DeagleLV3Texture;
					break;
				}
			}
			else
			{
				num3 = weaponManager.mGunLv_Deagle;
				machineGun.clips = 999999;
				gameObjectDeagle1 = transform.Find("Hands+Deagle/Deagle").gameObject;
				switch (num3)
				{
				case 2:
					gameObjectDeagle1.renderer.material = materials.DeagleLV2Material;
					break;
				case 3:
					gameObjectDeagle1.renderer.material = materials.DeagleLV3Material;
					break;
				}
			}
		}
		if (weaponName.Equals("M87T"))
		{
			int num4 = default(int);
			if ((weaponManager.onlinePlayerTag == "null" || weaponManager.onlinePlayerTag == string.Empty) && weaponManager.bPlayer)
			{
				switch (PlayerPrefs.GetInt("Rifle", 1))
				{
				case 1:
					ShotGun.bulletsPerClip = 10;
					ShotGun.clips = 80;
					break;
				case 2:
					ShotGun.bulletsPerClip = 12;
					ShotGun.clips = 120;
					gameObjectM87T11.renderer.material.mainTexture = textures.M87TLV2Texture;
					gameObjectM87T12.renderer.material.mainTexture = textures.M87TLV2Texture;
					break;
				case 3:
					ShotGun.bulletsPerClip = 15;
					ShotGun.clips = 150;
					gameObjectM87T11.renderer.material.mainTexture = textures.M87TLV3Texture;
					gameObjectM87T12.renderer.material.mainTexture = textures.M87TLV3Texture;
					break;
				}
			}
			else
			{
				num4 = weaponManager.mGunLv_M87T;
				ShotGun.clips = 999999;
				gameObjectM87T11 = transform.Find("Hands+M87T/M87T/Z").gameObject;
				switch (num4)
				{
				case 2:
					gameObjectM87T11.renderer.material = materials.M87TLV2Material;
					break;
				case 3:
					gameObjectM87T11.renderer.material = materials.M87TLV3Material;
					break;
				}
				gameObjectM87T11 = transform.Find("Hands+M87T/M87T/Z1").gameObject;
				switch (num4)
				{
				case 2:
					gameObjectM87T11.renderer.material = materials.M87TLV2Material;
					break;
				case 3:
					gameObjectM87T11.renderer.material = materials.M87TLV3Material;
					break;
				}
			}
		}
		if (weaponName.Equals("GLOCK21"))
		{
			int num5 = default(int);
			if ((weaponManager.onlinePlayerTag == "null" || weaponManager.onlinePlayerTag == string.Empty) && weaponManager.bPlayer)
			{
				switch (PlayerPrefs.GetInt("GLOCK21", 1))
				{
				case 1:
					machineGun.bulletsPerClip = 20;
					machineGun.clips = 60;
					break;
				case 2:
					machineGun.bulletsPerClip = 25;
					machineGun.clips = 150;
					gameObjectGlock211.renderer.material.mainTexture = textures.GLOCK21LV2Texture;
					break;
				case 3:
					machineGun.bulletsPerClip = 30;
					machineGun.clips = 180;
					gameObjectGlock211.renderer.material.mainTexture = textures.GLOCK21LV3Texture;
					break;
				}
			}
			else
			{
				num5 = weaponManager.mGunLv_GLOCK21;
				machineGun.clips = 999999;
				gameObjectGlock211 = transform.Find("Hands+Deagle/GLOCK21").gameObject;
				switch (num5)
				{
				case 2:
					gameObjectGlock211.renderer.material = materials.GLOCK21LV2Material;
					break;
				case 3:
					gameObjectGlock211.renderer.material = materials.GLOCK21LV3Material;
					break;
				}
			}
		}
		if (weaponName.Equals("MP5KA5"))
		{
			int num6 = default(int);
			if ((weaponManager.onlinePlayerTag == "null" || weaponManager.onlinePlayerTag == string.Empty) && weaponManager.bPlayer)
			{
				switch (PlayerPrefs.GetInt("MP5KA5", 1))
				{
				case 1:
					machineGun.bulletsPerClip = 50;
					machineGun.clips = 200;
					break;
				case 2:
					machineGun.bulletsPerClip = 60;
					machineGun.clips = 240;
					gameObjectMP5KA51.renderer.material.mainTexture = textures.MP5KA5LV2Texture;
					break;
				case 3:
					machineGun.bulletsPerClip = 70;
					machineGun.clips = 280;
					gameObjectMP5KA51.renderer.material.mainTexture = textures.MP5KA5LV3Texture;
					break;
				}
			}
			else
			{
				num6 = weaponManager.mGunLv_MP5KA5;
				machineGun.clips = 999999;
				gameObjectMP5KA51 = transform.Find("Hands+MP5KA5/MP5KA5").gameObject;
				switch (num6)
				{
				case 2:
					gameObjectMP5KA51.renderer.material = materials.MP5KA5LV2Material;
					break;
				case 3:
					gameObjectMP5KA51.renderer.material = materials.MP5KA5LV3Material;
					break;
				}
			}
		}
		if (weaponName.Equals("UZI"))
		{
			int num7 = default(int);
			if ((weaponManager.onlinePlayerTag == "null" || weaponManager.onlinePlayerTag == string.Empty) && weaponManager.bPlayer)
			{
				switch (PlayerPrefs.GetInt("UZI", 1))
				{
				case 1:
					machineGun.bulletsPerClip = 60;
					machineGun.clips = 240;
					break;
				case 2:
					machineGun.bulletsPerClip = 70;
					machineGun.clips = 280;
					gameObjectUZI1.renderer.material.mainTexture = textures.UZILV2Texture;
					break;
				case 3:
					machineGun.bulletsPerClip = 80;
					machineGun.clips = 320;
					gameObjectUZI1.renderer.material.mainTexture = textures.UZILV3Texture;
					break;
				}
			}
			else
			{
				num7 = weaponManager.mGunLv_UZI;
				machineGun.clips = 999999;
				gameObjectUZI1 = transform.Find("Hands+Deagle/UZI").gameObject;
				switch (num7)
				{
				case 2:
					gameObjectUZI1.renderer.material = materials.UZILV2Material;
					break;
				case 3:
					gameObjectUZI1.renderer.material = materials.UZILV3Material;
					break;
				}
			}
		}
		if (weaponName.Equals("G36K"))
		{
			int num8 = default(int);
			if ((weaponManager.onlinePlayerTag == "null" || weaponManager.onlinePlayerTag == string.Empty) && weaponManager.bPlayer)
			{
				switch (PlayerPrefs.GetInt("G36K", 1))
				{
				case 1:
					machineGun.bulletsPerClip = 50;
					machineGun.clips = 150;
					break;
				case 2:
					machineGun.bulletsPerClip = 60;
					machineGun.clips = 180;
					gameObjectG36K3.renderer.material.mainTexture = textures.G36KLV2Texture;
					break;
				case 3:
					machineGun.bulletsPerClip = 70;
					machineGun.clips = 210;
					gameObjectG36K3.renderer.material.mainTexture = textures.G36KLV3Texture;
					break;
				}
			}
			else
			{
				num8 = weaponManager.mGunLv_G36K;
				machineGun.clips = 999999;
				gameObjectG36K3 = transform.Find("Hands+STW-25/G36K").gameObject;
				switch (num8)
				{
				case 2:
					gameObjectG36K3.renderer.material = materials.G36KLV2Material;
					break;
				case 3:
					gameObjectG36K3.renderer.material = materials.G36KLV3Material;
					break;
				}
			}
		}
		if (weaponName.Equals("AUG"))
		{
			int num9 = default(int);
			if ((weaponManager.onlinePlayerTag == "null" || weaponManager.onlinePlayerTag == string.Empty) && weaponManager.bPlayer)
			{
				switch (PlayerPrefs.GetInt("AUG", 1))
				{
				case 1:
					machineGun.bulletsPerClip = 50;
					machineGun.clips = 150;
					break;
				case 2:
					machineGun.bulletsPerClip = 60;
					machineGun.clips = 180;
					gameObjectAUG3.renderer.material.mainTexture = textures.AUGLV2Texture;
					break;
				case 3:
					machineGun.bulletsPerClip = 70;
					machineGun.clips = 210;
					gameObjectAUG3.renderer.material.mainTexture = textures.AUGLV3Texture;
					break;
				}
			}
			else
			{
				num9 = weaponManager.mGunLv_AUG;
				machineGun.clips = 999999;
				gameObjectAUG3 = transform.Find("Hands+STW-25/AUG").gameObject;
				switch (num9)
				{
				case 2:
					gameObjectAUG3.renderer.material = materials.AUGLV2Material;
					break;
				case 3:
					gameObjectAUG3.renderer.material = materials.AUGLV3Material;
					break;
				}
			}
		}
		if (weaponName.Equals("M3"))
		{
			if (weaponManager.onlinePlayerTag == "null" || weaponManager.onlinePlayerTag == string.Empty)
			{
				ShotGun.bulletsPerClip = 7;
				ShotGun.clips = 28;
			}
			int num10 = default(int);
			if ((weaponManager.onlinePlayerTag == "null" || weaponManager.onlinePlayerTag == string.Empty) && weaponManager.bPlayer)
			{
				switch (PlayerPrefs.GetInt("M3", 1))
				{
				case 1:
					ShotGun.bulletsPerClip = 7;
					ShotGun.clips = 28;
					break;
				case 2:
					ShotGun.bulletsPerClip = 12;
					ShotGun.clips = 48;
					gameObjectM32.renderer.material.mainTexture = textures.M3LV2Texture;
					gameObjectM33.renderer.material.mainTexture = textures.M3LV2Texture;
					break;
				case 3:
					ShotGun.bulletsPerClip = 15;
					ShotGun.clips = 60;
					gameObjectM32.renderer.material.mainTexture = textures.M3LV3Texture;
					gameObjectM33.renderer.material.mainTexture = textures.M3LV3Texture;
					break;
				}
			}
			else
			{
				num10 = weaponManager.mGunLv_M3;
				ShotGun.clips = 999999;
				gameObjectM32 = transform.Find("Hands+M87T/M3/M3").gameObject;
				switch (num10)
				{
				case 2:
					gameObjectM32.renderer.material = materials.M3LV2Material;
					break;
				case 3:
					gameObjectM32.renderer.material = materials.M3LV3Material;
					break;
				}
				gameObjectM33 = transform.Find("Hands+M87T/M3/M3_1").gameObject;
				switch (num10)
				{
				case 2:
					gameObjectM33.renderer.material = materials.M3LV2Material;
					break;
				case 3:
					gameObjectM33.renderer.material = materials.M3LV3Material;
					break;
				}
			}
		}
		if (weaponName.Equals("BallisticKnife"))
		{
			int num11 = default(int);
			if ((weaponManager.onlinePlayerTag == "null" || weaponManager.onlinePlayerTag == string.Empty) && weaponManager.bPlayer)
			{
				switch (PlayerPrefs.GetInt("BallisticKnife", 1))
				{
				case 2:
					gameObjectKnife1.renderer.material.mainTexture = textures.BallisticKnifeLV2Texture;
					break;
				case 3:
					gameObjectKnife1.renderer.material.mainTexture = textures.BallisticKnifeLV3Texture;
					break;
				}
			}
			else
			{
				num11 = weaponManager.mGunLv_BallisticKnife;
				gameObjectKnife1 = transform.Find("Hands+BallisticKnife/Dao").gameObject;
				switch (num11)
				{
				case 2:
					gameObjectKnife1.renderer.material = materials.BallisticKnifeLV2Material;
					break;
				case 3:
					gameObjectKnife1.renderer.material = materials.BallisticKnifeLV3Material;
					break;
				}
			}
		}
		if (weaponName.Equals("M134"))
		{
			int num12 = default(int);
			if ((weaponManager.onlinePlayerTag == "null" || weaponManager.onlinePlayerTag == string.Empty) && weaponManager.bPlayer)
			{
				switch (PlayerPrefs.GetInt("M134", 1))
				{
				case 1:
					machineGun.bulletsPerClip = 80;
					machineGun.clips = 240;
					break;
				case 2:
					machineGun.bulletsPerClip = 90;
					machineGun.clips = 270;
					gameObjectM1342.renderer.material.mainTexture = textures.M134LV2Texture;
					gameObjectM1343.renderer.material.mainTexture = textures.M134LV2Texture;
					break;
				case 3:
					machineGun.bulletsPerClip = 100;
					machineGun.clips = 300;
					gameObjectM1342.renderer.material.mainTexture = textures.M134LV3Texture;
					gameObjectM1343.renderer.material.mainTexture = textures.M134LV3Texture;
					break;
				}
			}
			else
			{
				num12 = weaponManager.mGunLv_M134;
				ShotGun.clips = 999999;
				gameObjectM1342 = transform.Find("Hands+M134/M134/M134_1").gameObject;
				switch (num12)
				{
				case 2:
					gameObjectM1342.renderer.material = materials.M134LV2Material;
					break;
				case 3:
					gameObjectM1342.renderer.material = materials.M134LV3Material;
					break;
				}
				gameObjectM1343 = transform.Find("Hands+M134/M134/M134_2").gameObject;
				switch (num12)
				{
				case 2:
					gameObjectM1343.renderer.material = materials.M134LV2Material;
					break;
				case 3:
					gameObjectM1343.renderer.material = materials.M134LV3Material;
					break;
				}
			}
		}
		if (weaponName.Equals("G36K1"))
		{
			int num13 = default(int);
			if ((weaponManager.onlinePlayerTag == "null" || weaponManager.onlinePlayerTag == string.Empty) && weaponManager.bPlayer)
			{
				switch (PlayerPrefs.GetInt("G36K1", 1))
				{
				case 1:
					machineGun.bulletsPerClip = 50;
					machineGun.clips = 150;
					break;
				case 2:
					machineGun.bulletsPerClip = 60;
					machineGun.clips = 180;
					gameObjectG36K13.renderer.material.mainTexture = textures.G36K1LV2Texture;
					break;
				case 3:
					machineGun.bulletsPerClip = 70;
					machineGun.clips = 210;
					gameObjectG36K13.renderer.material.mainTexture = textures.G36K1LV3Texture;
					break;
				}
			}
			else
			{
				num13 = weaponManager.mGunLv_G36K1;
				machineGun.clips = 999999;
				gameObjectG36K13 = transform.Find("Hands+G36K1/G36K1").gameObject;
				switch (num13)
				{
				case 2:
					gameObjectG36K13.renderer.material = materials.G36K1LV2Material;
					break;
				case 3:
					gameObjectG36K13.renderer.material = materials.G36K1LV3Material;
					break;
				}
			}
		}
		if (weaponName.Equals("RAZER"))
		{
			int num14 = default(int);
			if ((weaponManager.onlinePlayerTag == "null" || weaponManager.onlinePlayerTag == string.Empty) && weaponManager.bPlayer)
			{
				switch (PlayerPrefs.GetInt("RAZER", 1))
				{
				case 1:
					machineGun.bulletsPerClip = 50;
					machineGun.clips = 150;
					break;
				case 2:
					machineGun.bulletsPerClip = 60;
					machineGun.clips = 180;
					gameObjectRAZER3.renderer.material.mainTexture = textures.RAZERLV2Texture;
					break;
				case 3:
					machineGun.bulletsPerClip = 70;
					machineGun.clips = 210;
					gameObjectRAZER3.renderer.material.mainTexture = textures.RAZERLV3Texture;
					break;
				}
			}
			else
			{
				num14 = weaponManager.mGunLv_RAZER;
				machineGun.clips = 999999;
				gameObjectRAZER3 = transform.Find("Hands+RAZER/RAZER").gameObject;
				switch (num14)
				{
				case 2:
					gameObjectRAZER3.renderer.material = materials.RAZERLV2Material;
					break;
				case 3:
					gameObjectRAZER3.renderer.material = materials.RAZERLV3Material;
					break;
				}
			}
		}
		if (weaponName.Equals("M1Carbine"))
		{
			int num15 = default(int);
			if ((weaponManager.onlinePlayerTag == "null" || weaponManager.onlinePlayerTag == string.Empty) && weaponManager.bPlayer)
			{
				switch (PlayerPrefs.GetInt("M1Carbine", 1))
				{
				case 1:
					machineGun.bulletsPerClip = 50;
					machineGun.clips = 150;
					break;
				case 2:
					machineGun.bulletsPerClip = 60;
					machineGun.clips = 180;
					gameObjectM1Carbine3.renderer.material.mainTexture = textures.M1CarbineLV2Texture;
					break;
				case 3:
					machineGun.bulletsPerClip = 70;
					machineGun.clips = 210;
					gameObjectM1Carbine3.renderer.material.mainTexture = textures.M1CarbineLV3Texture;
					break;
				}
			}
			else
			{
				num15 = weaponManager.mGunLv_M1Carbine;
				machineGun.clips = 999999;
				gameObjectM1Carbine3 = transform.Find("Hands+M1Carbine/M1Carbine").gameObject;
				switch (num15)
				{
				case 2:
					gameObjectM1Carbine3.renderer.material = materials.M1CarbineLV2Material;
					break;
				case 3:
					gameObjectM1Carbine3.renderer.material = materials.M1CarbineLV3Material;
					break;
				}
			}
		}
		if (weaponName.Equals("TeslaP1"))
		{
			int num16 = default(int);
			if ((weaponManager.onlinePlayerTag == "null" || weaponManager.onlinePlayerTag == string.Empty) && weaponManager.bPlayer)
			{
				switch (PlayerPrefs.GetInt("TeslaP1", 1))
				{
				case 1:
					machineGun.bulletsPerClip = 40;
					machineGun.clips = 160;
					break;
				case 2:
					machineGun.bulletsPerClip = 50;
					machineGun.clips = 200;
					gameObjectTeslaP13.renderer.material.mainTexture = textures.TeslaP1LV2Texture;
					break;
				case 3:
					machineGun.bulletsPerClip = 70;
					machineGun.clips = 280;
					gameObjectTeslaP13.renderer.material.mainTexture = textures.TeslaP1LV3Texture;
					break;
				}
			}
			else
			{
				num16 = weaponManager.mGunLv_TeslaP1;
				machineGun.clips = 999999;
				gameObjectTeslaP13 = transform.Find("Hands+TeslaP1/TeslaP1").gameObject;
				switch (num16)
				{
				case 2:
					gameObjectTeslaP13.renderer.material = materials.TeslaP1LV2Material;
					break;
				case 3:
					gameObjectTeslaP13.renderer.material = materials.TeslaP1LV3Material;
					break;
				}
			}
		}
		if (transform.root.tag == "Player")
		{
			PlayerPrefs.SetInt("OnAim", 0);
		}
		if (GunType == gunType.MACHINE_GUN)
		{
			machineGunAwake();
		}
		if (GunType == gunType.GRENADE_LAUNCHER)
		{
			grenadeLauncherAwake();
		}
		if (GunType == gunType.SHOTGUN)
		{
			shotGunAwake();
		}
		if (GunType == gunType.KNIFE)
		{
			knifeAwake();
		}
		switch (weaponName)
		{
		case "Deagle":
			FireAnimationTime = 0.1f;
			break;
		case "STW-25":
			FireAnimationTime = 0.1f;
			break;
		case "MP5KA4":
			FireAnimationTime = 0.1f;
			break;
		case "Blaser R93":
			FireAnimationTime = 0.1f;
			break;
		case "GLOCK21":
			FireAnimationTime = 0.1f;
			break;
		case "G36K":
			FireAnimationTime = 0.1f;
			break;
		case "MP5KA5":
			FireAnimationTime = 0.082f;
			break;
		case "UZI":
			FireAnimationTime = 0.079f;
			break;
		case "M249":
			FireAnimationTime = 0.1f;
			break;
		case "M87T":
			FireAnimationTime = 0.1f;
			break;
		case "ChristmasSniper":
			FireAnimationTime = 0.1f;
			break;
		case "CandyRifle":
			FireAnimationTime = 0.1f;
			break;
		case "SantaGun":
			FireAnimationTime = 0.079f;
			break;
		case "BallisticKnife":
			FireAnimationTime = 0.3f;
			break;
		case "GingerbreadKnife":
			FireAnimationTime = 0.24f;
			break;
		case "AUG":
			FireAnimationTime = 0.1f;
			break;
		case "M3":
			FireAnimationTime = 0.4f;
			break;
		case "M134":
			FireAnimationTime = 0.1f;
			break;
		case "G36K1":
			FireAnimationTime = 0.1f;
			break;
		case "RAZER":
			FireAnimationTime = 0.082f;
			break;
		case "FRF2":
			FireAnimationTime = 0.1f;
			break;
		case "M1Carbine":
			FireAnimationTime = 0.1f;
			break;
		case "TeslaP1":
			FireAnimationTime = 0.1f;
			break;
		}
	}

	public virtual void Update()
	{
		if (!(Time.timeScale >= 0.01f))
		{
			return;
		}
		if (PlayerPrefs.GetInt("OnAim", 0) == 1 && canAim && (weaponName == "Blaser R93" || weaponName == "ChristmasSniper" || weaponName == "FRF2") && transform.root.tag == "Player")
		{
			PlayerPrefs.SetInt("OnAim", 0);
			aimed = !aimed;
		}
		if (transform.root.tag == "Player")
		{
			Aiming();
		}
		if (Recoil && transform.root.tag == "Player")
		{
			cameraRecoilDo();
		}
		if (GunType == gunType.MACHINE_GUN)
		{
			if ((weaponName == "Deagle" || weaponName == "GLOCK21") && machineGun.clips == 0)
			{
				machineGun.clips += 60;
			}
			if (machineGun.bulletsLeft == 0 && machineGun.clips > 0 && reloadFlag)
			{
				reloadFlag = false;
				audio.clip = machineGun.reloadSound;
				audio.Play();
				StartCoroutine_Auto(machineGunReload());
			}
			machineGunFixedUpdate();
		}
		if (GunType == gunType.GRENADE_LAUNCHER)
		{
			grenadeLauncherFixedUpdate();
		}
		if (GunType == gunType.SHOTGUN)
		{
			if (ShotGun.bulletsLeft == 0 && ShotGun.clips > 0 && reloadFlag)
			{
				reloadFlag = false;
				audio.clip = ShotGun.reloadSound;
				audio.Play();
				StartCoroutine_Auto(shotGunReload());
			}
			shotGunFixedUpdate();
		}
		if (transform.root.tag == "Player")
		{
			buyBullet();
		}
		if (transform.root.tag == "Player" && !isReload)
		{
			if (PlayerPrefs.GetInt("WeaponSwipeLeft", 0) == 1)
			{
				PlayerPrefs.SetInt("WeaponSwipeLeft", 0);
				weaponManager.SwitchWeaponLeft();
			}
			if (PlayerPrefs.GetInt("WeaponSwipeRight", 0) == 1)
			{
				PlayerPrefs.SetInt("WeaponSwipeRight", 0);
				weaponManager.SwitchWeaponRight();
			}
		}
	}

	public virtual void setGunRenderEnabled()
	{
		if (weaponName == "BallisticKnife")
		{
			gameObjectKnife1.renderer.enabled = true;
			gameObjectKnife2.renderer.enabled = true;
			gameObjectKnife3.renderer.enabled = true;
		}
		else if (weaponName == "Deagle")
		{
			gameObjectDeagle1.renderer.enabled = true;
			gameObjectDeagle2.renderer.enabled = true;
			gameObjectDeagle3.renderer.enabled = true;
		}
		else if (weaponName == "M67")
		{
			if (gameObjectM671 != null)
			{
				gameObjectM671.renderer.enabled = true;
			}
			if (gameObjectM672 != null)
			{
				gameObjectM672.renderer.enabled = true;
			}
			if (gameObjectM673 != null)
			{
				gameObjectM673.renderer.enabled = true;
			}
		}
		else if (weaponName == "M87T")
		{
			gameObjectM87T11.renderer.enabled = true;
			gameObjectM87T12.renderer.enabled = true;
			gameObjectM87T2.renderer.enabled = true;
			gameObjectM87T3.renderer.enabled = true;
		}
		else if (weaponName == "RPG")
		{
			gameObjectRPG1.renderer.enabled = true;
			gameObjectRPG2.renderer.enabled = true;
			gameObjectRPG3.renderer.enabled = true;
			gameObjectRPG4.renderer.enabled = true;
		}
		else if (weaponName == "STW-25")
		{
			gameObjectSTW_251.renderer.enabled = true;
			gameObjectSTW_252.renderer.enabled = true;
			gameObjectSTW_253.renderer.enabled = true;
		}
		else if (weaponName == "MP5KA4")
		{
			gameObjectMP5KA41.renderer.enabled = true;
			gameObjectMP5KA42.renderer.enabled = true;
			gameObjectMP5KA43.renderer.enabled = true;
		}
		else if (weaponName == "Blaser R93")
		{
			gameObjectSniperRifle1.renderer.enabled = true;
			gameObjectSniperRifle2.renderer.enabled = true;
			gameObjectSniperRifle3.renderer.enabled = true;
		}
		else if (weaponName == "GLOCK21")
		{
			gameObjectGlock211.renderer.enabled = true;
			gameObjectGlock212.renderer.enabled = true;
			gameObjectGlock213.renderer.enabled = true;
		}
		else if (weaponName == "G36K")
		{
			gameObjectG36K1.renderer.enabled = true;
			gameObjectG36K2.renderer.enabled = true;
			gameObjectG36K3.renderer.enabled = true;
		}
		else if (weaponName == "MP5KA5")
		{
			gameObjectMP5KA51.renderer.enabled = true;
			gameObjectMP5KA52.renderer.enabled = true;
			gameObjectMP5KA53.renderer.enabled = true;
		}
		else if (weaponName == "UZI")
		{
			gameObjectUZI1.renderer.enabled = true;
			gameObjectUZI2.renderer.enabled = true;
			gameObjectUZI3.renderer.enabled = true;
		}
		else if (weaponName == "M249")
		{
			gameObjectM2491.renderer.enabled = true;
			gameObjectM2492.renderer.enabled = true;
			gameObjectM2493.renderer.enabled = true;
			gameObjectM2494.renderer.enabled = true;
		}
		else if (weaponName == "MilkBomb")
		{
			gameObjectMilkBomb1.renderer.enabled = true;
			gameObjectMilkBomb2.renderer.enabled = true;
			gameObjectMilkBomb3.renderer.enabled = true;
		}
		else if (weaponName == "GingerbreadKnife")
		{
			gameObjectGingerbreadKnife1.renderer.enabled = true;
			gameObjectGingerbreadKnife2.renderer.enabled = true;
			gameObjectGingerbreadKnife3.renderer.enabled = true;
		}
		else if (weaponName == "GingerbreadBomb")
		{
			gameObjectGingerbreadBomb1.renderer.enabled = true;
			gameObjectGingerbreadBomb2.renderer.enabled = true;
			gameObjectGingerbreadBomb3.renderer.enabled = true;
		}
		else if (weaponName == "ChristmasSniper")
		{
			gameObjectChristmasSniper1.renderer.enabled = true;
			gameObjectChristmasSniper2.renderer.enabled = true;
			gameObjectChristmasSniper3.renderer.enabled = true;
		}
		else if (weaponName == "CandyRifle")
		{
			gameObjectCandyRifle1.renderer.enabled = true;
			gameObjectCandyRifle2.renderer.enabled = true;
			gameObjectCandyRifle3.renderer.enabled = true;
		}
		else if (weaponName == "SantaGun")
		{
			gameObjectSantaGun1.renderer.enabled = true;
			gameObjectSantaGun2.renderer.enabled = true;
			gameObjectSantaGun3.renderer.enabled = true;
		}
		else if (weaponName == "AUG")
		{
			gameObjectAUG1.renderer.enabled = true;
			gameObjectAUG2.renderer.enabled = true;
			gameObjectAUG3.renderer.enabled = true;
		}
		else if (weaponName == "M3")
		{
			gameObjectM32.renderer.enabled = true;
			gameObjectM33.renderer.enabled = true;
			gameObjectM34.renderer.enabled = true;
			gameObjectM35.renderer.enabled = true;
		}
		else if (weaponName == "M134")
		{
			gameObjectM1342.renderer.enabled = true;
			gameObjectM1343.renderer.enabled = true;
			gameObjectM1344.renderer.enabled = true;
			gameObjectM1345.renderer.enabled = true;
		}
		else if (weaponName == "G36K1")
		{
			gameObjectG36K11.renderer.enabled = true;
			gameObjectG36K12.renderer.enabled = true;
			gameObjectG36K13.renderer.enabled = true;
		}
		else if (weaponName == "RAZER")
		{
			gameObjectRAZER1.renderer.enabled = true;
			gameObjectRAZER2.renderer.enabled = true;
			gameObjectRAZER3.renderer.enabled = true;
		}
		else if (weaponName == "FRF2")
		{
			gameObjectFRF21.renderer.enabled = true;
			gameObjectFRF22.renderer.enabled = true;
			gameObjectFRF23.renderer.enabled = true;
		}
		else if (weaponName == "M1Carbine")
		{
			gameObjectM1Carbine1.renderer.enabled = true;
			gameObjectM1Carbine2.renderer.enabled = true;
			gameObjectM1Carbine3.renderer.enabled = true;
		}
		else if (weaponName == "MiniCannon")
		{
			gameObjectMiniCannon1.renderer.enabled = true;
			gameObjectMiniCannon2.renderer.enabled = true;
			gameObjectMiniCannon3.renderer.enabled = true;
		}
		else if (weaponName == "TeslaP1")
		{
			gameObjectTeslaP11.renderer.enabled = true;
			gameObjectTeslaP12.renderer.enabled = true;
			gameObjectTeslaP13.renderer.enabled = true;
		}
	}

	public virtual void setGunRenderDisabled()
	{
		if (weaponName == "BallisticKnife")
		{
			gameObjectKnife1.renderer.enabled = false;
			gameObjectKnife2.renderer.enabled = false;
			gameObjectKnife3.renderer.enabled = false;
		}
		if (weaponName == "Deagle")
		{
			gameObjectDeagle1.renderer.enabled = false;
			gameObjectDeagle2.renderer.enabled = false;
			gameObjectDeagle3.renderer.enabled = false;
		}
		else if (weaponName == "M67")
		{
			if (gameObjectM671 != null)
			{
				gameObjectM671.renderer.enabled = false;
			}
			if (gameObjectM672 != null)
			{
				gameObjectM672.renderer.enabled = false;
			}
			if (gameObjectM673 != null)
			{
				gameObjectM673.renderer.enabled = false;
			}
		}
		else if (weaponName == "M87T")
		{
			gameObjectM87T11.renderer.enabled = false;
			gameObjectM87T12.renderer.enabled = false;
			gameObjectM87T2.renderer.enabled = false;
			gameObjectM87T3.renderer.enabled = false;
		}
		else if (weaponName == "RPG")
		{
			gameObjectRPG1.renderer.enabled = false;
			gameObjectRPG2.renderer.enabled = false;
			gameObjectRPG3.renderer.enabled = false;
			gameObjectRPG4.renderer.enabled = false;
		}
		else if (weaponName == "STW-25")
		{
			gameObjectSTW_251.renderer.enabled = false;
			gameObjectSTW_252.renderer.enabled = false;
			gameObjectSTW_253.renderer.enabled = false;
		}
		else if (weaponName == "MP5KA4")
		{
			gameObjectMP5KA41.renderer.enabled = false;
			gameObjectMP5KA42.renderer.enabled = false;
			gameObjectMP5KA43.renderer.enabled = false;
		}
		else if (weaponName == "Blaser R93")
		{
			gameObjectSniperRifle1.renderer.enabled = false;
			gameObjectSniperRifle2.renderer.enabled = false;
			gameObjectSniperRifle3.renderer.enabled = false;
		}
		else if (weaponName == "GLOCK21")
		{
			gameObjectGlock211.renderer.enabled = false;
			gameObjectGlock212.renderer.enabled = false;
			gameObjectGlock213.renderer.enabled = false;
		}
		else if (weaponName == "G36K")
		{
			gameObjectG36K1.renderer.enabled = false;
			gameObjectG36K2.renderer.enabled = false;
			gameObjectG36K3.renderer.enabled = false;
		}
		else if (weaponName == "MP5KA5")
		{
			gameObjectMP5KA51.renderer.enabled = false;
			gameObjectMP5KA52.renderer.enabled = false;
			gameObjectMP5KA53.renderer.enabled = false;
		}
		else if (weaponName == "UZI")
		{
			gameObjectUZI1.renderer.enabled = false;
			gameObjectUZI2.renderer.enabled = false;
			gameObjectUZI3.renderer.enabled = false;
		}
		else if (weaponName == "M249")
		{
			gameObjectM2491.renderer.enabled = false;
			gameObjectM2492.renderer.enabled = false;
			gameObjectM2493.renderer.enabled = false;
			gameObjectM2494.renderer.enabled = false;
		}
		else if (weaponName == "MilkBomb")
		{
			gameObjectMilkBomb1.renderer.enabled = false;
			gameObjectMilkBomb2.renderer.enabled = false;
			gameObjectMilkBomb3.renderer.enabled = false;
		}
		else if (weaponName == "GingerbreadKnife")
		{
			gameObjectGingerbreadKnife1.renderer.enabled = false;
			gameObjectGingerbreadKnife2.renderer.enabled = false;
			gameObjectGingerbreadKnife3.renderer.enabled = false;
		}
		else if (weaponName == "GingerbreadBomb")
		{
			gameObjectGingerbreadBomb1.renderer.enabled = false;
			gameObjectGingerbreadBomb2.renderer.enabled = false;
			gameObjectGingerbreadBomb3.renderer.enabled = false;
		}
		else if (weaponName == "ChristmasSniper")
		{
			gameObjectChristmasSniper1.renderer.enabled = false;
			gameObjectChristmasSniper2.renderer.enabled = false;
			gameObjectChristmasSniper3.renderer.enabled = false;
		}
		else if (weaponName == "CandyRifle")
		{
			gameObjectCandyRifle1.renderer.enabled = false;
			gameObjectCandyRifle2.renderer.enabled = false;
			gameObjectCandyRifle3.renderer.enabled = false;
		}
		else if (weaponName == "SantaGun")
		{
			gameObjectSantaGun1.renderer.enabled = false;
			gameObjectSantaGun2.renderer.enabled = false;
			gameObjectSantaGun3.renderer.enabled = false;
		}
		else if (weaponName == "AUG")
		{
			gameObjectAUG1.renderer.enabled = false;
			gameObjectAUG2.renderer.enabled = false;
			gameObjectAUG3.renderer.enabled = false;
		}
		else if (weaponName == "M3")
		{
			gameObjectM32.renderer.enabled = false;
			gameObjectM33.renderer.enabled = false;
			gameObjectM34.renderer.enabled = false;
			gameObjectM35.renderer.enabled = false;
		}
		else if (weaponName == "M134")
		{
			gameObjectM1342.renderer.enabled = false;
			gameObjectM1343.renderer.enabled = false;
			gameObjectM1344.renderer.enabled = false;
			gameObjectM1345.renderer.enabled = false;
		}
		else if (weaponName == "G36K1")
		{
			gameObjectG36K11.renderer.enabled = false;
			gameObjectG36K12.renderer.enabled = false;
			gameObjectG36K13.renderer.enabled = false;
		}
		else if (weaponName == "RAZER")
		{
			gameObjectRAZER1.renderer.enabled = false;
			gameObjectRAZER2.renderer.enabled = false;
			gameObjectRAZER3.renderer.enabled = false;
		}
		else if (weaponName == "FRF2")
		{
			gameObjectFRF21.renderer.enabled = false;
			gameObjectFRF22.renderer.enabled = false;
			gameObjectFRF23.renderer.enabled = false;
		}
		else if (weaponName == "M1Carbine")
		{
			gameObjectM1Carbine1.renderer.enabled = false;
			gameObjectM1Carbine2.renderer.enabled = false;
			gameObjectM1Carbine3.renderer.enabled = false;
		}
		else if (weaponName == "MiniCannon")
		{
			gameObjectMiniCannon1.renderer.enabled = false;
			gameObjectMiniCannon2.renderer.enabled = false;
			gameObjectMiniCannon3.renderer.enabled = false;
		}
		else if (weaponName == "TeslaP1")
		{
			gameObjectTeslaP11.renderer.enabled = false;
			gameObjectTeslaP12.renderer.enabled = false;
			gameObjectTeslaP13.renderer.enabled = false;
		}
	}

	public virtual void SynLocalFirePointPos(Vector3 pos)
	{
		firePoint.position = pos;
	}

	public virtual IEnumerator FireOnline()
	{
		return new $FireOnline$187(this).GetEnumerator();
	}

	public virtual void FireInSingleMode()
	{
		fire = true;
		if (GunType == gunType.MACHINE_GUN)
		{
			if (canFire && !isReload)
			{
				machineGunFire();
			}
			else
			{
				machineGunStopFire();
			}
		}
		if (GunType == gunType.SHOTGUN && canFire && !isReload && singleFire)
		{
			shotGunFire();
		}
		if (GunType == gunType.GRENADE_LAUNCHER && canFire && !isReload && singleFire)
		{
			grenadeLauncherFIre();
		}
		if (GunType == gunType.KNIFE && canFire && !isReload && singleFire)
		{
			knifeOneShot();
		}
	}

	public virtual void StopFireInSingleMode()
	{
		fire = false;
	}

	public virtual void weaponScriptSwitchWeapon()
	{
		weaponManager.SwitchWeapon();
	}

	public virtual void LateUpdate()
	{
		if (transform.parent.tag == "WeaponManagerOnline")
		{
			if (Time.timeScale >= 0.01f)
			{
			}
			return;
		}
		if (gameObjectIsDied.renderer.enabled)
		{
			setGunRenderDisabled();
			dieTimeLimit = 0;
			return;
		}
		if (dieTimeLimit <= 2)
		{
			setGunRenderEnabled();
			dieTimeLimit++;
		}
		if (!gameObjectIsPause.renderer.enabled || !(Time.timeScale >= 0.01f))
		{
			return;
		}
		if (PlayerPrefs.GetInt("FpsOnFire", 0) == 1)
		{
			if (GunType == gunType.MACHINE_GUN)
			{
				if (canFire && !isReload)
				{
					machineGunFire();
				}
				else
				{
					machineGunStopFire();
				}
			}
			if (GunType == gunType.SHOTGUN && canFire && !isReload && singleFire)
			{
				shotGunFire();
			}
			if (GunType == gunType.GRENADE_LAUNCHER && canFire && !isReload && singleFire)
			{
				grenadeLauncherFIre();
			}
			if (GunType == gunType.KNIFE && canFire && !isReload && singleFire)
			{
				knifeOneShot();
			}
		}
		if (PlayerPrefs.GetInt("FpsReload", 0) == 1)
		{
			PlayerPrefs.SetInt("FpsReload", 0);
			if (GunType == gunType.MACHINE_GUN && machineGun.bulletsPerClip - machineGun.bulletsLeft > 0 && machineGun.clips > 0 && !isReload)
			{
				audio.clip = machineGun.reloadSound;
				audio.Play();
				StartCoroutine_Auto(machineGunReload());
			}
			if (GunType == gunType.SHOTGUN && ShotGun.bulletsPerClip - ShotGun.bulletsLeft > 0 && ShotGun.clips > 0 && !isReload)
			{
				audio.clip = ShotGun.reloadSound;
				audio.Play();
				StartCoroutine_Auto(shotGunReload());
			}
		}
	}

	public virtual void firePointSetup()
	{
		if (transform.root.tag == "Player")
		{
			Vector3 position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.nearClipPlane));
			if (transform.parent.tag == "WeaponManager")
			{
				firePoint.position = position;
			}
		}
	}

	public virtual void machineGunAwake()
	{
		machineGun.bulletsLeft = machineGun.bulletsPerClip;
		if ((bool)machineGun.muzzleFlash)
		{
			machineGun.muzzleFlash.active = false;
		}
		if (weaponName == "Blaser R93" || weaponName == "ChristmasSniper" || weaponName == "FRF2")
		{
			canAim = true;
		}
		else
		{
			canAim = false;
		}
		canFire = true;
	}

	public virtual void machineGunFixedUpdate()
	{
		if (fire && !isReload)
		{
			machineGunFire();
		}
		else
		{
			machineGunStopFire();
			if ((bool)machineGun.muzzleFlash)
			{
				machineGun.muzzleFlash.active = false;
			}
		}
		if (isReload)
		{
			canAim = false;
		}
	}

	public virtual void machineGunFire()
	{
		if (!(Time.time - machineGun.fireRate <= nextFireTime))
		{
			nextFireTime = Time.time - Time.deltaTime;
		}
		while (!(nextFireTime >= Time.time) && machineGun.bulletsLeft != 0)
		{
			machineGunOneShot();
			nextFireTime += machineGun.fireRate;
		}
	}

	public virtual void machineGunStopFire()
	{
	}

	public virtual void machineGunOneShot()
	{
		if (!aimed)
		{
			firePointSetup();
		}
		Quaternion rotation = firePoint.rotation;
		firePoint.rotation = Quaternion.Euler(UnityEngine.Random.insideUnitSphere * errorAngle) * this.transform.rotation;
		Transform transform = null;
		if (!aimed)
		{
			transform = ((Transform)UnityEngine.Object.Instantiate(machineGun.bullet, firePoint.position, firePoint.rotation)) as Transform;
			switch (weaponName)
			{
			case "Deagle":
			{
				int num11 = PlayerPrefs.GetInt("Deagle", 1);
				if (num11 == 1)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(15f, 25f);
				}
				if (num11 == 2)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(25f, 30f);
				}
				if (num11 == 3)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(20f, 50f);
				}
				break;
			}
			case "MP5KA4":
			{
				int num9 = PlayerPrefs.GetInt("AK", 1);
				if (num9 == 1)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(14f, 20f);
				}
				if (num9 == 2)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(16f, 26f);
				}
				if (num9 == 3)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(18f, 35f);
				}
				break;
			}
			case "Blaser R93":
				((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(142f, 145f);
				break;
			case "STW-25":
			{
				int num13 = PlayerPrefs.GetInt("M4", 1);
				if (num13 == 1)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(14f, 18f);
				}
				if (num13 == 2)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(18f, 24f);
				}
				if (num13 == 3)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(20f, 30f);
				}
				break;
			}
			case "GLOCK21":
			{
				int num10 = PlayerPrefs.GetInt("GLOCK21", 1);
				if (num10 == 1)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(15f, 20f);
				}
				if (num10 == 2)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(20f, 30f);
				}
				if (num10 == 3)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(25f, 45f);
				}
				break;
			}
			case "G36K":
			{
				int num3 = PlayerPrefs.GetInt("G36K", 1);
				if (num3 == 1)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(16f, 26f);
				}
				if (num3 == 2)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(20f, 35f);
				}
				if (num3 == 3)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(25f, 55f);
				}
				break;
			}
			case "MP5KA5":
			{
				int num4 = PlayerPrefs.GetInt("MP5KA5", 1);
				if (num4 == 1)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(8f, 16f);
				}
				if (num4 == 2)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(10f, 22f);
				}
				if (num4 == 3)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(18f, 35f);
				}
				break;
			}
			case "UZI":
			{
				int num7 = PlayerPrefs.GetInt("MP5KA5", 1);
				if (num7 == 1)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(8f, 13f);
				}
				if (num7 == 2)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(12f, 20f);
				}
				if (num7 == 3)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(16f, 30f);
				}
				break;
			}
			case "M249":
				((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(40f, 55f);
				if (this.transform.root.tag == "Player")
				{
					gameObjectM2492.transform.rotation = gameObjectM2492.transform.rotation * Quaternion.EulerAngles(-0.1f, 0f, 0f);
				}
				break;
			case "ChristmasSniper":
				((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(142f, 147f);
				break;
			case "CandyRifle":
				((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(13f, 28f);
				break;
			case "SantaGun":
				((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(7f, 16f);
				break;
			case "AUG":
			{
				int num12 = PlayerPrefs.GetInt("AUG", 1);
				if (num12 == 1)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(20f, 40f);
				}
				if (num12 == 2)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(25f, 42f);
				}
				if (num12 == 3)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(30f, 55f);
				}
				break;
			}
			case "M134":
			{
				int num6 = PlayerPrefs.GetInt("M134", 1);
				if (num6 == 1)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(20f, 40f);
				}
				if (num6 == 2)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(25f, 42f);
				}
				if (num6 == 3)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(30f, 55f);
				}
				break;
			}
			case "G36K1":
			{
				int num2 = PlayerPrefs.GetInt("G36K1", 1);
				if (num2 == 1)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(20f, 30f);
				}
				if (num2 == 2)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(24f, 34f);
				}
				if (num2 == 3)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(28f, 38f);
				}
				break;
			}
			case "RAZER":
			{
				int num8 = PlayerPrefs.GetInt("RAZER", 1);
				if (num8 == 1)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(8f, 16f);
				}
				if (num8 == 2)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(10f, 22f);
				}
				if (num8 == 3)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(18f, 35f);
				}
				break;
			}
			case "FRF2":
				((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(142f, 147f);
				break;
			case "M1Carbine":
			{
				int num5 = PlayerPrefs.GetInt("M1Carbine", 1);
				if (num5 == 1)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(15f, 20f);
				}
				if (num5 == 2)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(20f, 25f);
				}
				if (num5 == 3)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(23f, 35f);
				}
				break;
			}
			case "TeslaP1":
			{
				int num = PlayerPrefs.GetInt("TeslaP1", 1);
				if (num == 1)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(15f, 30f);
				}
				if (num == 2)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(20f, 34f);
				}
				if (num == 3)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(24f, 40f);
				}
				break;
			}
			}
		}
		else
		{
			Vector3 position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.nearClipPlane));
			transform = ((Transform)UnityEngine.Object.Instantiate(machineGun.bullet, position, firePoint.rotation)) as Transform;
			((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(142f, 150f);
			aimed = false;
		}
		transform.gameObject.SendMessage("SetMyTag", weaponManager.onlinePlayerTag, SendMessageOptions.DontRequireReceiver);
		if (this.transform.root.tag == "Player")
		{
			((Bullet)transform.GetComponent(typeof(Bullet))).shooter = "player";
		}
		firePoint.rotation = rotation;
		lastShot = Time.time;
		if (this.transform.root.tag == "Player")
		{
			machineGun.bulletsLeft--;
		}
		audio.clip = machineGun.fireSound;
		audio.Play();
		if (this.transform.root.tag == "Player")
		{
			if (!aimed)
			{
				StartCoroutine_Auto(machineGunMuzzleFlash());
			}
		}
		else
		{
			StartCoroutine_Auto(machineGunMuzzleFlash());
		}
		if (!aimed || Aim.playAnimation)
		{
		}
		if (!aimed)
		{
			BroadcastMessage("Fire", FireAnimationTime, SendMessageOptions.DontRequireReceiver);
		}
		if (Recoil)
		{
			mouseLook.Recoil(CameraRecoil.recoilPower);
			StartCoroutine_Auto(machineGunCameraRecoil());
		}
	}

	public virtual IEnumerator machineGunMuzzleFlash()
	{
		return new $machineGunMuzzleFlash$190(this).GetEnumerator();
	}

	public virtual IEnumerator machineGunReload()
	{
		return new $machineGunReload$193(this).GetEnumerator();
	}

	public virtual IEnumerator machineGunCameraRecoil()
	{
		return new $machineGunCameraRecoil$197(this).GetEnumerator();
	}

	public virtual void grenadeLauncherAwake()
	{
		canAim = false;
		canFire = true;
	}

	public virtual void grenadeLauncherFixedUpdate()
	{
		if (fire && !isReload)
		{
			grenadeLauncherFIre();
		}
	}

	public virtual void grenadeLauncherFIre()
	{
		if (grenadeLauncher.ammoCount != 0 && canFire)
		{
			if (!(Time.time - grenadeLauncher.reloadTime <= nextFireTime))
			{
				nextFireTime = Time.time - Time.deltaTime;
			}
			while (!(nextFireTime >= Time.time) && grenadeLauncher.ammoCount > 0)
			{
				StartCoroutine_Auto(grenadeLauncherOneShot());
				nextFireTime += grenadeLauncher.reloadTime;
			}
		}
	}

	public virtual IEnumerator grenadeLauncherOneShot()
	{
		return new $grenadeLauncherOneShot$200(this).GetEnumerator();
	}

	public virtual IEnumerator grenadeLauncherReload()
	{
		return new $grenadeLauncherReload$206(this).GetEnumerator();
	}

	public virtual IEnumerator grenadeLauncherCameraRecoil()
	{
		return new $grenadeLauncherCameraRecoil$209(this).GetEnumerator();
	}

	public virtual void shotGunAwake()
	{
		ShotGun.bulletsLeft = ShotGun.bulletsPerClip;
		if ((bool)ShotGun.smoke)
		{
			ShotGun.smoke.emit = false;
		}
		canAim = false;
		canFire = true;
	}

	public virtual void shotGunFixedUpdate()
	{
		if (fire && !isReload)
		{
			shotGunFire();
		}
		else
		{
			shotGunStopFire();
		}
		if (isReload)
		{
			canAim = false;
		}
	}

	public virtual void shotGunFire()
	{
		if (!(Time.time - ShotGun.fireRate <= nextFireTime))
		{
			nextFireTime = Time.time - Time.deltaTime;
		}
		while (!(nextFireTime >= Time.time) && ShotGun.bulletsLeft != 0)
		{
			shotGunOneShot();
			nextFireTime += ShotGun.fireRate;
		}
	}

	public virtual void shotGunStopFire()
	{
	}

	public virtual void shotGunOneShot()
	{
		firePointSetup();
		Quaternion rotation = firePoint.rotation;
		for (int i = 0; i < ShotGun.fractions; i++)
		{
			firePoint.rotation = Quaternion.Euler(UnityEngine.Random.insideUnitSphere * ShotGun.errorAngle) * this.transform.rotation;
			Transform transform = ((Transform)UnityEngine.Object.Instantiate(ShotGun.bullet, firePoint.position, firePoint.rotation)) as Transform;
			if (this.transform.root.tag == "Player")
			{
				((Bullet)transform.GetComponent(typeof(Bullet))).shooter = "player";
			}
			string text = weaponName;
			if (text == "M87T")
			{
				int num = PlayerPrefs.GetInt("Rifle", 1);
				if (num == 1)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(8f, 11f);
				}
				if (num == 2)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(9f, 13f);
				}
				if (num == 3)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(12f, 16f);
				}
			}
			else if (text == "M3")
			{
				int num2 = PlayerPrefs.GetInt("M3", 1);
				if (num2 == 1)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(8f, 13f);
				}
				if (num2 == 2)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(10f, 14f);
				}
				if (num2 == 3)
				{
					((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(14f, 19f);
				}
			}
			transform.gameObject.SendMessage("SetMyTag", weaponManager.onlinePlayerTag, SendMessageOptions.DontRequireReceiver);
		}
		firePoint.rotation = rotation;
		lastShot = Time.time;
		audio.clip = ShotGun.fireSound;
		audio.Play();
		if (this.transform.root.tag == "Player")
		{
			ShotGun.bulletsLeft--;
		}
		if (!aimed || Aim.playAnimation)
		{
		}
		if (!aimed)
		{
			BroadcastMessage("Fire", FireAnimationTime, SendMessageOptions.DontRequireReceiver);
		}
		StartCoroutine_Auto(shotGunSmokeEffect());
		if (Recoil)
		{
			StartCoroutine_Auto(shotGunCameraRecoil());
			mouseLook.Recoil(CameraRecoil.recoilPower);
		}
	}

	public virtual IEnumerator shotGunReload()
	{
		return new $shotGunReload$212(this).GetEnumerator();
	}

	public virtual IEnumerator shotGunSmokeEffect()
	{
		return new $shotGunSmokeEffect$216(this).GetEnumerator();
	}

	public virtual IEnumerator shotGunCameraRecoil()
	{
		return new $shotGunCameraRecoil$219(this).GetEnumerator();
	}

	public virtual void knifeAwake()
	{
		canAim = false;
		canFire = true;
	}

	public virtual void knifeOneShot()
	{
		if (Time.time <= knife.fireRate + lastShot)
		{
			return;
		}
		firePointSetup();
		audio.clip = knife.fireSound;
		audio.Play();
		if (this.transform.root.tag == "Player")
		{
			BroadcastMessage("Fire", FireAnimationTime, SendMessageOptions.DontRequireReceiver);
		}
		Transform transform = ((Transform)UnityEngine.Object.Instantiate(knife.bullet, firePoint.position, firePoint.rotation)) as Transform;
		if (this.transform.root.tag == "Player")
		{
			((Bullet)transform.GetComponent(typeof(Bullet))).shooter = "player";
		}
		string text = weaponName;
		if (text == "BallisticKnife")
		{
			int num = PlayerPrefs.GetInt("BallisticKnife", 1);
			if (num == 1)
			{
				((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(12f, 20f);
			}
			if (num == 2)
			{
				((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(20f, 40f);
			}
			if (num == 3)
			{
				((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(30f, 70f);
			}
		}
		else if (text == "GingerbreadKnife")
		{
			((Bullet)transform.GetComponent(typeof(Bullet))).bulletDamage = UnityEngine.Random.Range(16f, 35f);
		}
		transform.gameObject.SendMessage("SetMyTag", weaponManager.onlinePlayerTag, SendMessageOptions.DontRequireReceiver);
		lastShot = Time.time;
	}

	public virtual void Aiming()
	{
		if (aimed)
		{
			currentPosition = Aim.aimPosition;
			currentFov = Aim.toFov;
			errorAngle = machineGun.AimErrorAngle;
			walkSway.bobbingAmount = Aim.aimBobbingAmount;
		}
		else
		{
			currentPosition = defaultPosition;
			currentFov = defaultFov;
			errorAngle = machineGun.NoAimErrorAngle;
			walkSway.bobbingAmount = defaultBobbingAmount;
		}
		Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, currentFov, Time.deltaTime / Aim.smoothTime);
	}

	public virtual void cameraRecoilDo()
	{
		Camera.main.transform.localRotation = Quaternion.Slerp(Camera.main.transform.localRotation, camPos, Time.deltaTime * CameraRecoil.smooth);
	}

	public virtual void RotationRealism()
	{
		float axis = Input.GetAxis("Mouse X");
		float axis2 = Input.GetAxis("Mouse Y");
		float y = default(float);
		float x = default(float);
		if (!(Mathf.Abs(axis) <= 0.1f))
		{
			if (!(axis >= 0.1f))
			{
				y = (0f - RotRealism.RotationAmplitude) * Mathf.Abs(axis);
			}
			else if (!(axis <= 0.1f))
			{
				y = RotRealism.RotationAmplitude * Mathf.Abs(axis);
			}
		}
		else
		{
			y = 0f;
		}
		if (!(Mathf.Abs(axis2) <= 0.1f))
		{
			if (!(axis2 >= 0.1f))
			{
				x = RotRealism.RotationAmplitude * Mathf.Abs(axis2);
			}
			else if (!(axis2 <= 0.1f))
			{
				x = (0f - RotRealism.RotationAmplitude) * Mathf.Abs(axis2);
			}
		}
		else
		{
			x = 0f;
		}
		Quaternion to = Quaternion.Euler(x, y, 0f);
		transform.localRotation = Quaternion.Slerp(transform.localRotation, to, Time.deltaTime * RotRealism.smooth);
	}

	public virtual void SmoothMove()
	{
		float y = controller.velocity.y;
		float num = default(float);
		float num2 = 0f - Input.GetAxis("Vertical");
		if (!(y <= SmoothMovement.maxAmount + 1f))
		{
			num = 0f - SmoothMovement.maxAmount;
		}
		if (!(y >= 0f - SmoothMovement.maxAmount - 1f))
		{
			num = SmoothMovement.maxAmount;
		}
		if (!(num2 <= SmoothMovement.maxAmount))
		{
			num2 = SmoothMovement.maxAmount;
		}
		if (!(num2 >= 0f - SmoothMovement.maxAmount))
		{
			num2 = 0f - SmoothMovement.maxAmount;
		}
		Vector3 to = new Vector3(transform.localPosition.x, transform.localPosition.y + num, transform.localPosition.z + num2);
		transform.localPosition = Vector3.Lerp(transform.localPosition, to, Time.deltaTime * SmoothMovement.Smooth);
	}

	public virtual void selectWeapon()
	{
		canFire = true;
		if (GunType != gunType.KNIFE)
		{
			canAim = true;
		}
		aimed = false;
		if (transform.root.tag == "Player")
		{
			BroadcastMessage("takeIn", SendMessageOptions.DontRequireReceiver);
		}
	}

	public virtual void deselectWeapon()
	{
		aimed = false;
		isReload = false;
		canFire = false;
		canAim = false;
		isReload = false;
	}

	public virtual void buyBullet()
	{
		if (!(weaponManager.onlinePlayerTag == "null") && !(weaponManager.onlinePlayerTag == string.Empty))
		{
			return;
		}
		if (PlayerPrefs.GetInt("AddBullet", 0) == 1)
		{
			int num = PlayerPrefs.GetInt("GameCoins", 0);
			PlayerPrefs.SetInt("AddBullet", 0);
			switch (weaponName)
			{
			case "Deagle":
				machineGun.clips += machineGun.bulletsPerClip;
				if (machineGun.bulletsLeft == 0)
				{
					StartCoroutine_Auto(machineGunReload());
				}
				break;
			case "MP5KA4":
				if (num < 5)
				{
					buyBulletPriceGO.SendMessage("AddBulletCallBack", -1, SendMessageOptions.DontRequireReceiver);
					return;
				}
				machineGun.clips += machineGun.bulletsPerClip;
				PlayerPrefs.SetInt("GameCoins", PlayerPrefs.GetInt("GameCoins", 0) - 5);
				if (machineGun.bulletsLeft == 0)
				{
					StartCoroutine_Auto(machineGunReload());
				}
				buyBulletPriceGO.SendMessage("AddBulletCallBack", 5, SendMessageOptions.DontRequireReceiver);
				break;
			case "STW-25":
				if (num < 5)
				{
					buyBulletPriceGO.SendMessage("AddBulletCallBack", -1, SendMessageOptions.DontRequireReceiver);
					return;
				}
				machineGun.clips += machineGun.bulletsPerClip;
				PlayerPrefs.SetInt("GameCoins", PlayerPrefs.GetInt("GameCoins", 0) - 5);
				if (machineGun.bulletsLeft == 0)
				{
					StartCoroutine_Auto(machineGunReload());
				}
				buyBulletPriceGO.SendMessage("AddBulletCallBack", 5, SendMessageOptions.DontRequireReceiver);
				break;
			case "M87T":
				if (num < 5)
				{
					buyBulletPriceGO.SendMessage("AddBulletCallBack", -1, SendMessageOptions.DontRequireReceiver);
					return;
				}
				ShotGun.clips += ShotGun.bulletsPerClip;
				PlayerPrefs.SetInt("GameCoins", PlayerPrefs.GetInt("GameCoins", 0) - 5);
				if (ShotGun.bulletsLeft == 0)
				{
					StartCoroutine_Auto(shotGunReload());
				}
				buyBulletPriceGO.SendMessage("AddBulletCallBack", 5, SendMessageOptions.DontRequireReceiver);
				break;
			case "RPG":
				if (num < 15)
				{
					buyBulletPriceGO.SendMessage("AddBulletCallBack", -1, SendMessageOptions.DontRequireReceiver);
					return;
				}
				grenadeLauncher.ammoCount += 5;
				PlayerPrefs.SetInt("GameCoins", PlayerPrefs.GetInt("GameCoins", 0) - 15);
				buyBulletPriceGO.SendMessage("AddBulletCallBack", 15, SendMessageOptions.DontRequireReceiver);
				break;
			case "M67":
				if (num < 10)
				{
					buyBulletPriceGO.SendMessage("AddBulletCallBack", -1, SendMessageOptions.DontRequireReceiver);
					return;
				}
				grenadeLauncher.ammoCount += 5;
				PlayerPrefs.SetInt("GameCoins", PlayerPrefs.GetInt("GameCoins", 0) - 10);
				buyBulletPriceGO.SendMessage("AddBulletCallBack", 10, SendMessageOptions.DontRequireReceiver);
				break;
			case "Blaser R93":
				if (num < 10)
				{
					buyBulletPriceGO.SendMessage("AddBulletCallBack", -1, SendMessageOptions.DontRequireReceiver);
					return;
				}
				machineGun.clips += machineGun.bulletsPerClip;
				PlayerPrefs.SetInt("GameCoins", PlayerPrefs.GetInt("GameCoins", 0) - 10);
				if (machineGun.bulletsLeft == 0)
				{
					StartCoroutine_Auto(machineGunReload());
				}
				buyBulletPriceGO.SendMessage("AddBulletCallBack", 10, SendMessageOptions.DontRequireReceiver);
				break;
			case "GLOCK21":
				machineGun.clips += machineGun.bulletsPerClip;
				if (machineGun.bulletsLeft == 0)
				{
					StartCoroutine_Auto(machineGunReload());
				}
				break;
			case "G36K":
				if (num < 5)
				{
					buyBulletPriceGO.SendMessage("AddBulletCallBack", -1, SendMessageOptions.DontRequireReceiver);
					return;
				}
				machineGun.clips += machineGun.bulletsPerClip;
				PlayerPrefs.SetInt("GameCoins", PlayerPrefs.GetInt("GameCoins", 0) - 5);
				if (machineGun.bulletsLeft == 0)
				{
					StartCoroutine_Auto(machineGunReload());
				}
				buyBulletPriceGO.SendMessage("AddBulletCallBack", 5, SendMessageOptions.DontRequireReceiver);
				break;
			case "MP5KA5":
				if (num < 5)
				{
					buyBulletPriceGO.SendMessage("AddBulletCallBack", -1, SendMessageOptions.DontRequireReceiver);
					return;
				}
				machineGun.clips += machineGun.bulletsPerClip;
				PlayerPrefs.SetInt("GameCoins", PlayerPrefs.GetInt("GameCoins", 0) - 5);
				if (machineGun.bulletsLeft == 0)
				{
					StartCoroutine_Auto(machineGunReload());
				}
				buyBulletPriceGO.SendMessage("AddBulletCallBack", 5, SendMessageOptions.DontRequireReceiver);
				break;
			case "UZI":
				if (num < 5)
				{
					buyBulletPriceGO.SendMessage("AddBulletCallBack", -1, SendMessageOptions.DontRequireReceiver);
					return;
				}
				machineGun.clips += machineGun.bulletsPerClip;
				PlayerPrefs.SetInt("GameCoins", PlayerPrefs.GetInt("GameCoins", 0) - 5);
				if (machineGun.bulletsLeft == 0)
				{
					StartCoroutine_Auto(machineGunReload());
				}
				buyBulletPriceGO.SendMessage("AddBulletCallBack", 5, SendMessageOptions.DontRequireReceiver);
				break;
			case "M249":
				if (num < 10)
				{
					buyBulletPriceGO.SendMessage("AddBulletCallBack", -1, SendMessageOptions.DontRequireReceiver);
					return;
				}
				machineGun.clips += machineGun.bulletsPerClip;
				PlayerPrefs.SetInt("GameCoins", PlayerPrefs.GetInt("GameCoins", 0) - 10);
				if (machineGun.bulletsLeft == 0)
				{
					StartCoroutine_Auto(machineGunReload());
				}
				buyBulletPriceGO.SendMessage("AddBulletCallBack", 10, SendMessageOptions.DontRequireReceiver);
				break;
			case "MilkBomb":
				if (num < 10)
				{
					buyBulletPriceGO.SendMessage("AddBulletCallBack", -1, SendMessageOptions.DontRequireReceiver);
					return;
				}
				grenadeLauncher.ammoCount += 5;
				PlayerPrefs.SetInt("GameCoins", PlayerPrefs.GetInt("GameCoins", 0) - 10);
				buyBulletPriceGO.SendMessage("AddBulletCallBack", 10, SendMessageOptions.DontRequireReceiver);
				break;
			case "GingerbreadBomb":
				if (num < 10)
				{
					buyBulletPriceGO.SendMessage("AddBulletCallBack", -1, SendMessageOptions.DontRequireReceiver);
					return;
				}
				grenadeLauncher.ammoCount += 8;
				PlayerPrefs.SetInt("GameCoins", PlayerPrefs.GetInt("GameCoins", 0) - 10);
				buyBulletPriceGO.SendMessage("AddBulletCallBack", 10, SendMessageOptions.DontRequireReceiver);
				break;
			case "CandyRifle":
				if (num < 5)
				{
					buyBulletPriceGO.SendMessage("AddBulletCallBack", -1, SendMessageOptions.DontRequireReceiver);
					return;
				}
				machineGun.clips += machineGun.bulletsPerClip;
				PlayerPrefs.SetInt("GameCoins", PlayerPrefs.GetInt("GameCoins", 0) - 5);
				if (machineGun.bulletsLeft == 0)
				{
					StartCoroutine_Auto(machineGunReload());
				}
				buyBulletPriceGO.SendMessage("AddBulletCallBack", 5, SendMessageOptions.DontRequireReceiver);
				break;
			case "SantaGun":
				if (num < 5)
				{
					buyBulletPriceGO.SendMessage("AddBulletCallBack", -1, SendMessageOptions.DontRequireReceiver);
					return;
				}
				machineGun.clips += machineGun.bulletsPerClip;
				PlayerPrefs.SetInt("GameCoins", PlayerPrefs.GetInt("GameCoins", 0) - 5);
				if (machineGun.bulletsLeft == 0)
				{
					StartCoroutine_Auto(machineGunReload());
				}
				buyBulletPriceGO.SendMessage("AddBulletCallBack", 5, SendMessageOptions.DontRequireReceiver);
				break;
			case "ChristmasSniper":
				if (num < 10)
				{
					buyBulletPriceGO.SendMessage("AddBulletCallBack", -1, SendMessageOptions.DontRequireReceiver);
					return;
				}
				machineGun.clips += machineGun.bulletsPerClip;
				PlayerPrefs.SetInt("GameCoins", PlayerPrefs.GetInt("GameCoins", 0) - 10);
				if (machineGun.bulletsLeft == 0)
				{
					StartCoroutine_Auto(machineGunReload());
				}
				buyBulletPriceGO.SendMessage("AddBulletCallBack", 10, SendMessageOptions.DontRequireReceiver);
				break;
			case "AUG":
				if (num < 5)
				{
					buyBulletPriceGO.SendMessage("AddBulletCallBack", -1, SendMessageOptions.DontRequireReceiver);
					return;
				}
				machineGun.clips += machineGun.bulletsPerClip;
				PlayerPrefs.SetInt("GameCoins", PlayerPrefs.GetInt("GameCoins", 0) - 5);
				if (machineGun.bulletsLeft == 0)
				{
					StartCoroutine_Auto(machineGunReload());
				}
				buyBulletPriceGO.SendMessage("AddBulletCallBack", 5, SendMessageOptions.DontRequireReceiver);
				break;
			case "M3":
				if (num < 5)
				{
					buyBulletPriceGO.SendMessage("AddBulletCallBack", -1, SendMessageOptions.DontRequireReceiver);
					return;
				}
				ShotGun.clips += ShotGun.bulletsPerClip;
				PlayerPrefs.SetInt("GameCoins", PlayerPrefs.GetInt("GameCoins", 0) - 5);
				if (ShotGun.bulletsLeft == 0)
				{
					StartCoroutine_Auto(shotGunReload());
				}
				buyBulletPriceGO.SendMessage("AddBulletCallBack", 5, SendMessageOptions.DontRequireReceiver);
				break;
			case "M134":
				if (num < 10)
				{
					buyBulletPriceGO.SendMessage("AddBulletCallBack", -1, SendMessageOptions.DontRequireReceiver);
					return;
				}
				machineGun.clips += machineGun.bulletsPerClip;
				PlayerPrefs.SetInt("GameCoins", PlayerPrefs.GetInt("GameCoins", 0) - 10);
				if (machineGun.bulletsLeft == 0)
				{
					StartCoroutine_Auto(machineGunReload());
				}
				buyBulletPriceGO.SendMessage("AddBulletCallBack", 10, SendMessageOptions.DontRequireReceiver);
				break;
			case "G36K1":
				if (num < 5)
				{
					buyBulletPriceGO.SendMessage("AddBulletCallBack", -1, SendMessageOptions.DontRequireReceiver);
					return;
				}
				machineGun.clips += machineGun.bulletsPerClip;
				PlayerPrefs.SetInt("GameCoins", PlayerPrefs.GetInt("GameCoins", 0) - 5);
				if (machineGun.bulletsLeft == 0)
				{
					StartCoroutine_Auto(machineGunReload());
				}
				buyBulletPriceGO.SendMessage("AddBulletCallBack", 5, SendMessageOptions.DontRequireReceiver);
				break;
			case "RAZER":
				if (num < 5)
				{
					buyBulletPriceGO.SendMessage("AddBulletCallBack", -1, SendMessageOptions.DontRequireReceiver);
					return;
				}
				machineGun.clips += machineGun.bulletsPerClip;
				PlayerPrefs.SetInt("GameCoins", PlayerPrefs.GetInt("GameCoins", 0) - 5);
				if (machineGun.bulletsLeft == 0)
				{
					StartCoroutine_Auto(machineGunReload());
				}
				buyBulletPriceGO.SendMessage("AddBulletCallBack", 5, SendMessageOptions.DontRequireReceiver);
				break;
			case "FRF2":
				if (num < 10)
				{
					buyBulletPriceGO.SendMessage("AddBulletCallBack", -1, SendMessageOptions.DontRequireReceiver);
					return;
				}
				machineGun.clips += machineGun.bulletsPerClip;
				PlayerPrefs.SetInt("GameCoins", PlayerPrefs.GetInt("GameCoins", 0) - 10);
				if (machineGun.bulletsLeft == 0)
				{
					StartCoroutine_Auto(machineGunReload());
				}
				buyBulletPriceGO.SendMessage("AddBulletCallBack", 10, SendMessageOptions.DontRequireReceiver);
				break;
			case "M1Carbine":
				if (num < 10)
				{
					buyBulletPriceGO.SendMessage("AddBulletCallBack", -1, SendMessageOptions.DontRequireReceiver);
					return;
				}
				machineGun.clips += machineGun.bulletsPerClip;
				PlayerPrefs.SetInt("GameCoins", PlayerPrefs.GetInt("GameCoins", 0) - 10);
				if (machineGun.bulletsLeft == 0)
				{
					StartCoroutine_Auto(machineGunReload());
				}
				buyBulletPriceGO.SendMessage("AddBulletCallBack", 10, SendMessageOptions.DontRequireReceiver);
				break;
			case "MiniCannon":
				if (num < 15)
				{
					buyBulletPriceGO.SendMessage("AddBulletCallBack", -1, SendMessageOptions.DontRequireReceiver);
					return;
				}
				grenadeLauncher.ammoCount += 8;
				PlayerPrefs.SetInt("GameCoins", PlayerPrefs.GetInt("GameCoins", 0) - 15);
				buyBulletPriceGO.SendMessage("AddBulletCallBack", 15, SendMessageOptions.DontRequireReceiver);
				break;
			case "TeslaP1":
				if (num < 10)
				{
					buyBulletPriceGO.SendMessage("AddBulletCallBack", -1, SendMessageOptions.DontRequireReceiver);
					return;
				}
				machineGun.clips += machineGun.bulletsPerClip;
				PlayerPrefs.SetInt("GameCoins", PlayerPrefs.GetInt("GameCoins", 0) - 10);
				if (machineGun.bulletsLeft == 0)
				{
					StartCoroutine_Auto(machineGunReload());
				}
				buyBulletPriceGO.SendMessage("AddBulletCallBack", 10, SendMessageOptions.DontRequireReceiver);
				break;
			}
		}
		if (PlayerPrefs.GetInt("SingleModeAddBullet", 0) != 1)
		{
			return;
		}
		PlayerPrefs.SetInt("SingleModeAddBullet", 0);
		switch (weaponName)
		{
		case "Deagle":
			machineGun.clips += machineGun.bulletsPerClip;
			if (machineGun.bulletsLeft == 0)
			{
				StartCoroutine_Auto(machineGunReload());
			}
			break;
		case "MP5KA4":
			machineGun.clips += machineGun.bulletsPerClip;
			if (machineGun.bulletsLeft == 0)
			{
				StartCoroutine_Auto(machineGunReload());
			}
			break;
		case "STW-25":
			machineGun.clips += machineGun.bulletsPerClip;
			if (machineGun.bulletsLeft == 0)
			{
				StartCoroutine_Auto(machineGunReload());
			}
			break;
		case "M87T":
			ShotGun.clips += ShotGun.bulletsPerClip;
			if (ShotGun.bulletsLeft == 0)
			{
				StartCoroutine_Auto(shotGunReload());
			}
			break;
		case "RPG":
			grenadeLauncher.ammoCount += 5;
			break;
		case "M67":
			grenadeLauncher.ammoCount += 5;
			break;
		case "Blaser R93":
			machineGun.clips += machineGun.bulletsPerClip;
			if (machineGun.bulletsLeft == 0)
			{
				StartCoroutine_Auto(machineGunReload());
			}
			break;
		case "GLOCK21":
			machineGun.clips += machineGun.bulletsPerClip;
			if (machineGun.bulletsLeft == 0)
			{
				StartCoroutine_Auto(machineGunReload());
			}
			break;
		case "G36K":
			machineGun.clips += machineGun.bulletsPerClip;
			if (machineGun.bulletsLeft == 0)
			{
				StartCoroutine_Auto(machineGunReload());
			}
			break;
		case "MP5KA5":
			machineGun.clips += machineGun.bulletsPerClip;
			if (machineGun.bulletsLeft == 0)
			{
				StartCoroutine_Auto(machineGunReload());
			}
			break;
		case "UZI":
			machineGun.clips += machineGun.bulletsPerClip;
			if (machineGun.bulletsLeft == 0)
			{
				StartCoroutine_Auto(machineGunReload());
			}
			break;
		case "M249":
			machineGun.clips += machineGun.bulletsPerClip;
			if (machineGun.bulletsLeft == 0)
			{
				StartCoroutine_Auto(machineGunReload());
			}
			break;
		case "MilkBomb":
			grenadeLauncher.ammoCount += 5;
			break;
		case "GingerbreadBomb":
			grenadeLauncher.ammoCount += 8;
			break;
		case "CandyRifle":
			machineGun.clips += machineGun.bulletsPerClip;
			if (machineGun.bulletsLeft == 0)
			{
				StartCoroutine_Auto(machineGunReload());
			}
			break;
		case "SantaGun":
			machineGun.clips += machineGun.bulletsPerClip;
			if (machineGun.bulletsLeft == 0)
			{
				StartCoroutine_Auto(machineGunReload());
			}
			break;
		case "ChristmasSniper":
			machineGun.clips += machineGun.bulletsPerClip;
			if (machineGun.bulletsLeft == 0)
			{
				StartCoroutine_Auto(machineGunReload());
			}
			break;
		case "AUG":
			machineGun.clips += machineGun.bulletsPerClip;
			if (machineGun.bulletsLeft == 0)
			{
				StartCoroutine_Auto(machineGunReload());
			}
			break;
		case "M3":
			ShotGun.clips += ShotGun.bulletsPerClip;
			if (ShotGun.bulletsLeft == 0)
			{
				StartCoroutine_Auto(shotGunReload());
			}
			break;
		case "M134":
			machineGun.clips += machineGun.bulletsPerClip;
			if (machineGun.bulletsLeft == 0)
			{
				StartCoroutine_Auto(machineGunReload());
			}
			break;
		case "G36K1":
			machineGun.clips += machineGun.bulletsPerClip;
			if (machineGun.bulletsLeft == 0)
			{
				StartCoroutine_Auto(machineGunReload());
			}
			break;
		case "RAZER":
			machineGun.clips += machineGun.bulletsPerClip;
			if (machineGun.bulletsLeft == 0)
			{
				StartCoroutine_Auto(machineGunReload());
			}
			break;
		case "FRF2":
			machineGun.clips += machineGun.bulletsPerClip;
			if (machineGun.bulletsLeft == 0)
			{
				StartCoroutine_Auto(machineGunReload());
			}
			break;
		case "M1Carbine":
			machineGun.clips += machineGun.bulletsPerClip;
			if (machineGun.bulletsLeft == 0)
			{
				StartCoroutine_Auto(machineGunReload());
			}
			break;
		case "MiniCannon":
			grenadeLauncher.ammoCount += 8;
			break;
		case "TeslaP1":
			machineGun.clips += machineGun.bulletsPerClip;
			if (machineGun.bulletsLeft == 0)
			{
				StartCoroutine_Auto(machineGunReload());
			}
			break;
		}
	}

	public virtual void ChangeWeaponSpeed(float attackSpeedForSingleMode)
	{
		machineGun.fireRate *= attackSpeedForSingleMode;
		ShotGun.fireRate *= attackSpeedForSingleMode;
		knife.fireRate *= attackSpeedForSingleMode;
	}

	public virtual void Main()
	{
	}
}
[Serializable]
public class PickObject : MonoBehaviour
{
	public TextMesh textField;

	public virtual void OnEnable()
	{
		FingerGestures.OnFingerDown += FingerGestures_OnFingerDown;
	}

	public virtual void OnDisable()
	{
		FingerGestures.OnFingerDown -= FingerGestures_OnFingerDown;
	}

	public virtual void FingerGestures_OnFingerDown(int fingerIndex, Vector2 fingerPos)
	{
		GameObject gameObject = PickObject(fingerPos);
		if ((bool)gameObject)
		{
			DisplayText("You pressed " + gameObject.name);
		}
		else
		{
			DisplayText("You didn't pressed any object");
		}
	}

	public virtual void DisplayText(object text)
	{
		if ((bool)textField)
		{
			TextMesh textMesh = textField;
			object obj = text;
			if (!(obj is string))
			{
				obj = RuntimeServices.Coerce(obj, typeof(string));
			}
			textMesh.text = (string)obj;
		}
		else
		{
			Debug.Log(text);
		}
	}

	public virtual GameObject PickObject(Vector2 screenPos)
	{
		Ray ray = Camera.main.ScreenPointToRay(screenPos);
		RaycastHit hitInfo = default(RaycastHit);
		return (!Physics.Raycast(ray, out hitInfo)) ? null : hitInfo.collider.gameObject;
	}

	public virtual void Main()
	{
	}
}
[Serializable]
public class TapToPickObject : MonoBehaviour
{
	public TextMesh textField;

	public virtual void OnEnable()
	{
		FingerGestures.OnFingerDown += FingerGestures_OnFingerDown;
	}

	public virtual void OnDisable()
	{
		FingerGestures.OnFingerDown -= FingerGestures_OnFingerDown;
	}

	public virtual void FingerGestures_OnFingerDown(int fingerIndex, Vector2 fingerPos)
	{
		GameObject gameObject = PickObject(fingerPos);
		if ((bool)gameObject)
		{
			DisplayText("You pressed " + gameObject.name);
		}
		else
		{
			DisplayText("You didn't pressed any object");
		}
	}

	public virtual void DisplayText(object text)
	{
		if ((bool)textField)
		{
			TextMesh textMesh = textField;
			object obj = text;
			if (!(obj is string))
			{
				obj = RuntimeServices.Coerce(obj, typeof(string));
			}
			textMesh.text = (string)obj;
		}
		else
		{
			Debug.Log(text);
		}
	}

	public virtual GameObject PickObject(Vector2 screenPos)
	{
		Ray ray = Camera.main.ScreenPointToRay(screenPos);
		RaycastHit hitInfo = default(RaycastHit);
		return (!Physics.Raycast(ray, out hitInfo)) ? null : hitInfo.collider.gameObject;
	}

	public virtual void Main()
	{
	}
}
[Serializable]
public class JavascriptSkeleton : MonoBehaviour
{
	public virtual void OnEnable()
	{
		FingerGestures.OnFingerDown += FingerGestures_OnFingerDown;
		FingerGestures.OnFingerStationaryBegin += FingerGestures_OnFingerStationaryBegin;
		FingerGestures.OnFingerStationary += FingerGestures_OnFingerStationary;
		FingerGestures.OnFingerStationaryEnd += FingerGestures_OnFingerStationaryEnd;
		FingerGestures.OnFingerMoveBegin += FingerGestures_OnFingerMoveBegin;
		FingerGestures.OnFingerMove += FingerGestures_OnFingerMove;
		FingerGestures.OnFingerMoveEnd += FingerGestures_OnFingerMoveEnd;
		FingerGestures.OnFingerUp += FingerGestures_OnFingerUp;
		FingerGestures.OnFingerLongPress += FingerGestures_OnFingerLongPress;
		FingerGestures.OnFingerTap += FingerGestures_OnFingerTap;
		FingerGestures.OnFingerSwipe += FingerGestures_OnFingerSwipe;
		FingerGestures.OnFingerDragBegin += FingerGestures_OnFingerDragBegin;
		FingerGestures.OnFingerDragMove += FingerGestures_OnFingerDragMove;
		FingerGestures.OnFingerDragEnd += FingerGestures_OnFingerDragEnd;
		FingerGestures.OnLongPress += FingerGestures_OnLongPress;
		FingerGestures.OnTap += FingerGestures_OnTap;
		FingerGestures.OnSwipe += FingerGestures_OnSwipe;
		FingerGestures.OnDragBegin += FingerGestures_OnDragBegin;
		FingerGestures.OnDragMove += FingerGestures_OnDragMove;
		FingerGestures.OnDragEnd += FingerGestures_OnDragEnd;
		FingerGestures.OnPinchBegin += FingerGestures_OnPinchBegin;
		FingerGestures.OnPinchMove += FingerGestures_OnPinchMove;
		FingerGestures.OnPinchEnd += FingerGestures_OnPinchEnd;
		FingerGestures.OnRotationBegin += FingerGestures_OnRotationBegin;
		FingerGestures.OnRotationMove += FingerGestures_OnRotationMove;
		FingerGestures.OnRotationEnd += FingerGestures_OnRotationEnd;
		FingerGestures.OnTwoFingerLongPress += FingerGestures_OnTwoFingerLongPress;
		FingerGestures.OnTwoFingerTap += FingerGestures_OnTwoFingerTap;
		FingerGestures.OnTwoFingerSwipe += FingerGestures_OnTwoFingerSwipe;
		FingerGestures.OnTwoFingerDragBegin += FingerGestures_OnTwoFingerDragBegin;
		FingerGestures.OnTwoFingerDragMove += FingerGestures_OnTwoFingerDragMove;
		FingerGestures.OnTwoFingerDragEnd += FingerGestures_OnTwoFingerDragEnd;
	}

	public virtual void OnDisable()
	{
		FingerGestures.OnFingerDown -= FingerGestures_OnFingerDown;
		FingerGestures.OnFingerStationaryBegin -= FingerGestures_OnFingerStationaryBegin;
		FingerGestures.OnFingerStationary -= FingerGestures_OnFingerStationary;
		FingerGestures.OnFingerStationaryEnd -= FingerGestures_OnFingerStationaryEnd;
		FingerGestures.OnFingerMoveBegin -= FingerGestures_OnFingerMoveBegin;
		FingerGestures.OnFingerMove -= FingerGestures_OnFingerMove;
		FingerGestures.OnFingerMoveEnd -= FingerGestures_OnFingerMoveEnd;
		FingerGestures.OnFingerUp -= FingerGestures_OnFingerUp;
		FingerGestures.OnFingerLongPress -= FingerGestures_OnFingerLongPress;
		FingerGestures.OnFingerTap -= FingerGestures_OnFingerTap;
		FingerGestures.OnFingerSwipe -= FingerGestures_OnFingerSwipe;
		FingerGestures.OnFingerDragBegin -= FingerGestures_OnFingerDragBegin;
		FingerGestures.OnFingerDragMove -= FingerGestures_OnFingerDragMove;
		FingerGestures.OnFingerDragEnd -= FingerGestures_OnFingerDragEnd;
		FingerGestures.OnLongPress -= FingerGestures_OnLongPress;
		FingerGestures.OnTap -= FingerGestures_OnTap;
		FingerGestures.OnSwipe -= FingerGestures_OnSwipe;
		FingerGestures.OnDragBegin -= FingerGestures_OnDragBegin;
		FingerGestures.OnDragMove -= FingerGestures_OnDragMove;
		FingerGestures.OnDragEnd -= FingerGestures_OnDragEnd;
		FingerGestures.OnPinchBegin -= FingerGestures_OnPinchBegin;
		FingerGestures.OnPinchMove -= FingerGestures_OnPinchMove;
		FingerGestures.OnPinchEnd -= FingerGestures_OnPinchEnd;
		FingerGestures.OnRotationBegin -= FingerGestures_OnRotationBegin;
		FingerGestures.OnRotationMove -= FingerGestures_OnRotationMove;
		FingerGestures.OnRotationEnd -= FingerGestures_OnRotationEnd;
		FingerGestures.OnTwoFingerLongPress -= FingerGestures_OnTwoFingerLongPress;
		FingerGestures.OnTwoFingerTap -= FingerGestures_OnTwoFingerTap;
		FingerGestures.OnTwoFingerSwipe -= FingerGestures_OnTwoFingerSwipe;
		FingerGestures.OnTwoFingerDragBegin -= FingerGestures_OnTwoFingerDragBegin;
		FingerGestures.OnTwoFingerDragMove -= FingerGestures_OnTwoFingerDragMove;
		FingerGestures.OnTwoFingerDragEnd -= FingerGestures_OnTwoFingerDragEnd;
	}

	public virtual void FingerGestures_OnFingerDown(int fingerIndex, Vector2 fingerPos)
	{
	}

	public virtual void FingerGestures_OnFingerUp(int fingerIndex, Vector2 fingerPos, float timeHeldDown)
	{
	}

	public virtual void FingerGestures_OnFingerMoveBegin(int fingerIndex, Vector2 fingerPos)
	{
	}

	public virtual void FingerGestures_OnFingerMove(int fingerIndex, Vector2 fingerPos)
	{
	}

	public virtual void FingerGestures_OnFingerMoveEnd(int fingerIndex, Vector2 fingerPos)
	{
	}

	public virtual void FingerGestures_OnFingerStationaryBegin(int fingerIndex, Vector2 fingerPos)
	{
	}

	public virtual void FingerGestures_OnFingerStationary(int fingerIndex, Vector2 fingerPos, float elapsedTime)
	{
	}

	public virtual void FingerGestures_OnFingerStationaryEnd(int fingerIndex, Vector2 fingerPos, float elapsedTime)
	{
	}

	public virtual void FingerGestures_OnFingerLongPress(int fingerIndex, Vector2 fingerPos)
	{
	}

	public virtual void FingerGestures_OnFingerTap(int fingerIndex, Vector2 fingerPos, int tapCount)
	{
	}

	public virtual void FingerGestures_OnFingerSwipe(int fingerIndex, Vector2 startPos, FingerGestures.SwipeDirection direction, float velocity)
	{
	}

	public virtual void FingerGestures_OnFingerDragBegin(int fingerIndex, Vector2 fingerPos, Vector2 startPos)
	{
	}

	public virtual void FingerGestures_OnFingerDragMove(int fingerIndex, Vector2 fingerPos, Vector2 delta)
	{
	}

	public virtual void FingerGestures_OnFingerDragEnd(int fingerIndex, Vector2 fingerPos)
	{
	}

	public virtual void FingerGestures_OnLongPress(Vector2 fingerPos)
	{
	}

	public virtual void FingerGestures_OnTap(Vector2 fingerPos, int tapCount)
	{
	}

	public virtual void FingerGestures_OnSwipe(Vector2 startPos, FingerGestures.SwipeDirection direction, float velocity)
	{
	}

	public virtual void FingerGestures_OnDragBegin(Vector2 fingerPos, Vector2 startPos)
	{
	}

	public virtual void FingerGestures_OnDragMove(Vector2 fingerPos, Vector2 delta)
	{
	}

	public virtual void FingerGestures_OnDragEnd(Vector2 fingerPos)
	{
	}

	public virtual void FingerGestures_OnPinchBegin(Vector2 fingerPos1, Vector2 fingerPos2)
	{
	}

	public virtual void FingerGestures_OnPinchMove(Vector2 fingerPos1, Vector2 fingerPos2, float delta)
	{
	}

	public virtual void FingerGestures_OnPinchEnd(Vector2 fingerPos1, Vector2 fingerPos2)
	{
	}

	public virtual void FingerGestures_OnRotationBegin(Vector2 fingerPos1, Vector2 fingerPos2)
	{
	}

	public virtual void FingerGestures_OnRotationMove(Vector2 fingerPos1, Vector2 fingerPos2, float rotationAngleDelta)
	{
	}

	public virtual void FingerGestures_OnRotationEnd(Vector2 fingerPos1, Vector2 fingerPos2, float totalRotationAngle)
	{
	}

	public virtual void FingerGestures_OnTwoFingerLongPress(Vector2 fingerPos)
	{
	}

	public virtual void FingerGestures_OnTwoFingerTap(Vector2 fingerPos, int tapCount)
	{
	}

	public virtual void FingerGestures_OnTwoFingerSwipe(Vector2 startPos, FingerGestures.SwipeDirection direction, float velocity)
	{
	}

	public virtual void FingerGestures_OnTwoFingerDragBegin(Vector2 fingerPos, Vector2 startPos)
	{
	}

	public virtual void FingerGestures_OnTwoFingerDragMove(Vector2 fingerPos, Vector2 delta)
	{
	}

	public virtual void FingerGestures_OnTwoFingerDragEnd(Vector2 fingerPos)
	{
	}

	public virtual void Main()
	{
	}
}
[Serializable]
public class JiguanqiangControl : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	internal sealed class $jiguanqiangMuzzleFlash$222 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class $ : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal JiguanqiangControl $self_$223;

			public $(JiguanqiangControl self_)
			{
				$self_$223 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					if ((bool)$self_$223.muzzleFlash)
					{
						$self_$223.muzzleFlash.active = true;
					}
					if ((bool)$self_$223.pointLight)
					{
						$self_$223.pointLight.enabled = true;
					}
					result = (Yield(2, new WaitForSeconds(0.04f)) ? 1 : 0);
					break;
				case 2:
					if ((bool)$self_$223.muzzleFlash)
					{
						$self_$223.muzzleFlash.active = false;
					}
					if ((bool)$self_$223.pointLight)
					{
						$self_$223.pointLight.enabled = false;
					}
					YieldDefault(1);
					goto case 1;
				case 1:
					result = 0;
					break;
				}
				return (byte)result != 0;
			}
		}

		internal JiguanqiangControl $self_$224;

		public $jiguanqiangMuzzleFlash$222(JiguanqiangControl self_)
		{
			$self_$224 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new $($self_$224);
		}
	}

	public Transform firePoint;

	public GameObject jiguanqiangbullet;

	public GameObject muzzleFlash;

	public Light pointLight;

	public AudioClip fireSound;

	private float fireRate;

	private float jiguanqiangDamage;

	private float attackTime;

	private float attackTimeDelay;

	private float attackStartTime;

	private bool fire;

	private float nextFireTime;

	private bool turnLeft;

	private bool turnRight;

	private bool turnDown;

	private bool turnUp;

	private bool jiguanqiangStart;

	public JiguanqiangControl()
	{
		nextFireTime = -1f;
		turnLeft = true;
		turnDown = true;
	}

	public virtual void Start()
	{
		switch (PlayerPrefs.GetInt("SingleModeChapterOneDifficulty", 1))
		{
		case 1:
			jiguanqiangDamage = 22f;
			fireRate = 0.15f;
			attackTime = UnityEngine.Random.Range(3f, 4f);
			attackTimeDelay = UnityEngine.Random.Range(1f, 2f);
			break;
		case 2:
			jiguanqiangDamage = 20f;
			fireRate = 0.11f;
			attackTime = UnityEngine.Random.Range(4.5f, 6f);
			attackTimeDelay = UnityEngine.Random.Range(0.5f, 1f);
			break;
		case 3:
			jiguanqiangDamage = 18f;
			fireRate = 0.09f;
			attackTime = UnityEngine.Random.Range(5.5f, 6f);
			attackTimeDelay = UnityEngine.Random.Range(0f, 0.5f);
			break;
		}
		transform.localEulerAngles += new Vector3(0f, UnityEngine.Random.Range(-8f, 20f), UnityEngine.Random.Range(-28f, 28f));
		muzzleFlash.active = false;
		pointLight.enabled = false;
	}

	public virtual void Update()
	{
		if (jiguanqiangStart)
		{
			attackStartTime += Time.deltaTime;
			if (!(attackStartTime <= attackTimeDelay) && !(attackStartTime > attackTimeDelay + attackTime))
			{
				fire = true;
			}
			if (!(attackStartTime <= attackTimeDelay + attackTime))
			{
				fire = false;
				attackStartTime = 0f;
			}
			if (fire)
			{
				Jiguanqiangfire();
			}
			if (!(transform.localEulerAngles.z <= 135f))
			{
				turnLeft = true;
				turnRight = false;
			}
			if (!(transform.localEulerAngles.z >= 55f))
			{
				turnLeft = false;
				turnRight = true;
			}
			if (!(transform.localEulerAngles.y <= 30f))
			{
				turnDown = true;
				turnUp = false;
			}
			if (transform.localEulerAngles.y < 1f || !(transform.localEulerAngles.y <= 355f))
			{
				turnDown = false;
				turnUp = true;
			}
			if (turnLeft)
			{
				float z = transform.localEulerAngles.z - 0.2f;
				Vector3 localEulerAngles = transform.localEulerAngles;
				float num = (localEulerAngles.z = z);
				Vector3 vector = (transform.localEulerAngles = localEulerAngles);
			}
			if (turnRight)
			{
				float z2 = transform.localEulerAngles.z + 0.2f;
				Vector3 localEulerAngles2 = transform.localEulerAngles;
				float num2 = (localEulerAngles2.z = z2);
				Vector3 vector3 = (transform.localEulerAngles = localEulerAngles2);
			}
			if (turnDown)
			{
				float y = transform.localEulerAngles.y - 0.3f;
				Vector3 localEulerAngles3 = transform.localEulerAngles;
				float num3 = (localEulerAngles3.y = y);
				Vector3 vector5 = (transform.localEulerAngles = localEulerAngles3);
			}
			if (turnUp)
			{
				float y2 = transform.localEulerAngles.y + 0.3f;
				Vector3 localEulerAngles4 = transform.localEulerAngles;
				float num4 = (localEulerAngles4.y = y2);
				Vector3 vector7 = (transform.localEulerAngles = localEulerAngles4);
			}
		}
	}

	public virtual void Jiguanqiangfire()
	{
		if (!(Time.time - fireRate <= nextFireTime))
		{
			nextFireTime = Time.time - Time.deltaTime;
		}
		while (nextFireTime < Time.time)
		{
			JiguanqiangOneShot();
			nextFireTime += fireRate;
		}
	}

	public virtual void JiguanqiangOneShot()
	{
		GameObject gameObject = ((GameObject)UnityEngine.Object.Instantiate(jiguanqiangbullet, firePoint.position, firePoint.rotation)) as GameObject;
		((Bullet)gameObject.GetComponent(typeof(Bullet))).bulletDamage = jiguanqiangDamage;
		StartCoroutine_Auto(jiguanqiangMuzzleFlash());
		audio.clip = fireSound;
		audio.Play();
	}

	public virtual IEnumerator jiguanqiangMuzzleFlash()
	{
		return new $jiguanqiangMuzzleFlash$222(this).GetEnumerator();
	}

	public virtual void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
			jiguanqiangStart = true;
		}
	}

	public virtual void OnTriggerExit(Collider other)
	{
		if (other.gameObject.tag == "Player")
		{
			jiguanqiangStart = false;
		}
	}
}
[Serializable]
public class JiguanqiangControl1 : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	internal sealed class $jiguanqiangMuzzleFlash$225 : GenericGenerator<WaitForSeconds>
	{
		[Serializable]
		[CompilerGenerated]
		internal sealed class $ : GenericGeneratorEnumerator<WaitForSeconds>, IEnumerator
		{
			internal JiguanqiangControl1 $self_$226;

			public $(JiguanqiangControl1 self_)
			{
				$self_$226 = self_;
			}

			public override bool MoveNext()
			{
				int result;
				switch (_state)
				{
				default:
					if ((bool)$self_$226.muzzleFlash)
					{
						$self_$226.muzzleFlash.active = true;
					}
					if ((bool)$self_$226.pointLight)
					{
						$self_$226.pointLight.enabled = true;
					}
					result = (Yield(2, new WaitForSeconds(0.04f)) ? 1 : 0);
					break;
				case 2:
					if ((bool)$self_$226.muzzleFlash)
					{
						$self_$226.muzzleFlash.active = false;
					}
					if ((bool)$self_$226.pointLight)
					{
						$self_$226.pointLight.enabled = false;
					}
					YieldDefault(1);
					goto case 1;
				case 1:
					result = 0;
					break;
				}
				return (byte)result != 0;
			}
		}

		internal JiguanqiangControl1 $self_$227;

		public $jiguanqiangMuzzleFlash$225(JiguanqiangControl1 self_)
		{
			$self_$227 = self_;
		}

		public override IEnumerator<WaitForSeconds> GetEnumerator()
		{
			return new $($self_$227);
		}
	}

	public Transform firePoint;

	public GameObject jiguanqiangbullet;

	public GameObject muzzleFlash;

	public Light pointLight;

	public AudioClip fireSound;

	private float fireRate;

	private float jiguanqiangDamage;

	private float attackTime;

	private float attackTimeDelay;

	private float attackStartTime;

	private bool fire;

	private float nextFireTime;

	private bool turnLeft;

	private bool turnRight;

	private bool turnDown;

	private bool turnUp;

	public JiguanqiangControl1()
	{
		nextFireTime = -1f;
		turnLeft = true;
		turnDown = true;
	}

	public virtual void Start()
	{
		switch (PlayerPrefs.GetInt("SingleModeChapterOneDifficulty", 1))
		{
		case 1:
			jiguanqiangDamage = 20f;
			fireRate = 0.15f;
			attackTime = UnityEngine.Random.Range(3f, 4f);
			attackTimeDelay = UnityEngine.Random.Range(1f, 2f);
			break;
		case 2:
			jiguanqiangDamage = 15f;
			fireRate = 0.12f;
			attackTime = UnityEngine.Random.Range(4.5f, 6f);
			attackTimeDelay = UnityEngine.Random.Range(0.5f, 1f);
			break;
		case 3:
			jiguanqiangDamage = 14f;
			fireRate = 0.09f;
			attackTime = UnityEngine.Random.Range(5.5f, 6f);
			attackTimeDelay = UnityEngine.Random.Range(0f, 0.5f);
			break;
		}
		transform.localEulerAngles += new Vector3(0f, 0f, UnityEngine.Random.Range(-28f, 28f));
		muzzleFlash.active = false;
		pointLight.enabled = false;
	}

	public virtual void Update()
	{
		attackStartTime += Time.deltaTime;
		if (!(attackStartTime <= attackTimeDelay) && !(attackStartTime > attackTimeDelay + attackTime))
		{
			fire = true;
		}
		if (!(attackStartTime <= attackTimeDelay + attackTime))
		{
			fire = false;
			attackStartTime = 0f;
		}
		if (fire)
		{
			Jiguanqiangfire();
		}
		if (!(transform.localEulerAngles.z <= 180f))
		{
			turnLeft = true;
			turnRight = false;
		}
		if (!(transform.localEulerAngles.z >= 2f))
		{
			turnLeft = false;
			turnRight = true;
		}
		if (turnLeft)
		{
			float z = transform.localEulerAngles.z - 0.2f;
			Vector3 localEulerAngles = transform.localEulerAngles;
			float num = (localEulerAngles.z = z);
			Vector3 vector = (transform.localEulerAngles = localEulerAngles);
		}
		if (turnRight)
		{
			float z2 = transform.localEulerAngles.z + 0.2f;
			Vector3 localEulerAngles2 = transform.localEulerAngles;
			float num2 = (localEulerAngles2.z = z2);
			Vector3 vector3 = (transform.localEulerAngles = localEulerAngles2);
		}
	}

	public virtual void Jiguanqiangfire()
	{
		if (!(Time.time - fireRate <= nextFireTime))
		{
			nextFireTime = Time.time - Time.deltaTime;
		}
		while (nextFireTime < Time.time)
		{
			JiguanqiangOneShot();
			nextFireTime += fireRate;
		}
	}

	public virtual void JiguanqiangOneShot()
	{
		GameObject gameObject = ((GameObject)UnityEngine.Object.Instantiate(jiguanqiangbullet, firePoint.position, firePoint.rotation)) as GameObject;
		((Bullet)gameObject.GetComponent(typeof(Bullet))).bulletDamage = jiguanqiangDamage;
		StartCoroutine_Auto(jiguanqiangMuzzleFlash());
		audio.clip = fireSound;
		audio.Play();
	}

	public virtual IEnumerator jiguanqiangMuzzleFlash()
	{
		return new $jiguanqiangMuzzleFlash$225(this).GetEnumerator();
	}
}
[Serializable]
public class MuzzleFX : MonoBehaviour
{
	public float scaleParam;

	public Vector3 scaleVector;

	public float rotationSpeed;

	public MuzzleFX()
	{
		scaleParam = 0.5f;
		scaleVector = Vector3.one;
		rotationSpeed = 0.5f;
	}

	public virtual void Update()
	{
		transform.localScale = scaleVector * UnityEngine.Random.Range(scaleParam, scaleParam * 3f);
		float z = UnityEngine.Random.Range(0f, 90f * rotationSpeed);
		Vector3 localEulerAngles = transform.localEulerAngles;
		float num = (localEulerAngles.z = z);
		Vector3 vector = (transform.localEulerAngles = localEulerAngles);
	}

	public virtual void Main()
	{
	}
}
[Serializable]
[RequireComponent(typeof(LineRenderer))]
public class TraceFX : MonoBehaviour
{
	public float ActivityTime;

	public float minZ;

	public float maxZ;

	public float speed;

	private LineRenderer Trace;

	private float RemainActivityTime;

	private float z;

	public TraceFX()
	{
		ActivityTime = 0.1f;
	}

	public virtual void Start()
	{
		Trace = (LineRenderer)gameObject.GetComponent(typeof(LineRenderer));
		z = minZ;
		RemainActivityTime = Time.time + ActivityTime;
	}

	public virtual void Update()
	{
		if (!(RemainActivityTime > Time.time))
		{
			if (!(z >= maxZ))
			{
				z += speed;
			}
			else
			{
				z = minZ;
				gameObject.SetActive(value: false);
				RemainActivityTime = Time.time + ActivityTime;
			}
			Trace.SetPosition(1, new Vector3(0f, 0f, z));
		}
	}

	public virtual void OnEnable()
	{
		z = minZ;
	}

	public virtual void Main()
	{
	}
}
[Serializable]
public class weaponFX
{
	public string caption;

	public GameObject FXObject;

	public bool randomize;

	public bool resetTransform;

	public float ActivityTime;

	private float RemainActivityTime;

	public weaponFX()
	{
		ActivityTime = 0.1f;
	}

	public virtual void Init()
	{
	}

	public virtual void Process()
	{
		if (!(RemainActivityTime > Time.time))
		{
			SetFXActive(active: false);
		}
	}

	public virtual void Activate()
	{
		bool fXActive = true;
		if (randomize && UnityEngine.Random.Range(-1, 1) < 0)
		{
			fXActive = false;
		}
		RemainActivityTime = Time.time + ActivityTime;
		SetFXActive(fXActive);
	}

	public virtual void SetFXActive(bool active)
	{
		if ((bool)FXObject)
		{
			FXObject.SetActive(active);
		}
	}
}
[Serializable]
[AddComponentMenu("EasyWeapons/WeaponFX/Weapon FX Manager")]
public class WeaponFX : MonoBehaviour
{
	public weaponFX[] FXs;

	public virtual void Start()
	{
		for (int i = 0; i < FXs.Length; i++)
		{
			if ((bool)FXs[i].FXObject && FXs[i].resetTransform)
			{
				FXs[i].FXObject.SetActive(value: false);
				FXs[i].FXObject.transform.position = transform.position;
				FXs[i].FXObject.transform.rotation = transform.rotation;
				FXs[i].FXObject.transform.parent = transform;
			}
		}
	}

	public virtual void Activate()
	{
		for (int i = 0; i < FXs.Length; i++)
		{
			FXs[i].Activate();
		}
	}

	public virtual void Update()
	{
		for (int i = 0; i < FXs.Length; i++)
		{
			FXs[i].Process();
		}
	}

	public virtual void Main()
	{
	}
}
