using System;
using System.Collections;

namespace Femiani.Forms.UI.Input
{
	/// <summary>
	/// Summary description for AutoCompleteDictionaryEntry.
	/// </summary>
	[Serializable]
	public class AutoCompleteEntry : IAutoCompleteEntry
	{

		private string[] matchStrings;
        public string[] MatchStrings
		{
			get
			{
				if (this.matchStrings == null)
				{
                    this.matchStrings = new string[] { this.DisplayName };
				}
				return this.matchStrings;
			}
		}

		private string displayName = string.Empty;
		public string DisplayName
		{
			get
			{
				return this.displayName;
			}
			set
			{
				this.displayName = value;
			}
		}

		public AutoCompleteEntry()
		{
		}

        public AutoCompleteEntry(string name, params string[] matchList)
		{
			this.displayName = name;
			this.matchStrings = matchList;
		}

		public override string ToString()
		{
			return this.DisplayName;
		}

	}
}
