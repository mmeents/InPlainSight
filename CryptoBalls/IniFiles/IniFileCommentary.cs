using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBalls.IniFiles {

	/// <summary>Represents one or more comment lines in a config file.</summary>
	public class IniFileCommentary : IniFileElement {
		private string _comment = "";
		private string? _commentChar = "";

		private IniFileCommentary() { }
		/// <summary>Initializes a new instance IniFileCommentary</summary>
		/// <param name="content">Actual content of a line in a INI file.</param>
		public IniFileCommentary(string content)	: base(content) {
			if (IniFileSettings.CommentChars.Length == 0)
				throw new NotSupportedException("Comments are disabled. Set the IniFileSettings.CommentChars property to turn them on.");			
			_commentChar = IniFileSettings.StartsWith(Content, IniFileSettings.CommentChars);
			if (_commentChar != null && Content.Length > _commentChar.Length)
				_comment = Content[_commentChar.Length..];
			else
				_comment = "";
		}
		/// <summary>Gets or sets comment char used in the config file for this comment.</summary>
		public string CommentChar {
			get { return _commentChar ?? ""; }
			set {
				if (_commentChar != value) {
					_commentChar = value; 
					Rewrite();
				}
			}
		}
		/// <summary>Gets or sets a commentary string.</summary>
		public string Comment {
			get { return _comment; }
			set {
				if (_comment != value) {
					_comment = value; Rewrite();
				}
			}
		}
		void Rewrite() {
			StringBuilder newContent = new();
			string[] lines;
			if (_comment != null) {
				lines = _comment.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
			} else {
				lines = "".Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
			}			
			newContent.Append(_commentChar + lines[0]);
			for (int i = 1; i < lines.Length; i++)
				newContent.Append(Environment.NewLine + _commentChar + lines[i]);
			Content = newContent.ToString();
		}
		/// <summary>Determines whether specified string is a representation of particular IniFileElement object.</summary>
		/// <param name="testString">Trimmed test string.</param>
		public static bool IsLineValid(string testString) {
			return IniFileSettings.StartsWith(testString.TrimStart(), IniFileSettings.CommentChars) != null;
		}
		/// <summary>Gets a string representation of this IniFileCommentary object.</summary>
		public override string ToString() {
			return "Comment: \"" + _comment + "\"";
		}
		/// <summary>Gets an IniFileCommentary object from commentary text.</summary>
		/// <param name="comment">Commentary text.</param>
		public static IniFileCommentary FromComment(string comment) {
			if (IniFileSettings.CommentChars.Length == 0)
				throw new NotSupportedException("Comments are disabled. Set the IniFileSettings.CommentChars property to turn them on.");
			IniFileCommentary ret = new() {
				_comment = comment,
				CommentChar = IniFileSettings.CommentChars[0]
			};
			return ret;
		}
		/// <summary>Formats IniFileCommentary object to default appearance.</summary>
		public override void FormatDefault() {
			base.FormatDefault();
			CommentChar = IniFileSettings.CommentChars[0];
			Rewrite();
		}
	}
}
