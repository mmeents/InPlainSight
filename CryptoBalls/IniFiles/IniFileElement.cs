using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBalls.IniFiles {
	/// <summary>Base class for all Config File elements.</summary>
	public class IniFileElement {
		private string _line;
		protected string formatting = "";
		/// <summary>Initializes a new, empty instance IniFileElement</summary>
		protected IniFileElement() { _line = ""; }
		/// <summary>Initializes a new instance IniFileElement</summary>
		/// <param name="content">Actual content of a line in a INI file.</param>
		public IniFileElement(string content) {
			_line = content.TrimEnd();
		}
		/// <summary>Gets or sets a formatting string of this INI file element, spicific to it's type. 
		/// See DefaultFormatting property in IniFileSettings for more info.</summary>
		public string Formatting { get { return formatting; } set { formatting = value; } }
		/// <summary>Gets or sets a string of white characters which precedes any meaningful content of a line.</summary>
		public string Intendation {
			get {
				StringBuilder intend = new();
				for (int i = 0; i < formatting.Length; i++) {
					if (!char.IsWhiteSpace(formatting[i])) break;
					intend.Append(formatting[i]);
				}
				return intend.ToString();
			}
			set {
				if (value.TrimStart().Length > 0)
					throw new ArgumentException("Intendation property cannot contain any characters which are not condsidered as white ones.");
				if (IniFileSettings.TabReplacement != null)
					value = value.Replace("\t", IniFileSettings.TabReplacement);
				formatting = value + formatting.TrimStart();
				_line = value + _line.TrimStart();
			}
		}
		/// <summary>Gets full text representation of a config file element, excluding intendation.</summary>
		public string Content {
			get { return _line.TrimStart(); }
			protected set { _line = value; }
		}
		/// <summary>Gets full text representation of a config file element, including intendation.</summary>
		public string Line {
			get {
				string intendation = Intendation;
				if (_line.Contains(Environment.NewLine)) {
					string[] lines = _line.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
					StringBuilder ret = new();
					ret.Append(lines[0]);
					for (int i = 1; i < lines.Length; i++)
						ret.Append(Environment.NewLine + intendation + lines[i]);
					return ret.ToString();
				} else
					return _line;
			}
		}
		/// <summary>Gets a string representation of this IniFileElement object.</summary>
		public override string ToString() {
			return "Line: \"" + _line + "\"";
		}
		/// <summary>Formats this config element</summary>
		public virtual void FormatDefault() {
			Intendation = "";
		}
	}

}
