using System;

namespace DSS.Delegate
{
    public class ConsolePrintHandler<T> : IRouterHandler
    {
		public string Name { get { return "CONSOLE"; } }

        public void Handle(string json)
		{
            Console.WriteLine("Handled by CONSOLE channel: " + json);
		}
    }
}
