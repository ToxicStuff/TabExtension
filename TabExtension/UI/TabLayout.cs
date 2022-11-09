using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Il2CppSystem;
using Il2CppSystem.Collections;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using TabExtension.Config;
using UnhollowerBaseLib;
using UnhollowerBaseLib.Attributes;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.UI;
using VRC.UI.Core.Styles;
using VRC.UI.Elements;
using VRC.UI.Elements.Controls;

namespace TabExtension.UI;

public class TabLayout : MonoBehaviour
{
	public static TabLayout Instance;

	private GameObject quickMenu;

	private GameObject layout;

	private RectTransform tooltipRect;

	private RectTransform backgroundRect;

	private BoxCollider menuCollider;

	private System.Collections.Generic.List<RectTransform> uixObjects;

	private System.Collections.Generic.Dictionary<string, int> tabSorting;

	private System.Collections.Generic.List<string> defaultSorting;

	private MenuStateController menuStateController;

	private bool useStyletor;

	public TabLayout(System.IntPtr value)
		: base(value)
	{
		Instance = this;
		tabSorting = Configuration.Load();
		if (tabSorting == null)
		{
			tabSorting = new System.Collections.Generic.Dictionary<string, int>();
		}
		Configuration.TabSorting.OnValueChanged += delegate(bool oldValue, bool newValue)
		{
			if (newValue)
			{
				tabSorting = Configuration.Load();
				if (tabSorting == null)
				{
					tabSorting = new System.Collections.Generic.Dictionary<string, int>();
				}
				ApplySorting();
			}
			else
			{
				ApplySorting(applyDefault: true);
			}
		};
		Configuration.TabsPerRow.OnValueChanged += delegate
		{
			MelonCoroutines.Start(RecalculateLayout());
		};
		quickMenu = TabExtensionMod.UserInterface.transform.Find("Canvas_QuickMenu(Clone)").gameObject;
		layout = quickMenu.transform.Find("CanvasGroup/Container/Window/Page_Buttons_QM/HorizontalLayoutGroup").gameObject;
		tooltipRect = quickMenu.transform.Find("CanvasGroup/Container/Window/ToolTipPanel").GetComponent<RectTransform>();
		UnityEngine.Object.DestroyImmediate(layout.GetComponent<HorizontalLayoutGroup>());
		GameObject background = quickMenu.transform.Find("CanvasGroup/Container/Window/Page_Buttons_QM/HorizontalLayoutGroup/Background_QM_PagePanel").gameObject;
		background.SetActive(Configuration.TabBackground.Value);
		Configuration.TabBackground.OnValueChanged += delegate(bool oldValue, bool newValue)
		{
			background.SetActive(newValue);
		};
		if (MelonHandler.Mods.Any((MelonMod mod) => mod.Info.Name.Equals("Styletor")))
		{
			useStyletor = true;
			StyleElement component = background.GetComponent<StyleElement>();
			component.field_Public_String_1 = "TabBottom";
			MelonLogger.Msg("Found Styletor. Style tag was applied to the tab background.");
		}
		else
		{
			Sprite sprite = null;
			StyleEngine component2 = quickMenu.GetComponent<StyleEngine>();
			Il2CppSystem.Collections.Generic.List<StyleResource.Resource> resources = component2.field_Public_StyleResource_0.resources;
			for (int i = 0; i < resources.Count; i++)
			{
				if (resources[i].obj.name.Equals("Page_Tab_Backdrop") && resources[i].obj.GetIl2CppType() == UnhollowerRuntimeLib.Il2CppType.Of<Sprite>())
				{
					sprite = resources[i].obj.Cast<Sprite>();
				}
			}
			if (sprite != null)
			{
				MelonLogger.Msg("Found sprite: " + sprite.name);
			}
			else
			{
				MelonLogger.Warning("Unable to find the Page_Tab_Backdrop sprite.");
			}
			Image component3 = background.GetComponent<Image>();
			component3.sprite = sprite;
			component3.color = new Color(1f, 1f, 1f, 0.8f);
		}
		backgroundRect = background.GetComponent<RectTransform>();
		backgroundRect.anchoredPosition = new Vector2(0f, -64f);
		backgroundRect.sizeDelta = new Vector2(950f, 128f);
		menuStateController = quickMenu.GetComponent<MenuStateController>();
	}

