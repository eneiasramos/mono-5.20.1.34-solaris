// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// CdsSyncEtwBCLProvider.cs
//
// <OWNER>Microsoft</OWNER>
//
// A helper class for firing ETW events related to the Coordination Data Structure 
// sync primitives. This provider is used by CDS sync primitives in both mscorlib.dll 
// and system.dll. The purpose of sharing the provider class is to be able to enable 
// ETW tracing on all CDS sync types with a single ETW provider GUID, and to minimize
// the number of providers in use.
//
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System;
using System.Collections.Generic;
using System.Text;
using System.Security;

namespace System.Threading
{
#if !FEATURE_PAL    // PAL doesn't support  eventing
    using System.Diagnostics.Tracing;

    /// <summary>Provides an event source for tracing CDS synchronization information.</summary>
    [System.Runtime.CompilerServices.FriendAccessAllowed]
    [EventSource(
        Name = "System.Threading.SynchronizationEventSource",
        Guid = "EC631D38-466B-4290-9306-834971BA0217", 
        LocalizationResources = "mscorlib")]
    internal sealed class CdsSyncEtwBCLProvider : EventSource
    {
        /// <summary>
        /// Defines the singleton instance for the CDS Sync ETW provider.
        /// The CDS Sync Event provider GUID is {EC631D38-466B-4290-9306-834971BA0217}.
        /// </summary>
        public static CdsSyncEtwBCLProvider Log = new CdsSyncEtwBCLProvider();
        /// <summary>Prevent external instantiation.  All logging should go through the Log instance.</summary>
        private CdsSyncEtwBCLProvider() { }

        /// <summary>Enabled for all keywords.</summary>
        private const EventKeywords ALL_KEYWORDS = (EventKeywords)(-1);

        //-----------------------------------------------------------------------------------
        //        
        // CDS Synchronization Event IDs (must be unique)
        //

        private const int SPINLOCK_FASTPATHFAILED_ID = 1;
        private const int SPINWAIT_NEXTSPINWILLYIELD_ID = 2;
        private const int BARRIER_PHASEFINISHED_ID = 3;

        /////////////////////////////////////////////////////////////////////////////////////
        //
        // SpinLock Events
        //

        [Event(SPINLOCK_FASTPATHFAILED_ID, Level = EventLevel.Warning)]
        public void SpinLock_FastPathFailed(int ownerID)
        {
            if (IsEnabled(EventLevel.Warning, ALL_KEYWORDS))
            {
                WriteEvent(SPINLOCK_FASTPATHFAILED_ID, ownerID);
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////
        //
        // SpinWait Events
        //

        [Event(SPINWAIT_NEXTSPINWILLYIELD_ID, Level = EventLevel.Informational)]
        public void SpinWait_NextSpinWillYield()
        {
            if (IsEnabled(EventLevel.Informational, ALL_KEYWORDS))
            {
                WriteEvent(SPINWAIT_NEXTSPINWILLYIELD_ID);
            }
        }


        //
        // Events below this point are used by the CDS types in System.dll
        //

        /////////////////////////////////////////////////////////////////////////////////////
        //
        // Barrier Events
        //

        [SecuritySafeCritical]
        [Event(BARRIER_PHASEFINISHED_ID, Level = EventLevel.Verbose, Version=1)]
        public void Barrier_PhaseFinished(bool currentSense, long phaseNum)
        {
            if (IsEnabled(EventLevel.Verbose, ALL_KEYWORDS))
            {
                // WriteEvent(BARRIER_PHASEFINISHED_ID, currentSense, phaseNum);

                // There is no explicit WriteEvent() overload matching this event's bool+long fields.
                // Therefore calling WriteEvent() would hit the "params" overload, which leads to an 
                // object allocation every time this event is fired. To prevent that problem we will 
                // call WriteEventCore(), which works with a stack based EventData array populated with 
                // the event fields.
                unsafe
                {
                    EventData* eventPayload = stackalloc EventData[2];

                    Int32 senseAsInt32 = currentSense ? 1 : 0; // write out Boolean as Int32
                    eventPayload[0].Size = sizeof(int);
                    eventPayload[0].DataPointer = ((IntPtr)(&senseAsInt32));
                    eventPayload[1].Size = sizeof(long);
                    eventPayload[1].DataPointer = ((IntPtr)(&phaseNum));

                    WriteEventCore(BARRIER_PHASEFINISHED_ID, 2, eventPayload);
                }
            }
        }

    }
#endif // !FEATURE_PAL
}
