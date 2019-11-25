﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Blish_HUD.Pathing.Behaviors.Activator
{
    [AttributeUsage(AttributeTargets.Class)]
    public class BehaviorActivatorAttribute : Attribute
    {
        /// <summary>
        ///     Identifies the *-when value that loads this activator.
        /// </summary>
        /// <param name="attributeName">The name of the activator.  This match is not case-sensitive.</param>
        public BehaviorActivatorAttribute(string whenValue)
        {
            this.ActivatorName = whenValue;
        }

        public string ActivatorName { get; }

        public static IEnumerable<Type> GetTypes(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(BehaviorActivatorAttribute), true).Any())
                {
                    yield return type;
                }
            }
        }

        public static BehaviorActivatorAttribute GetAttributesOnType(Type type)
        {
            return (BehaviorActivatorAttribute) type.GetCustomAttribute(typeof(BehaviorActivatorAttribute), true);
        }
    }
}