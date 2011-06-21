//-------------------------------------------------------------------------------
// <copyright file="IntegrationTests.cs" company="bbv Software Services AG">
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

    using FluentAssertions;

    using Ninject.Extensions.ContextPreservation;
    using Ninject.Extensions.NamedScope;
    using Xunit;

    /// <summary>
    /// Integration tests for the EventBrokerModule
    /// </summary>
    public class IntegrationTests
    {
        /// <summary>
        /// The kernel used in the tests.
        /// </summary>
        private StandardKernel kernel;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationTests"/> class.
        /// </summary>
        public IntegrationTests()
        {
            this.SetUp();
        }

        /// <summary>
        /// Sets up the tests.
        /// </summary>
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
        /// Objects that are configured to be registered on a global event broker can communicate
        /// using event broker events.
        /// </summary>
        [Fact]
        public void RegisterOnGlobalEventBroker()
        {
            const string EventBrokerName = "GlobalEventBroker2";
            this.kernel.AddGlobalEventBroker(EventBrokerName);
            this.kernel.Bind<Parent>().ToSelf().RegisterOnEventBroker(EventBrokerName);
            this.kernel.Bind<Child>().ToSelf().Named("FirstChild").RegisterOnEventBroker(EventBrokerName);
            this.kernel.Bind<Child>().ToSelf().Named("SecondChild");

            var parent = this.kernel.Get<Parent>();
            parent.FireSomeEvent();
                       
            parent.FirstChild.EventReceived.Should().BeTrue("Event was not received by child 1");
            parent.SecondChild.EventReceived.Should().BeFalse("Event was received by child 2");
        }

        /// <summary>
        /// Objects that are configured to be registered on a global event broker can communicate
        /// using event broker events.
        /// </summary>
        [Fact]
        public void RegisterOnDefaultGlobalEventBroker()
        {
            const string EventBrokerName = "GlobalEventBroker2";
            this.kernel.AddGlobalEventBroker(EventBrokerName);
            this.kernel.Bind<Parent>().ToSelf().RegisterOnGlobalEventBroker();
            this.kernel.Bind<Child>().ToSelf().Named("FirstChild").RegisterOnGlobalEventBroker();
            this.kernel.Bind<Child>().ToSelf().Named("SecondChild").RegisterOnEventBroker(EventBrokerName);

            var parent = this.kernel.Get<Parent>();
            parent.FireSomeEvent();

            parent.FirstChild.EventReceived.Should().BeTrue("Event was not received by child 1");
            parent.SecondChild.EventReceived.Should().BeFalse("Event was received by child 2");
        }

        /// <summary>
        /// Objects that are configured to be registered on a local event broker can communicate
        /// using event broker events. Objects on an other instance of this local event broker
        /// do not receive the events.
        /// </summary>
        [Fact]
        public void RegisterOnLocalEventBroker()
        {
            const string EventBrokerName = "LocalEventBroker";
            this.kernel.Bind<Foo>().ToSelf();
            this.kernel.Bind<Parent>().ToSelf().RegisterOnEventBroker(EventBrokerName).OwnsEventBroker(EventBrokerName);
            this.kernel.Bind<Child>().ToSelf().Named("FirstChild").RegisterOnEventBroker(EventBrokerName);
            this.kernel.Bind<Child>().ToSelf().Named("SecondChild").RegisterOnEventBroker(EventBrokerName);

            var foo = this.kernel.Get<Foo>();
            foo.Parent1.FireSomeEvent();

            foo.Parent1.FirstChild.EventReceived.Should().BeTrue("Event was not received by parent1.child1");
            foo.Parent1.SecondChild.EventReceived.Should().BeTrue("Event was not received by parent1.child2");
            foo.Parent2.FirstChild.EventReceived.Should().BeFalse("Event was received by parent2.child1");
            foo.Parent2.SecondChild.EventReceived.Should().BeFalse("Event was received by parent2.child2");
        }

        /// <summary>
        /// Test class
        /// </summary>
        public class Foo
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Foo"/> class.
            /// </summary>
            /// <param name="parent1">The parent1.</param>
            /// <param name="parent2">The parent2.</param>
            public Foo(Parent parent1, Parent parent2)
            {
                this.Parent1 = parent1;
                this.Parent2 = parent2;
            }

            /// <summary>
            /// Gets the first parent.
            /// </summary>
            /// <value>The first parent.</value>
            public Parent Parent1 { get; private set; }

            /// <summary>
            /// Gets the second parent.
            /// </summary>
            /// <value>The second parent.</value>
            public Parent Parent2 { get; private set; }
        }

        /// <summary>
        /// Test object that is able to fire an event that can be received by its children.
        /// </summary>
        public class Parent
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Parent"/> class.
            /// </summary>
            /// <param name="firstChild">The first child.</param>
            /// <param name="secondChild">The second child.</param>
            public Parent(
                [Named("FirstChild")]Child firstChild,
                [Named("SecondChild")]Child secondChild)
            {
                this.FirstChild = firstChild;
                this.SecondChild = secondChild;
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
            /// Gets the second child.
            /// </summary>
            /// <value>The second child.</value>
            public Child SecondChild { get; private set; }

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