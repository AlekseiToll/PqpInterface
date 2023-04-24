using System;

namespace Morell.GroupPanel
{
	/// <summary>
	/// Property changed event
	/// </summary>
	public delegate void PropChangeHandler(TabPage tabPage, Property prop, object oldValue);

	/// <summary>
	/// Property that has changed
	/// </summary>
	public enum Property
	{
		Text,
		ImageIndex
	}
}
