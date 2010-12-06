This extension adds support for bbv.Common.EventBroker. This is a component that allows 
communication between components using events without knowing each other directly to
register these events themself. See http://code.google.com/p/bbvcommon/wiki/EventBroker.
It requires Ninject.Extensions.NamedScope and Ninject.Extensions.ContextPreservation.

The extension adds funtionality to create event broker instances and to register objects
at creation on one or more of these event broker instances. There are two ways to create 
an instance of an event broker:

First, global event broker instances. This type of event broker has the same lifecicle
as the kernel:
this.kernel.AddGlobalEventBroker("MyGlobalEventBroker");
this.kernel.Bind<Component1>().ToSelf().RegisterOnEventBroker("MyGlobalEventBroker");
this.kernel.Bind<Component2>().ToSelf().RegisterOnEventBroker("MyGlobalEventBroker");

Or in case one global event broker is enough:
this.kernel.AddDefaultGlobalEventBroker();
this.kernel.Bind<Component1>().ToSelf().RegisterOnGlobalEventBroker();
this.kernel.Bind<Component2>().ToSelf().RegisterOnGlobalEventBroker();


Second, local event broker instances. These instances are created whenever an instance
of its owner is created. All objects created as dependencies can use this instance to
communicate. If a second instance of the owner is created these objects have thery own
event broker so that the communication i sepatated from the first set of objects:
const string EventBrokerName = "LocalEB1";

Example:
this.kernel.Bind<Parent>().ToSelf().OwnsEventBroker(EventBrokerName);
this.kernel.Bind<Component1>().ToSelf().RegisterOnGlobalBroker(EventBrokerName);
this.kernel.Bind<Component2>().ToSelf().RegisterOnGlobalBroker(EventBrokerName);

var parent1 = this.kernel.Get<Parent>();
var parent2 = this.kernel.Get<Parent>();

In this case if parents component1 fires an event it will be receives by 
parent1.component2 but not parent2.component2 or parent2.component1

Forthermore its possible to inject the event broker to the created instances in case you
to do something more special. Therefore create a parameter that has the same name as the
event broker. The name is case insensitive. The name of the global event broker is 
GlobalEventBroker.