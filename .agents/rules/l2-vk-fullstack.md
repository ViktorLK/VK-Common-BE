---
trigger: always_on
---

<layer id="vk-fullstack" level="L2" name="VK Full Stack Baseline" prefix="FS" inherits="vk-arch">
<charter>
scope: Client-server contracts shared by frontend and backend.
principle: The contract lives on the wire, not in either codebase. Both ends independently fulfill the same wire specification.
</charter>

<locked_rules>
FS-01 [LOCKED] Single Source of Truth on the Wire: The wire contract is defined by the server-generated OpenAPI specification. Frontend types and validation schemas (e.g., Zod) are derived from it or reconciled against it in CI. Hand-written duplicates that can drift are prohibited.
FS-02 [LOCKED] Isomorphic Error Model: Both ends use `VKResult<T>` semantics. Wire errors are RFC 7807 ProblemDetails with a stable error code, `traceId`, and structured field-level errors. Error codes are immutable string constants: defined centrally on the backend and mirrored as `as const` mappings on the frontend, with identical values. Error categories map to HTTP status in exactly one place per side. Renaming a published error code is a breaking change.
</locked_rules>

<rules>
FS-03 DTO and Domain Separation: DTOs are wire payloads only. They are validated at the inbound boundary (the backend validation layer, Zod on the frontend) and mapped into domain or view models. Domain logic, persistence entities, and UI views never operate on wire DTOs.
FS-04 Strongly Typed Identifiers: The backend uses strongly typed ID wrappers (e.g., a record struct with a private constructor and a validating factory). The frontend uses branded types. The wire format is a GUID string, parsed and validated at the boundary. Raw strings never carry IDs across layers.
FS-05 Time: Wire timestamps are ISO 8601 with an explicit offset; persistence stores UTC. The backend reads time only through `TimeProvider`; the frontend through an injected clock abstraction, parsing server dates only through one `parseServerDate()` utility. Direct `DateTime.Now` and unvalidated `new Date(string)` are prohibited.
FS-06 Cancellation, Timeout, Idempotency: `AbortSignal` and `CancellationToken` propagate end to end. Timeouts are set explicitly at transport boundaries. Automatic retries apply only to idempotent reads, or to writes carrying a deterministic idempotency key (e.g., TurnId). Rate limiting returns HTTP 429 with `Retry-After`, which clients must honor.
FS-07 Identity and Tenant Context: Identity and tenant are resolved at the boundary and travel through standard transport channels, never as per-DTO fields. The tenant comes from verified token claims. If a header selects among several tenants, the server MUST validate it against the authenticated user's tenant memberships; a client-supplied header is never an identity source. Frontend route guards serve user experience only; backend authorization is authoritative.
FS-08 Pagination and Query Contracts: Both ends use one pagination envelope (`VKPagedResult<T>`). The backend enforces a maximum page size. Filtering and sorting use documented, structured query contracts. Unbounded list endpoints are prohibited.
FS-09 Evolution and Compatibility: Additive changes are allowed within a version. Removing, renaming, or changing the meaning of a field requires a new version. Enums travel as strings, and the frontend handles unknown values without failing.
FS-10 Realtime Envelope: WebSocket, SSE, and SignalR messages share one versioned envelope (`type`, `version`, `correlationId`, `payload`, `timestamp`) and the same error model. Transports are swappable adapters, not part of the contract.
FS-11 Observability Propagation: W3C `traceparent` propagates from client to server. ProblemDetails returns the matching `traceId`. Both ends log with a correlation ID, and personal data is scrubbed before logging.
FS-12 Zero-Trust Data: External payloads, LLM output, and user rich text are untrusted. The backend validates structured LLM output against its schema before use. The frontend sanitizes rich content (e.g., DOMPurify) before rendering. Secrets never ship in client bundles. Dynamic styles and scripts support CSP nonces.
FS-13 Serialization Conventions ◇: JSON keys are camelCase; enums are strings. An omitted property means "not provided"; an explicit null means "clear the value". Public library types on both sides carry the `VK` prefix. Pending ADR on the null-versus-omitted semantics.
</rules>

<anti_patterns>

- Backend and frontend disagreeing on an error code's string value.
- Hand-written frontend DTO types that drift from the OpenAPI definition.
- Threading TenantId or UserId through every request DTO.
- Trusting a client-supplied tenant header, or treating client-side validation or route guards as authorization.
- Auto-retrying non-idempotent POST or PATCH requests without an idempotency key.
- Rendering raw LLM output or unescaped user markup into the DOM.
- Reading the local system clock instead of the injected time source.

</anti_patterns>

<audit_checklist phase="B">

- FS-01: OpenAPI contract and frontend schemas reconciled?
- FS-02: Error codes stable and identical on both sides?
- FS-03: DTOs separated from domain and view models?
- FS-04: Entity IDs strongly typed end to end?
- FS-05: Time read only through injected sources?
- FS-06: Cancellation propagated; retries limited to idempotent calls?
- FS-07: Backend authorization authoritative; tenant taken from verified claims?
- FS-12: Untrusted data validated or sanitized before use or rendering?

</audit_checklist>

<audit_checklist phase="A">

- FS-01: Endpoint defined in OpenAPI; counterpart side checked for error model, auth context, validation, resilience, storage, realtime, and observability?
- FS-02: New error codes registered on both sides?
- FS-06: Each write operation declares its idempotency and retry strategy?
- FS-08: Pagination, query contract, and unknown-enum handling compliant?
- FS-09: Change classified as additive or breaking?

</audit_checklist>
</layer>
