using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBalls.IniFiles {
	/// <summary>Static class containing format settings for INI files.</summary>
	public static class IniFileSettings {
		private static IniFlags flags = (IniFlags)24;
		private static string[] commentChars = { ";", "#" };
		private static char? quoteChar = null;
		private static string defaultValueFormatting = "?=$   ;";
		private static string defaultSectionFormatting = "[$]   ;";
		private static string sectionCloseBracket = "]";
		private static string equalsString = "=";
		private static string tabReplacement = "    ";
		private static string sectionOpenBracket = "[";

		// GroupElements CaseSensitive 
		private enum IniFlags {
			PreserveFormatting = 1, AllowEmptyValues = 2, AllowTextOnTheRight = 4,
			GroupElements = 8, CaseSensitive = 16, SeparateHeader = 32, AllowBlankLines = 64,
			AllowInlineComments = 128
		}
		//private static string DefaultCommentaryFormatting = ";$";

		#region Public properties

		/// <summary>Inficates whether parser should preserve formatting. Default TRUE.</summary>
		public static bool PreserveFormatting {
			get { return (flags & IniFlags.PreserveFormatting) == IniFlags.PreserveFormatting; }
			set {
				if (value)
					flags |= IniFlags.PreserveFormatting;
				else
					flags &= ~IniFlags.PreserveFormatting;
			}
		}
		/// <summary>If true empty keys will not be removed. Default TRUE.</summary>
		public static bool AllowEmptyValues {
			get { return (flags & IniFlags.AllowEmptyValues) == IniFlags.AllowEmptyValues; }
			set {
				if (value)
					flags |= IniFlags.AllowEmptyValues;
				else
					flags &= ~IniFlags.AllowEmptyValues;
			}
		}
		/// <summary>If Quotes are on, then it in such situation: |KEY = "VALUE" blabla|, 'blabla' is 
		/// a "text on the right". If this field is set to False, then such string will be ignored.</summary>
		public static bool AllowTextOnTheRight {
			get { return (flags & IniFlags.AllowTextOnTheRight) == IniFlags.AllowTextOnTheRight; }
			set {
				if (value)
					flags |= IniFlags.AllowTextOnTheRight;
				else
					flags &= ~IniFlags.AllowTextOnTheRight;
			}
		}
		/// <summary>Indicates whether comments and blank lines should be grouped
		/// (if true then multiple line comment will be parsed to the one single IniFileComment object).
		/// Otherwise, one IniFileElement will be always representing one single line in the file. Default TRUE.</summary>
		public static bool GroupElements {
			get { return (flags & IniFlags.GroupElements) == IniFlags.GroupElements; }
			set {
				if (value)
					flags |= IniFlags.GroupElements;
				else
					flags &= ~IniFlags.GroupElements;
			}
		}
		/// <summary>Determines whether all searching/testing operation are case-sensitive. Default TRUE.</summary>
		public static bool CaseSensitive {
			get { return (flags & IniFlags.CaseSensitive) == IniFlags.CaseSensitive; }
			set {
				if (value)
					flags |= IniFlags.CaseSensitive;
				else
					flags &= ~IniFlags.CaseSensitive;
			}
		}
		/// <summary>Determines whether a header comment of an INI file is separate from a comment of first section.
		/// If false, comment at the beginning of file may be considered both as header and commentary of the first section. Default TRUE.</summary>
		public static bool SeparateHeader {
			get { return (flags & IniFlags.SeparateHeader) == IniFlags.SeparateHeader; }
			set {
				if (value)
					flags |= IniFlags.SeparateHeader;
				else
					flags &= ~IniFlags.SeparateHeader;
			}
		}
		/// <summary>If true, blank lines will be written to a file. Otherwise, they will ignored.</summary>
		public static bool AllowBlankLines {
			get { return (flags & IniFlags.AllowBlankLines) == IniFlags.AllowBlankLines; }
			set {
				if (value)
					flags |= IniFlags.AllowBlankLines;
				else
					flags &= ~IniFlags.AllowBlankLines;
			}
		}
		/// <summary>If true, blank lines will be written to a file. Otherwise, they will ignored.</summary>
		public static bool AllowInlineComments {
			get { return (flags & IniFlags.AllowInlineComments) != 0; }
			set {
				if (value) flags |= IniFlags.AllowInlineComments;
				else flags &= ~IniFlags.AllowInlineComments;
			}
		}
		/// <summary>A string which represents close bracket for a section. If empty or null, sections will
		/// disabled. Default "]"</summary>
		public static string SectionCloseBracket {
			get { return IniFileSettings.sectionCloseBracket; }
			set {
				IniFileSettings.sectionCloseBracket = value ?? throw new ArgumentNullException("SectionCloseBracket");
			}
		}
		/// <summary>Gets or sets array of strings which start a comment line.
		/// Default is {"#" (hash), ";" (semicolon)}. If empty or null, commentaries
		/// will not be allowed.</summary>
		public static string[] CommentChars {
			get { return IniFileSettings.commentChars; }
			set {
				IniFileSettings.commentChars = value ?? throw new ArgumentNullException("CommentChars", "Use empty array to disable comments instead of null");
			}
		}
		/// <summary>Gets or sets a character which is used as quote. Default null (not using quotation marks).</summary>
		public static char? QuoteChar {
			get { return IniFileSettings.quoteChar; }
			set { IniFileSettings.quoteChar = value; }
		}
		/// <summary>A string which determines default formatting of section headers used in Format() method.
		/// '$' (dollar) means a section's name; '[' and ']' mean brackets; optionally, ';' is an inline comment. Default is "[$]  ;" (e.g. "[Section]  ;comment")</summary>
		public static string DefaultSectionFormatting {
			get { return IniFileSettings.defaultSectionFormatting; }
			set {
				if (value == null)
					throw new ArgumentNullException("DefaultSectionFormatting");
				string test = value.Replace("$", "").Replace("[", "").Replace("]", "").Replace(";", "");
				if (test.TrimStart().Length > 0)
					throw new ArgumentException("DefaultValueFormatting property cannot contain other characters than [,$,] and white spaces.");
				if (!(value.IndexOf('[') < value.IndexOf('$') && value.IndexOf('$') < value.IndexOf(']')
					&& (!value.Contains(';') || value.IndexOf(']') < value.IndexOf(';'))))
					throw new ArgumentException("Special charcters in the formatting strings are in the incorrect order. The valid is: [, $, ].");
				IniFileSettings.defaultSectionFormatting = value;
			}
		}
		/// <summary>A string which determines default formatting of values used in Format() method. '?' (question mark) means a key,
		/// '$' (dollar) means a value and '=' (equality sign) means EqualsString; optionally, ';' is an inline comment.
		/// If QouteChar is not null, '$' will be automatically surrounded with qouetes. Default "?=$  ;" (e.g. "Key=Value  ;comment".</summary>
		public static string DefaultValueFormatting {
			get { return IniFileSettings.defaultValueFormatting; }
			set {
				if (value == null)
					throw new ArgumentNullException("DefaultValueFormatting");
				string test = value.Replace("?", "").Replace("$", "").Replace("=", "").Replace(";", "");
				if (test.TrimStart().Length > 0)
					throw new ArgumentException("DefaultValueFormatting property cannot contain other characters than ?,$,= and white spaces.");
				if (!(((value.IndexOf('?') < value.IndexOf('=') && value.IndexOf('=') < value.IndexOf('$'))
					|| (!value.Contains('=') && test.IndexOf('?') < value.IndexOf('$')))
					&& (!value.Contains(';') || value.IndexOf('$') < value.IndexOf(';'))))
					throw new ArgumentException("Special charcters in the formatting strings are in the incorrect order. The valid is: ?, =, $.");
				IniFileSettings.defaultValueFormatting = value;
			}
		}

		/// <summary>A string which represents open bracket for a section. If empty or null, sections will
		/// disabled. Default "[".</summary>
		public static string SectionOpenBracket {
			get { return IniFileSettings.sectionOpenBracket; }
			set {
				IniFileSettings.sectionOpenBracket = value ?? throw new ArgumentNullException("SectionOpenBracket");
			}
		}
		/// <summary>Gets or sets string used as equality sign (which separates value from key). Default "=".</summary>
		public static string EqualsString {
			get { return IniFileSettings.equalsString; }
			set {
				IniFileSettings.equalsString = value ?? throw new ArgumentNullException("EqualsString");
			}
		}
		/// <summary>The string which all tabs in intendentation will be replaced with. If null, tabs will not be replaced. Default "    " (four spaces).</summary>
		public static string TabReplacement {
			get { return IniFileSettings.tabReplacement; }
			set { IniFileSettings.tabReplacement = value; }
		}
		#endregion

		internal static string TrimLeft(ref string str) {
			int i = 0;
			StringBuilder ret = new();
			while (i < str.Length && char.IsWhiteSpace(str, i)) {
				ret.Append(str[i]);
				i++;
			}
			if (str.Length > i)
				str = str[i..];
			else
				str = "";
			return ret.ToString();
		}
		internal static string TrimRight(ref string str) {
			int i = str.Length - 1;
			StringBuilder build = new();
			while (i >= 0 && char.IsWhiteSpace(str, i)) {
				build.Append(str[i]);
				i--;
			}
			StringBuilder reversed = new();
			for (int j = build.Length - 1; j >= 0; j--)
				reversed.Append(build[j]);
			if (str.Length - i > 0)
				str = str[..(i + 1)];
			else
				str = "";
			return reversed.ToString();
		}
		internal static string? StartsWith(string line, string[] array) {
			if (array == null) return null;
			for (int i = 0; i < array.Length; i++)
				if (line.StartsWith(array[i]))
					return array[i];
			return null;
		}
		internal struct IndexOfAnyResult {
			public int index;
			public string? any;
			public IndexOfAnyResult(int i, string? _any) {
				any = _any; index = i;
			}
		}
		internal static IndexOfAnyResult IndexOfAny(string text, string[] array) {
			for (int i = 0; i < array.Length; i++)
				if (text.Contains(array[i]))
					return new IndexOfAnyResult(text.IndexOf(array[i]), array[i]);
			return new IndexOfAnyResult(-1, null);
		}
		internal static string? OfAny(int index, string text, string[] array) {
			for (int i = 0; i < array.Length; i++)
				if (text.Length - index >= array[i].Length && text.Substring(index, array[i].Length) == array[i])
					return array[i];
			return null;
		}
	}
}
