using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MoreMountains.Tools
{	
	[RequireComponent(typeof(Rect))]
	[RequireComponent(typeof(CanvasGroup))]
	/// <summary>
	/// Add this component to a GUI Image to have it act as a button. 
	/// Bind pressed down, pressed continually and released actions to it from the inspector
	/// Handles mouse and multi touch
	/// </summary>
	public class MMTouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		[Header("Binding")]
		/// The method(s) to call when the button gets pressed down
		public UnityEvent ButtonPressedFirstTime;
		/// The method(s) to call when the button gets released
		public UnityEvent ButtonReleased;
		/// The method(s) to call while the button is being pressed
		public UnityEvent ButtonPressed;

		[Header("Pressed Behaviour")]
		[Information("Here you can set the opacity of the button when it's pressed. Useful for visual feedback.",InformationAttribute.InformationType.Info,false)]
		/// the new opacity to apply to the canvas group when the button is pressed
		public float PressedOpacity = 0.5f;

	    protected bool _zonePressed = false;
	    protected CanvasGroup _canvasGroup;
	    protected float _initialOpacity;

	    /// <summary>
	    /// On Start, we get our canvasgroup and set our initial alpha
	    /// </summary>
	    protected virtual void Start()
	    {
			_canvasGroup = GetComponent<CanvasGroup>();
			if (_canvasGroup!=null)
			{
				_initialOpacity = _canvasGroup.alpha;
			}
	    }

		/// <summary>
		/// Every frame, if the touch zone is pressed, we trigger the OnPointerPressed method, to detect continuous press
		/// </summary>
		protected virtual void Update()
	    {
	        if (_zonePressed)
	        {
	            OnPointerPressed();
	        }
	    }

		/// <summary>
		/// Triggers the bound pointer down action
		/// </summary>
		public virtual void OnPointerDown(PointerEventData data)
	    {
	        _zonePressed = true;
			if (_canvasGroup!=null)
			{
				_canvasGroup.alpha=PressedOpacity;
			}
			if (ButtonPressedFirstTime!=null)
	        {
				ButtonPressedFirstTime.Invoke();
	        }
	    }

		/// <summary>
		/// Triggers the bound pointer up action
		/// </summary>
		public virtual void OnPointerUp(PointerEventData data)
		{
	        _zonePressed = false;
			if (_canvasGroup!=null)
			{
				_canvasGroup.alpha=_initialOpacity;
			}
			if (ButtonReleased != null)
			{
				ButtonReleased.Invoke();
	        }
	    }

		/// <summary>
		/// Triggers the bound pointer pressed action
		/// </summary>
		public virtual void OnPointerPressed()
	    {
			if (ButtonPressed != null)
			{
				ButtonPressed.Invoke();
	        }
	    }
	}
}
