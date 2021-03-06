﻿using System;
using System.IO;
using System.Threading;

namespace Serpent_Theater_Server.FTP
{
    public class RateLimitingStream : Stream
    {
        private readonly Stream _baseStream;
        private readonly System.Diagnostics.Stopwatch _watch;
        private readonly int _speedLimit;
        private long _transmitted;
        private readonly double _resolution;

        public RateLimitingStream(Stream baseStream, int speedLimit) : this(baseStream, speedLimit, 1)
        {

        }

        public RateLimitingStream(Stream baseStream, int speedLimit, double resolution)
        {
            _baseStream = baseStream;
            _watch = new System.Diagnostics.Stopwatch();
            _speedLimit = speedLimit;
            _resolution = resolution;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!_watch.IsRunning)
            {
                _watch.Start();
            }

            while (_speedLimit > 0 && _transmitted >= (_speedLimit * _resolution))
            {
                Thread.Sleep(10);

                if (!(_watch.ElapsedMilliseconds > (1000*_resolution))) continue;
                _transmitted = 0;
                _watch.Restart();
            }

            _baseStream.Write(buffer, offset, count);
            _transmitted += count;

            if (_watch.ElapsedMilliseconds > (1000 * _resolution))
            {
                _transmitted = 0;
                _watch.Restart();
            }
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
            _baseStream.Flush();
        }

        public override long Length
        {
            get { return _baseStream.Length; }
        }

        public override long Position
        {
            get
            {
                return _baseStream.Position;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            _watch.Stop();
            base.Dispose(disposing);
        }
    }
}
