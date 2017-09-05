using System;

namespace DSS.Delegate
{
    public class ConsolePrintHandler<T> : IRouterHandler
    {
		public string Name { get { return "CONSOLE"; } }

        public void Handle(object obj)
		{
            Console.WriteLine("Handled by CONSOLE channel: " + obj.ToString());
		}
    }
}
