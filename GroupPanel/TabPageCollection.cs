using System;
using System.Collections;

namespace Morell.GroupPanel
{
	// Declare the event signatures
	public delegate void CollectionClear();
	public delegate void CollectionChange(int index, TabPage value);
	
	/// <summary>
	/// Tabpages collection
	/// </summary>
	public class TabPageCollection : IEnumerable
	{
		// Events
		public event CollectionClear Cleared;
		public event CollectionClear Clearing;
		public event CollectionChange Deleted;
		public event CollectionChange Added;
		
		private ArrayList _tabs;
		
		/// <summary>
		/// Constructor
		/// </summary>
		public TabPageCollection()
		{
			// Initialize the collection
			_tabs = new ArrayList();
		}
		
		/// <summary>
		/// Adds a tabPage to the collection
		/// </summary>
		/// <param name="tabPage">Tabpage to add</param>
		public void Add(TabPage tabPage)
		{
			_tabs.Add(tabPage);
			OnAdded(_tabs.Count-1, tabPage);
		}
		
		/// <summary>
		/// Adds a range of tabPages
		/// </summary>
		/// <param name="values">Tabpages to add</param>
		public void AddRange(TabPage[] values)
		{
			foreach (TabPage tabPage in values)
				Add(tabPage);
		}

		/// <summary>
		/// Removes a tabPage
		/// </summary>
		/// <param name="tabPage">Tabpage to remove</param>
		public void Remove(TabPage tabPage)
		{
			int index = _tabs.IndexOf(tabPage);
			_tabs.Remove(tabPage);
			OnDeleted(index, tabPage);
		}

		/// <summary>
		/// Removes the tabPage at index
		/// </summary>
		/// <param name="index">Tabpage index to remove</param>
		public void Remove(int index)
		{
			TabPage tabPage = (TabPage) _tabs[index];
			_tabs.RemoveAt(index);
			OnDeleted(index, tabPage);
		}

		/// <summary>
		/// Clears the collection
		/// </summary>
		public void Clear()
		{
			OnCollectionClearing();
			_tabs.Clear();
			OnCollectionClear();
		}

		/// <summary>
		/// Gets the number of tabPages in the collection
		/// </summary>
		public int Count
		{
			get {return _tabs.Count;}
		}

		/// <summary>
		/// Inserts a tabPage at index
		/// </summary>
		/// <param name="index">Index to insert the tabPage</param>
		/// <param name="tabPage">Tabpage to insert</param>
		public void Insert(int index, TabPage tabPage)
		{
			_tabs.Insert(index, tabPage);
			OnAdded(index, tabPage);
		}

		/// <summary>
		/// Gets the tabPage by index
		/// </summary>
		public TabPage this[int index]
		{
			get { return (_tabs[index] as TabPage); }
		}

		/// <summary>
		/// Gets the tabPage by text
		/// </summary>
		public TabPage this[string text]
		{
			get 
			{
				foreach(TabPage page in _tabs)
					if (page.Text == text)
						return page;
				return null;
			}
		}

		/// <summary>
		/// Gets the index of the tabPage
		/// </summary>
		/// <param name="tabPage">Tabpage to gather the index</param>
		/// <returns>Tabpage index</returns>
		public int IndexOf(TabPage tabPage)
		{
			return _tabs.IndexOf(tabPage);
		}

		/// <summary>
		/// Gets the collection's IEnumerator
		/// </summary>
		/// <returns>IEnumerator</returns>
		public IEnumerator GetEnumerator()
		{
			return _tabs.GetEnumerator();
		}

		/// <summary>
		/// A tabPage was added
		/// </summary>
		/// <param name="index">Tabpage index</param>
		/// <param name="tabPage">Tabpage</param>
		private void OnAdded(int index, TabPage tabPage)
		{
			if (Added != null)
				Added(index, tabPage);
		}

		/// <summary>
		/// A tabPage was deleted
		/// </summary>
		/// <param name="index">Tabpage index</param>
		/// <param name="tabPage">Tabpage</param>
		private void OnDeleted(int index, TabPage tabPage)
		{
			if (Deleted != null)
				Deleted(index, tabPage);
		}

		/// <summary>
		/// The collection is been cleared
		/// </summary>
		private void OnCollectionClearing()
		{
			if (Clearing != null)
				Clearing();
		}

		/// <summary>
		/// The collection was cleared
		/// </summary>
		private void OnCollectionClear()
		{
			if (Cleared != null)
				Cleared();
		}
	}
}
