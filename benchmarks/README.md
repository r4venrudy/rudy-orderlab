# Measure the checker

`dotnet run --project benchmarks/OrderLab.Benchmarks -c Release` prints versioned JSON for three fully checked synthetic rejection histories after one warm-up each. The fields include actual elapsed milliseconds, allocated managed bytes, visited states, transitions and exact-state cache hits. The twelve-write case has many equivalent prefixes; it exercises failed-state memoization rather than an early successful branch.

This is a reproducible workload, not a claim of competitive throughput. Machine, runtime, process load, instrumentation and input shape change the measurements. The checker is exponential in the worst case. State/time budgets and an `Inconclusive` verdict are part of its contract; partitioning is valid only for independent objects, never arbitrary map keys or multi-object transactions.
