using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MoreMountains.HighroadEngine
{	
	/// <summary>
	/// This class allows keyboard navigation (up and down arrows by default) through a set of buttons
	/// </summary>
	public class ButtonNavigation : MonoBehaviour
	{
	    /// the list of buttons the player will be able to navigate through
	    public List<Button> ButtonList;
		/// the time in second before each new keystroke gets registered, to prevent ultra fast movement between buttons
	    public float KeyboardSpeed = 0.3f;

	    public enum AxisChoices { Vertical,Horizontal }
	    public AxisChoices Axis;

		public string VerticalAxisInputName;

		public string HorizontalAxisInputName;

	    public bool AutoPressButtonOnFocus=false;
	    protected int _selectedIndex = 0;
	    protected float _timeCounter=0f;
	    protected bool _buttonPressRequired = true;

	    /// <summary>
	    /// Returns a new current index based on the direction the user has input
	    /// </summary>
	    /// <returns>The selection.</returns>
	    /// <param name="buttonList">Button list.</param>
	    /// <param name="currentIndex">Current index.</param>
	    /// <param name="direction">Direction.</param>
	    protected virtual int MenuSelection(List<Button>  buttonList, int currentIndex, string direction)
	    {
			
	        if (direction == "lower")
	        {
	            if (currentIndex == buttonList.Count - 1)
	            {
	                currentIndex = 0;
	            }
	            else
	            {
	                currentIndex += 1;
	            }
			}
			if (direction == "higher")
			{
				if (currentIndex == 0)
				{
					currentIndex = buttonList.Count - 1;
				}
				else
				{
					currentIndex -= 1;
				}
			}
			buttonList[currentIndex].Select();
	        return currentIndex;
	    }

		/// <summary>
		/// Every frame, checks for input and triggers the MoveMenu method if needed
		/// </summary>
	    protected virtual void Update()
	    {
			string selectedAxis = Axis == AxisChoices.Horizontal ? HorizontalAxisInputName : VerticalAxisInputName;

			if (Input.GetAxisRaw(selectedAxis) <0)
	        {
	            MoveMenu("lower");
	        }

			if (Input.GetAxisRaw(selectedAxis) > 0)
	        {
	            MoveMenu("higher");
	        }
	    }

		/// <summary>
		/// Prevents fast keystrokes and triggers menu movement.
		/// </summary>
		/// <param name="direction">Direction.</param>
	    public virtual void MoveMenu(string direction)
	    {
	        if (Time.realtimeSinceStartup - _timeCounter > KeyboardSpeed)
	        {
	            _timeCounter = Time.realtimeSinceStartup;
	            _selectedIndex = MenuSelection(ButtonList, _selectedIndex, direction);
	            _buttonPressRequired = true;
	        }
	    }
	}
}
