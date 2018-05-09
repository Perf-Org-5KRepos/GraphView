﻿namespace TransactionBenchmarkTest.YCSB
{
    using GraphView.Transaction;
    using ServiceStack.Redis;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Linq;

    class TxWorkloadWithTx
    {
        internal string TableId;
        internal string Key;
        internal string Value;
        internal string Type;
        internal Transaction tx;

        public TxWorkloadWithTx(string type, string tableId, string key, string value, Transaction tx)
        {
            this.TableId = tableId;
            this.Key = key;
            this.Value = value;
            this.Type = type;
            this.tx = tx;
        }

        public override string ToString()
        {
            return string.Format("key={0},value={1},type={2},tableId={3}", this.Key, this.Value, this.Type, this.TableId);
        }
    }

    class YCSBBenchmarkTest
    {
        public static readonly String TABLE_ID = "ycsb_table";

        public static readonly long REDIS_DB_INDEX = 7L;

        public static Func<object, object> ACTION = (object obj) =>
        {
            // parse those parameters
            Tuple<TxWorkload, VersionDb> tuple = obj as Tuple<TxWorkload, VersionDb>;
            TxWorkload workload = tuple.Item1;
            VersionDb versionDb = tuple.Item2;
            Transaction tx = new Transaction(null, versionDb);

            string readValue = null;
			switch (workload.Type)
			{
				case "READ":
					readValue = (string)tx.Read(workload.TableId, workload.Key);
					break;

				case "UPDATE":
					readValue = (string)tx.Read(workload.TableId, workload.Key);
					if (readValue != null)
					{
						tx.Update(workload.TableId, workload.Key, workload.Value);
					}
					break;

				case "DELETE":
					readValue = (string)tx.Read(workload.TableId, workload.Key);
					if (readValue != null)
					{
						tx.Delete(workload.TableId, workload.Key);
					}
					break;

				case "INSERT":
					readValue = (string)tx.ReadAndInitialize(workload.TableId, workload.Key);
					if (readValue == null)
					{
						tx.Insert(workload.TableId, workload.Key, workload.Value);
					}
					break;

				default:
					break;
			}
			// try to commit here
			if (tx.Commit())
			{
				return true;
			}
			return false;
		};

        /// <summary>
        /// The number of workers
        /// </summary>
        private int workerCount;

        /// <summary>
        /// The number of tasks per worker
        /// </summary>
        private int taskCountPerWorker;

        /// <summary>
        /// A list of workers
        /// </summary>
        private List<Worker> workers;

        /// <summary>
        /// The exact ticks when the test starts
        /// </summary>
        private long testBeginTicks;

        /// <summary>
        /// The exact ticks when then test ends
        /// </summary>
        private long testEndTicks;

        /// <summary>
        /// total redis commands processed
        /// </summary>
        private long commandCount = 0;

        /// <summary>
        /// The version db instance
        /// </summary>
        private VersionDb versionDb;

        internal int TxThroughput
        {
            get
            {
                double runSeconds = this.RunSeconds;
                int taskCount = this.workerCount * this.taskCountPerWorker;
                return (int)(taskCount / runSeconds);
            }
        }

        //internal double AbortRate
        //{
        //    get
        //    {
        //        return 1 - (COMMITED_TXS * 1.0 / FINISHED_TXS);
        //    }
        //}

        internal double RunSeconds
        {
            get
            {
                return ((this.testEndTicks - this.testBeginTicks) * 1.0) / 10000000;
            }
        }

        internal double RedisThroughput
        {
            get
            {
                double runSeconds = this.RunSeconds;
                return (int)(this.commandCount / runSeconds);
            }
        }

        public YCSBBenchmarkTest(int workerCount, int taskCountPerWorker, VersionDb versionDb = null)
        {
            this.workerCount = workerCount;
            this.taskCountPerWorker = taskCountPerWorker;
            
            if (versionDb != null)
            {
                this.versionDb = versionDb;
            }

            this.workers = new List<Worker>();
            for (int i = 0; i < this.workerCount; i++)
            {
                this.workers.Add(new Worker(i+1, taskCountPerWorker));
            }
        }

        internal void Setup(string dataFile, string operationFile)
        {
            // step1: clear the database
            this.versionDb.Clear();
            Console.WriteLine("Cleared the database");

            // step2: create version table
            versionDb.CreateVersionTable(TABLE_ID, REDIS_DB_INDEX);
            Console.WriteLine("Created version table {0}", TABLE_ID);

            // step3: load data
            using (StreamReader reader = new StreamReader(dataFile))
            {
                string line;
                int count = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] fields = this.ParseCommandFormat(line);
                    TxWorkload workload = new TxWorkload(fields[0], TABLE_ID, fields[2], fields[3]);
                    count++;

                    ACTION(Tuple.Create(workload, this.versionDb));
                    if (count % 5000 == 0)
                    {
                        Console.WriteLine("Loaded {0} records", count);
                    }
                }
                Console.WriteLine("Load records successfully, {0} records in total", count);
            }

            // step 4: fill workers' queue
            using (StreamReader reader = new StreamReader(operationFile))
            {
                string line;
                foreach (Worker worker in this.workers)
                {
                    for (int i = 0; i < this.taskCountPerWorker; i++)
                    {
                        line = reader.ReadLine();
                        string[] fields = this.ParseCommandFormat(line);
                        TxWorkload workload = new TxWorkload(fields[0], TABLE_ID, fields[2], fields[3]);
                        worker.EnqueueTxTask(new TxTask(ACTION, Tuple.Create(workload, this.versionDb)));
                    }
                }
            }
        }

        internal void Run()
        {
            Console.WriteLine("Try to run {0} tasks in {1} workers", (this.workerCount * this.taskCountPerWorker), this.workerCount);
            Console.WriteLine("Running......");

            // ONLY FOR REDIS VERSION DB
            long commandCountBeforeRun = long.MaxValue;
            if (this.versionDb is RedisVersionDb)
            {
                commandCountBeforeRun = this.GetCurrentCommandCount();
            }

            this.testBeginTicks = DateTime.Now.Ticks;
            List<Thread> threadList = new List<Thread>();

            foreach (Worker worker in this.workers)
            {
                Thread thread = new Thread(new ThreadStart(worker.Run));
                threadList.Add(thread);
                thread.Start();
            }

            foreach (Thread thread in threadList)
            {
                thread.Join();
            }
            this.testEndTicks = DateTime.Now.Ticks;

            if (this.versionDb is RedisVersionDb)
            {
                long commandCountAfterRun = this.GetCurrentCommandCount();
                this.commandCount = commandCountAfterRun - commandCountBeforeRun;
            }

            Console.WriteLine("Finished all tasks");
        }

        internal void Stats()
        {
            int taskCount = this.workerCount * this.taskCountPerWorker;
            Console.WriteLine("\nFinshed {0} requests in {1} seconds", taskCount, this.RunSeconds);
            Console.WriteLine("Transaction Throughput: {0} tx/second", this.TxThroughput);

            int totalTxs = 0, abortedTxs = 0;
            foreach (Worker worker in this.workers)
            {
                totalTxs += worker.FinishedTxs;
                abortedTxs += worker.AbortedTxs;
            }
            Console.WriteLine("\nFinshed {0} txs, Aborted {1} txs", totalTxs, abortedTxs);
            Console.WriteLine("Transaction AbortRate: {0}%", (abortedTxs*1.0/totalTxs) * 100);

            if (this.versionDb is RedisVersionDb)
            {
                Console.WriteLine("\nFinshed {0} commands in {1} seconds", this.commandCount, this.RunSeconds);
                Console.WriteLine("Redis Throughput: {0} cmd/second", this.RedisThroughput);
            }
            
            Console.WriteLine();
        }

        private string[] ParseCommandFormat(string line)
        {
            string[] fields = line.Split(' ');
            string value = null;
            int fieldsOffset = fields[0].Length + fields[1].Length + fields[2].Length + 3 + 9;
            int fieldsEnd = line.Length - 2;

            if (fieldsOffset < fieldsEnd)
            {
                value = line.Substring(fieldsOffset, fieldsEnd-fieldsOffset+1);
            }

            return new string[] {
                fields[0], fields[1], fields[2], value
            };
        }

        /// <summary>
        /// ONLY FOR REDIS VERSION DB
        /// 
        /// Here there is a bug in ServiceStack.Redis, it will not refresh the info aftering reget a client from
        /// the pool, which means the number of commands will be not changed.
        /// 
        /// The bug has been fixed in Version 4.0.58 (Commerical Version), our reference version is Version 3.9 (The last open source version).
        /// 
        /// So here we have to dispose the redis client manager and reconnect with redis to get the lastest commands.
        /// </summary>
        /// <returns></returns>
        private long GetCurrentCommandCount()
        {
            RedisVersionDb redisVersionDb = this.versionDb as RedisVersionDb;
            if (redisVersionDb == null)
            {
                return 0;
            }

            RedisClientManager clientManager = redisVersionDb.RedisManager;
            clientManager.Dispose();
            long commandCount = 0;
            for (int i = 0; i < clientManager.RedisInstanceCount; i++)
            {
                using (RedisClient redisClient = clientManager.GetLastestClient(0, 0))
                {
                    string countStr = redisClient.Info["total_commands_processed"];
                    long count = Convert.ToInt64(countStr);
                    commandCount += count;
                }
            }
            return commandCount;
        }
    }
}
