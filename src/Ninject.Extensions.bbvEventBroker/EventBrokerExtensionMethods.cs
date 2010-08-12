//-------------------------------------------------------------------------------
// <copyright file="EventBrokerExtensionMethods.cs" company="bbv Software Services AG">
//   Copyright (c) 2008 bbv Software Services AG
//   Author: Remo Gloor remo.gloor@bbv.ch
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
//-------------------------------------------------------------------------------

namespace Ninject.Extensions.bbvEventBroker
{
    using bbv.Common.EventBroker;
    using Ninject.Extensions.ContextPreservation;
    using Ninject.Extensions.NamedScope;
    using Ninject.Syntax;

    /// <summary>
    /// Extension methods for registering objects on the event broker.
    /// </summary>
    public static class EventBrokerExtensionMethods
    {
        /// <summary>
        /// Defines that the object created by the binding shall be registered on the specified event broker.
        /// </summary>
        /// <typeparam name="T">The type of the binding.</typeparam>
        /// <param name="syntax">The syntax.</param>
        /// <param name="eventBrokerName">Name of the event broker.</param>
        /// <returns>The syntax</returns>
        public static IBindingOnSyntax<T> RegisterOnEventBroker<T>(
            this IBindingOnSyntax<T> syntax, string eventBrokerName)
        {
            return
                syntax.OnActivation((ctx, instance) => ctx.ContextPreservingGet<IEventBroker>(eventBrokerName).Register(instance))
                      .OnDeactivation((ctx, instance) => ctx.ContextPreservingGet<IEventBroker>(eventBrokerName).Unregister(instance));
        }

        /// <summary>
        /// Defines that the object created by the binding shall be registered on the default global event broker.
        /// </summary>
        /// <typeparam name="T">The type of the binding.</typeparam>
        /// <param name="syntax">The syntax.</param>
        /// <returns>The syntax</returns>
        public static IBindingOnSyntax<T> RegisterOnGlobalEventBroker<T>(
            this IBindingOnSyntax<T> syntax)
        {
            return RegisterOnEventBroker(syntax, null);
        }

        /// <summary>
        /// Adds a global event broker to the kernel.
        /// </summary>
        /// <param name="bindingRoot">The binding root.</param>
        /// <param name="eventBrokerName">Name of the event broker.</param>
        public static void AddGlobalEventBroker(this IBindingRoot bindingRoot, string eventBrokerName)
        {
            bindingRoot.Bind<IEventBroker>().To<EventBroker>().InSingletonScope().Named(eventBrokerName);
        }

        /// <summary>
        /// Defines that the object created by a binding owns an event broker.
        /// Object created in the object tree below this binding can use this event broker.
        /// </summary>
        /// <typeparam name="T">The type of the binding.</typeparam>
        /// <param name="syntax">The syntax.</param>
        /// <param name="eventBrokerName">Name of the event broker.</param>
        /// <returns>The syntax</returns>
        public static IBindingOnSyntax<T> OwnsEventBroker<T>(this IBindingOnSyntax<T> syntax, string eventBrokerName)
        {
            string namedScopeName = "EventBrokerScope" + eventBrokerName;
            syntax.DefinesNamedScope(namedScopeName);
            syntax.Kernel.Bind<IEventBroker>().To<EventBroker>().InNamedScope(namedScopeName).Named(eventBrokerName);
            return syntax;
        }
    }
}