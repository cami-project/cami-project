using System;

namespace DSS.Delegate
{

	public interface IRouterHandler<T>
	{
		void Handle(T obj);
	}

	public class ConsolePrintHandler<T> : IRouterHandler<T>
	{
		public void Handle(T obj)
		{
            Console.WriteLine(obj);
		}
	}
}
