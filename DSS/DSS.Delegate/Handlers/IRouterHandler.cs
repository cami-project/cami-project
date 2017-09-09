﻿using System;
using DSS.RMQ;
using Newtonsoft.Json;

namespace DSS.Delegate
{

    public interface IRouterHandler
    {
		string Name { get; }
        void Handle(string json);

	}
  
}
