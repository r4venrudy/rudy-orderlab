# Search and evidence design

The implementation is an original bounded history checker using established linearizability search principles, not a claim of a new consistency definition. See [Herlihy and Wing](https://www.cs.cmu.edu/~wing/publications/HerlihyWing90.pdf), [Wing and Gong](https://www.cs.cmu.edu/~wing/publications/WingGong93.pdf), and the independent [Porcupine project](https://github.com/anishathalye/porcupine) for related work. No Porcupine source is included or required.

1. Validate the whole schema, clock and sequential clients. Validate every operation against its model before checking partitions, so an early negative partition cannot hide a malformed later operation.
2. Precompute a predecessor bitset for each operation from `other.end < operation.start`. A BigInteger membership mask supports all 256 operations without machine-word aliasing.
3. Search legal abstract executions. Each node has completed membership plus a canonical ordinal mapping from object identities to exact model-state strings. Only enabled operations with matching observed outcomes advance. State is immutable between branches.
4. Memoize fully failed `(completed, exact-state)` nodes. Equality uses the entire key, never just a digest. Caching stops after 16,777,216 UTF-16 units of retained keys; dictionary/object overhead is additional. Reaching that cache threshold disables further insertions, not search correctness.
5. Reaching every operation yields a witness. The CLI independently replays it using remaining-operation real-time comparisons, without DFS masks or memoization. Every model transition and operation membership must match.
6. If all candidates fail, emit `NotLinearizable`. If a state/time budget or cancellation stops exploration, emit `Inconclusive`. A deepest legal prefix and rejected enabled transitions explain one explored frontier; they are not a unique root-cause proof.

## Independent objects

Optional composition first checks independent objects, allocating the remaining global state/time allowance to each check. An exhaustively rejected valid partition rejects the whole history. Positive per-object witnesses are merged in a real-time-respecting topological order, then the global result is independently replayed. Any incomplete check/merge becomes inconclusive. The model and initial-state contract must be the same for every object. Transactions, arbitrary per-key map decomposition and externally coupled invariants are outside this optimization.

## Reduction

`Shrinker` starts only from a fully rejected history. It deletes one operation, rechecks the candidate and keeps the deletion only after another exhaustive rejection. It restarts after a successful deletion. A final fully conclusive pass can establish one-deletion minimality; state/time/check limits leave minimality unproved. Deleting operations may introduce or remove violations; every retained candidate is explicitly checked. The retained initial state remains unchanged. This is a useful smaller reproduction, not a globally minimum counterexample or a causal proof.

## Complexity and limits

Search is exponential in the worst case. Memoization reduces duplicate work but does not change the worst-case contract. Model states and reports are bounded by input/operation limits; cache text is capped, but the program does not promise a fixed process working set. Time is checked at search-node boundaries; one built-in transition is bounded by the model/input limits. Arbitrary compiled custom models cannot be forcibly preempted. The recording adapter serializes event timestamps under a lock and may change scheduling.

## Evidence preservation

The command bounds source length before allocation, opens it read-only, hashes the captured bytes and rechecks the same opened file after analysis. This detects persistent changes, not every adversarial change/restore or replacement of the pathname. Output is bounded UTF-8, exclusive-create, owner-only where POSIX supports it, and uses inherited permissions on Windows. Filesystem errors can leave partial new reports. No input-directed network activity, plugin loading, subprocess execution or target replay exists.

Reports map identities to numeric labels, retain value digests and omit plaintext unless explicitly requested. SHA-256 supports equality inspection, not anonymization. HTML escapes all dynamic strings, has no JavaScript or remote page assets, and includes a restrictive CSP. Timing and shape remain potentially identifying. Reduced/recorded histories retain full values by design.
