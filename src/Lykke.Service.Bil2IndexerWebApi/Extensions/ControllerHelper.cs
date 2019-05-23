using System;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Extensions
{
    public static class ControllerHelper
    {
        public static string GetShortName<T>() where T : ControllerBase
        {
            return GetShortName(typeof(T));
        }

        public static string GetShortName(Type controllerType)
        {
            if (!controllerType.IsSubclassOf(typeof(ControllerBase)))
                throw new InvalidOperationException($"{controllerType.Name} must derive from {typeof(ControllerBase).Name}");

            const string controllerPostfix = "Controller";
            var name = controllerType.Name;
            return name.EndsWith(controllerPostfix)
                ? name.Substring(0, name.Length - controllerPostfix.Length)
                : name;
        }
    }
}
