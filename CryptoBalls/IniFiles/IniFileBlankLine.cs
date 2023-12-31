﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBalls.IniFiles {
	/// <summary>Represents one or more blank lines within a config file.</summary>
	public class IniFileBlankLine : IniFileElement {
		/// <summary>Initializes a new instance IniFileBlankLine</summary>
		/// <param name="amount">Number of blank lines.</param>
		public IniFileBlankLine(int amount)
			: base("") {
			Amount = amount;
		}
		/// <summary>Gets or sets a number of blank lines.</summary>
		public int Amount {
			get { return Line.Length / Environment.NewLine.Length + 1; }
			set {
				if (value < 1)
					throw new ArgumentOutOfRangeException("Amount", "Amount to less than 1.");
				StringBuilder build = new();
				for (int i = 1; i < value; i++)
					build.Append(Environment.NewLine);
				Content = build.ToString();
			}
		}
		/// <summary>Determines whether specified string is a representation of particular IniFileElement object.</summary>
		/// <param name="testString">Trimmed test string.</param>
		public static bool IsLineValid(string testString) {
			return testString == "";
		}
		/// <summary>Gets a string representation of this IniFileBlankLine object.</summary>
		public override string ToString() {
			return Amount.ToString() + " blank line(s)";
		}
		/// <summary>Formats the IniFileElement object using directions in IniFileSettings.</summary>
		public override void FormatDefault() {
			Amount = 1;
			base.FormatDefault();
		}
	}
}
