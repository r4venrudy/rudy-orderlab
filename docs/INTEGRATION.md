# Record your own component

Buildable example:

```sh
mkdir -p artifacts
dotnet run --project examples/OrderLab.Sample -c Release -- artifacts/new-queue-history.json
dotnet run --project src/OrderLab.Cli -c Release -- check artifacts/new-queue-history.json --format html --output artifacts/queue-report.html
```

On PowerShell create the folder with `New-Item -ItemType Directory artifacts`. The example wraps an actual `ConcurrentQueue<string>` across four workers, records enqueue/dequeue invocation and response boundaries, and independently replays the witness. The same calls work around your own concurrent object:

```csharp
var recording = new HistoryRecorder("register", "initial-value");
var invocation = recording.Begin("sequential-worker-1", "cache-key-1", "write", "new-value");
// Call your implementation here, then capture its actual outcome.
recording.Complete(invocation, null); // null is this model's write acknowledgement
var history = recording.Snapshot();
var result = Checker.Check(history, new RegisterModel(), new Limits(100_000, 10_000));
var verified = result.Verdict == Verdict.Linearizable &&
    WitnessVerifier.Verify(history, new RegisterModel(), result.Witness);
```

Use **one recorder** per history. Every `Begin` and `Complete` obtains a distinct position on a process-local logical clock. Instrumentation can perturb scheduling; this adapter is not a distributed clock-synchronization system. An exception/timeout with an unknown effect must remain pending; `Snapshot` rejects pending operations. Never turn unknown outcomes into acknowledged successes, silently discard pending calls, or combine wall clocks from different machines.

`client` means a sequential worker; async tasks that overlap need different IDs. `object` means a whole independently modeled object. Each object starts from the same declared `initial` state. Map `size` couples keys: keep the entire map in one object. Transactions that span objects are outside this version's composition contract.

The library has no package dependencies. Reference `src/OrderLab.Core/OrderLab.Core.csproj`. `IModel` allows caller-supplied deterministic sequential specifications in trusted compiled code; `Initial`, `Validate` and `Apply` must be deterministic, bounded and side-effect free. The CLI never loads code from a history. Time budgets cannot preempt a custom model that blocks inside `Apply`; built-in models have bounded transitions. Arrays in input `History` and `Operation` are caller-owned: keep them immutable throughout checking. Recorder snapshots make defensive copies.
