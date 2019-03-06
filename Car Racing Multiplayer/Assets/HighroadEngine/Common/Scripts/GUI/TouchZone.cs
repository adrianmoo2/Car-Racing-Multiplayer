using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MoreMountains.Tools;

namespace MoreMountains.HighroadEngine
{	
	[RequireComponent(typeof(Rect))]
	/// <summary>
	/// Add this component to a GUI button to have it act as a proxy for a certain action on touch devices.
	/// Detects press down, press up, and continuous press. 
	/// These are really basic mobile/touch controls. I believe that for infinite runners they're sufficient. 
	/// </summary>
	public class TouchZone : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		/// the list of possible bindable actions
	    public enum TouchZoneActions { MainAction,Left,Right,Up,Down }
		/// the binding of a possible action
	    public TouchZoneActions TouchZoneBinding;
		/// should the component send an event when the zone is pressed down ?
		public bool SendDownEvent = true;
		/// should the component send an event when the zone is pressed up ?
		public bool SendUpEvent = true;
		/// should the component send an event when the zone is being pressed ?
	    public bool SendPressedEvent = false;

	    protected bool _zonePressed = false;
	    protected string OnPointerDownAction;
	    protected string OnPointerUpAction;
	    protected string OnPointerPressedAction;

		/// <summary>
		/// Triggered at initialization, binds the touch zone to input manager's methods
		/// </summary>
	    protected void Start ()
	    {   
	        // for each possible binding, we set the corresponding method
	        switch( TouchZoneBinding)
	        {
	            case TouchZoneActions.MainAction:
	                OnPointerDownAction = "MainActionButtonDown";
	                OnPointerUpAction = "MainActionButtonUp";
	                OnPointerPressedAction = "MainActionButtonPressed";
	                break;
	            case TouchZoneActions.Left:
	                OnPointerDownAction = "LeftButtonDown";
	                OnPointerUpAction = "LeftButtonUp";
	                OnPointerPressedAction = "LeftButtonPressed";
	                break;
	            case TouchZoneActions.Right:
	                OnPointerDownAction = "RightButtonDown";
	                OnPointerUpAction = "RightButtonUp";
	                OnPointerPressedAction = "RightButtonPressed";
	                break;
	            case TouchZoneActions.Up:
	                OnPointerDownAction = "UpButtonDown";
	                OnPointerUpAction = "UpButtonUp";
	                OnPointerPressedAction = "UpButtonPressed";
	                break;
	            case TouchZoneActions.Down:
	                OnPointerDownAction = "DownButtonDown";
	                OnPointerUpAction = "DownButtonUp";
	                OnPointerPressedAction = "DownButtonPressed";
	                break;
	        }

	        if (!SendDownEvent) { OnPointerDownAction = null; }
	        if (!SendUpEvent) { OnPointerUpAction = null; }
	        if (!SendPressedEvent) { OnPointerPressedAction = null; }
		}

		/// <summary>
		/// Every frame, if the touch zone is pressed, we trigger the OnPointerPressed method, to detect continuous press
		/// </summary>
	    protected void Update()
	    {
	        if (_zonePressed)
	        {
	            OnPointerPressed();
	        }
	    }

		/// <summary>
		/// Triggers the bound pointer down action
		/// </summary>
	    public void OnPointerDown(PointerEventData data)
	    {
	        _zonePressed = true;
	        if (OnPointerDownAction!=null)
	            InputManager.Instance.SendMessage(OnPointerDownAction);
	    }

		/// <summary>
		/// Triggers the bound pointer up action
		/// </summary>
	    public void OnPointerUp(PointerEventData data)
	    {
	        _zonePressed = false;
	        if (OnPointerUpAction != null)
	            InputManager.Instance.SendMessage(OnPointerUpAction);
	    }

		/// <summary>
		/// Triggers the bound pointer pressed action
		/// </summary>
	    public void OnPointerPressed()
	    {
	        if (OnPointerPressedAction != null)
	            InputManager.Instance.SendMessage(OnPointerPressedAction);
	    }
	}
}
