using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace AsyncDolls.Comonaden
{
    public class IncomingPipeline
    {
        readonly List<IIncomingStep> executingSteps;

        public IncomingPipeline(IEnumerable<IIncomingStep> steps)
        {
            executingSteps = new List<IIncomingStep>(steps);
        }

        public Task Invoke(IncomingContext context)
        {
            return InnerInvoke(context);
        }

        public async Task<ExceptionDispatchInfo> InnerInvoke(IncomingContext context)
        {
            var continuations = new Stack<Continuation>();
            ExceptionDispatchInfo exception = null;
            foreach (var step in executingSteps)
            {
                var continuation = Continuation.Empty;
                try
                {
                    continuation = await step.Invoke(context).ConfigureAwait(false);
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