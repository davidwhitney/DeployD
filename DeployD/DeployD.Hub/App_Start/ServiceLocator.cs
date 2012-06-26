using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ninject;

namespace DeployD.Hub.App_Start
{
    public static class ServiceLocator
    {
        public static IKernel Instance { get; private set; }

        public static void Initialize(IKernel kernel)
        {
            Instance = kernel;
        }
    }
}