// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace System.Data.SqlClient
{
    internal class TdsParserStateObjectNative : TdsParserStateObject
    {
        private SNIHandle _sessionHandle = null;              // the SNI handle we're to work on

        private SNIPacket _sniPacket = null;                // Will have to re-vamp this for MARS
        internal SNIPacket _sniAsyncAttnPacket = null;                // Packet to use to send Attn
        private readonly WritePacketCache _writePacketCache = new WritePacketCache(); // Store write packets that are ready to be re-used

        public TdsParserStateObjectNative(TdsParser parser) : base(parser) { }

        private GCHandle _gcHandle;                                    // keeps this object alive until we're closed.

        private Dictionary<IntPtr, SNIPacket> _pendingWritePackets = new Dictionary<IntPtr, SNIPacket>(); // Stores write packets that have been sent to SNI, but have not yet finished writing (i.e. we are waiting for SNI's callback)

        internal TdsParserStateObjectNative(TdsParser parser, TdsParserStateObject physicalConnection, bool async) :
            base(parser, physicalConnection, async)
        {
        }

        internal SNIHandle Handle => _sessionHandle;

        internal override UInt32 Status => _sessionHandle != null ? _sessionHandle.Status : TdsEnums.SNI_UNINITIALIZED;

        internal override object SessionHandle => _sessionHandle;

        protected override object EmptyReadPacket => IntPtr.Zero;

        protected override void CreateSessionHandle(TdsParserStateObject physicalConnection, bool async)
        {
            Debug.Assert(physicalConnection is TdsParserStateObjectNative, "Expected a stateObject of type " + this.GetType());
            TdsParserStateObjectNative nativeSNIObject = physicalConnection as TdsParserStateObjectNative;
            SNINativeMethodWrapper.ConsumerInfo myInfo = CreateConsumerInfo(async);
            _sessionHandle = new SNIHandle(myInfo, nativeSNIObject.Handle);
        }

        private SNINativeMethodWrapper.ConsumerInfo CreateConsumerInfo(bool async)
        {
            SNINativeMethodWrapper.ConsumerInfo myInfo = new SNINativeMethodWrapper.ConsumerInfo();

            Debug.Assert(_outBuff.Length == _inBuff.Length, "Unexpected unequal buffers.");

            myInfo.defaultBufferSize = _outBuff.Length; // Obtain packet size from outBuff size.

            if (async)
            {
                myInfo.readDelegate = SNILoadHandle.SingletonInstance.ReadAsyncCallbackDispatcher;
                myInfo.writeDelegate = SNILoadHandle.SingletonInstance.WriteAsyncCallbackDispatcher;
                _gcHandle = GCHandle.Alloc(this, GCHandleType.Normal);
                myInfo.key = (IntPtr)_gcHandle;
            }
            return myInfo;
        }

        internal override void CreatePhysicalSNIHandle(string serverName, bool ignoreSniOpenTimeout, long timerExpire, out byte[] instanceName, ref byte[] spnBuffer, bool flushCache, bool async, bool fParallel, bool isIntegratedSecurity)
        {
            // We assume that the loadSSPILibrary has been called already. now allocate proper length of buffer
            spnBuffer = null;
            if (isIntegratedSecurity)
            {
                // now allocate proper length of buffer
                spnBuffer = new byte[SNINativeMethodWrapper.SniMaxComposedSpnLength];
            }

            SNINativeMethodWrapper.ConsumerInfo myInfo = CreateConsumerInfo(async);

            // Translate to SNI timeout values (Int32 milliseconds)
            long timeout;
            if (Int64.MaxValue == timerExpire)
            {
                timeout = Int32.MaxValue;
            }
            else
            {
                timeout = ADP.TimerRemainingMilliseconds(timerExpire);
                if (timeout > Int32.MaxValue)
                {
                    timeout = Int32.MaxValue;
                }
                else if (0 > timeout)
                {
                    timeout = 0;
                }
            }

            _sessionHandle = new SNIHandle(myInfo, serverName, spnBuffer, ignoreSniOpenTimeout, checked((int)timeout), out instanceName, flushCache, !async, fParallel);
        }

        protected override uint SNIPacketGetData(object packet, byte[] _inBuff, ref uint dataSize) => SNINativeMethodWrapper.SNIPacketGetData((IntPtr)packet, _inBuff, ref dataSize);

        protected override bool CheckPacket(object packet, TaskCompletionSource<object> source)
        {
            IntPtr ptr = (IntPtr)(object)packet;
            return IntPtr.Zero == ptr || IntPtr.Zero != ptr && source != null;
        }

        public void ReadAsyncCallback(IntPtr key, IntPtr packet, UInt32 error) => ReadAsyncCallback(key, packet, error);

        public void WriteAsyncCallback(IntPtr key, IntPtr packet, UInt32 sniError) => WriteAsyncCallback(key, packet, sniError);

        protected override void RemovePacketFromPendingList(object ptr)
        {
            IntPtr pointer = (IntPtr)ptr;

            SNIPacket recoveredPacket;

            lock (_writePacketLockObject)
            {
                if (_pendingWritePackets.TryGetValue(pointer, out recoveredPacket))
                {
                    _pendingWritePackets.Remove(pointer);
                    _writePacketCache.Add(recoveredPacket);
                }
#if DEBUG
                else
                {
                    Debug.Assert(false, "Removing a packet from the pending list that was never added to it");
                }
#endif
            }
        }


        internal override void Dispose()
        {
            SafeHandle packetHandle = _sniPacket;
            SafeHandle sessionHandle = _sessionHandle;
            SafeHandle asyncAttnPacket = _sniAsyncAttnPacket;

            _sniPacket = null;
            _sessionHandle = null;
            _sniAsyncAttnPacket = null;

            DisposeCounters();

            if (null != sessionHandle || null != packetHandle)
            {
                packetHandle?.Dispose();
                asyncAttnPacket?.Dispose();
                    
                if (sessionHandle != null)
                {
                    sessionHandle.Dispose();
                    DecrementPendingCallbacks(true); // Will dispose of GC handle.
                }
            }

            DisposePacketCache();
        }

        protected override void FreeGcHandle(int remaining, bool release)
        {
            if ((0 == remaining || release) && _gcHandle.IsAllocated)
            {
                _gcHandle.Free();
            }
        }

        internal override bool IsFailedHandle() => _sessionHandle.Status != TdsEnums.SNI_SUCCESS;

        internal override object ReadSyncOverAsync(int timeoutRemaining, out uint error)
        {
            SNIHandle handle = Handle;
            if (handle == null)
            {
                throw ADP.ClosedConnectionError();
            }
            IntPtr readPacketPtr = IntPtr.Zero;
            error = SNINativeMethodWrapper.SNIReadSyncOverAsync(handle, ref readPacketPtr, GetTimeoutRemaining());
            return readPacketPtr;
        }

        internal override bool IsPacketEmpty(object readPacket) => IntPtr.Zero == (IntPtr)readPacket;

        internal override void ReleasePacket(object syncReadPacket) => SNINativeMethodWrapper.SNIPacketRelease((IntPtr)syncReadPacket);

        internal override uint CheckConnection()
        {
            SNIHandle handle = Handle;
            return handle == null ? TdsEnums.SNI_SUCCESS : SNINativeMethodWrapper.SNICheckConnection(handle);
        }

        internal override object ReadAsync(out uint error, ref object handle)
        {
            IntPtr readPacketPtr = IntPtr.Zero;
            error = SNINativeMethodWrapper.SNIReadAsync((SNIHandle)handle, ref readPacketPtr);
            return readPacketPtr;
        }

        internal override object CreateAndSetAttentionPacket()
        {
            SNIHandle handle = Handle;
            SNIPacket attnPacket = new SNIPacket(handle);
            _sniAsyncAttnPacket = attnPacket;
            SetPacketData(attnPacket, SQL.AttentionHeader, TdsEnums.HEADER_LEN);
            return attnPacket;
        }

        internal override uint WritePacket(object packet, bool sync) => SNINativeMethodWrapper.SNIWritePacket(Handle, (SNIPacket)packet, sync);

        internal override object AddPacketToPendingList(object packetToAdd)
        {
            SNIPacket packet = (SNIPacket)packetToAdd;
            Debug.Assert(packet == _sniPacket, "Adding a packet other than the current packet to the pending list");
            _sniPacket = null;
            IntPtr pointer = packet.DangerousGetHandle();

            lock (_writePacketLockObject)
            {
                _pendingWritePackets.Add(pointer, packet);
            }

            return pointer;
        }

        internal override bool IsValidPacket(object packetPointer) => (IntPtr)packetPointer != IntPtr.Zero;

        internal override object GetResetWritePacket()
        {
            if (_sniPacket != null)
            {
                SNINativeMethodWrapper.SNIPacketReset(Handle, SNINativeMethodWrapper.IOType.WRITE, _sniPacket, SNINativeMethodWrapper.ConsumerNumber.SNI_Consumer_SNI);
            }
            else
            {
                lock (_writePacketLockObject)
                {
                    _sniPacket = _writePacketCache.Take(Handle);
                }
            }
            return _sniPacket;
        }

        internal override void ClearAllWritePackets()
        {
            if (_sniPacket != null)
            {
                _sniPacket.Dispose();
                _sniPacket = null;
            }
            lock (_writePacketLockObject)
            {
                Debug.Assert(_pendingWritePackets.Count == 0 && _asyncWriteCount == 0, "Should not clear all write packets if there are packets pending");
                _writePacketCache.Clear();
            }
        }

        internal override void SetPacketData(object packet, byte[] buffer, int bytesUsed) 
            => SNINativeMethodWrapper.SNIPacketSetData((SNIPacket)packet, buffer, bytesUsed);

        internal override uint SniGetConnectionId(ref Guid clientConnectionId) 
            => SNINativeMethodWrapper.SniGetConnectionId(Handle, ref clientConnectionId);

        internal override uint DisabeSsl() 
            => SNINativeMethodWrapper.SNIRemoveProvider(Handle, SNINativeMethodWrapper.ProviderEnum.SSL_PROV);

        internal override uint EnableMars(ref uint info) 
            => SNINativeMethodWrapper.SNIAddProvider(Handle, SNINativeMethodWrapper.ProviderEnum.SMUX_PROV, ref info);

        internal override uint EnableSsl(ref uint info)
        {
            // Add SSL (Encryption) SNI provider.
            return SNINativeMethodWrapper.SNIAddProvider(Handle, SNINativeMethodWrapper.ProviderEnum.SSL_PROV, ref info);
        }

        internal override uint SetConnectionBufferSize(ref uint unsignedPacketSize) 
            => SNINativeMethodWrapper.SNISetInfo(Handle, SNINativeMethodWrapper.QTypes.SNI_QUERY_CONN_BUFSIZE, ref unsignedPacketSize);

        internal override uint GenerateSspiClientContext(byte[] receivedBuff, uint receivedLength, ref byte[] sendBuff, ref uint sendLength, byte[] _sniSpnBuffer) 
            => SNINativeMethodWrapper.SNISecGenClientContext(Handle, receivedBuff, receivedLength, sendBuff, ref sendLength, _sniSpnBuffer);
        

        internal override uint WaitForSSLHandShakeToComplete() 
            => SNINativeMethodWrapper.SNIWaitForSSLHandshakeToComplete(Handle, GetTimeoutRemaining());

        internal override void DisposePacketCache()
        {
            lock (_writePacketLockObject)
            {
                _writePacketCache.Dispose();
                // Do not set _writePacketCache to null, just in case a WriteAsyncCallback completes after this point
            }
        }
        
        internal sealed class WritePacketCache : IDisposable
        {
            private bool _disposed;
            private Stack<SNIPacket> _packets;

            public WritePacketCache()
            {
                _disposed = false;
                _packets = new Stack<SNIPacket>();
            }

            public SNIPacket Take(SNIHandle sniHandle)
            {
                SNIPacket packet;
                if (_packets.Count > 0)
                {
                    // Success - reset the packet
                    packet = _packets.Pop();
                    SNINativeMethodWrapper.SNIPacketReset(sniHandle, SNINativeMethodWrapper.IOType.WRITE, packet, SNINativeMethodWrapper.ConsumerNumber.SNI_Consumer_SNI);
                }
                else
                {
                    // Failed to take a packet - create a new one
                    packet = new SNIPacket(sniHandle);
                }
                return packet;
            }

            public void Add(SNIPacket packet)
            {
                if (!_disposed)
                {
                    _packets.Push(packet);
                }
                else
                {
                    // If we're disposed, then get rid of any packets added to us
                    packet.Dispose();
                }
            }

            public void Clear()
            {
                while (_packets.Count > 0)
                {
                    _packets.Pop().Dispose();
                }
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _disposed = true;
                    Clear();
                }
            }
        }
    }
}
