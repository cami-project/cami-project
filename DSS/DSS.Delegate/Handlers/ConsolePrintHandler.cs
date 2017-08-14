using System;

namespace DSS.Delegate
{
    public class ConsolePrintHandler<T> : IRouterHandler<T> where T : Event
    {
		public string Name { get { return "CONSOLE"; } }

		public void Handle(T obj)
		{
            Console.WriteLine("Handled by CONSOLE channel: " + obj.ToString());
		}
    }
}