	internal void OnEnable()
	{
		MelonCoroutines.Start(RecalculateLayout());
	}

	[HideFromIl2Cpp]
	public System.Collections.IEnumerator RecalculateLayout()
	{
		if (uixObjects == null)
		{
			uixObjects = new System.Collections.Generic.List<RectTransform>();
			Il2CppSystem.Collections.IEnumerator enumerator = quickMenu.transform.Find("CanvasGroup/Container").transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Il2CppSystem.Object t2 = enumerator.Current;
					Transform child2 = t2.Cast<Transform>();
					if (child2.gameObject.name.StartsWith("QuickMenuExpandoRoot"))
					{
						uixObjects.Add(child2.Find("Content").GetComponent<RectTransform>());
					}
				}
			}
			finally
			{
				if (enumerator is System.IDisposable disposable)
				{
					disposable.Dispose();
				}
			}
		}
		if (menuCollider == null)
		{
			yield return null;
			yield return null;
			yield return null;
			menuCollider = quickMenu.transform.Find("CanvasGroup/Container/Window/Page_Buttons_QM").GetComponent<BoxCollider>();
			Il2CppSystem.Collections.IEnumerator enumerator2 = base.transform.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					Il2CppSystem.Object t3 = enumerator2.Current;
					Transform child3 = t3.Cast<Transform>();
					if (child3.gameObject.name != "Background_QM_PagePanel")
					{
						child3.gameObject.AddComponent<LayoutListener>();
					}
				}
			}
			finally
			{
				if (enumerator2 is System.IDisposable disposable2)
				{
					disposable2.Dispose();
				}
			}
		}
		System.Collections.Generic.List<Transform> childs = new System.Collections.Generic.List<Transform>();
		for (int j = 0; j < base.transform.childCount; j++)
		{
			Transform t = base.transform.GetChild(j);
			Transform child = t.Cast<Transform>();
			if (child.gameObject.activeSelf && child.gameObject.name != "Background_QM_PagePanel")
			{
				childs.Add(child);
			}
		}
		int pivotX = 0;
		for (int i = 0; i < childs.Count; i++)
		{
			int y = i / Configuration.TabsPerRow.Value;
			int x = i - y * Configuration.TabsPerRow.Value;
			if (x == 0)
			{
				pivotX = ((Configuration.ParsedTabAlignment == Configuration.Alignment.Left) ? (-(Configuration.TabsPerRow.DefaultValue * 64 - Configuration.TabsPerRow.DefaultValue % 2 * 64)) : ((Configuration.ParsedTabAlignment != Configuration.Alignment.Right) ? (-(((childs.Count - i >= Configuration.TabsPerRow.Value) ? Configuration.TabsPerRow.Value : (childs.Count - i)) * 64) + 64) : (Configuration.TabsPerRow.DefaultValue * 64 + Configuration.TabsPerRow.DefaultValue % 2 * 64 - ((childs.Count - i >= Configuration.TabsPerRow.Value) ? Configuration.TabsPerRow.Value : (childs.Count - i)) * 128)));
			}
			if (childs.Count > i)
			{
				RectTransform rect = childs[i].transform.GetComponent<RectTransform>();
				rect.anchoredPosition = new Vector2(pivotX + x * 128, -(y * 128));
				rect.pivot = new Vector2(0.5f, 1f);
			}
		}
		int backgroundPivotX = 0;
		if (Configuration.ParsedTabAlignment == Configuration.Alignment.Left)
		{
			backgroundPivotX -= (Configuration.TabsPerRow.DefaultValue - Configuration.TabsPerRow.Value) * 64;
		}
		else if (Configuration.ParsedTabAlignment == Configuration.Alignment.Right)
		{
			backgroundPivotX += (Configuration.TabsPerRow.DefaultValue - Configuration.TabsPerRow.Value) * 64;
		}
		backgroundRect.anchoredPosition = new Vector2(backgroundPivotX, -64f);
		backgroundRect.sizeDelta = new Vector2(Configuration.TabsPerRow.Value * 128 + 54, 128f);
		menuCollider.size = new Vector3(900f, 128 + (childs.Count - 1) / Configuration.TabsPerRow.Value * 128, 1f);
		menuCollider.center = new Vector3(0f, -64 - (childs.Count - 1) / Configuration.TabsPerRow.Value * 64, 0f);
		tooltipRect.anchoredPosition = new Vector2(0f, -140 - (childs.Count - 1) / Configuration.TabsPerRow.Value * 128);
		foreach (RectTransform transform in uixObjects)
		{
			transform.anchoredPosition = new Vector2(transform.anchoredPosition.x, -((childs.Count - 1) / Configuration.TabsPerRow.Value * 42));
		}
		if (useStyletor)
		{
			yield return null;
			backgroundRect.anchoredPosition = new Vector2(0f, -64f);
			backgroundRect.sizeDelta = new Vector2(950f, 128f);
		}
	}

	[HideFromIl2Cpp]
	public void ApplySorting(bool applyDefault = false)
	{
		if (!applyDefault && !Configuration.TabSorting.Value)
		{
			return;
		}
		System.Collections.Generic.Dictionary<string, (Transform, UIPage)> dictionary = new System.Collections.Generic.Dictionary<string, (Transform, UIPage)>();
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			Transform transform = child.Cast<Transform>();
			if (transform.gameObject.name != "Background_QM_PagePanel")
			{
				string text = transform.gameObject.GetComponent<MenuTab>()?.field_Public_String_0;
				if (menuStateController.field_Private_Dictionary_2_String_UIPage_0.ContainsKey(text))
				{
					UIPage uIPage = menuStateController.field_Private_Dictionary_2_String_UIPage_0[text];
					dictionary.Add(uIPage.field_Public_String_0, System.ValueTuple.Create(transform, uIPage));
				}
				else
				{
					MelonLogger.Warning("Menu tab \"" + text + "\" has no UIPage.");
				}
			}
		}
		if (defaultSorting == null)
		{
			defaultSorting = new System.Collections.Generic.List<string>();
			defaultSorting.AddRange(dictionary.Keys);
		}
		bool flag = false;
		foreach (string key in dictionary.Keys)
		{
			if (!tabSorting.ContainsKey(key))
			{
				tabSorting.Add(key, tabSorting.Count + 1);
				flag = true;
			}
		}
		if (flag)
		{
			Configuration.Save(tabSorting);
		}
		System.Collections.Generic.List<string> list = (applyDefault ? defaultSorting : tabSorting.OrderBy((System.Collections.Generic.KeyValuePair<string, int> x) => x.Value).ToDictionary((System.Collections.Generic.KeyValuePair<string, int> k) => k.Key, (System.Collections.Generic.KeyValuePair<string, int> v) => v.Value).Keys.ToList());
		int num = 0;
		for (int j = 0; j < list.Count; j++)
		{
			if (dictionary.ContainsKey(list[j]))
			{
				dictionary[list[j]].Item1.SetSiblingIndex(num + 1);
				SetPageIndex(dictionary[list[j]].Item2, num);
				num++;
			}
		}
	}

	[HideFromIl2Cpp]
	private void SetPageIndex(UIPage uiPage, int index)
	{
		Il2CppReferenceArray<UIPage> field_Public_ArrayOf_UIPage_ = menuStateController.field_Public_ArrayOf_UIPage_0;
		for (int i = 0; i < field_Public_ArrayOf_UIPage_.Count; i++)
		{
			if (field_Public_ArrayOf_UIPage_[i].Equals(uiPage))
			{
				Switch(field_Public_ArrayOf_UIPage_, i, index);
			}
		}
	}

	[HideFromIl2Cpp]
	private static void Switch<T>(System.Collections.Generic.IList<T> array, int index, int newIndex)
	{
		T value = array[index];
		array[index] = array[newIndex];
		array[newIndex] = value;
	}
}
