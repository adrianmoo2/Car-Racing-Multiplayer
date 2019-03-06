using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace MoreMountains.Tools
{	
	/// <summary>
	/// Various static methods used throughout the Infinite Runner Engine and the Corgi Engine.
	/// </summary>

	public static class MMAnimator 
	{
		/// <summary>
		/// Updates the animator bool.
		/// </summary>
		/// <param name="animator">Animator.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">If set to <c>true</c> value.</param>
		public static void UpdateAnimatorBool(Animator animator, string parameterName,bool value)
		{
			if (animator.HasParameterOfType (parameterName, AnimatorControllerParameterType.Bool))
				animator.SetBool(parameterName,value);
		}

		public static void UpdateAnimatorTrigger(Animator animator, string parameterName)
		{
			if (animator.HasParameterOfType (parameterName, AnimatorControllerParameterType.Trigger))
				animator.SetTrigger(parameterName);
		}

		/// <summary>
		/// Triggers an animator trigger.
		/// </summary>
		/// <param name="animator">Animator.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">If set to <c>true</c> value.</param>
		public static void SetAnimatorTrigger(Animator animator, string parameterName)
		{
			if (animator.HasParameterOfType (parameterName, AnimatorControllerParameterType.Trigger))
				animator.SetTrigger(parameterName);
		}
		
		/// <summary>
		/// Updates the animator float.
		/// </summary>
		/// <param name="animator">Animator.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">Value.</param>
		public static void UpdateAnimatorFloat(Animator animator, string parameterName,float value)
		{
			if (animator.HasParameterOfType (parameterName, AnimatorControllerParameterType.Float))
				animator.SetFloat(parameterName,value);
		}
		
		/// <summary>
		/// Updates the animator integer.
		/// </summary>
		/// <param name="animator">Animator.</param>
		/// <param name="parameterName">Parameter name.</param>
		/// <param name="value">Value.</param>
		public static void UpdateAnimatorInteger(Animator animator, string parameterName,int value)
		{
			if (animator.HasParameterOfType (parameterName, AnimatorControllerParameterType.Int))
				animator.SetInteger(parameterName,value);
		}	   
	}
}
