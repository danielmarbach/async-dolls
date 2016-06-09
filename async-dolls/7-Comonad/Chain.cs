using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace AsyncDolls.Comonaden
{
    public class Chain
    {
        readonly List<ILinkElement> executingElements;

        public Chain(IEnumerable<ILinkElement> steps)
        {
            executingElements = new List<ILinkElement>(steps);
        }

        public Task Invoke(IncomingContext context)
        {
            return InnerInvoke(context);
        }

        public async Task<ExceptionDispatchInfo> InnerInvoke(IncomingContext context)
        {
            var continuations = new Stack<Continuation>();
            ExceptionDispatchInfo exception = null;
            foreach (var element in executingElements)
            {
                var continuation = Continuation.Empty;
                try
                {
                    continuation = await element.Invoke(context).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    exception = ExceptionDispatchInfo.Capture(e);
                    break;
                }
                finally
                {
                    continuations.Push(continuation);
                }
            }

            foreach (var continuation in continuations)
            {
                if (exception == null)
                {
                    try
                    {
                        await continuation.After().ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        exception = ExceptionDispatchInfo.Capture(e);
                    }
                }
                else
                {
                    try
                    {
                        if (continuation.Catch != null)
                        {
                            exception = await continuation.Catch(exception).ConfigureAwait(false);
                        }
                    }
                    catch (Exception e)
                    {
                        exception = ExceptionDispatchInfo.Capture(e);
                    }
                }

                await continuation.Finally().ConfigureAwait(false);
            }

            return exception;
        }
    }
}