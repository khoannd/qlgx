using System;

namespace Femiani.Forms.UI.Input
{
	/// <summary>
	/// Summary description for TriggerState.
	/// </summary>
	[Serializable]
	public enum TriggerState : int
	{
		None = 0,
		Show = 1,
		ShowAndConsume = 2,
		Hide = 3,
		HideAndConsume = 4,
		Select = 5,
		SelectAndConsume = 6
	}
}
