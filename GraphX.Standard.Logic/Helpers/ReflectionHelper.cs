﻿using System;

namespace GraphX.Logic.Helpers
{
    public static class ReflectionHelper
    {
        public static T CreateDefaultGraphInstance<T>()
        {
            return (T)Activator.CreateInstance(typeof(T), null);
        }
    }
}