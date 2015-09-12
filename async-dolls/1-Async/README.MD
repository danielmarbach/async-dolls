# Async
#### Intro
My name is Daniel Marbach. I'm the CEO of tracelight GmbH in Switzerland and also working as a Solutions Architect for Particular Software, the folks behind NServiceBus.

#### Async Recap
Parralelism and Asynchronism are the same things, in fact they got often confused. Consider for example how NServiceBus allows you to control message throughput is by setting the `MaximumConcurrencyLevel` option.

The `MaximumConcurrencyLevel` limits the maximum number of message consuming threads that NServiceBus will allocate. Let us assume we set the `MaximumConcurrencyLevel`m to eight. In this case, NServiceBus will use a maximum of eight message consuming threads from the thread pool.

This is like building an eight-lane highway. There may be no cars on the road, or one, or two, but the number of lanes is fixed and at no time can there be any more than eight cars. So at any given time we can process a maximum of eight messages in parallel. Strictly speaking, we currently process messages in parallel, but not asynchronously.

Asynchronous means non-blocking, but during the consumption of those eight messages in parallel, currently each thread (and all of its allocated resources) is blocked until that message is consumed.

If we could combine parallelism with non-blocking, asynchronous execution, we could free up that message processing thread to do other work while the asynchronous operation is happening. This is where async/await comes into play.

In the case of those eight message consumption threads, every time a handler calls into an I/O bound resource (e.g. a database, web service, or the underlying transport) then that thread could go process other messages until the I/O operation completes, therefore allowing NServiceBus to more efficiently use your datacenter's resources.

You might not be that worried about better resource usage in your datacenter, but if you are running in the cloud, using resources more efficiently means potentially saving a lot of money.

In summary we can say

* CPU Bound work can benefit from parallelism
* IO Bound work should use asynchronism and seldom ever use parallelism

Translated to C# this means:
* Whenever possible use the new Task Parallel library and no longer Threads or the ThreadPool directly
* Free yourself from the mindset of `Task` equals `Thread`. This is no longer true!
* For CPU bound work use `Parallel.For`, `Parallel.ForEach`, `Task.Run` or `Task.StartNew` but prefer `Task.Run` over `Task.StartNew`. If you require to asynchronously wait for the completion you can `await` the returned tasks
* For IO bound work don't use `Task.Run` or `Task.StartNew` and `await` the asynchronous operation.

#### Async Void
> There are three possible return types for async methods: `Task`, `Task<T>` and `void`, but the natural return types for async methods are just `Task` and `Task<T>`. When converting from synchronous to asynchronous code, any method returning a type T becomes an async method returning `Task<T>`, and any method returning `void` becomes an async method returning Task.

> Void-returning async methods have a specific purpose: to make asynchronous event handlers possible.

> Async void methods have different error-handling semantics. When an exception is thrown out of an `async Task` or `async Task<T>` method, that exception is captured and placed on the Task object. With `async void` methods, there is no `Task` object, so any exceptions thrown out of an `async void` method will be raised directly on the `SynchronizationContext` that was active when the `async void` method started.

> Async void methods have different composing semantics. Async methods returning Task or Task<T> can be easily composed using await, Task.WhenAny, Task.WhenAll and so on. Async methods returning void don’t provide an easy way to notify the calling code that they’ve completed. It’s easy to start several async void methods, but it’s not easy to determine when they’ve finished. Async void methods will notify their `SynchronizationContext` when they start and finish, but a custom `SynchronizationContext` is a complex solution for regular application code.

> Async void methods are difficult to test. Because of the differences in error handling and composing, it’s difficult to write unit tests that call async void methods. The MSTest asynchronous testing support only works for async methods returning Task or Task<T>. It’s possible to install a SynchronizationContext that detects when all async void methods have completed and collects any exceptions, but it’s much easier to just make the async void methods return Task instead.

> It’s clear that async void methods have several disadvantages compared to async Task methods, but they’re quite useful in one particular case: asynchronous event handlers. The differences in semantics make sense for asynchronous event handlers. They raise their exceptions directly on the SynchronizationContext, which is similar to how synchronous event handlers behave.

> Async void methods can wreak havoc if the caller isn’t expecting them to be async. When the return type is Task, the caller knows it’s dealing with a future operation; when the return type is void, the caller might assume the method is complete by the time it returns. This problem can crop up in many unexpected ways. It’s usually wrong to provide an async implementation (or override) of a void-returning method on an interface (or base class). Some events also assume that their handlers are complete when they return. One subtle trap is passing an async lambda to a method taking an Action parameter; in this case, the async lambda returns void and inherits all the problems of async void methods. As a general rule, async lambdas should only be used if they’re converted to a delegate type that returns Task (for example, Func<Task>).

>To summarize this guideline

> * you should prefer async Task to async void.

>Async Task methods enable easier error-handling, composability and testability. The exception to this guideline is asynchronous event handlers, which must return void. This exception includes methods that are logically event handlers even if they’re not literally event handlers (for example, `ICommand.Execute` implementations). [Source](https://msdn.microsoft.com/en-us/magazine/jj991977.aspx)
