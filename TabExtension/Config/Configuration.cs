using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using MelonLoader;
using MelonLoader.Preferences;
using MelonLoader.TinyJSON;

namespace TabExtension.Config;

public static class Configuration
{
	public enum Alignment
	{
		Left,
		Center,
		Right
	}

	private class IntegerValidator : ValueValidator
	{
		public int DefaultValue;

		public int MinValue;

		public int MaxValue;

		public IntegerValidator(int defaultValue, int minValue, int maxValue)
		{
			DefaultValue = defaultValue;
			MinValue = minValue;
			MaxValue = maxValue;
		}

		public override object EnsureValid(object value)
		{
			if (IsValid(value))
			{
				return value;
			}
			return DefaultValue;
		}

		public override bool IsValid(object value)
		{
			int num = Convert.ToInt32(value);
			return num >= MinValue && num <= MaxValue;
		}
	}

	internal class UIXIntegration
	{
	}

	private static readonly MelonPreferences_Category Category = MelonPreferences.CreateCategory("TabExtension", "Tab Extension");

	private static readonly string Path = "UserData\\TabExtension\\";

	private static readonly string FileName = "TabSorting.json";

	public static MelonPreferences_Entry<bool> TabSorting;

	public static MelonPreferences_Entry<bool> TabBackground;

	public static MelonPreferences_Entry<string> TabAlignment;

	public static MelonPreferences_Entry<int> TabsPerRow;

	public static Alignment ParsedTabAlignment;

	public static void Init()
	{
		TabSorting = Category.CreateEntry("TabSorting", default_value: false, "Tab Sorting (config in UserData)");
		TabBackground = Category.CreateEntry("TabBackground", default_value: true, "Tab Background");
		TabAlignment = Category.CreateEntry("TabAlignment", "Center", "Tab Alignment");
		TabsPerRow = Category.CreateEntry("TabsPerRow", 7, "Tabs Per Row", null, is_hidden: false, dont_save_default: false, new IntegerValidator(7, 1, 7));
		Action<string> parseAlignmentAction = delegate(string value)
		{
			if (Enum.TryParse<Alignment>(value, ignoreCase: true, out var result))
			{
				ParsedTabAlignment = result;
			}
		};
		TabAlignment.OnValueChanged += delegate(string oldValue, string newValue)
		{
			parseAlignmentAction(newValue);
		};
		parseAlignmentAction(TabAlignment.Value);
		if (!Directory.Exists(Path))
		{
			Directory.CreateDirectory(Path);
		}
	}

	public static void Save(Dictionary<string, int> tabSorting)
	{
		try
		{
			File.WriteAllText(Path + FileName, Encoder.Encode(tabSorting, EncodeOptions.PrettyPrint));
			MelonLogger.Msg(FileName + " was saved.");
		}
		catch (Exception ex)
		{
			MelonLogger.Error("Error while saving " + FileName + ": " + ex.ToString());
		}
	}

	public static Dictionary<string, int> Load()
	{
		if (!File.Exists(Path + FileName))
		{
			return null;
		}
		try
		{
			return Decoder.Decode(File.ReadAllText("UserData/TabExtension/TabSorting.json")).Make<Dictionary<string, int>>();
		}
		catch (Exception ex)
		{
			MelonLogger.Error("Error while loading " + FileName + ": " + ex.ToString());
			return null;
		}
	}
}
