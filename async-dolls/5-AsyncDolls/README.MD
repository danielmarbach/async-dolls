# Async Dolls
#### The Dolls
Methods which itself have a parameter to the next method in the form of `Action` can be easily composed together manually.

#### The Dolls Generic
The above methods can then be composed together dynamically by enqueuing them, dequeue the first action and execute it by recursively passing in the invocation and the remaining items until the queue is empty.

#### The async Dolls
The previously shown approach relatively easy be made asynchronous by changing the method signature from `Action` to `Func<Task>`.

#### The async Dolls Generic
The algorithm is the same but this time we return the `Task` return by the next call to the previous caller.

#### What this allows us todo
With that generic async algorithm in place we can now dynamically extend our async dolls by introducing surrounding dolls which can implement cross-cutting concerns like exception handling and more.

#### The async dolls with floating state
The previous function definitions didn't allow us to float state into the continuations. Using trickery like `ThreadStatic` or `ThreadLocal` doesn't work with async. So we need a way to float state into the execution pipeline of the dolls.
