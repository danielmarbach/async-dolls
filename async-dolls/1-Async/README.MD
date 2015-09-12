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

*
