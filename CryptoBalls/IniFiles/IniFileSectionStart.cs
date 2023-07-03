using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StaticExtensions;

namespace CryptoBalls.IniFiles {
	/// <summary>Represents section's start line, e.g. "[SectionName]".</summary>
	public class IniFileSectionStart : IniFileElement {
		private string sectionName = "";
		private readonly string textOnTheRight = "";
		private string inlineComment = "";
		private IniFileSectionStart() : base() { }
		/// <summary>Initializes a new instance IniFileSectionStart</summary>
		/// <param name="content">Actual content of a line in an INI file. Initializer assumes that it is valid.</param>
		public IniFileSectionStart(string content) : base(content) {
			formatting = ExtractFormat(content);
			content = content.TrimStart();
			if (IniFileSettings.AllowInlineComments) {
				IniFileSettings.IndexOfAnyResult result = IniFileSettings.IndexOfAny(content, IniFileSettings.CommentChars);
				if (result.index > content.IndexOf(IniFileSettings.SectionCloseBracket) && result.any != null) {
					inlineComment = content[(result.index + result.any.Length)..];
					content = content[..result.index];
				}
			}
			if (IniFileSettings.AllowTextOnTheRight) {
				int closeBracketPos = content.LastIndexOf(IniFileSettings.SectionCloseBracket);
				if (closeBracketPos != content.Length - 1) {
					textOnTheRight = content[(closeBracketPos + 1)..];
					content = content[..closeBracketPos];
				}
			}
			sectionName = content[IniFileSettings.SectionOpenBracket.Length..^IniFileSettings.SectionCloseBracket.Length].ParseFirst("] ");
			Content = content;
			Format();
		}
		/// <summary>Gets or sets a secion's name.</summary>
		public string SectionName {
			get { return sectionName; }
			set {
				sectionName = value;
				Format();
			}
		}
		/// <summary>Gets or sets an inline comment, which appear after the value.</summary>
		public string InlineComment {
			get { return inlineComment; }
			set {
				if (!IniFileSettings.AllowInlineComments || IniFileSettings.CommentChars.Length == 0)
					throw new NotSupportedException("Inline comments are disabled.");
				inlineComment = value; Format();
			}
		}
		/// <summary>Determines whether specified string is a representation of particular IniFileElement object.</summary>
		/// <param name="testString">Trimmed test string.</param>
		public static bool IsLineValid(string testString) {
			string sLocal = testString;
			if (sLocal.Contains(';')) { 
				sLocal = sLocal.ParseFirst(";").TrimEnd();
			}
			return sLocal.StartsWith(IniFileSettings.SectionOpenBracket) && sLocal.EndsWith(IniFileSettings.SectionCloseBracket);
		}
		/// <summary>Gets a string representation of this IniFileSectionStart object.</summary>
		public override string ToString() {
			return "Section: \"" + sectionName + "\"";
		}
		/// <summary>Creates a new IniFileSectionStart object basing on a name of section and the formatting style of this section.</summary>
		/// <param name="sectName">Name of the new section</param>
		public IniFileSectionStart CreateNew(string sectName) {
			IniFileSectionStart ret = new() {
				sectionName = sectName
			};
			if (IniFileSettings.PreserveFormatting) {
				ret.formatting = formatting;
				ret.Format();
			} else
				ret.Format();
			return ret;
		}
		/// <summary>Creates a formatting string basing on an actual content of a line.</summary>
		public static string ExtractFormat(string content) {
			bool beforeS = false;
			bool afterS = false;
			bool beforeEvery = true;
			char currC;
			string insideWhiteChars = "";
			StringBuilder form = new();
			for (int i = 0; i < content.Length; i++) {
				currC = content[i];
				if (char.IsLetterOrDigit(currC) && beforeS) {
					afterS = true; beforeS = false; form.Append('$');
				} else if (afterS && char.IsLetterOrDigit(currC)) {
					insideWhiteChars = "";
				} else if (content.Length - i >= IniFileSettings.SectionOpenBracket.Length && content.Substring(i, IniFileSettings.SectionOpenBracket.Length) == IniFileSettings.SectionOpenBracket && beforeEvery) {
					beforeS = true; beforeEvery = false; form.Append('[');
				} else if (content.Length - i >= IniFileSettings.SectionCloseBracket.Length && content.Substring(i, IniFileSettings.SectionOpenBracket.Length) == IniFileSettings.SectionCloseBracket && afterS) {
					form.Append(insideWhiteChars);
					afterS = false; form.Append(IniFileSettings.SectionCloseBracket);
				} else if ((IniFileSettings.OfAny(i, content, IniFileSettings.CommentChars)) != null) {
					form.Append(';');
				} else if (char.IsWhiteSpace(currC)) {
					if (afterS) insideWhiteChars += currC;
					else form.Append(currC);
				}
			}
			string ret = form.ToString();
			if (!ret.Contains(';'))
				ret += "   ;";
			return ret;
		}
		/// <summary>Formats the IniFileElement object using default format specified in IniFileSettings.</summary>
		public override void FormatDefault() {
			Formatting = IniFileSettings.DefaultSectionFormatting;
			Format();
		}
		/// <summary>Formats this element using a formatting string in Formatting property.</summary>
		public void Format() { Format(formatting); }
		/// <summary>Formats this element using given formatting string</summary>
		/// <param name="formatting">Formatting template, where '['-open bracket, '$'-section name, ']'-close bracket, ';'-inline comments.</param>
		public void Format(string formatting) {
			char currC;
      StringBuilder build = new();
			for (int i = 0; i < formatting.Length; i++) {
				currC = formatting[i];
				if (currC == '$')
					build.Append(sectionName);
				else if (currC == '[')
					build.Append(IniFileSettings.SectionOpenBracket);
				else if (currC == ']')
					build.Append(IniFileSettings.SectionCloseBracket);
				else if (currC == ';' && IniFileSettings.CommentChars.Length > 0 && inlineComment != null)
					build.Append(IniFileSettings.CommentChars[0]).Append(inlineComment);
				else if (char.IsWhiteSpace(formatting[i]))
					build.Append(formatting[i]);
			}
			Content = build.ToString().TrimEnd() + (IniFileSettings.AllowTextOnTheRight ? textOnTheRight : "");
		}
		/// <summary>Crates a IniFileSectionStart object from name of a section.</summary>
		/// <param name="sectionName">Name of a section</param>
		public static IniFileSectionStart FromName(string sectionName) {
			IniFileSectionStart ret = new() {
				SectionName = sectionName
			};
			ret.FormatDefault();
			return ret;
		}
	}
}
