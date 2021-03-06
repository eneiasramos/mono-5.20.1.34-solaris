// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Buffers;
using System.Threading.Tasks;
using Xunit;

namespace System.IO.Pipelines.Tests
{
    public class FlushAsyncCompletionTests : PipeTest
    {
        [Fact]
        public void AwaitingFlushAsyncAwaitableTwiceCompletesReaderWithException()
        {
            async Task Await(ValueTask<FlushResult> a)
            {
                await a;
            }

            PipeWriter writeBuffer = Pipe.Writer;
            writeBuffer.Write(new byte[MaximumSizeHigh]);
            ValueTask<FlushResult> awaitable = writeBuffer.FlushAsync();

            Task task1 = Await(awaitable);
            Task task2 = Await(awaitable);

            Assert.Equal(true, task1.IsCompleted);
            Assert.Equal(true, task1.IsFaulted);
            Assert.Equal("Concurrent reads or writes are not supported.", task1.Exception.InnerExceptions[0].Message);

            Assert.Equal(true, task2.IsCompleted);
            Assert.Equal(true, task2.IsFaulted);
            Assert.Equal("Concurrent reads or writes are not supported.", task2.Exception.InnerExceptions[0].Message);
        }
    }
}
