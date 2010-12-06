//-------------------------------------------------------------------------------
// <copyright file="InjectEventBrokerTest.cs" company="bbv Software Services AG">
//   Copyright (c) 2010 Software Services AG
//   Remo Gloor (remo.gloor@gmail.com)
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
    using bbv.Common.EventBroker.Handlers;
    using Ninject.Extensions.ContextPreservation;
    using Ninject.Extensions.NamedScope;
#if SILVERLIGHT
#if SILVERLIGHT_MSTEST
    using MsTest.Should;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Assert = AssertWithThrows;
    using Fact = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
#else
    using UnitDriven;
    using UnitDriven.Should;
    using Assert = AssertWithThrows;
    using Fact = UnitDriven.TestMethodAttribute;
#endif
#else
    using Ninject.Extensions.bbvEventBroker.MSTestAttributes;
    using Xunit;
    using Xunit.Should;
#endif

    /// <summary>
    /// Integration tests for the EventBrokerModule
    /// </summary>
    [TestClass]
    public class InjectEventBrokerTest
    {
        /// <summary>
        /// The kernel used in the tests.
        /// </summary>
        private StandardKernel kernel;

        /// <summary>
        /// Initializes a new instance of the <see cref="InjectEventBrokerTest"/> class.
        /// </summary>
        public InjectEventBrokerTest()
        {
            this.SetUp();
        }

        /// <summary>
        /// Sets up all tests.
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
#if SILVERLIGHT
            this.kernel = new StandardKernel();
#else
            this.kernel = new StandardKernel(new NinjectSettings { LoadExtensions = false });
#endif
            this.kernel.Load(new NamedScopeModule());
            this.kernel.Load(new ContextPreservationModule());
            this.kernel.Load(new EventBrokerModule());
        }

        /// <summary>
        /// Tests the injection of the default global event broker.
        /// </summary>
        [Fact]
        public void InjectDefaultGlobalEventBroker()
        {
            this.kernel.Bind<ParentWithDefaultEventBroker>().ToSelf().RegisterOnGlobalEventBroker();
            this.kernel.Bind<Child>().ToSelf();

            var parent = this.kernel.Get<ParentWithDefaultEventBroker>();
            parent.FireSomeEvent();

            parent.FirstChild.EventReceived.ShouldBeTrue("Event was not received by child 1");
        }

        /// <summary>
        /// Tests the injection of a named global event broker.
        /// </summary>
        [Fact]
        public void InjectNamedGlobalEventBroker()
        {
            const string EventBrokerName = "EventBrokerName";
            this.kernel.AddGlobalEventBroker(EventBrokerName);
            this.kernel.Bind<Parent>().ToSelf().RegisterOnEventBroker(EventBrokerName);
            this.kernel.Bind<Child>().ToSelf();

            var parent = this.kernel.Get<Parent>();
            parent.FireSomeEvent();

            parent.FirstChild.EventReceived.ShouldBeTrue("Event was not received by child 1");
        }

        /// <summary>
        /// Tests the injection of a local event broker.
        /// </summary>
        [Fact]
        public void InjectLocalEventBroker()
        {
            const string EventBrokerName = "EventBrokerName";
            this.kernel.Bind<Parent>().ToSelf().RegisterOnEventBroker(EventBrokerName).OwnsEventBroker(EventBrokerName);
            this.kernel.Bind<Child>().ToSelf();

            var parent = this.kernel.Get<Parent>();
            parent.FireSomeEvent();

            parent.FirstChild.EventReceived.ShouldBeTrue("Event was not received by child 1");
        }

        /// <summary>
        /// A test object that gets the default global event broker injected
        /// </summary>
        public class ParentWithDefaultEventBroker : Parent
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ParentWithDefaultEventBroker"/> class.
            /// </summary>
            /// <param name="child">The child.</param>
            /// <param name="globalEventBroker">The global event broker.</param>
            public ParentWithDefaultEventBroker(Child child, IEventBroker globalEventBroker) 
                : base(child, globalEventBroker)
            {
            }
        }

        /// <summary>
        /// Test object that is able to fire an event that can be received by its children.
        /// </summary>
        public class Parent
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Parent"/> class.
            /// </summary>
            /// <param name="child">The child.</param>
            /// <param name="eventBrokerName">The event broker.</param>
            public Parent(Child child, IEventBroker eventBrokerName)
            {
                this.FirstChild = child;
                eventBrokerName.Register(child);
            }

            /// <summary>
            /// Event registered on the event broker..
            /// </summary>
            [EventPublication("SomeEventTopic")]
            public event EventHandler SomeEvent;

            /// <summary>
            /// Gets the first child.
            /// </summary>
            /// <value>The first child.</value>
            public Child FirstChild { get; private set; }

            /// <summary>
            /// Fires some event.
            /// </summary>
            public void FireSomeEvent()
            {
                if (this.SomeEvent != null)
                {
                    this.SomeEvent(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The child used in the tests
        /// </summary>
        public class Child
        {
            /// <summary>
            /// Gets a value indicating whether the event was received.
            /// </summary>
            /// <value><c>true</c> if the event was received; otherwise, <c>false</c>.</value>
            public bool EventReceived { get; private set; }

            /// <summary>
            /// Handles some event.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
            [EventSubscription("SomeEventTopic", typeof(Publisher))]
            public void HandleSomeEvent(object sender, EventArgs e)
            {
                this.EventReceived = true;
            }
        }
    }
}