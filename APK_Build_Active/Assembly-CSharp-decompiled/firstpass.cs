using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using KamcordJSON;
using Tests;
using Unibill;
using Unibill.Impl;
using Uniject;
using Uniject.Impl;
using UnityEngine;
using unibill.Dummy;

[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: AssemblyVersion("0.0.0.0")]
public abstract class AveragedGestureRecognizer : GestureRecognizer
{
	public int RequiredFingerCount = 1;

	private Vector2 startPos = Vector2.zero;

	private Vector2 pos = Vector2.zero;

	public Vector2 StartPosition
	{
		get
		{
			return startPos;
		}
		protected set
		{
			startPos = value;
		}
	}

	public Vector2 Position
	{
		get
		{
			return pos;
		}
		protected set
		{
			pos = value;
		}
	}

	protected override int GetRequiredFingerCount()
	{
		return RequiredFingerCount;
	}
}
public abstract class FGComponent : MonoBehaviour
{
	public delegate void EventDelegate<T>(T source) where T : FGComponent;

	protected virtual void Awake()
	{
	}

	protected virtual void Start()
	{
	}

	protected virtual void OnEnable()
	{
		FingerGestures.OnFingersUpdated += FingerGestures_OnFingersUpdated;
	}

	protected virtual void OnDisable()
	{
		FingerGestures.OnFingersUpdated -= FingerGestures_OnFingersUpdated;
	}

	private void FingerGestures_OnFingersUpdated()
	{
		OnUpdate(FingerGestures.Touches);
	}

	protected abstract void OnUpdate(FingerGestures.IFingerList touches);
}
public abstract class GestureRecognizer : FGComponent
{
	public enum GestureState
	{
		Ready,
		InProgress,
		Failed,
		Recognized
	}

	public enum GestureResetMode
	{
		NextFrame,
		EndOfTouchSequence,
		StartOfTouchSequence
	}

	public delegate bool CanBeginDelegate(GestureRecognizer gr, FingerGestures.IFingerList touches);

	private GestureState prevState;

	private GestureState state;

	public GestureResetMode ResetMode = GestureResetMode.StartOfTouchSequence;

	private int lastTouchesCount;

	private CanBeginDelegate canBeginDelegate;

	private FingerGestures.ITouchFilter touchFilter;

	public GestureState PreviousState => prevState;

	public GestureState State
	{
		get
		{
			return state;
		}
		protected set
		{
			if (state != value)
			{
				prevState = state;
				state = value;
				if (this.OnStateChanged != null)
				{
					this.OnStateChanged(this);
				}
			}
		}
	}

	public bool IsActive => State == GestureState.InProgress;

	public FingerGestures.ITouchFilter TouchFilter
	{
		get
		{
			return touchFilter;
		}
		set
		{
			touchFilter = value;
		}
	}

	public event EventDelegate<GestureRecognizer> OnStateChanged;

	protected virtual void Reset()
	{
		State = GestureState.Ready;
	}

	protected override void Start()
	{
		base.Start();
		Reset();
	}

	protected virtual void OnTouchSequenceStarted()
	{
		if (ResetMode == GestureResetMode.StartOfTouchSequence && (State == GestureState.Recognized || State == GestureState.Failed))
		{
			Reset();
		}
	}

	protected virtual void OnTouchSequenceEnded()
	{
		if (ResetMode == GestureResetMode.EndOfTouchSequence && (State == GestureState.Recognized || State == GestureState.Failed))
		{
			Reset();
		}
	}

	protected override void OnUpdate(FingerGestures.IFingerList touches)
	{
		if (touchFilter != null)
		{
			touches = touchFilter.Apply(touches);
		}
		if (touches.Count > 0 && lastTouchesCount == 0)
		{
			OnTouchSequenceStarted();
		}
		switch (State)
		{
		case GestureState.Failed:
		case GestureState.Recognized:
			if (ResetMode == GestureResetMode.NextFrame)
			{
				Reset();
			}
			break;
		case GestureState.Ready:
			State = OnReady(touches);
			break;
		case GestureState.InProgress:
			State = OnActive(touches);
			break;
		default:
			Debug.LogError(string.Concat(this, " - Unhandled state: ", State, ". Failing recognizer."));
			State = GestureState.Failed;
			break;
		}
		if (touches.Count == 0 && lastTouchesCount > 0)
		{
			OnTouchSequenceEnded();
		}
		lastTouchesCount = touches.Count;
	}

	protected virtual GestureState OnReady(FingerGestures.IFingerList touches)
	{
		if (ShouldFailFromReady(touches))
		{
			return GestureState.Failed;
		}
		if (CanBegin(touches))
		{
			OnBegin(touches);
			return GestureState.InProgress;
		}
		return GestureState.Ready;
	}

	protected virtual bool ShouldFailFromReady(FingerGestures.IFingerList touches)
	{
		if (touches.Count != GetRequiredFingerCount() && touches.Count > 0 && !Young(touches))
		{
			return true;
		}
		return false;
	}

	protected virtual bool CanBegin(FingerGestures.IFingerList touches)
	{
		if (touches.Count != GetRequiredFingerCount())
		{
			return false;
		}
		if (!CheckCanBeginDelegate(touches))
		{
			return false;
		}
		return true;
	}

	public virtual bool CheckCanBeginDelegate(FingerGestures.IFingerList touches)
	{
		if (canBeginDelegate != null && !canBeginDelegate(this, touches))
		{
			return false;
		}
		return true;
	}

	public void SetCanBeginDelegate(CanBeginDelegate f)
	{
		canBeginDelegate = f;
	}

	public CanBeginDelegate GetCanBeginDelegate()
	{
		return canBeginDelegate;
	}

	protected abstract int GetRequiredFingerCount();

	protected abstract void OnBegin(FingerGestures.IFingerList touches);

	protected abstract GestureState OnActive(FingerGestures.IFingerList touches);

	protected bool Young(FingerGestures.IFingerList touches)
	{
		FingerGestures.Finger oldest = touches.GetOldest();
		if (oldest == null)
		{
			return false;
		}
		float num = Time.time - oldest.StarTime;
		return num < 0.25f;
	}
}
public abstract class MultiFingerGestureRecognizer : GestureRecognizer
{
	private Vector2[] startPos;

	private Vector2[] pos;

	protected Vector2[] StartPosition
	{
		get
		{
			return startPos;
		}
		set
		{
			startPos = value;
		}
	}

	protected Vector2[] Position
	{
		get
		{
			return pos;
		}
		set
		{
			pos = value;
		}
	}

	public int RequiredFingerCount => GetRequiredFingerCount();

	protected override void Start()
	{
		base.Start();
		OnFingerCountChanged(GetRequiredFingerCount());
	}

	protected void OnFingerCountChanged(int fingerCount)
	{
		StartPosition = new Vector2[fingerCount];
		Position = new Vector2[fingerCount];
	}

	public Vector2 GetPosition(int index)
	{
		return pos[index];
	}

	public Vector2 GetStartPosition(int index)
	{
		return startPos[index];
	}
}
[AddComponentMenu("FingerGestures/Gesture Recognizers/Drag")]
public class DragGestureRecognizer : AveragedGestureRecognizer
{
	public float MoveTolerance = 5f;

	private Vector2 delta = Vector2.zero;

	private Vector2 lastPos = Vector2.zero;

	public Vector2 MoveDelta
	{
		get
		{
			return delta;
		}
		private set
		{
			delta = value;
		}
	}

	public event EventDelegate<DragGestureRecognizer> OnDragBegin;

	public event EventDelegate<DragGestureRecognizer> OnDragMove;

	public event EventDelegate<DragGestureRecognizer> OnDragEnd;

	protected override bool CanBegin(FingerGestures.IFingerList touches)
	{
		if (!base.CanBegin(touches))
		{
			return false;
		}
		if (touches.GetAverageDistanceFromStart() < MoveTolerance)
		{
			return false;
		}
		return true;
	}

	protected override void OnBegin(FingerGestures.IFingerList touches)
	{
		base.Position = touches.GetAveragePosition();
		base.StartPosition = base.Position;
		MoveDelta = Vector2.zero;
		lastPos = base.Position;
		RaiseOnDragBegin();
	}

	protected override GestureState OnActive(FingerGestures.IFingerList touches)
	{
		if (touches.Count != RequiredFingerCount)
		{
			if (touches.Count < RequiredFingerCount)
			{
				RaiseOnDragEnd();
				return GestureState.Recognized;
			}
			return GestureState.Failed;
		}
		base.Position = touches.GetAveragePosition();
		MoveDelta = base.Position - lastPos;
		if (MoveDelta.sqrMagnitude > 0f)
		{
			RaiseOnDragMove();
			lastPos = base.Position;
		}
		return GestureState.InProgress;
	}

	protected void RaiseOnDragBegin()
	{
		if (this.OnDragBegin != null)
		{
			this.OnDragBegin(this);
		}
	}

	protected void RaiseOnDragMove()
	{
		if (this.OnDragMove != null)
		{
			this.OnDragMove(this);
		}
	}

	protected void RaiseOnDragEnd()
	{
		if (this.OnDragEnd != null)
		{
			this.OnDragEnd(this);
		}
	}
}
public class FingerMotionDetector : FGComponent
{
	public enum MotionState
	{
		None,
		Stationary,
		Moving
	}

	public float MoveThreshold = 5f;

	private FingerGestures.Finger finger;

	private MotionState state;

	private MotionState prevState;

	private int moves;

	private float stationaryStartTime;

	private Vector2 anchorPos = Vector2.zero;

	private bool wasDown;

	public virtual FingerGestures.Finger Finger
	{
		get
		{
			return finger;
		}
		set
		{
			finger = value;
		}
	}

	protected MotionState State
	{
		get
		{
			return state;
		}
		private set
		{
			state = value;
		}
	}

	protected MotionState PreviousState
	{
		get
		{
			return prevState;
		}
		private set
		{
			prevState = value;
		}
	}

	public int Moves
	{
		get
		{
			return moves;
		}
		private set
		{
			moves = value;
		}
	}

	public bool Moved => Moves > 0;

	public bool WasMoving => PreviousState == MotionState.Moving;

	public bool Moving => State == MotionState.Moving;

	public float ElapsedStationaryTime => Time.time - stationaryStartTime;

	public Vector2 AnchorPos
	{
		get
		{
			return anchorPos;
		}
		private set
		{
			anchorPos = value;
		}
	}

	public event EventDelegate<FingerMotionDetector> OnMoveBegin;

	public event EventDelegate<FingerMotionDetector> OnMove;

	public event EventDelegate<FingerMotionDetector> OnMoveEnd;

	public event EventDelegate<FingerMotionDetector> OnStationaryBegin;

	public event EventDelegate<FingerMotionDetector> OnStationary;

	public event EventDelegate<FingerMotionDetector> OnStationaryEnd;

	protected override void OnUpdate(FingerGestures.IFingerList touches)
	{
		if (Finger.IsDown)
		{
			if (!wasDown)
			{
				Moves = 0;
				AnchorPos = Finger.Position;
				State = MotionState.Stationary;
			}
			if (Finger.Phase == FingerGestures.FingerPhase.Moved)
			{
				if (State != MotionState.Moving)
				{
					if ((Finger.Position - AnchorPos).sqrMagnitude >= MoveThreshold * MoveThreshold)
					{
						State = MotionState.Moving;
					}
					else
					{
						State = MotionState.Stationary;
					}
				}
			}
			else
			{
				State = MotionState.Stationary;
			}
		}
		else
		{
			State = MotionState.None;
		}
		RaiseEvents();
		PreviousState = State;
		wasDown = Finger.IsDown;
	}

	private void RaiseEvents()
	{
		if (State != PreviousState)
		{
			if (PreviousState == MotionState.Moving)
			{
				RaiseOnMoveEnd();
				AnchorPos = Finger.Position;
			}
			else if (PreviousState == MotionState.Stationary)
			{
				RaiseOnStationaryEnd();
			}
			if (State == MotionState.Moving)
			{
				RaiseOnMoveBegin();
				Moves++;
			}
			else if (State == MotionState.Stationary)
			{
				stationaryStartTime = Time.time;
				RaiseOnStationaryBegin();
			}
		}
		if (State == MotionState.Stationary)
		{
			RaiseOnStationary();
		}
		else if (State == MotionState.Moving)
		{
			RaiseOnMove();
		}
	}

	protected void RaiseOnMoveBegin()
	{
		if (this.OnMoveBegin != null)
		{
			this.OnMoveBegin(this);
		}
	}

	protected void RaiseOnMove()
	{
		if (this.OnMove != null)
		{
			this.OnMove(this);
		}
	}

	protected void RaiseOnMoveEnd()
	{
		if (this.OnMoveEnd != null)
		{
			this.OnMoveEnd(this);
		}
	}

	protected void RaiseOnStationaryBegin()
	{
		if (this.OnStationaryBegin != null)
		{
			this.OnStationaryBegin(this);
		}
	}

	protected void RaiseOnStationary()
	{
		if (this.OnStationary != null)
		{
			this.OnStationary(this);
		}
	}

	protected void RaiseOnStationaryEnd()
	{
		if (this.OnStationaryEnd != null)
		{
			this.OnStationaryEnd(this);
		}
	}
}
[AddComponentMenu("FingerGestures/Gesture Recognizers/Long Press")]
public class LongPressGestureRecognizer : AveragedGestureRecognizer
{
	public float Duration = 1f;

	public float MoveTolerance = 5f;

	private float startTime;

	public float StartTime => startTime;

	public event EventDelegate<LongPressGestureRecognizer> OnLongPress;

	protected override void OnBegin(FingerGestures.IFingerList touches)
	{
		base.Position = touches.GetAveragePosition();
		base.StartPosition = base.Position;
		startTime = Time.time;
	}

	protected override GestureState OnActive(FingerGestures.IFingerList touches)
	{
		if (touches.Count != RequiredFingerCount)
		{
			return GestureState.Failed;
		}
		float num = Time.time - startTime;
		if (num >= Duration)
		{
			RaiseOnLongPress();
			return GestureState.Recognized;
		}
		if (touches.GetAverageDistanceFromStart() > MoveTolerance)
		{
			return GestureState.Failed;
		}
		return GestureState.InProgress;
	}

	protected void RaiseOnLongPress()
	{
		if (this.OnLongPress != null)
		{
			this.OnLongPress(this);
		}
	}
}
[AddComponentMenu("FingerGestures/Gesture Recognizers/Mouse Pinch")]
public class MousePinchGestureRecognizer : PinchGestureRecognizer
{
	public string axis = "Mouse ScrollWheel";

	private int requiredFingers = 2;

	private float resetTime;

	protected override int GetRequiredFingerCount()
	{
		return requiredFingers;
	}

	protected override bool CanBegin(FingerGestures.IFingerList touches)
	{
		if (!CheckCanBeginDelegate(touches))
		{
			return false;
		}
		float f = Input.GetAxis(axis);
		if (Mathf.Abs(f) < 0.0001f)
		{
			return false;
		}
		return true;
	}

	protected override void OnBegin(FingerGestures.IFingerList touches)
	{
		ref Vector2 reference = ref base.StartPosition[0];
		ref Vector2 reference2 = ref base.StartPosition[1];
		reference = (reference2 = Input.mousePosition);
		ref Vector2 reference3 = ref base.Position[0];
		ref Vector2 reference4 = ref base.Position[1];
		reference3 = (reference4 = Input.mousePosition);
		delta = 0f;
		RaiseOnPinchBegin();
		delta = DeltaScale * Input.GetAxis(axis);
		resetTime = Time.time + 0.1f;
		RaiseOnPinchMove();
	}

	protected override GestureState OnActive(FingerGestures.IFingerList touches)
	{
		float num = Input.GetAxis(axis);
		if (Mathf.Abs(num) < 0.001f)
		{
			if (resetTime <= Time.time)
			{
				RaiseOnPinchEnd();
				return GestureState.Recognized;
			}
			return GestureState.InProgress;
		}
		resetTime = Time.time + 0.1f;
		ref Vector2 reference = ref base.Position[0];
		ref Vector2 reference2 = ref base.Position[1];
		reference = (reference2 = Input.mousePosition);
		delta = DeltaScale * num;
		RaiseOnPinchMove();
		return GestureState.InProgress;
	}
}
[AddComponentMenu("FingerGestures/Gesture Recognizers/Pinch")]
public class PinchGestureRecognizer : MultiFingerGestureRecognizer
{
	public float MinDOT = -0.7f;

	public float MinDistance = 5f;

	public float DeltaScale = 1f;

	protected float delta;

	public float Delta => delta;

	public event EventDelegate<PinchGestureRecognizer> OnPinchBegin;

	public event EventDelegate<PinchGestureRecognizer> OnPinchMove;

	public event EventDelegate<PinchGestureRecognizer> OnPinchEnd;

	protected override int GetRequiredFingerCount()
	{
		return 2;
	}

	protected override bool CanBegin(FingerGestures.IFingerList touches)
	{
		if (!base.CanBegin(touches))
		{
			return false;
		}
		FingerGestures.Finger finger = touches[0];
		FingerGestures.Finger finger2 = touches[1];
		if (!FingerGestures.AllFingersMoving(finger, finger2))
		{
			return false;
		}
		if (!FingersMovedInOppositeDirections(finger, finger2))
		{
			return false;
		}
		float f = ComputeGapDelta(finger, finger2, finger.StartPosition, finger2.StartPosition);
		if (Mathf.Abs(f) < MinDistance)
		{
			return false;
		}
		return true;
	}

	protected override void OnBegin(FingerGestures.IFingerList touches)
	{
		FingerGestures.Finger finger = touches[0];
		FingerGestures.Finger finger2 = touches[1];
		ref Vector2 reference = ref base.StartPosition[0];
		reference = finger.StartPosition;
		ref Vector2 reference2 = ref base.StartPosition[1];
		reference2 = finger2.StartPosition;
		ref Vector2 reference3 = ref base.Position[0];
		reference3 = finger.Position;
		ref Vector2 reference4 = ref base.Position[1];
		reference4 = finger2.Position;
		RaiseOnPinchBegin();
		float num = ComputeGapDelta(finger, finger2, finger.StartPosition, finger2.StartPosition);
		delta = DeltaScale * (num - Mathf.Sign(num) * MinDistance);
		RaiseOnPinchMove();
	}

	protected override GestureState OnActive(FingerGestures.IFingerList touches)
	{
		if (touches.Count != base.RequiredFingerCount)
		{
			if (touches.Count < base.RequiredFingerCount)
			{
				RaiseOnPinchEnd();
				return GestureState.Recognized;
			}
			return GestureState.Failed;
		}
		FingerGestures.Finger finger = touches[0];
		FingerGestures.Finger finger2 = touches[1];
		ref Vector2 reference = ref base.Position[0];
		reference = finger.Position;
		ref Vector2 reference2 = ref base.Position[1];
		reference2 = finger2.Position;
		if (!FingerGestures.AllFingersMoving(finger, finger2))
		{
			return GestureState.InProgress;
		}
		float num = ComputeGapDelta(finger, finger2, finger.PreviousPosition, finger2.PreviousPosition);
		if (Mathf.Abs(num) > 0.001f)
		{
			if (!FingersMovedInOppositeDirections(finger, finger2))
			{
				return GestureState.InProgress;
			}
			delta = DeltaScale * num;
			RaiseOnPinchMove();
		}
		return GestureState.InProgress;
	}

	protected void RaiseOnPinchBegin()
	{
		if (this.OnPinchBegin != null)
		{
			this.OnPinchBegin(this);
		}
	}

	protected void RaiseOnPinchMove()
	{
		if (this.OnPinchMove != null)
		{
			this.OnPinchMove(this);
		}
	}

	protected void RaiseOnPinchEnd()
	{
		if (this.OnPinchEnd != null)
		{
			this.OnPinchEnd(this);
		}
	}

	private bool FingersMovedInOppositeDirections(FingerGestures.Finger finger0, FingerGestures.Finger finger1)
	{
		return FingerGestures.FingersMovedInOppositeDirections(finger0, finger1, MinDOT);
	}

	private float ComputeGapDelta(FingerGestures.Finger finger0, FingerGestures.Finger finger1, Vector2 refPos1, Vector2 refPos2)
	{
		Vector2 vector = finger0.Position - finger1.Position;
		Vector2 vector2 = refPos1 - refPos2;
		return vector.magnitude - vector2.magnitude;
	}
}
[AddComponentMenu("FingerGestures/Gesture Recognizers/Rotation")]
public class RotationGestureRecognizer : MultiFingerGestureRecognizer
{
	public float MinDOT = -0.7f;

	public float MinRotation = 1f;

	private float totalRotation;

	private float rotationDelta;

	public float TotalRotation => totalRotation;

	public float RotationDelta => rotationDelta;

	public event EventDelegate<RotationGestureRecognizer> OnRotationBegin;

	public event EventDelegate<RotationGestureRecognizer> OnRotationMove;

	public event EventDelegate<RotationGestureRecognizer> OnRotationEnd;

	private bool FingersMovedInOppositeDirections(FingerGestures.Finger finger0, FingerGestures.Finger finger1)
	{
		return FingerGestures.FingersMovedInOppositeDirections(finger0, finger1, MinDOT);
	}

	private static float SignedAngularGap(FingerGestures.Finger finger0, FingerGestures.Finger finger1, Vector2 refPos0, Vector2 refPos1)
	{
		Vector2 normalized = (finger0.Position - finger1.Position).normalized;
		Vector2 normalized2 = (refPos0 - refPos1).normalized;
		return 57.29578f * FingerGestures.SignedAngle(normalized2, normalized);
	}

	protected override int GetRequiredFingerCount()
	{
		return 2;
	}

	protected override bool CanBegin(FingerGestures.IFingerList touches)
	{
		if (!base.CanBegin(touches))
		{
			return false;
		}
		FingerGestures.Finger finger = touches[0];
		FingerGestures.Finger finger2 = touches[1];
		if (!FingerGestures.AllFingersMoving(finger, finger2))
		{
			return false;
		}
		if (!FingersMovedInOppositeDirections(finger, finger2))
		{
			return false;
		}
		float f = SignedAngularGap(finger, finger2, finger.StartPosition, finger2.StartPosition);
		if (Mathf.Abs(f) < MinRotation)
		{
			return false;
		}
		return true;
	}

	protected override void OnBegin(FingerGestures.IFingerList touches)
	{
		FingerGestures.Finger finger = touches[0];
		FingerGestures.Finger finger2 = touches[1];
		ref Vector2 reference = ref base.StartPosition[0];
		reference = finger.StartPosition;
		ref Vector2 reference2 = ref base.StartPosition[1];
		reference2 = finger2.StartPosition;
		ref Vector2 reference3 = ref base.Position[0];
		reference3 = finger.Position;
		ref Vector2 reference4 = ref base.Position[1];
		reference4 = finger2.Position;
		float num = SignedAngularGap(finger, finger2, finger.StartPosition, finger2.StartPosition);
		totalRotation = Mathf.Sign(num) * MinRotation;
		rotationDelta = 0f;
		if (this.OnRotationBegin != null)
		{
			this.OnRotationBegin(this);
		}
		rotationDelta = num - totalRotation;
		totalRotation = num;
		if (this.OnRotationMove != null)
		{
			this.OnRotationMove(this);
		}
	}

	protected override GestureState OnActive(FingerGestures.IFingerList touches)
	{
		if (touches.Count != base.RequiredFingerCount)
		{
			if (touches.Count < base.RequiredFingerCount)
			{
				if (this.OnRotationEnd != null)
				{
					this.OnRotationEnd(this);
				}
				return GestureState.Recognized;
			}
			return GestureState.Failed;
		}
		FingerGestures.Finger finger = touches[0];
		FingerGestures.Finger finger2 = touches[1];
		ref Vector2 reference = ref base.Position[0];
		reference = finger.Position;
		ref Vector2 reference2 = ref base.Position[1];
		reference2 = finger2.Position;
		if (!FingerGestures.AllFingersMoving(finger, finger2))
		{
			return GestureState.InProgress;
		}
		rotationDelta = SignedAngularGap(finger, finger2, finger.PreviousPosition, finger2.PreviousPosition);
		totalRotation += rotationDelta;
		if (this.OnRotationMove != null)
		{
			this.OnRotationMove(this);
		}
		return GestureState.InProgress;
	}
}
[AddComponentMenu("FingerGestures/Gesture Recognizers/Swipe")]
public class SwipeGestureRecognizer : AveragedGestureRecognizer
{
	public FingerGestures.SwipeDirection ValidDirections = FingerGestures.SwipeDirection.All;

	public float MinDistance = 1f;

	public float MinVelocity = 1f;

	public float DirectionTolerance = 0.2f;

	private Vector2 move;

	private FingerGestures.SwipeDirection direction;

	private float velocity;

	private float startTime;

	public Vector2 Move
	{
		get
		{
			return move;
		}
		private set
		{
			move = value;
		}
	}

	public FingerGestures.SwipeDirection Direction => direction;

	public float Velocity => velocity;

	public event EventDelegate<SwipeGestureRecognizer> OnSwipe;

	public bool IsValidDirection(FingerGestures.SwipeDirection dir)
	{
		if (dir == FingerGestures.SwipeDirection.None)
		{
			return false;
		}
		return (ValidDirections & dir) == dir;
	}

	protected override bool CanBegin(FingerGestures.IFingerList touches)
	{
		if (!base.CanBegin(touches))
		{
			return false;
		}
		if (touches.GetAverageDistanceFromStart() < 0.5f)
		{
			return false;
		}
		return true;
	}

	protected override void OnBegin(FingerGestures.IFingerList touches)
	{
		base.Position = touches.GetAveragePosition();
		base.StartPosition = base.Position;
		direction = FingerGestures.SwipeDirection.None;
		startTime = Time.time;
	}

	protected override GestureState OnActive(FingerGestures.IFingerList touches)
	{
		if (touches.Count != RequiredFingerCount)
		{
			if (touches.Count < RequiredFingerCount && direction != FingerGestures.SwipeDirection.None)
			{
				if (this.OnSwipe != null)
				{
					this.OnSwipe(this);
				}
				return GestureState.Recognized;
			}
			return GestureState.Failed;
		}
		base.Position = touches.GetAveragePosition();
		Move = base.Position - base.StartPosition;
		float magnitude = Move.magnitude;
		if (magnitude < MinDistance)
		{
			return GestureState.InProgress;
		}
		float num = Time.time - startTime;
		if (num > 0f)
		{
			velocity = magnitude / num;
		}
		else
		{
			velocity = 0f;
		}
		if (velocity < MinVelocity)
		{
			return GestureState.Failed;
		}
		FingerGestures.SwipeDirection swipeDirection = FingerGestures.GetSwipeDirection(Move.normalized, DirectionTolerance);
		if (!IsValidDirection(swipeDirection) || (direction != FingerGestures.SwipeDirection.None && swipeDirection != direction))
		{
			return GestureState.Failed;
		}
		direction = swipeDirection;
		return GestureState.InProgress;
	}
}
[AddComponentMenu("FingerGestures/Gesture Recognizers/Tap")]
public class TapGestureRecognizer : AveragedGestureRecognizer
{
	public int RequiredTaps;

	public bool RaiseEventOnEachTap;

	public float MaxDelayBetweenTaps = 0.25f;

	public float MaxDuration;

	public float MoveTolerance = 5f;

	private int taps;

	private bool down;

	private bool wasDown;

	private float lastDownTime;

	private float lastTapTime;

	private float startTime;

	public int Taps => taps;

	public event EventDelegate<TapGestureRecognizer> OnTap;

	private bool MovedTooFar(Vector2 curPos)
	{
		return (curPos - base.StartPosition).sqrMagnitude >= MoveTolerance * MoveTolerance;
	}

	private bool HasTimedOut()
	{
		if (MaxDelayBetweenTaps > 0f && Time.time - lastTapTime > MaxDelayBetweenTaps)
		{
			return true;
		}
		if (MaxDuration > 0f && Time.time - startTime > MaxDuration)
		{
			return true;
		}
		return false;
	}

	protected override void Reset()
	{
		taps = 0;
		down = false;
		wasDown = false;
		base.Reset();
	}

	protected override void OnBegin(FingerGestures.IFingerList touches)
	{
		base.Position = touches.GetAveragePosition();
		base.StartPosition = base.Position;
		lastTapTime = Time.time;
		startTime = Time.time;
	}

	protected override GestureState OnActive(FingerGestures.IFingerList touches)
	{
		wasDown = down;
		down = false;
		if (touches.Count == RequiredFingerCount)
		{
			down = true;
			lastDownTime = Time.time;
		}
		else if (touches.Count == 0)
		{
			down = false;
		}
		else
		{
			if (touches.Count >= RequiredFingerCount)
			{
				return GestureState.Failed;
			}
			if (Time.time - lastDownTime > 0.25f)
			{
				return GestureState.Failed;
			}
		}
		if (HasTimedOut())
		{
			if (RequiredTaps == 0 && Taps > 0)
			{
				if (!RaiseEventOnEachTap)
				{
					RaiseOnTap();
				}
				return GestureState.Recognized;
			}
			return GestureState.Failed;
		}
		if (down)
		{
			Vector2 averagePosition = touches.GetAveragePosition();
			if (MovedTooFar(averagePosition))
			{
				return GestureState.Failed;
			}
		}
		if (wasDown != down && !down)
		{
			taps++;
			lastTapTime = Time.time;
			if (RequiredTaps > 0 && taps >= RequiredTaps)
			{
				RaiseOnTap();
				return GestureState.Recognized;
			}
			if (RaiseEventOnEachTap)
			{
				RaiseOnTap();
			}
		}
		return GestureState.InProgress;
	}

	protected void RaiseOnTap()
	{
		if (this.OnTap != null)
		{
			this.OnTap(this);
		}
	}
}
public abstract class FingerGestures : MonoBehaviour
{
	public enum FingerPhase
	{
		None,
		Began,
		Moved,
		Stationary,
		Ended
	}

	public class Finger
	{
		public delegate void FingerEventDelegate(Finger finger);

		private int index;

		private bool wasDown;

		private bool down;

		private float startTime;

		private FingerPhase phase;

		private Vector2 startPos = Vector2.zero;

		private Vector2 pos = Vector2.zero;

		private Vector2 prevPos = Vector2.zero;

		private Vector2 deltaPos = Vector2.zero;

		private float distFromStart;

		public int Index => index;

		public FingerPhase Phase => phase;

		public bool IsDown => down;

		public bool WasDown => wasDown;

		public float StarTime => startTime;

		public Vector2 StartPosition => startPos;

		public Vector2 Position => pos;

		public Vector2 PreviousPosition => prevPos;

		public Vector2 DeltaPosition => deltaPos;

		public float DistanceFromStart => distFromStart;

		public event FingerEventDelegate OnDown;

		public event FingerEventDelegate OnUp;

		public Finger(int index)
		{
			this.index = index;
		}

		public override string ToString()
		{
			return "Finger" + index;
		}

		internal void Update(FingerPhase newPhase, Vector2 newPos)
		{
			if (phase != newPhase)
			{
				if (newPhase == FingerPhase.None && phase != FingerPhase.Ended)
				{
					Debug.LogWarning("Correcting bad FingerPhase transition (FingerPhase.Ended skipped)");
					Update(FingerPhase.Ended, PreviousPosition);
					return;
				}
				if (!down && (newPhase == FingerPhase.Moved || newPhase == FingerPhase.Stationary))
				{
					Debug.LogWarning("Correcting bad FingerPhase transition (FingerPhase.Began skipped)");
					Update(FingerPhase.Began, newPos);
					return;
				}
				if ((down && newPhase == FingerPhase.Began) || (!down && newPhase == FingerPhase.Ended))
				{
					Debug.LogWarning(string.Concat("Invalid state FingerPhase transition from ", phase, " to ", newPhase, " - Skipping."));
					return;
				}
			}
			else if (newPhase == FingerPhase.Began || newPhase == FingerPhase.Ended)
			{
				Debug.LogWarning("Duplicated FingerPhase." + newPhase.ToString() + " - skipping.");
				return;
			}
			if (newPhase != FingerPhase.None)
			{
				if (newPhase == FingerPhase.Ended)
				{
					down = false;
				}
				else
				{
					if (newPhase == FingerPhase.Began)
					{
						down = true;
						startPos = newPos;
						prevPos = newPos;
						startTime = Time.time;
					}
					prevPos = pos;
					pos = newPos;
					deltaPos = pos - prevPos;
					distFromStart = Vector3.Distance(startPos, pos);
				}
			}
			phase = newPhase;
		}

		internal void PostUpdate()
		{
			if (wasDown != down)
			{
				if (down)
				{
					if (this.OnDown != null)
					{
						this.OnDown(this);
					}
				}
				else if (this.OnUp != null)
				{
					this.OnUp(this);
				}
			}
			wasDown = down;
		}
	}

	[Serializable]
	public class DefaultComponentCreationFlags
	{
		[Serializable]
		public class PerFinger
		{
			public bool enabled = true;

			public bool touch = true;

			public bool motion = true;

			public bool longPress = true;

			public bool drag = true;

			public bool swipe = true;

			public bool tap = true;
		}

		[Serializable]
		public class GlobalGestures
		{
			public bool enabled = true;

			public bool longPress = true;

			public bool drag = true;

			public bool swipe = true;

			public bool tap = true;

			public bool pinch = true;

			public bool rotation = true;

			public bool twoFingerLongPress = true;

			public bool twoFingerDrag = true;

			public bool twoFingerSwipe = true;

			public bool twoFingerTap = true;
		}

		public PerFinger perFinger;

		public GlobalGestures globalGestures;
	}

	public class DefaultComponents
	{
		public class FingerComponents
		{
			public FingerMotionDetector Motion;

			public LongPressGestureRecognizer LongPress;

			public DragGestureRecognizer Drag;

			public TapGestureRecognizer Tap;

			public SwipeGestureRecognizer Swipe;
		}

		private FingerComponents[] fingers;

		public LongPressGestureRecognizer LongPress;

		public DragGestureRecognizer Drag;

		public TapGestureRecognizer Tap;

		public SwipeGestureRecognizer Swipe;

		public PinchGestureRecognizer Pinch;

		public RotationGestureRecognizer Rotation;

		public LongPressGestureRecognizer TwoFingerLongPress;

		public DragGestureRecognizer TwoFingerDrag;

		public TapGestureRecognizer TwoFingerTap;

		public SwipeGestureRecognizer TwoFingerSwipe;

		public FingerComponents[] Fingers => fingers;

		public DefaultComponents(int fingerCount)
		{
			fingers = new FingerComponents[fingerCount];
			for (int i = 0; i < fingers.Length; i++)
			{
				fingers[i] = new FingerComponents();
			}
		}
	}

	public interface IFingerList : IEnumerable<Finger>, IEnumerable
	{
		Finger this[int index] { get; }

		int Count { get; }

		Vector2 GetAveragePosition();

		Vector2 GetAveragePreviousPosition();

		float GetAverageDistanceFromStart();

		Finger GetOldest();
	}

	public class FingerList : IEnumerable<Finger>, IFingerList, IEnumerable
	{
		public delegate T FingerPropertyGetterDelegate<T>(Finger finger);

		private List<Finger> list;

		public Finger this[int index] => list[index];

		public int Count => list.Count;

		public FingerList()
		{
			list = new List<Finger>();
		}

		public FingerList(List<Finger> list)
		{
			this.list = list;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<Finger> GetEnumerator()
		{
			return list.GetEnumerator();
		}

		public void Add(Finger touch)
		{
			list.Add(touch);
		}

		public void Clear()
		{
			list.Clear();
		}

		public Vector2 AverageVector(FingerPropertyGetterDelegate<Vector2> getProperty)
		{
			Vector2 zero = Vector2.zero;
			if (Count > 0)
			{
				foreach (Finger item in list)
				{
					zero += getProperty(item);
				}
				zero /= (float)Count;
			}
			return zero;
		}

		public float AverageFloat(FingerPropertyGetterDelegate<float> getProperty)
		{
			float num = 0f;
			if (Count > 0)
			{
				foreach (Finger item in list)
				{
					num += getProperty(item);
				}
				num /= (float)Count;
			}
			return num;
		}

		private static Vector2 GetFingerPosition(Finger finger)
		{
			return finger.Position;
		}

		private static Vector2 GetFingerPreviousPosition(Finger finger)
		{
			return finger.PreviousPosition;
		}

		private static float GetFingerDistanceFromStart(Finger finger)
		{
			return finger.DistanceFromStart;
		}

		public Vector2 GetAveragePosition()
		{
			return AverageVector(GetFingerPosition);
		}

		public Vector2 GetAveragePreviousPosition()
		{
			return AverageVector(GetFingerPreviousPosition);
		}

		public float GetAverageDistanceFromStart()
		{
			return AverageFloat(GetFingerDistanceFromStart);
		}

		public Finger GetOldest()
		{
			Finger finger = null;
			foreach (Finger item in list)
			{
				if (finger == null || item.StarTime < finger.StarTime)
				{
					finger = item;
				}
			}
			return finger;
		}
	}

	[Flags]
	public enum SwipeDirection
	{
		Right = 1,
		Left = 2,
		Up = 4,
		Down = 8,
		None = 0,
		All = 0xF,
		Vertical = 0xC,
		Horizontal = 3
	}

	public interface ITouchFilter
	{
		IFingerList Apply(IFingerList touches);
	}

	public class SingleFingerFilter : ITouchFilter
	{
		private FingerList fingerList = new FingerList();

		private FingerList emptyList = new FingerList();

		private Finger finger;

		public Finger Finger => finger;

		public SingleFingerFilter(Finger finger)
		{
			this.finger = finger;
			fingerList.Add(finger);
		}

		public IFingerList Apply(IFingerList touches)
		{
			foreach (Finger touch in touches)
			{
				if (touch == Finger)
				{
					return fingerList;
				}
			}
			return emptyList;
		}
	}

	public delegate void FingerDownEventHandler(int fingerIndex, Vector2 fingerPos);

	public delegate void FingerUpEventHandler(int fingerIndex, Vector2 fingerPos, float timeHeldDown);

	public delegate void FingerStationaryBeginEventHandler(int fingerIndex, Vector2 fingerPos);

	public delegate void FingerStationaryEventHandler(int fingerIndex, Vector2 fingerPos, float elapsedTime);

	public delegate void FingerStationaryEndEventHandler(int fingerIndex, Vector2 fingerPos, float elapsedTime);

	public delegate void FingerMoveEventHandler(int fingerIndex, Vector2 fingerPos);

	public delegate void FingerLongPressEventHandler(int fingerIndex, Vector2 fingerPos);

	public delegate void FingerTapEventHandler(int fingerIndex, Vector2 fingerPos, int tapCount);

	public delegate void FingerSwipeEventHandler(int fingerIndex, Vector2 startPos, SwipeDirection direction, float velocity);

	public delegate void FingerDragBeginEventHandler(int fingerIndex, Vector2 fingerPos, Vector2 startPos);

	public delegate void FingerDragMoveEventHandler(int fingerIndex, Vector2 fingerPos, Vector2 delta);

	public delegate void FingerDragEndEventHandler(int fingerIndex, Vector2 fingerPos);

	public delegate void LongPressEventHandler(Vector2 fingerPos);

	public delegate void TapEventHandler(Vector2 fingerPos, int tapCount);

	public delegate void SwipeEventHandler(Vector2 startPos, SwipeDirection direction, float velocity);

	public delegate void DragBeginEventHandler(Vector2 fingerPos, Vector2 startPos);

	public delegate void DragMoveEventHandler(Vector2 fingerPos, Vector2 delta);

	public delegate void DragEndEventHandler(Vector2 fingerPos);

	public delegate void PinchEventHandler(Vector2 fingerPos1, Vector2 fingerPos2);

	public delegate void PinchMoveEventHandler(Vector2 fingerPos1, Vector2 fingerPos2, float delta);

	public delegate void RotationBeginEventHandler(Vector2 fingerPos1, Vector2 fingerPos2);

	public delegate void RotationMoveEventHandler(Vector2 fingerPos1, Vector2 fingerPos2, float rotationAngleDelta);

	public delegate void RotationEndEventHandler(Vector2 fingerPos1, Vector2 fingerPos2, float totalRotationAngle);

	public delegate void FingersUpdatedEventDelegate();

	private static FingerGestures instance;

	private Finger[] fingers;

	private FingerList touches = new FingerList();

	public FingerGesturesPrefabs defaultPrefabs;

	private Transform globalComponentNode;

	private Transform[] fingerComponentNodes;

	public DefaultComponentCreationFlags defaultCompFlags;

	private DefaultComponents defaultComponents;

	public static FingerGestures Instance => instance;

	public static IFingerList Touches => instance.touches;

	public abstract int MaxFingers { get; }

	public static DefaultComponents Defaults => instance.defaultComponents;

	public static event FingerDownEventHandler OnFingerDown;

	public static event FingerUpEventHandler OnFingerUp;

	public static event FingerStationaryBeginEventHandler OnFingerStationaryBegin;

	public static event FingerStationaryEventHandler OnFingerStationary;

	public static event FingerStationaryEndEventHandler OnFingerStationaryEnd;

	public static event FingerMoveEventHandler OnFingerMoveBegin;

	public static event FingerMoveEventHandler OnFingerMove;

	public static event FingerMoveEventHandler OnFingerMoveEnd;

	public static event FingerLongPressEventHandler OnFingerLongPress;

	public static event FingerDragBeginEventHandler OnFingerDragBegin;

	public static event FingerDragMoveEventHandler OnFingerDragMove;

	public static event FingerDragEndEventHandler OnFingerDragEnd;

	public static event FingerTapEventHandler OnFingerTap;

	public static event FingerSwipeEventHandler OnFingerSwipe;

	public static event LongPressEventHandler OnLongPress;

	public static event DragBeginEventHandler OnDragBegin;

	public static event DragMoveEventHandler OnDragMove;

	public static event DragEndEventHandler OnDragEnd;

	public static event TapEventHandler OnTap;

	public static event SwipeEventHandler OnSwipe;

	public static event PinchEventHandler OnPinchBegin;

	public static event PinchMoveEventHandler OnPinchMove;

	public static event PinchEventHandler OnPinchEnd;

	public static event RotationBeginEventHandler OnRotationBegin;

	public static event RotationMoveEventHandler OnRotationMove;

	public static event RotationEndEventHandler OnRotationEnd;

	public static event DragBeginEventHandler OnTwoFingerDragBegin;

	public static event DragMoveEventHandler OnTwoFingerDragMove;

	public static event DragEndEventHandler OnTwoFingerDragEnd;

	public static event TapEventHandler OnTwoFingerTap;

	public static event SwipeEventHandler OnTwoFingerSwipe;

	public static event LongPressEventHandler OnTwoFingerLongPress;

	public static event FingersUpdatedEventDelegate OnFingersUpdated;

	internal static void RaiseOnFingerDown(int fingerIndex, Vector2 fingerPos)
	{
		if (FingerGestures.OnFingerDown != null)
		{
			FingerGestures.OnFingerDown(fingerIndex, fingerPos);
		}
	}

	internal static void RaiseOnFingerUp(int fingerIndex, Vector2 fingerPos, float timeHeldDown)
	{
		if (FingerGestures.OnFingerUp != null)
		{
			FingerGestures.OnFingerUp(fingerIndex, fingerPos, timeHeldDown);
		}
	}

	internal static void RaiseOnFingerStationaryBegin(int fingerIndex, Vector2 fingerPos)
	{
		if (FingerGestures.OnFingerStationaryBegin != null)
		{
			FingerGestures.OnFingerStationaryBegin(fingerIndex, fingerPos);
		}
	}

	internal static void RaiseOnFingerStationary(int fingerIndex, Vector2 fingerPos, float elapsedTime)
	{
		if (FingerGestures.OnFingerStationary != null)
		{
			FingerGestures.OnFingerStationary(fingerIndex, fingerPos, elapsedTime);
		}
	}

	internal static void RaiseOnFingerStationaryEnd(int fingerIndex, Vector2 fingerPos, float elapsedTime)
	{
		if (FingerGestures.OnFingerStationaryEnd != null)
		{
			FingerGestures.OnFingerStationaryEnd(fingerIndex, fingerPos, elapsedTime);
		}
	}

	internal static void RaiseOnFingerMoveBegin(int fingerIndex, Vector2 fingerPos)
	{
		if (FingerGestures.OnFingerMoveBegin != null)
		{
			FingerGestures.OnFingerMoveBegin(fingerIndex, fingerPos);
		}
	}

	internal static void RaiseOnFingerMove(int fingerIndex, Vector2 fingerPos)
	{
		if (FingerGestures.OnFingerMove != null)
		{
			FingerGestures.OnFingerMove(fingerIndex, fingerPos);
		}
	}

	internal static void RaiseOnFingerMoveEnd(int fingerIndex, Vector2 fingerPos)
	{
		if (FingerGestures.OnFingerMoveEnd != null)
		{
			FingerGestures.OnFingerMoveEnd(fingerIndex, fingerPos);
		}
	}

	internal static void RaiseOnFingerLongPress(int fingerIndex, Vector2 fingerPos)
	{
		if (FingerGestures.OnFingerLongPress != null)
		{
			FingerGestures.OnFingerLongPress(fingerIndex, fingerPos);
		}
	}

	internal static void RaiseOnFingerDragBegin(int fingerIndex, Vector2 fingerPos, Vector2 startPos)
	{
		if (FingerGestures.OnFingerDragBegin != null)
		{
			FingerGestures.OnFingerDragBegin(fingerIndex, fingerPos, startPos);
		}
	}

	internal static void RaiseOnFingerDragMove(int fingerIndex, Vector2 fingerPos, Vector2 delta)
	{
		if (FingerGestures.OnFingerDragMove != null)
		{
			FingerGestures.OnFingerDragMove(fingerIndex, fingerPos, delta);
		}
	}

	internal static void RaiseOnFingerDragEnd(int fingerIndex, Vector2 fingerPos)
	{
		if (FingerGestures.OnFingerDragEnd != null)
		{
			FingerGestures.OnFingerDragEnd(fingerIndex, fingerPos);
		}
	}

	internal static void RaiseOnFingerTap(int fingerIndex, Vector2 fingerPos, int tapCount)
	{
		if (FingerGestures.OnFingerTap != null)
		{
			FingerGestures.OnFingerTap(fingerIndex, fingerPos, tapCount);
		}
	}

	internal static void RaiseOnFingerSwipe(int fingerIndex, Vector2 startPos, SwipeDirection direction, float velocity)
	{
		if (FingerGestures.OnFingerSwipe != null)
		{
			FingerGestures.OnFingerSwipe(fingerIndex, startPos, direction, velocity);
		}
	}

	internal static void RaiseOnLongPress(Vector2 fingerPos)
	{
		if (FingerGestures.OnLongPress != null)
		{
			FingerGestures.OnLongPress(fingerPos);
		}
	}

	internal static void RaiseOnDragBegin(Vector2 fingerPos, Vector2 startPos)
	{
		if (FingerGestures.OnDragBegin != null)
		{
			FingerGestures.OnDragBegin(fingerPos, startPos);
		}
	}

	internal static void RaiseOnDragMove(Vector2 fingerPos, Vector2 delta)
	{
		if (FingerGestures.OnDragMove != null)
		{
			FingerGestures.OnDragMove(fingerPos, delta);
		}
	}

	internal static void RaiseOnDragEnd(Vector2 fingerPos)
	{
		if (FingerGestures.OnDragEnd != null)
		{
			FingerGestures.OnDragEnd(fingerPos);
		}
	}

	internal static void RaiseOnTap(Vector2 fingerPos, int tapCount)
	{
		if (FingerGestures.OnTap != null)
		{
			FingerGestures.OnTap(fingerPos, tapCount);
		}
	}

	internal static void RaiseOnSwipe(Vector2 startPos, SwipeDirection direction, float velocity)
	{
		if (FingerGestures.OnSwipe != null)
		{
			FingerGestures.OnSwipe(startPos, direction, velocity);
		}
	}

	internal static void RaiseOnPinchBegin(Vector2 fingerPos1, Vector2 fingerPos2)
	{
		if (FingerGestures.OnPinchBegin != null)
		{
			FingerGestures.OnPinchBegin(fingerPos1, fingerPos2);
		}
	}

	internal static void RaiseOnPinchMove(Vector2 fingerPos1, Vector2 fingerPos2, float delta)
	{
		if (FingerGestures.OnPinchMove != null)
		{
			FingerGestures.OnPinchMove(fingerPos1, fingerPos2, delta);
		}
	}

	internal static void RaiseOnPinchEnd(Vector2 fingerPos1, Vector2 fingerPos2)
	{
		if (FingerGestures.OnPinchEnd != null)
		{
			FingerGestures.OnPinchEnd(fingerPos1, fingerPos2);
		}
	}

	internal static void RaiseOnRotationBegin(Vector2 fingerPos1, Vector2 fingerPos2)
	{
		if (FingerGestures.OnRotationBegin != null)
		{
			FingerGestures.OnRotationBegin(fingerPos1, fingerPos2);
		}
	}

	internal static void RaiseOnRotationMove(Vector2 fingerPos1, Vector2 fingerPos2, float rotationAngleDelta)
	{
		if (FingerGestures.OnRotationMove != null)
		{
			FingerGestures.OnRotationMove(fingerPos1, fingerPos2, rotationAngleDelta);
		}
	}

	internal static void RaiseOnRotationEnd(Vector2 fingerPos1, Vector2 fingerPos2, float totalRotationAngle)
	{
		if (FingerGestures.OnRotationEnd != null)
		{
			FingerGestures.OnRotationEnd(fingerPos1, fingerPos2, totalRotationAngle);
		}
	}

	internal static void RaiseOnTwoFingerLongPress(Vector2 fingerPos)
	{
		if (FingerGestures.OnTwoFingerLongPress != null)
		{
			FingerGestures.OnTwoFingerLongPress(fingerPos);
		}
	}

	internal static void RaiseOnTwoFingerDragBegin(Vector2 fingerPos, Vector2 startPos)
	{
		if (FingerGestures.OnTwoFingerDragBegin != null)
		{
			FingerGestures.OnTwoFingerDragBegin(fingerPos, startPos);
		}
	}

	internal static void RaiseOnTwoFingerDragMove(Vector2 fingerPos, Vector2 delta)
	{
		if (FingerGestures.OnTwoFingerDragMove != null)
		{
			FingerGestures.OnTwoFingerDragMove(fingerPos, delta);
		}
	}

	internal static void RaiseOnTwoFingerDragEnd(Vector2 fingerPos)
	{
		if (FingerGestures.OnTwoFingerDragEnd != null)
		{
			FingerGestures.OnTwoFingerDragEnd(fingerPos);
		}
	}

	internal static void RaiseOnTwoFingerTap(Vector2 fingerPos, int tapCount)
	{
		if (FingerGestures.OnTwoFingerTap != null)
		{
			FingerGestures.OnTwoFingerTap(fingerPos, tapCount);
		}
	}

	internal static void RaiseOnTwoFingerSwipe(Vector2 startPos, SwipeDirection direction, float velocity)
	{
		if (FingerGestures.OnTwoFingerSwipe != null)
		{
			FingerGestures.OnTwoFingerSwipe(startPos, direction, velocity);
		}
	}

	public static Finger GetFinger(int index)
	{
		return instance.fingers[index];
	}

	protected virtual void OnEnable()
	{
		instance = this;
		InitFingers(MaxFingers);
	}

	protected virtual void Start()
	{
		if (fingers == null)
		{
			InitFingers(MaxFingers);
		}
	}

	protected virtual void Update()
	{
		UpdateFingers();
		if (FingerGestures.OnFingersUpdated != null)
		{
			FingerGestures.OnFingersUpdated();
		}
	}

	protected abstract FingerPhase GetPhase(Finger finger);

	protected abstract Vector2 GetPosition(Finger finger);

	private void InitFingers(int count)
	{
		if (fingers == null)
		{
			fingers = new Finger[count];
			for (int i = 0; i < count; i++)
			{
				fingers[i] = new Finger(i);
			}
		}
		InitDefaultComponents();
	}

	private void UpdateFingers()
	{
		touches.Clear();
		Finger[] array = fingers;
		foreach (Finger finger in array)
		{
			Vector2 newPos = Vector2.zero;
			FingerPhase phase = GetPhase(finger);
			if (phase != FingerPhase.None)
			{
				newPos = GetPosition(finger);
			}
			finger.Update(phase, newPos);
			if (finger.IsDown)
			{
				touches.Add(finger);
			}
		}
		Finger[] array2 = fingers;
		foreach (Finger finger2 in array2)
		{
			finger2.PostUpdate();
		}
	}

	private T CreateDefaultComponent<T>(T prefab, Transform parent) where T : FGComponent
	{
		T result = UnityEngine.Object.Instantiate(prefab) as T;
		result.gameObject.name = prefab.name;
		result.transform.parent = parent;
		return result;
	}

	private T CreateDefaultGlobalComponent<T>(T prefab) where T : FGComponent
	{
		return CreateDefaultComponent(prefab, globalComponentNode);
	}

	private T CreateDefaultFingerComponent<T>(Finger finger, T prefab) where T : FGComponent
	{
		return CreateDefaultComponent(prefab, fingerComponentNodes[finger.Index]);
	}

	private Transform CreateNode(string name, Transform parent)
	{
		GameObject gameObject = new GameObject(name);
		gameObject.transform.parent = parent;
		return gameObject.transform;
	}

	private void InitDefaultComponents()
	{
		int num = fingers.Length;
		if ((bool)globalComponentNode)
		{
			UnityEngine.Object.Destroy(globalComponentNode.gameObject);
		}
		if (fingerComponentNodes != null)
		{
			Transform[] array = fingerComponentNodes;
			foreach (Transform transform in array)
			{
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
		globalComponentNode = CreateNode("Global Components", base.transform);
		fingerComponentNodes = new Transform[num];
		for (int j = 0; j < fingerComponentNodes.Length; j++)
		{
			fingerComponentNodes[j] = CreateNode("Finger" + j, base.transform);
		}
		defaultComponents = new DefaultComponents(num);
		if (defaultCompFlags.globalGestures.enabled)
		{
			InitGlobalGestures();
		}
		if (defaultCompFlags.perFinger.enabled)
		{
			Finger[] array2 = fingers;
			foreach (Finger finger in array2)
			{
				InitDefaultComponents(finger);
			}
		}
	}

	private void InitGlobalGestures()
	{
		if (defaultCompFlags.globalGestures.longPress)
		{
			LongPressGestureRecognizer longPressGestureRecognizer = CreateDefaultGlobalComponent(defaultPrefabs.longPress);
			longPressGestureRecognizer.OnLongPress += delegate(LongPressGestureRecognizer rec)
			{
				RaiseOnLongPress(rec.Position);
			};
			defaultComponents.LongPress = longPressGestureRecognizer;
		}
		if (defaultCompFlags.globalGestures.twoFingerLongPress)
		{
			LongPressGestureRecognizer longPressGestureRecognizer2 = CreateDefaultGlobalComponent(defaultPrefabs.twoFingerLongPress);
			longPressGestureRecognizer2.RequiredFingerCount = 2;
			longPressGestureRecognizer2.OnLongPress += delegate(LongPressGestureRecognizer rec)
			{
				RaiseOnTwoFingerLongPress(rec.Position);
			};
			defaultComponents.TwoFingerLongPress = longPressGestureRecognizer2;
		}
		if (defaultCompFlags.globalGestures.drag)
		{
			DragGestureRecognizer dragGestureRecognizer = CreateDefaultGlobalComponent(defaultPrefabs.drag);
			dragGestureRecognizer.OnDragBegin += delegate(DragGestureRecognizer rec)
			{
				RaiseOnDragBegin(rec.Position, rec.StartPosition);
			};
			dragGestureRecognizer.OnDragMove += delegate(DragGestureRecognizer rec)
			{
				RaiseOnDragMove(rec.Position, rec.MoveDelta);
			};
			dragGestureRecognizer.OnDragEnd += delegate(DragGestureRecognizer rec)
			{
				RaiseOnDragEnd(rec.Position);
			};
			defaultComponents.Drag = dragGestureRecognizer;
		}
		if (defaultCompFlags.globalGestures.twoFingerDrag)
		{
			DragGestureRecognizer dragGestureRecognizer2 = CreateDefaultGlobalComponent(defaultPrefabs.twoFingerDrag);
			dragGestureRecognizer2.RequiredFingerCount = 2;
			dragGestureRecognizer2.OnDragBegin += delegate(DragGestureRecognizer rec)
			{
				RaiseOnTwoFingerDragBegin(rec.Position, rec.StartPosition);
			};
			dragGestureRecognizer2.OnDragMove += delegate(DragGestureRecognizer rec)
			{
				RaiseOnTwoFingerDragMove(rec.Position, rec.MoveDelta);
			};
			dragGestureRecognizer2.OnDragEnd += delegate(DragGestureRecognizer rec)
			{
				RaiseOnTwoFingerDragEnd(rec.Position);
			};
			defaultComponents.TwoFingerDrag = dragGestureRecognizer2;
		}
		if (defaultCompFlags.globalGestures.swipe)
		{
			SwipeGestureRecognizer swipeGestureRecognizer = CreateDefaultGlobalComponent(defaultPrefabs.swipe);
			swipeGestureRecognizer.OnSwipe += delegate(SwipeGestureRecognizer rec)
			{
				RaiseOnSwipe(rec.StartPosition, rec.Direction, rec.Velocity);
			};
			defaultComponents.Swipe = swipeGestureRecognizer;
		}
		if (defaultCompFlags.globalGestures.twoFingerSwipe)
		{
			SwipeGestureRecognizer swipeGestureRecognizer2 = CreateDefaultGlobalComponent(defaultPrefabs.twoFingerSwipe);
			swipeGestureRecognizer2.RequiredFingerCount = 2;
			swipeGestureRecognizer2.OnSwipe += delegate(SwipeGestureRecognizer rec)
			{
				RaiseOnTwoFingerSwipe(rec.StartPosition, rec.Direction, rec.Velocity);
			};
			defaultComponents.TwoFingerSwipe = swipeGestureRecognizer2;
		}
		if (defaultCompFlags.globalGestures.tap)
		{
			TapGestureRecognizer tapGestureRecognizer = CreateDefaultGlobalComponent(defaultPrefabs.tap);
			tapGestureRecognizer.RequiredTaps = 0;
			tapGestureRecognizer.OnTap += delegate(TapGestureRecognizer rec)
			{
				RaiseOnTap(rec.Position, rec.Taps);
			};
			defaultComponents.Tap = tapGestureRecognizer;
		}
		if (defaultCompFlags.globalGestures.twoFingerTap)
		{
			TapGestureRecognizer tapGestureRecognizer2 = CreateDefaultGlobalComponent(defaultPrefabs.twoFingerTap);
			tapGestureRecognizer2.RequiredFingerCount = 2;
			tapGestureRecognizer2.RequiredTaps = 0;
			tapGestureRecognizer2.OnTap += delegate(TapGestureRecognizer rec)
			{
				RaiseOnTwoFingerTap(rec.Position, rec.Taps);
			};
			defaultComponents.TwoFingerTap = tapGestureRecognizer2;
		}
		if (defaultCompFlags.globalGestures.pinch)
		{
			PinchGestureRecognizer pinchGestureRecognizer = CreateDefaultGlobalComponent(defaultPrefabs.pinch);
			pinchGestureRecognizer.OnPinchBegin += delegate(PinchGestureRecognizer rec)
			{
				RaiseOnPinchBegin(rec.GetPosition(0), rec.GetPosition(1));
			};
			pinchGestureRecognizer.OnPinchMove += delegate(PinchGestureRecognizer rec)
			{
				RaiseOnPinchMove(rec.GetPosition(0), rec.GetPosition(1), rec.Delta);
			};
			pinchGestureRecognizer.OnPinchEnd += delegate(PinchGestureRecognizer rec)
			{
				RaiseOnPinchEnd(rec.GetPosition(0), rec.GetPosition(1));
			};
			defaultComponents.Pinch = pinchGestureRecognizer;
		}
		if (defaultCompFlags.globalGestures.rotation)
		{
			RotationGestureRecognizer rotationGestureRecognizer = CreateDefaultGlobalComponent(defaultPrefabs.rotation);
			rotationGestureRecognizer.OnRotationBegin += delegate(RotationGestureRecognizer rec)
			{
				RaiseOnRotationBegin(rec.GetPosition(0), rec.GetPosition(1));
			};
			rotationGestureRecognizer.OnRotationMove += delegate(RotationGestureRecognizer rec)
			{
				RaiseOnRotationMove(rec.GetPosition(0), rec.GetPosition(1), rec.RotationDelta);
			};
			rotationGestureRecognizer.OnRotationEnd += delegate(RotationGestureRecognizer rec)
			{
				RaiseOnRotationEnd(rec.GetPosition(0), rec.GetPosition(1), rec.TotalRotation);
			};
			defaultComponents.Rotation = rotationGestureRecognizer;
		}
	}

	private void InitDefaultComponents(Finger finger)
	{
		ITouchFilter touchFilter = new SingleFingerFilter(finger);
		DefaultComponents.FingerComponents fingerComponents = defaultComponents.Fingers[finger.Index];
		if (defaultCompFlags.perFinger.touch)
		{
			finger.OnDown += PerFinger_OnDown;
			finger.OnUp += PerFinger_OnUp;
		}
		if (defaultCompFlags.perFinger.motion)
		{
			FingerMotionDetector fingerMotionDetector = CreateDefaultFingerComponent(finger, defaultPrefabs.fingerMotion);
			fingerMotionDetector.Finger = finger;
			fingerMotionDetector.OnMoveBegin += PerFinger_OnMoveBegin;
			fingerMotionDetector.OnMove += PerFinger_OnMove;
			fingerMotionDetector.OnMoveEnd += PerFinger_OnMoveEnd;
			fingerMotionDetector.OnStationaryBegin += PerFinger_OnStationaryBegin;
			fingerMotionDetector.OnStationary += PerFinger_OnStationary;
			fingerMotionDetector.OnStationaryEnd += PerFinger_OnStationaryEnd;
			fingerComponents.Motion = fingerMotionDetector;
		}
		if (defaultCompFlags.perFinger.longPress)
		{
			LongPressGestureRecognizer longPressGestureRecognizer = CreateDefaultFingerComponent(finger, defaultPrefabs.fingerLongPress);
			longPressGestureRecognizer.TouchFilter = touchFilter;
			longPressGestureRecognizer.OnLongPress += PerFinger_OnLongPress;
			fingerComponents.LongPress = longPressGestureRecognizer;
		}
		if (defaultCompFlags.perFinger.drag)
		{
			DragGestureRecognizer dragGestureRecognizer = CreateDefaultFingerComponent(finger, defaultPrefabs.fingerDrag);
			dragGestureRecognizer.TouchFilter = touchFilter;
			dragGestureRecognizer.OnDragBegin += PerFinger_OnDragBegin;
			dragGestureRecognizer.OnDragMove += PerFinger_OnDragMove;
			dragGestureRecognizer.OnDragEnd += PerFinger_OnDragEnd;
			fingerComponents.Drag = dragGestureRecognizer;
		}
		if (defaultCompFlags.perFinger.swipe)
		{
			SwipeGestureRecognizer swipeGestureRecognizer = CreateDefaultFingerComponent(finger, defaultPrefabs.fingerSwipe);
			swipeGestureRecognizer.TouchFilter = touchFilter;
			swipeGestureRecognizer.OnSwipe += PerFinger_OnSwipe;
			fingerComponents.Swipe = swipeGestureRecognizer;
		}
		if (defaultCompFlags.perFinger.tap)
		{
			TapGestureRecognizer tapGestureRecognizer = CreateDefaultFingerComponent(finger, defaultPrefabs.fingerTap);
			tapGestureRecognizer.TouchFilter = touchFilter;
			tapGestureRecognizer.RequiredTaps = 0;
			tapGestureRecognizer.OnTap += PerFinger_OnTap;
			fingerComponents.Tap = tapGestureRecognizer;
		}
	}

	private static Finger GetFingerFromTouchFilter(GestureRecognizer recognizer)
	{
		if (recognizer.TouchFilter is SingleFingerFilter singleFingerFilter)
		{
			return singleFingerFilter.Finger;
		}
		return null;
	}

	private void PerFinger_OnDown(Finger source)
	{
		RaiseOnFingerDown(source.Index, source.Position);
	}

	private void PerFinger_OnUp(Finger source)
	{
		RaiseOnFingerUp(source.Index, source.Position, Time.time - source.StarTime);
	}

	private void PerFinger_OnStationaryBegin(FingerMotionDetector source)
	{
		RaiseOnFingerStationaryBegin(source.Finger.Index, source.AnchorPos);
	}

	private void PerFinger_OnStationary(FingerMotionDetector source)
	{
		RaiseOnFingerStationary(source.Finger.Index, source.Finger.Position, source.ElapsedStationaryTime);
	}

	private void PerFinger_OnStationaryEnd(FingerMotionDetector source)
	{
		RaiseOnFingerStationaryEnd(source.Finger.Index, source.Finger.PreviousPosition, source.ElapsedStationaryTime);
	}

	private void PerFinger_OnMoveBegin(FingerMotionDetector source)
	{
		RaiseOnFingerMoveBegin(source.Finger.Index, source.AnchorPos);
	}

	private void PerFinger_OnMove(FingerMotionDetector source)
	{
		RaiseOnFingerMove(source.Finger.Index, source.Finger.Position);
	}

	private void PerFinger_OnMoveEnd(FingerMotionDetector source)
	{
		RaiseOnFingerMoveEnd(source.Finger.Index, source.Finger.Position);
	}

	private void PerFinger_OnDragBegin(DragGestureRecognizer source)
	{
		Finger fingerFromTouchFilter = GetFingerFromTouchFilter(source);
		RaiseOnFingerDragBegin(fingerFromTouchFilter.Index, source.Position, source.StartPosition);
	}

	private void PerFinger_OnDragMove(DragGestureRecognizer source)
	{
		Finger fingerFromTouchFilter = GetFingerFromTouchFilter(source);
		RaiseOnFingerDragMove(fingerFromTouchFilter.Index, source.Position, source.MoveDelta);
	}

	private void PerFinger_OnDragEnd(DragGestureRecognizer source)
	{
		Finger fingerFromTouchFilter = GetFingerFromTouchFilter(source);
		RaiseOnFingerDragEnd(fingerFromTouchFilter.Index, source.Position);
	}

	private void PerFinger_OnLongPress(LongPressGestureRecognizer source)
	{
		Finger fingerFromTouchFilter = GetFingerFromTouchFilter(source);
		RaiseOnFingerLongPress(fingerFromTouchFilter.Index, source.Position);
	}

	private void PerFinger_OnSwipe(SwipeGestureRecognizer source)
	{
		Finger fingerFromTouchFilter = GetFingerFromTouchFilter(source);
		RaiseOnFingerSwipe(fingerFromTouchFilter.Index, source.StartPosition, source.Direction, source.Velocity);
	}

	private void PerFinger_OnTap(TapGestureRecognizer source)
	{
		Finger fingerFromTouchFilter = GetFingerFromTouchFilter(source);
		RaiseOnFingerTap(fingerFromTouchFilter.Index, source.Position, source.Taps);
	}

	public static SwipeDirection GetSwipeDirection(Vector3 dir, float tolerance)
	{
		float num = Mathf.Clamp01(1f - tolerance);
		if (Vector2.Dot(dir, Vector2.right) >= num)
		{
			return SwipeDirection.Right;
		}
		if (Vector2.Dot(dir, -Vector2.right) >= num)
		{
			return SwipeDirection.Left;
		}
		if (Vector2.Dot(dir, Vector2.up) >= num)
		{
			return SwipeDirection.Up;
		}
		if (Vector2.Dot(dir, -Vector2.up) >= num)
		{
			return SwipeDirection.Down;
		}
		return SwipeDirection.None;
	}

	public static bool AllFingersMoving(params Finger[] fingers)
	{
		if (fingers.Length == 0)
		{
			return false;
		}
		foreach (Finger finger in fingers)
		{
			if (finger.Phase != FingerPhase.Moved)
			{
				return false;
			}
		}
		return true;
	}

	public static bool FingersMovedInOppositeDirections(Finger finger0, Finger finger1, float minDOT)
	{
		float num = Vector2.Dot(finger0.DeltaPosition.normalized, finger1.DeltaPosition.normalized);
		return num < minDOT;
	}

	public static float SignedAngle(Vector2 from, Vector2 to)
	{
		float y = from.x * to.y - from.y * to.x;
		return Mathf.Atan2(y, Vector2.Dot(from, to));
	}
}
public class FingerGesturesInitializer : MonoBehaviour
{
	public FingerGestures editorGestures;

	public FingerGestures desktopGestures;

	public FingerGestures iosGestures;

	public FingerGestures androidGestures;

	public bool makePersistent = true;

	private void Awake()
	{
		if (!FingerGestures.Instance)
		{
			FingerGestures fingerGestures = ((!Application.isEditor) ? androidGestures : editorGestures);
			FingerGestures fingerGestures2 = UnityEngine.Object.Instantiate(fingerGestures) as FingerGestures;
			fingerGestures2.name = fingerGestures.name;
			if (makePersistent)
			{
				UnityEngine.Object.DontDestroyOnLoad(fingerGestures2.gameObject);
			}
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
public class FingerGesturesPrefabs : MonoBehaviour
{
	public FingerMotionDetector fingerMotion;

	public DragGestureRecognizer fingerDrag;

	public TapGestureRecognizer fingerTap;

	public SwipeGestureRecognizer fingerSwipe;

	public LongPressGestureRecognizer fingerLongPress;

	public DragGestureRecognizer drag;

	public TapGestureRecognizer tap;

	public SwipeGestureRecognizer swipe;

	public LongPressGestureRecognizer longPress;

	public PinchGestureRecognizer pinch;

	public RotationGestureRecognizer rotation;

	public DragGestureRecognizer twoFingerDrag;

	public TapGestureRecognizer twoFingerTap;

	public SwipeGestureRecognizer twoFingerSwipe;

	public LongPressGestureRecognizer twoFingerLongPress;
}
public class MouseGestures : FingerGestures
{
	public int maxMouseButtons = 3;

	public override int MaxFingers => maxMouseButtons;

	protected override void Start()
	{
		base.Start();
	}

	protected override FingerPhase GetPhase(Finger finger)
	{
		int index = finger.Index;
		if (Input.GetMouseButton(index))
		{
			if (Input.GetMouseButtonDown(index))
			{
				return FingerPhase.Began;
			}
			if (((Vector3)(GetPosition(finger) - finger.Position)).sqrMagnitude < 1f)
			{
				return FingerPhase.Stationary;
			}
			return FingerPhase.Moved;
		}
		if (Input.GetMouseButtonUp(index))
		{
			return FingerPhase.Ended;
		}
		return FingerPhase.None;
	}

	protected override Vector2 GetPosition(Finger finger)
	{
		return Input.mousePosition;
	}
}
public class TouchScreenGestures : FingerGestures
{
	public int maxFingers = 5;

	private Touch nullTouch = default(Touch);

	private int[] finger2touchMap;

	public override int MaxFingers => maxFingers;

	protected override void Start()
	{
		finger2touchMap = new int[MaxFingers];
		base.Start();
	}

	protected override FingerPhase GetPhase(Finger finger)
	{
		if (HasValidTouch(finger))
		{
			return GetTouch(finger).phase switch
			{
				TouchPhase.Began => FingerPhase.Began, 
				TouchPhase.Moved => FingerPhase.Moved, 
				TouchPhase.Stationary => FingerPhase.Stationary, 
				_ => FingerPhase.Ended, 
			};
		}
		return FingerPhase.None;
	}

	protected override Vector2 GetPosition(Finger finger)
	{
		return GetTouch(finger).position;
	}

	private void UpdateFingerTouchMap()
	{
		for (int i = 0; i < finger2touchMap.Length; i++)
		{
			finger2touchMap[i] = -1;
		}
		for (int j = 0; j < Input.touchCount; j++)
		{
			int fingerId = Input.touches[j].fingerId;
			if (fingerId < finger2touchMap.Length)
			{
				finger2touchMap[fingerId] = j;
			}
		}
	}

	private bool HasValidTouch(Finger finger)
	{
		return finger2touchMap[finger.Index] != -1;
	}

	private Touch GetTouch(Finger finger)
	{
		int num = finger2touchMap[finger.Index];
		if (num == -1)
		{
			return nullTouch;
		}
		return Input.touches[num];
	}

	protected override void Update()
	{
		UpdateFingerTouchMap();
		base.Update();
	}
}
public class GameCenterAchievement
{
	public string identifier;

	public bool isHidden;

	public bool completed;

	public DateTime lastReportedDate;

	public float percentComplete;

	public GameCenterAchievement(Hashtable ht)
	{
		if (ht.Contains("identifier"))
		{
			identifier = ht["identifier"] as string;
		}
		if (ht.Contains("hidden"))
		{
			isHidden = (bool)ht["hidden"];
		}
		if (ht.Contains("completed"))
		{
			completed = (bool)ht["completed"];
		}
		if (ht.Contains("percentComplete"))
		{
			percentComplete = float.Parse(ht["percentComplete"].ToString());
		}
		if (ht.Contains("lastReportedDate"))
		{
			double value = double.Parse(ht["lastReportedDate"].ToString());
			lastReportedDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(value);
		}
	}

	public static List<GameCenterAchievement> fromJSON(string json)
	{
		List<GameCenterAchievement> list = new List<GameCenterAchievement>();
		ArrayList arrayList = json.arrayListFromJson();
		foreach (Hashtable item in arrayList)
		{
			list.Add(new GameCenterAchievement(item));
		}
		return list;
	}

	public override string ToString()
	{
		return $"<Achievement> identifier: {identifier}, hidden: {isHidden}, completed: {completed}, percentComplete: {percentComplete}, lastReported: {lastReportedDate}";
	}
}
public class GameCenterAchievementMetadata
{
	public string identifier;

	public string description;

	public string unachievedDescription;

	public bool isHidden;

	public int maximumPoints;

	public string title;

	public GameCenterAchievementMetadata(Hashtable ht)
	{
		identifier = ht["identifier"] as string;
		description = ht["achievedDescription"] as string;
		unachievedDescription = ht["unachievedDescription"] as string;
		isHidden = (bool)ht["hidden"];
		maximumPoints = int.Parse(ht["maximumPoints"].ToString());
		title = ht["title"] as string;
	}

	public static List<GameCenterAchievementMetadata> fromJSON(string json)
	{
		List<GameCenterAchievementMetadata> list = new List<GameCenterAchievementMetadata>();
		ArrayList arrayList = json.arrayListFromJson();
		foreach (Hashtable item in arrayList)
		{
			list.Add(new GameCenterAchievementMetadata(item));
		}
		return list;
	}

	public override string ToString()
	{
		return $"<AchievementMetaData> identifier: {identifier}, hidden: {isHidden}, maxPoints: {maximumPoints}, title: {title} desc: {description}, unachDesc: {unachievedDescription}";
	}
}
public enum GameCenterLeaderboardTimeScope
{
	Today,
	Week,
	AllTime
}
public class GameCenterBinding
{
	[DllImport("__Internal")]
	private static extern bool _gameCenterIsGameCenterAvailable();

	public static bool isGameCenterAvailable()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return _gameCenterIsGameCenterAvailable();
		}
		return false;
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterAuthenticateLocalPlayer();

	public static void authenticateLocalPlayer()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterAuthenticateLocalPlayer();
		}
	}

	[DllImport("__Internal")]
	private static extern bool _gameCenterIsPlayerAuthenticated();

	public static bool isPlayerAuthenticated()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return _gameCenterIsPlayerAuthenticated();
		}
		return false;
	}

	[DllImport("__Internal")]
	private static extern string _gameCenterPlayerAlias();

	public static string playerAlias()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return _gameCenterPlayerAlias();
		}
		return string.Empty;
	}

	[DllImport("__Internal")]
	private static extern string _gameCenterPlayerIdentifier();

	public static string playerIdentifier()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return _gameCenterPlayerIdentifier();
		}
		return string.Empty;
	}

	[DllImport("__Internal")]
	private static extern bool _gameCenterIsUnderage();

	public static bool isUnderage()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			return _gameCenterIsUnderage();
		}
		return false;
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterRetrieveFriends();

	public static void retrieveFriends()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterRetrieveFriends();
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterLoadPlayerData(string playerIds);

	public static void loadPlayerData(string[] playerIdArray)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterLoadPlayerData(string.Join(",", playerIdArray));
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterLoadLeaderboardLeaderboardTitles();

	public static void loadLeaderboardTitles()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterLoadLeaderboardLeaderboardTitles();
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterReportScore(long score, string leaderboardId);

	public static void reportScore(long score, string leaderboardId)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterReportScore(score, leaderboardId);
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterShowLeaderboardWithTimeScope(int timeScope);

	public static void showLeaderboardWithTimeScope(GameCenterLeaderboardTimeScope timeScope)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterShowLeaderboardWithTimeScope((int)timeScope);
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterShowLeaderboardWithTimeScopeAndLeaderboardId(int timeScope, string leaderboardId);

	public static void showLeaderboardWithTimeScopeAndLeaderboard(GameCenterLeaderboardTimeScope timeScope, string leaderboardId)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterShowLeaderboardWithTimeScopeAndLeaderboardId((int)timeScope, leaderboardId);
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterRetrieveScores(bool friendsOnly, int timeScope, int start, int end);

	public static void retrieveScores(bool friendsOnly, GameCenterLeaderboardTimeScope timeScope, int start, int end)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterRetrieveScores(friendsOnly, (int)timeScope, start, end);
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterRetrieveScoresForLeaderboard(bool friendsOnly, int timeScope, int start, int end, string leaderboardId);

	public static void retrieveScores(bool friendsOnly, GameCenterLeaderboardTimeScope timeScope, int start, int end, string leaderboardId)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterRetrieveScoresForLeaderboard(friendsOnly, (int)timeScope, start, end, leaderboardId);
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterRetrieveScoresForPlayerId(string playerId);

	public static void retrieveScoresForPlayerId(string playerId)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterRetrieveScoresForPlayerId(playerId);
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterRetrieveScoresForPlayerIdAndLeaderboard(string playerId, string leaderboardId);

	public static void retrieveScoresForPlayerId(string playerId, string leaderboardId)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterRetrieveScoresForPlayerIdAndLeaderboard(playerId, leaderboardId);
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterReportAchievement(string identifier, float percent);

	public static void reportAchievement(string identifier, float percent)
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterReportAchievement(identifier, percent);
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterGetAchievements();

	public static void getAchievements()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterGetAchievements();
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterResetAchievements();

	public static void resetAchievements()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterResetAchievements();
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterShowAchievements();

	public static void showAchievements()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterShowAchievements();
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterRetrieveAchievementMetadata();

	public static void retrieveAchievementMetadata()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterRetrieveAchievementMetadata();
		}
	}

	[DllImport("__Internal")]
	private static extern void _gameCenterShowCompletionBannerForAchievements();

	public static void showCompletionBannerForAchievements()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_gameCenterShowCompletionBannerForAchievements();
		}
	}
}
public class GameCenterEventListener : MonoBehaviour
{
	private void Start()
	{
		GameCenterManager.loadPlayerDataFailed += loadPlayerDataFailed;
		GameCenterManager.playerDataLoaded += playerDataLoaded;
		GameCenterManager.playerAuthenticated += playerAuthenticated;
		GameCenterManager.playerFailedToAuthenticate += playerFailedToAuthenticate;
		GameCenterManager.playerLoggedOut += playerLoggedOut;
		GameCenterManager.loadCategoryTitlesFailed += loadCategoryTitlesFailed;
		GameCenterManager.categoriesLoaded += categoriesLoaded;
		GameCenterManager.reportScoreFailed += reportScoreFailed;
		GameCenterManager.reportScoreFinished += reportScoreFinished;
		GameCenterManager.retrieveScoresFailed += retrieveScoresFailed;
		GameCenterManager.scoresLoaded += scoresLoaded;
		GameCenterManager.retrieveScoresForPlayerIdFailed += retrieveScoresForPlayerIdFailed;
		GameCenterManager.scoresForPlayerIdLoaded += scoresForPlayerIdLoaded;
		GameCenterManager.reportAchievementFailed += reportAchievementFailed;
		GameCenterManager.reportAchievementFinished += reportAchievementFinished;
		GameCenterManager.loadAchievementsFailed += loadAchievementsFailed;
		GameCenterManager.achievementsLoaded += achievementsLoaded;
		GameCenterManager.resetAchievementsFailed += resetAchievementsFailed;
		GameCenterManager.resetAchievementsFinished += resetAchievementsFinished;
		GameCenterManager.retrieveAchievementMetadataFailed += retrieveAchievementMetadataFailed;
		GameCenterManager.achievementMetadataLoaded += achievementMetadataLoaded;
	}

	private void OnDisable()
	{
		GameCenterManager.loadPlayerDataFailed -= loadPlayerDataFailed;
		GameCenterManager.playerDataLoaded -= playerDataLoaded;
		GameCenterManager.playerAuthenticated -= playerAuthenticated;
		GameCenterManager.playerLoggedOut -= playerLoggedOut;
		GameCenterManager.loadCategoryTitlesFailed -= loadCategoryTitlesFailed;
		GameCenterManager.categoriesLoaded -= categoriesLoaded;
		GameCenterManager.reportScoreFailed -= reportScoreFailed;
		GameCenterManager.reportScoreFinished -= reportScoreFinished;
		GameCenterManager.retrieveScoresFailed -= retrieveScoresFailed;
		GameCenterManager.scoresLoaded -= scoresLoaded;
		GameCenterManager.retrieveScoresForPlayerIdFailed -= retrieveScoresForPlayerIdFailed;
		GameCenterManager.scoresForPlayerIdLoaded -= scoresForPlayerIdLoaded;
		GameCenterManager.reportAchievementFailed -= reportAchievementFailed;
		GameCenterManager.reportAchievementFinished -= reportAchievementFinished;
		GameCenterManager.loadAchievementsFailed -= loadAchievementsFailed;
		GameCenterManager.achievementsLoaded -= achievementsLoaded;
		GameCenterManager.resetAchievementsFailed -= resetAchievementsFailed;
		GameCenterManager.resetAchievementsFinished -= resetAchievementsFinished;
		GameCenterManager.retrieveAchievementMetadataFailed -= retrieveAchievementMetadataFailed;
		GameCenterManager.achievementMetadataLoaded -= achievementMetadataLoaded;
	}

	private void playerAuthenticated()
	{
		Debug.Log("playerAuthenticated");
	}

	private void playerFailedToAuthenticate(string error)
	{
		Debug.Log("playerFailedToAuthenticate: " + error);
	}

	private void playerLoggedOut()
	{
		Debug.Log("playerLoggedOut");
	}

	private void playerDataLoaded(List<GameCenterPlayer> players)
	{
		Debug.Log("playerDataLoaded");
		foreach (GameCenterPlayer player in players)
		{
			Debug.Log(player);
		}
	}

	private void loadPlayerDataFailed(string error)
	{
		Debug.Log("loadPlayerDataFailed: " + error);
	}

	private void categoriesLoaded(List<GameCenterLeaderboard> leaderboards)
	{
		Debug.Log("categoriesLoaded");
		foreach (GameCenterLeaderboard leaderboard in leaderboards)
		{
			Debug.Log(leaderboard);
		}
	}

	private void loadCategoryTitlesFailed(string error)
	{
		Debug.Log("loadCategoryTitlesFailed: " + error);
	}

	private void scoresLoaded(List<GameCenterScore> scores)
	{
		Debug.Log("scoresLoaded");
		foreach (GameCenterScore score in scores)
		{
			Debug.Log(score);
		}
	}

	private void retrieveScoresFailed(string error)
	{
		Debug.Log("retrieveScoresFailed: " + error);
	}

	private void retrieveScoresForPlayerIdFailed(string error)
	{
		Debug.Log("retrieveScoresForPlayerIdFailed: " + error);
	}

	private void scoresForPlayerIdLoaded(List<GameCenterScore> scores)
	{
		Debug.Log("scoresForPlayerIdLoaded");
		foreach (GameCenterScore score in scores)
		{
			Debug.Log(score);
		}
	}

	private void reportScoreFinished(string category)
	{
		Debug.Log("reportScoreFinished for category: " + category);
	}

	private void reportScoreFailed(string error)
	{
		Debug.Log("reportScoreFailed: " + error);
	}

	private void achievementMetadataLoaded(List<GameCenterAchievementMetadata> achievementMetadata)
	{
		Debug.Log("achievementMetadatLoaded");
		foreach (GameCenterAchievementMetadata achievementMetadatum in achievementMetadata)
		{
			Debug.Log(achievementMetadatum);
		}
	}

	private void retrieveAchievementMetadataFailed(string error)
	{
		Debug.Log("retrieveAchievementMetadataFailed: " + error);
	}

	private void resetAchievementsFinished()
	{
		Debug.Log("resetAchievmenetsFinished");
	}

	private void resetAchievementsFailed(string error)
	{
		Debug.Log("resetAchievementsFailed: " + error);
	}

	private void achievementsLoaded(List<GameCenterAchievement> achievements)
	{
		Debug.Log("achievementsLoaded");
		foreach (GameCenterAchievement achievement in achievements)
		{
			Debug.Log(achievement);
		}
	}

	private void loadAchievementsFailed(string error)
	{
		Debug.Log("loadAchievementsFailed: " + error);
	}

	private void reportAchievementFinished(string identifier)
	{
		Debug.Log("reportAchievementFinished: " + identifier);
	}

	private void reportAchievementFailed(string error)
	{
		Debug.Log("reportAchievementFailed: " + error);
	}
}
public class GameCenterLeaderboard
{
	public string leaderboardId;

	public string title;

	public GameCenterLeaderboard(string leaderboardId, string title)
	{
		this.leaderboardId = leaderboardId;
		this.title = title;
	}

	public static List<GameCenterLeaderboard> fromJSON(string json)
	{
		List<GameCenterLeaderboard> list = new List<GameCenterLeaderboard>();
		Hashtable hashtable = json.hashtableFromJson();
		foreach (DictionaryEntry item in hashtable)
		{
			list.Add(new GameCenterLeaderboard(item.Value as string, item.Key as string));
		}
		return list;
	}

	public override string ToString()
	{
		return $"<Leaderboard> leaderboardId: {leaderboardId}, title: {title}";
	}
}
public class GameCenterManager : MonoBehaviour
{
	public static event Action<string> loadPlayerDataFailed;

	public static event Action<List<GameCenterPlayer>> playerDataLoaded;

	public static event Action playerAuthenticated;

	public static event Action<string> playerFailedToAuthenticate;

	public static event Action playerLoggedOut;

	public static event Action<string> loadCategoryTitlesFailed;

	public static event Action<List<GameCenterLeaderboard>> categoriesLoaded;

	public static event Action<string> reportScoreFailed;

	public static event Action<string> reportScoreFinished;

	public static event Action<string> retrieveScoresFailed;

	public static event Action<List<GameCenterScore>> scoresLoaded;

	public static event Action<string> retrieveScoresForPlayerIdFailed;

	public static event Action<List<GameCenterScore>> scoresForPlayerIdLoaded;

	public static event Action<string> reportAchievementFailed;

	public static event Action<string> reportAchievementFinished;

	public static event Action<string> loadAchievementsFailed;

	public static event Action<List<GameCenterAchievement>> achievementsLoaded;

	public static event Action<string> resetAchievementsFailed;

	public static event Action resetAchievementsFinished;

	public static event Action<string> retrieveAchievementMetadataFailed;

	public static event Action<List<GameCenterAchievementMetadata>> achievementMetadataLoaded;

	private void Awake()
	{
		base.gameObject.name = GetType().ToString();
	}

	public void loadPlayerDataDidFail(string error)
	{
		if (GameCenterManager.loadPlayerDataFailed != null)
		{
			GameCenterManager.loadPlayerDataFailed(error);
		}
	}

	public void loadPlayerDataDidLoad(string jsonFriendList)
	{
		List<GameCenterPlayer> obj = GameCenterPlayer.fromJSON(jsonFriendList);
		if (GameCenterManager.playerDataLoaded != null)
		{
			GameCenterManager.playerDataLoaded(obj);
		}
	}

	public void playerDidLogOut()
	{
		if (GameCenterManager.playerLoggedOut != null)
		{
			GameCenterManager.playerLoggedOut();
		}
	}

	public void playerDidAuthenticate()
	{
		if (GameCenterManager.playerAuthenticated != null)
		{
			GameCenterManager.playerAuthenticated();
		}
	}

	public void playerAuthenticationFailed(string error)
	{
		if (GameCenterManager.playerFailedToAuthenticate != null)
		{
			GameCenterManager.playerFailedToAuthenticate(error);
		}
	}

	public void loadCategoryTitlesDidFail(string error)
	{
		if (GameCenterManager.loadCategoryTitlesFailed != null)
		{
			GameCenterManager.loadCategoryTitlesFailed(error);
		}
	}

	public void categoriesDidLoad(string jsonCategoryList)
	{
		List<GameCenterLeaderboard> obj = GameCenterLeaderboard.fromJSON(jsonCategoryList);
		if (GameCenterManager.categoriesLoaded != null)
		{
			GameCenterManager.categoriesLoaded(obj);
		}
	}

	public void reportScoreDidFail(string error)
	{
		if (GameCenterManager.reportScoreFailed != null)
		{
			GameCenterManager.reportScoreFailed(error);
		}
	}

	public void reportScoreDidFinish(string category)
	{
		if (GameCenterManager.reportScoreFinished != null)
		{
			GameCenterManager.reportScoreFinished(category);
		}
	}

	public void retrieveScoresDidFail(string category)
	{
		if (GameCenterManager.retrieveScoresFailed != null)
		{
			GameCenterManager.retrieveScoresFailed(category);
		}
	}

	public void retrieveScoresDidLoad(string jsonScoresList)
	{
		List<GameCenterScore> obj = GameCenterScore.fromJSON(jsonScoresList);
		if (GameCenterManager.scoresLoaded != null)
		{
			GameCenterManager.scoresLoaded(obj);
		}
	}

	public void retrieveScoresForPlayerIdDidFail(string error)
	{
		if (GameCenterManager.retrieveScoresForPlayerIdFailed != null)
		{
			GameCenterManager.retrieveScoresForPlayerIdFailed(error);
		}
	}

	public void retrieveScoresForPlayerIdDidLoad(string jsonScoresList)
	{
		List<GameCenterScore> obj = GameCenterScore.fromJSON(jsonScoresList);
		if (GameCenterManager.scoresForPlayerIdLoaded != null)
		{
			GameCenterManager.scoresForPlayerIdLoaded(obj);
		}
	}

	public void reportAchievementDidFail(string error)
	{
		if (GameCenterManager.reportAchievementFailed != null)
		{
			GameCenterManager.reportAchievementFailed(error);
		}
	}

	public void reportAchievementDidFinish(string identifier)
	{
		if (GameCenterManager.reportAchievementFinished != null)
		{
			GameCenterManager.reportAchievementFinished(identifier);
		}
	}

	public void loadAchievementsDidFail(string error)
	{
		if (GameCenterManager.loadAchievementsFailed != null)
		{
			GameCenterManager.loadAchievementsFailed(error);
		}
	}

	public void achievementsDidLoad(string jsonAchievmentList)
	{
		List<GameCenterAchievement> obj = GameCenterAchievement.fromJSON(jsonAchievmentList);
		if (GameCenterManager.achievementsLoaded != null)
		{
			GameCenterManager.achievementsLoaded(obj);
		}
	}

	public void resetAchievementsDidFail(string error)
	{
		if (GameCenterManager.resetAchievementsFailed != null)
		{
			GameCenterManager.resetAchievementsFailed(error);
		}
	}

	public void resetAchievementsDidFinish(string emptyString)
	{
		if (GameCenterManager.resetAchievementsFinished != null)
		{
			GameCenterManager.resetAchievementsFinished();
		}
	}

	public void retrieveAchievementsMetadataDidFail(string error)
	{
		if (GameCenterManager.retrieveAchievementMetadataFailed != null)
		{
			GameCenterManager.retrieveAchievementMetadataFailed(error);
		}
	}

	public void achievementMetadataDidLoad(string jsonAchievementDescriptionList)
	{
		List<GameCenterAchievementMetadata> obj = GameCenterAchievementMetadata.fromJSON(jsonAchievementDescriptionList);
		if (GameCenterManager.achievementMetadataLoaded != null)
		{
			GameCenterManager.achievementMetadataLoaded(obj);
		}
	}
}
public class GameCenterPlayer
{
	public string playerId;

	public string alias;

	public bool isFriend;

	public GameCenterPlayer(Hashtable ht)
	{
		if (ht.Contains("playerId"))
		{
			playerId = ht["playerId"] as string;
		}
		if (ht.Contains("alias"))
		{
			alias = ht["alias"] as string;
		}
		if (ht.Contains("isFriend"))
		{
			isFriend = (bool)ht["isFriend"];
		}
	}

	public static List<GameCenterPlayer> fromJSON(string json)
	{
		List<GameCenterPlayer> list = new List<GameCenterPlayer>();
		ArrayList arrayList = json.arrayListFromJson();
		foreach (Hashtable item in arrayList)
		{
			list.Add(new GameCenterPlayer(item));
		}
		return list;
	}

	public override string ToString()
	{
		return $"<Player> playerId: {playerId}, alias: {alias}, isFriend: {isFriend}";
	}
}
public class GameCenterScore
{
	public string category;

	public string formattedValue;

	public int value;

	public DateTime date;

	public string playerId;

	public int rank;

	public bool isFriend;

	public string alias;

	public GameCenterScore(Hashtable ht)
	{
		if (ht.Contains("category"))
		{
			category = ht["category"] as string;
		}
		if (ht.Contains("formattedValue"))
		{
			formattedValue = ht["formattedValue"] as string;
		}
		if (ht.Contains("value"))
		{
			value = int.Parse(ht["value"].ToString());
		}
		if (ht.Contains("playerId"))
		{
			playerId = ht["playerId"] as string;
		}
		if (ht.Contains("rank"))
		{
			rank = int.Parse(ht["rank"].ToString());
		}
		if (ht.Contains("isFriend"))
		{
			isFriend = (bool)ht["isFriend"];
		}
		if (ht.Contains("alias"))
		{
			alias = ht["alias"] as string;
		}
		if (ht.Contains("date"))
		{
			double num = double.Parse(ht["date"].ToString());
			date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(num);
		}
	}

	public static List<GameCenterScore> fromJSON(string json)
	{
		List<GameCenterScore> list = new List<GameCenterScore>();
		ArrayList arrayList = json.arrayListFromJson();
		foreach (Hashtable item in arrayList)
		{
			list.Add(new GameCenterScore(item));
		}
		return list;
	}

	public override string ToString()
	{
		return $"<Score> category: {category}, formattedValue: {formattedValue}, date: {date}, rank: {rank}, alias: {alias}";
	}
}
public class GameCenterGUIManager : MonoBehaviour
{
	private List<GameCenterLeaderboard> leaderboards;

	private List<GameCenterAchievementMetadata> achievementMetadata;

	private void Start()
	{
		GameCenterManager.categoriesLoaded += delegate(List<GameCenterLeaderboard> leaderboards)
		{
			this.leaderboards = leaderboards;
		};
		GameCenterManager.achievementMetadataLoaded += delegate(List<GameCenterAchievementMetadata> achievementMetadata)
		{
			this.achievementMetadata = achievementMetadata;
		};
	}

	private void OnGUI()
	{
		float num = 5f;
		float left = 5f;
		float num2 = ((Screen.width < 960 && Screen.height < 960) ? 160 : 320);
		float num3 = ((Screen.width < 960 && Screen.height < 960) ? 40 : 80);
		float num4 = num3 + 10f;
		if (GUI.Button(new Rect(left, num, num2, num3), "Authenticate"))
		{
			GameCenterBinding.authenticateLocalPlayer();
		}
		if (GUI.Button(new Rect(left, num += num4, num2, num3), "Load Achievement Metadata"))
		{
			GameCenterBinding.retrieveAchievementMetadata();
		}
		if (GUI.Button(new Rect(left, num += num4, num2, num3), "Get Raw Achievements"))
		{
			GameCenterBinding.getAchievements();
		}
		if (GUI.Button(new Rect(left, num += num4, num2, num3), "Post Achievement") && achievementMetadata != null && achievementMetadata.Count > 0)
		{
			int num5 = UnityEngine.Random.Range(2, 60);
			Debug.Log("sending percentComplete: " + num5);
			GameCenterBinding.reportAchievement(achievementMetadata[0].identifier, num5);
		}
		if (GUI.Button(new Rect(left, num += num4, num2, num3), "Show Achievements"))
		{
			GameCenterBinding.showAchievements();
		}
		if (GUI.Button(new Rect(left, num += num4, num2, num3), "Reset Achievements"))
		{
			GameCenterBinding.resetAchievements();
		}
		if (GUI.Button(new Rect(left, num += num4, num2, num3), "Multiplayer Scene"))
		{
			Application.LoadLevel("GameCenterMultiplayerTestScene");
		}
		left = (float)Screen.width - num2 - 5f;
		num = 5f;
		if (GUI.Button(new Rect(left, num, num2, num3), "Get Player Alias"))
		{
			string text = GameCenterBinding.playerAlias();
			Debug.Log("Player alias: " + text);
		}
		if (GUI.Button(new Rect(left, num += num4, num2, num3), "Load Leaderboard Data"))
		{
			GameCenterBinding.loadLeaderboardTitles();
		}
		if (GUI.Button(new Rect(left, num += num4, num2, num3), "Post Score") && leaderboards != null && leaderboards.Count > 0)
		{
			Debug.Log("about to report a random score for leaderboard: " + leaderboards[0].leaderboardId);
			GameCenterBinding.reportScore(UnityEngine.Random.Range(1, 99999), leaderboards[0].leaderboardId);
		}
		if (GUI.Button(new Rect(left, num += num4, num2, num3), "Show Leaderboards"))
		{
			GameCenterBinding.showLeaderboardWithTimeScope(GameCenterLeaderboardTimeScope.AllTime);
		}
		if (GUI.Button(new Rect(left, num += num4, num2, num3), "Get Raw Score Data"))
		{
			if (leaderboards != null && leaderboards.Count > 0)
			{
				GameCenterBinding.retrieveScores(friendsOnly: false, GameCenterLeaderboardTimeScope.AllTime, 1, 25, leaderboards[0].leaderboardId);
			}
			else
			{
				Debug.Log("Load leaderboard data before attempting to retrieve scores");
			}
		}
		if (GUI.Button(new Rect(left, num += num4, num2, num3), "Get Scores for Me"))
		{
			if (leaderboards != null && leaderboards.Count > 0)
			{
				GameCenterBinding.retrieveScoresForPlayerId(GameCenterBinding.playerIdentifier(), leaderboards[0].leaderboardId);
			}
			else
			{
				Debug.Log("Load leaderboard data before attempting to retrieve scores");
			}
		}
		if (GUI.Button(new Rect(left, num += num4, num2, num3), "Retrieve Friends"))
		{
			GameCenterBinding.retrieveFriends();
		}
	}
}
public class Kamcord : MonoBehaviour
{
	public enum VideoQuality
	{
		Standard,
		Trailer
	}

	public enum MetadataType
	{
		level = 0,
		score = 1,
		list = 2,
		other = 1000
	}

	public enum ShareTarget
	{
		Facebook = 0,
		Twitter = 1,
		YouTube = 2,
		Email = 3,
		LINE = 5
	}

	public enum YouTubeVideoCategory
	{
		Comedy,
		Education,
		Entertainment,
		Games,
		Music
	}

	[Serializable]
	public class KamcordBlacklist
	{
		public bool ipod4Gen;

		public bool ipod5Gen;

		public bool iphone3GS;

		public bool iphone4;

		public bool iphone4S;

		public bool iphone5;

		public bool iphone5c;

		public bool iphone5S;

		public bool ipad1;

		public bool ipad2;

		public bool ipadMini;

		public bool ipad3;

		public bool ipad4;

		public bool ipadAir;
	}

	public class Implementation
	{
		public virtual void SetLoggingEnabled(bool value)
		{
		}

		public virtual bool IsEnabled()
		{
			return false;
		}

		public virtual string GetDisabledReason()
		{
			return string.Empty;
		}

		public virtual void WhitelistBoard(string boardName)
		{
		}

		public virtual void BlacklistBoard(string boardName)
		{
		}

		public virtual void WhitelistDevice(string deviceName)
		{
		}

		public virtual void BlacklistDevice(string deviceName)
		{
		}

		public virtual void WhitelistBoard(string boardName, int sdkVersion)
		{
		}

		public virtual void BlacklistBoard(string boardName, int sdkVersion)
		{
		}

		public virtual void WhitelistDevice(string deviceName, int sdkVersion)
		{
		}

		public virtual void BlacklistDevice(string deviceName, int sdkVersion)
		{
		}

		public virtual void WhitelistAllBoards()
		{
		}

		public virtual void BlacklistAllBoards()
		{
		}

		public virtual void WhitelistAll()
		{
		}

		public virtual void BlacklistAll()
		{
		}

		public virtual string GetBoard()
		{
			return string.Empty;
		}

		public virtual string GetDevice()
		{
			return string.Empty;
		}

		public virtual bool IsWhitelisted(string boardName)
		{
			return false;
		}

		public virtual bool IsWhitelisted()
		{
			return false;
		}

		public virtual void DoneChangingWhitelist()
		{
		}

		public virtual void SetVideoTitle(string title)
		{
		}

		public virtual void SetYouTubeSettings(string description, string tags)
		{
		}

		public virtual void SetFacebookAppID(string facebookAppID)
		{
		}

		public virtual void SetFacebookAppIDAndShareAuth(string facebookAppID, bool useSharedAuth)
		{
		}

		public virtual void LogoutOfSharedFacebookAuth()
		{
		}

		public virtual void SetWeChatAppID(string weChatAppID)
		{
		}

		public virtual void SetFacebookDescription(string facebookDescription)
		{
		}

		public virtual void SetDefaultTweet(string tweet)
		{
		}

		public virtual void SetTwitterDescription(string twitterDescription)
		{
		}

		public virtual void SetDefaultEmailSubject(string subject)
		{
		}

		public virtual void SetDefaultEmailBody(string body)
		{
		}

		public virtual void SetShareTargets(ShareTarget target1, ShareTarget target2, ShareTarget target3, ShareTarget target4)
		{
		}

		public virtual void SetVideoMetadata(Dictionary<string, object> metadata)
		{
		}

		public virtual void SetMaxFreeDiskSpacePercentageUsage(double percentage)
		{
		}

		public virtual string Version()
		{
			return string.Empty;
		}

		public virtual void SetLevelAndScore(string level, double score)
		{
		}

		public virtual void SetDeveloperMetadata(MetadataType metadataType, string displayKey, string displayValue)
		{
		}

		public virtual void SetDeveloperMetadataWithNumericValue(MetadataType metadataType, string displayKey, string displayValue, double numericValue)
		{
		}

		public virtual bool VideoExistsWithMetadataConstraints(Dictionary<string, object> metadata)
		{
			return false;
		}

		public virtual void ShowVideoWithMetadataConstraints(Dictionary<string, object> metadata, string title)
		{
		}

		public virtual void ShowVideoWithVideoID(string videoID, string title)
		{
		}

		public virtual void BeginDraw()
		{
		}

		public virtual void EndDraw()
		{
		}

		public virtual void StartRecording()
		{
		}

		public virtual void StopRecording()
		{
		}

		public virtual void Pause()
		{
		}

		public virtual void Resume()
		{
		}

		public virtual bool IsRecording()
		{
			return false;
		}

		public virtual bool IsPaused()
		{
			return false;
		}

		public virtual bool IsViewShowing()
		{
			return false;
		}

		public virtual void Snapshot(string filename)
		{
		}

		public virtual void SetVideoQuality(VideoQuality quality)
		{
		}

		public virtual void SetUseFastRender(bool useFastRender)
		{
		}

		public virtual void SetVoiceOverlayEnabled(bool enabled)
		{
		}

		public virtual bool VoiceOverlayEnabled()
		{
			return false;
		}

		public virtual void ActivateVoiceOverlay(bool activate)
		{
		}

		public virtual bool VoiceOverlayActivated()
		{
			return false;
		}

		public virtual void CaptureFrame()
		{
		}

		public virtual void SetNotificationsEnabled(bool notificationsEnabled)
		{
		}

		public virtual void FireTestNotification()
		{
		}

		public virtual void ShowView()
		{
		}

		public virtual void ShowWatchView()
		{
		}

		public virtual void SetMaximumVideoLength(uint seconds)
		{
		}

		public virtual uint MaximumVideoLength()
		{
			return 0u;
		}

		public virtual void SetVideoFPS(uint videoFPS)
		{
		}

		public virtual uint VideoFPS()
		{
			return 0u;
		}

		public virtual void SetShouldPauseGameEngine(bool shouldPause)
		{
		}

		public virtual bool ShouldPauseGameEngine()
		{
			return false;
		}

		public virtual void UploadVideo(string title)
		{
		}

		public virtual void Disable()
		{
		}

		public virtual void TurnOffAutomaticAudioRecording(bool state)
		{
		}

		public virtual void PlayBackgroundSound(string fileName, bool loop)
		{
		}

		public virtual void Init(string devKey, string devSecret, string appName, VideoQuality videoQuality)
		{
		}

		public virtual void SetDeviceBlacklist(bool disableiPod4G, bool disableiPod5G, bool disableiPhone3GS, bool disableiPhone4, bool disableiPhone4S, bool disableiPhone5, bool disableiPhone5C, bool disableiPhone5S, bool disableiPad1, bool disableiPad2, bool disableiPadMini, bool disableiPad3, bool disableiPad4, bool disableiPadAir)
		{
		}

		public virtual void SetDefaultTitle(string title)
		{
		}

		public virtual void SetYouTubeVideoCategory(YouTubeVideoCategory category)
		{
		}

		public virtual void SetFacebookSettings(string title, string caption, string description)
		{
		}

		public virtual void SetDefaultEmailSubjectAndBody(string subject, string body)
		{
		}

		public virtual void Awake(Kamcord kamcordInstance)
		{
		}

		public virtual void Start(Kamcord kamcordInstance)
		{
		}

		public virtual void SetCrossPromoIcon(string localFileImageURL)
		{
		}

		public virtual void SetMode(int mode)
		{
		}

		public virtual void SetAgeRestrictionEnabled(bool restricted)
		{
		}

		public virtual bool IsAgeRestrictionEnabled()
		{
			return false;
		}

		public virtual void SetVideoIncompleteWarningEnabled(bool enabled)
		{
		}

		public virtual bool IsVideoComplete()
		{
			return true;
		}

		public virtual void SetAudioSettings(int sampleRate, int numChannels)
		{
		}

		public virtual void WriteAudioData(float[] data, int length)
		{
		}

		public virtual void SetFlushOnCopy(bool flush)
		{
		}
	}

	public delegate void KamcordDidStartRecording();

	public delegate void KamcordDidStopRecording();

	public delegate void KamcordViewDidAppear();

	public delegate void KamcordViewWillDisappear();

	public delegate void KamcordViewDidDisappear();

	public delegate void KamcordViewDidNotAppear();

	public delegate void KamcordWatchViewDidAppear();

	public delegate void KamcordWatchViewWillDisappear();

	public delegate void KamcordWatchViewDidDisappear();

	public delegate void VideoThumbnailReadyAtFilePath(string filepath);

	public delegate void ShareButtonPressed();

	public delegate void VideoWillBeginUploading(string videoID, string URL);

	public delegate void VideoUploadProgressed(string videoID, float progress);

	public delegate void VideoFinishedUploading(string videoID, bool success);

	public delegate void VideoSharedTo(string kamcordVideoID, string networkName, bool success);

	public delegate void VideoSharedToFacebook(string kamcordVideoID, bool success);

	public delegate void VideoSharedToTwitter(string kamcordVideoID, bool success);

	public delegate void VideoSharedToYoutube(string kamcordVideoID, bool success);

	public delegate void SnapshotReadyAtFilePath(string filepath);

	public delegate void PushNotifCallToActionButtonPressed();

	public delegate void AttributedKamcordInstall();

	public delegate void AdjustAndroidWhitelist();

	public delegate void IsEnabledChanged(bool isEnabled);

	public bool enableIOS = true;

	public bool enableAndroid;

	public bool enableLogging = true;

	public string developerKey = "Kamcord developer key";

	public string developerSecret = "Kamcord developer secret";

	public string appName = "Application name";

	public VideoQuality videoQuality;

	public bool enableVoiceOverlay = true;

	public bool useFastRender = true;

	public KamcordBlacklist deviceBlacklist;

	public static bool iOSEnabled_ = true;

	public static bool loggingEnabled_ = true;

	public static bool androidEnabled_ = false;

	private static Implementation implementation_;

	private static bool developerSetVoiceOverlay = false;

	public static Kamcord instance;

	protected static List<KamcordCallbackInterface> listeners = new List<KamcordCallbackInterface>();

	private static float timeScale;

	public static event KamcordDidStartRecording kamcordDidStartRecording;

	public static event KamcordDidStopRecording kamcordDidStopRecording;

	public static event KamcordViewDidAppear kamcordViewDidAppear;

	public static event KamcordViewWillDisappear kamcordViewWillDisappear;

	public static event KamcordViewDidDisappear kamcordViewDidDisappear;

	public static event KamcordViewDidNotAppear kamcordViewDidNotAppear;

	public static event KamcordWatchViewDidAppear kamcordWatchViewDidAppear;

	public static event KamcordWatchViewWillDisappear kamcordWatchViewWillDisappear;

	public static event KamcordWatchViewDidDisappear kamcordWatchViewDidDisappear;

	public static event VideoThumbnailReadyAtFilePath videoThumbnailReadyAtFilePath;

	public static event ShareButtonPressed shareButtonPressed;

	public static event VideoWillBeginUploading videoWillBeginUploading;

	public static event VideoUploadProgressed videoUploadProgressed;

	public static event VideoFinishedUploading videoFinishedUploading;

	public static event VideoSharedTo videoSharedTo;

	public static event VideoSharedToFacebook videoSharedToFacebook;

	public static event VideoSharedToTwitter videoSharedToTwitter;

	public static event VideoSharedToYoutube videoSharedToYoutube;

	public static event SnapshotReadyAtFilePath snapshotReadyAtFilePath;

	public static event PushNotifCallToActionButtonPressed pushNotifCallToActionButtonPressed;

	public static event AttributedKamcordInstall attributedKamcordInstall;

	public static event AdjustAndroidWhitelist adjustAndroidWhitelist;

	public static event IsEnabledChanged isEnabledChanged;

	public static void UnsubscribeFromAllCallbacks()
	{
		Kamcord.kamcordViewDidAppear = null;
		Kamcord.kamcordViewWillDisappear = null;
		Kamcord.kamcordViewDidDisappear = null;
		Kamcord.kamcordViewDidNotAppear = null;
		Kamcord.kamcordWatchViewDidAppear = null;
		Kamcord.kamcordWatchViewWillDisappear = null;
		Kamcord.kamcordWatchViewDidDisappear = null;
		Kamcord.videoThumbnailReadyAtFilePath = null;
		Kamcord.videoWillBeginUploading = null;
		Kamcord.videoUploadProgressed = null;
		Kamcord.videoFinishedUploading = null;
		Kamcord.videoSharedTo = null;
		Kamcord.videoSharedToFacebook = null;
		Kamcord.videoSharedToTwitter = null;
		Kamcord.videoSharedToYoutube = null;
		Kamcord.snapshotReadyAtFilePath = null;
		Kamcord.pushNotifCallToActionButtonPressed = null;
		Kamcord.adjustAndroidWhitelist = null;
		Kamcord.isEnabledChanged = null;
		ClearListeners();
	}

	protected static void CallKamcordDidStartRecording()
	{
		if (Kamcord.kamcordDidStartRecording != null)
		{
			Kamcord.kamcordDidStartRecording();
		}
	}

	protected static void CallKamcordDidStopRecording()
	{
		if (Kamcord.kamcordDidStopRecording != null)
		{
			Kamcord.kamcordDidStopRecording();
		}
	}

	protected static void CallKamcordViewDidAppear()
	{
		if (Kamcord.kamcordViewDidAppear != null)
		{
			Kamcord.kamcordViewDidAppear();
		}
	}

	protected static void CallKamcordViewDidDisappear()
	{
		if (Kamcord.kamcordViewDidDisappear != null)
		{
			Kamcord.kamcordViewDidDisappear();
		}
	}

	protected static void CallKamcordViewWillDisappear()
	{
		if (Kamcord.kamcordViewWillDisappear != null)
		{
			Kamcord.kamcordViewWillDisappear();
		}
	}

	protected static void CallKamcordViewDidNotAppear()
	{
		if (Kamcord.kamcordViewDidNotAppear != null)
		{
			Kamcord.kamcordViewDidNotAppear();
		}
	}

	protected static void CallKamcordWatchViewDidAppear()
	{
		if (Kamcord.kamcordWatchViewDidAppear != null)
		{
			Kamcord.kamcordWatchViewDidAppear();
		}
	}

	protected static void CallKamcordWatchViewDidDisappear()
	{
		if (Kamcord.kamcordWatchViewDidDisappear != null)
		{
			Kamcord.kamcordWatchViewDidDisappear();
		}
	}

	protected static void CallKamcordWatchViewWillDisappear()
	{
		if (Kamcord.kamcordWatchViewWillDisappear != null)
		{
			Kamcord.kamcordWatchViewWillDisappear();
		}
	}

	protected static void CallVideoWillBeginUploading(string videoID, string URL)
	{
		if (Kamcord.videoWillBeginUploading != null)
		{
			Kamcord.videoWillBeginUploading(videoID, URL);
		}
	}

	protected static void CallVideoUploadProgressed(string videoID, float progress)
	{
		if (Kamcord.videoUploadProgressed != null)
		{
			Kamcord.videoUploadProgressed(videoID, progress);
		}
	}

	protected static void CallVideoFinishedUploading(string videoID, bool success)
	{
		if (Kamcord.videoFinishedUploading != null)
		{
			Kamcord.videoFinishedUploading(videoID, success);
		}
	}

	protected static void CallVideoSharedTo(string videoID, string networkName, bool success)
	{
		if (Kamcord.videoSharedTo != null)
		{
			Kamcord.videoSharedTo(videoID, networkName, success);
		}
	}

	protected static void CallVideoSharedToFacebook(string videoID, bool success)
	{
		if (Kamcord.videoSharedToFacebook != null)
		{
			Kamcord.videoSharedToFacebook(videoID, success);
		}
	}

	protected static void CallVideoSharedToTwitter(string videoID, bool success)
	{
		if (Kamcord.videoSharedToTwitter != null)
		{
			Kamcord.videoSharedToTwitter(videoID, success);
		}
	}

	protected static void CallVideoSharedToYoutube(string videoID, bool success)
	{
		if (Kamcord.videoSharedToYoutube != null)
		{
			Kamcord.videoSharedToYoutube(videoID, success);
		}
	}

	protected static void CallShareButtonPressed()
	{
		if (Kamcord.shareButtonPressed != null)
		{
			Kamcord.shareButtonPressed();
		}
	}

	protected static void CallVideoThumbnailReadyAtFilePath(string url)
	{
		if (Kamcord.videoThumbnailReadyAtFilePath != null)
		{
			Kamcord.videoThumbnailReadyAtFilePath(url);
		}
	}

	protected static void CallPushNotifCallToActionButtonPressed()
	{
		if (Kamcord.pushNotifCallToActionButtonPressed != null)
		{
			Kamcord.pushNotifCallToActionButtonPressed();
		}
	}

	protected static void CallSnapshotReadyAtFilePath(string filepath)
	{
		if (Kamcord.snapshotReadyAtFilePath != null)
		{
			Kamcord.snapshotReadyAtFilePath(filepath);
		}
	}

	protected static void CallAttributedKamcordInstall()
	{
		if (Kamcord.attributedKamcordInstall != null)
		{
			Kamcord.attributedKamcordInstall();
		}
	}

	public static void CallAdjustAndroidWhitelist()
	{
		if (Kamcord.adjustAndroidWhitelist != null)
		{
			Kamcord.adjustAndroidWhitelist();
		}
	}

	public static void CallIsEnabledChanged(bool isEnabled)
	{
		if (Kamcord.isEnabledChanged != null)
		{
			Kamcord.isEnabledChanged(isEnabled);
		}
	}

	private static Implementation implementation()
	{
		if (implementation_ == null && (iOSEnabled_ || androidEnabled_) && (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) && androidEnabled_ && KamcordImplementationAndroid.getSDKVersion() >= 16)
		{
			implementation_ = new KamcordImplementationAndroid();
		}
		if (implementation_ == null)
		{
			implementation_ = new Implementation();
		}
		return implementation_;
	}

	public static void SetLoggingEnabled(bool value)
	{
		implementation().SetLoggingEnabled(value);
	}

	public static bool IsEnabled()
	{
		return implementation().IsEnabled();
	}

	public static string GetDisabledReason()
	{
		return implementation().GetDisabledReason();
	}

	public static void WhitelistBoard(string boardName)
	{
		implementation().WhitelistBoard(boardName);
	}

	public static void BlacklistBoard(string boardName)
	{
		implementation().BlacklistBoard(boardName);
	}

	public static void WhitelistDevice(string deviceName)
	{
		implementation().WhitelistDevice(deviceName);
	}

	public static void BlacklistDevice(string deviceName)
	{
		implementation().BlacklistDevice(deviceName);
	}

	public static void WhitelistBoard(string boardName, int sdkVersion)
	{
		implementation().WhitelistBoard(boardName, sdkVersion);
	}

	public static void BlacklistBoard(string boardName, int sdkVersion)
	{
		implementation().BlacklistBoard(boardName, sdkVersion);
	}

	public static void WhitelistDevice(string deviceName, int sdkVersion)
	{
		implementation().WhitelistDevice(deviceName, sdkVersion);
	}

	public static void BlacklistDevice(string deviceName, int sdkVersion)
	{
		implementation().BlacklistDevice(deviceName, sdkVersion);
	}

	public static void WhitelistAllBoards()
	{
		implementation().WhitelistAllBoards();
	}

	public static void BlacklistAllBoards()
	{
		implementation().BlacklistAllBoards();
	}

	public static void WhitelistAll()
	{
		implementation().WhitelistAll();
	}

	public static void BlacklistAll()
	{
		implementation().BlacklistAll();
	}

	public static string GetBoard()
	{
		return implementation().GetBoard();
	}

	public static bool IsWhitelisted(string boardName)
	{
		return implementation().IsWhitelisted(boardName);
	}

	public static void DoneChangingWhitelist()
	{
		implementation().DoneChangingWhitelist();
	}

	public static void SetVideoTitle(string title)
	{
		implementation().SetVideoTitle(title);
	}

	public static void SetYouTubeSettings(string description, string tags)
	{
		implementation().SetYouTubeSettings(description, tags);
	}

	public static void SetFacebookAppID(string facebookAppID)
	{
		implementation().SetFacebookAppID(facebookAppID);
	}

	public static void SetFacebookAppIDAndShareAuth(string facebookAppID, bool useSharedAuth)
	{
		implementation().SetFacebookAppIDAndShareAuth(facebookAppID, useSharedAuth);
	}

	public static void LogoutOfSharedFacebookAuth()
	{
		implementation().LogoutOfSharedFacebookAuth();
	}

	public static void SetWeChatAppID(string weChatAppID)
	{
		implementation().SetWeChatAppID(weChatAppID);
	}

	public static void SetFacebookDescription(string facebookDescription)
	{
		implementation().SetFacebookDescription(facebookDescription);
	}

	public static void SetDefaultTweet(string tweet)
	{
		implementation().SetDefaultTweet(tweet);
	}

	public static void SetTwitterDescription(string twitterDescription)
	{
		implementation().SetTwitterDescription(twitterDescription);
	}

	public static void SetDefaultEmailSubject(string subject)
	{
		implementation().SetDefaultEmailSubject(subject);
	}

	public static void SetDefaultEmailBody(string body)
	{
		implementation().SetDefaultEmailBody(body);
	}

	public static void SetShareTargets(ShareTarget target1, ShareTarget target2, ShareTarget target3, ShareTarget target4)
	{
		implementation().SetShareTargets(target1, target2, target3, target4);
	}

	public static void SetVideoMetadata(Dictionary<string, object> metadata)
	{
		implementation().SetVideoMetadata(metadata);
	}

	public static void SetDeveloperMetadata(MetadataType metadataType, string displayKey, string displayValue)
	{
		implementation().SetDeveloperMetadata(metadataType, displayKey, displayValue);
	}

	public static void SetDeveloperMetadataWithNumericValue(MetadataType metadataType, string displayKey, string displayValue, double numericValue)
	{
		implementation().SetDeveloperMetadataWithNumericValue(metadataType, displayKey, displayValue, numericValue);
	}

	public static bool VideoExistsWithMetadataConstraints(Dictionary<string, object> metadata)
	{
		return implementation().VideoExistsWithMetadataConstraints(metadata);
	}

	public static void ShowVideoWithMetadataConstraints(Dictionary<string, object> metadata, string title)
	{
		implementation().ShowVideoWithMetadataConstraints(metadata, title);
	}

	public static void ShowVideoWithVideoID(string videoID, string title)
	{
		implementation().ShowVideoWithVideoID(videoID, title);
	}

	public static void SetMaxFreeDiskSpacePercentageUsage(double percentage)
	{
		implementation().SetMaxFreeDiskSpacePercentageUsage(percentage);
	}

	public static string Version()
	{
		return implementation().Version();
	}

	public static void SetLevelAndScore(string level, double score)
	{
		implementation().SetLevelAndScore(level, score);
	}

	public static void BeginDraw()
	{
		implementation().BeginDraw();
	}

	public static void EndDraw()
	{
		implementation().EndDraw();
	}

	public static void StartRecording()
	{
		implementation().StartRecording();
		CallKamcordDidStartRecording();
	}

	public static void StopRecording()
	{
		implementation().StopRecording();
		CallKamcordDidStopRecording();
	}

	public static void Pause()
	{
		implementation().Pause();
	}

	public static void Resume()
	{
		implementation().Resume();
	}

	public static bool IsRecording()
	{
		return implementation().IsRecording();
	}

	public static bool IsPaused()
	{
		return implementation().IsPaused();
	}

	public static bool IsViewShowing()
	{
		return implementation().IsViewShowing();
	}

	public static void Snapshot(string filename)
	{
		implementation().Snapshot(filename);
	}

	public static void SetVideoQuality(VideoQuality quality)
	{
		implementation().SetVideoQuality(quality);
	}

	public static void SetVoiceOverlayEnabled(bool enabled)
	{
		implementation().SetVoiceOverlayEnabled(enabled);
		developerSetVoiceOverlay = true;
	}

	public static bool VoiceOverlayEnabled()
	{
		return implementation().VoiceOverlayEnabled();
	}

	public static void ActivateVoiceOverlay(bool activate)
	{
		implementation().ActivateVoiceOverlay(activate);
	}

	public static bool VoiceOverlayActivated()
	{
		return implementation().VoiceOverlayActivated();
	}

	public static void CaptureFrame()
	{
		implementation().CaptureFrame();
	}

	public static void SetNotificationsEnabled(bool notificationsEnabled)
	{
		implementation().SetNotificationsEnabled(notificationsEnabled);
	}

	public static void FireTestNotification()
	{
		implementation().FireTestNotification();
	}

	public static void ShowView()
	{
		implementation().ShowView();
	}

	public static void ShowWatchView()
	{
		implementation().ShowWatchView();
	}

	public static void SetMaximumVideoLength(uint seconds)
	{
		implementation().SetMaximumVideoLength(seconds);
	}

	public static uint MaximumVideoLength()
	{
		return implementation().MaximumVideoLength();
	}

	public static void SetVideoFPS(uint videoFPS)
	{
		implementation().SetVideoFPS(videoFPS);
	}

	public static uint VideoFPS()
	{
		return implementation().VideoFPS();
	}

	public static void SetShouldPauseGameEngine(bool shouldPause)
	{
		implementation().SetShouldPauseGameEngine(shouldPause);
	}

	public static bool ShouldPauseGameEngine()
	{
		return implementation().ShouldPauseGameEngine();
	}

	public static void SetAgeRestrictionEnabled(bool restricted)
	{
		implementation().SetAgeRestrictionEnabled(restricted);
	}

	public static bool IsAgeRestrictionEnabled()
	{
		return implementation().IsAgeRestrictionEnabled();
	}

	public static void SetVideoIncompleteWarningEnabled(bool enabled)
	{
		implementation().SetVideoIncompleteWarningEnabled(enabled);
	}

	public static bool IsVideoComplete()
	{
		return implementation().IsVideoComplete();
	}

	public static void Disable()
	{
		implementation().Disable();
	}

	public static void SetAudioListener(AudioListener audioListener)
	{
		GameObject gameObject = audioListener.gameObject;
		bool flag = true;
		Component[] components = gameObject.GetComponents(typeof(MonoBehaviour));
		Component[] array = components;
		foreach (Component component in array)
		{
			if (typeof(KamcordAudioRecorder).Equals(component.GetType()))
			{
				Debug.Log("Game Object already has KamcordAudioRecorder attached, not re-attaching for scene " + Application.loadedLevelName);
				flag = false;
			}
		}
		int numChannels = ((AudioSettings.speakerMode == AudioSpeakerMode.Mono) ? 1 : 2);
		implementation().SetAudioSettings(AudioSettings.outputSampleRate, numChannels);
		if (flag)
		{
			Debug.Log("Programmatically adding KamcordAudioRecorder for scene " + Application.loadedLevelName);
			audioListener.enabled = false;
			gameObject.AddComponent("KamcordAudioRecorder");
			audioListener.enabled = true;
		}
	}

	public static void WriteAudioData(float[] data, int numSamples)
	{
		implementation().WriteAudioData(data, numSamples);
	}

	public static void Init(string devKey, string devSecret, string appName, VideoQuality videoQuality)
	{
		implementation().Init(devKey, devSecret, appName, videoQuality);
	}

	public static void SetDeviceBlacklist(bool disableiPod4G, bool disableiPod5G, bool disableiPhone3GS, bool disableiPhone4, bool disableiPhone4S, bool disableiPhone5, bool disableiPhone5C, bool disableiPhone5S, bool disableiPad1, bool disableiPad2, bool disableiPadMini, bool disableiPad3, bool disableiPad4, bool disableiPadAir)
	{
		implementation().SetDeviceBlacklist(disableiPod4G, disableiPod5G, disableiPhone3GS, disableiPhone4, disableiPhone4S, disableiPhone5, disableiPhone5C, disableiPhone5S, disableiPad1, disableiPad2, disableiPadMini, disableiPad3, disableiPad4, disableiPadAir);
	}

	public static void SetDefaultTitle(string title)
	{
		implementation().SetDefaultTitle(title);
	}

	public static void SetYouTubeVideoCategory(YouTubeVideoCategory category)
	{
		implementation().SetYouTubeVideoCategory(category);
	}

	public static void SetFacebookSettings(string title, string caption, string description)
	{
		implementation().SetFacebookSettings(title, caption, description);
	}

	public static void SetDefaultEmailSubjectAndBody(string subject, string body)
	{
		implementation().SetDefaultEmailSubjectAndBody(subject, body);
	}

	public static void SetCrossPromoIcon(string localImageFileURL)
	{
		implementation().SetCrossPromoIcon(localImageFileURL);
	}

	public static void TurnOffAutomaticAudioRecording(bool status)
	{
		implementation().TurnOffAutomaticAudioRecording(status);
	}

	public static void SetMode(int mode)
	{
		implementation().SetMode(mode);
	}

	public static void SetFlushOnCopy(bool flush)
	{
		implementation().SetFlushOnCopy(flush);
	}

	private void Awake()
	{
		if (instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		base.gameObject.name = "KamcordPrefab";
		UnityEngine.Object.DontDestroyOnLoad(this);
		instance = this;
		iOSEnabled_ = enableIOS;
		androidEnabled_ = enableAndroid;
		loggingEnabled_ = enableLogging;
		implementation().Awake(this);
		SetMode(0);
	}

	private void Start()
	{
		implementation().Start(this);
		if (!developerSetVoiceOverlay)
		{
			implementation().SetVoiceOverlayEnabled(enableVoiceOverlay);
		}
	}

	private void OnApplicationPause(bool pause)
	{
		if (pause)
		{
			Pause();
		}
		else
		{
			Resume();
		}
	}

	public static void AddListener(KamcordCallbackInterface listener)
	{
		if (!listeners.Contains(listener))
		{
			listeners.Add(listener);
		}
	}

	public static void RemoveListener(KamcordCallbackInterface listener)
	{
		listeners.Remove(listener);
	}

	public static void ClearListeners()
	{
		listeners.Clear();
	}

	private void _KamcordViewDidAppear(string empty)
	{
		timeScale = Time.timeScale;
		Time.timeScale = 0f;
		CallKamcordViewDidAppear();
	}

	private void _KamcordViewDidDisappear(string empty)
	{
		Time.timeScale = timeScale;
		CallKamcordViewDidDisappear();
	}

	private void _KamcordViewDidNotAppear(string empty)
	{
		CallKamcordViewDidNotAppear();
	}

	private void _KamcordVideoWillBeginUploading(string jsonString)
	{
		Dictionary<string, object> dictionary = Json.Deserialize(jsonString) as Dictionary<string, object>;
		CallVideoWillBeginUploading((string)dictionary["videoID"], (string)dictionary["URL"]);
	}

	private void _KamcordVideoUploadProgressed(string jsonString)
	{
		Dictionary<string, object> dictionary = Json.Deserialize(jsonString) as Dictionary<string, object>;
		CallVideoUploadProgressed((string)dictionary["videoID"], Convert.ToSingle(dictionary["progress"]));
	}

	private void _KamcordVideoFinishedUploading(string jsonString)
	{
		Dictionary<string, object> dictionary = Json.Deserialize(jsonString) as Dictionary<string, object>;
		CallVideoFinishedUploading((string)dictionary["videoID"], (bool)dictionary["success"]);
	}

	private void _KamcordVideoSharedTo(string jsonString)
	{
		Dictionary<string, object> dictionary = Json.Deserialize(jsonString) as Dictionary<string, object>;
		CallVideoSharedTo((string)dictionary["videoID"], (string)dictionary["networkName"], (bool)dictionary["success"]);
	}

	private void _KamcordVideoSharedToFacebook(string jsonString)
	{
		Dictionary<string, object> dictionary = Json.Deserialize(jsonString) as Dictionary<string, object>;
		CallVideoSharedToFacebook((string)dictionary["videoID"], (bool)dictionary["success"]);
	}

	private void _KamcordVideoSharedToTwitter(string jsonString)
	{
		Dictionary<string, object> dictionary = Json.Deserialize(jsonString) as Dictionary<string, object>;
		CallVideoSharedToTwitter((string)dictionary["videoID"], (bool)dictionary["success"]);
	}

	private void _KamcordVideoSharedToYoutube(string jsonString)
	{
		Dictionary<string, object> dictionary = Json.Deserialize(jsonString) as Dictionary<string, object>;
		CallVideoSharedToYoutube((string)dictionary["videoID"], (bool)dictionary["success"]);
	}

	private void _KamcordVideoThumbnailReadyAtFilePath(string jsonString)
	{
		Dictionary<string, object> dictionary = Json.Deserialize(jsonString) as Dictionary<string, object>;
		CallVideoThumbnailReadyAtFilePath((string)dictionary["url"]);
	}

	private void _KamcordIsEnabledChanged(string jsonString)
	{
		Dictionary<string, object> dictionary = Json.Deserialize(jsonString) as Dictionary<string, object>;
		CallIsEnabledChanged((bool)dictionary["isEnabled"]);
	}
}
public class KamcordAndroidCameraAttachment : MonoBehaviour
{
	private void Awake()
	{
		KamcordImplementationAndroid.SetRenderCameraEnabled("Pre", flag: false);
		KamcordImplementationAndroid.SetRenderCameraEnabled("Post", flag: false);
	}

	private void OnDestroy()
	{
		KamcordImplementationAndroid.SetRenderCameraEnabled("Pre", flag: true);
		KamcordImplementationAndroid.SetRenderCameraEnabled("Post", flag: true);
	}

	private void OnPreRender()
	{
		Kamcord.BeginDraw();
	}

	private void OnPostRender()
	{
		Kamcord.EndDraw();
	}
}
public class KamcordAndroidPostRender : MonoBehaviour
{
	private IEnumerator OnPostRender()
	{
		yield return new WaitForEndOfFrame();
		Kamcord.EndDraw();
	}
}
public class KamcordAndroidPreRender : MonoBehaviour
{
	private void OnPreRender()
	{
		Kamcord.BeginDraw();
	}
}
public class KamcordAudioRecorder : MonoBehaviour
{
	private void OnAudioFilterRead(float[] data, int numChannels)
	{
		if (Kamcord.IsRecording())
		{
			Kamcord.WriteAudioData(data, data.Length / numChannels);
		}
	}
}
public interface KamcordCallbackInterface
{
}
namespace KamcordJSON
{
	public static class Json
	{
		private sealed class Parser : IDisposable
		{
			private enum TOKEN
			{
				NONE,
				CURLY_OPEN,
				CURLY_CLOSE,
				SQUARED_OPEN,
				SQUARED_CLOSE,
				COLON,
				COMMA,
				STRING,
				NUMBER,
				TRUE,
				FALSE,
				NULL
			}

			private const string WHITE_SPACE = " \t\n\r";

			private const string WORD_BREAK = " \t\n\r{}[],:\"";

			private StringReader json;

			private char PeekChar => Convert.ToChar(json.Peek());

			private char NextChar => Convert.ToChar(json.Read());

			private string NextWord
			{
				get
				{
					StringBuilder stringBuilder = new StringBuilder();
					while (" \t\n\r{}[],:\"".IndexOf(PeekChar) == -1)
					{
						stringBuilder.Append(NextChar);
						if (json.Peek() == -1)
						{
							break;
						}
					}
					return stringBuilder.ToString();
				}
			}

			private TOKEN NextToken
			{
				get
				{
					EatWhitespace();
					if (json.Peek() == -1)
					{
						return TOKEN.NONE;
					}
					switch (PeekChar)
					{
					case '{':
						return TOKEN.CURLY_OPEN;
					case '}':
						json.Read();
						return TOKEN.CURLY_CLOSE;
					case '[':
						return TOKEN.SQUARED_OPEN;
					case ']':
						json.Read();
						return TOKEN.SQUARED_CLOSE;
					case ',':
						json.Read();
						return TOKEN.COMMA;
					case '"':
						return TOKEN.STRING;
					case ':':
						return TOKEN.COLON;
					case '-':
					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
						return TOKEN.NUMBER;
					default:
						return NextWord switch
						{
							"false" => TOKEN.FALSE, 
							"true" => TOKEN.TRUE, 
							"null" => TOKEN.NULL, 
							_ => TOKEN.NONE, 
						};
					}
				}
			}

			private Parser(string jsonString)
			{
				json = new StringReader(jsonString);
			}

			public static object Parse(string jsonString)
			{
				using Parser parser = new Parser(jsonString);
				return parser.ParseValue();
			}

			public void Dispose()
			{
				json.Dispose();
				json = null;
			}

			private Dictionary<string, object> ParseObject()
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				json.Read();
				while (true)
				{
					switch (NextToken)
					{
					case TOKEN.COMMA:
						continue;
					case TOKEN.NONE:
						return null;
					case TOKEN.CURLY_CLOSE:
						return dictionary;
					}
					string text = ParseString();
					if (text == null)
					{
						return null;
					}
					if (NextToken != TOKEN.COLON)
					{
						return null;
					}
					json.Read();
					dictionary[text] = ParseValue();
				}
			}

			private List<object> ParseArray()
			{
				List<object> list = new List<object>();
				json.Read();
				bool flag = true;
				while (flag)
				{
					TOKEN nextToken = NextToken;
					switch (nextToken)
					{
					case TOKEN.NONE:
						return null;
					case TOKEN.SQUARED_CLOSE:
						flag = false;
						break;
					default:
					{
						object item = ParseByToken(nextToken);
						list.Add(item);
						break;
					}
					case TOKEN.COMMA:
						break;
					}
				}
				return list;
			}

			private object ParseValue()
			{
				TOKEN nextToken = NextToken;
				return ParseByToken(nextToken);
			}

			private object ParseByToken(TOKEN token)
			{
				return token switch
				{
					TOKEN.STRING => ParseString(), 
					TOKEN.NUMBER => ParseNumber(), 
					TOKEN.CURLY_OPEN => ParseObject(), 
					TOKEN.SQUARED_OPEN => ParseArray(), 
					TOKEN.TRUE => true, 
					TOKEN.FALSE => false, 
					TOKEN.NULL => null, 
					_ => null, 
				};
			}

			private string ParseString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				json.Read();
				bool flag = true;
				while (flag)
				{
					if (json.Peek() == -1)
					{
						flag = false;
						break;
					}
					char nextChar = NextChar;
					switch (nextChar)
					{
					case '"':
						flag = false;
						break;
					case '\\':
						if (json.Peek() == -1)
						{
							flag = false;
							break;
						}
						nextChar = NextChar;
						switch (nextChar)
						{
						case '"':
						case '/':
						case '\\':
							stringBuilder.Append(nextChar);
							break;
						case 'b':
							stringBuilder.Append('\b');
							break;
						case 'f':
							stringBuilder.Append('\f');
							break;
						case 'n':
							stringBuilder.Append('\n');
							break;
						case 'r':
							stringBuilder.Append('\r');
							break;
						case 't':
							stringBuilder.Append('\t');
							break;
						case 'u':
						{
							StringBuilder stringBuilder2 = new StringBuilder();
							for (int i = 0; i < 4; i++)
							{
								stringBuilder2.Append(NextChar);
							}
							stringBuilder.Append((char)Convert.ToInt32(stringBuilder2.ToString(), 16));
							break;
						}
						}
						break;
					default:
						stringBuilder.Append(nextChar);
						break;
					}
				}
				return stringBuilder.ToString();
			}

			private object ParseNumber()
			{
				string nextWord = NextWord;
				if (nextWord.IndexOf('.') == -1)
				{
					long.TryParse(nextWord, out var result);
					return result;
				}
				double.TryParse(nextWord, out var result2);
				return result2;
			}

			private void EatWhitespace()
			{
				while (" \t\n\r".IndexOf(PeekChar) != -1)
				{
					json.Read();
					if (json.Peek() == -1)
					{
						break;
					}
				}
			}
		}

		private sealed class Serializer
		{
			private StringBuilder builder;

			private Serializer()
			{
				builder = new StringBuilder();
			}

			public static string Serialize(object obj)
			{
				Serializer serializer = new Serializer();
				serializer.SerializeValue(obj);
				return serializer.builder.ToString();
			}

			private void SerializeValue(object value)
			{
				if (value == null)
				{
					builder.Append("null");
				}
				else if (value is string str)
				{
					SerializeString(str);
				}
				else if (value is bool)
				{
					builder.Append(value.ToString().ToLower());
				}
				else if (value is IList anArray)
				{
					SerializeArray(anArray);
				}
				else if (value is IDictionary obj)
				{
					SerializeObject(obj);
				}
				else if (value is char)
				{
					SerializeString(value.ToString());
				}
				else
				{
					SerializeOther(value);
				}
			}

			private void SerializeObject(IDictionary obj)
			{
				bool flag = true;
				builder.Append('{');
				foreach (object key in obj.Keys)
				{
					if (!flag)
					{
						builder.Append(',');
					}
					SerializeString(key.ToString());
					builder.Append(':');
					SerializeValue(obj[key]);
					flag = false;
				}
				builder.Append('}');
			}

			private void SerializeArray(IList anArray)
			{
				builder.Append('[');
				bool flag = true;
				foreach (object item in anArray)
				{
					if (!flag)
					{
						builder.Append(',');
					}
					SerializeValue(item);
					flag = false;
				}
				builder.Append(']');
			}

			private void SerializeString(string str)
			{
				builder.Append('"');
				char[] array = str.ToCharArray();
				char[] array2 = array;
				foreach (char c in array2)
				{
					switch (c)
					{
					case '"':
						builder.Append("\\\"");
						continue;
					case '\\':
						builder.Append("\\\\");
						continue;
					case '\b':
						builder.Append("\\b");
						continue;
					case '\f':
						builder.Append("\\f");
						continue;
					case '\n':
						builder.Append("\\n");
						continue;
					case '\r':
						builder.Append("\\r");
						continue;
					case '\t':
						builder.Append("\\t");
						continue;
					}
					int num = Convert.ToInt32(c);
					if (num >= 32 && num <= 126)
					{
						builder.Append(c);
					}
					else
					{
						builder.Append("\\u" + Convert.ToString(num, 16).PadLeft(4, '0'));
					}
				}
				builder.Append('"');
			}

			private void SerializeOther(object value)
			{
				if (value is float || value is int || value is uint || value is long || value is double || value is sbyte || value is byte || value is short || value is ushort || value is ulong || value is decimal)
				{
					builder.Append(value.ToString());
				}
				else
				{
					SerializeString(value.ToString());
				}
			}
		}

		public static object Deserialize(string json)
		{
			if (json == null)
			{
				return null;
			}
			return Parser.Parse(json);
		}

		public static string Serialize(object obj)
		{
			return Serializer.Serialize(obj);
		}
	}
}
public class KamcordThumbnailUpdater : MonoBehaviour
{
	public Texture2D playButtonTexture;

	public float thumbnailRelativeX = 0.25f;

	public float thumbnailRelativeY = 0.25f;

	public float thumbnailToScreenRatio = 0.4f;

	private GUITexture theGuiTexture;

	private float playButtonToThumbnailRatio = 0.5f;

	private Rect playButtonLocationAndSize;

	public void EnableThumbnail(bool enable)
	{
		if (theGuiTexture != null)
		{
			theGuiTexture.enabled = enable;
		}
	}

	private void Start()
	{
		base.gameObject.AddComponent("GUITexture");
		GUITexture[] components = base.gameObject.GetComponents<GUITexture>();
		if (components.Length == 0)
		{
			throw new Exception("Kamcord script " + base.name + " needs to have at least one GUITexture component on the attached game object named: " + base.gameObject.name);
		}
		theGuiTexture = components[0];
		Kamcord.videoThumbnailReadyAtFilePath += VideoThumbnailReadyAtFilePath;
		EnableThumbnail(enable: false);
	}

	private void OnDestroy()
	{
		Kamcord.videoThumbnailReadyAtFilePath -= VideoThumbnailReadyAtFilePath;
	}

	private void Update()
	{
		if (!(theGuiTexture != null))
		{
			return;
		}
		Touch[] touches = Input.touches;
		for (int i = 0; i < touches.Length; i++)
		{
			Touch touch = touches[i];
			if (touch.phase == TouchPhase.Began && theGuiTexture.HitTest(touch.position))
			{
				Kamcord.ShowView();
				break;
			}
		}
	}

	private void OnGUI()
	{
		if (theGuiTexture != null && theGuiTexture.enabled)
		{
			GUI.Label(playButtonLocationAndSize, playButtonTexture);
		}
	}

	public void VideoThumbnailReadyAtFilePath(string filepath)
	{
		Debug.Log("Thumbnail exists at " + filepath);
		if (File.Exists(filepath))
		{
			SetThumbnailTextureToFilepath(filepath);
		}
	}

	private IEnumerator WaitForLoadToFinishAndThenSetThumbnail(WWW loader)
	{
		yield return loader;
		if (loader.error == null)
		{
			if (thumbnailToScreenRatio < 0.2f)
			{
				thumbnailToScreenRatio = 0.2f;
			}
			float absoluteX = thumbnailRelativeX * (float)Screen.width;
			float absoluteY = thumbnailRelativeY * (float)Screen.height;
			float absoluteWidth = thumbnailToScreenRatio * (float)Screen.width;
			float absoluteHeight = thumbnailToScreenRatio * (float)Screen.height;
			float playButtonWidth = Mathf.Min(playButtonTexture.width, playButtonToThumbnailRatio * absoluteWidth);
			float playButtonHeight = playButtonWidth;
			float playButtonAbsoluteX = absoluteX + 0.5f * (absoluteWidth - playButtonWidth);
			float playButtonAbsoluteY = (float)Screen.height - absoluteY - 0.5f * (absoluteHeight + playButtonHeight);
			playButtonLocationAndSize = new Rect(playButtonAbsoluteX, playButtonAbsoluteY, playButtonWidth, playButtonHeight);
			base.transform.position = Vector3.zero;
			base.transform.localScale = Vector3.zero;
			theGuiTexture.pixelInset = new Rect(absoluteX, absoluteY, absoluteWidth, absoluteHeight);
			theGuiTexture.texture = loader.texture;
			EnableThumbnail(enable: true);
		}
	}

	private void SetThumbnailTextureToFilepath(string filepath)
	{
		WWW loader = new WWW("file://" + filepath);
		StartCoroutine(WaitForLoadToFinishAndThenSetThumbnail(loader));
	}
}
public class KamcordImplementationAndroid : Kamcord.Implementation
{
	private AndroidJavaClass kamcordJavaClass_;

	private static bool beginDrawErrorOnce;

	private bool frameCaptured;

	private static bool endDrawErrorOnce;

	private AndroidJavaClass kamcordJavaClass()
	{
		if (kamcordJavaClass_ == null)
		{
			kamcordJavaClass_ = new AndroidJavaClass("com.kamcord.android.Kamcord");
		}
		if (kamcordJavaClass_ == null)
		{
			Debug.Log("Kamcord: Unable to find Kamcord java class.");
		}
		return kamcordJavaClass_;
	}

	public override void SetLoggingEnabled(bool value)
	{
		if (kamcordJavaClass() != null)
		{
			kamcordJavaClass().CallStatic("setLoggingEnabled", value);
		}
	}

	public override bool IsEnabled()
	{
		if (kamcordJavaClass() != null)
		{
			return kamcordJavaClass().CallStatic<bool>("isEnabled", new object[0]);
		}
		return false;
	}

	public override string GetDisabledReason()
	{
		if (kamcordJavaClass() != null)
		{
			return kamcordJavaClass().CallStatic<string>("getDisabledReason", new object[0]);
		}
		return "Kamcord java class object not accessible from Unity script.";
	}

	public override void WhitelistBoard(string boardName)
	{
		if (kamcordJavaClass() == null)
		{
			Debug.Log("Kamcord: WhitelistBoard called with no kamcordJavaClass");
			return;
		}
		kamcordJavaClass().CallStatic("whitelistBoard", boardName);
	}

	public override void BlacklistBoard(string boardName)
	{
		if (kamcordJavaClass() == null)
		{
			Debug.Log("Kamcord: BlacklistBoard called with no kamcordJavaClass");
			return;
		}
		kamcordJavaClass().CallStatic("blacklistBoard", boardName);
	}

	public override void WhitelistDevice(string deviceName)
	{
		if (kamcordJavaClass() == null)
		{
			Debug.Log("Kamcord: WhitelistDevice called with no kamcordJavaClass");
			return;
		}
		kamcordJavaClass().CallStatic("whitelistDevice", deviceName);
	}

	public override void BlacklistDevice(string deviceName)
	{
		if (kamcordJavaClass() == null)
		{
			Debug.Log("Kamcord: BlacklistDevice called with no kamcordJavaClass");
			return;
		}
		kamcordJavaClass().CallStatic("blacklistDevice", deviceName);
	}

	public override void WhitelistBoard(string boardName, int sdkVersion)
	{
		if (kamcordJavaClass() == null)
		{
			Debug.Log("Kamcord: WhitelistBoard called with no kamcordJavaClass");
			return;
		}
		kamcordJavaClass().CallStatic("whitelistBoard", boardName, sdkVersion);
	}

	public override void BlacklistBoard(string boardName, int sdkVersion)
	{
		if (kamcordJavaClass() == null)
		{
			Debug.Log("Kamcord: BlacklistBoard called with no kamcordJavaClass");
			return;
		}
		kamcordJavaClass().CallStatic("blacklistBoard", boardName, sdkVersion);
	}

	public override void WhitelistDevice(string deviceName, int sdkVersion)
	{
		if (kamcordJavaClass() == null)
		{
			Debug.Log("Kamcord: WhitelistDevice called with no kamcordJavaClass");
			return;
		}
		kamcordJavaClass().CallStatic("whitelistDevice", deviceName, sdkVersion);
	}

	public override void BlacklistDevice(string deviceName, int sdkVersion)
	{
		if (kamcordJavaClass() == null)
		{
			Debug.Log("Kamcord: BlacklistDevice called with no kamcordJavaClass");
			return;
		}
		kamcordJavaClass().CallStatic("blacklistDevice", deviceName, sdkVersion);
	}

	public override void WhitelistAllBoards()
	{
		if (kamcordJavaClass() == null)
		{
			Debug.Log("Kamcord: WhitelistAllBoards called with no kamcordJavaClass");
		}
		else
		{
			kamcordJavaClass().CallStatic("whitelistAllBoards");
		}
	}

	public override void BlacklistAllBoards()
	{
		if (kamcordJavaClass() == null)
		{
			Debug.Log("Kamcord: BlacklistAllBoards called with no kamcordJavaClass");
		}
		else
		{
			kamcordJavaClass().CallStatic("blacklistAllBoards");
		}
	}

	public override void WhitelistAll()
	{
		if (kamcordJavaClass() == null)
		{
			Debug.Log("Kamcord: WhitelistAll called with no kamcordJavaClass");
		}
		else
		{
			kamcordJavaClass().CallStatic("whitelistAll");
		}
	}

	public override void BlacklistAll()
	{
		if (kamcordJavaClass() == null)
		{
			Debug.Log("Kamcord: BlacklistAll called with no kamcordJavaClass");
		}
		else
		{
			kamcordJavaClass().CallStatic("blacklistAll");
		}
	}

	public override string GetBoard()
	{
		if (kamcordJavaClass() == null)
		{
			Debug.Log("Kamcord: GetBoard called with no kamcordJavaClass");
			return string.Empty;
		}
		return kamcordJavaClass().CallStatic<string>("getBoard", new object[0]);
	}

	public override string GetDevice()
	{
		if (kamcordJavaClass() == null)
		{
			Debug.Log("Kamcord: GetDevice called with no kamcordJavaClass");
			return string.Empty;
		}
		return kamcordJavaClass().CallStatic<string>("getDevice", new object[0]);
	}

	public override bool IsWhitelisted(string boardName)
	{
		if (kamcordJavaClass() == null)
		{
			Debug.Log("Kamcord: IsWhitelisted called with no kamcordJavaClass");
			return false;
		}
		return kamcordJavaClass().CallStatic<bool>("isWhitelisted", new object[1] { boardName });
	}

	public override bool IsWhitelisted()
	{
		if (kamcordJavaClass() == null)
		{
			Debug.Log("Kamcord: IsWhitelisted called with no kamcordJavaClass");
			return false;
		}
		return kamcordJavaClass().CallStatic<bool>("isWhitelisted", new object[0]);
	}

	public override void DoneChangingWhitelist()
	{
		if (kamcordJavaClass() == null)
		{
			Debug.Log("Kamcord: DoneChangingWhitelist called with no kamcordJavaClass");
		}
		else
		{
			kamcordJavaClass().CallStatic("doneChangingWhitelist");
		}
	}

	public override void SetVideoTitle(string title)
	{
		kamcordJavaClass().CallStatic("setDefaultVideoTitle", title);
	}

	public override void SetYouTubeSettings(string description, string tags)
	{
		kamcordJavaClass().CallStatic("setDefaultYoutubeDescription", description);
		kamcordJavaClass().CallStatic("setDefaultYoutubeKeywords", tags);
	}

	public override void SetFacebookDescription(string facebookDescription)
	{
		kamcordJavaClass().CallStatic("setDefaultFacebookDescription", facebookDescription);
	}

	public override void SetDefaultTweet(string tweet)
	{
		kamcordJavaClass().CallStatic("setDefaultTweet", tweet);
	}

	public override void SetTwitterDescription(string twitterDescription)
	{
		kamcordJavaClass().CallStatic("setDefaultTwitterDescription", twitterDescription);
	}

	public override void SetDefaultEmailSubject(string subject)
	{
		kamcordJavaClass().CallStatic("setDefaultEmailSubject", subject);
	}

	public override void SetDefaultEmailBody(string body)
	{
		kamcordJavaClass().CallStatic("setDefaultEmailBody", body);
	}

	public override void SetShareTargets(Kamcord.ShareTarget target1, Kamcord.ShareTarget target2, Kamcord.ShareTarget target3, Kamcord.ShareTarget target4)
	{
		kamcordJavaClass().CallStatic("setShareTargets", new int[4]
		{
			(int)target1,
			(int)target2,
			(int)target3,
			(int)target4
		});
	}

	public override void SetVideoMetadata(Dictionary<string, object> metadata)
	{
		if (metadata != null && metadata.Count > 0)
		{
			kamcordJavaClass().CallStatic("setVideoMetadata", Json.Serialize(metadata));
		}
	}

	public override string Version()
	{
		return kamcordJavaClass().CallStatic<string>("version", new object[0]);
	}

	public override void SetLevelAndScore(string level, double score)
	{
		kamcordJavaClass().CallStatic("setLevel", level);
		kamcordJavaClass().CallStatic("setScore", score);
	}

	public override void SetDeveloperMetadata(Kamcord.MetadataType metadataType, string displayKey, string displayValue)
	{
		kamcordJavaClass().CallStatic("setDeveloperMetadata", (int)metadataType, displayKey, displayValue);
	}

	public override void SetDeveloperMetadataWithNumericValue(Kamcord.MetadataType metadataType, string displayKey, string displayValue, double numericValue)
	{
		kamcordJavaClass().CallStatic("setDeveloperMetadataWithNumericValue", (int)metadataType, displayKey, displayValue, numericValue);
	}

	public override void BeginDraw()
	{
		frameCaptured = false;
		if (kamcordJavaClass() == null)
		{
			if (!beginDrawErrorOnce)
			{
				Debug.Log("Kamcord: BeginDraw called with no kamcordJavaClass().  This error only prints once even if it happens more.");
				beginDrawErrorOnce = true;
			}
		}
		else
		{
			kamcordJavaClass().CallStatic("beginDraw");
		}
	}

	public override void EndDraw()
	{
		if (kamcordJavaClass() == null)
		{
			if (!endDrawErrorOnce)
			{
				Debug.Log("Kamcord: EndDraw called with no kamcordJavaClass().  This error only prints once even if it happens more.");
				endDrawErrorOnce = true;
			}
		}
		else if (!frameCaptured)
		{
			kamcordJavaClass().CallStatic("endDraw");
		}
	}

	public override void StartRecording()
	{
		if (kamcordJavaClass() == null)
		{
			Debug.Log("Kamcord: StartRecording called with no kamcordJavaClass().");
		}
		else
		{
			kamcordJavaClass().CallStatic("startRecording");
		}
	}

	public override void StopRecording()
	{
		if (kamcordJavaClass() == null)
		{
			Debug.Log("Kamcord: StopRecording called with no kamcordJavaClass().");
		}
		else
		{
			kamcordJavaClass().CallStatic("stopRecording");
		}
	}

	public override void Pause()
	{
		if (kamcordJavaClass() == null)
		{
			Debug.Log("Kamcord: Pause called with no kamcordJavaClass().");
		}
		else
		{
			kamcordJavaClass().CallStatic("pauseRecording");
		}
	}

	public override void Resume()
	{
		if (kamcordJavaClass() == null)
		{
			Debug.Log("Kamcord: Resume called with no kamcordJavaClass().");
		}
		else
		{
			kamcordJavaClass().CallStatic("resumeRecording");
		}
	}

	public override bool IsRecording()
	{
		if (kamcordJavaClass() == null)
		{
			Debug.Log("Kamcord: IsRecording called with no kamcordJavaClass().");
			return false;
		}
		return kamcordJavaClass().CallStatic<bool>("isRecording", new object[0]);
	}

	public override bool IsPaused()
	{
		if (kamcordJavaClass() == null)
		{
			Debug.Log("Kamcord: isPaused called with no kamcordJavaClass().");
			return false;
		}
		return kamcordJavaClass().CallStatic<bool>("isPaused", new object[0]);
	}

	public override void SetVideoQuality(Kamcord.VideoQuality quality)
	{
		kamcordJavaClass().CallStatic("setVideoQuality", quality);
	}

	public override bool VoiceOverlayEnabled()
	{
		return kamcordJavaClass().CallStatic<bool>("voiceOverlayEnabled", new object[0]);
	}

	public override bool VoiceOverlayActivated()
	{
		return kamcordJavaClass().CallStatic<bool>("voiceOverlayActivated", new object[0]);
	}

	public override void SetVoiceOverlayEnabled(bool enabled)
	{
		kamcordJavaClass().CallStatic("setVoiceOverlayEnabled", enabled);
	}

	public override void ActivateVoiceOverlay(bool activate)
	{
		kamcordJavaClass().CallStatic("setVoiceOverlayActivated", activate);
	}

	public override void SetVideoIncompleteWarningEnabled(bool enabled)
	{
		kamcordJavaClass().CallStatic("setVideoIncompleteWarningEnabled", enabled);
	}

	public override void TurnOffAutomaticAudioRecording(bool state)
	{
		kamcordJavaClass().CallStatic("setAutomaticAudioRecording", !state);
	}

	public override bool IsVideoComplete()
	{
		return kamcordJavaClass().CallStatic<bool>("isVideoComplete", new object[0]);
	}

	public override void CaptureFrame()
	{
		EndDraw();
		frameCaptured = true;
	}

	public override void ShowView()
	{
		kamcordJavaClass().CallStatic("showView");
	}

	public override void ShowWatchView()
	{
		kamcordJavaClass().CallStatic("showWatchView");
	}

	public override void Init(string devKey, string devSecret, string appName, Kamcord.VideoQuality videoQuality)
	{
		if (kamcordJavaClass() == null)
		{
			Debug.Log("Kamcord: Init called with no kamcordJavaClass().");
			return;
		}
		int num = 1;
		num = ((videoQuality != Kamcord.VideoQuality.Standard && videoQuality == Kamcord.VideoQuality.Trailer) ? 1 : 0);
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
		kamcordJavaClass().CallStatic("initActivity", androidJavaObject);
		kamcordJavaClass().CallStatic("initKeyAndSecret", devKey, devSecret, appName, num);
	}

	public override void Awake(Kamcord kamcordInstance)
	{
		if (kamcordJavaClass() == null)
		{
			Debug.Log("Kamcord: Java class not accessible from C#.");
			return;
		}
		InitializeRenderCamera("Pre");
		InitializeRenderCamera("Post");
	}

	public override void Start(Kamcord kamcordInstance)
	{
		Kamcord.CallAdjustAndroidWhitelist();
		SetLoggingEnabled(Kamcord.loggingEnabled_);
		if (kamcordJavaClass() == null)
		{
			Debug.Log("Kamcord: Java class not accessible from C#.");
		}
		else
		{
			Init(kamcordInstance.developerKey, kamcordInstance.developerSecret, kamcordInstance.appName, kamcordInstance.videoQuality);
		}
	}

	public override void SetAudioSettings(int sampleRate, int numChannels)
	{
		kamcordJavaClass().CallStatic("setAudioSettings", sampleRate, numChannels);
	}

	public override void WriteAudioData(float[] data, int length)
	{
		kamcordJavaClass().CallStatic("writeAudioData", data, length);
	}

	private void InitializeRenderCamera(string type)
	{
		if ((type.Equals("Pre") || type.Equals("Post")) && GameObject.Find("kamcord" + type + "Camera") == null)
		{
			GameObject gameObject = new GameObject();
			Camera camera = (Camera)gameObject.AddComponent("Camera");
			camera.name = "kamcord" + type + "Camera";
			camera.clearFlags = CameraClearFlags.Nothing;
			camera.cullingMask = 0;
			if (type.Equals("Pre"))
			{
				camera.depth = float.MinValue;
			}
			else
			{
				camera.depth = float.MaxValue;
			}
			camera.gameObject.AddComponent("KamcordAndroid" + type + "Render");
			gameObject.SetActive(value: true);
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
		}
	}

	public static void SetRenderCameraEnabled(string type, bool flag)
	{
		if (type.Equals("Pre") || type.Equals("Post"))
		{
			GameObject gameObject = GameObject.Find("kamcord" + type + "Camera");
			if (gameObject != null)
			{
				gameObject.SetActive(flag);
			}
		}
	}

	public static int getSDKVersion()
	{
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("android.os.Build$VERSION");
		return androidJavaClass.GetStatic<int>("SDK_INT");
	}
}
public class MiniJSON
{
	private const int TOKEN_NONE = 0;

	private const int TOKEN_CURLY_OPEN = 1;

	private const int TOKEN_CURLY_CLOSE = 2;

	private const int TOKEN_SQUARED_OPEN = 3;

	private const int TOKEN_SQUARED_CLOSE = 4;

	private const int TOKEN_COLON = 5;

	private const int TOKEN_COMMA = 6;

	private const int TOKEN_STRING = 7;

	private const int TOKEN_NUMBER = 8;

	private const int TOKEN_TRUE = 9;

	private const int TOKEN_FALSE = 10;

	private const int TOKEN_NULL = 11;

	private const int BUILDER_CAPACITY = 2000;

	protected static int lastErrorIndex = -1;

	protected static string lastDecode = string.Empty;

	public static object jsonDecode(string json)
	{
		lastDecode = json;
		if (json != null)
		{
			char[] json2 = json.ToCharArray();
			int index = 0;
			bool success = true;
			object result = parseValue(json2, ref index, ref success);
			if (success)
			{
				lastErrorIndex = -1;
			}
			else
			{
				lastErrorIndex = index;
			}
			return result;
		}
		return null;
	}

	public static string jsonEncode(object json)
	{
		StringBuilder stringBuilder = new StringBuilder(2000);
		return (!serializeValue(json, stringBuilder)) ? null : stringBuilder.ToString();
	}

	public static bool lastDecodeSuccessful()
	{
		return lastErrorIndex == -1;
	}

	public static int getLastErrorIndex()
	{
		return lastErrorIndex;
	}

	public static string getLastErrorSnippet()
	{
		if (lastErrorIndex == -1)
		{
			return string.Empty;
		}
		int num = lastErrorIndex - 5;
		int num2 = lastErrorIndex + 15;
		if (num < 0)
		{
			num = 0;
		}
		if (num2 >= lastDecode.Length)
		{
			num2 = lastDecode.Length - 1;
		}
		return lastDecode.Substring(num, num2 - num + 1);
	}

	protected static Hashtable parseObject(char[] json, ref int index)
	{
		Hashtable hashtable = new Hashtable();
		nextToken(json, ref index);
		bool flag = false;
		while (!flag)
		{
			switch (lookAhead(json, index))
			{
			case 0:
				return null;
			case 6:
				nextToken(json, ref index);
				continue;
			case 2:
				nextToken(json, ref index);
				return hashtable;
			}
			string text = parseString(json, ref index);
			if (text == null)
			{
				return null;
			}
			int num = nextToken(json, ref index);
			if (num != 5)
			{
				return null;
			}
			bool success = true;
			object value = parseValue(json, ref index, ref success);
			if (!success)
			{
				return null;
			}
			hashtable[text] = value;
		}
		return hashtable;
	}

	protected static ArrayList parseArray(char[] json, ref int index)
	{
		ArrayList arrayList = new ArrayList();
		nextToken(json, ref index);
		bool flag = false;
		while (!flag)
		{
			switch (lookAhead(json, index))
			{
			case 0:
				return null;
			case 6:
				nextToken(json, ref index);
				continue;
			case 4:
				break;
			default:
			{
				bool success = true;
				object value = parseValue(json, ref index, ref success);
				if (!success)
				{
					return null;
				}
				arrayList.Add(value);
				continue;
			}
			}
			nextToken(json, ref index);
			break;
		}
		return arrayList;
	}

	protected static object parseValue(char[] json, ref int index, ref bool success)
	{
		switch (lookAhead(json, index))
		{
		case 7:
			return parseString(json, ref index);
		case 8:
			return parseNumber(json, ref index);
		case 1:
			return parseObject(json, ref index);
		case 3:
			return parseArray(json, ref index);
		case 9:
			nextToken(json, ref index);
			return bool.Parse("TRUE");
		case 10:
			nextToken(json, ref index);
			return bool.Parse("FALSE");
		case 11:
			nextToken(json, ref index);
			return null;
		default:
			success = false;
			return null;
		}
	}

	protected static string parseString(char[] json, ref int index)
	{
		string text = string.Empty;
		eatWhitespace(json, ref index);
		char c = json[index++];
		bool flag = false;
		while (!flag && index != json.Length)
		{
			c = json[index++];
			switch (c)
			{
			case '"':
				flag = true;
				break;
			case '\\':
				if (index != json.Length)
				{
					switch (json[index++])
					{
					case '"':
						text += '"';
						continue;
					case '\\':
						text += '\\';
						continue;
					case '/':
						text += '/';
						continue;
					case 'b':
						text += '\b';
						continue;
					case 'f':
						text += '\f';
						continue;
					case 'n':
						text += '\n';
						continue;
					case 'r':
						text += '\r';
						continue;
					case 't':
						text += '\t';
						continue;
					case 'u':
						break;
					default:
						continue;
					}
					int num = json.Length - index;
					if (num >= 4)
					{
						char[] array = new char[4];
						Array.Copy(json, index, array, 0, 4);
						text = text + "&#x" + new string(array) + ";";
						index += 4;
						continue;
					}
				}
				break;
			default:
				text += c;
				continue;
			}
			break;
		}
		if (!flag)
		{
			return null;
		}
		return text;
	}

	protected static double parseNumber(char[] json, ref int index)
	{
		eatWhitespace(json, ref index);
		int lastIndexOfNumber = getLastIndexOfNumber(json, index);
		int num = lastIndexOfNumber - index + 1;
		char[] array = new char[num];
		Array.Copy(json, index, array, 0, num);
		index = lastIndexOfNumber + 1;
		return double.Parse(new string(array));
	}

	protected static int getLastIndexOfNumber(char[] json, int index)
	{
		int i;
		for (i = index; i < json.Length && "0123456789+-.eE".IndexOf(json[i]) != -1; i++)
		{
		}
		return i - 1;
	}

	protected static void eatWhitespace(char[] json, ref int index)
	{
		while (index < json.Length && " \t\n\r".IndexOf(json[index]) != -1)
		{
			index++;
		}
	}

	protected static int lookAhead(char[] json, int index)
	{
		int index2 = index;
		return nextToken(json, ref index2);
	}

	protected static int nextToken(char[] json, ref int index)
	{
		eatWhitespace(json, ref index);
		if (index == json.Length)
		{
			return 0;
		}
		char c = json[index];
		index++;
		switch (c)
		{
		case '{':
			return 1;
		case '}':
			return 2;
		case '[':
			return 3;
		case ']':
			return 4;
		case ',':
			return 6;
		case '"':
			return 7;
		case '-':
		case '0':
		case '1':
		case '2':
		case '3':
		case '4':
		case '5':
		case '6':
		case '7':
		case '8':
		case '9':
			return 8;
		case ':':
			return 5;
		default:
		{
			index--;
			int num = json.Length - index;
			if (num >= 5 && json[index] == 'f' && json[index + 1] == 'a' && json[index + 2] == 'l' && json[index + 3] == 's' && json[index + 4] == 'e')
			{
				index += 5;
				return 10;
			}
			if (num >= 4 && json[index] == 't' && json[index + 1] == 'r' && json[index + 2] == 'u' && json[index + 3] == 'e')
			{
				index += 4;
				return 9;
			}
			if (num >= 4 && json[index] == 'n' && json[index + 1] == 'u' && json[index + 2] == 'l' && json[index + 3] == 'l')
			{
				index += 4;
				return 11;
			}
			return 0;
		}
		}
	}

	protected static bool serializeObjectOrArray(object objectOrArray, StringBuilder builder)
	{
		if (objectOrArray is Hashtable)
		{
			return serializeObject((Hashtable)objectOrArray, builder);
		}
		if (objectOrArray is ArrayList)
		{
			return serializeArray((ArrayList)objectOrArray, builder);
		}
		return false;
	}

	protected static bool serializeObject(Hashtable anObject, StringBuilder builder)
	{
		builder.Append("{");
		IDictionaryEnumerator enumerator = anObject.GetEnumerator();
		bool flag = true;
		while (enumerator.MoveNext())
		{
			string aString = enumerator.Key.ToString();
			object value = enumerator.Value;
			if (!flag)
			{
				builder.Append(", ");
			}
			serializeString(aString, builder);
			builder.Append(":");
			if (!serializeValue(value, builder))
			{
				return false;
			}
			flag = false;
		}
		builder.Append("}");
		return true;
	}

	protected static bool serializeDictionary(Dictionary<string, string> dict, StringBuilder builder)
	{
		builder.Append("{");
		bool flag = true;
		foreach (KeyValuePair<string, string> item in dict)
		{
			if (!flag)
			{
				builder.Append(", ");
			}
			serializeString(item.Key, builder);
			builder.Append(":");
			serializeString(item.Value, builder);
			flag = false;
		}
		builder.Append("}");
		return true;
	}

	protected static bool serializeArray(ArrayList anArray, StringBuilder builder)
	{
		builder.Append("[");
		bool flag = true;
		for (int i = 0; i < anArray.Count; i++)
		{
			object value = anArray[i];
			if (!flag)
			{
				builder.Append(", ");
			}
			if (!serializeValue(value, builder))
			{
				return false;
			}
			flag = false;
		}
		builder.Append("]");
		return true;
	}

	protected static bool serializeValue(object value, StringBuilder builder)
	{
		if (value == null)
		{
			builder.Append("null");
		}
		else if (value.GetType().IsArray)
		{
			serializeArray(new ArrayList((ICollection)value), builder);
		}
		else if (value is string)
		{
			serializeString((string)value, builder);
		}
		else if (value is char)
		{
			serializeString(Convert.ToString((char)value), builder);
		}
		else if (value is Hashtable)
		{
			serializeObject((Hashtable)value, builder);
		}
		else if (value is Dictionary<string, string>)
		{
			serializeDictionary((Dictionary<string, string>)value, builder);
		}
		else if (value is ArrayList)
		{
			serializeArray((ArrayList)value, builder);
		}
		else if (value is bool && (bool)value)
		{
			builder.Append("true");
		}
		else if (value is bool && !(bool)value)
		{
			builder.Append("false");
		}
		else
		{
			if (!value.GetType().IsPrimitive)
			{
				return false;
			}
			serializeNumber(Convert.ToDouble(value), builder);
		}
		return true;
	}

	protected static void serializeString(string aString, StringBuilder builder)
	{
		builder.Append("\"");
		char[] array = aString.ToCharArray();
		foreach (char c in array)
		{
			switch (c)
			{
			case '"':
				builder.Append("\\\"");
				continue;
			case '\\':
				builder.Append("\\\\");
				continue;
			case '\b':
				builder.Append("\\b");
				continue;
			case '\f':
				builder.Append("\\f");
				continue;
			case '\n':
				builder.Append("\\n");
				continue;
			case '\r':
				builder.Append("\\r");
				continue;
			case '\t':
				builder.Append("\\t");
				continue;
			}
			int num = Convert.ToInt32(c);
			if (num >= 32 && num <= 126)
			{
				builder.Append(c);
			}
			else
			{
				builder.Append("\\u" + Convert.ToString(num, 16).PadLeft(4, '0'));
			}
		}
		builder.Append("\"");
	}

	protected static void serializeNumber(double number, StringBuilder builder)
	{
		builder.Append(Convert.ToString(number));
	}
}
public static class MiniJsonExtensions
{
	public static string toJson(this Hashtable obj)
	{
		return MiniJSON.jsonEncode(obj);
	}

	public static string toJson(this Dictionary<string, string> obj)
	{
		return MiniJSON.jsonEncode(obj);
	}

	public static ArrayList arrayListFromJson(this string json)
	{
		return MiniJSON.jsonDecode(json) as ArrayList;
	}

	public static Hashtable hashtableFromJson(this string json)
	{
		return MiniJSON.jsonDecode(json) as Hashtable;
	}
}
public class StoreKitEventListener : MonoBehaviour
{
}
public class StoreKitManager : MonoBehaviour
{
}
public class StoreKitGUIManager : MonoBehaviour
{
}
namespace Uniject
{
	public interface ILevelLoadListener
	{
		void registerListener(Action action);
	}
	public interface ILogger
	{
		string prefix { get; set; }

		void Log(string message);

		void Log(string message, params object[] formatArgs);

		void LogWarning(string message, params object[] formatArgs);

		void LogError(string message, params object[] formatArgs);
	}
	public interface IResourceLoader
	{
		TextReader openTextFile(string path);
	}
	public interface IStorage
	{
		int GetInt(string key, int defaultValue);

		void SetInt(string key, int value);

		string GetString(string key, string defaultValue);

		void SetString(string key, string val);
	}
	public interface IHTTPRequest
	{
		Dictionary<string, string> responseHeaders { get; }

		byte[] bytes { get; }

		string contentString { get; }

		string error { get; }
	}
	public interface IURLFetcher
	{
		object doGet(string url, Dictionary<string, string> headers);

		object doPost(string url, Dictionary<string, string> parameters);

		IHTTPRequest getResponse();
	}
	public interface IUtil
	{
		RuntimePlatform Platform { get; }

		bool IsEditor { get; }

		string persistentDataPath { get; }

		DateTime currentTime { get; }

		string DeviceModel { get; }

		string DeviceName { get; }

		DeviceType DeviceType { get; }

		string OperatingSystem { get; }

		T[] getAnyComponentsOfType<T>() where T : class;

		string loadedLevelName();

		object InitiateCoroutine(IEnumerator start);

		object getWaitForSeconds(int seconds);

		void InitiateCoroutine(IEnumerator start, int delayInSeconds);

		void RunOnThreadPool(Action runnable);

		void RunOnMainThread(Action runnable);
	}
}
namespace Uniject.Impl
{
	public class UnityLevelLoadListener : MonoBehaviour, ILevelLoadListener
	{
		private Action listener;

		public void registerListener(Action action)
		{
			listener = action;
		}

		private void Start()
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}

		private void OnLevelWasLoaded(int level)
		{
			if (listener != null)
			{
				listener();
			}
		}
	}
	public class UnityLogger : ILogger
	{
		public string prefix { get; set; }

		public void LogWarning(string message, params object[] formatArgs)
		{
			Debug.LogWarning(string.Format(message, formatArgs));
		}

		public void Log(string message)
		{
			Debug.Log(formatMessageWithPrefix(message));
		}

		public void Log(string message, object[] args)
		{
			Log(safeFormat(message, args));
		}

		public void LogError(string message, params object[] formatArgs)
		{
			Debug.LogError(formatMessageWithPrefix(safeFormat(message, formatArgs)));
		}

		private string safeFormat(string message, params object[] formatArgs)
		{
			try
			{
				return string.Format(message, formatArgs);
			}
			catch (FormatException ex)
			{
				Log(ex.Data.ToString());
				return message;
			}
		}

		private string formatMessageWithPrefix(string message)
		{
			if (prefix == null)
			{
				return message;
			}
			return safeFormat("{0}: {1}", prefix, message);
		}
	}
	public class UnityPlayerPrefsStorage : IStorage
	{
		public int GetInt(string key, int defaultValue)
		{
			return PlayerPrefs.GetInt(key, defaultValue);
		}

		public void SetInt(string key, int value)
		{
			PlayerPrefs.SetInt(key, value);
		}

		public string GetString(string key, string defaultValue)
		{
			return PlayerPrefs.GetString(key, defaultValue);
		}

		public void SetString(string key, string val)
		{
			PlayerPrefs.SetString(key, val);
		}
	}
	public class UnityResourceLoader : IResourceLoader
	{
		public TextReader openTextFile(string path)
		{
			return new StringReader(((TextAsset)Resources.Load(path, typeof(TextAsset))).text);
		}
	}
}
public class UnityUtil : MonoBehaviour, IUtil
{
	private static List<RuntimePlatform> PCControlledPlatforms = new List<RuntimePlatform>
	{
		RuntimePlatform.FlashPlayer,
		RuntimePlatform.LinuxPlayer,
		RuntimePlatform.NaCl,
		RuntimePlatform.OSXDashboardPlayer,
		RuntimePlatform.OSXEditor,
		RuntimePlatform.OSXPlayer,
		RuntimePlatform.OSXWebPlayer,
		RuntimePlatform.WindowsEditor,
		RuntimePlatform.WindowsPlayer,
		RuntimePlatform.WindowsWebPlayer
	};

	private Queue<Action> mainThreadTasks = new Queue<Action>();

	public DateTime currentTime => DateTime.Now;

	public string persistentDataPath => Application.persistentDataPath;

	public RuntimePlatform Platform => Application.platform;

	public bool IsEditor => Application.isEditor;

	public string DeviceModel => SystemInfo.deviceModel;

	public string DeviceName => SystemInfo.deviceName;

	public DeviceType DeviceType => SystemInfo.deviceType;

	public string OperatingSystem => SystemInfo.operatingSystem;

	object IUtil.InitiateCoroutine(IEnumerator start)
	{
		return StartCoroutine(start);
	}

	void IUtil.InitiateCoroutine(IEnumerator start, int delay)
	{
		delayedCoroutine(start, delay);
	}

	public T[] getAnyComponentsOfType<T>() where T : class
	{
		GameObject[] array = (GameObject[])UnityEngine.Object.FindObjectsOfType(typeof(GameObject));
		List<T> list = new List<T>();
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			MonoBehaviour[] components = gameObject.GetComponents<MonoBehaviour>();
			foreach (MonoBehaviour monoBehaviour in components)
			{
				if (monoBehaviour is T)
				{
					list.Add(monoBehaviour as T);
				}
			}
		}
		return list.ToArray();
	}

	private void Start()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	public string loadedLevelName()
	{
		return Application.loadedLevelName;
	}

	public static T findInstanceOfType<T>() where T : MonoBehaviour
	{
		return (T)UnityEngine.Object.FindObjectOfType(typeof(T));
	}

	public static T loadResourceInstanceOfType<T>() where T : MonoBehaviour
	{
		return ((GameObject)UnityEngine.Object.Instantiate(Resources.Load(typeof(T).ToString()))).GetComponent<T>();
	}

	public static bool pcPlatform()
	{
		return PCControlledPlatforms.Contains(Application.platform);
	}

	public static void DebugLog(string message, params object[] args)
	{
		try
		{
			Debug.Log($"com.ballatergames.debug - {string.Format(message, args)}");
		}
		catch (ArgumentNullException message2)
		{
			Debug.Log(message2);
		}
		catch (FormatException message3)
		{
			Debug.Log(message3);
		}
	}

	public static float[] getFrustumBoundaries(Camera camera)
	{
		Plane[] array = GeometryUtility.CalculateFrustumPlanes(camera);
		return new float[6]
		{
			(-array[0].normal * array[0].distance).x,
			(-array[1].normal * array[1].distance).x,
			(-array[5].normal * array[5].distance).y,
			(-array[4].normal * array[4].distance).y,
			(-array[2].normal * array[2].distance).z,
			(-array[3].normal * array[3].distance).z
		};
	}

	private IEnumerator delayedCoroutine(IEnumerator coroutine, int delay)
	{
		yield return new WaitForSeconds(delay);
		StartCoroutine(coroutine);
	}

	public void RunOnThreadPool(Action runnable)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			runnable();
		});
	}

	private void Update()
	{
		while (mainThreadTasks.Count > 0)
		{
			Action action;
			lock (mainThreadTasks)
			{
				action = mainThreadTasks.Dequeue();
			}
			action();
		}
	}

	public void RunOnMainThread(Action runnable)
	{
		lock (mainThreadTasks)
		{
			mainThreadTasks.Enqueue(runnable);
		}
	}

	public object getWaitForSeconds(int seconds)
	{
		return new WaitForSeconds(seconds);
	}
}
namespace Unibill.Impl
{
	public class AmazonAppStoreBillingService : IBillingService
	{
		private IBillingServiceCallback callback;

		private ProductIdRemapper remapper;

		private UnibillConfiguration db;

		private ILogger logger;

		private IRawAmazonAppStoreBillingInterface amazon;

		private HashSet<string> unknownAmazonProducts = new HashSet<string>();

		private TransactionDatabase tDb;

		private bool finishedSetup;

		public AmazonAppStoreBillingService(IRawAmazonAppStoreBillingInterface amazon, ProductIdRemapper remapper, UnibillConfiguration db, TransactionDatabase tDb, ILogger logger)
		{
			this.remapper = remapper;
			this.db = db;
			this.logger = logger;
			logger.prefix = "UnibillAmazonBillingService";
			this.amazon = amazon;
			this.tDb = tDb;
		}

		public void initialise(IBillingServiceCallback biller)
		{
			callback = biller;
			amazon.initialise(this);
			amazon.initiateItemDataRequest(remapper.getAllPlatformSpecificProductIds());
		}

		public void purchase(string item, string developerPayload)
		{
			if (unknownAmazonProducts.Contains(item))
			{
				callback.logError(UnibillError.AMAZONAPPSTORE_ATTEMPTING_TO_PURCHASE_PRODUCT_NOT_RETURNED_BY_AMAZON, item);
				callback.onPurchaseFailedEvent(item);
			}
			else
			{
				amazon.initiatePurchaseRequest(item);
			}
		}

		public void restoreTransactions()
		{
			amazon.restoreTransactions();
		}

		public void onSDKAvailable(string isSandbox)
		{
			bool flag = bool.Parse(isSandbox);
			logger.Log("Running against {0} Amazon environment", (!flag) ? "PRODUCTION" : "SANDBOX");
		}

		public void onGetItemDataFailed()
		{
			callback.logError(UnibillError.AMAZONAPPSTORE_GETITEMDATAREQUEST_FAILED);
			callback.onSetupComplete(successful: true);
		}

		public void onProductListReceived(string productListString)
		{
			Dictionary<string, object> dic = (Dictionary<string, object>)MiniJSON.jsonDecode(productListString);
			onUserIdRetrieved(dic.getString("userId", string.Empty));
			Dictionary<string, object> hash = dic.getHash("products");
			if (hash.Count == 0)
			{
				callback.logError(UnibillError.AMAZONAPPSTORE_GETITEMDATAREQUEST_NO_PRODUCTS_RETURNED);
				callback.onSetupComplete(successful: false);
				return;
			}
			HashSet<PurchasableItem> hashSet = new HashSet<PurchasableItem>();
			foreach (string key in hash.Keys)
			{
				PurchasableItem purchasableItemFromPlatformSpecificId = remapper.getPurchasableItemFromPlatformSpecificId(key.ToString());
				Dictionary<string, object> dictionary = (Dictionary<string, object>)hash[key];
				PurchasableItem.Writer.setAvailable(purchasableItemFromPlatformSpecificId, available: true);
				PurchasableItem.Writer.setLocalizedPrice(purchasableItemFromPlatformSpecificId, dictionary["price"].ToString());
				PurchasableItem.Writer.setLocalizedTitle(purchasableItemFromPlatformSpecificId, (string)dictionary["localizedTitle"]);
				PurchasableItem.Writer.setLocalizedDescription(purchasableItemFromPlatformSpecificId, (string)dictionary["localizedDescription"]);
				PurchasableItem.Writer.setISOCurrencySymbol(purchasableItemFromPlatformSpecificId, dictionary.getString("isoCurrencyCode", string.Empty));
				PurchasableItem.Writer.setPriceInLocalCurrency(purchasableItemFromPlatformSpecificId, decimal.Parse(dictionary.getString("priceDecimal", string.Empty)));
				hashSet.Add(purchasableItemFromPlatformSpecificId);
			}
			HashSet<PurchasableItem> hashSet2 = new HashSet<PurchasableItem>(db.AllPurchasableItems);
			hashSet2.ExceptWith(hashSet);
			if (hashSet2.Count <= 0)
			{
				return;
			}
			foreach (PurchasableItem item in hashSet2)
			{
				unknownAmazonProducts.Add(remapper.mapItemIdToPlatformSpecificId(item));
				callback.logError(UnibillError.AMAZONAPPSTORE_GETITEMDATAREQUEST_MISSING_PRODUCT, item.Id, remapper.mapItemIdToPlatformSpecificId(item));
			}
		}

		private void onUserIdRetrieved(string userId)
		{
			tDb.UserId = userId;
		}

		public void onTransactionsRestored(string successString)
		{
			if (bool.Parse(successString))
			{
				callback.onTransactionsRestoredSuccess();
			}
			else
			{
				callback.onTransactionsRestoredFail(string.Empty);
			}
		}

		public void onPurchaseFailed(string item)
		{
			callback.onPurchaseFailedEvent(item);
		}

		public void onPurchaseCancelled(string item)
		{
			callback.onPurchaseCancelledEvent(item);
		}

		public void onPurchaseSucceeded(string json)
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJSON.jsonDecode(json);
			string platformSpecificId = (string)dictionary["productId"];
			string receipt = (string)dictionary["purchaseToken"];
			callback.onPurchaseSucceeded(platformSpecificId, receipt);
		}

		public void onPurchaseUpdateFailed()
		{
			logger.LogWarning("AmazonAppStoreBillingService: onPurchaseUpdate() failed.");
		}

		public void onPurchaseUpdateSuccess(string json)
		{
			Dictionary<string, object> dic = (Dictionary<string, object>)MiniJSON.jsonDecode(json);
			List<object> list = dic.get<List<object>>("restored");
			foreach (Dictionary<string, object> item2 in list)
			{
				callback.onPurchaseSucceeded(item2.getString("sku", string.Empty), item2.getString("receipt", string.Empty));
			}
			List<object> list2 = dic.get<List<object>>("revoked");
			foreach (string item3 in list2)
			{
				callback.onPurchaseRefundedEvent(item3);
			}
			if (!finishedSetup)
			{
				finishedSetup = true;
				callback.onSetupComplete(successful: true);
			}
		}

		public bool hasReceipt(string forItem)
		{
			return false;
		}

		public string getReceipt(string forItem)
		{
			throw new NotImplementedException();
		}
	}
}
[AddComponentMenu("")]
public class AmazonAppStoreCallbackMonoBehaviour : MonoBehaviour
{
	private AmazonAppStoreBillingService amazon;

	public void Start()
	{
		base.gameObject.name = GetType().ToString();
		UnityEngine.Object.DontDestroyOnLoad(this);
	}

	public void initialise(AmazonAppStoreBillingService amazon)
	{
		this.amazon = amazon;
	}

	public void onSDKAvailable(string isSandboxEnvironment)
	{
		amazon.onSDKAvailable(isSandboxEnvironment);
	}

	public void onGetItemDataFailed(string empty)
	{
		amazon.onGetItemDataFailed();
	}

	public void onProductListReceived(string productCSVString)
	{
		amazon.onProductListReceived(productCSVString);
	}

	public void onPurchaseFailed(string item)
	{
		amazon.onPurchaseFailed(item);
	}

	public void onPurchaseSucceeded(string item)
	{
		amazon.onPurchaseSucceeded(item);
	}

	public void onTransactionsRestored(string success)
	{
		amazon.onTransactionsRestored(success);
	}

	public void onPurchaseUpdateFailed(string empty)
	{
		amazon.onPurchaseUpdateFailed();
	}

	public void onPurchaseUpdateSuccess(string data)
	{
		amazon.onPurchaseUpdateSuccess(data);
	}
}
namespace Unibill.Impl
{
	public interface IRawAmazonAppStoreBillingInterface
	{
		void initialise(AmazonAppStoreBillingService amazon);

		void initiateItemDataRequest(string[] productIds);

		void initiatePurchaseRequest(string productId);

		void restoreTransactions();
	}
	public class RawAmazonAppStoreBillingInterface : IRawAmazonAppStoreBillingInterface
	{
		private AndroidJavaObject amazon;

		public RawAmazonAppStoreBillingInterface(UnibillConfiguration config)
		{
			if (config.CurrentPlatform == BillingPlatform.AmazonAppstore && config.AmazonSandboxEnabled)
			{
				string text = ((TextAsset)Resources.Load("amazon.sdktester.json")).text;
				File.WriteAllText("/sdcard/amazon.sdktester.json", text);
			}
			using AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.outlinegames.unibillAmazon.Unibill");
			amazon = androidJavaClass.CallStatic<AndroidJavaObject>("instance", new object[0]);
		}

		public void initialise(AmazonAppStoreBillingService amazon)
		{
			new GameObject().AddComponent<AmazonAppStoreCallbackMonoBehaviour>().initialise(amazon);
		}

		public void initiateItemDataRequest(string[] productIds)
		{
			IntPtr methodID = AndroidJNI.GetMethodID(amazon.GetRawClass(), "initiateItemDataRequest", "([Ljava/lang/String;)V");
			AndroidJNI.CallVoidMethod(amazon.GetRawObject(), methodID, AndroidJNIHelper.CreateJNIArgArray(new object[1] { productIds }));
		}

		public void initiatePurchaseRequest(string productId)
		{
			amazon.Call("initiatePurchaseRequest", productId);
		}

		public void restoreTransactions()
		{
			amazon.Call("restoreTransactions");
		}
	}
	public class AppleAppStoreBillingService : IBillingService
	{
		private IBillingServiceCallback biller;

		private ProductIdRemapper remapper;

		private HashSet<PurchasableItem> products;

		private HashSet<string> productsNotReturnedByStorekit = new HashSet<string>();

		private string appReceipt;

		private ILogger logger;

		private bool restoreInProgress;

		public IStoreKitPlugin storekit { get; private set; }

		public AppleAppStoreBillingService(UnibillConfiguration db, ProductIdRemapper mapper, IStoreKitPlugin storekit, ILogger logger)
		{
			this.storekit = storekit;
			remapper = mapper;
			this.logger = logger;
			storekit.initialise(this);
			products = new HashSet<PurchasableItem>(db.AllPurchasableItems);
		}

		public void initialise(IBillingServiceCallback biller)
		{
			this.biller = biller;
			if (storekit.storeKitPaymentsAvailable())
			{
				string[] allPlatformSpecificProductIds = remapper.getAllPlatformSpecificProductIds();
				storekit.storeKitRequestProductData(string.Join(",", allPlatformSpecificProductIds), allPlatformSpecificProductIds);
			}
			else
			{
				biller.logError(UnibillError.STOREKIT_BILLING_UNAVAILABLE);
				biller.onSetupComplete(successful: false);
			}
		}

		public void purchase(string item, string developerPayload)
		{
			if (productsNotReturnedByStorekit.Contains(item))
			{
				biller.logError(UnibillError.STOREKIT_ATTEMPTING_TO_PURCHASE_PRODUCT_NOT_RETURNED_BY_STOREKIT, item);
				biller.onPurchaseFailedEvent(item);
			}
			else
			{
				storekit.storeKitPurchaseProduct(item);
			}
		}

		public void restoreTransactions()
		{
			restoreInProgress = true;
			storekit.storeKitRestoreTransactions();
		}

		public void onProductListReceived(string productListString)
		{
			if (productListString.Length == 0)
			{
				biller.logError(UnibillError.STOREKIT_RETURNED_NO_PRODUCTS);
				biller.onSetupComplete(successful: false);
				return;
			}
			Dictionary<string, object> dic = (Dictionary<string, object>)MiniJSON.jsonDecode(productListString);
			appReceipt = dic.getString("appReceipt", string.Empty);
			Dictionary<string, object> hash = dic.getHash("products");
			HashSet<PurchasableItem> hashSet = new HashSet<PurchasableItem>();
			foreach (string key in hash.Keys)
			{
				if (!remapper.canMapProductSpecificId(key.ToString()))
				{
					biller.logError(UnibillError.UNIBILL_UNKNOWN_PRODUCTID, key.ToString());
					continue;
				}
				PurchasableItem purchasableItemFromPlatformSpecificId = remapper.getPurchasableItemFromPlatformSpecificId(key.ToString());
				Dictionary<string, object> dictionary = (Dictionary<string, object>)hash[key];
				PurchasableItem.Writer.setAvailable(purchasableItemFromPlatformSpecificId, available: true);
				PurchasableItem.Writer.setLocalizedPrice(purchasableItemFromPlatformSpecificId, dictionary["price"].ToString());
				PurchasableItem.Writer.setLocalizedTitle(purchasableItemFromPlatformSpecificId, dictionary["localizedTitle"].ToString());
				PurchasableItem.Writer.setLocalizedDescription(purchasableItemFromPlatformSpecificId, dictionary["localizedDescription"].ToString());
				if (dictionary.ContainsKey("isoCurrencyCode"))
				{
					PurchasableItem.Writer.setISOCurrencySymbol(purchasableItemFromPlatformSpecificId, dictionary["isoCurrencyCode"].ToString());
				}
				if (dictionary.ContainsKey("priceDecimal"))
				{
					PurchasableItem.Writer.setPriceInLocalCurrency(purchasableItemFromPlatformSpecificId, decimal.Parse(dictionary["priceDecimal"].ToString()));
				}
				hashSet.Add(purchasableItemFromPlatformSpecificId);
			}
			HashSet<PurchasableItem> hashSet2 = new HashSet<PurchasableItem>(products);
			hashSet2.ExceptWith(hashSet);
			if (hashSet2.Count > 0)
			{
				foreach (PurchasableItem item in hashSet2)
				{
					biller.logError(UnibillError.STOREKIT_REQUESTPRODUCTS_MISSING_PRODUCT, item.Id, remapper.mapItemIdToPlatformSpecificId(item));
				}
			}
			productsNotReturnedByStorekit = new HashSet<string>(hashSet2.Select((PurchasableItem x) => remapper.mapItemIdToPlatformSpecificId(x)));
			storekit.addTransactionObserver();
			if (appReceipt != null)
			{
				biller.setAppReceipt(appReceipt);
			}
			biller.onSetupComplete(successful: true);
		}

		public void onPurchaseSucceeded(string data)
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJSON.jsonDecode(data);
			string value = (string)dictionary["receipt"];
			if (!string.IsNullOrEmpty(value))
			{
				appReceipt = value;
			}
			string text = (string)dictionary["productId"];
			if (restoreInProgress && remapper.canMapProductSpecificId(text) && remapper.getPurchasableItemFromPlatformSpecificId(text).PurchaseType == PurchaseType.Consumable)
			{
				logger.Log("Ignoring restore of consumable: " + text);
			}
			else
			{
				biller.onPurchaseSucceeded(text, appReceipt);
			}
		}

		public void onPurchaseCancelled(string productId)
		{
			biller.onPurchaseCancelledEvent(productId);
		}

		public void onPurchaseFailed(string productId)
		{
			biller.onPurchaseFailedEvent(productId);
		}

		public void onPurchaseDeferred(string productId)
		{
			biller.onPurchaseDeferredEvent(productId);
		}

		public void onTransactionsRestoredSuccess()
		{
			restoreInProgress = false;
			biller.onTransactionsRestoredSuccess();
		}

		public void onTransactionsRestoredFail(string error)
		{
			restoreInProgress = false;
			biller.onTransactionsRestoredFail(error);
		}

		public void onFailedToRetrieveProductList()
		{
			biller.logError(UnibillError.STOREKIT_FAILED_TO_RETRIEVE_PRODUCT_DATA);
			biller.onSetupComplete(successful: true);
		}

		public bool hasReceipt(string forItem)
		{
			return !string.IsNullOrEmpty(appReceipt);
		}

		public string getReceipt(string forItem)
		{
			return appReceipt;
		}
	}
}
[AddComponentMenu("")]
public class AppleAppStoreCallbackMonoBehaviour : MonoBehaviour
{
	private AppleAppStoreBillingService callback;

	public void Awake()
	{
		base.gameObject.name = GetType().ToString();
		UnityEngine.Object.DontDestroyOnLoad(this);
	}

	public void initialise(AppleAppStoreBillingService callback)
	{
		this.callback = callback;
	}

	public void onProductListReceived(string productList)
	{
		callback.onProductListReceived(productList);
	}

	public void onProductPurchaseSuccess(string productId)
	{
		callback.onPurchaseSucceeded(productId);
	}

	public void onProductPurchaseCancelled(string productId)
	{
		callback.onPurchaseCancelled(productId);
	}

	public void onProductPurchaseFailed(string productId)
	{
		callback.onPurchaseFailed(productId);
	}

	public void onProductPurchaseDeferred(string productId)
	{
		callback.onPurchaseDeferred(productId);
	}

	public void onTransactionsRestoredSuccess(string empty)
	{
		callback.onTransactionsRestoredSuccess();
	}

	public void onTransactionsRestoredFail(string error)
	{
		callback.onTransactionsRestoredFail(error);
	}

	public void onFailedToRetrieveProductList(string nop)
	{
		callback.onFailedToRetrieveProductList();
	}
}
namespace Unibill.Impl
{
	public interface IStoreKitPlugin
	{
		void initialise(AppleAppStoreBillingService callback);

		bool storeKitPaymentsAvailable();

		void storeKitRequestProductData(string productIdentifiers, string[] productIds);

		void storeKitPurchaseProduct(string productId);

		void storeKitRestoreTransactions();

		void addTransactionObserver();
	}
	public class OSXStoreKitPluginImpl : IStoreKitPlugin
	{
		private static AppleAppStoreBillingService callback;

		public void initialise(AppleAppStoreBillingService callback)
		{
			OSXStoreKitPluginImpl.callback = callback;
		}

		public bool storeKitPaymentsAvailable()
		{
			throw new NotImplementedException();
		}

		public void storeKitRequestProductData(string productIdentifiers, string[] productIds)
		{
			throw new NotImplementedException();
		}

		public void storeKitPurchaseProduct(string productId)
		{
			throw new NotImplementedException();
		}

		public void storeKitRestoreTransactions()
		{
			throw new NotImplementedException();
		}

		public void addTransactionObserver()
		{
		}

		public static void UnibillSendMessage(string method, string argument)
		{
			switch (method)
			{
			case "onProductListReceived":
				onProductListReceived(argument);
				break;
			case "onProductPurchaseSuccess":
				onProductPurchaseSuccess(argument);
				break;
			case "onProductPurchaseCancelled":
				onProductPurchaseCancelled(argument);
				break;
			case "onProductPurchaseFailed":
				onProductPurchaseFailed(argument);
				break;
			case "onTransactionsRestoredSuccess":
				onTransactionsRestoredSuccess(argument);
				break;
			case "onTransactionsRestoredFail":
				onTransactionsRestoredFail(argument);
				break;
			case "onFailedToRetrieveProductList":
				onFailedToRetrieveProductList(argument);
				break;
			}
		}

		public static void onProductListReceived(string productList)
		{
			callback.onProductListReceived(productList);
		}

		public static void onProductPurchaseSuccess(string productId)
		{
			callback.onPurchaseSucceeded(productId);
		}

		public static void onProductPurchaseCancelled(string productId)
		{
			callback.onPurchaseCancelled(productId);
		}

		public static void onProductPurchaseFailed(string productId)
		{
			callback.onPurchaseFailed(productId);
		}

		public static void onTransactionsRestoredSuccess(string empty)
		{
			callback.onTransactionsRestoredSuccess();
		}

		public static void onTransactionsRestoredFail(string error)
		{
			callback.onTransactionsRestoredFail(error);
		}

		public static void onFailedToRetrieveProductList(string nop)
		{
			callback.onFailedToRetrieveProductList();
		}
	}
	public class StoreKitPluginImpl : IStoreKitPlugin
	{
		public void initialise(AppleAppStoreBillingService svc)
		{
			GameObject gameObject = new GameObject();
			gameObject.AddComponent<AppleAppStoreCallbackMonoBehaviour>().initialise(svc);
		}

		public bool storeKitPaymentsAvailable()
		{
			throw new NotImplementedException();
		}

		public void storeKitRequestProductData(string productIdentifiers, string[] productIds)
		{
			throw new NotImplementedException();
		}

		public void storeKitPurchaseProduct(string productId)
		{
			throw new NotImplementedException();
		}

		public void storeKitRestoreTransactions()
		{
			throw new NotImplementedException();
		}

		public void addTransactionObserver()
		{
		}
	}
	public class GooglePlayBillingService : IBillingService
	{
		private string publicKey;

		private IRawGooglePlayInterface rawInterface;

		private IBillingServiceCallback callback;

		private ProductIdRemapper remapper;

		private UnibillConfiguration db;

		private ILogger logger;

		private RSACryptoServiceProvider cryptoProvider;

		private HashSet<string> unknownAmazonProducts = new HashSet<string>();

		public GooglePlayBillingService(IRawGooglePlayInterface rawInterface, UnibillConfiguration config, ProductIdRemapper remapper, ILogger logger)
		{
			this.rawInterface = rawInterface;
			publicKey = config.GooglePlayPublicKey;
			this.remapper = remapper;
			db = config;
			this.logger = logger;
			cryptoProvider = PEMKeyLoader.CryptoServiceProviderFromPublicKeyInfo(publicKey);
		}

		public void initialise(IBillingServiceCallback callback)
		{
			this.callback = callback;
			if (publicKey == null || publicKey.Equals("[Your key]"))
			{
				callback.logError(UnibillError.GOOGLEPLAY_PUBLICKEY_NOTCONFIGURED, publicKey);
				callback.onSetupComplete(successful: false);
				return;
			}
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("publicKey", publicKey);
			List<string> list = new List<string>();
			List<object> list2 = new List<object>();
			foreach (PurchasableItem allPurchasableItem in db.AllPurchasableItems)
			{
				Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
				string text = remapper.mapItemIdToPlatformSpecificId(allPurchasableItem);
				list.Add(text);
				dictionary2.Add("productId", text);
				dictionary2.Add("consumable", allPurchasableItem.PurchaseType == PurchaseType.Consumable);
				list2.Add(dictionary2);
			}
			dictionary.Add("products", list2);
			string text2 = dictionary.toJson();
			rawInterface.initialise(this, text2, list.ToArray());
		}

		public void restoreTransactions()
		{
			rawInterface.restoreTransactions();
		}

		public void purchase(string item, string developerPayload)
		{
			if (unknownAmazonProducts.Contains(item))
			{
				callback.logError(UnibillError.GOOGLEPLAY_ATTEMPTING_TO_PURCHASE_PRODUCT_NOT_RETURNED_BY_GOOGLEPLAY, item);
				callback.onPurchaseFailedEvent(item);
			}
			else
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				dictionary["productId"] = item;
				dictionary["developerPayload"] = developerPayload;
				rawInterface.purchase(MiniJSON.jsonEncode(dictionary));
			}
		}

		public void onBillingNotSupported()
		{
			callback.logError(UnibillError.GOOGLEPLAY_BILLING_UNAVAILABLE);
			callback.onSetupComplete(successful: false);
		}

		public void onPurchaseSucceeded(string json)
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJSON.jsonDecode(json);
			string receipt = (string)dictionary["signature"];
			string text = (string)dictionary["productId"];
			if (!verifyReceipt(receipt))
			{
				logger.Log("Signature is invalid!");
				onPurchaseFailed(text);
			}
			else
			{
				callback.onPurchaseSucceeded(text, receipt);
			}
		}

		public void onPurchaseCancelled(string item)
		{
			callback.onPurchaseCancelledEvent(item);
		}

		public void onPurchaseRefunded(string item)
		{
			callback.onPurchaseRefundedEvent(item);
		}

		public void onPurchaseFailed(string item)
		{
			callback.onPurchaseFailedEvent(item);
		}

		public void onTransactionsRestored(string success)
		{
			if (bool.Parse(success))
			{
				callback.onTransactionsRestoredSuccess();
			}
			else
			{
				callback.onTransactionsRestoredFail(string.Empty);
			}
		}

		public void onInvalidPublicKey(string key)
		{
			callback.logError(UnibillError.GOOGLEPLAY_PUBLICKEY_INVALID, key);
			callback.onSetupComplete(successful: false);
		}

		public void onProductListReceived(string productListString)
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJSON.jsonDecode(productListString);
			if (dictionary.Count == 0)
			{
				callback.logError(UnibillError.GOOGLEPLAY_NO_PRODUCTS_RETURNED);
				callback.onSetupComplete(successful: false);
				return;
			}
			HashSet<PurchasableItem> hashSet = new HashSet<PurchasableItem>();
			foreach (string key in dictionary.Keys)
			{
				if (remapper.canMapProductSpecificId(key.ToString()))
				{
					PurchasableItem purchasableItemFromPlatformSpecificId = remapper.getPurchasableItemFromPlatformSpecificId(key.ToString());
					Dictionary<string, object> dictionary2 = (Dictionary<string, object>)dictionary[key];
					PurchasableItem.Writer.setAvailable(purchasableItemFromPlatformSpecificId, available: true);
					PurchasableItem.Writer.setLocalizedPrice(purchasableItemFromPlatformSpecificId, dictionary2["price"].ToString());
					PurchasableItem.Writer.setLocalizedTitle(purchasableItemFromPlatformSpecificId, (string)dictionary2["localizedTitle"]);
					PurchasableItem.Writer.setLocalizedDescription(purchasableItemFromPlatformSpecificId, (string)dictionary2["localizedDescription"]);
					PurchasableItem.Writer.setISOCurrencySymbol(purchasableItemFromPlatformSpecificId, dictionary2.getString("isoCurrencyCode", string.Empty));
					long value = dictionary2.getLong("priceInMicros");
					decimal amount = new decimal(value) / 1000000m;
					PurchasableItem.Writer.setPriceInLocalCurrency(purchasableItemFromPlatformSpecificId, amount);
					hashSet.Add(purchasableItemFromPlatformSpecificId);
				}
				else
				{
					logger.LogError("Warning: Unknown product identifier: {0}", key.ToString());
				}
			}
			HashSet<PurchasableItem> hashSet2 = new HashSet<PurchasableItem>(db.AllPurchasableItems);
			hashSet2.ExceptWith(hashSet);
			if (hashSet2.Count > 0)
			{
				foreach (PurchasableItem item in hashSet2)
				{
					unknownAmazonProducts.Add(remapper.mapItemIdToPlatformSpecificId(item));
					callback.logError(UnibillError.GOOGLEPLAY_MISSING_PRODUCT, item.Id, remapper.mapItemIdToPlatformSpecificId(item));
				}
			}
			logger.Log("Received product list, polling for consumables...");
			rawInterface.pollForConsumables();
		}

		public void onPollForConsumablesFinished(string json)
		{
			logger.Log("Finished poll for consumables, completing init.");
			Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJSON.jsonDecode(json);
			if (dictionary != null)
			{
				List<string> stringList = dictionary.getStringList("ownedSubscriptions");
				if (stringList != null)
				{
					callback.onActiveSubscriptionsRetrieved(stringList);
				}
				Dictionary<string, object> hash = dictionary.getHash("ownedItems");
				if (hash != null)
				{
					foreach (KeyValuePair<string, object> item in hash)
					{
						callback.onPurchaseReceiptRetrieved(item.Key, item.Value.ToString());
					}
				}
			}
			callback.onSetupComplete(successful: true);
		}

		public bool hasReceipt(string forItem)
		{
			return false;
		}

		public string getReceipt(string forItem)
		{
			throw new NotImplementedException();
		}

		private bool verifyReceipt(string receipt)
		{
			try
			{
				Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJSON.jsonDecode(receipt);
				if (dictionary == null)
				{
					return false;
				}
				string text = dictionary.getString("signature", string.Empty);
				string text2 = dictionary.getString("json", string.Empty);
				if (text == null || text2 == null)
				{
					return false;
				}
				byte[] signature = Convert.FromBase64String(text);
				SHA1Managed halg = new SHA1Managed();
				byte[] bytes = Encoding.UTF8.GetBytes(text2);
				return cryptoProvider.VerifyData(bytes, halg, signature);
			}
			catch (Exception ex)
			{
				logger.Log("Validation exception");
				logger.Log(ex.Message);
				logger.Log(ex.StackTrace.ToString());
				return false;
			}
		}
	}
}
[AddComponentMenu("")]
public class GooglePlayCallbackMonoBehaviour : MonoBehaviour
{
	private GooglePlayBillingService callback;

	public void Awake()
	{
		base.gameObject.name = GetType().ToString();
		UnityEngine.Object.DontDestroyOnLoad(this);
	}

	public void Initialise(GooglePlayBillingService callback)
	{
		this.callback = callback;
	}

	public void onProductListReceived(string json)
	{
		callback.onProductListReceived(json);
	}

	public void onBillingNotSupported()
	{
		callback.onBillingNotSupported();
	}

	public void onPurchaseSucceeded(string productId)
	{
		callback.onPurchaseSucceeded(productId);
	}

	public void onPurchaseCancelled(string productId)
	{
		callback.onPurchaseCancelled(productId);
	}

	public void onPurchaseRefunded(string productId)
	{
		callback.onPurchaseRefunded(productId);
	}

	public void onPurchaseFailed(string productId)
	{
		callback.onPurchaseFailed(productId);
	}

	public void onTransactionsRestored(string successString)
	{
		callback.onTransactionsRestored(successString);
	}

	public void onInvalidPublicKey(string publicKey)
	{
		callback.onInvalidPublicKey(publicKey);
	}

	public void onPollForConsumablesFinished(string result)
	{
		callback.onPollForConsumablesFinished(result);
	}
}
namespace Unibill.Impl
{
	public interface IRawGooglePlayInterface
	{
		void initialise(GooglePlayBillingService callback, string publicKey, string[] productIds);

		void pollForConsumables();

		void purchase(string json);

		void restoreTransactions();
	}
	public class PEMKeyLoader
	{
		private static byte[] SeqOID = new byte[15]
		{
			48, 13, 6, 9, 42, 134, 72, 134, 247, 13,
			1, 1, 1, 5, 0
		};

		private static bool CompareBytearrays(byte[] a, byte[] b)
		{
			if (a.Length != b.Length)
			{
				return false;
			}
			int num = 0;
			foreach (byte b2 in a)
			{
				if (b2 != b[num])
				{
					return false;
				}
				num++;
			}
			return true;
		}

		public static RSACryptoServiceProvider CryptoServiceProviderFromPublicKeyInfo(byte[] x509key)
		{
			byte[] array = new byte[15];
			if (x509key == null || x509key.Length == 0)
			{
				return null;
			}
			MemoryStream input = new MemoryStream(x509key);
			BinaryReader binaryReader = new BinaryReader(input);
			byte b = 0;
			ushort num = 0;
			try
			{
				switch (binaryReader.ReadUInt16())
				{
				case 33072:
					binaryReader.ReadByte();
					break;
				case 33328:
					binaryReader.ReadInt16();
					break;
				default:
					return null;
				}
				array = binaryReader.ReadBytes(15);
				if (!CompareBytearrays(array, SeqOID))
				{
					return null;
				}
				switch (binaryReader.ReadUInt16())
				{
				case 33027:
					binaryReader.ReadByte();
					break;
				case 33283:
					binaryReader.ReadInt16();
					break;
				default:
					return null;
				}
				if (binaryReader.ReadByte() != 0)
				{
					return null;
				}
				switch (binaryReader.ReadUInt16())
				{
				case 33072:
					binaryReader.ReadByte();
					break;
				case 33328:
					binaryReader.ReadInt16();
					break;
				default:
					return null;
				}
				num = binaryReader.ReadUInt16();
				byte b2 = 0;
				byte b3 = 0;
				switch (num)
				{
				case 33026:
					b2 = binaryReader.ReadByte();
					break;
				case 33282:
					b3 = binaryReader.ReadByte();
					b2 = binaryReader.ReadByte();
					break;
				default:
					return null;
				}
				byte[] value = new byte[4] { b2, b3, 0, 0 };
				int num2 = BitConverter.ToInt32(value, 0);
				if (binaryReader.PeekChar() == 0)
				{
					binaryReader.ReadByte();
					num2--;
				}
				byte[] modulus = binaryReader.ReadBytes(num2);
				if (binaryReader.ReadByte() != 2)
				{
					return null;
				}
				int count = binaryReader.ReadByte();
				byte[] exponent = binaryReader.ReadBytes(count);
				RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider();
				rSACryptoServiceProvider.ImportParameters(new RSAParameters
				{
					Modulus = modulus,
					Exponent = exponent
				});
				return rSACryptoServiceProvider;
			}
			finally
			{
				binaryReader.Close();
			}
		}

		public static RSACryptoServiceProvider CryptoServiceProviderFromPublicKeyInfo(string base64EncodedKey)
		{
			try
			{
				return CryptoServiceProviderFromPublicKeyInfo(Convert.FromBase64String(base64EncodedKey));
			}
			catch (FormatException)
			{
			}
			return null;
		}
	}
	public class RawGooglePlayInterface : IRawGooglePlayInterface
	{
		private AndroidJavaObject plugin;

		public void initialise(GooglePlayBillingService callback, string publicKey, string[] productIds)
		{
			new GameObject().AddComponent<GooglePlayCallbackMonoBehaviour>().Initialise(callback);
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.outlinegames.unibill.UniBill"))
			{
				plugin = androidJavaClass.CallStatic<AndroidJavaObject>("instance", new object[0]);
			}
			plugin.Call("initialise", publicKey);
		}

		public void restoreTransactions()
		{
			plugin.Call("restoreTransactions");
		}

		public void purchase(string json)
		{
			plugin.Call("purchaseProduct", json);
		}

		public void pollForConsumables()
		{
			plugin.Call("pollForConsumables");
		}
	}
	public interface IRawSamsungAppsBillingService
	{
		void initialise(SamsungAppsBillingService samsung);

		void getProductList(string json);

		void initiatePurchaseRequest(string productId);

		void restoreTransactions();
	}
	public class RawSamsungAppsBillingInterface : IRawSamsungAppsBillingService
	{
		private AndroidJavaObject samsung;

		public RawSamsungAppsBillingInterface()
		{
			using AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.outlinegames.unibill.samsung.Unibill");
			samsung = androidJavaClass.CallStatic<AndroidJavaObject>("instance", new object[0]);
		}

		public void initialise(SamsungAppsBillingService samsung)
		{
			new GameObject().AddComponent<SamsungAppsCallbackMonoBehaviour>().initialise(samsung);
		}

		public void getProductList(string json)
		{
			samsung.Call("initialise", json);
		}

		public void initiatePurchaseRequest(string productId)
		{
			samsung.Call("initiatePurchaseRequest", productId);
		}

		public void restoreTransactions()
		{
			samsung.Call("restoreTransactions");
		}
	}
	public class SamsungAppsBillingService : IBillingService
	{
		private IBillingServiceCallback callback;

		private ProductIdRemapper remapper;

		private UnibillConfiguration config;

		private IRawSamsungAppsBillingService rawSamsung;

		private ILogger logger;

		private HashSet<string> unknownSamsungProducts = new HashSet<string>();

		public SamsungAppsBillingService(UnibillConfiguration config, ProductIdRemapper remapper, IRawSamsungAppsBillingService rawSamsung, ILogger logger)
		{
			this.config = config;
			this.remapper = remapper;
			this.rawSamsung = rawSamsung;
			this.logger = logger;
		}

		public void initialise(IBillingServiceCallback biller)
		{
			callback = biller;
			rawSamsung.initialise(this);
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("mode", config.SamsungAppsMode.ToString());
			dictionary.Add("itemGroupId", config.SamsungItemGroupId);
			rawSamsung.getProductList(dictionary.toJson());
		}

		public void purchase(string item, string developerPayload)
		{
			if (unknownSamsungProducts.Contains(item))
			{
				callback.logError(UnibillError.SAMSUNG_APPS_ATTEMPTING_TO_PURCHASE_PRODUCT_NOT_RETURNED_BY_SAMSUNG, item);
				callback.onPurchaseFailedEvent(item);
			}
			else
			{
				rawSamsung.initiatePurchaseRequest(item);
			}
		}

		public void restoreTransactions()
		{
			rawSamsung.restoreTransactions();
		}

		public void onProductListReceived(string productListString)
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJSON.jsonDecode(productListString);
			if (dictionary.Count == 0)
			{
				callback.logError(UnibillError.SAMSUNG_APPS_NO_PRODUCTS_RETURNED);
				callback.onSetupComplete(successful: false);
				return;
			}
			HashSet<PurchasableItem> hashSet = new HashSet<PurchasableItem>();
			foreach (string key in dictionary.Keys)
			{
				if (remapper.canMapProductSpecificId(key.ToString()))
				{
					PurchasableItem purchasableItemFromPlatformSpecificId = remapper.getPurchasableItemFromPlatformSpecificId(key.ToString());
					Dictionary<string, object> dictionary2 = (Dictionary<string, object>)dictionary[key];
					PurchasableItem.Writer.setAvailable(purchasableItemFromPlatformSpecificId, available: true);
					PurchasableItem.Writer.setLocalizedPrice(purchasableItemFromPlatformSpecificId, dictionary2["price"].ToString());
					PurchasableItem.Writer.setLocalizedTitle(purchasableItemFromPlatformSpecificId, (string)dictionary2["localizedTitle"]);
					PurchasableItem.Writer.setLocalizedDescription(purchasableItemFromPlatformSpecificId, (string)dictionary2["localizedDescription"]);
					PurchasableItem.Writer.setISOCurrencySymbol(purchasableItemFromPlatformSpecificId, dictionary2.getString("isoCurrencyCode", string.Empty));
					PurchasableItem.Writer.setPriceInLocalCurrency(purchasableItemFromPlatformSpecificId, decimal.Parse(dictionary2.getString("priceDecimal", "0")));
					hashSet.Add(purchasableItemFromPlatformSpecificId);
				}
				else
				{
					logger.LogError("Warning: Unknown product identifier: {0}", key.ToString());
				}
			}
			HashSet<PurchasableItem> hashSet2 = new HashSet<PurchasableItem>(config.AllPurchasableItems);
			hashSet2.ExceptWith(hashSet);
			if (hashSet2.Count > 0)
			{
				foreach (PurchasableItem item in hashSet2)
				{
					unknownSamsungProducts.Add(remapper.mapItemIdToPlatformSpecificId(item));
					callback.logError(UnibillError.SAMSUNG_APPS_MISSING_PRODUCT, item.Id, remapper.mapItemIdToPlatformSpecificId(item));
				}
			}
			callback.onSetupComplete(successful: true);
		}

		public void onPurchaseFailed(string item)
		{
			callback.onPurchaseFailedEvent(item);
		}

		public void onPurchaseCancelled(string item)
		{
			callback.onPurchaseCancelledEvent(item);
		}

		public void onPurchaseSucceeded(string json)
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJSON.jsonDecode(json);
			callback.onPurchaseSucceeded((string)dictionary["productId"], (string)dictionary["signature"]);
		}

		public void onTransactionsRestored(string success)
		{
			if (bool.Parse(success))
			{
				callback.onTransactionsRestoredSuccess();
			}
			else
			{
				callback.onTransactionsRestoredFail(string.Empty);
			}
		}

		public void onInitFail()
		{
			callback.onSetupComplete(successful: false);
		}

		public bool hasReceipt(string forItem)
		{
			return false;
		}

		public string getReceipt(string forItem)
		{
			throw new NotImplementedException();
		}
	}
}
[AddComponentMenu("")]
public class SamsungAppsCallbackMonoBehaviour : MonoBehaviour
{
	private SamsungAppsBillingService samsung;

	public void Start()
	{
		base.gameObject.name = GetType().ToString();
		UnityEngine.Object.DontDestroyOnLoad(this);
	}

	public void initialise(SamsungAppsBillingService samsung)
	{
		this.samsung = samsung;
	}

	public void onProductListReceived(string productCSVString)
	{
		samsung.onProductListReceived(productCSVString);
	}

	public void onPurchaseFailed(string item)
	{
		samsung.onPurchaseFailed(item);
	}

	public void onPurchaseSucceeded(string item)
	{
		samsung.onPurchaseSucceeded(item);
	}

	public void onPurchaseCancelled(string item)
	{
		samsung.onPurchaseCancelled(item);
	}

	public void onTransactionsRestored(string success)
	{
		samsung.onTransactionsRestored(success);
	}

	public void onInitFail()
	{
		samsung.onInitFail();
	}
}
namespace Unibill.Impl
{
	public class WP8BillingService : IBillingService, IWindowsIAPCallback
	{
		private IWindowsIAP wp8;

		private IBillingServiceCallback callback;

		private UnibillConfiguration db;

		private TransactionDatabase tDb;

		private ProductIdRemapper remapper;

		private ILogger logger;

		private HashSet<string> unknownProducts = new HashSet<string>();

		private static int count;

		public WP8BillingService(IWindowsIAP wp8, UnibillConfiguration config, ProductIdRemapper remapper, TransactionDatabase tDb, ILogger logger)
		{
			this.wp8 = wp8;
			db = config;
			this.tDb = tDb;
			this.remapper = remapper;
			this.logger = logger;
		}

		public void initialise(IBillingServiceCallback biller)
		{
			callback = biller;
			init(0);
		}

		private void init(int delay)
		{
			wp8.Initialise(this, delay);
		}

		public void log(string message)
		{
			logger.Log(message);
		}

		public void purchase(string item, string developerPayload)
		{
			if (unknownProducts.Contains(item))
			{
				callback.logError(UnibillError.WP8_ATTEMPTING_TO_PURCHASE_PRODUCT_NOT_RETURNED_BY_MICROSOFT, item);
				callback.onPurchaseFailedEvent(item);
			}
			else
			{
				wp8.Purchase(item);
			}
		}

		public void restoreTransactions()
		{
			enumerateLicenses();
			callback.onTransactionsRestoredSuccess();
		}

		public void enumerateLicenses()
		{
			wp8.EnumerateLicenses();
		}

		public void logError(string error)
		{
			logger.LogError(error);
		}

		public void OnProductListReceived(Product[] products)
		{
			if (products.Length == 0)
			{
				callback.logError(UnibillError.WP8_NO_PRODUCTS_RETURNED);
				callback.onSetupComplete(successful: false);
				return;
			}
			HashSet<string> hashSet = new HashSet<string>();
			foreach (Product product in products)
			{
				if (remapper.canMapProductSpecificId(product.Id))
				{
					hashSet.Add(product.Id);
					PurchasableItem purchasableItemFromPlatformSpecificId = remapper.getPurchasableItemFromPlatformSpecificId(product.Id);
					PurchasableItem.Writer.setAvailable(purchasableItemFromPlatformSpecificId, available: true);
					PurchasableItem.Writer.setLocalizedPrice(purchasableItemFromPlatformSpecificId, product.Price);
					PurchasableItem.Writer.setLocalizedTitle(purchasableItemFromPlatformSpecificId, product.Title);
					PurchasableItem.Writer.setLocalizedDescription(purchasableItemFromPlatformSpecificId, product.Description);
					PurchasableItem.Writer.setISOCurrencySymbol(purchasableItemFromPlatformSpecificId, product.IsoCurrencyCode);
					PurchasableItem.Writer.setPriceInLocalCurrency(purchasableItemFromPlatformSpecificId, product.PriceDecimal);
				}
				else
				{
					logger.LogError("Warning: Unknown product identifier: {0}", product.Id);
				}
			}
			unknownProducts = new HashSet<string>(db.AllNonSubscriptionPurchasableItems.Select((PurchasableItem x) => remapper.mapItemIdToPlatformSpecificId(x)));
			unknownProducts.ExceptWith(hashSet);
			if (unknownProducts.Count > 0)
			{
				foreach (string unknownProduct in unknownProducts)
				{
					callback.logError(UnibillError.WP8_MISSING_PRODUCT, unknownProduct, remapper.getPurchasableItemFromPlatformSpecificId(unknownProduct).Id);
				}
			}
			enumerateLicenses();
			callback.onSetupComplete(successful: true);
		}

		public void RunOnUIThread(Action<int> act)
		{
			throw new NotImplementedException();
		}

		public void OnPurchaseFailed(string productId, string error)
		{
			logger.LogError("Purchase failed: {0}, {1}", productId, error);
			callback.onPurchaseFailedEvent(productId);
		}

		public void OnPurchaseCancelled(string productId)
		{
			callback.onPurchaseCancelledEvent(productId);
		}

		public void OnPurchaseSucceeded(string productId, string receipt)
		{
			logger.LogError("PURCHASE SUCCEEDED!:{0}", count++);
			if (!remapper.canMapProductSpecificId(productId))
			{
				logger.LogError("Purchased unknown product: {0}. Ignoring!", productId);
				return;
			}
			PurchasableItem purchasableItemFromPlatformSpecificId = remapper.getPurchasableItemFromPlatformSpecificId(productId);
			switch (purchasableItemFromPlatformSpecificId.PurchaseType)
			{
			case PurchaseType.Consumable:
				callback.onPurchaseSucceeded(productId, receipt);
				break;
			case PurchaseType.NonConsumable:
			case PurchaseType.Subscription:
			{
				PurchasableItem purchasableItemFromPlatformSpecificId2 = remapper.getPurchasableItemFromPlatformSpecificId(productId);
				if (tDb.getPurchaseHistory(purchasableItemFromPlatformSpecificId2) == 0)
				{
					callback.onPurchaseSucceeded(productId, receipt);
				}
				break;
			}
			}
		}

		public void OnPurchaseSucceeded(string productId)
		{
			OnPurchaseSucceeded(productId, string.Empty);
		}

		public void OnProductListError(string message)
		{
			if (message.Contains("0x805A0194"))
			{
				callback.logError(UnibillError.WP8_APP_ID_NOT_KNOWN);
				callback.onSetupComplete(successful: false);
			}
			else
			{
				logError("Unable to retrieve product listings. Unibill will automatically retry...");
				logError(message);
				init(3000);
			}
		}

		public bool hasReceipt(string forItem)
		{
			return false;
		}

		public string getReceipt(string forItem)
		{
			throw new NotImplementedException();
		}
	}
}
internal class WP8Eventhook : MonoBehaviour
{
	public WP8BillingService callback;

	public void Start()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	public void OnApplicationPause(bool paused)
	{
		if (!paused && callback != null)
		{
			callback.enumerateLicenses();
		}
	}
}
namespace Unibill.Impl
{
	internal class Win8_1BillingService : IBillingService, IWindowsIAPCallback
	{
		private static int count;

		public Win8_1BillingService(IWindowsIAP wp8, UnibillConfiguration config, ProductIdRemapper remapper, TransactionDatabase tDb, ILogger logger)
		{
		}

		public void initialise(IBillingServiceCallback biller)
		{
		}

		private void init(int delay)
		{
		}

		public void purchase(string item, string developerPayload)
		{
		}

		public void restoreTransactions()
		{
		}

		public void enumerateLicenses()
		{
		}

		public void logError(string error)
		{
		}

		public void OnProductListReceived(Product[] products)
		{
		}

		public void log(string message)
		{
		}

		public void OnPurchaseFailed(string productId, string error)
		{
		}

		public void OnPurchaseCancelled(string productId)
		{
		}

		public void OnPurchaseSucceeded(string productId, string receipt)
		{
		}

		public void OnPurchaseSucceeded(string productId)
		{
			OnPurchaseSucceeded(productId, string.Empty);
		}

		public void OnProductListError(string message)
		{
		}

		public bool hasReceipt(string forItem)
		{
			return false;
		}

		public string getReceipt(string forItem)
		{
			throw new NotImplementedException();
		}
	}
}
internal class Win8Eventhook : MonoBehaviour
{
	public Win8_1BillingService callback;

	public void Start()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	public void OnApplicationPause(bool paused)
	{
		if (!paused && callback != null)
		{
			callback.enumerateLicenses();
		}
	}
}
namespace Unibill.Impl
{
	public class AnalyticsReporter
	{
		private enum EventType
		{
			purchase_succeeded,
			purchase_cancelled,
			purchase_failed,
			purchase_refunded,
			new_installation,
			new_session,
			level_change
		}

		private const string ANALYTICS_URL = "http://stats.unibiller.com/stats";

		private const string USER_ID_KEY = "com.outlinegames.unilytics.analytics.userId";

		public const string UNIBILL_VERSION = "1.7.19";

		private UnibillConfiguration config;

		private IHTTPClient client;

		private IUtil util;

		private string userId;

		private bool restoreInProgress;

		private string levelName;

		private DateTime levelLoadTime;

		public AnalyticsReporter(Biller biller, UnibillConfiguration config, IHTTPClient client, IStorage storage, IUtil util, ILevelLoadListener listener)
		{
			this.config = config;
			this.client = client;
			this.util = util;
			userId = getUserId(storage);
			biller.onPurchaseComplete += onSucceeded;
			biller.onPurchaseCancelled += delegate(PurchasableItem obj)
			{
				onEvent(EventType.purchase_cancelled, obj, null);
			};
			biller.onPurchaseRefunded += delegate(PurchasableItem obj)
			{
				onEvent(EventType.purchase_refunded, obj, null);
			};
			biller.onTransactionRestoreBegin += delegate
			{
				restoreInProgress = true;
			};
			biller.onTransactionsRestored += delegate
			{
				restoreInProgress = false;
			};
			listener.registerListener(delegate
			{
				onLevelLoad();
			});
			onEvent(EventType.new_session, null, null);
			levelName = util.loadedLevelName();
			levelLoadTime = DateTime.UtcNow;
		}

		private void onLevelLoad()
		{
			Dictionary<string, object> baseRequest = getBaseRequest(EventType.level_change);
			baseRequest.Add("levelChange", encodeLevelChange());
			levelLoadTime = DateTime.UtcNow;
			levelName = Application.loadedLevelName;
			onEvent(baseRequest);
		}

		private string getUserId(IStorage storage)
		{
			string text = storage.GetString("com.outlinegames.unilytics.analytics.userId", string.Empty);
			if (string.IsNullOrEmpty(text))
			{
				text = Guid.NewGuid().ToString();
				storage.SetString("com.outlinegames.unilytics.analytics.userId", text);
				onEvent(EventType.new_installation, null, null);
			}
			return text;
		}

		private void onSucceeded(PurchaseEvent e)
		{
			if (!restoreInProgress)
			{
				onEvent(EventType.purchase_succeeded, e.PurchasedItem, e.Receipt);
			}
		}

		private void onCancelled(PurchaseEvent e)
		{
			onEvent(EventType.purchase_cancelled, e.PurchasedItem, null);
		}

		private void onEvent(EventType e, PurchasableItem item, string receipt)
		{
			Dictionary<string, object> baseRequest = getBaseRequest(e);
			if (item != null)
			{
				baseRequest.Add("item", encodeItem(item, receipt));
			}
			onEvent(baseRequest);
		}

		private void onEvent(Dictionary<string, object> e)
		{
			if (!string.IsNullOrEmpty(config.UnibillAnalyticsAppId))
			{
				string value = MiniJSON.jsonEncode(e);
				client.doPost("http://stats.unibiller.com/stats", new PostParameter("payload", value));
			}
		}

		private Dictionary<string, object> encodeLevelChange()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("fromLevel", levelName);
			dictionary.Add("fromTime", formatTimestamp(levelLoadTime));
			dictionary.Add("toLevel", util.loadedLevelName());
			dictionary.Add("toTime", formatTimestamp(DateTime.UtcNow));
			return dictionary;
		}

		private static string formatTimestamp(DateTime timestamp)
		{
			return timestamp.ToString("s", CultureInfo.InvariantCulture);
		}

		private Dictionary<string, object> getBaseRequest(EventType eventType)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("appId", config.UnibillAnalyticsAppId);
			dictionary.Add("userId", userId);
			dictionary.Add("appSecret", config.UnibillAnalyticsAppSecret);
			dictionary.Add("eventType", eventType.ToString());
			dictionary.Add("platform", config.CurrentPlatform.ToString());
			dictionary.Add("unibillVersion", "1.7.19");
			dictionary.Add("nonce", Guid.NewGuid().ToString());
			dictionary.Add("deviceInfo", encodeDeviceInfo());
			dictionary.Add("config", encodeConfig());
			return dictionary;
		}

		private Dictionary<string, object> encodeConfig()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("useAmazonSandbox", config.AmazonSandboxEnabled);
			dictionary.Add("samsungAppsMode", config.SamsungAppsMode.ToString());
			dictionary.Add("useHostedConfig", config.UseHostedConfig);
			dictionary.Add("useWin81Sandbox", config.UseWin8_1Sandbox);
			dictionary.Add("useWP8Sandbox", config.WP8SandboxEnabled);
			return dictionary;
		}

		private Dictionary<string, object> encodeItem(PurchasableItem item, string receipt)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("id", item.Id);
			dictionary.Add("currency", item.isoCurrencySymbol);
			dictionary.Add("price", item.priceInLocalCurrency.ToString());
			dictionary.Add("priceString", item.localizedPriceString);
			if (receipt != null)
			{
				dictionary.Add("receipt", receipt);
			}
			return dictionary;
		}

		private Dictionary<string, object> encodeDeviceInfo()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("deviceModel", util.DeviceModel);
			dictionary.Add("deviceName", util.DeviceName);
			dictionary.Add("deviceType", util.DeviceType.ToString());
			dictionary.Add("os", util.OperatingSystem);
			return dictionary;
		}
	}
	public enum BillerState
	{
		INITIALISING,
		INITIALISED,
		INITIALISED_WITH_ERROR,
		INITIALISED_WITH_CRITICAL_ERROR
	}
}
namespace Unibill
{
	public class Biller : IBillingServiceCallback
	{
		private TransactionDatabase transactionDatabase;

		private ILogger logger;

		private HelpCentre help;

		private ProductIdRemapper remapper;

		private CurrencyManager currencyManager;

		public UnibillConfiguration InventoryDatabase { get; private set; }

		public IBillingService billingSubsystem { get; private set; }

		public BillerState State { get; private set; }

		public List<UnibillError> Errors { get; private set; }

		public bool Ready => State == BillerState.INITIALISED || State == BillerState.INITIALISED_WITH_ERROR;

		public string[] CurrencyIdentifiers => currencyManager.Currencies;

		public event Action<bool> onBillerReady;

		public event Action<PurchaseEvent> onPurchaseComplete;

		public event Action<bool> onTransactionRestoreBegin;

		public event Action<bool> onTransactionsRestored;

		public event Action<PurchasableItem> onPurchaseCancelled;

		public event Action<PurchasableItem> onPurchaseRefunded;

		public event Action<PurchasableItem> onPurchaseFailed;

		public event Action<PurchasableItem> onPurchaseDeferred;

		public Biller(UnibillConfiguration config, TransactionDatabase tDb, IBillingService billingSubsystem, ILogger logger, HelpCentre help, ProductIdRemapper remapper, CurrencyManager currencyManager)
		{
			InventoryDatabase = config;
			transactionDatabase = tDb;
			this.billingSubsystem = billingSubsystem;
			this.logger = logger;
			logger.prefix = "UnibillBiller";
			this.help = help;
			Errors = new List<UnibillError>();
			this.remapper = remapper;
			this.currencyManager = currencyManager;
		}

		public void Initialise()
		{
			if (InventoryDatabase.AllPurchasableItems.Count == 0)
			{
				logError(UnibillError.UNIBILL_NO_PRODUCTS_DEFINED);
				onSetupComplete(available: false);
			}
			else
			{
				billingSubsystem.initialise(this);
			}
		}

		public int getPurchaseHistory(PurchasableItem item)
		{
			return transactionDatabase.getPurchaseHistory(item);
		}

		public int getPurchaseHistory(string purchasableId)
		{
			PurchasableItem itemById = InventoryDatabase.getItemById(purchasableId);
			if (itemById == null)
			{
				return -1;
			}
			return getPurchaseHistory(itemById);
		}

		public decimal getCurrencyBalance(string identifier)
		{
			return currencyManager.GetCurrencyBalance(identifier);
		}

		public void creditCurrencyBalance(string identifier, decimal amount)
		{
			currencyManager.CreditBalance(identifier, amount);
		}

		public bool debitCurrencyBalance(string identifier, decimal amount)
		{
			return currencyManager.DebitBalance(identifier, amount);
		}

		public void purchase(PurchasableItem item, string developerPayload = "")
		{
			if (State == BillerState.INITIALISING)
			{
				logError(UnibillError.BILLER_NOT_READY);
				this.onPurchaseFailed(item);
			}
			else if (State == BillerState.INITIALISED_WITH_CRITICAL_ERROR)
			{
				logError(UnibillError.UNIBILL_INITIALISE_FAILED_WITH_CRITICAL_ERROR);
				this.onPurchaseFailed(item);
			}
			else if (item == null)
			{
				logger.LogError("Trying to purchase null PurchasableItem");
			}
			else if (item.PurchaseType == PurchaseType.NonConsumable && transactionDatabase.getPurchaseHistory(item) > 0)
			{
				logError(UnibillError.UNIBILL_ATTEMPTING_TO_PURCHASE_ALREADY_OWNED_NON_CONSUMABLE);
				this.onPurchaseFailed(item);
			}
			else
			{
				billingSubsystem.purchase(remapper.mapItemIdToPlatformSpecificId(item), developerPayload);
				logger.Log("purchase({0})", item.Id);
			}
		}

		public void purchase(string purchasableId, string developerPayload = "")
		{
			PurchasableItem itemById = InventoryDatabase.getItemById(purchasableId);
			if (itemById == null)
			{
				logger.LogWarning("Unable to purchase unknown item with id: {0}", purchasableId);
			}
			purchase(itemById, developerPayload);
		}

		public void restoreTransactions()
		{
			logger.Log("restoreTransactions()");
			if (!Ready)
			{
				logError(UnibillError.BILLER_NOT_READY);
				return;
			}
			if (this.onTransactionRestoreBegin != null)
			{
				this.onTransactionRestoreBegin(obj: true);
			}
			billingSubsystem.restoreTransactions();
		}

		public void onPurchaseSucceeded(string id, string receipt)
		{
			if (!verifyPlatformId(id))
			{
				return;
			}
			if (receipt != null)
			{
				onPurchaseReceiptRetrieved(id, receipt);
			}
			PurchasableItem purchasableItemFromPlatformSpecificId = remapper.getPurchasableItemFromPlatformSpecificId(id);
			if (purchasableItemFromPlatformSpecificId.PurchaseType == PurchaseType.NonConsumable && transactionDatabase.getPurchaseHistory(purchasableItemFromPlatformSpecificId) > 0)
			{
				logger.Log("Ignoring multi purchase of non consumable");
				return;
			}
			logger.Log("onPurchaseSucceeded({0})", purchasableItemFromPlatformSpecificId.Id);
			transactionDatabase.onPurchase(purchasableItemFromPlatformSpecificId);
			currencyManager.OnPurchased(purchasableItemFromPlatformSpecificId.Id);
			if (this.onPurchaseComplete != null)
			{
				this.onPurchaseComplete(new PurchaseEvent(purchasableItemFromPlatformSpecificId, receipt));
			}
		}

		public void onSetupComplete(bool available)
		{
			logger.Log("onSetupComplete({0})", available);
			State = ((!available) ? BillerState.INITIALISED_WITH_CRITICAL_ERROR : ((Errors.Count <= 0) ? BillerState.INITIALISED : BillerState.INITIALISED_WITH_ERROR));
			if (this.onBillerReady != null)
			{
				this.onBillerReady(Ready);
			}
		}

		public void onPurchaseCancelledEvent(string id)
		{
			if (verifyPlatformId(id))
			{
				PurchasableItem purchasableItemFromPlatformSpecificId = remapper.getPurchasableItemFromPlatformSpecificId(id);
				logger.Log("onPurchaseCancelledEvent({0})", purchasableItemFromPlatformSpecificId.Id);
				if (this.onPurchaseCancelled != null)
				{
					this.onPurchaseCancelled(purchasableItemFromPlatformSpecificId);
				}
			}
		}

		public void onPurchaseDeferredEvent(string id)
		{
			if (verifyPlatformId(id))
			{
				PurchasableItem purchasableItemFromPlatformSpecificId = remapper.getPurchasableItemFromPlatformSpecificId(id);
				logger.Log("onPurchaseDeferredEvent({0})", purchasableItemFromPlatformSpecificId.Id);
				if (this.onPurchaseDeferred != null)
				{
					this.onPurchaseDeferred(purchasableItemFromPlatformSpecificId);
				}
			}
		}

		public void onPurchaseRefundedEvent(string id)
		{
			if (verifyPlatformId(id))
			{
				PurchasableItem purchasableItemFromPlatformSpecificId = remapper.getPurchasableItemFromPlatformSpecificId(id);
				logger.Log("onPurchaseRefundedEvent({0})", purchasableItemFromPlatformSpecificId.Id);
				transactionDatabase.onRefunded(purchasableItemFromPlatformSpecificId);
				if (this.onPurchaseRefunded != null)
				{
					this.onPurchaseRefunded(purchasableItemFromPlatformSpecificId);
				}
			}
		}

		public void onPurchaseFailedEvent(string id)
		{
			if (verifyPlatformId(id))
			{
				PurchasableItem purchasableItemFromPlatformSpecificId = remapper.getPurchasableItemFromPlatformSpecificId(id);
				logger.Log("onPurchaseFailedEvent({0})", purchasableItemFromPlatformSpecificId.Id);
				if (this.onPurchaseFailed != null)
				{
					this.onPurchaseFailed(purchasableItemFromPlatformSpecificId);
				}
			}
		}

		public void onTransactionsRestoredSuccess()
		{
			logger.Log("onTransactionsRestoredSuccess()");
			if (this.onTransactionsRestored != null)
			{
				this.onTransactionsRestored(obj: true);
			}
		}

		public void ClearPurchases()
		{
			foreach (PurchasableItem allPurchasableItem in InventoryDatabase.AllPurchasableItems)
			{
				transactionDatabase.clearPurchases(allPurchasableItem);
			}
		}

		public void onTransactionsRestoredFail(string error)
		{
			logger.Log("onTransactionsRestoredFail({0})", error);
			this.onTransactionsRestored(obj: false);
		}

		public bool isOwned(PurchasableItem item)
		{
			return getPurchaseHistory(item) > 0;
		}

		public void setAppReceipt(string receipt)
		{
			foreach (PurchasableItem allPurchasableItem in InventoryDatabase.AllPurchasableItems)
			{
				if (getPurchaseHistory(allPurchasableItem) > 0)
				{
					allPurchasableItem.receipt = receipt;
				}
			}
		}

		public void onActiveSubscriptionsRetrieved(IEnumerable<string> subscriptions)
		{
			foreach (PurchasableItem allSubscription in InventoryDatabase.AllSubscriptions)
			{
				transactionDatabase.clearPurchases(allSubscription);
			}
			foreach (string subscription in subscriptions)
			{
				if (!remapper.canMapProductSpecificId(subscription))
				{
					logger.LogError("Entitled to unknown subscription: {0}. Ignoring", subscription);
				}
				else
				{
					transactionDatabase.onPurchase(remapper.getPurchasableItemFromPlatformSpecificId(subscription));
				}
			}
		}

		public void logError(UnibillError error)
		{
			logError(error, new object[0]);
		}

		public void logError(UnibillError error, params object[] args)
		{
			Errors.Add(error);
			logger.LogError(help.getMessage(error), args);
		}

		public void onPurchaseReceiptRetrieved(string platformSpecificItemId, string receipt)
		{
			if (remapper.canMapProductSpecificId(platformSpecificItemId))
			{
				PurchasableItem purchasableItemFromPlatformSpecificId = remapper.getPurchasableItemFromPlatformSpecificId(platformSpecificItemId);
				purchasableItemFromPlatformSpecificId.receipt = receipt;
			}
		}

		private bool verifyPlatformId(string platformId)
		{
			if (!remapper.canMapProductSpecificId(platformId))
			{
				logError(UnibillError.UNIBILL_UNKNOWN_PRODUCTID, platformId);
				return false;
			}
			return true;
		}
	}
}
namespace Unibill.Impl
{
	public class BillerFactory
	{
		private IResourceLoader loader;

		private ILogger logger;

		private IStorage storage;

		private IRawBillingPlatformProvider platformProvider;

		private IUtil util;

		private UnibillConfiguration config;

		private CurrencyManager _currencyManager;

		private TransactionDatabase _tDb;

		private HelpCentre _helpCentre;

		private ProductIdRemapper _remapper;

		public BillerFactory(IResourceLoader resourceLoader, ILogger logger, IStorage storage, IRawBillingPlatformProvider platformProvider, UnibillConfiguration config, IUtil util)
		{
			loader = resourceLoader;
			this.logger = logger;
			this.storage = storage;
			this.platformProvider = platformProvider;
			this.config = config;
			this.util = util;
		}

		public Biller instantiate()
		{
			IBillingService billingSubsystem = instantiateBillingSubsystem();
			Biller biller = new Biller(config, getTransactionDatabase(), billingSubsystem, getLogger(), getHelp(), getMapper(), getCurrencyManager());
			instantiateAnalytics(biller);
			return biller;
		}

		public DownloadManager instantiateDownloadManager(Biller biller)
		{
			DownloadManager downloadManager = new DownloadManager(util, storage, new UnityURLFetcher(), logger, biller.InventoryDatabase.CurrentPlatform, biller.InventoryDatabase.UnibillAnalyticsAppSecret, biller.InventoryDatabase.UnibillAnalyticsAppId);
			util.InitiateCoroutine(downloadManager.monitorDownloads());
			return downloadManager;
		}

		public void instantiateAnalytics(Biller biller)
		{
			if (!string.IsNullOrEmpty(config.UnibillAnalyticsAppId))
			{
				new AnalyticsReporter(biller, config, platformProvider.getHTTPClient(util), getStorage(), util, platformProvider.getLevelLoadListener());
			}
		}

		private IBillingService instantiateBillingSubsystem()
		{
			switch (config.CurrentPlatform)
			{
			case BillingPlatform.AppleAppStore:
				return new AppleAppStoreBillingService(config, getMapper(), platformProvider.getStorekit(), getLogger());
			case BillingPlatform.AmazonAppstore:
				return new AmazonAppStoreBillingService(platformProvider.getAmazon(), getMapper(), config, getTransactionDatabase(), getLogger());
			case BillingPlatform.GooglePlay:
				return new GooglePlayBillingService(platformProvider.getGooglePlay(), config, getMapper(), getLogger());
			case BillingPlatform.MacAppStore:
				return new AppleAppStoreBillingService(config, getMapper(), platformProvider.getStorekit(), getLogger());
			case BillingPlatform.WindowsPhone8:
			{
				WP8BillingService wP8BillingService = new WP8BillingService(Factory.Create(config.WP8SandboxEnabled, GetDummyProducts()), config, getMapper(), getTransactionDatabase(), getLogger());
				new GameObject().AddComponent<WP8Eventhook>().callback = wP8BillingService;
				return wP8BillingService;
			}
			case BillingPlatform.Windows8_1:
			{
				Win8_1BillingService win8_1BillingService = new Win8_1BillingService(Factory.Create(config.UseWin8_1Sandbox, GetDummyProducts()), config, getMapper(), getTransactionDatabase(), getLogger());
				new GameObject().AddComponent<Win8Eventhook>().callback = win8_1BillingService;
				return win8_1BillingService;
			}
			case BillingPlatform.SamsungApps:
				return new SamsungAppsBillingService(config, getMapper(), platformProvider.getSamsung(), getLogger());
			default:
				return new FakeBillingService(getMapper());
			}
		}

		private CurrencyManager getCurrencyManager()
		{
			if (_currencyManager == null)
			{
				_currencyManager = new CurrencyManager(config, getStorage());
			}
			return _currencyManager;
		}

		private Product[] GetDummyProducts()
		{
			IEnumerable<Product> source = from x in config.AllPurchasableItems
				where x.PurchaseType != PurchaseType.Subscription
				select new Product
				{
					Consumable = (x.PurchaseType == PurchaseType.Consumable),
					Description = x.description,
					Id = x.LocalId,
					Price = "$123.45",
					PriceDecimal = 123.45m,
					Title = x.name
				};
			return source.ToArray();
		}

		private TransactionDatabase getTransactionDatabase()
		{
			if (_tDb == null)
			{
				_tDb = new TransactionDatabase(getStorage(), getLogger());
			}
			return _tDb;
		}

		private IStorage getStorage()
		{
			return storage;
		}

		private HelpCentre getHelp()
		{
			if (_helpCentre == null)
			{
				_helpCentre = new HelpCentre(loader.openTextFile("unibillStrings.json").ReadToEnd());
			}
			return _helpCentre;
		}

		private ProductIdRemapper getMapper()
		{
			if (_remapper == null)
			{
				_remapper = new ProductIdRemapper(config);
			}
			return _remapper;
		}

		private ILogger getLogger()
		{
			return logger;
		}

		private IResourceLoader getResourceLoader()
		{
			return loader;
		}
	}
	public class CurrencyManager
	{
		private IStorage storage;

		private UnibillConfiguration config;

		public string[] Currencies { get; private set; }

		public CurrencyManager(UnibillConfiguration config, IStorage storage)
		{
			this.storage = storage;
			this.config = config;
			Currencies = config.currencies.Select((VirtualCurrency x) => x.currencyId).ToArray();
		}

		public void OnPurchased(string id)
		{
			foreach (VirtualCurrency currency in config.currencies)
			{
				if (currency.mappings.ContainsKey(id))
				{
					CreditBalance(currency.currencyId, currency.mappings[id]);
				}
			}
		}

		public decimal GetCurrencyBalance(string id)
		{
			return storage.GetInt(getKey(id), 0);
		}

		public void CreditBalance(string id, decimal amount)
		{
			storage.SetInt(getKey(id), (int)(GetCurrencyBalance(id) + amount));
		}

		public void SetBalance(string id, decimal amount)
		{
			storage.SetInt(getKey(id), (int)amount);
		}

		public bool DebitBalance(string id, decimal amount)
		{
			decimal currencyBalance = GetCurrencyBalance(id);
			if (currencyBalance - amount >= 0m)
			{
				storage.SetInt(getKey(id), (int)(currencyBalance - amount));
				return true;
			}
			return false;
		}

		private string getKey(string id)
		{
			return $"com.outlinegames.unibill.currencies.{id}.balance";
		}
	}
	public class DownloadManager
	{
		private const string DOWNLOAD_TOKEN_URL = "http://cdn.unibiller.com/download_token";

		private const string SCHEDULED_DOWNLOADS_KEY = "com.outlinegames.unibill.scheduled_downloads";

		private const int DEFAULT_BUFFER_SIZE = 2000000;

		private IUtil util;

		private IStorage storage;

		private IURLFetcher fetcher;

		private ILogger logger;

		private volatile string persistentDataPath;

		private List<string> scheduledDownloads = new List<string>();

		private int bufferSize = 2000000;

		private byte[] BUFFER = new byte[2200000];

		private AutoResetEvent DATA_READY = new AutoResetEvent(initialState: false);

		private volatile bool UNPACK_FINISHED;

		private volatile bool DATA_FLUSHED;

		private volatile FileStream fileStream;

		private volatile int bytesReceived;

		private BillingPlatform platform;

		private string appSecret;

		private string appId;

		private WaitForFixedUpdate waiter = new WaitForFixedUpdate();

		private System.Random rand = new System.Random();

		public event Action<string, string> onDownloadCompletedEvent;

		public event Action<string, string> onDownloadFailedEvent;

		public event Action<string, int> onDownloadProgressedEvent;

		public DownloadManager(IUtil util, IStorage storage, IURLFetcher fetcher, ILogger logger, BillingPlatform platform, string appSecret, string appId)
		{
			this.util = util;
			this.storage = storage;
			this.fetcher = fetcher;
			this.logger = logger;
			this.platform = platform;
			this.appSecret = appSecret;
			this.appId = appId;
			scheduledDownloads = deserialiseDownloads();
			persistentDataPath = util.persistentDataPath;
			Thread thread = new Thread(DownloadFlusher);
			thread.IsBackground = true;
			thread.Start();
		}

		public void setBufferSize(int size)
		{
			bufferSize = size;
		}

		public void downloadContentFor(string fileBundleId, string receipt)
		{
			if (isDownloaded(fileBundleId))
			{
				this.onDownloadCompletedEvent(fileBundleId, getContentPath(fileBundleId));
			}
			else if (!scheduledDownloads.Contains(fileBundleId))
			{
				createDataPathIfNecessary(fileBundleId);
				saveReceipt(fileBundleId, receipt);
				scheduledDownloads.Add(fileBundleId);
				serialiseDownloads();
			}
		}

		public bool isDownloadScheduled(string bundleId)
		{
			return scheduledDownloads.Contains(bundleId);
		}

		public IEnumerator checkDownloads()
		{
			for (int t = 0; t < scheduledDownloads.Count; t++)
			{
				string scheduledDownload = scheduledDownloads[t];
				yield return util.InitiateCoroutine(download(scheduledDownload.ToString()));
			}
		}

		public IEnumerator monitorDownloads()
		{
			while (true)
			{
				if (scheduledDownloads.Count > 0)
				{
					yield return util.InitiateCoroutine(download(scheduledDownloads[0]));
				}
				else
				{
					yield return waiter;
				}
			}
		}

		public int getQueueSize()
		{
			return scheduledDownloads.Count;
		}

		private List<string> deserialiseDownloads()
		{
			List<object> list = storage.GetString("com.outlinegames.unibill.scheduled_downloads", "[]").arrayListFromJson();
			List<string> list2 = new List<string>();
			if (list != null)
			{
				foreach (object item in list)
				{
					list2.Add(item.ToString());
				}
			}
			return list2;
		}

		private void serialiseDownloads()
		{
			List<object> list = new List<object>();
			foreach (string scheduledDownload in scheduledDownloads)
			{
				list.Add(scheduledDownload);
			}
			storage.SetString("com.outlinegames.unibill.scheduled_downloads", MiniJSON.jsonEncode(list));
		}

		private IEnumerator download(string bundleId)
		{
			createDataPathIfNecessary(bundleId);
			if (!File.Exists(getZipPath(bundleId)))
			{
				logger.Log(bundleId);
				string downloadToken = string.Empty;
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				try
				{
					parameters.Add("receipt", getReceipt(bundleId));
				}
				catch (IOException)
				{
					onDownloadFailedPermanently(bundleId, $"Bundle {bundleId} no longer defined in inventory!");
					yield break;
				}
				parameters.Add("appId", appId);
				parameters.Add("bundleName", bundleId);
				parameters.Add("platform", platform.ToString());
				parameters.Add("appSecret", appSecret);
				parameters.Add("version", getVersionToDownload(bundleId));
				parameters.Add("unibillVersion", "1.7.19");
				yield return fetcher.doPost("http://cdn.unibiller.com/download_token", parameters);
				IHTTPRequest response = fetcher.getResponse();
				if (!string.IsNullOrEmpty(response.error))
				{
					logger.Log("Error downloading content: {0}. Unibill will retry later.", response.error);
					yield return getRandomSleep();
					yield break;
				}
				Dictionary<string, object> downloadTokenHash = (Dictionary<string, object>)MiniJSON.jsonDecode(response.contentString);
				if (downloadTokenHash == null)
				{
					logger.Log("Error fetching download token. Unibill will retry later.");
					yield return getRandomSleep();
					yield break;
				}
				if (!bool.Parse(downloadTokenHash["success"].ToString()))
				{
					logger.LogError("Error downloading bundle {0}. Download abandoned.", bundleId);
					string errorString = string.Empty;
					if (downloadTokenHash.ContainsKey("error"))
					{
						errorString = downloadTokenHash["error"].ToString();
						logger.LogError(errorString);
					}
					onDownloadFailedPermanently(bundleId, errorString);
					yield break;
				}
				if (!downloadTokenHash.ContainsKey("url"))
				{
					logger.LogError("Error fetching download token. Missing URL. Will retry");
					yield return getRandomSleep();
					yield break;
				}
				downloadToken = downloadTokenHash["url"].ToString();
				if (!downloadTokenHash.ContainsKey("version"))
				{
					logger.LogError("Error fetching download token. Missing version. Will retry");
					yield return getRandomSleep();
					yield break;
				}
				string version = downloadTokenHash["version"].ToString();
				saveVersion(bundleId, version);
				Dictionary<string, string> headers = new Dictionary<string, string>
				{
					["If-Modified-Since"] = "Tue, 1 Jan 1980 00:00:00 GMT",
					["If-None-Match"] = "notanetag",
					["Range"] = "bytes=0-1"
				};
				yield return fetcher.doGet(downloadToken, headers);
				IHTTPRequest response2 = fetcher.getResponse();
				if (isContentNotFound(response2))
				{
					string error = $"404 - Downloadable Content missing for bundle {bundleId}!";
					logger.LogError(error);
					onDownloadFailedPermanently(bundleId, error);
					yield break;
				}
				if (!response2.responseHeaders.ContainsKey("CONTENT-RANGE"))
				{
					logger.LogError("Malformed server response. Missing content-range");
					logger.LogError(response2.error);
					yield return getRandomSleep();
					yield break;
				}
				string contentRange = response2.responseHeaders["CONTENT-RANGE"].ToString();
				long contentLength = long.Parse(contentRange.Split(new char[1] { '/' }, 2)[1]);
				using (fileStream = openDownload(bundleId))
				{
					long rangeStart = fileStream.Length;
					if (rangeStart > 0)
					{
						fileStream.Seek(0L, SeekOrigin.End);
					}
					long rangeEnd = Math.Min(rangeStart + bufferSize, contentLength);
					int lastProgress = -1;
					while (rangeStart < rangeEnd)
					{
						string header = $"bytes={rangeStart}-{rangeEnd}";
						headers["Range"] = header;
						yield return fetcher.doGet(downloadToken, headers);
						IHTTPRequest response3 = fetcher.getResponse();
						if (!string.IsNullOrEmpty(response3.error))
						{
							logger.LogError("Error downloading content. Will retry.");
							logger.LogError(response3.error);
							yield return getRandomSleep();
							yield break;
						}
						int progress = (int)((float)rangeEnd / (float)contentLength * 100f);
						progress = Math.Min(99, progress);
						if (this.onDownloadProgressedEvent != null && lastProgress != progress)
						{
							this.onDownloadProgressedEvent(bundleId, progress);
							lastProgress = progress;
						}
						if (response3.bytes.Length > BUFFER.Length)
						{
							logger.LogError("Malformed content. Unexpected length. Will retry.");
							yield return getRandomSleep();
							yield break;
						}
						Buffer.BlockCopy(response3.bytes, 0, BUFFER, 0, response3.bytes.Length);
						bytesReceived = response3.bytes.Length;
						DATA_FLUSHED = false;
						DATA_READY.Set();
						while (!DATA_FLUSHED)
						{
							yield return waiter;
						}
						rangeStart = rangeEnd + 1;
						rangeEnd = rangeStart + bufferSize;
						rangeEnd = Math.Min(rangeEnd, contentLength);
					}
				}
				File.Move(getPartialPath(bundleId), getZipPath(bundleId));
				File.Delete(getVersionPath(bundleId));
			}
			UNPACK_FINISHED = false;
			string bundleId2 = default(string);
			util.RunOnThreadPool(delegate
			{
				Unpack(bundleId2);
			});
			while (!UNPACK_FINISHED)
			{
				yield return waiter;
			}
			removeDownloadFromQueues(bundleId);
			if (this.onDownloadCompletedEvent != null)
			{
				this.onDownloadCompletedEvent(bundleId, getContentPath(bundleId));
			}
		}

		private bool isContentNotFound(IHTTPRequest request)
		{
			foreach (KeyValuePair<string, string> responseHeader in request.responseHeaders)
			{
				if (responseHeader.Value.ToUpper().Contains("404 NOT FOUND"))
				{
					return true;
				}
			}
			if (request.error != null)
			{
				return request.error.Contains("404");
			}
			return false;
		}

		private void Unpack(string bundleId)
		{
			try
			{
				string zipPath = getZipPath(bundleId);
				if (!File.Exists(zipPath))
				{
					logger.LogError("No download found: " + zipPath);
					return;
				}
				logger.Log("Verifying download...");
				if (!verifyDownload(zipPath))
				{
					logger.LogError("Download failed integrity check. Deleting...");
					Directory.Delete(getDataPath(bundleId), recursive: true);
					return;
				}
				logger.Log("Download verified.");
				logger.Log("Unpacking");
				DeleteIfExists(getUnpackPath(bundleId));
				Directory.CreateDirectory(getUnpackPath(bundleId));
				using (FileStream stream = new FileStream(getZipPath(bundleId), FileMode.Open))
				{
					ZipUtils.decompress(stream, getUnpackPath(bundleId));
				}
				logger.Log("Unpack complete");
				DeleteIfExists(getContentPath(bundleId));
				Directory.Move(getUnpackPath(bundleId), getContentPath(bundleId));
				File.Delete(getZipPath(bundleId));
			}
			catch (IOException ex)
			{
				logger.LogError(ex.Message);
				onDownloadFailedPermanently(bundleId, ex.Message);
			}
			catch (Exception ex2)
			{
				logger.LogError(ex2.Message);
				logger.LogError(ex2.StackTrace);
				onDownloadFailedPermanently(bundleId, ex2.Message);
			}
			finally
			{
				UNPACK_FINISHED = true;
			}
		}

		private void DeleteIfExists(string folder)
		{
			if (Directory.Exists(folder))
			{
				Directory.Delete(folder, recursive: true);
			}
		}

		private void onDownloadFailedPermanently(string bundleId, string error)
		{
			util.RunOnMainThread(delegate
			{
				removeDownloadFromQueues(bundleId);
				deleteContent(bundleId);
				if (this.onDownloadFailedEvent != null)
				{
					try
					{
						this.onDownloadFailedEvent(bundleId, error);
					}
					catch (ArgumentException)
					{
						this.onDownloadFailedEvent(null, error);
					}
				}
			});
		}

		private void removeDownloadFromQueues(string bundleId)
		{
			scheduledDownloads.Remove(bundleId);
			serialiseDownloads();
		}

		private bool verifyDownload(string filepath)
		{
			try
			{
				using ZipFile zipFile = new ZipFile(filepath);
				return zipFile.TestArchive(testData: true);
			}
			catch (Exception)
			{
				return false;
			}
		}

		private void DownloadFlusher()
		{
			while (true)
			{
				DATA_READY.WaitOne();
				fileStream.Write(BUFFER, 0, bytesReceived);
				DATA_FLUSHED = true;
			}
		}

		private byte[] decodeBase64String(string s)
		{
			return Convert.FromBase64String(s);
		}

		private FileStream openDownload(string bundleId)
		{
			return new FileStream(getPartialPath(bundleId), FileMode.OpenOrCreate);
		}

		public string getContentPath(string bundleId)
		{
			return Path.Combine(getDataPath(bundleId), "content");
		}

		private string getUnpackPath(string bundleId)
		{
			return Path.Combine(getDataPath(bundleId), "unpack");
		}

		private string getZipPath(string bundleId)
		{
			return Path.Combine(getDataPath(bundleId), "download.zip");
		}

		private string getPartialPath(string bundleId)
		{
			return Path.Combine(getDataPath(bundleId), "download.partial");
		}

		private void saveVersion(string bundleId, string version)
		{
			Util.WriteAllText(getVersionPath(bundleId), version);
		}

		private string getVersionToDownload(string bundleId)
		{
			string versionPath = getVersionPath(bundleId);
			if (File.Exists(versionPath))
			{
				string text = Util.ReadAllText(versionPath);
				if (long.TryParse(text, out var _))
				{
					return text;
				}
			}
			return "*";
		}

		private void saveReceipt(string bundleId, string receipt)
		{
			File.WriteAllText(getReceiptPath(bundleId), receipt);
		}

		private string getReceipt(string bundleId)
		{
			return File.ReadAllText(getReceiptPath(bundleId));
		}

		private string getReceiptPath(string bundleId)
		{
			return Path.Combine(getDataPath(bundleId), "receipt");
		}

		private string getVersionPath(string bundleId)
		{
			return Path.Combine(getDataPath(bundleId), "download.version");
		}

		private string getRootContentPath()
		{
			return Path.Combine(persistentDataPath, "unibill-content");
		}

		public string getDataPath(string bundleId)
		{
			return Path.Combine(getRootContentPath(), bundleId);
		}

		public bool isDownloaded(string bundleId)
		{
			return Directory.Exists(getContentPath(bundleId));
		}

		private void createDataPathIfNecessary(string bundleId)
		{
			Directory.CreateDirectory(getDataPath(bundleId));
		}

		public void deleteContent(string bundleId)
		{
			if (isDownloadScheduled(bundleId))
			{
				logger.LogError("Bundle id {0} is still downloading", bundleId);
			}
			else if (!Directory.Exists(getDataPath(bundleId)))
			{
				logger.LogError("Bundle id {0} is not downloaded", bundleId);
			}
			else
			{
				Directory.Delete(getDataPath(bundleId), recursive: true);
			}
		}

		private object getRandomSleep()
		{
			int num = 30 + rand.Next(30);
			logger.Log("Backing off for {0} seconds", num);
			return util.getWaitForSeconds(num);
		}
	}
}
namespace Tests
{
	public class FakeBillingService : IBillingService
	{
		private IBillingServiceCallback biller;

		private List<string> purchasedItems = new List<string>();

		private ProductIdRemapper remapper;

		public bool reportError;

		public bool reportCriticalError;

		public bool purchaseCalled;

		public bool restoreCalled;

		public FakeBillingService(ProductIdRemapper remapper)
		{
			this.remapper = remapper;
		}

		public void initialise(IBillingServiceCallback biller)
		{
			this.biller = biller;
			if (reportError)
			{
				biller.logError(UnibillError.AMAZONAPPSTORE_GETITEMDATAREQUEST_FAILED);
			}
			biller.onSetupComplete(!reportCriticalError);
		}

		public void purchase(string item, string developerPayload)
		{
			purchaseCalled = true;
			if (remapper.getPurchasableItemFromPlatformSpecificId(item).PurchaseType == PurchaseType.NonConsumable)
			{
				purchasedItems.Add(item);
			}
			biller.onPurchaseReceiptRetrieved(item, "fake receipt");
			biller.onPurchaseSucceeded(item, "{ \"this\" : \"is a fake receipt\" }");
		}

		public void restoreTransactions()
		{
			restoreCalled = true;
			foreach (string purchasedItem in purchasedItems)
			{
				biller.onPurchaseSucceeded(purchasedItem, "{ \"this\" : \"is a fake receipt\" }");
			}
			biller.onTransactionsRestoredSuccess();
		}

		public bool hasReceipt(string forItem)
		{
			return true;
		}

		public string getReceipt(string forItem)
		{
			return "fake";
		}
	}
}
namespace Unibill.Impl
{
	public class HTTPClient : IHTTPClient
	{
		private class PostRequest
		{
			public string url;

			public PostParameter[] parameters;

			public PostRequest(string url, params PostParameter[] parameters)
			{
				this.url = url;
				this.parameters = parameters;
			}
		}

		private Queue<PostRequest> events = new Queue<PostRequest>();

		private WaitForSeconds wait = new WaitForSeconds(5f);

		public HTTPClient(IUtil util)
		{
			util.InitiateCoroutine(pump());
		}

		public void doPost(string url, params PostParameter[] parameters)
		{
			events.Enqueue(new PostRequest(url, parameters));
		}

		private IEnumerator pump()
		{
			while (true)
			{
				if (events.Count > 0)
				{
					PostRequest e = events.Dequeue();
					WWWForm form = new WWWForm();
					for (int t = 0; t < e.parameters.Length; t++)
					{
						form.AddField(e.parameters[0].name, e.parameters[t].value);
					}
					WWW w = new WWW(e.url, form);
					yield return w;
					if (string.IsNullOrEmpty(w.error))
					{
						continue;
					}
					events.Enqueue(e);
					yield return new WaitForSeconds(60f);
				}
				yield return wait;
			}
		}
	}
	public class HelpCentre
	{
		private Dictionary<string, object> helpMap;

		public HelpCentre(string json)
		{
			helpMap = (Dictionary<string, object>)MiniJSON.jsonDecode(json);
		}

		public string getMessage(UnibillError error)
		{
			string arg = $"http://www.outlinegames.com/unibillerrors#{error}";
			return $"{helpMap[error.ToString()]}.\nSee {arg}";
		}
	}
	public interface IBillingService
	{
		void initialise(IBillingServiceCallback biller);

		void purchase(string item, string developerPayload);

		void restoreTransactions();

		bool hasReceipt(string forItem);

		string getReceipt(string forItem);
	}
	public interface IBillingServiceCallback
	{
		void onSetupComplete(bool successful);

		void onPurchaseSucceeded(string platformSpecificId, string receipt);

		void onPurchaseCancelledEvent(string item);

		void onPurchaseRefundedEvent(string item);

		void onPurchaseFailedEvent(string item);

		void onPurchaseDeferredEvent(string item);

		void onTransactionsRestoredSuccess();

		void onTransactionsRestoredFail(string error);

		void onActiveSubscriptionsRetrieved(IEnumerable<string> subscriptions);

		void onPurchaseReceiptRetrieved(string productId, string receipt);

		void setAppReceipt(string receipt);

		void logError(UnibillError error, params object[] args);

		void logError(UnibillError error);
	}
	public class PostParameter
	{
		public string name { get; private set; }

		public string value { get; private set; }

		public PostParameter(string name, string value)
		{
			this.name = name;
			this.value = value;
		}
	}
	public interface IHTTPClient
	{
		void doPost(string url, params PostParameter[] parameters);
	}
	public interface IRawBillingPlatformProvider
	{
		IRawGooglePlayInterface getGooglePlay();

		IRawAmazonAppStoreBillingInterface getAmazon();

		IStoreKitPlugin getStorekit();

		IRawSamsungAppsBillingService getSamsung();

		ILevelLoadListener getLevelLoadListener();

		IHTTPClient getHTTPClient(IUtil util);
	}
}
public enum PurchaseType
{
	Consumable,
	NonConsumable,
	Subscription
}
public class PurchaseEvent
{
	public PurchasableItem PurchasedItem { get; private set; }

	public string Receipt { get; private set; }

	internal PurchaseEvent(PurchasableItem purchasedItem, string receipt)
	{
		PurchasedItem = purchasedItem;
		Receipt = receipt;
	}
}
public class PurchasableItem : IEquatable<PurchasableItem>
{
	public class Writer
	{
		public static void setLocalizedPrice(PurchasableItem item, decimal price)
		{
			item.localizedPrice = price;
			item.localizedPriceString = price.ToString();
		}

		public static void setLocalizedPrice(PurchasableItem item, string price)
		{
			item.localizedPriceString = price;
		}

		public static void setLocalizedTitle(PurchasableItem item, string title)
		{
			item.localizedTitle = title;
		}

		public static void setLocalizedDescription(PurchasableItem item, string description)
		{
			item.localizedDescription = description;
		}

		public static void setPriceInLocalCurrency(PurchasableItem item, decimal amount)
		{
			item.priceInLocalCurrency = amount;
		}

		public static void setISOCurrencySymbol(PurchasableItem item, string code)
		{
			item.isoCurrencySymbol = code;
		}

		public static void setAvailable(PurchasableItem item, bool available)
		{
			item.AvailableToPurchase = available;
		}
	}

	public Dictionary<BillingPlatform, Dictionary<string, object>> platformBundles;

	private BillingPlatform platform;

	public bool AvailableToPurchase { get; internal set; }

	public string Id { get; internal set; }

	public PurchaseType PurchaseType { get; internal set; }

	public string name { get; internal set; }

	public string description { get; internal set; }

	public decimal localizedPrice { get; private set; }

	public string localizedPriceString { get; private set; }

	public string localizedTitle { get; private set; }

	public string localizedDescription { get; private set; }

	public string isoCurrencySymbol { get; private set; }

	public decimal priceInLocalCurrency { get; private set; }

	public bool hasDownloadableContent => !string.IsNullOrEmpty(downloadableContentId);

	public string downloadableContentId { get; internal set; }

	public string LocalId
	{
		get
		{
			if (string.IsNullOrEmpty(LocalIds[platform]))
			{
				return Id;
			}
			return LocalIds[platform];
		}
	}

	public string receipt { get; internal set; }

	public Dictionary<BillingPlatform, string> LocalIds { get; private set; }

	public PurchasableItem()
	{
		Id = new System.Random().Next().ToString();
		description = "Describe me!";
		name = "Name me!";
		PurchaseType = PurchaseType.NonConsumable;
		platformBundles = new Dictionary<BillingPlatform, Dictionary<string, object>>();
		LocalIds = new Dictionary<BillingPlatform, string>();
		foreach (int value in Enum.GetValues(typeof(BillingPlatform)))
		{
			platformBundles[(BillingPlatform)value] = new Dictionary<string, object>();
			LocalIds[(BillingPlatform)value] = string.Empty;
		}
	}

	public PurchasableItem(string id, Dictionary<string, object> hash, BillingPlatform platform)
	{
		Id = id;
		this.platform = platform;
		Deserialize(hash);
	}

	private void Deserialize(Dictionary<string, object> hash)
	{
		PurchaseType = hash.getEnum<PurchaseType>("@purchaseType");
		name = hash.get<string>("name");
		description = hash.get<string>("description");
		downloadableContentId = hash.get<string>("downloadableContentId");
		localizedTitle = name;
		localizedDescription = description;
		priceInLocalCurrency = 1m;
		isoCurrencySymbol = "USD";
		LocalIds = new Dictionary<BillingPlatform, string>();
		platformBundles = new Dictionary<BillingPlatform, Dictionary<string, object>>();
		Dictionary<string, object> dictionary = ((!hash.ContainsKey("platforms")) ? new Dictionary<string, object>() : ((Dictionary<string, object>)hash["platforms"]));
		foreach (int value in Enum.GetValues(typeof(BillingPlatform)))
		{
			if (dictionary.ContainsKey(((BillingPlatform)value).ToString()))
			{
				Dictionary<string, object> dictionary2 = (Dictionary<string, object>)dictionary[((BillingPlatform)value).ToString()];
				string key = $"{(BillingPlatform)value}.Id";
				if (dictionary2 != null && dictionary2.ContainsKey(key))
				{
					LocalIds.Add((BillingPlatform)value, (string)dictionary2[key]);
				}
				if (dictionary2 != null)
				{
					platformBundles[(BillingPlatform)value] = dictionary2;
				}
			}
			if (!LocalIds.ContainsKey((BillingPlatform)value))
			{
				LocalIds[(BillingPlatform)value] = Id;
			}
			if (!platformBundles.ContainsKey((BillingPlatform)value))
			{
				platformBundles[(BillingPlatform)value] = new Dictionary<string, object>();
			}
		}
	}

	public Dictionary<string, object> Serialize()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("@id", Id);
		dictionary.Add("@purchaseType", PurchaseType.ToString());
		dictionary.Add("name", name);
		dictionary.Add("description", description);
		dictionary.Add("downloadableContentId", downloadableContentId);
		dictionary.Add("platforms", platformBundles);
		return dictionary;
	}

	public bool Equals(PurchasableItem other)
	{
		return other.Id == Id;
	}
}
namespace Unibill.Impl
{
	public class WritablePurchasable
	{
		public PurchasableItem item { get; private set; }

		public string Id
		{
			get
			{
				return item.Id;
			}
			set
			{
				item.Id = value;
			}
		}

		public PurchaseType PurchaseType
		{
			get
			{
				return item.PurchaseType;
			}
			set
			{
				item.PurchaseType = value;
			}
		}

		public string description
		{
			get
			{
				return item.description;
			}
			set
			{
				item.description = value;
			}
		}

		public string name
		{
			get
			{
				return item.name;
			}
			set
			{
				item.name = value;
			}
		}

		public string downloadableContentId
		{
			get
			{
				return item.downloadableContentId;
			}
			set
			{
				item.downloadableContentId = value;
			}
		}

		public WritablePurchasable(PurchasableItem item)
		{
			this.item = item;
		}
	}
}
public class VirtualCurrency
{
	public string currencyId { get; set; }

	public Dictionary<string, decimal> mappings { get; private set; }

	public VirtualCurrency(string id, Dictionary<string, decimal> mappings)
	{
		currencyId = id;
		this.mappings = mappings;
	}

	public Dictionary<string, object> Serialize()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("currencyId", currencyId);
		List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
		foreach (KeyValuePair<string, decimal> mapping in mappings)
		{
			Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
			dictionary2.Add("id", mapping.Key);
			dictionary2.Add("amount", mapping.Value);
			list.Add(dictionary2);
		}
		dictionary.Add("mappings", list);
		return dictionary;
	}
}
namespace Unibill.Impl
{
	public class MiniJSON
	{
		private const int TOKEN_NONE = 0;

		private const int TOKEN_CURLY_OPEN = 1;

		private const int TOKEN_CURLY_CLOSE = 2;

		private const int TOKEN_SQUARED_OPEN = 3;

		private const int TOKEN_SQUARED_CLOSE = 4;

		private const int TOKEN_COLON = 5;

		private const int TOKEN_COMMA = 6;

		private const int TOKEN_STRING = 7;

		private const int TOKEN_NUMBER = 8;

		private const int TOKEN_TRUE = 9;

		private const int TOKEN_FALSE = 10;

		private const int TOKEN_NULL = 11;

		private const int BUILDER_CAPACITY = 2000;

		protected static int lastErrorIndex = -1;

		protected static string lastDecode = string.Empty;

		public static object jsonDecode(string json)
		{
			lastDecode = json;
			if (json != null)
			{
				char[] json2 = json.ToCharArray();
				int index = 0;
				bool success = true;
				object result = parseValue(json2, ref index, ref success);
				if (success)
				{
					lastErrorIndex = -1;
				}
				else
				{
					lastErrorIndex = index;
				}
				return result;
			}
			return null;
		}

		public static string jsonEncode(object json)
		{
			StringBuilder stringBuilder = new StringBuilder(2000);
			return (!serializeValue(json, stringBuilder)) ? null : stringBuilder.ToString();
		}

		public static bool lastDecodeSuccessful()
		{
			return lastErrorIndex == -1;
		}

		public static int getLastErrorIndex()
		{
			return lastErrorIndex;
		}

		public static string getLastErrorSnippet()
		{
			if (lastErrorIndex == -1)
			{
				return string.Empty;
			}
			int num = lastErrorIndex - 5;
			int num2 = lastErrorIndex + 15;
			if (num < 0)
			{
				num = 0;
			}
			if (num2 >= lastDecode.Length)
			{
				num2 = lastDecode.Length - 1;
			}
			return lastDecode.Substring(num, num2 - num + 1);
		}

		protected static Dictionary<string, object> parseObject(char[] json, ref int index)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			nextToken(json, ref index);
			bool flag = false;
			while (!flag)
			{
				switch (lookAhead(json, index))
				{
				case 0:
					return null;
				case 6:
					nextToken(json, ref index);
					continue;
				case 2:
					nextToken(json, ref index);
					return dictionary;
				}
				string text = parseString(json, ref index);
				if (text == null)
				{
					return null;
				}
				int num = nextToken(json, ref index);
				if (num != 5)
				{
					return null;
				}
				bool success = true;
				object value = parseValue(json, ref index, ref success);
				if (!success)
				{
					return null;
				}
				dictionary[text] = value;
			}
			return dictionary;
		}

		protected static List<object> parseArray(char[] json, ref int index)
		{
			List<object> list = new List<object>();
			nextToken(json, ref index);
			bool flag = false;
			while (!flag)
			{
				switch (lookAhead(json, index))
				{
				case 0:
					return null;
				case 6:
					nextToken(json, ref index);
					continue;
				case 4:
					break;
				default:
				{
					bool success = true;
					object item = parseValue(json, ref index, ref success);
					if (!success)
					{
						return null;
					}
					list.Add(item);
					continue;
				}
				}
				nextToken(json, ref index);
				break;
			}
			return list;
		}

		protected static object parseValue(char[] json, ref int index, ref bool success)
		{
			switch (lookAhead(json, index))
			{
			case 7:
				return parseString(json, ref index);
			case 8:
				return parseNumber(json, ref index);
			case 1:
				return parseObject(json, ref index);
			case 3:
				return parseArray(json, ref index);
			case 9:
				nextToken(json, ref index);
				return bool.Parse("TRUE");
			case 10:
				nextToken(json, ref index);
				return bool.Parse("FALSE");
			case 11:
				nextToken(json, ref index);
				return null;
			default:
				success = false;
				return null;
			}
		}

		protected static string parseString(char[] json, ref int index)
		{
			string text = string.Empty;
			eatWhitespace(json, ref index);
			char c = json[index++];
			bool flag = false;
			while (!flag && index != json.Length)
			{
				c = json[index++];
				switch (c)
				{
				case '"':
					flag = true;
					break;
				case '\\':
					if (index != json.Length)
					{
						switch (json[index++])
						{
						case '"':
							text += '"';
							continue;
						case '\\':
							text += '\\';
							continue;
						case '/':
							text += '/';
							continue;
						case 'b':
							text += '\b';
							continue;
						case 'f':
							text += '\f';
							continue;
						case 'n':
							text += '\n';
							continue;
						case 'r':
							text += '\r';
							continue;
						case 't':
							text += '\t';
							continue;
						case 'u':
							break;
						default:
							continue;
						}
						int num = json.Length - index;
						if (num >= 4)
						{
							char[] array = new char[4];
							Array.Copy(json, index, array, 0, 4);
							uint utf = uint.Parse(new string(array), NumberStyles.HexNumber);
							text += char.ConvertFromUtf32((int)utf);
							index += 4;
							continue;
						}
					}
					break;
				default:
					text += c;
					continue;
				}
				break;
			}
			if (!flag)
			{
				return null;
			}
			return text;
		}

		protected static double parseNumber(char[] json, ref int index)
		{
			eatWhitespace(json, ref index);
			int lastIndexOfNumber = getLastIndexOfNumber(json, index);
			int num = lastIndexOfNumber - index + 1;
			char[] array = new char[num];
			Array.Copy(json, index, array, 0, num);
			index = lastIndexOfNumber + 1;
			return double.Parse(new string(array), CultureInfo.InvariantCulture);
		}

		protected static int getLastIndexOfNumber(char[] json, int index)
		{
			int i;
			for (i = index; i < json.Length && "0123456789+-.eE".IndexOf(json[i]) != -1; i++)
			{
			}
			return i - 1;
		}

		protected static void eatWhitespace(char[] json, ref int index)
		{
			while (index < json.Length && " \t\n\r".IndexOf(json[index]) != -1)
			{
				index++;
			}
		}

		protected static int lookAhead(char[] json, int index)
		{
			int index2 = index;
			return nextToken(json, ref index2);
		}

		protected static int nextToken(char[] json, ref int index)
		{
			eatWhitespace(json, ref index);
			if (index == json.Length)
			{
				return 0;
			}
			char c = json[index];
			index++;
			switch (c)
			{
			case '{':
				return 1;
			case '}':
				return 2;
			case '[':
				return 3;
			case ']':
				return 4;
			case ',':
				return 6;
			case '"':
				return 7;
			case '-':
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
				return 8;
			case ':':
				return 5;
			default:
			{
				index--;
				int num = json.Length - index;
				if (num >= 5 && json[index] == 'f' && json[index + 1] == 'a' && json[index + 2] == 'l' && json[index + 3] == 's' && json[index + 4] == 'e')
				{
					index += 5;
					return 10;
				}
				if (num >= 4 && json[index] == 't' && json[index + 1] == 'r' && json[index + 2] == 'u' && json[index + 3] == 'e')
				{
					index += 4;
					return 9;
				}
				if (num >= 4 && json[index] == 'n' && json[index + 1] == 'u' && json[index + 2] == 'l' && json[index + 3] == 'l')
				{
					index += 4;
					return 11;
				}
				return 0;
			}
			}
		}

		protected static bool serializeObjectOrArray(object objectOrArray, StringBuilder builder)
		{
			if (objectOrArray is Dictionary<string, object>)
			{
				return serializeObject((Dictionary<string, object>)objectOrArray, builder);
			}
			if (objectOrArray is List<object>)
			{
				return serializeArray((List<object>)objectOrArray, builder);
			}
			return false;
		}

		protected static bool serializeObject(Dictionary<string, object> anObject, StringBuilder builder)
		{
			builder.Append("{");
			IDictionaryEnumerator dictionaryEnumerator = anObject.GetEnumerator();
			bool flag = true;
			while (dictionaryEnumerator.MoveNext())
			{
				string aString = dictionaryEnumerator.Key.ToString();
				object value = dictionaryEnumerator.Value;
				if (!flag)
				{
					builder.Append(", ");
				}
				serializeString(aString, builder);
				builder.Append(":");
				if (!serializeValue(value, builder))
				{
					return false;
				}
				flag = false;
			}
			builder.Append("}");
			return true;
		}

		protected static bool serializeDictionary(Dictionary<string, string> dict, StringBuilder builder)
		{
			builder.Append("{");
			bool flag = true;
			foreach (KeyValuePair<string, string> item in dict)
			{
				if (!flag)
				{
					builder.Append(", ");
				}
				serializeString(item.Key, builder);
				builder.Append(":");
				serializeString(item.Value, builder);
				flag = false;
			}
			builder.Append("}");
			return true;
		}

		protected static bool serializeArray(List<object> anArray, StringBuilder builder)
		{
			builder.Append("[");
			bool flag = true;
			for (int i = 0; i < anArray.Count; i++)
			{
				object value = anArray[i];
				if (!flag)
				{
					builder.Append(", ");
				}
				if (!serializeValue(value, builder))
				{
					return false;
				}
				flag = false;
			}
			builder.Append("]");
			return true;
		}

		protected static bool serializeValue(object value, StringBuilder builder)
		{
			if (value == null)
			{
				builder.Append("null");
			}
			else if (value.GetType().IsArray)
			{
				serializeArray(new List<object>((object[])value), builder);
			}
			else if (value is string)
			{
				serializeString((string)value, builder);
			}
			else if (value is char)
			{
				serializeString(Convert.ToString((char)value), builder);
			}
			else if (value is decimal)
			{
				serializeString(Convert.ToString((decimal)value), builder);
			}
			else if (value is Dictionary<string, object>)
			{
				serializeObject((Dictionary<string, object>)value, builder);
			}
			else if (value is Dictionary<string, string>)
			{
				serializeDictionary((Dictionary<string, string>)value, builder);
			}
			else if (value is List<object>)
			{
				serializeArray((List<object>)value, builder);
			}
			else if (value is bool && (bool)value)
			{
				builder.Append("true");
			}
			else
			{
				if (!(value is bool) || (bool)value)
				{
					return false;
				}
				builder.Append("false");
			}
			return true;
		}

		protected static void serializeString(string aString, StringBuilder builder)
		{
			builder.Append("\"");
			char[] array = aString.ToCharArray();
			foreach (char c in array)
			{
				switch (c)
				{
				case '"':
					builder.Append("\\\"");
					continue;
				case '\\':
					builder.Append("\\\\");
					continue;
				case '\b':
					builder.Append("\\b");
					continue;
				case '\f':
					builder.Append("\\f");
					continue;
				case '\n':
					builder.Append("\\n");
					continue;
				case '\r':
					builder.Append("\\r");
					continue;
				case '\t':
					builder.Append("\\t");
					continue;
				}
				int num = Convert.ToInt32(c);
				if (num >= 32 && num <= 126)
				{
					builder.Append(c);
				}
				else
				{
					builder.Append("\\u" + Convert.ToString(num, 16).PadLeft(4, '0'));
				}
			}
			builder.Append("\"");
		}

		protected static void serializeNumber(double number, StringBuilder builder)
		{
			builder.Append(Convert.ToString(number));
		}
	}
	public static class MiniJsonExtensions
	{
		public static Dictionary<string, object> getHash(this Dictionary<string, object> dic, string key)
		{
			return (Dictionary<string, object>)dic[key];
		}

		public static T getEnum<T>(this Dictionary<string, object> dic, string key)
		{
			if (dic.ContainsKey(key))
			{
				return (T)Enum.Parse(typeof(T), dic[key].ToString(), ignoreCase: true);
			}
			return default(T);
		}

		public static string getString(this Dictionary<string, object> dic, string key, string defaultValue = "")
		{
			if (dic.ContainsKey(key))
			{
				return dic[key].ToString();
			}
			return defaultValue;
		}

		public static long getLong(this Dictionary<string, object> dic, string key)
		{
			if (dic.ContainsKey(key))
			{
				return long.Parse(dic[key].ToString());
			}
			return 0L;
		}

		public static List<string> getStringList(this Dictionary<string, object> dic, string key)
		{
			if (dic.ContainsKey(key))
			{
				List<string> list = new List<string>();
				List<object> list2 = (List<object>)dic[key];
				{
					foreach (object item in list2)
					{
						list.Add(item.ToString());
					}
					return list;
				}
			}
			return new List<string>();
		}

		public static bool getBool(this Dictionary<string, object> dic, string key)
		{
			if (dic.ContainsKey(key))
			{
				return bool.Parse(dic[key].ToString());
			}
			return false;
		}

		public static T get<T>(this Dictionary<string, object> dic, string key)
		{
			if (dic.ContainsKey(key))
			{
				return (T)dic[key];
			}
			return default(T);
		}

		public static string toJson(this Dictionary<string, object> obj)
		{
			return MiniJSON.jsonEncode(obj);
		}

		public static string toJson(this Dictionary<string, string> obj)
		{
			return MiniJSON.jsonEncode(obj);
		}

		public static List<object> arrayListFromJson(this string json)
		{
			return MiniJSON.jsonDecode(json) as List<object>;
		}

		public static Dictionary<string, object> hashtableFromJson(this string json)
		{
			return MiniJSON.jsonDecode(json) as Dictionary<string, object>;
		}
	}
}
namespace Unibill
{
	public class ProductDefinition
	{
		public string PlatformSpecificId { get; private set; }

		public PurchaseType Type { get; private set; }

		public ProductDefinition(string platformSpecificId, PurchaseType type)
		{
			PlatformSpecificId = platformSpecificId;
			Type = type;
		}
	}
}
namespace Unibill.Impl
{
	public class ProductIdRemapper
	{
		private Dictionary<string, string> genericToPlatformSpecificIds;

		private Dictionary<string, string> platformSpecificToGenericIds;

		public UnibillConfiguration db;

		public ProductIdRemapper(UnibillConfiguration config)
		{
			db = config;
			initialiseForPlatform(config.CurrentPlatform);
		}

		public void initialiseForPlatform(BillingPlatform platform)
		{
			genericToPlatformSpecificIds = new Dictionary<string, string>();
			platformSpecificToGenericIds = new Dictionary<string, string>();
			foreach (PurchasableItem item in db.inventory)
			{
				genericToPlatformSpecificIds[item.Id] = item.LocalId;
				platformSpecificToGenericIds[item.LocalId] = item.Id;
			}
		}

		public string[] getAllPlatformSpecificProductIds()
		{
			List<string> list = new List<string>();
			foreach (PurchasableItem allPurchasableItem in db.AllPurchasableItems)
			{
				list.Add(mapItemIdToPlatformSpecificId(allPurchasableItem));
			}
			return list.ToArray();
		}

		public string mapItemIdToPlatformSpecificId(PurchasableItem item)
		{
			if (!genericToPlatformSpecificIds.ContainsKey(item.Id))
			{
				throw new ArgumentException("Unknown product id: " + item.Id);
			}
			return genericToPlatformSpecificIds[item.Id];
		}

		public PurchasableItem getPurchasableItemFromPlatformSpecificId(string platformSpecificId)
		{
			string id = platformSpecificToGenericIds[platformSpecificId];
			return db.getItemById(id);
		}

		public bool canMapProductSpecificId(string id)
		{
			if (platformSpecificToGenericIds.ContainsKey(id))
			{
				return true;
			}
			return false;
		}
	}
	internal class RawBillingPlatformProvider : IRawBillingPlatformProvider
	{
		private UnibillConfiguration config;

		private GameObject gameObject;

		private ILevelLoadListener listener;

		private IHTTPClient client;

		public RawBillingPlatformProvider(UnibillConfiguration config)
		{
			this.config = config;
			gameObject = new GameObject();
		}

		public IRawGooglePlayInterface getGooglePlay()
		{
			return new RawGooglePlayInterface();
		}

		public IRawAmazonAppStoreBillingInterface getAmazon()
		{
			return new RawAmazonAppStoreBillingInterface(config);
		}

		public IStoreKitPlugin getStorekit()
		{
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				return new StoreKitPluginImpl();
			}
			return new OSXStoreKitPluginImpl();
		}

		public IRawSamsungAppsBillingService getSamsung()
		{
			return new RawSamsungAppsBillingInterface();
		}

		public ILevelLoadListener getLevelLoadListener()
		{
			if (listener == null)
			{
				listener = gameObject.AddComponent<UnityLevelLoadListener>();
			}
			return listener;
		}

		public IHTTPClient getHTTPClient(IUtil util)
		{
			if (client == null)
			{
				client = new HTTPClient(util);
			}
			return client;
		}
	}
}
public class RemoteConfigFetcher : MonoBehaviour
{
	public void Start()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	public void Fetch(IStorage storage, string url, string key)
	{
		StartCoroutine(fetch(storage, url, key));
	}

	private IEnumerator fetch(IStorage storage, string url, string key)
	{
		WWW request = new WWW(url);
		log("Fetching latest Unibill config from " + url);
		while (!request.isDone)
		{
			yield return new WaitForSeconds(1f);
		}
		if (!string.IsNullOrEmpty(request.error))
		{
			log($"Failed to fetch inventory: {request.error}");
			yield break;
		}
		log("Fetched and stored latest inventory");
		storage.SetString(key, request.text);
	}

	private void log(string message)
	{
		Debug.Log("UnibillConfigFetcher:" + message);
	}
}
namespace Unibill.Impl
{
	public class RemoteConfigManager
	{
		private const string CACHED_CONFIG_PATH = "com.outlinegames.unibill.cached.config";

		private IStorage storage;

		public string XML;

		public UnibillConfiguration Config { get; private set; }

		public RemoteConfigManager(IResourceLoader loader, IStorage storage, ILogger logger, RuntimePlatform platform, List<ProductDefinition> runtimeProducts = null)
		{
			this.storage = storage;
			logger.prefix = "Unibill.RemoteConfigManager";
			XML = loader.openTextFile("unibillInventory.json").ReadToEnd();
			Config = new UnibillConfiguration(XML, platform, logger, runtimeProducts);
			if (Config.UseHostedConfig)
			{
				string text = storage.GetString("com.outlinegames.unibill.cached.config", string.Empty);
				if (string.IsNullOrEmpty(text))
				{
					logger.Log("No cached config available. Using bundled");
				}
				else
				{
					logger.Log("Cached config found, attempting to parse");
					try
					{
						Config = new UnibillConfiguration(text, platform, logger, runtimeProducts);
						if (Config.inventory.Count == 0)
						{
							logger.LogError("No purchasable items in cached config, ignoring.");
							Config = new UnibillConfiguration(XML, platform, logger, runtimeProducts);
						}
						else
						{
							logger.Log($"Using cached config with {Config.inventory.Count} purchasable items");
							XML = text;
						}
					}
					catch (Exception ex)
					{
						logger.LogError("Error parsing inventory: {0}", ex.Message);
						Config = new UnibillConfiguration(XML, platform, logger, runtimeProducts);
					}
				}
				refreshCachedConfig(Config.HostedConfigUrl, logger);
			}
			else
			{
				logger.Log("Not using cached inventory, using bundled.");
				Config = new UnibillConfiguration(XML, platform, logger, runtimeProducts);
			}
		}

		private void refreshCachedConfig(string url, ILogger logger)
		{
			logger.Log("Trying to fetch remote config...");
			new GameObject().AddComponent<RemoteConfigFetcher>().Fetch(storage, Config.HostedConfigUrl, "com.outlinegames.unibill.cached.config");
		}
	}
}
public class TransactionDatabase
{
	private IStorage storage;

	private ILogger logger;

	public string UserId { get; set; }

	public TransactionDatabase(IStorage storage, ILogger logger)
	{
		this.storage = storage;
		this.logger = logger;
		UserId = "default";
	}

	public int getPurchaseHistory(PurchasableItem item)
	{
		return storage.GetInt(getKey(item.Id), 0);
	}

	public void onPurchase(PurchasableItem item)
	{
		int purchaseHistory = getPurchaseHistory(item);
		if (item.PurchaseType != PurchaseType.Consumable && purchaseHistory != 0)
		{
			logger.LogWarning("Apparently multi purchased a non consumable:{0}", item.Id);
		}
		else
		{
			storage.SetInt(getKey(item.Id), purchaseHistory + 1);
		}
	}

	public void clearPurchases(PurchasableItem item)
	{
		storage.SetInt(getKey(item.Id), 0);
	}

	public void onRefunded(PurchasableItem item)
	{
		int purchaseHistory = getPurchaseHistory(item);
		purchaseHistory = Math.Max(0, purchaseHistory - 1);
		storage.SetInt(getKey(item.Id), purchaseHistory);
	}

	private string getKey(string fragment)
	{
		return $"{UserId}.{fragment}";
	}
}
namespace Unibill.Impl
{
	public enum BillingPlatform
	{
		GooglePlay,
		AmazonAppstore,
		SamsungApps,
		AppleAppStore,
		MacAppStore,
		WindowsPhone8,
		Windows8_1,
		UnityEditor
	}
	public enum SamsungAppsMode
	{
		PRODUCTION,
		ALWAYS_SUCCEED,
		ALWAYS_FAIL
	}
	public class UnibillConfiguration
	{
		private ILogger logger;

		public List<PurchasableItem> inventory = new List<PurchasableItem>();

		public List<VirtualCurrency> currencies = new List<VirtualCurrency>();

		public BillingPlatform CurrentPlatform { get; set; }

		public string iOSSKU { get; set; }

		public string macAppStoreSKU { get; set; }

		public BillingPlatform AndroidBillingPlatform { get; set; }

		public string GooglePlayPublicKey { get; set; }

		public bool AmazonSandboxEnabled { get; set; }

		public bool WP8SandboxEnabled { get; set; }

		public bool UseHostedConfig { get; set; }

		public string HostedConfigUrl { get; set; }

		public string UnibillAnalyticsAppId { get; set; }

		public string UnibillAnalyticsAppSecret { get; set; }

		public bool UseWin8_1Sandbox { get; set; }

		public SamsungAppsMode SamsungAppsMode { get; set; }

		public string SamsungItemGroupId { get; set; }

		public List<PurchasableItem> AllPurchasableItems => new List<PurchasableItem>(inventory);

		public List<PurchasableItem> AllNonConsumablePurchasableItems => inventory.FindAll((PurchasableItem x) => x.PurchaseType == PurchaseType.NonConsumable);

		public List<PurchasableItem> AllConsumablePurchasableItems => inventory.FindAll((PurchasableItem x) => x.PurchaseType == PurchaseType.Consumable);

		public List<PurchasableItem> AllSubscriptions => inventory.FindAll((PurchasableItem x) => x.PurchaseType == PurchaseType.Subscription);

		public List<PurchasableItem> AllNonSubscriptionPurchasableItems => inventory.FindAll((PurchasableItem x) => x.PurchaseType != PurchaseType.Subscription);

		public UnibillConfiguration(string json, RuntimePlatform runtimePlatform, ILogger logger, List<ProductDefinition> runtimeProducts = null)
		{
			this.logger = logger;
			Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJSON.jsonDecode(json);
			iOSSKU = dictionary.getString("iOSSKU", string.Empty);
			macAppStoreSKU = dictionary.getString("macAppStoreSKU", string.Empty);
			AndroidBillingPlatform = dictionary.getEnum<BillingPlatform>("androidBillingPlatform");
			GooglePlayPublicKey = dictionary.get<string>("GooglePlayPublicKey");
			AmazonSandboxEnabled = dictionary.getBool("useAmazonSandbox");
			WP8SandboxEnabled = dictionary.getBool("UseWP8MockingFramework");
			UseHostedConfig = dictionary.getBool("useHostedConfig");
			HostedConfigUrl = dictionary.get<string>("hostedConfigUrl");
			UseWin8_1Sandbox = dictionary.getBool("UseWin8_1Sandbox");
			SamsungAppsMode = dictionary.getEnum<SamsungAppsMode>("samsungAppsMode");
			SamsungItemGroupId = dictionary.getString("samsungAppsItemGroupId", string.Empty);
			UnibillAnalyticsAppId = dictionary.getString("unibillAnalyticsAppId", string.Empty);
			UnibillAnalyticsAppSecret = dictionary.getString("unibillAnalyticsAppSecret", string.Empty);
			switch (runtimePlatform)
			{
			case RuntimePlatform.Android:
				CurrentPlatform = AndroidBillingPlatform;
				break;
			case RuntimePlatform.IPhonePlayer:
				CurrentPlatform = BillingPlatform.AppleAppStore;
				break;
			case RuntimePlatform.OSXPlayer:
				CurrentPlatform = BillingPlatform.MacAppStore;
				break;
			case RuntimePlatform.WP8Player:
				CurrentPlatform = BillingPlatform.WindowsPhone8;
				break;
			case RuntimePlatform.MetroPlayerX86:
			case RuntimePlatform.MetroPlayerX64:
			case RuntimePlatform.MetroPlayerARM:
				CurrentPlatform = BillingPlatform.Windows8_1;
				break;
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.WindowsEditor:
				CurrentPlatform = BillingPlatform.UnityEditor;
				break;
			default:
				CurrentPlatform = BillingPlatform.UnityEditor;
				break;
			}
			Dictionary<string, object> dictionary2 = (Dictionary<string, object>)dictionary["purchasableItems"];
			foreach (KeyValuePair<string, object> item3 in dictionary2)
			{
				PurchasableItem item = new PurchasableItem(item3.Key, (Dictionary<string, object>)item3.Value, CurrentPlatform);
				inventory.Add(item);
			}
			if (runtimeProducts != null)
			{
				foreach (ProductDefinition runtimeProduct in runtimeProducts)
				{
					Dictionary<string, object> hash = new Dictionary<string, object> { 
					{
						"@purchaseType",
						runtimeProduct.Type.ToString()
					} };
					PurchasableItem item2 = new PurchasableItem(runtimeProduct.PlatformSpecificId, hash, CurrentPlatform);
					if (!inventory.Exists((PurchasableItem x) => x.Id == item2.Id))
					{
						inventory.Add(item2);
					}
				}
			}
			loadCurrencies(dictionary);
		}

		private void loadCurrencies(Dictionary<string, object> root)
		{
			currencies = new List<VirtualCurrency>();
			Dictionary<string, object> hash = root.getHash("currencies");
			if (hash == null)
			{
				return;
			}
			foreach (KeyValuePair<string, object> item in hash)
			{
				Dictionary<string, decimal> dictionary = new Dictionary<string, decimal>();
				foreach (KeyValuePair<string, object> item2 in (Dictionary<string, object>)item.Value)
				{
					dictionary.Add(item2.Key, decimal.Parse(item2.Value.ToString()));
				}
				currencies.Add(new VirtualCurrency(item.Key, dictionary));
			}
		}

		public PurchasableItem AddItem()
		{
			PurchasableItem purchasableItem = new PurchasableItem();
			inventory.Add(purchasableItem);
			return purchasableItem;
		}

		public Dictionary<string, object> Serialize()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("iOSSKU", iOSSKU);
			dictionary.Add("macAppStoreSKU", macAppStoreSKU);
			dictionary.Add("androidBillingPlatform", AndroidBillingPlatform.ToString());
			dictionary.Add("GooglePlayPublicKey", GooglePlayPublicKey);
			dictionary.Add("useAmazonSandbox", AmazonSandboxEnabled);
			dictionary.Add("UseWP8MockingFramework", WP8SandboxEnabled);
			dictionary.Add("useHostedConfig", UseHostedConfig);
			dictionary.Add("hostedConfigUrl", HostedConfigUrl);
			dictionary.Add("UseWin8_1Sandbox", UseWin8_1Sandbox);
			dictionary.Add("samsungAppsMode", SamsungAppsMode.ToString());
			dictionary.Add("samsungAppsItemGroupId", SamsungItemGroupId);
			dictionary.Add("unibillAnalyticsAppId", UnibillAnalyticsAppId);
			dictionary.Add("unibillAnalyticsAppSecret", UnibillAnalyticsAppSecret);
			Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
			foreach (PurchasableItem item in inventory)
			{
				dictionary2.Add(item.Id, item.Serialize());
			}
			dictionary.Add("purchasableItems", dictionary2);
			Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
			foreach (VirtualCurrency currency in currencies)
			{
				dictionary3.Add(currency.currencyId, currency.mappings);
			}
			dictionary.Add("currencies", dictionary3);
			return dictionary;
		}

		public PurchasableItem getItemById(string id)
		{
			PurchasableItem purchasableItem = inventory.Find((PurchasableItem x) => x.Id == id);
			if (purchasableItem == null)
			{
				logger.LogWarning("Unknown purchasable item:{0}. Check your Unibill inventory configuration.", id);
			}
			return purchasableItem;
		}

		public VirtualCurrency getCurrency(string id)
		{
			return currencies.Find((VirtualCurrency x) => x.currencyId == id);
		}

		private bool tryGetBoolean(string name, Dictionary<string, object> root)
		{
			if (root.ContainsKey(name))
			{
				return bool.Parse(root[name].ToString());
			}
			return false;
		}
	}
}
public enum UnibillError
{
	BILLER_NOT_READY,
	STOREKIT_BILLING_UNAVAILABLE,
	STOREKIT_RETURNED_NO_PRODUCTS,
	STOREKIT_REQUESTPRODUCTS_MISSING_PRODUCT,
	STOREKIT_ATTEMPTING_TO_PURCHASE_PRODUCT_NOT_RETURNED_BY_STOREKIT,
	STOREKIT_FAILED_TO_RETRIEVE_PRODUCT_DATA,
	STOREKIT_UNKNOWN_PRODUCT_ID,
	GOOGLEPLAY_BILLING_UNAVAILABLE,
	GOOGLEPLAY_PUBLICKEY_NOTCONFIGURED,
	GOOGLEPLAY_PUBLICKEY_INVALID,
	GOOGLEPLAY_ATTEMPTING_TO_PURCHASE_PRODUCT_NOT_RETURNED_BY_GOOGLEPLAY,
	GOOGLEPLAY_NO_PRODUCTS_RETURNED,
	GOOGLEPLAY_MISSING_PRODUCT,
	AMAZONAPPSTORE_GETITEMDATAREQUEST_FAILED,
	AMAZONAPPSTORE_GETITEMDATAREQUEST_MISSING_PRODUCT,
	AMAZONAPPSTORE_GETITEMDATAREQUEST_NO_PRODUCTS_RETURNED,
	AMAZONAPPSTORE_ATTEMPTING_TO_PURCHASE_PRODUCT_NOT_RETURNED_BY_AMAZON,
	SAMSUNG_APPS_MISSING_PRODUCT,
	SAMSUNG_APPS_NO_PRODUCTS_RETURNED,
	SAMSUNG_APPS_ATTEMPTING_TO_PURCHASE_PRODUCT_NOT_RETURNED_BY_SAMSUNG,
	WP8_MISSING_PRODUCT,
	WP8_NO_PRODUCTS_RETURNED,
	WP8_ATTEMPTING_TO_PURCHASE_PRODUCT_NOT_RETURNED_BY_MICROSOFT,
	WP8_APP_ID_NOT_KNOWN,
	WIN_8_1_MISSING_PRODUCT,
	WIN_8_1_NO_PRODUCTS_RETURNED,
	WIN_8_1_APP_NOT_KNOWN,
	WIN_8_1_ATTEMPTING_TO_PURCHASE_PRODUCT_NOT_RETURNED_BY_MICROSOFT,
	UNIBILL_UNKNOWN_PRODUCTID,
	UNIBILL_INITIALISE_FAILED_WITH_CRITICAL_ERROR,
	UNIBILL_NO_PRODUCTS_DEFINED,
	UNIBILL_ATTEMPTING_TO_PURCHASE_ALREADY_OWNED_NON_CONSUMABLE
}
public enum UnibillState
{
	SUCCESS,
	SUCCESS_WITH_ERRORS,
	CRITICAL_ERROR
}
public class Unibiller
{
	private static Biller biller;

	private static DownloadManager downloadManager;

	private static DownloadManager DownloadManager;

	private static Action<PurchaseEvent> m_onPurchaseCompleteEvent;

	public static BillingPlatform BillingPlatform
	{
		get
		{
			if (biller != null)
			{
				return biller.InventoryDatabase.CurrentPlatform;
			}
			return BillingPlatform.UnityEditor;
		}
	}

	public static bool Initialised
	{
		get
		{
			if (biller != null)
			{
				return biller.State == BillerState.INITIALISED || biller.State == BillerState.INITIALISED_WITH_ERROR;
			}
			return false;
		}
	}

	public static UnibillError[] Errors
	{
		get
		{
			if (biller != null)
			{
				return biller.Errors.ToArray();
			}
			return new UnibillError[0];
		}
	}

	public static PurchasableItem[] AllPurchasableItems => biller.InventoryDatabase.AllPurchasableItems.ToArray();

	public static PurchasableItem[] AllNonConsumablePurchasableItems => biller.InventoryDatabase.AllNonConsumablePurchasableItems.ToArray();

	public static PurchasableItem[] AllConsumablePurchasableItems => biller.InventoryDatabase.AllConsumablePurchasableItems.ToArray();

	public static PurchasableItem[] AllSubscriptions => biller.InventoryDatabase.AllSubscriptions.ToArray();

	public static string[] AllCurrencies => biller.CurrencyIdentifiers;

	public static event Action<UnibillState> onBillerReady;

	public static event Action<PurchasableItem> onPurchaseCancelled;

	public static event Action<PurchaseEvent> onPurchaseCompleteEvent
	{
		[MethodImpl(MethodImplOptions.Synchronized)]
		add
		{
			Unibiller.onPurchaseComplete = (Action<PurchaseEvent>)Delegate.Combine(Unibiller.onPurchaseComplete, value);
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		remove
		{
			Unibiller.onPurchaseComplete = (Action<PurchaseEvent>)Delegate.Remove(Unibiller.onPurchaseComplete, value);
		}
	}

	public static event Action<PurchasableItem> onPurchaseComplete;

	public static event Action<PurchasableItem> onPurchaseFailed;

	public static event Action<PurchasableItem> onPurchaseDeferred;

	public static event Action<PurchasableItem> onPurchaseRefunded;

	public static event Action<string, DirectoryInfo> onDownloadCompletedEvent;

	public static event Action<string, string> onDownloadCompletedEventString;

	public static event Action<string, int> onDownloadProgressedEvent;

	public static event Action<string, string> onDownloadFailedEvent;

	public static event Action<bool> onTransactionsRestored;

	public static void Initialise(List<ProductDefinition> runtimeProducts = null)
	{
		if (biller == null)
		{
			RemoteConfigManager remoteConfigManager = new RemoteConfigManager(new UnityResourceLoader(), new UnityPlayerPrefsStorage(), new UnityLogger(), Application.platform, runtimeProducts);
			UnibillConfiguration config = remoteConfigManager.Config;
			GameObject gameObject = new GameObject();
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
			UnityUtil util = gameObject.AddComponent<UnityUtil>();
			BillerFactory billerFactory = new BillerFactory(new UnityResourceLoader(), new UnityLogger(), new UnityPlayerPrefsStorage(), new RawBillingPlatformProvider(config), config, util);
			biller = billerFactory.instantiate();
			_internal_hook_events(biller, billerFactory);
		}
		biller.Initialise();
	}

	public static PurchasableItem GetPurchasableItemById(string unibillPurchasableId)
	{
		if (biller != null)
		{
			return biller.InventoryDatabase.getItemById(unibillPurchasableId);
		}
		return null;
	}

	public static void initiatePurchase(PurchasableItem purchasable, string developerPayload = "")
	{
		if (biller != null)
		{
			biller.purchase(purchasable, developerPayload);
		}
	}

	public static void initiatePurchase(string purchasableId, string developerPayload = "")
	{
		if (biller != null)
		{
			biller.purchase(purchasableId, developerPayload);
		}
	}

	public static int GetPurchaseCount(PurchasableItem item)
	{
		if (biller != null)
		{
			return biller.getPurchaseHistory(item);
		}
		return 0;
	}

	public static int GetPurchaseCount(string purchasableId)
	{
		if (biller != null)
		{
			return biller.getPurchaseHistory(purchasableId);
		}
		return 0;
	}

	public static decimal GetCurrencyBalance(string currencyIdentifier)
	{
		if (biller != null)
		{
			return biller.getCurrencyBalance(currencyIdentifier);
		}
		return 0m;
	}

	public static void CreditBalance(string currencyIdentifier, decimal amount)
	{
		if (biller != null)
		{
			biller.creditCurrencyBalance(currencyIdentifier, amount);
		}
	}

	public static bool DebitBalance(string currencyIdentifier, decimal amount)
	{
		if (biller != null)
		{
			return biller.debitCurrencyBalance(currencyIdentifier, amount);
		}
		return false;
	}

	public static void restoreTransactions()
	{
		if (biller != null)
		{
			biller.restoreTransactions();
		}
	}

	public static void clearTransactions()
	{
		if (biller != null)
		{
			biller.ClearPurchases();
		}
	}

	public static void DownloadContent(string bundleId, PurchasableItem proofOfPurchase = null)
	{
		if (downloadManager == null)
		{
			return;
		}
		string receipt = string.Empty;
		if (proofOfPurchase != null)
		{
			if (GetPurchaseCount(proofOfPurchase) == 0 && Unibiller.onDownloadFailedEvent != null)
			{
				Unibiller.onDownloadFailedEvent(bundleId, "Proof of purchase is not owned!");
				return;
			}
			receipt = proofOfPurchase.receipt;
		}
		downloadManager.downloadContentFor(bundleId, receipt);
	}

	public static DirectoryInfo GetDownloadableContentFor(PurchasableItem item)
	{
		if (downloadManager != null && item.hasDownloadableContent)
		{
			return new DirectoryInfo(downloadManager.getContentPath(item.downloadableContentId));
		}
		return null;
	}

	public static string GetDownloadableContentPathFor(string bundleId)
	{
		if (downloadManager != null)
		{
			return downloadManager.getContentPath(bundleId);
		}
		return null;
	}

	public static bool IsContentDownloaded(string bundleId)
	{
		if (downloadManager != null)
		{
			return downloadManager.isDownloaded(bundleId);
		}
		return false;
	}

	public static bool IsDownloadScheduled(string bundleId)
	{
		if (downloadManager != null)
		{
			return downloadManager.isDownloadScheduled(bundleId);
		}
		return false;
	}

	public static void DeleteDownloadedContent(string bundleId)
	{
		if (downloadManager != null)
		{
			downloadManager.deleteContent(bundleId);
		}
	}

	public static void _internal_hook_events(Biller biller, BillerFactory factory)
	{
		biller.onBillerReady += delegate(bool success)
		{
			if (Unibiller.onBillerReady != null)
			{
				if (success)
				{
					downloadManager = factory.instantiateDownloadManager(biller);
					downloadManager.onDownloadCompletedEvent += delegate(string item, string path)
					{
						if (Unibiller.onDownloadCompletedEvent != null)
						{
							Unibiller.onDownloadCompletedEvent(item, new DirectoryInfo(path));
						}
					};
					downloadManager.onDownloadCompletedEvent += Unibiller.onDownloadCompletedEventString;
					downloadManager.onDownloadFailedEvent += Unibiller.onDownloadFailedEvent;
					downloadManager.onDownloadProgressedEvent += Unibiller.onDownloadProgressedEvent;
					Unibiller.onBillerReady((biller.State != BillerState.INITIALISED) ? UnibillState.SUCCESS_WITH_ERRORS : UnibillState.SUCCESS);
				}
				else
				{
					Unibiller.onBillerReady(UnibillState.CRITICAL_ERROR);
				}
			}
		};
		biller.onPurchaseCancelled += _onPurchaseCancelled;
		biller.onPurchaseComplete += _onPurchaseComplete;
		biller.onPurchaseFailed += _onPurchaseFailed;
		biller.onPurchaseDeferred += _onPurchaseDeferred;
		biller.onPurchaseRefunded += _onPurchaseRefunded;
		biller.onTransactionsRestored += _onTransactionsRestored;
	}

	private static void _onPurchaseCancelled(PurchasableItem item)
	{
		if (Unibiller.onPurchaseCancelled != null)
		{
			Unibiller.onPurchaseCancelled(item);
		}
	}

	private static void _onPurchaseComplete(PurchaseEvent e)
	{
		if (Unibiller.onPurchaseComplete != null)
		{
			Unibiller.onPurchaseComplete(e.PurchasedItem);
		}
		if (Unibiller.onPurchaseCompleteEvent != null)
		{
			Unibiller.onPurchaseCompleteEvent(e);
		}
	}

	private static void _onPurchaseFailed(PurchasableItem item)
	{
		if (Unibiller.onPurchaseFailed != null)
		{
			Unibiller.onPurchaseFailed(item);
		}
	}

	private static void _onPurchaseDeferred(PurchasableItem item)
	{
		if (Unibiller.onPurchaseDeferred != null)
		{
			Unibiller.onPurchaseDeferred(item);
		}
	}

	private static void _onPurchaseRefunded(PurchasableItem item)
	{
		if (Unibiller.onPurchaseRefunded != null)
		{
			Unibiller.onPurchaseRefunded(item);
		}
	}

	private static void _onTransactionsRestored(bool success)
	{
		if (Unibiller.onTransactionsRestored != null)
		{
			Unibiller.onTransactionsRestored(success);
		}
	}
}
namespace Unibill.Impl
{
	public class UnityHTTPRequest : IHTTPRequest
	{
		private WWW w;

		public Dictionary<string, string> responseHeaders => w.responseHeaders;

		public byte[] bytes => w.bytes;

		public string contentString => w.text;

		public string error => w.error;

		public UnityHTTPRequest(WWW w)
		{
			this.w = w;
		}
	}
	public class UnityURLFetcher : IURLFetcher
	{
		private UnityHTTPRequest request;

		public object doGet(string url, Dictionary<string, string> headers)
		{
			WWW wWW = new WWW(url, null, headers);
			request = new UnityHTTPRequest(wWW);
			return wWW;
		}

		public object doPost(string url, Dictionary<string, string> parameters)
		{
			WWWForm wWWForm = new WWWForm();
			foreach (KeyValuePair<string, string> parameter in parameters)
			{
				wWWForm.AddField(parameter.Key, parameter.Value);
			}
			WWW wWW = new WWW(url, wWWForm);
			request = new UnityHTTPRequest(wWW);
			return wWW;
		}

		public IHTTPRequest getResponse()
		{
			return request;
		}
	}
	public class Util
	{
		public static string ReadAllText(string path)
		{
			using StreamReader streamReader = new StreamReader(path);
			return streamReader.ReadToEnd();
		}

		public static void WriteAllText(string path, string text)
		{
			using StreamWriter streamWriter = new StreamWriter(path);
			streamWriter.Write(text);
		}
	}
	public class ZipUtils
	{
		public static void decompress(Stream stream, string outputPath)
		{
			ZipInputStream zipInputStream = new ZipInputStream(stream);
			ZipEntry nextEntry = zipInputStream.GetNextEntry();
			byte[] buffer = new byte[4096];
			while (nextEntry != null)
			{
				string name = nextEntry.Name;
				string path = Path.Combine(outputPath, name);
				string directoryName = Path.GetDirectoryName(path);
				if (directoryName.Length > 0)
				{
					Directory.CreateDirectory(directoryName);
				}
				if (!nextEntry.IsDirectory)
				{
					using FileStream destination = File.Create(path);
					Copy(zipInputStream, destination, buffer);
				}
				nextEntry = zipInputStream.GetNextEntry();
			}
		}

		private static void Copy(Stream source, Stream destination, byte[] buffer)
		{
			bool flag = true;
			while (flag)
			{
				int num = source.Read(buffer, 0, buffer.Length);
				if (num > 0)
				{
					destination.Write(buffer, 0, num);
					continue;
				}
				destination.Flush();
				flag = false;
			}
		}
	}
}
[AddComponentMenu("Camera-Control/Mouse Look")]
public class MouseLook : MonoBehaviour
{
	public enum RotationAxes
	{
		MouseXAndY,
		MouseX,
		MouseY
	}

	public RotationAxes axes;

	public float sensitivityX = 15f;

	public float sensitivityY = 15f;

	public float minimumX = -360f;

	public float maximumX = 360f;

	public float minimumY = -60f;

	public float maximumY = 60f;

	private float rotationY;

	private void Update()
	{
		if (axes == RotationAxes.MouseXAndY)
		{
			float y = base.transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
			base.transform.localEulerAngles = new Vector3(0f - rotationY, y, 0f);
		}
		else if (axes == RotationAxes.MouseX)
		{
			base.transform.Rotate(0f, Input.GetAxis("Mouse X") * sensitivityX, 0f);
		}
		else
		{
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
			base.transform.localEulerAngles = new Vector3(0f - rotationY, base.transform.localEulerAngles.y, 0f);
		}
	}

	private void Start()
	{
		if ((bool)base.rigidbody)
		{
			base.rigidbody.freezeRotation = true;
		}
	}
}
[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Blur/Blur")]
public class BlurEffect : MonoBehaviour
{
	public int iterations = 3;

	public float blurSpread = 0.6f;

	public Shader blurShader;

	private static Material m_Material;

	protected Material material
	{
		get
		{
			if (m_Material == null)
			{
				m_Material = new Material(blurShader);
				m_Material.hideFlags = HideFlags.DontSave;
			}
			return m_Material;
		}
	}

	protected void OnDisable()
	{
		if ((bool)m_Material)
		{
			UnityEngine.Object.DestroyImmediate(m_Material);
		}
	}

	protected void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
		}
		else if (!blurShader || !material.shader.isSupported)
		{
			base.enabled = false;
		}
	}

	public void FourTapCone(RenderTexture source, RenderTexture dest, int iteration)
	{
		float num = 0.5f + (float)iteration * blurSpread;
		Graphics.BlitMultiTap(source, dest, material, new Vector2(0f - num, 0f - num), new Vector2(0f - num, num), new Vector2(num, num), new Vector2(num, 0f - num));
	}

	private void DownSample4x(RenderTexture source, RenderTexture dest)
	{
		float num = 1f;
		Graphics.BlitMultiTap(source, dest, material, new Vector2(0f - num, 0f - num), new Vector2(0f - num, num), new Vector2(num, num), new Vector2(num, 0f - num));
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		int width = source.width / 4;
		int height = source.height / 4;
		RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 0);
		DownSample4x(source, renderTexture);
		for (int i = 0; i < iterations; i++)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0);
			FourTapCone(renderTexture, temporary, i);
			RenderTexture.ReleaseTemporary(renderTexture);
			renderTexture = temporary;
		}
		Graphics.Blit(renderTexture, destination);
		RenderTexture.ReleaseTemporary(renderTexture);
	}
}
[AddComponentMenu("Image Effects/Color Adjustments/Color Correction (Ramp)")]
[ExecuteInEditMode]
public class ColorCorrectionEffect : ImageEffectBase
{
	public Texture textureRamp;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		base.material.SetTexture("_RampTex", textureRamp);
		Graphics.Blit(source, destination, base.material);
	}
}
[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Color Adjustments/Contrast Stretch")]
public class ContrastStretchEffect : MonoBehaviour
{
	public float adaptationSpeed = 0.02f;

	public float limitMinimum = 0.2f;

	public float limitMaximum = 0.6f;

	private RenderTexture[] adaptRenderTex = new RenderTexture[2];

	private int curAdaptIndex;

	public Shader shaderLum;

	private Material m_materialLum;

	public Shader shaderReduce;

	private Material m_materialReduce;

	public Shader shaderAdapt;

	private Material m_materialAdapt;

	public Shader shaderApply;

	private Material m_materialApply;

	protected Material materialLum
	{
		get
		{
			if (m_materialLum == null)
			{
				m_materialLum = new Material(shaderLum);
				m_materialLum.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_materialLum;
		}
	}

	protected Material materialReduce
	{
		get
		{
			if (m_materialReduce == null)
			{
				m_materialReduce = new Material(shaderReduce);
				m_materialReduce.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_materialReduce;
		}
	}

	protected Material materialAdapt
	{
		get
		{
			if (m_materialAdapt == null)
			{
				m_materialAdapt = new Material(shaderAdapt);
				m_materialAdapt.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_materialAdapt;
		}
	}

	protected Material materialApply
	{
		get
		{
			if (m_materialApply == null)
			{
				m_materialApply = new Material(shaderApply);
				m_materialApply.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_materialApply;
		}
	}

	private void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
		}
		else if (!shaderAdapt.isSupported || !shaderApply.isSupported || !shaderLum.isSupported || !shaderReduce.isSupported)
		{
			base.enabled = false;
		}
	}

	private void OnEnable()
	{
		for (int i = 0; i < 2; i++)
		{
			if (!adaptRenderTex[i])
			{
				adaptRenderTex[i] = new RenderTexture(1, 1, 0);
				adaptRenderTex[i].hideFlags = HideFlags.HideAndDontSave;
			}
		}
	}

	private void OnDisable()
	{
		for (int i = 0; i < 2; i++)
		{
			UnityEngine.Object.DestroyImmediate(adaptRenderTex[i]);
			adaptRenderTex[i] = null;
		}
		if ((bool)m_materialLum)
		{
			UnityEngine.Object.DestroyImmediate(m_materialLum);
		}
		if ((bool)m_materialReduce)
		{
			UnityEngine.Object.DestroyImmediate(m_materialReduce);
		}
		if ((bool)m_materialAdapt)
		{
			UnityEngine.Object.DestroyImmediate(m_materialAdapt);
		}
		if ((bool)m_materialApply)
		{
			UnityEngine.Object.DestroyImmediate(m_materialApply);
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		RenderTexture renderTexture = RenderTexture.GetTemporary(source.width / 1, source.height / 1);
		Graphics.Blit(source, renderTexture, materialLum);
		while (renderTexture.width > 1 || renderTexture.height > 1)
		{
			int num = renderTexture.width / 2;
			if (num < 1)
			{
				num = 1;
			}
			int num2 = renderTexture.height / 2;
			if (num2 < 1)
			{
				num2 = 1;
			}
			RenderTexture temporary = RenderTexture.GetTemporary(num, num2);
			Graphics.Blit(renderTexture, temporary, materialReduce);
			RenderTexture.ReleaseTemporary(renderTexture);
			renderTexture = temporary;
		}
		CalculateAdaptation(renderTexture);
		materialApply.SetTexture("_AdaptTex", adaptRenderTex[curAdaptIndex]);
		Graphics.Blit(source, destination, materialApply);
		RenderTexture.ReleaseTemporary(renderTexture);
	}

	private void CalculateAdaptation(Texture curTexture)
	{
		int num = curAdaptIndex;
		curAdaptIndex = (curAdaptIndex + 1) % 2;
		float value = 1f - Mathf.Pow(1f - adaptationSpeed, 30f * Time.deltaTime);
		value = Mathf.Clamp(value, 0.01f, 1f);
		materialAdapt.SetTexture("_CurTex", curTexture);
		materialAdapt.SetVector("_AdaptParams", new Vector4(value, limitMinimum, limitMaximum, 0f));
		Graphics.SetRenderTarget(adaptRenderTex[curAdaptIndex]);
		GL.Clear(clearDepth: false, clearColor: true, Color.black);
		Graphics.Blit(adaptRenderTex[num], adaptRenderTex[curAdaptIndex], materialAdapt);
	}
}
[AddComponentMenu("Image Effects/Bloom and Glow/Glow (Deprecated)")]
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class GlowEffect : MonoBehaviour
{
	public float glowIntensity = 1.5f;

	public int blurIterations = 3;

	public float blurSpread = 0.7f;

	public Color glowTint = new Color(1f, 1f, 1f, 0f);

	public Shader compositeShader;

	private Material m_CompositeMaterial;

	public Shader blurShader;

	private Material m_BlurMaterial;

	public Shader downsampleShader;

	private Material m_DownsampleMaterial;

	protected Material compositeMaterial
	{
		get
		{
			if (m_CompositeMaterial == null)
			{
				m_CompositeMaterial = new Material(compositeShader);
				m_CompositeMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_CompositeMaterial;
		}
	}

	protected Material blurMaterial
	{
		get
		{
			if (m_BlurMaterial == null)
			{
				m_BlurMaterial = new Material(blurShader);
				m_BlurMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_BlurMaterial;
		}
	}

	protected Material downsampleMaterial
	{
		get
		{
			if (m_DownsampleMaterial == null)
			{
				m_DownsampleMaterial = new Material(downsampleShader);
				m_DownsampleMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_DownsampleMaterial;
		}
	}

	protected void OnDisable()
	{
		if ((bool)m_CompositeMaterial)
		{
			UnityEngine.Object.DestroyImmediate(m_CompositeMaterial);
		}
		if ((bool)m_BlurMaterial)
		{
			UnityEngine.Object.DestroyImmediate(m_BlurMaterial);
		}
		if ((bool)m_DownsampleMaterial)
		{
			UnityEngine.Object.DestroyImmediate(m_DownsampleMaterial);
		}
	}

	protected void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
			return;
		}
		if (downsampleShader == null)
		{
			Debug.Log("No downsample shader assigned! Disabling glow.");
			base.enabled = false;
			return;
		}
		if (!blurMaterial.shader.isSupported)
		{
			base.enabled = false;
		}
		if (!compositeMaterial.shader.isSupported)
		{
			base.enabled = false;
		}
		if (!downsampleMaterial.shader.isSupported)
		{
			base.enabled = false;
		}
	}

	public void FourTapCone(RenderTexture source, RenderTexture dest, int iteration)
	{
		float num = 0.5f + (float)iteration * blurSpread;
		Graphics.BlitMultiTap(source, dest, blurMaterial, new Vector2(num, num), new Vector2(0f - num, num), new Vector2(num, 0f - num), new Vector2(0f - num, 0f - num));
	}

	private void DownSample4x(RenderTexture source, RenderTexture dest)
	{
		downsampleMaterial.color = new Color(glowTint.r, glowTint.g, glowTint.b, glowTint.a / 4f);
		Graphics.Blit(source, dest, downsampleMaterial);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		glowIntensity = Mathf.Clamp(glowIntensity, 0f, 10f);
		blurIterations = Mathf.Clamp(blurIterations, 0, 30);
		blurSpread = Mathf.Clamp(blurSpread, 0.5f, 1f);
		int width = source.width / 4;
		int height = source.height / 4;
		RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 0);
		DownSample4x(source, renderTexture);
		float num = Mathf.Clamp01((glowIntensity - 1f) / 4f);
		blurMaterial.color = new Color(1f, 1f, 1f, 0.25f + num);
		for (int i = 0; i < blurIterations; i++)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0);
			FourTapCone(renderTexture, temporary, i);
			RenderTexture.ReleaseTemporary(renderTexture);
			renderTexture = temporary;
		}
		Graphics.Blit(source, destination);
		BlitGlow(renderTexture, destination);
		RenderTexture.ReleaseTemporary(renderTexture);
	}

	public void BlitGlow(RenderTexture source, RenderTexture dest)
	{
		compositeMaterial.color = new Color(1f, 1f, 1f, Mathf.Clamp01(glowIntensity));
		Graphics.Blit(source, dest, compositeMaterial);
	}
}
[AddComponentMenu("Image Effects/Color Adjustments/Grayscale")]
[ExecuteInEditMode]
public class GrayscaleEffect : ImageEffectBase
{
	public Texture textureRamp;

	public float rampOffset;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		base.material.SetTexture("_RampTex", textureRamp);
		base.material.SetFloat("_RampOffset", rampOffset);
		Graphics.Blit(source, destination, base.material);
	}
}
[RequireComponent(typeof(Camera))]
[AddComponentMenu("")]
public class ImageEffectBase : MonoBehaviour
{
	public Shader shader;

	private Material m_Material;

	protected Material material
	{
		get
		{
			if (m_Material == null)
			{
				m_Material = new Material(shader);
				m_Material.hideFlags = HideFlags.HideAndDontSave;
			}
			return m_Material;
		}
	}

	protected virtual void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
		}
		else if (!shader || !shader.isSupported)
		{
			base.enabled = false;
		}
	}

	protected virtual void OnDisable()
	{
		if ((bool)m_Material)
		{
			UnityEngine.Object.DestroyImmediate(m_Material);
		}
	}
}
[AddComponentMenu("")]
public class ImageEffects
{
	public static void RenderDistortion(Material material, RenderTexture source, RenderTexture destination, float angle, Vector2 center, Vector2 radius)
	{
		if (source.texelSize.y < 0f)
		{
			center.y = 1f - center.y;
			angle = 0f - angle;
		}
		Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, angle), Vector3.one);
		material.SetMatrix("_RotationMatrix", matrix);
		material.SetVector("_CenterRadius", new Vector4(center.x, center.y, radius.x, radius.y));
		material.SetFloat("_Angle", angle * ((float)Math.PI / 180f));
		Graphics.Blit(source, destination, material);
	}

	[Obsolete("Use Graphics.Blit(source,dest) instead")]
	public static void Blit(RenderTexture source, RenderTexture dest)
	{
		Graphics.Blit(source, dest);
	}

	[Obsolete("Use Graphics.Blit(source, destination, material) instead")]
	public static void BlitWithMaterial(Material material, RenderTexture source, RenderTexture dest)
	{
		Graphics.Blit(source, dest, material);
	}
}
[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Blur/Motion Blur (Color Accumulation)")]
public class MotionBlur : ImageEffectBase
{
	public float blurAmount = 0.8f;

	public bool extraBlur;

	private RenderTexture accumTexture;

	protected override void Start()
	{
		if (!SystemInfo.supportsRenderTextures)
		{
			base.enabled = false;
		}
		else
		{
			base.Start();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		UnityEngine.Object.DestroyImmediate(accumTexture);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (accumTexture == null || accumTexture.width != source.width || accumTexture.height != source.height)
		{
			UnityEngine.Object.DestroyImmediate(accumTexture);
			accumTexture = new RenderTexture(source.width, source.height, 0);
			accumTexture.hideFlags = HideFlags.HideAndDontSave;
			Graphics.Blit(source, accumTexture);
		}
		if (extraBlur)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(source.width / 4, source.height / 4, 0);
			accumTexture.MarkRestoreExpected();
			Graphics.Blit(accumTexture, temporary);
			Graphics.Blit(temporary, accumTexture);
			RenderTexture.ReleaseTemporary(temporary);
		}
		blurAmount = Mathf.Clamp(blurAmount, 0f, 0.92f);
		base.material.SetTexture("_MainTex", accumTexture);
		base.material.SetFloat("_AccumOrig", 1f - blurAmount);
		accumTexture.MarkRestoreExpected();
		Graphics.Blit(source, accumTexture, base.material);
		Graphics.Blit(accumTexture, destination);
	}
}
[AddComponentMenu("Image Effects/Noise/Noise and Scratches")]
[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class NoiseEffect : MonoBehaviour
{
	public bool monochrome = true;

	private bool rgbFallback;

	public float grainIntensityMin = 0.1f;

	public float grainIntensityMax = 0.2f;

	public float grainSize = 2f;

	public float scratchIntensityMin = 0.05f;

	public float scratchIntensityMax = 0.25f;

	public float scratchFPS = 10f;

	public float scratchJitter = 0.01f;

	public Texture grainTexture;

	public Texture scratchTexture;

	public Shader shaderRGB;

	public Shader shaderYUV;

	private Material m_MaterialRGB;

	private Material m_MaterialYUV;

	private float scratchTimeLeft;

	private float scratchX;

	private float scratchY;

	protected Material material
	{
		get
		{
			if (m_MaterialRGB == null)
			{
				m_MaterialRGB = new Material(shaderRGB);
				m_MaterialRGB.hideFlags = HideFlags.HideAndDontSave;
			}
			if (m_MaterialYUV == null && !rgbFallback)
			{
				m_MaterialYUV = new Material(shaderYUV);
				m_MaterialYUV.hideFlags = HideFlags.HideAndDontSave;
			}
			return (rgbFallback || monochrome) ? m_MaterialRGB : m_MaterialYUV;
		}
	}

	protected void Start()
	{
		if (!SystemInfo.supportsImageEffects)
		{
			base.enabled = false;
		}
		else if (shaderRGB == null || shaderYUV == null)
		{
			Debug.Log("Noise shaders are not set up! Disabling noise effect.");
			base.enabled = false;
		}
		else if (!shaderRGB.isSupported)
		{
			base.enabled = false;
		}
		else if (!shaderYUV.isSupported)
		{
			rgbFallback = true;
		}
	}

	protected void OnDisable()
	{
		if ((bool)m_MaterialRGB)
		{
			UnityEngine.Object.DestroyImmediate(m_MaterialRGB);
		}
		if ((bool)m_MaterialYUV)
		{
			UnityEngine.Object.DestroyImmediate(m_MaterialYUV);
		}
	}

	private void SanitizeParameters()
	{
		grainIntensityMin = Mathf.Clamp(grainIntensityMin, 0f, 5f);
		grainIntensityMax = Mathf.Clamp(grainIntensityMax, 0f, 5f);
		scratchIntensityMin = Mathf.Clamp(scratchIntensityMin, 0f, 5f);
		scratchIntensityMax = Mathf.Clamp(scratchIntensityMax, 0f, 5f);
		scratchFPS = Mathf.Clamp(scratchFPS, 1f, 30f);
		scratchJitter = Mathf.Clamp(scratchJitter, 0f, 1f);
		grainSize = Mathf.Clamp(grainSize, 0.1f, 50f);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		SanitizeParameters();
		if (scratchTimeLeft <= 0f)
		{
			scratchTimeLeft = UnityEngine.Random.value * 2f / scratchFPS;
			scratchX = UnityEngine.Random.value;
			scratchY = UnityEngine.Random.value;
		}
		scratchTimeLeft -= Time.deltaTime;
		Material material = this.material;
		material.SetTexture("_GrainTex", grainTexture);
		material.SetTexture("_ScratchTex", scratchTexture);
		float num = 1f / grainSize;
		material.SetVector("_GrainOffsetScale", new Vector4(UnityEngine.Random.value, UnityEngine.Random.value, (float)Screen.width / (float)grainTexture.width * num, (float)Screen.height / (float)grainTexture.height * num));
		material.SetVector("_ScratchOffsetScale", new Vector4(scratchX + UnityEngine.Random.value * scratchJitter, scratchY + UnityEngine.Random.value * scratchJitter, (float)Screen.width / (float)scratchTexture.width, (float)Screen.height / (float)scratchTexture.height));
		material.SetVector("_Intensity", new Vector4(UnityEngine.Random.Range(grainIntensityMin, grainIntensityMax), UnityEngine.Random.Range(scratchIntensityMin, scratchIntensityMax), 0f, 0f));
		Graphics.Blit(source, destination, material);
	}
}
[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Rendering/Screen Space Ambient Occlusion")]
public class SSAOEffect : MonoBehaviour
{
	public enum SSAOSamples
	{
		Low,
		Medium,
		High
	}

	public float m_Radius = 0.4f;

	public SSAOSamples m_SampleCount = SSAOSamples.Medium;

	public float m_OcclusionIntensity = 1.5f;

	public int m_Blur = 2;

	public int m_Downsampling = 2;

	public float m_OcclusionAttenuation = 1f;

	public float m_MinZ = 0.01f;

	public Shader m_SSAOShader;

	private Material m_SSAOMaterial;

	public Texture2D m_RandomTexture;

	private bool m_Supported;

	private static Material CreateMaterial(Shader shader)
	{
		if (!shader)
		{
			return null;
		}
		Material material = new Material(shader);
		material.hideFlags = HideFlags.HideAndDontSave;
		return material;
	}

	private static void DestroyMaterial(Material mat)
	{
		if ((bool)mat)
		{
			UnityEngine.Object.DestroyImmediate(mat);
			mat = null;
		}
	}

	private void OnDisable()
	{
		DestroyMaterial(m_SSAOMaterial);
	}

	private void Start()
	{
		if (!SystemInfo.supportsImageEffects || !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
		{
			m_Supported = false;
			base.enabled = false;
			return;
		}
		CreateMaterials();
		if (!m_SSAOMaterial || m_SSAOMaterial.passCount != 5)
		{
			m_Supported = false;
			base.enabled = false;
		}
		else
		{
			m_Supported = true;
		}
	}

	private void OnEnable()
	{
		base.camera.depthTextureMode |= DepthTextureMode.DepthNormals;
	}

	private void CreateMaterials()
	{
		if (!m_SSAOMaterial && m_SSAOShader.isSupported)
		{
			m_SSAOMaterial = CreateMaterial(m_SSAOShader);
			m_SSAOMaterial.SetTexture("_RandomTexture", m_RandomTexture);
		}
	}

	[ImageEffectOpaque]
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!m_Supported || !m_SSAOShader.isSupported)
		{
			base.enabled = false;
			return;
		}
		CreateMaterials();
		m_Downsampling = Mathf.Clamp(m_Downsampling, 1, 6);
		m_Radius = Mathf.Clamp(m_Radius, 0.05f, 1f);
		m_MinZ = Mathf.Clamp(m_MinZ, 1E-05f, 0.5f);
		m_OcclusionIntensity = Mathf.Clamp(m_OcclusionIntensity, 0.5f, 4f);
		m_OcclusionAttenuation = Mathf.Clamp(m_OcclusionAttenuation, 0.2f, 2f);
		m_Blur = Mathf.Clamp(m_Blur, 0, 4);
		RenderTexture renderTexture = RenderTexture.GetTemporary(source.width / m_Downsampling, source.height / m_Downsampling, 0);
		float fieldOfView = base.camera.fieldOfView;
		float farClipPlane = base.camera.farClipPlane;
		float num = Mathf.Tan(fieldOfView * ((float)Math.PI / 180f) * 0.5f) * farClipPlane;
		float x = num * base.camera.aspect;
		m_SSAOMaterial.SetVector("_FarCorner", new Vector3(x, num, farClipPlane));
		int num2;
		int num3;
		if ((bool)m_RandomTexture)
		{
			num2 = m_RandomTexture.width;
			num3 = m_RandomTexture.height;
		}
		else
		{
			num2 = 1;
			num3 = 1;
		}
		m_SSAOMaterial.SetVector("_NoiseScale", new Vector3((float)renderTexture.width / (float)num2, (float)renderTexture.height / (float)num3, 0f));
		m_SSAOMaterial.SetVector("_Params", new Vector4(m_Radius, m_MinZ, 1f / m_OcclusionAttenuation, m_OcclusionIntensity));
		bool flag = m_Blur > 0;
		Graphics.Blit((!flag) ? source : null, renderTexture, m_SSAOMaterial, (int)m_SampleCount);
		if (flag)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0);
			m_SSAOMaterial.SetVector("_TexelOffsetScale", new Vector4((float)m_Blur / (float)source.width, 0f, 0f, 0f));
			m_SSAOMaterial.SetTexture("_SSAO", renderTexture);
			Graphics.Blit(null, temporary, m_SSAOMaterial, 3);
			RenderTexture.ReleaseTemporary(renderTexture);
			RenderTexture temporary2 = RenderTexture.GetTemporary(source.width, source.height, 0);
			m_SSAOMaterial.SetVector("_TexelOffsetScale", new Vector4(0f, (float)m_Blur / (float)source.height, 0f, 0f));
			m_SSAOMaterial.SetTexture("_SSAO", temporary);
			Graphics.Blit(source, temporary2, m_SSAOMaterial, 3);
			RenderTexture.ReleaseTemporary(temporary);
			renderTexture = temporary2;
		}
		m_SSAOMaterial.SetTexture("_SSAO", renderTexture);
		Graphics.Blit(source, destination, m_SSAOMaterial, 4);
		RenderTexture.ReleaseTemporary(renderTexture);
	}
}
[AddComponentMenu("Image Effects/Color Adjustments/Sepia Tone")]
[ExecuteInEditMode]
public class SepiaToneEffect : ImageEffectBase
{
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination, base.material);
	}
}
[AddComponentMenu("Image Effects/Displacement/Twirl")]
[ExecuteInEditMode]
public class TwirlEffect : ImageEffectBase
{
	public Vector2 radius = new Vector2(0.3f, 0.3f);

	public float angle = 50f;

	public Vector2 center = new Vector2(0.5f, 0.5f);

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		ImageEffects.RenderDistortion(base.material, source, destination, angle, center, radius);
	}
}
[AddComponentMenu("Image Effects/Displacement/Vortex")]
[ExecuteInEditMode]
public class VortexEffect : ImageEffectBase
{
	public Vector2 radius = new Vector2(0.4f, 0.4f);

	public float angle = 50f;

	public Vector2 center = new Vector2(0.5f, 0.5f);

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		ImageEffects.RenderDistortion(base.material, source, destination, angle, center, radius);
	}
}
[ExecuteInEditMode]
public class Water : MonoBehaviour
{
	public enum WaterMode
	{
		Simple,
		Reflective,
		Refractive
	}

	public WaterMode m_WaterMode = WaterMode.Refractive;

	public bool m_DisablePixelLights = true;

	public int m_TextureSize = 256;

	public float m_ClipPlaneOffset = 0.07f;

	public LayerMask m_ReflectLayers = -1;

	public LayerMask m_RefractLayers = -1;

	private Hashtable m_ReflectionCameras = new Hashtable();

	private Hashtable m_RefractionCameras = new Hashtable();

	private RenderTexture m_ReflectionTexture;

	private RenderTexture m_RefractionTexture;

	private WaterMode m_HardwareWaterSupport = WaterMode.Refractive;

	private int m_OldReflectionTextureSize;

	private int m_OldRefractionTextureSize;

	private static bool s_InsideWater;

	public void OnWillRenderObject()
	{
		if (!base.enabled || !base.renderer || !base.renderer.sharedMaterial || !base.renderer.enabled)
		{
			return;
		}
		Camera current = Camera.current;
		if ((bool)current && !s_InsideWater)
		{
			s_InsideWater = true;
			m_HardwareWaterSupport = FindHardwareWaterSupport();
			WaterMode waterMode = GetWaterMode();
			CreateWaterObjects(current, out var reflectionCamera, out var refractionCamera);
			Vector3 position = base.transform.position;
			Vector3 up = base.transform.up;
			int pixelLightCount = QualitySettings.pixelLightCount;
			if (m_DisablePixelLights)
			{
				QualitySettings.pixelLightCount = 0;
			}
			UpdateCameraModes(current, reflectionCamera);
			UpdateCameraModes(current, refractionCamera);
			if (waterMode >= WaterMode.Reflective)
			{
				float w = 0f - Vector3.Dot(up, position) - m_ClipPlaneOffset;
				Vector4 plane = new Vector4(up.x, up.y, up.z, w);
				Matrix4x4 reflectionMat = Matrix4x4.zero;
				CalculateReflectionMatrix(ref reflectionMat, plane);
				Vector3 position2 = current.transform.position;
				Vector3 position3 = reflectionMat.MultiplyPoint(position2);
				reflectionCamera.worldToCameraMatrix = current.worldToCameraMatrix * reflectionMat;
				Vector4 clipPlane = CameraSpacePlane(reflectionCamera, position, up, 1f);
				Matrix4x4 projection = current.projectionMatrix;
				CalculateObliqueMatrix(ref projection, clipPlane);
				reflectionCamera.projectionMatrix = projection;
				reflectionCamera.cullingMask = -17 & m_ReflectLayers.value;
				reflectionCamera.targetTexture = m_ReflectionTexture;
				GL.SetRevertBackfacing(revertBackFaces: true);
				reflectionCamera.transform.position = position3;
				Vector3 eulerAngles = current.transform.eulerAngles;
				reflectionCamera.transform.eulerAngles = new Vector3(0f - eulerAngles.x, eulerAngles.y, eulerAngles.z);
				reflectionCamera.Render();
				reflectionCamera.transform.position = position2;
				GL.SetRevertBackfacing(revertBackFaces: false);
				base.renderer.sharedMaterial.SetTexture("_ReflectionTex", m_ReflectionTexture);
			}
			if (waterMode >= WaterMode.Refractive)
			{
				refractionCamera.worldToCameraMatrix = current.worldToCameraMatrix;
				Vector4 clipPlane2 = CameraSpacePlane(refractionCamera, position, up, -1f);
				Matrix4x4 projection2 = current.projectionMatrix;
				CalculateObliqueMatrix(ref projection2, clipPlane2);
				refractionCamera.projectionMatrix = projection2;
				refractionCamera.cullingMask = -17 & m_RefractLayers.value;
				refractionCamera.targetTexture = m_RefractionTexture;
				refractionCamera.transform.position = current.transform.position;
				refractionCamera.transform.rotation = current.transform.rotation;
				refractionCamera.Render();
				base.renderer.sharedMaterial.SetTexture("_RefractionTex", m_RefractionTexture);
			}
			if (m_DisablePixelLights)
			{
				QualitySettings.pixelLightCount = pixelLightCount;
			}
			switch (waterMode)
			{
			case WaterMode.Simple:
				Shader.EnableKeyword("WATER_SIMPLE");
				Shader.DisableKeyword("WATER_REFLECTIVE");
				Shader.DisableKeyword("WATER_REFRACTIVE");
				break;
			case WaterMode.Reflective:
				Shader.DisableKeyword("WATER_SIMPLE");
				Shader.EnableKeyword("WATER_REFLECTIVE");
				Shader.DisableKeyword("WATER_REFRACTIVE");
				break;
			case WaterMode.Refractive:
				Shader.DisableKeyword("WATER_SIMPLE");
				Shader.DisableKeyword("WATER_REFLECTIVE");
				Shader.EnableKeyword("WATER_REFRACTIVE");
				break;
			}
			s_InsideWater = false;
		}
	}

	private void OnDisable()
	{
		if ((bool)m_ReflectionTexture)
		{
			UnityEngine.Object.DestroyImmediate(m_ReflectionTexture);
			m_ReflectionTexture = null;
		}
		if ((bool)m_RefractionTexture)
		{
			UnityEngine.Object.DestroyImmediate(m_RefractionTexture);
			m_RefractionTexture = null;
		}
		foreach (DictionaryEntry reflectionCamera in m_ReflectionCameras)
		{
			UnityEngine.Object.DestroyImmediate(((Camera)reflectionCamera.Value).gameObject);
		}
		m_ReflectionCameras.Clear();
		foreach (DictionaryEntry refractionCamera in m_RefractionCameras)
		{
			UnityEngine.Object.DestroyImmediate(((Camera)refractionCamera.Value).gameObject);
		}
		m_RefractionCameras.Clear();
	}

	private void Update()
	{
		if ((bool)base.renderer)
		{
			Material sharedMaterial = base.renderer.sharedMaterial;
			if ((bool)sharedMaterial)
			{
				Vector4 vector = sharedMaterial.GetVector("WaveSpeed");
				float num = sharedMaterial.GetFloat("_WaveScale");
				Vector4 vector2 = new Vector4(num, num, num * 0.4f, num * 0.45f);
				double num2 = (double)Time.timeSinceLevelLoad / 20.0;
				Vector4 vector3 = new Vector4((float)Math.IEEERemainder((double)(vector.x * vector2.x) * num2, 1.0), (float)Math.IEEERemainder((double)(vector.y * vector2.y) * num2, 1.0), (float)Math.IEEERemainder((double)(vector.z * vector2.z) * num2, 1.0), (float)Math.IEEERemainder((double)(vector.w * vector2.w) * num2, 1.0));
				sharedMaterial.SetVector("_WaveOffset", vector3);
				sharedMaterial.SetVector("_WaveScale4", vector2);
				Vector3 size = base.renderer.bounds.size;
				Matrix4x4 matrix = Matrix4x4.TRS(s: new Vector3(size.x * vector2.x, size.z * vector2.y, 1f), pos: new Vector3(vector3.x, vector3.y, 0f), q: Quaternion.identity);
				sharedMaterial.SetMatrix("_WaveMatrix", matrix);
				matrix = Matrix4x4.TRS(s: new Vector3(size.x * vector2.z, size.z * vector2.w, 1f), pos: new Vector3(vector3.z, vector3.w, 0f), q: Quaternion.identity);
				sharedMaterial.SetMatrix("_WaveMatrix2", matrix);
			}
		}
	}

	private void UpdateCameraModes(Camera src, Camera dest)
	{
		if (dest == null)
		{
			return;
		}
		dest.clearFlags = src.clearFlags;
		dest.backgroundColor = src.backgroundColor;
		if (src.clearFlags == CameraClearFlags.Skybox)
		{
			Skybox skybox = src.GetComponent(typeof(Skybox)) as Skybox;
			Skybox skybox2 = dest.GetComponent(typeof(Skybox)) as Skybox;
			if (!skybox || !skybox.material)
			{
				skybox2.enabled = false;
			}
			else
			{
				skybox2.enabled = true;
				skybox2.material = skybox.material;
			}
		}
		dest.farClipPlane = src.farClipPlane;
		dest.nearClipPlane = src.nearClipPlane;
		dest.orthographic = src.orthographic;
		dest.fieldOfView = src.fieldOfView;
		dest.aspect = src.aspect;
		dest.orthographicSize = src.orthographicSize;
	}

	private void CreateWaterObjects(Camera currentCamera, out Camera reflectionCamera, out Camera refractionCamera)
	{
		WaterMode waterMode = GetWaterMode();
		reflectionCamera = null;
		refractionCamera = null;
		if (waterMode >= WaterMode.Reflective)
		{
			if (!m_ReflectionTexture || m_OldReflectionTextureSize != m_TextureSize)
			{
				if ((bool)m_ReflectionTexture)
				{
					UnityEngine.Object.DestroyImmediate(m_ReflectionTexture);
				}
				m_ReflectionTexture = new RenderTexture(m_TextureSize, m_TextureSize, 16);
				m_ReflectionTexture.name = "__WaterReflection" + GetInstanceID();
				m_ReflectionTexture.isPowerOfTwo = true;
				m_ReflectionTexture.hideFlags = HideFlags.DontSave;
				m_OldReflectionTextureSize = m_TextureSize;
			}
			reflectionCamera = m_ReflectionCameras[currentCamera] as Camera;
			if (!reflectionCamera)
			{
				GameObject gameObject = new GameObject("Water Refl Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox));
				reflectionCamera = gameObject.camera;
				reflectionCamera.enabled = false;
				reflectionCamera.transform.position = base.transform.position;
				reflectionCamera.transform.rotation = base.transform.rotation;
				reflectionCamera.gameObject.AddComponent("FlareLayer");
				gameObject.hideFlags = HideFlags.HideAndDontSave;
				m_ReflectionCameras[currentCamera] = reflectionCamera;
			}
		}
		if (waterMode < WaterMode.Refractive)
		{
			return;
		}
		if (!m_RefractionTexture || m_OldRefractionTextureSize != m_TextureSize)
		{
			if ((bool)m_RefractionTexture)
			{
				UnityEngine.Object.DestroyImmediate(m_RefractionTexture);
			}
			m_RefractionTexture = new RenderTexture(m_TextureSize, m_TextureSize, 16);
			m_RefractionTexture.name = "__WaterRefraction" + GetInstanceID();
			m_RefractionTexture.isPowerOfTwo = true;
			m_RefractionTexture.hideFlags = HideFlags.DontSave;
			m_OldRefractionTextureSize = m_TextureSize;
		}
		refractionCamera = m_RefractionCameras[currentCamera] as Camera;
		if (!refractionCamera)
		{
			GameObject gameObject2 = new GameObject("Water Refr Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox));
			refractionCamera = gameObject2.camera;
			refractionCamera.enabled = false;
			refractionCamera.transform.position = base.transform.position;
			refractionCamera.transform.rotation = base.transform.rotation;
			refractionCamera.gameObject.AddComponent("FlareLayer");
			gameObject2.hideFlags = HideFlags.HideAndDontSave;
			m_RefractionCameras[currentCamera] = refractionCamera;
		}
	}

	private WaterMode GetWaterMode()
	{
		if (m_HardwareWaterSupport < m_WaterMode)
		{
			return m_HardwareWaterSupport;
		}
		return m_WaterMode;
	}

	private WaterMode FindHardwareWaterSupport()
	{
		if (!SystemInfo.supportsRenderTextures || !base.renderer)
		{
			return WaterMode.Simple;
		}
		Material sharedMaterial = base.renderer.sharedMaterial;
		if (!sharedMaterial)
		{
			return WaterMode.Simple;
		}
		string text = sharedMaterial.GetTag("WATERMODE", searchFallbacks: false);
		if (text == "Refractive")
		{
			return WaterMode.Refractive;
		}
		if (text == "Reflective")
		{
			return WaterMode.Reflective;
		}
		return WaterMode.Simple;
	}

	private static float sgn(float a)
	{
		if (a > 0f)
		{
			return 1f;
		}
		if (a < 0f)
		{
			return -1f;
		}
		return 0f;
	}

	private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
	{
		Vector3 v = pos + normal * m_ClipPlaneOffset;
		Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
		Vector3 lhs = worldToCameraMatrix.MultiplyPoint(v);
		Vector3 rhs = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
		return new Vector4(rhs.x, rhs.y, rhs.z, 0f - Vector3.Dot(lhs, rhs));
	}

	private static void CalculateObliqueMatrix(ref Matrix4x4 projection, Vector4 clipPlane)
	{
		Vector4 b = projection.inverse * new Vector4(sgn(clipPlane.x), sgn(clipPlane.y), 1f, 1f);
		Vector4 vector = clipPlane * (2f / Vector4.Dot(clipPlane, b));
		projection[2] = vector.x - projection[3];
		projection[6] = vector.y - projection[7];
		projection[10] = vector.z - projection[11];
		projection[14] = vector.w - projection[15];
	}

	private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
	{
		reflectionMat.m00 = 1f - 2f * plane[0] * plane[0];
		reflectionMat.m01 = -2f * plane[0] * plane[1];
		reflectionMat.m02 = -2f * plane[0] * plane[2];
		reflectionMat.m03 = -2f * plane[3] * plane[0];
		reflectionMat.m10 = -2f * plane[1] * plane[0];
		reflectionMat.m11 = 1f - 2f * plane[1] * plane[1];
		reflectionMat.m12 = -2f * plane[1] * plane[2];
		reflectionMat.m13 = -2f * plane[3] * plane[1];
		reflectionMat.m20 = -2f * plane[2] * plane[0];
		reflectionMat.m21 = -2f * plane[2] * plane[1];
		reflectionMat.m22 = 1f - 2f * plane[2] * plane[2];
		reflectionMat.m23 = -2f * plane[3] * plane[2];
		reflectionMat.m30 = 0f;
		reflectionMat.m31 = 0f;
		reflectionMat.m32 = 0f;
		reflectionMat.m33 = 1f;
	}
}
