using System;

namespace GIGXR.Platform.Utilities
{
    public static class EventHandlerGenericExtensions
    {
        public static void InvokeSafely<T>(this EventHandler<T> eventHandler, object sender, T eventArgs)
        {
            if (eventHandler != null)
            {
                foreach (EventHandler<T> handler in eventHandler.GetInvocationList())
                {
                    try
                    {
                        handler.DynamicInvoke(sender, eventArgs);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error in the handler {handler.Method.Name}: {e.Message}");
                    }
                }
            }
        }
    }

    public static class EventHandlerExtensions
    {
        public static void InvokeSafely<T>(this EventHandler eventHandler, object sender, T eventArgs)
        {
            if (eventHandler != null)
            {
                foreach (EventHandler handler in eventHandler.GetInvocationList())
                {
                    try
                    {
                        handler.DynamicInvoke(sender, eventArgs);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error in the handler {handler.Method.Name}: {e.Message}");
                    }
                }
            }
        }
    }
}