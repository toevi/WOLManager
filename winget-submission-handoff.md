# Handoff: zgłoszenie `toevi.WOLManager` do winget

Stan na dzień: **2026-07-21**

Dokończenie zgłoszenia lokalnie z Claude Code w Visual Studio (Windows, `winget`,
pełny dostęp do GitHuba). Gałąź z manifestami jest już wypchnięta — zostaje tylko
walidacja lokalna i otwarcie PR-a.

---

## ✅ Co jest już zrobione

- Pobrane realne dane z release'u **v1.1.1** i potwierdzony **SHA256** z digestem assetu GitHuba.
- Zweryfikowane, że instalator jest podpisany Authenticode **`CN=tmfgroup`** + znacznik czasu DigiCert
  (kluczowa uwaga moderatora) oraz że manifest `toevi.WOLManager` jeszcze nie istnieje w winget-pkgs.
- Manifest zbudowany na wzór **zaakceptowanego `toevi.WinMD` 1.1.0** (ten sam fork i moderator @stephengillie),
  z poprawkami z planu:
  - `AppsAndFeaturesEntries` → **tylko `Publisher: tmfgroup`**, zagnieżdżone pod instalatorem
    (bez zlokalizowanego `DisplayName`, zgodnie z feedbackiem na #389759)
  - `Publisher: tmfgroup` w locale (zgodnie z podmiotem certyfikatu podpisującego)
  - `MinimumOSVersion: 10.0.19041.0`, `InstallerType: inno`, `Architecture: x64`, schemat `1.9.0`
- Trzy pliki manifestu wypchnięte na gałąź forka.

## 📦 Dane pakietu

| Pole | Wartość |
|------|---------|
| PackageIdentifier | `toevi.WOLManager` |
| Wersja | `1.1.1` |
| URL instalatora | https://github.com/toevi/WOLManager/releases/download/v1.1.1/WOLManager-Setup-1.1.exe |
| SHA256 | `22884C6975CDB66733BFC3429D622B935A9FE7E8E0BE264D6907C55881B3A84D` |
| Typ instalatora | `inno` (Inno Setup) |
| Architektura | x64 |
| Min. OS | `10.0.19041.0` |
| Licencja | MIT |
| Publisher | `tmfgroup` |
| Podpis | Authenticode `CN=tmfgroup` + timestamp DigiCert |

## 🌿 Stan repo winget-pkgs

- **Fork:** `toevi/winget-pkgs`
- **Gałąź (już wypchnięta):** `toevi.WOLManager-1.1.1`
- **Ścieżka plików:** `manifests/t/toevi/WOLManager/1.1.1/`
  - `toevi.WOLManager.yaml`
  - `toevi.WOLManager.installer.yaml`
  - `toevi.WOLManager.locale.en-US.yaml`

---

## ⛔ Czego NIE dało się dokończyć w sesji web

Otwarcie PR-a wymaga repo `microsoft/winget-pkgs`, a sesja Claude Code na webie blokuje
dodawanie repo innego właściciela niż `toevi`. Dodatkowo `winget validate` / `winget install`
wymagają Windowsa. Oba kroki wykonasz lokalnie.

---

## ➡️ Kroki do dokończenia (Windows / PowerShell)

### 1. Pobierz gałąź z forka
```powershell
git clone --branch toevi.WOLManager-1.1.1 https://github.com/toevi/winget-pkgs.git
cd winget-pkgs
```

### 2. Walidacja lokalna
```powershell
winget validate --manifest manifests\t\toevi\WOLManager\1.1.1
winget install --manifest manifests\t\toevi\WOLManager\1.1.1
```

### 3. Otwórz PR (gh CLI)
```powershell
gh pr create --repo microsoft/winget-pkgs --base master `
  --head toevi:toevi.WOLManager-1.1.1 `
  --title "New package: toevi.WOLManager version 1.1.1" `
  --body-file pr-body.md
```

Alternatywnie — przez przeglądarkę (tytuł wstępnie wypełniony):

```
https://github.com/microsoft/winget-pkgs/compare/master...toevi:winget-pkgs:toevi.WOLManager-1.1.1?expand=1&title=New%20package%3A%20toevi.WOLManager%20version%201.1.1
```

### Treść `pr-body.md`
Odhacz `winget validate` / `winget install` po ich przejściu lokalnie.

```markdown
## 📖 Description
New package submission: **toevi.WOLManager** version **1.1.1**.

WOL Manager is a Windows desktop tool for network administrators — Wake-on-LAN,
remote power control (restart/shutdown), RDP, SSH, network share access, and port
scanning in one place.

- Installer: Inno Setup, x64, from the official GitHub release (v1.1.1).
- Code signing: Authenticode-signed `CN=tmfgroup` + DigiCert timestamp — same cert
  as the already-published `toevi.WinMD` package.
- `AppsAndFeaturesEntries`: only `Publisher: tmfgroup` (no `DisplayName`), per prior
  moderator feedback (#389759) — the Inno Setup ARP DisplayName is localized.
- Structure mirrors the accepted `toevi.WinMD` 1.1.0 manifests.

## ✅ Checklist
- [x] Signed the Contributor License Agreement

## 📦 Manifest Checklist
- [x] Checked there aren't other open PRs for the same manifest
- [x] This PR only modifies one (1) manifest
- [x] Validated locally with `winget validate`
- [x] Tested locally with `winget install`
- [x] Manifest uses schema 1.9.0 (same as accepted toevi.WinMD 1.1.0)
```

---

## ⚠️ Uwagi

- Jeśli od zgłoszenia WinMD schemat **1.12** stał się wymagany, `winget validate` to wyłapie —
  wtedy wystarczy podbić trzy linie `ManifestVersion` z `1.9.0` na `1.12.0`
  (żadne wymagane pola się nie zmieniają) i wypchnąć ponownie na gałąź.
- Po otwarciu PR-a pilnuj pipeline'u: `Azure-Pipeline-passed` → `Validation-Completed` →
  `Moderator-Approved` → `Publish-Pipeline-Succeeded` → merge.
- Po odświeżeniu indeksu: `winget install toevi.WOLManager`.

## Checklist (stan)

- [x] `.exe` podpisany `CN=tmfgroup` (zweryfikowany)
- [x] SHA256 policzony z assetu release'u
- [x] Manifest: `Publisher: tmfgroup`, bez `DisplayName` w ARP
- [x] Manifesty wypchnięte na gałąź forka `toevi.WOLManager-1.1.1`
- [ ] `winget validate` OK — *do zrobienia lokalnie*
- [ ] `winget install --manifest` OK — *do zrobienia lokalnie*
- [ ] PR otwarty na fork toevi → microsoft:master
- [ ] Moderator-Approved + publikacja
