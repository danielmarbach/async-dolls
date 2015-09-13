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

> `Async void` methods have different composing semantics. Async methods returning `Task` or `Task<T>` can be easily composed using `await`, `Task.WhenAny`, `Task.WhenAll` and so on. Async methods returning `void` don’t provide an easy way to notify the calling code that they’ve completed. It’s easy to start several `async void` methods, but it’s not easy to determine when they’ve finished. `Async void` methods will notify their `SynchronizationContext` when they start and finish, but a custom `SynchronizationContext` is a complex solution for regular application code.

> `Async void` methods are difficult to test. Because of the differences in error handling and composing, it’s difficult to write unit tests that call `async void` methods. The MSTest asynchronous testing support only works for async methods returning `Task` or `Task<T>`. It’s possible to install a `SynchronizationContext` that detects when all `async void` methods have completed and collects any exceptions, but it’s much easier to just make the `async void` methods return Task instead.

> It’s clear that async void methods have several disadvantages compared to async Task methods, but they’re quite useful in one particular case: asynchronous event handlers. The differences in semantics make sense for asynchronous event handlers. They raise their exceptions directly on the SynchronizationContext, which is similar to how synchronous event handlers behave.

> `Async void` methods can wreak havoc if the caller isn’t expecting them to be async. When the return type is `Task`, the caller knows it’s dealing with a future operation; when the return type is `void`, the caller might assume the method is complete by the time it returns. This problem can crop up in many unexpected ways. It’s usually wrong to provide an `async` implementation (or override) of a void-returning method on an interface (or base class). Some events also assume that their handlers are complete when they return. One subtle trap is passing an async lambda to a method taking an `Action` parameter; in this case, the async lambda returns `void` and inherits all the problems of `async void` methods. As a general rule, async lambdas should only be used if they’re converted to a delegate type that returns Task (for example, `Func<Task>`).

>To summarize this guideline

> * you should prefer async Task to async void.

>Async Task methods enable easier error-handling, composability and testability. The exception to this guideline is asynchronous event handlers, which must return void. This exception includes methods that are logically event handlers even if they’re not literally event handlers (for example, `ICommand.Execute` implementations). [Source](https://msdn.microsoft.com/en-us/magazine/jj991977.aspx)

Addition:
* NUnit and xunit support `async void` returning unit tests and they implement their own `SynchronizationContext` to capture any exceptions raised in the background. With their custom implemented `SynchronizationContext` they can also intercept the execution of the `async void` method and only mark the test as completed when that method is done.

#### ConfigureAwait(false) is your friend
> As asynchronous GUI applications grow larger, you might find many small parts of async methods all using the GUI thread as their context. This can cause sluggishness as responsiveness suffers from “thousands of paper cuts.”

> To mitigate this, await the result of `ConfigureAwait` whenever you can.

> By using `ConfigureAwait`, you enable a small amount of parallelism: Some asynchronous code can run in parallel with the GUI thread instead of constantly badgering it with bits of work to do.

Note: `ConfigureAwait(false)` doesn't mean the continuation code will never run on the original context. It might still be running on the original context if the task returned is already completed or completed quickly.

> You should not use `ConfigureAwait` when you have code after the await in the method that needs the context. For GUI apps, this includes any code that manipulates GUI elements, writes data-bound properties or depends on a GUI-specific type such as Dispatcher/CoreDispatcher. For ASP.NET apps, this includes any code that uses `HttpContext.Current` or builds an ASP.NET response, including return statements in controller actions

> Each async method has its own context, so if one async method calls another async method, their contexts are independent. [Source](https://msdn.microsoft.com/en-us/magazine/jj991977.aspx)

#### Don't mix blocking and async code
>“Async all the way” means that

>*  you shouldn’t mix synchronous and asynchronous code without carefully considering the consequences.

> In particular, it’s usually a bad idea to block on async code by calling `Task.Wait` or `Task.Result`. This is an especially common problem for programmers who are “dipping their toes” into asynchronous programming, converting just a small part of their application and wrapping it in a synchronous API so the rest of the application is isolated from the changes. Unfortunately, they run into problems with deadlocks.

> Allowing async to grow through the codebase is the best solution, but this means there’s a lot of initial work for an application to see real benefit from async code. There are a few techniques for incrementally converting a large codebase to async code, but they’re outside the scope of this article. In some cases, using Task.Wait or `Task.Result` can help with a partial conversion, but you need to be aware of the deadlock problem as well as the error-handling problem.

> Every Task will store a list of exceptions. When you await a Task, the first exception is re-thrown, so you can catch the specific exception type (such as `InvalidOperationException`). However, when you synchronously block on a Task using Task.Wait or `Task.Result`, all of the exceptions are wrapped in an `AggregateException` and thrown.

> To summarize this guideline, you should avoid mixing async and blocking code. Mixed async and blocking code can cause deadlocks, more-complex error handling and unexpected blocking of context threads. The exception to this guideline is the Main method for console applications, or—if you’re an advanced user—managing a partially asynchronous codebase.


#### Mixing concurrent and asychronous processing with potential blocking operations
A few key points from the code:

* Prefer `Task.Run` over `Task.Factory.StartNew`
* Passing the `CancellationToken` to the task creation means when the token is cancelled the task automatically switches into the cancelled state.
* For continuous polling tasks provide `TaskCreationOptions.LongRunnin`
* Check the `CancellationToken` as much as needed. For graceful cancellation use the state query API instead of throwing an `OperationCancelledException`
* `Task.Factory.StartNew` with async lambda requires an `Unwrap`!
* Don't `await` a task if you want to concurrently process
* You only need `Task.Run` if the API called is not truly async
* If you are using a custom scheduler and you are invoking user code then use `TaskCreationOptions.HideScheduler`
* For throttling `SemaphoreSlim` is quite handy
* `TaskContinuationOptions.ExecuteSynchronously` usually means the continuation is executed on the thread which handled the antecedent task before the antecedent task moves into the completed state. But this is not always true. When the thread handling the antecedent task was aborted then the continuation might still be executed on any background thread.

#### Mixing concurrent and asynchronous processing with true async operations
* If the API you are calling is truly async, then don't use `Task.Run` or `Task.Factory.StartNew`, this allows much more efficient resource usage
