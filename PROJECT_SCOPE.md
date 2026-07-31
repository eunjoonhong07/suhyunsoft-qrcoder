# QRCoder Project — Scope Document

_Status: draft for review • Owner: Eunjoon Hong • Repo: `eunjoonhong07/QR-Code-Test` (to be made **private**)_

## 1. Objective

Optimize the QRCoder library, produce supporting documentation (function map, sequence
diagram, API user guide), and deliver a reusable `QRCoder.dll` plus a sample application to a
private GitHub repository, with daily commits tracking all changes.

## 2. Deliverables

| # | Deliverable | Format | Notes |
|---|-------------|--------|-------|
| 1 | Optimized QRCoder source code | source tree | "unused code/components" removed — scope TBD (see §4.1) |
| 2 | `QRCoder.dll` | compiled binary | single-target `net8.0` build (already produced) |
| 3 | QRCoder Sample Application | EXE project (`QRCoderSamples`) | demonstrates the DLL |
| 4 | QRCoder Function Tree Map | doc (paper-based reference) | organized function/call hierarchy |
| 5 | `QRCoder_SequenceDiagram.docx` | Word document | sequence diagram of a QR-generation flow |
| 6 | `QRCoder_API_User_Guide.docx` | Word document | based on a provided template (see §5) |
| 7 | Daily commit history | git log on private repo | record of all changes |

## 3. Current baseline (already done)

- Solution reduced to **two projects**: `QRCoder` (net8.0 class library → `QRCoder.dll`) and
  `QRCoderSamples` (net10.0-windows console EXE).
- Source tree reorganized (`Rendering/`, `QRCodeGenerator/`, `PayloadGenerator/`, ...).
- README updated to reflect the fork.
- Local commits exist; a `git pull --rebase` + `push` is still pending to sync with `origin`.

## 4. Work breakdown & scope per deliverable

### 4.1 Optimize source code (remove unused code/components)
- **Decision required — interpretation of "unused":**
  - **(A) Trim to what the sample uses** (aggressive): keep only the OTP payload, the renderers
    actually used (PNG/SVG/BMP/PDF/JPEG-via-QRCode), and their dependencies; delete the other
    ~24 payload generators (WiFi, vCard, SwissQrCode, ...) and unused renderers
    (ASCII, Base64, PostScript, Art). Large reduction; changes public API surface.
  - **(B) Remove only dead/unreachable code** (conservative): keep the full public API; remove
    genuinely unreferenced private members, unused `usings`, and now-irrelevant multi-target
    conditionals. Behavior- and API-preserving.
- **Recommended:** confirm with stakeholder. Default assumption = **(A)**, since the goal is a
  lean library for this specific use case, but (A) is destructive and should be explicitly approved.
- **Out of scope:** rewriting algorithms, performance tuning beyond code removal.
- **Verification:** `dotnet build -c Release` clean; `dotnet run --project QRCoderSamples`
  passes all self-checks.

### 4.2 Function Tree Map
- A hierarchical reference of the library's public entry points and their internal call chains
  (e.g. `CreateTotpQr → OneTimePassword.ToString → QRCodeGenerator.CreateQrCode → <renderer>`).
- **Format:** authored in Markdown (with an indented tree / Mermaid), then exported to PDF or
  Word for the "paper-based" reference.
- **Scope:** public API + one level of significant internal calls; not every private helper.

### 4.3 Sequence Diagram (`QRCoder_SequenceDiagram.docx`)
- Depicts the runtime flow of generating a QR code (sample app → OtpQrGenerator → QRCoder DLL →
  renderer → bytes), including the grid path.
- **Format:** authored as Mermaid/PlantUML, rendered to an image, embedded in a `.docx`.

### 4.4 API User Guide (`QRCoder_API_User_Guide.docx`)
- User-facing guide to the QRCoder API: installation/build, quick start, renderers, payloads,
  OTP helper, troubleshooting.
- **Must follow the provided template** (structure, headings, styling) — **template not yet
  available** (see §5).

### 4.5 Daily commits / change record
- Commit to the **private** repo at least daily; use clear messages; optionally maintain a
  `CHANGELOG.md`.
- **Note:** an AI session cannot span calendar days on its own — cadence is executed per working
  session; a scheduled reminder or manual routine is needed for true "daily."

### 4.6 Final upload
- Push source + `QRCoder.dll` + sample app + both `.docx` files to the private repo.

## 5. Dependencies & blockers

1. **API-guide template — MISSING.** No template document was found in the workspace/Downloads.
   Needed before §4.4 can start. _Action: obtain the template file (path or upload)._
2. **`.docx` generation tooling — NOT INSTALLED.** Neither `pandoc` nor Python/`python-docx` is
   available. Required to produce §4.3 and §4.4 as real `.docx`. _Options: install pandoc, install
   Python+python-docx, or author in Markdown and convert in Word/Google Docs manually._
3. **Repository visibility.** Repo must be **private**; current visibility unconfirmed.
   _Action: verify/set repo to private (Settings → Danger Zone, or `gh repo edit --visibility private`
   once `gh` is installed)._
4. **Pending git sync.** The rebase+push from the prior task is unfinished; resolve before new work.
5. **Optimization scope (§4.1)** blocked on the (A) vs (B) decision.

## 6. Risks

- Aggressive optimization (A) may remove code that tests or the sample indirectly rely on →
  mitigate with a clean build + sample run after each removal.
- `.docx` fidelity to the template may be limited if generated programmatically → may need
  manual finishing in Word.
- "Daily commit" continuity depends on a human/scheduled trigger.

## 7. Acceptance criteria

- [ ] Optimized source builds clean and the sample app runs green.
- [ ] `QRCoder.dll` present and usable from a separate C# app.
- [ ] Function Tree Map covers all public entry points.
- [ ] `QRCoder_SequenceDiagram.docx` opens in Word with a legible diagram.
- [ ] `QRCoder_API_User_Guide.docx` matches the provided template’s structure.
- [ ] Private repo contains all deliverables with a dated commit history.

## 8. Open questions (need answers to proceed)

1. "Remove all unused code" — interpretation **(A) trim to sample usage** or **(B) dead-code only**?
2. Where is the **API User Guide template**? (file path or upload)
3. Is installing **pandoc** (or Python + python-docx) acceptable for generating the `.docx` files,
   or should docs be delivered as Markdown/PDF for manual conversion?
4. Is the target repo already **private**, and is `main`-direct commits fine (vs. PRs)?
5. Preferred format for the Function Tree Map — Word, PDF, or Markdown?
