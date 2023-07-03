using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBalls.IniFiles {
	/// <summary>Represents one key-value pair.</summary>
	public class IniFileValue : IniFileElement {
		private string key = "";
		private string value = "";
		private readonly string textOnTheRight = ""; // only if qoutes are on, e.g. "Name = 'Jack' text-on-the-right"
		private string inlineComment = "", inlineCommentChar = "";
		private IniFileValue() : base() { }
		/// <summary>Initializes a new instance IniFileValue.</summary>
		/// <param name="content">Actual content of a line in an INI file. Initializer assumes that it is valid.</param>
		public IniFileValue(string content)
			: base(content) {
			string[] split = Content.Split(new string[] { IniFileSettings.EqualsString }, StringSplitOptions.None);
			formatting = ExtractFormat(content);
			string split0 = split[0].Trim();
			string split1 = split.Length >= 1 ?
				split[1].Trim()
				: "";

			if (split0.Length > 0) {
				if (IniFileSettings.AllowInlineComments) {
					IniFileSettings.IndexOfAnyResult result = IniFileSettings.IndexOfAny(split1, IniFileSettings.CommentChars);
					if (result.index != -1 && result.any != null) {
						inlineComment = split1[(result.index + result.any.Length)..];
						split1 = split1[..result.index].TrimEnd();
						inlineCommentChar = result.any;
					}
				}
				if (IniFileSettings.QuoteChar != null && split1.Length >= 2) {
					char quoteChar = (char)IniFileSettings.QuoteChar;
					if (split1[0] == quoteChar) {
						int lastQuotePos;
						if (IniFileSettings.AllowTextOnTheRight) {
							lastQuotePos = split1.LastIndexOf(quoteChar);
							if (lastQuotePos != split1.Length - 1)
								textOnTheRight = split1[(lastQuotePos + 1)..];
						} else
							lastQuotePos = split1.Length - 1;
						if (lastQuotePos > 0) {
							if (split1.Length == 2)
								split1 = "";
							else
								split1 = split1[1..lastQuotePos];
						}
					}
				}
				key = split0;
				value = split1;
			}
			Format();
		}
		/// <summary>Gets or sets a name of value.</summary>
		public string Key {
			get { return key; }
			set { key = value; Format(); }
		}
		/// <summary>Gets or sets a value.</summary>
		public string Value {
			get { return value; }
			set { this.value = value; Format(); }
		}
		/// <summary>Gets or sets an inline comment, which appear after the value.</summary>
		public string InlineComment {
			get { return inlineComment; }
			set {
				if (!IniFileSettings.AllowInlineComments || IniFileSettings.CommentChars.Length == 0)
					throw new NotSupportedException("Inline comments are disabled.");
				if (inlineCommentChar == null)
					inlineCommentChar = IniFileSettings.CommentChars[0];
				inlineComment = value; Format();
			}
		}
		enum FormatExtractorState // stare of format extractor (ExtractFormat method)
		{
			BeforeEvery, AfterKey, BeforeVal, AfterVal
		}
		/// <summary>Creates a formatting string basing on an actual content of a line.</summary>
		public static string ExtractFormat(string content) {
			//bool afterKey = false; bool beforeVal = false; bool beforeEvery = true; bool afterVal = false;
			//return IniFileSettings.DefaultValueFormatting;
			FormatExtractorState pos = FormatExtractorState.BeforeEvery;
			char currC; string insideWhiteChars = ""; string theWhiteChar; ;
			StringBuilder form = new();
			for (int i = 0; i < content.Length; i++) {
				currC = content[i];
				if (char.IsLetterOrDigit(currC)) {
					if (pos == FormatExtractorState.BeforeEvery) {
						form.Append('?');
						pos = FormatExtractorState.AfterKey;
						//afterKey = true; beforeEvery = false; ;
					} else if (pos == FormatExtractorState.BeforeVal) {
						form.Append('$');
						pos = FormatExtractorState.AfterVal;
					}
				} else if (pos == FormatExtractorState.AfterKey && content.Length - i >= IniFileSettings.EqualsString.Length && content.Substring(i, IniFileSettings.EqualsString.Length) == IniFileSettings.EqualsString) {
					form.Append(insideWhiteChars);
					pos = FormatExtractorState.BeforeVal;
					//afterKey = false; beforeVal = true; 
					form.Append('=');
				} else if ((IniFileSettings.OfAny(i, content, IniFileSettings.CommentChars)) != null) {
					form.Append(insideWhiteChars);
					form.Append(';');
				} else if (char.IsWhiteSpace(currC)) {
					if (currC == '\t' && IniFileSettings.TabReplacement != null)
						theWhiteChar = IniFileSettings.TabReplacement;
					else
						theWhiteChar = currC.ToString();
					if (pos == FormatExtractorState.AfterKey || pos == FormatExtractorState.AfterVal) {
						insideWhiteChars += theWhiteChar;
						continue;
					} else
						form.Append(theWhiteChar);
				}
				insideWhiteChars = "";
			}
			if (pos == FormatExtractorState.BeforeVal) {
				form.Append('$');
			}
			string ret = form.ToString();
			if (!ret.Contains(';'))
				ret += "   ;";
			return ret;
		}

		/// <summary>Formats this element using the format string in Formatting property.</summary>
		public void Format() {
			Format(formatting);
		}
		/// <summary>Formats this element using given formatting string</summary>
		/// <param name="formatting">Formatting template, where '?'-key, '='-equality sign, '$'-value, ';'-inline comments.</param>
		public void Format(string formatting) {
			char currC;
			StringBuilder build = new();
			for (int i = 0; i < formatting.Length; i++) {
				currC = formatting[i];
				if (currC == '?')
					build.Append(key);
				else if (currC == '$') {
					if (IniFileSettings.QuoteChar != null) {
						char quoteChar = (char)IniFileSettings.QuoteChar;
						build.Append(quoteChar).Append(value).Append(quoteChar);
					} else
						build.Append(value);
				} else if (currC == '=')
					build.Append(IniFileSettings.EqualsString);
				else if (currC == ';')
					build.Append(inlineCommentChar + inlineComment);
				else if (char.IsWhiteSpace(formatting[i]))
					build.Append(currC);
			}
			Content = build.ToString().TrimEnd() + (IniFileSettings.AllowTextOnTheRight ? textOnTheRight : "");
		}
		/// <summary>Formats content using a scheme specified in IniFileSettings.DefaultValueFormatting.</summary>
		public override void FormatDefault() {
			Formatting = IniFileSettings.DefaultValueFormatting;
			Format();
		}
		/// <summary>Creates a new IniFileValue object basing on a key and a value and the formatting  of this IniFileValue.</summary>
		/// <param name="key">Name of value</param>
		/// <param name="value">Value</param>
		public IniFileValue CreateNew(string key, string value) {
			IniFileValue ret = new() {
				key = key, value = value
			};
			if (IniFileSettings.PreserveFormatting) {
				ret.formatting = formatting;
				if (IniFileSettings.AllowInlineComments)
					ret.inlineCommentChar = inlineCommentChar;
				ret.Format();
			} else
				ret.FormatDefault();
			return ret;
		}
		/// <summary>Determines whether specified string is a representation of particular IniFileElement object.</summary>
		/// <param name="testString">Trimmed test string.</param>
		public static bool IsLineValid(string testString) {
			int index = testString.IndexOf(IniFileSettings.EqualsString);
			return index > 0;
		}
		/// <summary>Sets both key and values. Recommended when both properties have to be changed.</summary>
		public void Set(string key, string value) {
			this.key = key; this.value = value;
			Format();
		}
		/// <summary>Gets a string representation of this IniFileValue object.</summary>
		public override string ToString() {
			return "Value: \"" + key + " = " + value + "\"";
		}
		/// <summary>Crates a IniFileValue object from it's data.</summary>
		/// <param name="key">Value name.</param>
		/// <param name="value">Associated value.</param>
		public static IniFileValue FromData(string key, string value) {
			IniFileValue ret = new() {
				key = key, value = value
			};
			ret.FormatDefault();
			return ret;
		}
	}
}
