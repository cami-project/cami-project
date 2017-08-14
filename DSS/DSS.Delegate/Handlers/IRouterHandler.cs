﻿using System;
using DSS.RMQ;
using Newtonsoft.Json;

namespace DSS.Delegate
{
    
	public interface IRouterHandler<T>
	{ 
        string Name { get; }
		void Handle(T obj);
	}
}
