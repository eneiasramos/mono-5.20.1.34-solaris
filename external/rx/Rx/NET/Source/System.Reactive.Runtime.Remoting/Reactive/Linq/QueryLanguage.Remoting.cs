// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NO_REMOTING
using System.Reactive.Disposables;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Security;
using System.Threading;

//
// DESIGN: The MarshalByRefObject (MBRO) implementations for RemotableObserver and RemotableSubscription act as
//         self-sponsoring objects controlling their lease times in order to tie those to the lifetime of the
//         underlying observable sequence (ended by OnError or OnCompleted) or the user-controlled subscription
//         lifetime. If we were to implement InitializeLifetimeService to return null, we'd end up with leases
//         that are infinite, so we need a more fine-grained lease scheme. The default configuration would time
//         out after 5 minutes, causing clients to fail while they're still observing the sequence. To solve
//         this, those MBROs also implement ISponsor with a Renewal method that continues to renew the lease
//         upon every call. When the sequence comes to an end or the subscription is disposed, the sponsor gets
//         unregistered, allowing the objects to be reclaimed eventually by the Remoting infrastructure.
//
// SECURITY: Registration and unregistration of sponsors is protected by SecurityCritical annotations. The
//           implementation of ISponsor is known (i.e. no foreign implementation can be passed in) at the call
//           sites of the Register and Unregister methods. The call to Register happens in the SecurityCritical
//           InitializeLifetimeService method and is called by trusted Remoting infrastructure. The Renewal
//           method is also marked as SecurityCritical and called by Remoting. The Unregister method is wrapped
//           in a ***SecurityTreatAsSafe*** private method which only gets called by the observer's OnError and
//           OnCompleted notifications, or the subscription's Dispose method. In the former case, the sequence
//           indicates it has reached the end, and hence resources can be reclaimed. Clients will no longer be
//           connected to the source due to auto-detach behavior enforced in the SerializableObservable client-
//           side implementation. In the latter case of disposing the subscription, the client is in control
//           and will cause the underlying remote subscription to be disposed as well, allowing resources to be
//           reclaimed. Rogue messages on either the data or the subscription channel can cause a DoS of the
//           client-server communication but this is subject to the security of the Remoting channels used. In
//           no case an untrusted party can cause _extension_ of the lease time.
//
//
// Notice this assembly is marked as APTCA in official builds, causing methods to be treated as transparent,
// thus requiring the ***SecurityTreatAsSafe*** annotation on the security boundaries described above. When not
// applied, the following exception would occur at runtime:
//
//    System.MethodAccessException:
//
//    Attempt by security transparent method 'System.Reactive.Linq.QueryLanguage+RemotableObservable`1+
//    RemotableSubscription<T>.Unregister()' to access security critical method 'System.Runtime.Remoting.Lifetime.
//    ILease.Unregister(System.Runtime.Remoting.Lifetime.ISponsor)' failed.
//
//    Assembly 'System.Reactive.Linq, Version=2.0.ymmdd.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
//    is marked with the AllowPartiallyTrustedCallersAttribute, and uses the level 2 security transparency model.
//    Level 2 transparency causes all methods in AllowPartiallyTrustedCallers assemblies to become security
//    transparent by default, which may be the cause of this exception.
//
//
// The two CodeAnalysis suppressions below are explained by the Justification property (scroll to the right):
//
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2136:TransparencyAnnotationsShouldNotConflictFxCopRule", Scope = "member", Target = "System.Reactive.Linq.QueryLanguage+RemotableObserver`1.#Unregister()", Justification = "This error only occurs while running FxCop on local builds that don't have NO_CODECOVERAGE set, causing the assembly not to be marked with APTCA (see AssemblyInfo.cs). When APTCA is enabled in official builds, this SecurityTreatAsSafe annotation is required.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2136:TransparencyAnnotationsShouldNotConflictFxCopRule", Scope = "member", Target = "System.Reactive.Linq.QueryLanguage+RemotableObservable`1+RemotableSubscription.#Unregister()", Justification = "This error only occurs while running FxCop on local builds that don't have NO_CODECOVERAGE set, causing the assembly not to be marked with APTCA (see AssemblyInfo.cs). When APTCA is enabled in official builds, this SecurityTreatAsSafe annotation is required.")]

namespace System.Reactive.Linq
{
    public static partial class RemotingObservable
    {
        #region Remotable

        private static IObservable<TSource> Remotable_<TSource>(IObservable<TSource> source)
        {
            return new SerializableObservable<TSource>(new RemotableObservable<TSource>(source, null));
        }

