# Security and evidence handling

This preview processes local history files. It has no network client, remote model loading, target access, command execution from input, or telemetry. Source builds and tests use platform libraries with external package feeds cleared. Native release publishing downloads runtime packs from the explicitly selected official NuGet feed; native packages include that runtime and license notices.

Use stable authorized copies. Reports contain input/value digests, intervals, operation shape and numeric labels. Low-entropy values may be guessed from hashes; this is not anonymization. `--include-values` exposes plaintext in JSON/HTML. Source recordings and reduced histories retain full original values. Do not place secrets in public bug reports or shared timelines.

Schema, string, operation, state, cache-text, time and report limits reduce accidental resource exhaustion. Parsing and model validation must finish before an analyzed verdict is emitted. Unknown or partial histories are rejected, and budget exhaustion is inconclusive. The exponential search is not a general untrusted multi-tenant computation service. A custom compiled model can block; the library cannot preempt arbitrary caller code.

Output uses exclusive creation. POSIX requests owner-only access; Windows inherits directory permissions. An I/O failure can leave a partial new file. Re-reading the same opened source detects persistent content changes, not hostile ABA changes or every path replacement.

For a potentially sensitive vulnerability, contact the repository owner privately before publishing full evidence. Provide the source version, minimal synthetic history, expected contract, observed result and bounded reproduction. Do not include production credentials or another party's data. This preview has no security certification or production support guarantee.
