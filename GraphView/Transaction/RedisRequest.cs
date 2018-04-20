﻿
using System;

namespace GraphView.Transaction
{
    internal enum RedisRequestType
    {
        HGet,
        HMGet,
        HGetAll,
        HSetNX,
        HSet,
        HMSet,
        HDel,
        EvalSha,
    }

    internal class RedisRequest
    {
        internal object Result { get; private set; }

        internal string HashId { get; private set; }
        internal byte[] Key { get; private set; }
        internal byte[] Value { get; private set; }
        internal byte[][] Keys { get; private set; }
        internal byte[][] Values { get; private set; }
        internal string Sha1 { get; private set; }
        internal int NumberKeysInArgs { get; private set; }
        internal bool Finished { get; set; } = false;
        internal RedisRequestType Type { get; private set; }
        
        /// <summary>
        /// for HSet, HSetNX command
        /// </summary>
        public RedisRequest(string hashId, byte[] key, byte[] value, RedisRequestType type)
        {
            this.HashId = hashId;
            this.Key = key;
            this.Value = value;
            this.Type = type;
        }

        /// <summary>
        /// for HGet, HDel command
        /// </summary>
        public RedisRequest(string hashId, byte[] key, RedisRequestType type)
        {
            this.HashId = hashId;
            this.Key = key;
            this.Type = type;
        }

        /// <summary>
        /// for HMGet command
        /// </summary>
        public RedisRequest(string hashId, byte[][] keys, RedisRequestType type)
        {
            this.HashId = hashId;
            this.Keys = keys;
            this.Type = type;
        }

        /// <summary>
        /// For HMSet command
        /// </summary>
        public RedisRequest(string hashId, byte[][] keys, byte[][] values, RedisRequestType type)
        {
            this.HashId = hashId;
            this.Keys = keys;
            this.Values = values;
            this.Type = type;
        }

        /// <summary>
        /// for EvalSha command
        /// </summary>
        public RedisRequest(byte[][] keys, string sha1, int numberOfKeysInArg, RedisRequestType type)
        {
            this.Keys = keys;
            this.Sha1 = sha1;
            this.NumberKeysInArgs = numberOfKeysInArg;
            this.Type = type;
        } 

        /// <summary>
        /// for HGetAll command
        /// </summary>
        public RedisRequest(string hashId, RedisRequestType type)
        {
            this.HashId = hashId;
            this.Type = type;
        }

        internal void SetValue(byte[] result)
        {
            this.Result = result;
            this.Finished = true;
        }

        internal void SetLong(long result)
        {
            this.Result = result;
            this.Finished = true;
        }

        internal void SetValues(byte[][] result)
        {
            this.Result = result;
            this.Finished = true;
        }

        internal void SetVoid()
        {
            this.Finished = true;
        }

        internal void SetError(Exception e)
        {
            this.Finished = true;
        }
    }
}