        private static IObservable<TSource> Remotable_<TSource>(IObservable<TSource> source, ILease lease)
        {
            return new SerializableObservable<TSource>(new RemotableObservable<TSource>(source, lease));
        }

        [Serializable]
        class SerializableObservable<T> : IObservable<T>
        {
            readonly RemotableObservable<T> remotableObservable;

            public SerializableObservable(RemotableObservable<T> remotableObservable)
            {
                this.remotableObservable = remotableObservable;
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                var d = new SingleAssignmentDisposable();

                var o = Observer.Create<T>(
                    observer.OnNext,
                    ex =>
                    {
                        //
                        // Make call to the remote subscription, causing lease renewal to be stopped.
                        //
                        using (d)
                        {
                            observer.OnError(ex);
                        }
                    },
                    () =>
                    {
                        //
                        // Make call to the remote subscription, causing lease renewal to be stopped.
                        //
                        using (d)
                        {
                            observer.OnCompleted();
                        }
                    }
                );

                //
                // [OK] Use of unsafe Subscribe: non-pretentious transparent wrapping through remoting; exception coming from the remote object is not re-routed.
                //
                d.Disposable = remotableObservable.Subscribe/*Unsafe*/(new RemotableObserver<T>(o));

                return d;
            }
        }

        class RemotableObserver<T> : MarshalByRefObject, IObserver<T>, ISponsor
        {
            readonly IObserver<T> underlyingObserver;

            public RemotableObserver(IObserver<T> underlyingObserver)
            {
                this.underlyingObserver = underlyingObserver;
            }

            public void OnNext(T value)
            {
                underlyingObserver.OnNext(value);
            }

            public void OnError(Exception exception)
            {
                try
                {
                    underlyingObserver.OnError(exception);
                }
                finally
                {
                    Unregister();
                }
            }

            public void OnCompleted()
            {
                try
                {
                    underlyingObserver.OnCompleted();
                }
                finally
                {
                    Unregister();
                }
            }

            [SecuritySafeCritical] // See remarks at the top of the file.
            private void Unregister()
            {
                var lease = (ILease)RemotingServices.GetLifetimeService(this);
                if (lease != null)
                    lease.Unregister(this);
            }

            [SecurityCritical]
            public override object InitializeLifetimeService()
            {
                var lease = (ILease)base.InitializeLifetimeService();
                lease.Register(this);
                return lease;
            }

            [SecurityCritical]
            TimeSpan ISponsor.Renewal(ILease lease)
            {
                return lease.InitialLeaseTime;
            }
        }

        [Serializable]
        sealed class RemotableObservable<T> : MarshalByRefObject, IObservable<T>
        {
            readonly IObservable<T> underlyingObservable;
            readonly ILease lease;

            public RemotableObservable(IObservable<T> underlyingObservable, ILease lease)
            {
                this.underlyingObservable = underlyingObservable;
                this.lease = lease;
            }

            public IDisposable Subscribe(IObserver<T> observer)
            {
                //
                // [OK] Use of unsafe Subscribe: non-pretentious transparent wrapping through remoting; throwing across remoting boundaries is fine.
                //
                return new RemotableSubscription(underlyingObservable.Subscribe/*Unsafe*/(observer));
            }

            [SecurityCritical]
            public override object InitializeLifetimeService()
            {
                return lease;
            }

            sealed class RemotableSubscription : MarshalByRefObject, IDisposable, ISponsor
            {
                private IDisposable underlyingSubscription;

                public RemotableSubscription(IDisposable underlyingSubscription)
                {
                    this.underlyingSubscription = underlyingSubscription;
                }

                public void Dispose()
                {
                    //
                    // Avoiding double-dispose and dropping the reference upon disposal.
                    //
                    using (Interlocked.Exchange(ref underlyingSubscription, Disposable.Empty))
                    {
                        Unregister();
                    }
                }

                [SecuritySafeCritical] // See remarks at the top of the file.
                private void Unregister()
                {
                    var lease = (ILease)RemotingServices.GetLifetimeService(this);
                    if (lease != null)
                        lease.Unregister(this);
                }

                [SecurityCritical]
                public override object InitializeLifetimeService()
                {
                    var lease = (ILease)base.InitializeLifetimeService();
                    lease.Register(this);
                    return lease;
                }

                [SecurityCritical]
                TimeSpan ISponsor.Renewal(ILease lease)
                {
                    return lease.InitialLeaseTime;
                }
            }
        }

        #endregion
    }
}
#endif