﻿using System;
using DSS.RMQ;
using Newtonsoft.Json;

namespace DSS.Delegate
{

    public interface IRouterHandler
    {
        void Handle(string json);

	}
  
}
