using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBalls.IniFiles {
	/// <summary>Object model for a section in an INI file, which stores a all values in memory.</summary>
	public class IniFileSection {
		internal List<IniFileElement> elements = new();
		internal IniFileSectionStart sectionStart;
		internal IniFile parent;

		internal IniFileSection(IniFile _parent, IniFileSectionStart sect) {
			sectionStart = sect;
			parent = _parent;
		}
		/// <summary>Gets or sets the name of the section</summary>
		public string Name {
			get { return sectionStart.SectionName; }
			set { sectionStart.SectionName = value; }
		}
		/// <summary>Gets or sets comment associated with this section. In the file a comment must appear exactly
		/// above section's declaration. Returns "" if no comment is provided.</summary>
		public string Comment {
			get {
				return Name == "" ? "" : GetComment(sectionStart);
			}
			set {
				if (Name != "")
					SetComment(sectionStart, value);
			}
		}
		void SetComment(IniFileElement el, string comment) {
			int index = parent.elements.IndexOf(el);
			if (IniFileSettings.CommentChars.Length == 0)
				throw new NotSupportedException("Comments are currently disabled. Setup ConfigFileSettings.CommentChars property to enable them.");
			IniFileCommentary com;
			if (index > 0 && parent.elements[index - 1] is IniFileCommentary commentary) {
				com = commentary;
				if (comment == "")
					parent.elements.Remove(com);
				else {
					com.Comment = comment;
					com.Intendation = el.Intendation;
				}
			} else if (comment != "") {
				com = IniFileCommentary.FromComment(comment);
				com.Intendation = el.Intendation;
				parent.elements.Insert(index, com);
			}
		}
		string GetComment(IniFileElement el) {
			int index = parent.elements.IndexOf(el);
			return index != 0 && parent.elements[index - 1] is IniFileCommentary commentary ? commentary.Comment : "";
		}
		IniFileValue? GetValue(string key) {
			string lower = key.ToLowerInvariant();
			IniFileValue val;
			for (int i = 0; i < elements.Count; i++)
				if (elements[i] is IniFileValue value) {
					val = value;
					if (val.Key == key || (!IniFileSettings.CaseSensitive && val.Key.ToLowerInvariant() == lower))
						return val;
				}
			return null;
		}
		/// <summary>Sets the comment for given key.</summary>
		public void SetComment(string key, string comment) {
			IniFileValue? val = GetValue(key);
			if (val == null) return;
			SetComment(val, comment);
		}
		/// <summary>Sets the inline comment for given key.</summary>
		public void SetInlineComment(string key, string comment) {
			IniFileValue? val = GetValue(key);
			if (val == null) return;
			val.InlineComment = comment;
		}
		/// <summary>Gets the inline comment for given key.</summary>
		public string? GetInlineComment(string key) {
			IniFileValue? val = GetValue(key);
			if (val == null) return null;
			return val.InlineComment;
		}
		/// <summary>Gets or sets the inline for this section.</summary>
		public string InlineComment {
			get { return sectionStart.InlineComment; }
			set { sectionStart.InlineComment = value; }
		}
		/// <summary>Gets the comment associated to given key. If there is no comment, empty string is returned.
		/// If the key does not exist, NULL is returned.</summary>
		public string? GetComment(string key) {
			IniFileValue? val = GetValue(key);
			if (val == null) return null;
			return GetComment(val);
		}
		/// <summary>Renames a key.</summary>
		public void RenameKey(string key, string newName) {
			IniFileValue? v = GetValue(key);
			if (key == null) return;
			if (v == null) return;
			v.Key = newName;
		}
		/// <summary>Deletes a key.</summary>
		public void DeleteKey(string key) {
			IniFileValue? v = GetValue(key);
			if (key == null) return;
			if (v == null) return;
			parent.elements.Remove(v);
			elements.Remove(v);
		}
		/// <summary>Gets or sets value of the key</summary>
		/// <param name="key">Name of key.</param>
		public string? this[string key] {
			get {
				IniFileValue? v = GetValue(key);
				return (v?.Value);
			}
			set {
				if (value == null) { 
					if (key != null) { 
					  this.DeleteKey(key);
					}
			  } else { 
					IniFileValue? v = GetValue(key);				
					if (v != null) {
						v.Value = value;
						return;
					}
					SetValue(key, value);
				}
			}
		}
		/// <summary>Gets or sets value of a key.</summary>
		/// <param name="key">Name of the key.</param>
		/// <param name="defaultValue">A value to return if the requested key was not found.</param>
		public string this[string key, string defaultValue] {
			get {
				string? val = this[key];
				if (val == "" || val == null)
					return defaultValue;
				return val;
			}
			set { this[key] = value; }
		}
		private void SetValue(string key, string value) {
			IniFileValue? ret = null;
			IniFileValue? prev = LastValue();

			if (IniFileSettings.PreserveFormatting) {
				if (prev != null && prev.Intendation.Length >= sectionStart.Intendation.Length)
					ret = prev.CreateNew(key, value);
				else {
					IniFileElement el;
					bool valFound = false;
					for (int i = parent.elements.IndexOf(sectionStart) - 1; i >= 0; i--) {
						el = parent.elements[i];
						if (el is IniFileValue value1) {
							ret = value1.CreateNew(key, value);
							valFound = true;
							break;
						}
					}
					if (!valFound)
						ret = IniFileValue.FromData(key, value);
					if (ret != null && ret.Intendation.Length < sectionStart.Intendation.Length)
						ret.Intendation = sectionStart.Intendation;
				}
			} else
				ret = IniFileValue.FromData(key, value);
			if(ret != null) { 
				if (prev == null) {
					elements.Insert(elements.IndexOf(sectionStart) + 1, ret);
					parent.elements.Insert(parent.elements.IndexOf(sectionStart) + 1, ret);
				} else {
					elements.Insert(elements.IndexOf(prev) + 1, ret);
					parent.elements.Insert(parent.elements.IndexOf(prev) + 1, ret);
				}
			}
		}
		internal IniFileValue? LastValue() {
			for (int i = elements.Count - 1; i >= 0; i--) {
				if (elements[i] is IniFileValue value)
					return value;
			}
			return null;
		}
		internal IniFileValue? FirstValue() {
			for (int i = 0; i < elements.Count; i++) {
				if (elements[i] is IniFileValue value)
					return value;
			}
			return null;
		}
		/// <summary>Gets an array of names of values in this section.</summary>
		public System.Collections.ObjectModel.ReadOnlyCollection<string> GetKeys() {
			List<string> list = new(elements.Count);
			for (int i = 0; i < elements.Count; i++)
				if (elements[i] is IniFileValue value)
					list.Add(value.Key);
			return new System.Collections.ObjectModel.ReadOnlyCollection<string>(list); ;
		}
		/// <summary>Gets a string representation of this IniFileSectionReader object.</summary>
		public override string ToString() {
			return sectionStart.ToString() + " (" + elements.Count.ToString() + " elements)";
		}
		/// <summary>Formats whole section.</summary>
		/// <param name="preserveIntendation">Determines whether intendation should be preserved.</param>
		public void Format(bool preserveIntendation) {
			IniFileElement el;
			string lastIntend;
			for (int i = 0; i < elements.Count; i++) {
				el = elements[i];
				lastIntend = el.Intendation;
				el.FormatDefault();
				if (preserveIntendation)
					el.Intendation = lastIntend;
			}
		}
	}
}
