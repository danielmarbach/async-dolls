# AsyncTransactions
### The secret of transactions and async/await

Did you ever ask yourself, when using async/await together with Entity Framework and transactions to load data from your database, what kind of black magic Entity Framework is pulling off behind the scenes? Or do you need to write custom code which participates asynchronously within a transaction? Together we explore the inner workings of transactions. We learn how to leverage transactions with asynchronous code and get to know why there is no way around async void when working with transactions. After this session there will be no more black magic.

#### Intro
Disclaimer first: If you are attending this presentation to learn something about Entity Framework then shutdown your computer and got to bed! This presentation is not about EntityFramework. It is more a deep dive into the curiosity of TransactionScope and async/await. I'm sure you will learn a lot of unnecessary stuff which makes you the king story teller in your local geek bar.

#### Brief refresh
The .NET platform has under `System.Transactions` a very nifty class called `TransactionScope`. The `TransactionScope` allows you to wrap your database code, your infrastructure code and sometimes even third-party code (if supported by the third-party library) inside a transaction and only perform the actions when you actually want to commit (or complete) the transaction. As long as all the code inside the `TransactionScope` is executed on the same thread, all the code on the callstack can participate with the `TransactionScope` defined in your code. You can nest scopes, create new independent scopes inside a parent transaction scope or even creates clones of a `TransactionScope` and pass the clone to another thread and join back onto the calling thread but all this is not part of this talk. By wrapping your code with a transaction scope you are using an implicit transaction model or also called ambient transactions. The benefits and also drawbacks of the `TransactionScope` (depends how you want to see it) is that the local transaction automatically escalates to a distributed transaction if necessary. The scope also simplifies programming with transactions if you favour implicit over explicit. There are a few caveats:

* It has a limited usefulness in cloud scenarios because the cloud doesn't support distributed transactions **phew! We can finally get rid of that beast**
* It only works with async/await in .NET 4.5.1

#### TransactionScope and Async
When async/await was introduced with C# 5.0 and .NET 4.5 Microsoft completely forgot one tiny little detail. The statemachine introduced didn't properly "float" the transaction around when an async method was called under a wrapping `TransactionScope`. So in order to make `TransactionScope` and async work properly you need to upgrade to .NET 4.5.1.

#### TransactionScope and Async (proper)
With .NET 4.5.1 the `TransactionScope` has a new enum which can be provided in the constructor.

`TransactionScopeAsyncFlowOption.Enabled`
Specifies that transaction flow across thread continuations is enabled.

the default is `Suppress` (Non breaking changes you know!)

#### Store Async and THE database
For the sake of this presentation let's assume we are building our own NoSQL database. Side note: Never, ever do that unless you really know what you are doing! The database needs to store objects in memory and as soon as `SaveAsync` is called it should write it into the underlying storage. Let us briefly explore the code.

#### Store Async and TransactionScope
Now suppose we want to add ambient transaction support to our NoSQL database. How could we do that? Well nothing simpler than that. We need to write an `IEnlistmentNotification` implementation which we enlist volatile (meaning it cannot recover from failures in case when something unforeseen happened to the resource manager). I don't want to see heads exploding. Therefore I'm not diving more into resource manager, single phase and multi phase commits...

Let us briefly explore the code.

##### Synchronous way
We are voilating an important best practice of async/await called Async all the way. “Async all the way” means that you shouldn’t mix synchronous and asynchronous code without carefully considering the consequences. In particular, it’s usually a bad idea to block on async code by calling `Task.Wait` or `Task.Result`. You risk running into deadlocks. Google for “Why does my partially async code deadlock?”, by far the most-asked question by newcomers to async.

The root cause of this deadlock is due to the way await handles contexts. By default, when an incomplete Task is awaited, the current “context” is captured and used to resume the method when the Task completes. This “context” is the current `SynchronizationContext` unless it’s null, in which case it’s the current `TaskScheduler`. When the await completes, it attempts to execute the remainder of the async method within the captured context. But that context already has a thread in it, which is (synchronously) waiting for the async method to complete. They’re each waiting for the other, causing a deadlock.

##### Asynchronous way
Then let's just await the operation inside the resource manager, right? Noooooooooo! There is a great rule of thumb: "Avoid async void" except in event handlers. By making the resource manager commit `async void` we completely lost control in case of exception. If the `SaveInternalAsync` method throws an exception that exception is never raised to the calling thread and therefore we risk losing important business data.

##### Asynchronous blocking way
So what can we do then if blocking and await is not an option? We can write an asynchronous pump which allows us to bridge the `void` world over the the asynchronous world. The asynchronous pump captures the currently executing `SynchronizationContex` and replaces it with a `SingleThreadedSynchronizationContext`. Then the task is asynchronously executed with a continuation which marks the `SingleThreadedSynchronizationContext` as completed. The pump blocks until all asynchronously posted callbacks are executed on the `SynchronizationContext`.

Note: The `AsyncPump` implementation could still be tweaked. For example if we know that in the majority of case we invoke multiple asynchronous methods we could extend the pump to support an enumerable of tasks. If course this change would also affect the resource manager implementations.

We are now in a better situation regarding asynchronous execution and exception marshaling but we are still blocking the calling thread and essentially losing all the benefits of async/await.

#### What did we just learn
* Async void is evil! use it carefully.
* Async all the way.
* In a cloud first and async world (no pun intended) try to avoid TransactionScope
* Modern frameworks like Entity Framework 6 and higher support their own transactions which is the recommended way and actually works properly with async.
* Use the AsyncPump if you need `TransactionScope` support in order to enlist your own transactional stuff asynchronously.
* If you are writing an async enabled library then `ConfigureAwait(false)` is your friend
