//-------------------------------------------------------------------------------
// <copyright file="EventBrokerModule.cs" company="bbv Software Services AG">
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
    using System;

    using bbv.Common.EventBroker;

    using Ninject.Extensions.ContextPreservation;
    using Ninject.Extensions.NamedScope;
    using Ninject.Modules;

    /// <summary>
    /// Module for the event broker extension.
    /// </summary>
    public class EventBrokerModule : NinjectModule
    {
        /// <summary>
        /// Loads the module into the kernel.
        /// </summary>
        public override void Load()
        {
            if (!this.Kernel.HasModule(typeof(NamedScopeModule).FullName))
            {
                throw new InvalidOperationException("The EventBrokerModule requires NamedScopeModule!");
            }

            if (!this.Kernel.HasModule(typeof(ContextPreservationModule).FullName))
            {
                throw new InvalidOperationException("The EventBrokerModule requires ContextPreservationModule!");
            }

            this.Kernel.Bind<IEventBroker>().To<EventBroker>().InSingletonScope();
        }
    }
}