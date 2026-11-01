# Contributing

Reproduce a concrete contract failure with an invented history. A result disagreement must identify the sequential specification, initial state, complete outcomes and a valid globally ordered invocation/response clock. A stale physical timestamp or omitted pending operation is an adapter issue before it is a checker issue.

Run `dotnet run --project tests/OrderLab.Tests -c Release`, build `src/OrderLab.Cli`, then run `python tests/integration.py PATH_TO_ORDERLAB .`. Run the ConcurrentQueue sample when changing recording. The native tests require no external test framework or package feed. Keep generated histories/reports under `artifacts/`.

For a model change, add a focused semantic regression and update the independent permutation oracle as a separately reasoned implementation. Preserve strict malformed-input behavior, fail-closed budgets, witness replay, exact cache equality and deterministic model state. Add explicit schema versions before incompatible format changes. Do not treat benchmark timing as a portable speed guarantee.

Submit cohesive changes with their reproduction and validation. Changes to `IModel` must remain bounded, deterministic and free of side effects. Do not add credentials, real user histories, dynamic code loading or network replay to examples. Follow the existing RUDY banner and report style, with accurate feature descriptions.
