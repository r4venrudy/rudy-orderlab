# Complete history schema v1

UTF-8 JSON, one root object with **exactly** `schema`, `model`, `initial`, `operations`. Duplicate properties, unknown fields, comments, trailing commas, unknown schemas, non-integer ticks and malformed Unicode are rejected. Integers are JSON numbers; all abstract data values, integer results and boolean results are JSON **strings**. `null` is a distinguished acknowledgement/absence, never the string `"null"`.

```json
{
  "schema": 1,
  "model": "register",
  "initial": ["0"],
  "operations": [
    {"id":"write","client":"worker-a","object":"main","start":0,"end":3,"kind":"write","args":["1"],"result":null},
    {"id":"read","client":"worker-b","object":"main","start":1,"end":2,"kind":"read","args":[],"result":"0"}
  ]
}
```

Each operation has exactly the eight illustrated properties. IDs are unique, nonempty ordinal Unicode strings; clients and objects are nonempty. All strings are at most 1,024 UTF-16 code units, without control characters or unpaired surrogates. Values may be empty strings. No culture-dependent comparisons or normalization are performed.

`start` and `end` are nonnegative signed 64-bit logical ticks, globally unique across all invocation and response events; `start < end`. They are globally comparable event positions from a trusted recorder, not arbitrary wall clocks. The array need not be chronological. When A ends before B begins, A must precede B in a witness. Overlap alone adds no order. Calls with the same sequential client cannot overlap, including across objects.

All calls must have definitive modeled outcomes. Pending calls, timeouts with unknown effects, failed transport responses, partial recordings and indeterminate writes are unsupported. Silently dropping such calls can change whether a history is linearizable. This preview rejects incomplete contracts rather than inventing completions.

Each object independently starts in the same `initial` state. A whole map is one object; there is no multi-object transaction specification or arbitrary per-key partitioning.

| Model | Initial | Operation contract |
| --- | --- | --- |
| `register` | Exactly one value | `write`: one argument, null result; `read`: no arguments, nonnull value result |
| `cas-register` | Exactly one value | Register operations; `cas`: expected/new arguments, result `"true"` iff expected matches prior state, replace only on success |
| `counter` | One canonical signed 64-bit integer string | `read`: no args, current value; `add`: one signed delta, result is the new checked value; overflow has no valid transition |
| `set` | Distinct elements | `add`: one element, true iff absent and inserted; `remove`: true iff present and removed; `contains`: true iff present; `size`: no args, nonnegative integer result |
| `queue` | Ordered front to back | `enqueue`: one value, null acknowledgement; `dequeue`/`peek`: no args, front value or null if empty; only dequeue removes |
| `stack` | Ordered bottom to top | `push`: one value, null acknowledgement; `pop`/`peek`: no args, top value or null if empty; only pop removes |
| `map` | Alternating key/value strings, distinct keys | `put`: key/value, null acknowledgement; `get`: key, value or null if absent; `delete`: key, true iff present and removed; `size`: no args, nonnegative integer result |

Canonical integer strings use `0`, negative nonzero decimals or positive decimals without leading zero or `+`. Boolean strings are exactly lowercase `"true"` / `"false"`. Register/CAS compare values ordinally. Counter overflow is a contradicted transition; syntactically out-of-range integer input is invalid. An empty history is linearizable only if its model and initial state are valid.

Boundaries: 8 MiB encoded file, 256 operations, 256 initial strings, four arguments per operation. Model-specific arities further constrain those limits. Current complete-history restrictions are stricter than the full mathematical treatment of histories with pending calls.
