using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CryptoBalls.IniFiles {
	/// <summary>Object model for INI file, which stores a whole structure in memory.</summary>
	public class IniFile {
		internal List<IniFileSection> sections = new();
		internal List<IniFileElement> elements = new();

		/// <summary>Creates new instance of IniFile.</summary>
		public IniFile() {
		}
		/// <summary>Gets a IniFileSection object from it's name</summary>
		/// <param name="sectionName">Name of section to search for. If not found, new one is created.</param>
		public IniFileSection this[string sectionName] {
			get {
				IniFileSection? sect = GetSection(sectionName);
				if (sect != null)
					return sect;
				IniFileSectionStart start;
				if (sections.Count > 0) {
					IniFileSectionStart prev = sections[^1].sectionStart;
					start = prev.CreateNew(sectionName);
				} else
					start = IniFileSectionStart.FromName(sectionName);
				elements.Add(start);
				sect = new IniFileSection(this, start);
				sections.Add(sect);
				return sect;
			}
		}

		IniFileSection? GetSection(string name) {
			string lower = name.ToLowerInvariant();
			for (int i = 0; i < sections.Count; i++)
				if (sections[i].Name == name || (!IniFileSettings.CaseSensitive && sections[i].Name.ToLowerInvariant() == lower))
					return sections[i];
			return null;
		}
		/// <summary>Gets an array of names of sections in this INI file.</summary>
		public string[] GetSectionNames() {
			string[] ret = new string[sections.Count];
			for (int i = 0; i < sections.Count; i++)
				ret[i] = sections[i].Name;
			return ret;
		}

		/// <summary>Reads a INI file from a file or creates one.</summary>
		public static IniFile FromFile(string path) {
			if (!File.Exists(path)) {
				File.Create(path).Close();
				return new IniFile();
			}
			IniFileReader reader = new(path);
			IniFile ret = FromStream(reader);
			reader.Close();
			return ret;
		}
		/// <summary>Creates a new IniFile from elements collection (Advanced member).</summary>
		/// <param name="elemes">Elements collection.</param>
		public static IniFile FromElements(IEnumerable<IniFileElement> elemes) {
			IniFile ret = new();
			ret.elements.AddRange(elemes);
			if (ret.elements.Count > 0) {
				IniFileSection? section = null;
				IniFileElement el;

				if (ret.elements[^1] is IniFileBlankLine)
					ret.elements.RemoveAt(ret.elements.Count - 1);
				for (int i = 0; i < ret.elements.Count; i++) {
					el = ret.elements[i];
					if (el is IniFileSectionStart start) {
						section = new IniFileSection(ret, start);
						ret.sections.Add(section);
					} else if (section != null)
						section.elements.Add(el);
					else if (ret.sections.Exists(delegate (IniFileSection a) { return a.Name == ""; }))
						ret.sections[0].elements.Add(el);
					else if (el is IniFileValue) {
						section = new IniFileSection(ret, IniFileSectionStart.FromName(""));
						section.elements.Add(el);
						ret.sections.Add(section);
					}
				}
			}
			return ret;
		}
		/// <summary>Reads a INI file from a stream.</summary>
		public static IniFile FromStream(IniFileReader reader) {
			if (reader == null) throw new Exception("reader cannot be null.");
			var theRead = reader.ReadElementsToEnd();
			return FromElements(theRead);						
		}
		/// <summary>Writes a INI file to a disc, using options in IniFileSettings class</summary>
		public void Save(string path) {
      IniFileWriter writer = new(path);
			Save(writer);
			writer.Close();
		}
		/// <summary>Writes a INI file to a stream, using options in IniFileSettings class</summary>
		public void Save(IniFileWriter writer) {
			writer.WriteIniFile(this);
		}
		/// <summary>Deletes a section and all it's values and comments. No exception is thrown if there is no section of requested name.</summary>
		/// <param name="name">Name of section to delete.</param>
		public void DeleteSection(string name) {
			IniFileSection? section = GetSection(name);
			if (section == null)
				return;
			IniFileSectionStart sect = section.sectionStart;
			elements.Remove(sect);
			for (int i = elements.IndexOf(sect) + 1; i < elements.Count; i++) {
				if (elements[i] is IniFileSectionStart)
					break;
				elements.RemoveAt(i);
			}
		}
		/// <summary>Formats whole INI file.</summary>
		/// <param name="preserveIntendation">If true, old intendation will be standarized but not removed.</param>
		public void Format(bool preserveIntendation) {
			string lastSectIntend = "";
			string lastValIntend = "";
			IniFileElement el;
			for (int i = 0; i < elements.Count; i++) {
				el = elements[i];
				if (preserveIntendation) {
					if (el is IniFileSectionStart)
						lastValIntend = lastSectIntend = el.Intendation;
					else if (el is IniFileValue)
						lastValIntend = el.Intendation;
				}
				el.FormatDefault();
				if (preserveIntendation) {
					if (el is IniFileSectionStart)
						el.Intendation = lastSectIntend;
					else if (el is IniFileCommentary && i != elements.Count - 1 && elements[i + 1] is not IniFileBlankLine)
						el.Intendation = elements[i + 1].Intendation;
					else
						el.Intendation = lastValIntend;
				}
			}
		}
		/// <summary>Joins sections which are definied more than one time.</summary>
		public void UnifySections() {
			Dictionary<string, int> dict = new();
			IniFileSection sect;
			IniFileElement el;
			IniFileValue? val = null;
			int index;
			for (int i = 0; i < sections.Count; i++) {
				sect = sections[i];
				if (dict.ContainsKey(sect.Name)) {
					index = dict[sect.Name] + 1;
					elements.Remove(sect.sectionStart);
					sections.Remove(sect);
					for (int j = sect.elements.Count - 1; j >= 0; j--) {
						el = sect.elements[j];
						if (!(j == sect.elements.Count - 1 && el is IniFileCommentary))
							elements.Remove(el);
						if (el is not IniFileBlankLine) {
							elements.Insert(index, el);
							var thisSectName = this[sect.Name];
							if (thisSectName != null) {
								val = thisSectName.FirstValue();
							}							
							if (val != null)
								el.Intendation = val.Intendation;
							else {
                if (thisSectName != null) el.Intendation = thisSectName.sectionStart.Intendation;
							}								
						}
					}
				} else
					dict.Add(sect.Name, elements.IndexOf(sect.sectionStart));
			}
		}
		/// <summary>Gets or sets a header commentary of an INI file. Header comment must if separate from
		/// comment of a first section except when IniFileSetting.SeparateHeader is set to false.</summary>
		public string Header {
			get {
				if (elements.Count > 0)
					if (elements[0] is IniFileCommentary commentary && !(!IniFileSettings.SeparateHeader
						&& elements.Count > 1 && elements[1] is not IniFileBlankLine))
						return commentary.Comment;
				return "";
			}
			set {
				if (elements.Count > 0 && elements[0] is IniFileCommentary commentary && !(!IniFileSettings.SeparateHeader
					&& elements.Count > 1 && elements[1] is not IniFileBlankLine)) {
					if (value == "") {
						elements.RemoveAt(0);
						if (IniFileSettings.SeparateHeader && elements.Count > 0 && elements[0] is IniFileBlankLine)
							elements.RemoveAt(0);
					} else
						commentary.Comment = value;
				} else if (value != "") {
					if ((elements.Count == 0 || elements[0] is not IniFileBlankLine) && IniFileSettings.SeparateHeader)
						elements.Insert(0, new IniFileBlankLine(1));
					elements.Insert(0, IniFileCommentary.FromComment(value));
				}
			}
		}
		/// <summary>Gets or sets a commentary at the end of an INI file.</summary>
		public string Foot {
			get {
				if (elements.Count > 0) {
					if (elements[^1] is IniFileCommentary commentary)
						return commentary.Comment;
				}
				return "";
			}
			set {
				if (value == "") {
					if (elements.Count > 0 && elements[^1] is IniFileCommentary) {
						elements.RemoveAt(elements.Count - 1);
						if (elements.Count > 0 && elements[^1] is IniFileBlankLine)
							elements.RemoveAt(elements.Count - 1);
					}
				} else {
					if (elements.Count > 0) {
						if (elements[^1] is IniFileCommentary commentary)
							commentary.Comment = value;
						else
							elements.Add(IniFileCommentary.FromComment(value));
						if (elements.Count > 2) {
							if (elements[^2] is not IniFileBlankLine && IniFileSettings.SeparateHeader) { 
								elements.Insert(elements.Count - 1, new IniFileBlankLine(1));
							}
							else if (value == "")
								elements.RemoveAt(elements.Count - 2);
						}
					} else
						elements.Add(IniFileCommentary.FromComment(value));
				}
			}
		}
	}
	
}
