using System;
using System.Collections.Generic;

namespace DSS.Delegate
{
	public class Channel<T>
	{
		private List<T> Types;
		public string Name;

		public Channel(string name)
		{
			Types = new List<T>();
			this.Name = name;
		}

		public void Push(T obj)
		{
			Types.Add(obj);
		}

		public bool Contains(T obj)
		{
			return Types.Contains(obj);

		}

	}
}
