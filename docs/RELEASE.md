# Rudy OrderLab v0.1.0-preview

Native C# complete-history linearizability investigations for registers, compare/exchange registers, counters, sets, queues, stacks and maps. Includes bounded DFS, exact failed-state memoization, optional independent-object composition, independently replayed witnesses, rejection frontiers, budgeted history reduction, digest-first JSON/text/local-HTML reports and real concurrent recording examples.

## Packages

- `orderlab-0.1.0-preview-windows-x64.zip`: self-contained Windows x64 command.
- `orderlab-0.1.0-preview-linux-x64.tar.gz`: self-contained Linux x64 command; target is Ubuntu 24.04 or a compatible .NET 10 native runtime environment.
- `orderlab-0.1.0-preview-macos-arm64.tar.gz`: self-contained Apple Silicon command built on macOS 15. It is not Apple-signed/notarized.
- `Rudy.OrderLab.Tool.0.1.0-preview.nupkg`: local-feed .NET tool, requires the .NET 10 runtime or SDK. This release does not publish to NuGet.org or GitHub Packages.
- `SHA256SUMS.txt`: hashes for the four packages; digests detect mismatches, not independent publisher identity.

Verify `sha256sum -c SHA256SUMS.txt` in the download directory, or use PowerShell `Get-FileHash -Algorithm SHA256`. Extract the native archive and run `bin/orderlab --version` (`bin\orderlab.exe` on Windows). Twelve synthetic histories, expected verdicts, example reports and a reduced history are included. Example C# project references require a source checkout; the native command and included JSON examples do not.

To install the downloaded `.nupkg`, put it in an otherwise controlled local feed folder and use the source/archive `NuGet.Config`:

```sh
dotnet tool install Rudy.OrderLab.Tool --version 0.1.0-preview --tool-path ./tools --add-source ./feed --configfile NuGet.Config
./tools/orderlab --version
```

The release workflow validates on three operating systems and tests all native installed layouts plus a fresh `.nupkg` installation before publishing. Its write permission is isolated to the final publication job. All external Actions are pinned to immutable commits. Source builds have no external package dependencies; native publication restores official runtime packs from the explicit NuGet.org endpoint.

Preview boundary: complete definitive operations and a valid globally comparable logical clock are required. No distributed clock repair, liveness guarantee, transactions across objects, handling of unknown write outcomes, network access or dynamic model loading. Search may be exponential and may return inconclusive. Passing one history never proves an implementation correct.
