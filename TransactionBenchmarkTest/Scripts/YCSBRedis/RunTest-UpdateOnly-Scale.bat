TransactionBenchmarkTest.exe -record=2000000 -workload=200000 -worker_per_redis=6 -worker=6 -pipeline=400 -type=hybrid -scale=0.6 -load=true -clear=true -run=true -dist=zipf -readperc=0 -query=2