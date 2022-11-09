using System;
using MelonLoader;
using UnityEngine;

namespace TabExtension.UI;

public class LayoutListener : MonoBehaviour
{
	private bool lastState;

	public LayoutListener(IntPtr value)
		: base(value)
	{
	}

	internal void Awake()
	{
		lastState = base.gameObject.activeSelf;
		MelonCoroutines.Start(TabLayout.Instance.RecalculateLayout());
		TabLayout.Instance.ApplySorting();
	}

	internal void OnEnable()
	{
		if (!lastState)
		{
			MelonCoroutines.Start(TabLayout.Instance.RecalculateLayout());
		}
		lastState = base.gameObject.activeSelf;
	}

	internal void OnDisable()
	{
		if (lastState && base.transform.parent.gameObject.activeInHierarchy)
		{
			MelonCoroutines.Start(TabLayout.Instance.RecalculateLayout());
		}
		lastState = base.gameObject.activeSelf;
	}
}
