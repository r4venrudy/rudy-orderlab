# Histories you can inspect

Run `python examples/make_histories.py artifacts/histories`. The new directory contains twelve invented JSON histories and an expected-verdict manifest. Release packages include this generated directory at `examples/synthetic`.

`overlap-register` permits a read before a concurrent write. `stale-register` requires the write before the read and therefore rejects the stale result. `lost-update` and `double-cas` represent impossible atomic outcomes. `fifo-inversion` violates FIFO. `shrink-me` contains one unrelated operation that can be removed without losing the rejection. Empty histories are valid for a valid model and initial state.

For a real process-local recording, use `orderlab record-demo NEW_HISTORY.json`. Four workers perform 32 actual `Interlocked.Increment` calls. Scheduling, intervals and the source digest vary by run; the operation results should still admit a replay. This exercise validates the adapter, not an external distributed service.
