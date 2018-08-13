﻿using Hl7.Fhir.ElementModel;
using System;

namespace Hl7.Fhir.Utility
{
    

    public static class ExceptionSourceExtensions
    {
        public static void NotifyOrThrow(this ExceptionNotificationHandler handler, object source, ExceptionNotification args)
        {
            if (handler != null)
                handler(source, args);
            else if (args.Severity == ExceptionSeverity.Error)
                throw args.Exception;
        }

        public static void NotifyOrThrow(this IExceptionSource ies, object source, ExceptionNotification args)
        {
            if (ies?.ExceptionHandler != null)
                ies.ExceptionHandler(source, args);
            else if (args.Severity == ExceptionSeverity.Error)
                throw args.Exception;
        }


    public static IDisposable Catch(this IElementNavigator source, ExceptionNotificationHandler handler) =>
            source is IExceptionSource s ? s.Catch(handler) : throw new NotImplementedException("source does not implement IExceptionSource");

        public static IDisposable Catch(this IExceptionSource source, ExceptionNotificationHandler handler, bool forward=false) => new ExceptionInterceptor(source, handler, forward);

        private class ExceptionInterceptor : IDisposable
        {
            private readonly IExceptionSource _source;
            private readonly ExceptionNotificationHandler _originalHandler;
            private bool _forward;

            public ExceptionInterceptor(IExceptionSource source, ExceptionNotificationHandler handler, bool forward)
            {
                _source = source;
                _originalHandler = source.ExceptionHandler;
                source.ExceptionHandler = nestedHandler;
                _forward = forward;

                void nestedHandler(object s, ExceptionNotification a)
                {
                    handler(s, a);

                    if(forward)  _originalHandler.NotifyOrThrow(s, a);
                }
            }

            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        _source.ExceptionHandler = _originalHandler;
                    }

                    disposedValue = true;
                }
            }

            // This code added to correctly implement the disposable pattern.
            void IDisposable.Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
            }
            #endregion
        }
    }
}
