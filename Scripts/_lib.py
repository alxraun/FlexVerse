# parents: @.frostline/graph/p-flexverse-tooling.xml
"""The shared kernel of the Scripts/ commands, in one module (_lib.py).

Sections (top to bottom): core -> dotnet -> game -> runs -> ops (shared
pipeline stages + argv helpers). Domain-free machinery comes first; each
section only uses what precedes it. The Scripts/{test,play} commands own
their pipeline composition and import everything they need from here.
"""
from __future__ import annotations


# === core: the machinery layer — errors, config, models, hashing,
# === task store, the linear pipeline. Domain-free by contract: only
# === paths, the exit-code vocabulary and the pipeline live here.

# === errors: the 0/1/2 exit-code vocabulary ==================================

import sys
from enum import IntEnum

class ExitCode(IntEnum):
    """The 0/1/2 contract (S-VERIFICATION.REQ-001). Never widen."""
    PASS = 0
    FAIL = 1
    DID_NOT_RUN = 2


class ToolingError(Exception):
    """Base for all loud, expected tooling failures."""

    def __init__(self, message: str, code: ExitCode = ExitCode.DID_NOT_RUN):
        super().__init__(message)
        self.code = code


class ConfigError(ToolingError):
    """Unknown names, malformed declarations: fail at startup, before staging."""


class StageFailure(ToolingError):
    """A task failed; the pipeline cannot proceed."""


class HarnessDidNotRun(ToolingError):
    """Anti-false-green: silence is an error, never a pass."""


def die(msg: str, code: ExitCode = ExitCode.DID_NOT_RUN) -> None:
    """Print and exit with a contract code. The only authorized exit path."""
    print(f"[test] error: {msg}", file=sys.stderr, flush=True)
    raise SystemExit(int(code))


# === config: paths, coordinates, marker contract (data only) =================

import os
from dataclasses import dataclass
from enum import Enum
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[1]
CACHE = REPO_ROOT / ".cache"


class Profile(str, Enum):
    """The only source axis: where the game comes from (selection by name)."""
    LOCAL = "local"
    CI = "ci"

    @classmethod
    def parse(cls, value: str) -> "Profile":
        try:
            return cls(value)
        except ValueError:
            known = " | ".join(m.value for m in cls)
            raise ValueError(
                f"unknown profile '{value}' (expected: {known})") from None


@dataclass(frozen=True, slots=True)
class CacheLayout:
    """The .cache/ layout: regenerable by definition (PRIN-003).

      game/RimWorld/         shared immutable game install (mount ro -> /game)
      mods/<key>/            mod cache: downloaded exactly once per pin
      staging/<run>/         per-run scratch: {Mods, unity3d}
      runs/<run>/            per-run evidence dirs
      runs/latest -> <run>   symlink to the newest run
      pkg/{nuget,nuget-http,xdg}  dotnet-side tool caches (never ~/.nuget)
      tasks/                 task fingerprint store (NOT staging/)
      buildx/                docker build cache
    """
    root: Path = CACHE

    @property
    def game_root(self) -> Path:
        return self.root / "game"

    @property
    def game_install(self) -> Path:
        return self.game_root / "RimWorld"

    @property
    def mods(self) -> Path:
        return self.root / "mods"

    @property
    def staging(self) -> Path:
        return self.root / "staging"

    @property
    def runs(self) -> Path:
        return self.root / "runs"

    @property
    def latest(self) -> Path:
        return self.runs / "latest"

    @property
    def pkg(self) -> Path:
        return self.root / "pkg"

    @property
    def tasks(self) -> Path:
        return self.root / "tasks"

    @property
    def buildx(self) -> Path:
        return self.root / "buildx"

    def run_staging(self, run: str) -> Path:
        return self.staging / run

    def run_dir(self, run: str) -> Path:
        return self.runs / run

    def nuget_env(self) -> dict[str, str]:
        """Keep every dotnet-side cache inside .cache/pkg."""
        env = dict(os.environ)
        env["NUGET_PACKAGES"] = str(self.pkg / "nuget")
        env["NUGET_HTTP_CACHEPATH"] = str(self.pkg / "nuget-http")
        env["XDG_DATA_HOME"] = str(self.pkg / "xdg")
        return env

    def docker_env(self) -> dict[str, str]:
        """Keep buildx state inside .cache/buildx (never in the repo tree)."""
        env = dict(os.environ)
        env["DOCKER_CONFIG"] = str(self.buildx / "docker-config")
        env["BUILDX_CONFIG"] = str(self.buildx / "buildx")
        return env


#: Build cache exclusion: directory names never hashed as task inputs.
INPUT_EXCLUDE_DIRS = frozenset({"bin", "obj", ".cache", "__pycache__", ".git"})


def console(msg: str) -> None:
    """Human-facing gate log (stage-side; not the FlexVerse.Log contract)."""
    print(f"\n=== [test] {msg}", flush=True)


def warn(msg: str) -> None:
    print(f"[test] warning: {msg}", flush=True)


# === models: typed domain objects crossing task boundaries ===================

from dataclasses import dataclass


@dataclass(frozen=True, slots=True)
class RunSpec:
    """What to run: game source + extra mods + seed (CLI-interpreted once).

    The positional subject is harness-owned: `dotnet_filter` is the native
    dotnet test filter (xunit), `feature` is a .feature file name (pickle).
    """
    profile: Profile = Profile.LOCAL
    with_mods: tuple[str, ...] = ()
    seed: int = 42
    dotnet_filter: str | None = None   # xunit subject
    feature: str | None = None         # pickle subject
    vm: bool = False                   # play only: virtual display session


@dataclass(frozen=True, slots=True)
class GameInfo:
    """Provenance of the staged game install (the game-source artifact)."""
    platform: str
    directory: str
    executable: str
    version: str
    branch: str | None = None

    def as_stamp(self) -> dict:
        return {
            "platform": self.platform,
            "directory": self.directory,
            "executable": self.executable,
            "version": self.version,
            **({"branch": self.branch} if self.branch else {}),
        }


@dataclass(frozen=True, slots=True)
class ModPin:
    """A single mods.json mod declaration (source channel + coordinates)."""
    id: str
    source: str                       # github | workshop | workspace
    modDir: str
    repo: str | None = None
    tag: str | None = None
    asset: str | None = None
    sha256: str | None = None
    publishedFileId: str | int | None = None
    features: tuple[str, ...] = ()
    knownNoise: tuple[str, ...] = ()
    expectedLog: tuple[str, ...] = ()

    def as_stamp(self) -> dict:
        return {
            "id": self.id,
            "source": self.source,
            **({"sha256": self.sha256} if self.sha256 else {}),
        }


@dataclass(frozen=True, slots=True)
class RunComposition:
    """The materialized mod set of one run: base + --with extras."""
    name: str
    mods: tuple[str, ...]
    active_mods: tuple[str, ...]
    game_version: str

    def is_differential(self) -> bool:
        return self.name.startswith("compat-")


@dataclass(frozen=True, slots=True)
class RunPlan:
    """Everything the run and evidence stages need, resolved."""
    composition: RunComposition
    staged: tuple[ModPin, ...]
    filter: str
    seed: int

    @property
    def run_name(self) -> str:
        return self.composition.name

    @property
    def differential(self) -> bool:
        return self.composition.is_differential()

    def mod_stamps(self) -> list[dict]:
        return [m.as_stamp() for m in self.staged]

    def noise_terms(self, registry: dict[str, ModPin]) -> set[str]:
        out: set[str] = set()
        for pin in self.staged:
            declared = registry.get(pin.id)
            if declared:
                out.update(declared.knownNoise)
                out.update(declared.expectedLog)
        return out


@dataclass(frozen=True, slots=True)
class Scenario:
    """One Pickle scenario verdict line, derived from Player.log only."""
    name: str
    status: str                        # PASSED | FAILED


@dataclass(frozen=True, slots=True)
class Verdict:
    """The derived verdict section of report.json (anti-false-green)."""
    passed: bool
    exit_code: int
    harness_ran: bool
    scenarios: tuple[Scenario, ...]
    failed_scenarios: tuple[Scenario, ...]
    fv_verify_markers: int
    scenario_regions: tuple[str, ...]
    framework_errors: tuple[str, ...]
    other_errors_count: int
    other_errors_sample: tuple[str, ...]

    def as_dict(self) -> dict:
        return {
            "passed": self.passed,
            "exit_code": self.exit_code,
            "harness_ran": self.harness_ran,
            "scenarios": [{"name": s.name, "status": s.status}
                          for s in self.scenarios],
            "failed_scenarios": [{"name": s.name, "status": s.status}
                                 for s in self.failed_scenarios],
            "fv_verify_markers": self.fv_verify_markers,
            "scenario_regions": list(self.scenario_regions),
            "framework_errors": list(self.framework_errors),
            "other_errors_count": self.other_errors_count,
            "other_errors_sample": list(self.other_errors_sample),
        }


@dataclass(frozen=True, slots=True)
class DiffResult:
    """Differential compat verdict: FACTS only, noise subtracted."""
    base: str
    run: str
    scenario_sets_equal: bool
    outcomes_equal: bool
    new_framework_errors: tuple[str, ...]
    run_harness_ran: bool
    compatible: bool

    def as_dict(self) -> dict:
        return {
            "base": self.base,
            "run": self.run,
            "scenario_sets_equal": self.scenario_sets_equal,
            "outcomes_equal": self.outcomes_equal,
            "new_framework_errors": list(self.new_framework_errors),
            "run_harness_ran": self.run_harness_ran,
            "compatible": self.compatible,
        }


# === hashing: deterministic fingerprints of task inputs ======================

import hashlib
import json

_CHUNK = 1 << 20


def file_digest(path: Path) -> str:
    h = hashlib.sha256()
    with path.open("rb") as f:
        while chunk := f.read(_CHUNK):
            h.update(chunk)
    return h.hexdigest()


def tree_digest(roots: list[Path], *, base: Path | None = None,
                exclude_dirs: frozenset[str] = INPUT_EXCLUDE_DIRS) -> str:
    """sha256 over every file under `roots` (relpath + content digest).

    Excluded directory names are pruned during the walk (bin/obj/.cache ...).
    Missing roots contribute nothing: adding the first file changes the hash.
    """
    base = base or Path.cwd()
    base = base.resolve()
    entries: list[dict[str, str]] = []
    for root in roots:
        root = root if Path(root).is_absolute() else base / root
        root = Path(root)
        if root.is_file():
            entries.append({"p": str(root.relative_to(base)),
                            "d": file_digest(root)})
            continue
        if not root.is_dir():
            continue
        for path in sorted(root.rglob("*")):
            if not path.is_file():
                continue
            if any(part in exclude_dirs for part in path.relative_to(base).parts):
                continue
            entries.append({"p": str(path.relative_to(base)),
                            "d": file_digest(path)})
    doc = json.dumps(entries, sort_keys=True, separators=(",", ":"))
    return hashlib.sha256(doc.encode()).hexdigest()


def doc_digest(doc: object) -> str:
    """sha256 of any JSON-serializable object (canonical form)."""
    payload = json.dumps(doc, sort_keys=True, separators=(",", ":"),
                         default=str)
    return hashlib.sha256(payload.encode()).hexdigest()


# === store: stage skip-decisions =============================================

class TaskStore:
    """Remembers the last executed fingerprint per stage id."""

    def __init__(self, root: Path):
        self.root = root
        self.root.mkdir(parents=True, exist_ok=True)

    def _task_file(self, task_id: str) -> Path:
        return self.root / f"{doc_digest(task_id)[:24]}.json"

    def hit(self, task_id: str, fingerprint: str) -> bool:
        f = self._task_file(task_id)
        if not f.exists():
            return False
        try:
            return json.loads(f.read_text(encoding="utf-8"))["fp"] == fingerprint
        except Exception:
            return False

    def record(self, task_id: str, fingerprint: str) -> None:
        self._task_file(task_id).write_text(
            json.dumps({"task": task_id, "fp": fingerprint}, indent=2),
            encoding="utf-8")


# === pipeline: linear stages with fingerprint resume =========================

from typing import Any, Callable  # noqa: E402

#: A stage body: (cached: bool) -> typed result. On a fingerprint hit the
#: body recomputes cheap outputs and skips expensive side effects (PRIN-002).
StageBody = Callable[[bool], Any]

#: Optional input fingerprint: identical value => the stage resumes.
Fingerprint = Callable[[], str]


class Pipeline:
    """Run declared stages in order; results are typed outputs by stage id.

    Deliberately linear, no dependency machinery: every command's flow is a
    straight line, so the composition is the call order itself (and stays
    readable as such). Fingerprint-hit stages resume instead of re-running
    expensive side effects; stages without a fingerprint always run.
    """

    def __init__(self):
        self._store = TaskStore(root=CacheLayout().tasks)
        self.outputs: dict[str, Any] = {}

    def run(self, name: str, body: StageBody,
            fingerprint: Fingerprint | None = None,
            description: str = "") -> Any:
        cached = bool(fingerprint) and self._store.hit(name, fingerprint())
        console(f"stage {name}"
                + (": cached, resuming" if cached else "")
                + (f" — {description}" if description else ""))
        value = body(cached)
        self.outputs[name] = value
        if fingerprint is not None and not cached:
            self._store.record(name, fingerprint())
        return value

    def out(self, name: str) -> Any:
        """A declared earlier stage's output; reading ahead fails loudly."""
        try:
            return self.outputs[name]
        except KeyError:
            raise StageFailure(f"stage '{name}' has no output (not run yet?)") \
                from None


# === dotnet domain: builds and the offline xunit harness. MSBuild
# === remains the single build mechanism (M-MSBUILD); invoked here
# === as a command, with content-addressed skip decisions.

import subprocess
from dataclasses import dataclass
from pathlib import Path



@dataclass(frozen=True, slots=True)
class ProjectSet:
    """Declarative project coordinates consumed by the dotnet domain."""
    product: tuple[str, ...]
    test: str
    test_mods: tuple[str, ...]


#: The built surface of the repository (the dotnet domain's own coordinates).
PROJECTS = ProjectSet(
    product=(
        "Source/FlexVerse/FlexVerse.csproj",
        "Source/FlexVerse.SourceGenerators/FlexVerse.SourceGenerators.csproj",
        "Source/FlexVerse.PropertyWeaver.Fody/FlexVerse.PropertyWeaver.Fody.csproj",
        "Source/FlexVerse.Package/FlexVerse.Package.csproj",
        "Source/FlexTools/FlexTools.csproj",
    ),
    test="Source/FlexVerse.Test/Xunit",
    test_mods=(
        "Source/FlexVerse.Test/Explorer/FlexVerse.Test.Explorer.csproj",
        "Source/FlexVerse.Test/Pickle/FlexVerse.Test.Pickle.csproj",
    ),
)


def _dotnet(args: list[str], env: dict[str, str]) -> None:
    argv = ["dotnet", *args]
    print(f"[test] $ {' '.join(argv)}", flush=True)
    proc = subprocess.run(argv, cwd=REPO_ROOT, env=env)
    if proc.returncode != 0:
        raise StageFailure(
            f"`{' '.join(argv)}` exited {proc.returncode}",
            code=ExitCode.DID_NOT_RUN)


#: Gate builds restore with NuGetAudit off: the vulnerability-data worker
#: ignores nuget_env cache redirects (NU1900 noise) and audits belong to the
#: delivery command, not the verification gate.
_AUDIT_OFF = ["-p:NuGetAudit=false"]


def source_fingerprint() -> str:
    """Content fingerprint of everything that can change build outputs."""
    roots = [Path("Source"), Path("Directory.Build.props"),
             Path("Directory.Build.targets"), Path("global.json"),
             Path("nuget.config")]
    return tree_digest(roots, base=REPO_ROOT)


def make_product(cached: bool) -> str:
    """All projects (product + test mods) in Debug — the development build.

    Release is NOT built here: iterations pay Debug-only costs. A Release
    compile check belongs to the delivery concern, not to the verification
    gate. The game-consumed mod must be Debug so that
    [Conditional("DEBUG")] verification markers reach the container."""
    console("build: product projects (Debug)")
    env = CacheLayout().nuget_env()
    if not cached:
        for proj in (*PROJECTS.product, *PROJECTS.test_mods):
            _dotnet(["build", proj, "-c", "Debug", "--nologo", "-v", "q",
                     *_AUDIT_OFF], env)
    return source_fingerprint()


def xunit_run(filter: str | None = None, *, check: bool = True) -> int:
    """The offline xunit harness. Returns dotnet's exit code; with check=True
    a nonzero exit fails loudly as harness-did-not-run.
    `filter` is the native dotnet test filter (class/namespace/wildcard)."""
    args = ["test", PROJECTS.test, "-c", "Release", "--nologo",
            *_AUDIT_OFF]
    if filter:
        args += ["--filter", "|".join(filter.split(","))]
    argv = ["dotnet", *args]
    print(f"[test] $ {' '.join(argv)}", flush=True)
    proc = subprocess.run(argv, cwd=REPO_ROOT,
                          env=CacheLayout().nuget_env())
    if check and proc.returncode != 0:
        raise StageFailure(
            f"`{' '.join(argv)}` exited {proc.returncode}",
            code=ExitCode.DID_NOT_RUN)
    return proc.returncode


# === game domain: finding, provisioning and running RimWorld.
# === Everything about the external game: local Steam discovery,
# === steamcmd CI provisioning, the runtime container image (inline
# === below), and BOTH container runs through ONE launch mechanism.

import base64
import datetime as _dt
import json
import os
import re
import shutil
import subprocess
import sys
from pathlib import Path


#: Game source marker: shared install provenance.
GAME_SOURCE_MARKER = CACHE / "game" / ".fv-source.json"

#: Discovery cache for the local RimWorld adapter.
FIND_GAME_CACHE = CACHE / "find-rimworld.json"

#: Docker coordinates (the game domain).
GAME_IMAGE = "flexverse/game-base:latest"
STEAMCMD_IMAGE = "cm2network/steamcmd:latest"
RIMWORLD_APP_ID = "294100"


def appdata(run: str) -> Path:
    """The Unity config dir of one run (linux form): a sibling of Mods/,
    mirroring the two mount points of the real world."""
    return (CacheLayout().run_staging(run) / "unity3d" /
            "Ludeon Studios" / "RimWorld by Ludeon Studios" / "Config")


#: The only source axis: where the game comes from (selection by name,
#: P-FLEXVERSE-TOOLING.PRIN-006). Credentials come from the environment.
SOURCE_ADAPTERS = {
    Profile.LOCAL: lambda: stage_local(),
    Profile.CI: lambda: stage_ci(os.environ.get("GAME_BRANCH")),
}


# === local discovery =========================================================

EXECUTABLES = {
    "linux": "RimWorldLinux",
    "windows": "RimWorldWin64.exe",
    "macos": "RimWorldMac.app/Contents/MacOS/RimWorldMac",
}


def find_installation(use_cache: bool = True) -> dict | None:
    """Return {'platform', 'directory', 'executable', 'version'} or None.
    FLEXVERSE_GAME_DIR overrides everything."""
    if override := os.environ.get("FLEXVERSE_GAME_DIR"):
        return _describe(Path(override))

    if use_cache and FIND_GAME_CACHE.exists():
        try:
            cached = json.loads(FIND_GAME_CACHE.read_text(encoding="utf-8"))
            if Path(cached["directory"]).exists():
                return cached
        except Exception:
            pass

    platform = _platform()
    info = None
    for steam_root in _steam_roots(platform):
        info = _search_steam_root(steam_root, platform)
        if info:
            break

    if info:
        FIND_GAME_CACHE.parent.mkdir(parents=True, exist_ok=True)
        FIND_GAME_CACHE.write_text(json.dumps(info, indent=2), encoding="utf-8")
    return info


def _platform() -> str:
    if sys.platform.startswith("win"):
        return "windows"
    if sys.platform == "darwin":
        return "macos"
    return "linux"


def _steam_roots(platform: str) -> list[Path]:
    home = Path.home()
    if platform == "windows":
        roots = []
        for hive_key in (
            (r"SOFTWARE\Valve\Steam", 0),              # HKCU
            (r"SOFTWARE\WOW6432Node\Valve\Steam", 1),  # HKLM view
        ):
            try:
                import winreg
                hive = winreg.HKEY_CURRENT_USER if hive_key[1] == 0 \
                    else winreg.HKEY_LOCAL_MACHINE
                with winreg.OpenKey(hive, hive_key[0]) as k:
                    roots.append(Path(winreg.QueryValueEx(k, "SteamPath")[0]))
            except OSError:
                pass
        return roots
    if platform == "macos":
        return [home / "Library" / "Application Support" / "Steam"]
    return [home / ".steam" / "steam", home / ".local" / "share" / "Steam"]


def _search_steam_root(steam_root: Path, platform: str) -> dict | None:
    libraries = [steam_root]
    vdf = steam_root / "steamapps" / "libraryfolders.vdf"
    if vdf.exists():
        for m in re.finditer(r'"path"\s+"([^"]+)"',
                             vdf.read_text(encoding="utf-8", errors="replace")):
            p = Path(m.group(1).replace("\\\\", "\\"))
            if p not in libraries:
                libraries.append(p)
    for lib in libraries:
        game_dir = lib / "steamapps" / "common" / "RimWorld"
        info = _describe(game_dir, platform)
        if info:
            return info
    return None


def _describe(game_dir: Path, platform: str | None = None) -> dict | None:
    platform = platform or _platform()
    exe = game_dir / EXECUTABLES[platform]
    if not exe.exists():
        return None
    return {
        "platform": platform,
        "directory": str(game_dir),
        "executable": EXECUTABLES[platform],
        "version": _read_version(game_dir),
    }


def _read_version(game_dir: Path) -> str:
    """Best-effort version read; 'unknown' is acceptable — the CI stamp is
    the arbiter of truth, local skew only warns."""
    for candidate in ("Version.txt", "version.txt",
                      "RimWorldLinux_Data/version.txt"):
        f = game_dir / candidate
        if f.exists():
            try:
                return f.read_text(encoding="utf-8").strip()
            except OSError:
                pass
    return "unknown"


# === provisioning adapters: both produce .cache/game/RimWorld ================

def stage_local() -> GameInfo:
    raw = find_installation()
    if not raw:
        raise ConfigError(
            "no local RimWorld installation found. Install the game via your "
            "own Steam, or set FLEXVERSE_GAME_DIR.")
    src = Path(raw["directory"])
    _install_from(src, raw)
    return GameInfo(platform=raw["platform"], directory=str(src),
                    executable=raw["executable"], version=raw["version"])


def stage_ci(branch: str | None) -> GameInfo:
    """steamcmd provisioning. STEAM_CONFIG_VDF_B64+STEAM_USERNAME preferred
    (no password travels anywhere), STEAM_USER/STEAM_PASSWORD legacy.
    steamcmd self-updates on first run and rewrites its config dir, so we
    bootstrap (+quit) BEFORE planting config.vdf."""
    cache = CacheLayout()
    user = os.environ.get("STEAM_USERNAME") or os.environ.get("STEAM_USER")
    vdf_b64 = os.environ.get("STEAM_CONFIG_VDF_B64")
    password = os.environ.get("STEAM_PASSWORD")
    session = cache.root / "steam-session"

    volumes = ["-v", f"{cache.game_root}:/home/steam/Game"]
    if vdf_b64 and user:
        session.mkdir(parents=True, exist_ok=True)
        _docker(_STEAMCMD_IMAGE, ["/home/steam/steamcmd/steamcmd.sh", "+quit"],
                volumes=["-v", f"{session}:/home/steam/Steam"],
                check=False)  # bootstrap self-update; rc may be nonzero
        cfg = session / "config"
        cfg.mkdir(parents=True, exist_ok=True)
        vdf = cfg / "config.vdf"
        vdf.write_bytes(base64.b64decode(vdf_b64))
        vdf.chmod(0o600)
        volumes = ["-v", f"{session}:/home/steam/Steam", *volumes]
        login = ["+login", user]
    elif user and password:
        login = ["+login", user, password]
    else:
        raise ConfigError(
            "CI provisioning requires STEAM_CONFIG_VDF_B64+STEAM_USERNAME "
            "(preferred) or STEAM_USER/STEAM_PASSWORD")

    args = ["+force_install_dir", "/home/steam/Game", *login,
            "+app_update", RIMWORLD_APP_ID]
    if branch:
        args += ["-beta", branch]
    args += ["validate", "+quit"]
    console(f"provisioning RimWorld via steamcmd (branch: {branch or 'public'})")
    _docker(_STEAMCMD_IMAGE, ["/home/steam/steamcmd/steamcmd.sh", *args],
            volumes)
    if not (cache.game_install / "RimWorldLinux").exists():
        raise ConfigError("steamcmd provisioning produced no game")
    return GameInfo(platform="linux", directory=str(cache.game_install),
                    executable="RimWorldLinux", version="ci",
                    branch=branch or "public")


#: DLC Data folders, by directory name. Runs are physically DLC-free: the
#: shared install simply never receives them (ModLister then sees no
#: installed expansions at all — no menu backgrounds, no defs, nothing).
DLC_DATA_DIRS = ("Royalty", "Ideology", "Biotech", "Anomaly", "Odyssey")

#: Marker schema of the staged-install provenance; bump to force one re-copy
#: of the shared install (e.g. when the copy layout changes).
GAME_MARKER_LAYOUT = 2


def _install_from(src: Path, raw: dict) -> None:
    """Ensure the shared install matches the host source; the provenance
    marker (layout + directory + mtime + version) makes re-staging a skip.
    DLC Data folders are excluded from the copy."""
    cache = CacheLayout()
    install = cache.game_install
    src_mtime = max((f.stat().st_mtime for f in src.rglob("*") if f.is_file()),
                    default=0.0)

    if _marker_matches(src, src_mtime):
        print("  game already staged (source unchanged)")
        return
    console(f"staging game from local install: {src} (DLC excluded)")
    if install.exists():
        shutil.rmtree(install)

    def skip_dlc(dirpath: str, names: list[str]) -> list[str]:
        # only the top level of the game's Data/: a mod subfolder that merely
        # shares a DLC's name must survive the copy
        if Path(dirpath) == src / "Data":
            return [n for n in names if n in DLC_DATA_DIRS]
        return []

    shutil.copytree(src, install, symlinks=True, ignore=skip_dlc)
    _write_marker(src, src_mtime, raw, None)


def _marker() -> dict:
    if not GAME_SOURCE_MARKER.exists():
        return {}
    try:
        return json.loads(GAME_SOURCE_MARKER.read_text(encoding="utf-8"))
    except Exception:
        return {}


def _marker_matches(src: Path, src_mtime: float) -> bool:
    cache = CacheLayout()
    if not (cache.game_install / "RimWorldLinux").exists():
        return False
    prev = _marker()
    return (prev.get("layout") == GAME_MARKER_LAYOUT
            and prev.get("directory") == str(src)
            and abs(prev.get("mtime", 0) - src_mtime) < 1.0)


def _write_marker(src: Path, src_mtime: float, raw: dict,
                  branch: str | None) -> None:
    GAME_SOURCE_MARKER.write_text(json.dumps(
        {"layout": GAME_MARKER_LAYOUT, "platform": _platform(),
         "directory": str(src), "mtime": src_mtime,
         "version": raw["version"], **({"branch": branch} if branch else {})},
        indent=2), encoding="utf-8")


def last_info() -> GameInfo | None:
    """Resume data for a cached game-source task: the marker's provenance.

    Reconstructs the full typed GameInfo the adapter would have produced;
    returns None when no staged install is recorded."""
    if not GAME_SOURCE_MARKER.exists():
        return None
    try:
        raw = json.loads(GAME_SOURCE_MARKER.read_text(encoding="utf-8"))
        platform = raw.get("platform", _platform())
        return GameInfo(platform=platform,
                        directory=raw["directory"],
                        executable=EXECUTABLES[platform],
                        version=raw.get("version", "unknown"),
                        branch=raw.get("branch"))
    except Exception:
        return None


# === container image: defined inline, game-free ==============================
# Public, game-free base for the runnable image, derived from
# RimWorks/steam-game-image-action. The game is provided by a read-only
# mount (local Steam install or steamcmd provisioning), never baked in.

_DOCKERFILE = """\
FROM ubuntu:24.04
ENV DEBIAN_FRONTEND=noninteractive
RUN apt-get update \\
    && apt-get install -y --no-install-recommends \\
        ca-certificates \\
        libasound2t64 \\
        libgl1 \\
        libglu1-mesa \\
        libgtk-3-0 \\
        libnss3 \\
        libpulse0 \\
        libsdl2-2.0-0 \\
        libx11-6 \\
        libxcursor1 \\
        libxext6 \\
        libxi6 \\
        libxinerama1 \\
        libxrandr2 \\
        libxrender1 \\
        libwayland-client0 \\
        libwayland-cursor0 \\
        libwayland-egl1 \\
        libegl1 \\
        libegl-mesa0 \\
        libxkbcommon0 \\
        locales \\
        xdotool \\
        xvfb \\
        imagemagick \\
        x11-apps \\
    && rm -rf /var/lib/apt/lists/*
ENV LANG=C.UTF-8
COPY --chmod=755 run-headless /usr/local/bin/run-headless
# no USER: run-headless needs root to chown the mount parents, then drops to uid 1000.
RUN userdel --remove ubuntu 2>/dev/null; useradd --create-home --uid 1000 app
ENV HOME=/home/app
WORKDIR /game
# Pre-create the mountpoint for the per-run Mods overlay: /game is a read-only
# bind mount at create time, so runc cannot mkdir /game/Mods itself.
RUN mkdir -p /game/Mods
"""

#: The `run-headless` entrypoint: root drops to the game user (chowning only
#: the mount parents docker created), then runs the game under Xvfb.
_RUN_HEADLESS = """\
#!/bin/sh
# run-headless /game/YourGame <args...>
set -eu
: "${SCREEN:=1920x1080x24}"
: "${GAME_UID:=1000}"

drop_to_game_user() {
  [ "$(id -u)" = 0 ] || return 0

  chown "$GAME_UID:$GAME_UID" "$HOME" 2>/dev/null || true

  # only the parents docker created for the mounts, never a mount itself, or this
  # chowns the caller's host files. mountinfo octal-escapes space, tab and backslash.
  awk -v home="$HOME" '
    { mp = $5
      gsub(/\\\\040/, " ",  mp); gsub(/\\\\011/, "\\t", mp)
      gsub(/\\\\012/, "\\n", mp); gsub(/\\\\134/, "\\\\", mp)
      if (index(mp, home "/") == 1) print mp }
  ' /proc/self/mountinfo \\
  | while IFS= read -r mp; do
      d=$(dirname "$mp")
      while [ "$d" != "$HOME" ] && [ "$d" != "/" ]; do
        chown "$GAME_UID:$GAME_UID" "$d" 2>/dev/null || true
        d=$(dirname "$d")
      done
    done

  exec setpriv --reuid "$GAME_UID" --regid "$GAME_UID" --clear-groups \\
    env HOME="$HOME" SCREEN="$SCREEN" DESKTOP="${DESKTOP:-}" "$0" "$@"
}

# do not exec this. Xvfb signals SIGUSR1 to xvfb-run when the display is ready and
# that never lands if xvfb-run is PID 1, which is what makes --init unnecessary.
run_under_xvfb() {
  xvfb-run -a -s "-screen 0 $SCREEN" "$@" &
  child=$!
  trap 'kill -TERM "$child" 2>/dev/null' HUP INT TERM
  status=0
  wait "$child" || status=$?
  return "$status"
}

drop_to_game_user "$@"
run_under_xvfb "$@"
"""


def image_fingerprint() -> str:
    """Content fingerprint of the inline image definition."""
    return doc_digest({"dockerfile": _DOCKERFILE,
                       "entrypoint": _RUN_HEADLESS})


def build_image() -> None:
    """The runtime-deps-only image, built from the inline definition.
    Build context + cache live under .cache (regenerable, PRIN-003)."""
    ctx = CacheLayout().buildx / "image-context"
    ctx.mkdir(parents=True, exist_ok=True)
    (ctx / "Dockerfile").write_text(_DOCKERFILE, encoding="utf-8")
    (ctx / "run-headless").write_text(_RUN_HEADLESS, encoding="utf-8")
    _run_docker_env(["docker", "build", "-q", "-t", GAME_IMAGE, str(ctx)])


# === container runs: ONE launch mechanism ====================================

def _mounts(plan_run_name: str, run_dir: Path) -> list[str]:
    staging = CacheLayout().run_staging(plan_run_name)
    return [
        "--volume", f"{CacheLayout().game_install}:/game",
        "--volume", f"{staging / 'Mods'}:/game/Mods:ro",
        "--volume", f"{staging / 'unity3d'}:/home/app/.config/unity3d",
        "--volume", f"{run_dir}:/reports",
    ]


def run_test(plan: RunPlan, run_dir: Path) -> int:
    """One headless Pickle run: the SAME launch mechanism as play, with the
    harness entrypoint. Assumes the `build_image` graph task has run.
    Returns the game's exit code."""
    run_dir.mkdir(parents=True, exist_ok=True)
    argv = ["docker", "run", "--rm", *_mounts(plan.run_name, run_dir),
            "--entrypoint", "run-headless", GAME_IMAGE, "/game/RimWorldLinux",
            f"-pickle-run={plan.filter}",
            f"-pickle-seed={plan.seed}",
            "-pickle-report-dir=/reports",
            "-pickle-no-http",
            "-logfile", "/reports/Player.log"]
    return subprocess.run(argv).returncode


def run_play(plan: RunPlan, run_dir: Path) -> int:
    """Interactive session: same container, GUI passthrough to the host.

    Backend auto-detection: Wayland socket (preferred by the Unity player),
    else X11/XWayland (FLEXVERSE_X11_SOCKET overrides; WSLg uses
    /mnt/wslg/.X11-unix) with XAUTHORITY when present. GPU via /dev/dri.
    On Windows/macOS use a third-party X server pointed at host.docker.internal:0.
    """
    run_dir.mkdir(parents=True, exist_ok=True)
    opts = _gui_options()
    argv = ["docker", "run", "--rm", *_mounts(plan.run_name, run_dir), *opts,
            "--entrypoint", "/bin/sh", GAME_IMAGE, "-c",
            "exec setpriv --reuid 1000 --regid 1000 --clear-groups "
            "env HOME=/home/app /game/RimWorldLinux -logfile /reports/Player.log"]
    console("game window opens on the host display; quit the game to end "
            "the session")
    return subprocess.run(argv).returncode


def _gui_options() -> list[str]:
    wayland_socket = os.environ.get("WAYLAND_DISPLAY")
    x11_dir = os.environ.get("FLEXVERSE_X11_SOCKET", "/tmp/.X11-unix")
    opts: list[str] = []

    if wayland_socket:
        runtime_dir = os.environ.get("XDG_RUNTIME_DIR")
        if not runtime_dir:
            raise ToolingError(
                "WAYLAND_DISPLAY set but XDG_RUNTIME_DIR is not")
        socket_path = Path(wayland_socket)
        if not socket_path.is_absolute():
            socket_path = Path(runtime_dir) / wayland_socket
        if not socket_path.exists():
            raise ToolingError(f"Wayland socket not found: {socket_path}")
        opts += ["--volume", f"{socket_path}:/run/user/1000/{wayland_socket}",
                 "-e", f"WAYLAND_DISPLAY={wayland_socket}",
                 "-e", f"XDG_RUNTIME_DIR=/run/user/1000"]
        console("play backend: native Wayland")
    elif os.environ.get("DISPLAY"):
        if not Path(x11_dir).is_dir():
            raise ToolingError(
                f"no X11 socket dir at {x11_dir} (set FLEXVERSE_X11_SOCKET "
                "or start XWayland)")
        opts += ["--volume", f"{x11_dir}:{x11_dir}:ro",
                 "-e", f"DISPLAY={os.environ['DISPLAY']}"]
        xauth = os.environ.get("XAUTHORITY") or \
            str(Path.home() / ".Xauthority")
        if Path(xauth).is_file():
            opts += ["--volume", f"{xauth}:/home/app/.Xauthority:ro",
                     "-e", f"XAUTHORITY=/home/app/.Xauthority"]
        console(f"play backend: X11 on display {os.environ['DISPLAY']}")
    else:
        raise ToolingError(
            "play needs a GUI socket: WAYLAND_DISPLAY (Wayland) or "
            "DISPLAY (X11/XWayland/X server)")

    if Path("/dev/dri").exists():
        opts += ["--device", "/dev/dri"]
    return opts


# === virtual-display play (Scripts/play --vm) ================================
#
# The game, a virtual screen, mouse/keyboard input and image capture all live
# INSIDE one container (pattern: ProjectPandemonium run_play_vm). No host
# display socket is mounted and no host dotool/grim/cursor is involved: the
# agent drives the session with `docker exec ... xdotool / import`.
# This is NOT headless: the player renders fully on the virtual display.

VM_DISPLAY = ":99"
VM_GEOMETRY = "1600x900x24"
VM_WIDTH = 1600
VM_HEIGHT = 900


def _vm_prefs_fix() -> str:
    """Pin the Unity window resolution in the staged Prefs.xml before launch.
    The game re-asserts this file's resolution on scene changes (and rewrites
    it from host-monitor sessions), so window fitting alone always loses."""
    cfg = ("/home/app/.config/unity3d/Ludeon Studios/"
           "RimWorld by Ludeon Studios/Config/Prefs.xml")
    return (
        f' if [ -f "{cfg}" ]; then sed -i'
        f" -e 's#<screenWidth>[0-9]*</screenWidth>#"
        f"<screenWidth>{VM_WIDTH}</screenWidth>#'"
        f" -e 's#<screenHeight>[0-9]*</screenHeight>#"
        f"<screenHeight>{VM_HEIGHT}</screenHeight>#'"
        f' "{cfg}"; chown 1000:1000 "{cfg}"; fi'
    )


def _vm_inner() -> str:
    """The in-container entry script: start Xvfb, WAIT for the display to be
    ready (xdotool polling, no fixed sleep), run the game fullscreen-off, then
    WAIT for the game window and FIT it to the virtual screen — Unity ignores
    our Prefs sizing on first boot and opens 1920x1080 on Xvfb. The game's
    exit status is recorded into the mounted run dir; the container then
    stops (never auto-removed)."""
    return (
        f"Xvfb {VM_DISPLAY} -screen 0 {VM_GEOMETRY} -nolisten tcp &"
        " xvfb=$!;"
        f" i=0; until DISPLAY={VM_DISPLAY} xdotool getdisplaygeometry"
        " >/dev/null 2>&1; do"
        " i=$((i+1)); if [ $i -gt 100 ]; then"
        " echo 'vm: X server did not become ready' >&2; exit 3; fi;"
        " sleep 0.2; done;"
        f" echo \"vm: display ready: $(DISPLAY={VM_DISPLAY} "
        "xdotool getdisplaygeometry)\";"
        + _vm_prefs_fix() + ";"
        f" setpriv --reuid 1000 --regid 1000 --clear-groups "
        f"env HOME=/home/app DISPLAY={VM_DISPLAY}"
        f" /game/RimWorldLinux -logfile /reports/Player.log"
        f" -screen-width {VM_WIDTH} -screen-height {VM_HEIGHT} &"
        " game=$!;"
        " n=0; w='';"
        " until w=$(DISPLAY=:99 xdotool search --onlyvisible"
        " --name 'RimWorld' 2>/dev/null | head -1) && [ -n \"$w\" ]; do"
        " n=$((n+1)); if [ $n -gt 240 ]; then"
        " echo 'vm: game window not found' >&2; exit 4; fi;"
        " sleep 0.5; done;"
        # Unity asserts its own resolution while booting (and rewrites Prefs);
        # fitting early loses the race. Wait until the geometry is stable,
        # then fit and verify (bounded retries).
        " prev=''; stable=0; i=0;"
        " while [ $stable -lt 5 ] && [ $i -lt 90 ]; do"
        f" cur=$(DISPLAY={VM_DISPLAY} xdotool getwindowgeometry --shell \"$w\""
        " | grep -E '^(WIDTH|HEIGHT)=' | cut -d= -f2 | tr '\\n' 'x');"
        " [ \"$cur\" = \"$prev\" ] && stable=$((stable+1)) || stable=0;"
        " prev=$cur; i=$((i+1)); sleep 1; done;"
        " k=0;"
        f" while [ $k -lt 10 ]; do"
        f" cur=$(DISPLAY={VM_DISPLAY} xdotool getwindowgeometry --shell \"$w\""
        " | grep -E '^(WIDTH|HEIGHT)=' | cut -d= -f2 | tr '\\n' 'x');"
        f" [ \"$cur\" = \"{VM_WIDTH}x{VM_HEIGHT}x\" ] && break;"
        f" DISPLAY={VM_DISPLAY} xdotool windowsize --sync \"$w\""
        f" {VM_WIDTH} {VM_HEIGHT};"
        f" DISPLAY={VM_DISPLAY} xdotool windowmove \"$w\" 0 0;"
        " k=$((k+1)); sleep 1; done;"
        f" DISPLAY={VM_DISPLAY} xdotool windowfocus --sync \"$w\" || true;"
        " echo \"$w\" > /reports/window-id;"
        " echo \"vm: game window $w fitted to the screen\";"
        " wait $game;"
        " rc=$?;"
        " echo \"$rc\" > /reports/exit-status;"
        " echo \"vm: game exited with status $rc\";"
        " exit $rc"
    )


def run_play_vm(run_name: str, run_dir: Path) -> str:
    """One virtual-display play session over the run dir `run_name` (the
    composition name — vm and ordinary play share 'base'): launch a uniquely
    named detached container, write session.json. Returns the container name.

    The session stays live while the game runs; drive it with
    `docker exec -e DISPLAY=:99 <container> xdotool ...` and capture with
    `import -window root`. End it with vm_stop(container) (graceful) or
    `docker stop` / `docker rm` (forced — reported as such)."""
    run_dir.mkdir(parents=True, exist_ok=True)
    (run_dir / "screenshots").mkdir(exist_ok=True)
    container = "fv-play-vm-" + _dt.datetime.now().strftime("%Y%m%d-%H%M%S")
    if _container_exists(container):
        raise ToolingError(
            f"container '{container}' already exists — retry in a second")
    (run_dir / "session.json").write_text(json.dumps({
        "schema": "flexverse.vm-session/1",
        "session": container,
        "run": run_name,
        "container": container,
        "display": VM_DISPLAY,
        "geometry": VM_GEOMETRY,
        "image": GAME_IMAGE,
        "window": {"width": VM_WIDTH, "height": VM_HEIGHT,
                   "fullscreen": False, "position": [0, 0],
                   "decorated": False},
        "run_dir": str(run_dir),
        "screenshots": str(run_dir / "screenshots"),
        "game_args": ["-logfile", "/reports/Player.log",
                      "-screen-width", str(VM_WIDTH),
                      "-screen-height", str(VM_HEIGHT)],
        "host_display_used": False,
        "status": "running",
        "started": _dt.datetime.now().isoformat(timespec="seconds"),
    }, indent=2), encoding="utf-8")

    argv = ["docker", "run", "-d", "--name", container,
            *_mounts(run_name, run_dir),
            "--entrypoint", "/bin/sh", GAME_IMAGE, "-c", _vm_inner()]
    print(f"[test] $ {' '.join(argv)}", flush=True)
    proc = subprocess.run(argv, capture_output=True, text=True)
    if proc.returncode != 0:
        raise ToolingError(
            f"vm container failed to start: {proc.stderr.strip()}")
    console(f"vm session live: container '{container}', "
            f"DISPLAY={VM_DISPLAY}, run dir: {run_dir}")
    console(f"drive: docker exec -e DISPLAY={VM_DISPLAY} {container} "
            f"xdotool ... ; stop: uv run Scripts/play --stop {container}")
    return container


def vm_stop(container: str, *, force: bool = False) -> int:
    """End a vm session. Default: graceful — SIGTERM to the game process and
    wait for the container to stop on its own (the game records its exit
    status). force=True is the loud, non-graceful path (docker stop timer)."""
    sig = "-9" if force else "-TERM"
    who = "docker stop (SIGKILL after timeout)" if force \
        else "SIGTERM to the game process"
    console(f"stopping vm session '{container}': {who}")
    proc = subprocess.run(["docker", "exec", container,
                           "pkill", sig, "-x", "RimWorldLinux"],
                          capture_output=True, text=True)
    if force or proc.returncode != 0:
        # nothing to signal (or forced): fall back to the docker-level stop
        return subprocess.run(["docker", "stop", "-t",
                               "0" if force else "30", container]).returncode
    return subprocess.run(["docker", "wait", container],
                          capture_output=True, text=True).returncode


def _container_exists(name: str) -> bool:
    proc = subprocess.run(
        ["docker", "container", "inspect", name],
        capture_output=True, text=True,
        env=CacheLayout().docker_env())
    return proc.returncode == 0


# === subprocess plumbing =====================================================

def _docker(image: str, cmd: list[str], volumes: list[str],
            *, check: bool = True) -> None:
    cache = CacheLayout()
    argv = ["docker", "run", "--rm", *volumes, image, *cmd]
    print(f"[test] $ {' '.join(argv)}", flush=True)
    proc = subprocess.run(argv, env=cache.docker_env())
    if check and proc.returncode != 0:
        raise ConfigError(f"`{' '.join(argv)}` exited {proc.returncode}")


def _run_docker_env(cmd: list[str]) -> None:
    print(f"[test] $ {' '.join(cmd)}", flush=True)
    proc = subprocess.run(cmd, env=CacheLayout().docker_env())
    if proc.returncode != 0:
        raise ToolingError(f"`{' '.join(cmd)}` exited {proc.returncode}")


# === run domain: mod sets, per-run staging, and evidence. A run is
# === one staged composition with its own Mods overlay, Unity config
# === and evidence dir. Verdicts derive ONLY from grep-stable
# === Player.log markers; the raw log stays beside them as truth.

import datetime as _dt
import hashlib
import json
import os
import re
import shutil
import subprocess
import tempfile
import urllib.request
from pathlib import Path


MODS_JSON = REPO_ROOT / "Scripts" / "mods.json"

#: Grep-stable evidence markers (the evidence contract).
HARNESS_MARKERS = re.compile(
    r"PASSED in|FAILED in|passed in|failed in|matched no scenarios|suite runner error")

#: Pickle scenarios are filtered by FILE name including the .feature extension.
DEFAULT_FILTER = "flexverse-smoke.feature,flexevent.feature,tween.feature"

#: Official DLC package ids, in the game's own release order
#: (Verse.ModsConfig.ExpansionsInReleaseOrder). Pre-declared as known so the
#: game never auto-activates them: runs are DLC-free by default.
OFFICIAL_DLC = (
    "ludeon.rimworld.royalty",
    "ludeon.rimworld.ideology",
    "ludeon.rimworld.biotech",
    "ludeon.rimworld.anomaly",
    "ludeon.rimworld.odyssey",
)

_SCENARIO_RE = re.compile(
    r"(?P<status>passed|failed) in \d+ms:\s*(?P<name>.+)$", re.M)
_COLOR_TAG_RE = re.compile(r"<[^>]*>")


def _clean(line: str) -> str:
    return _COLOR_TAG_RE.sub("", line).strip()


# === typed registry and composition ==========================================

def load_registry() -> dict[str, ModPin]:
    doc = json.loads(MODS_JSON.read_text(encoding="utf-8"))
    out: dict[str, ModPin] = {}
    for mod_id, entry in doc["mods"].items():
        out[mod_id] = ModPin(
            id=mod_id,
            source=entry["source"],
            modDir=entry["modDir"],
            repo=entry.get("repo"),
            tag=entry.get("tag"),
            asset=entry.get("asset"),
            sha256=entry.get("sha256"),
            publishedFileId=entry.get("publishedFileId"),
            features=tuple(entry.get("features", ())),
            knownNoise=tuple(entry.get("knownNoise", ())),
            expectedLog=tuple(entry.get("expectedLog", ())),
        )
    return out


def load_base() -> RunComposition:
    doc = json.loads(MODS_JSON.read_text(encoding="utf-8"))
    mods = tuple(doc["base"])
    return RunComposition(
        name="base",
        mods=mods,
        active_mods=("ludeon.rimworld", *mods),
        game_version=doc.get("gameVersion", "1.6"))


def load_play_extras() -> tuple[str, ...]:
    doc = json.loads(MODS_JSON.read_text(encoding="utf-8"))
    return tuple(doc.get("playWith", ()))


def compose(extra: tuple[str, ...]) -> RunComposition:
    """base + extra mods; a --with run (name compat-<id>) diffs vs runs/base."""
    registry = load_registry()
    for mod_id in extra:
        if mod_id not in registry:
            raise ConfigError(f"mod '{mod_id}' not in Scripts/mods.json")
    base = load_base()
    mods = list(base.mods)
    active = list(base.active_mods)
    for mod_id in extra:
        if mod_id not in mods:
            mods.append(mod_id)
        if mod_id not in active:
            active.append(mod_id)
    name = "compat-" + "-".join(extra) if extra else base.name
    return RunComposition(name=name, mods=tuple(mods), active_mods=tuple(active),
                          game_version=base.game_version)


def run_filter(composition: RunComposition, override: str | None = None) -> str:
    """The Pickle filter: a mods.json mod may carry specific features."""
    if override is not None:
        return override
    registry = load_registry()
    features = [f for mid in composition.mods for f in registry[mid].features]
    if len(features) > 1:
        warn(f"multiple features declared {features}; using only the last: "
             f"{features[-1]}")
    return features[-1] if features else DEFAULT_FILTER


# === download cache: each pin fetched exactly once ===========================

def _cache_key(pin: ModPin) -> str:
    if pin.source == "github" and pin.sha256:
        return f"github-{pin.sha256[:16]}"
    if pin.source == "workshop":
        return f"workshop-{pin.publishedFileId}"
    if pin.source == "workspace":
        return f"workspace-{pin.modDir}"
    raise ConfigError(f"unknown mod source '{pin.source}'")


def ensure_cached(pin: ModPin) -> Path:
    """Resolve a pin into the cache: download (github/workshop) or the
    workspace build output."""
    cache = CacheLayout()
    dest = cache.mods / _cache_key(pin) / pin.modDir
    if pin.source == "workspace":
        if dest.is_dir():
            return dest
        built = REPO_ROOT / "Mods" / pin.modDir
        if built.is_dir():
            return built
        raise ConfigError(
            f"workspace mod '{pin.modDir}' is not built — expected {dest} or "
            f"{built} (run Scripts/test or dotnet build first)")

    if dest.exists():
        print(f"  cached {pin.id} -> {dest.parent.name}")
        return dest

    cache.mods.mkdir(parents=True, exist_ok=True)
    if pin.source == "github":
        with tempfile.TemporaryDirectory() as td:
            z = Path(td) / "mod.zip"
            _fetch(_github_release_url(pin), z, pin.sha256)
            _run(["unzip", "-oq", str(z), "-d", td])
            entries = [e for e in os.listdir(td) if e != "mod.zip"]
            if len(entries) != 1:
                raise ConfigError(
                    f"unexpected archive layout for {pin.id}: {entries}")
            shutil.move(str(Path(td) / entries[0]), str(dest))
    elif pin.source == "workshop":
        item = str(pin.publishedFileId)
        workdir = cache.mods / _cache_key(pin)
        workdir.mkdir(parents=True, exist_ok=True)
        _run(["docker", "run", "--rm", "-v", f"{workdir}:/out",
              "cm2network/steamcmd:latest", "bash", "-c",
              f"/home/steam/steamcmd/steamcmd.sh +force_install_dir /out/game "
              f"+login anonymous "
              f"+workshop_download_item 294100 {item} validate +quit "
              f"&& cp -r /out/game/steamapps/workshop/content/294100/{item} "
              f"/out/{pin.modDir}"])
    else:
        raise ConfigError(f"unknown mod source '{pin.source}'")
    print(f"  fetched {pin.id} -> {dest}")
    return dest


def _github_release_url(pin: ModPin) -> str:
    return (f"https://github.com/{pin.repo}/releases/download/"
            f"{pin.tag}/{pin.asset}")


def _fetch(url: str, dest: Path, sha256: str | None = None) -> None:
    tmp = Path(str(dest) + ".part")
    urllib.request.urlretrieve(url, tmp)
    if sha256:
        h = hashlib.sha256(tmp.read_bytes()).hexdigest()
        if h != sha256:
            tmp.unlink(missing_ok=True)
            raise ConfigError(f"sha256 mismatch for {url}: {h} != {sha256}")
    os.replace(tmp, dest)


def _run(cmd: list[str]) -> None:
    print(f"[test] $ {' '.join(cmd)}", flush=True)
    proc = subprocess.run(cmd)
    if proc.returncode != 0:
        raise ConfigError(f"`{' '.join(cmd)}` exited {proc.returncode}")


# === materialization =========================================================

def materialize(composition: RunComposition) -> tuple[ModPin, ...]:
    """Stage Mods/ + unity3d config for one run; returns the staged pins."""
    registry = load_registry()
    cache = CacheLayout()
    mods_dir = cache.run_staging(composition.name) / "Mods"
    mods_dir.mkdir(parents=True, exist_ok=True)
    appdata(composition.name).mkdir(parents=True, exist_ok=True)

    staged: list[ModPin] = []
    for mod_id in composition.mods:
        pin = registry.get(mod_id)
        if pin is None:
            raise ConfigError(f"mod '{mod_id}' missing from mods.json")
        cached = ensure_cached(pin)
        _link_or_copy(cached, mods_dir / pin.modDir)
        staged.append(pin)
        print(f"  staged {mod_id} -> Mods/{pin.modDir}")

    _write_config(composition)
    return tuple(staged)


def _link_or_copy(src: Path, dest: Path) -> None:
    if dest.exists():
        shutil.rmtree(dest)
    try:
        shutil.copytree(src, dest, copy_function=os.link)
    except OSError:  # cross-device or fs without hardlinks
        shutil.rmtree(dest, ignore_errors=True)
        shutil.copytree(src, dest)


def _write_config(composition: RunComposition) -> None:
    active = "".join(f"\n    <li>{m}</li>" for m in composition.active_mods)
    # All official DLCs pre-declared as known: ModsConfig only auto-activates
    # official expansions it considers NEW (absent from knownExpansions,
    # Verse.ModsConfig.CheckModsConfigsCompatVersion); the staging config dir
    # is rebuilt every run, so without this the game re-enables every
    # installed DLC on every boot. Absent from activeMods => disabled.
    known = "".join(f"\n    <li>{m}</li>" for m in OFFICIAL_DLC)
    config_dir = appdata(composition.name)
    (config_dir / "ModsConfig.xml").write_text(f"""<?xml version="1.0" encoding="utf-8"?>
<ModsConfigData>
  <version>{composition.game_version}</version>
  <activeMods>{active}
  </activeMods>
  <knownExpansions>{known}
  </knownExpansions>
</ModsConfigData>
""", encoding="utf-8")
    (config_dir / "Prefs.xml").write_text("""<?xml version="1.0" encoding="utf-8"?>
<PrefsData>
  <screenWidth>1600</screenWidth>
  <screenHeight>900</screenHeight>
  <fullscreen>False</fullscreen>
  <volumeGame>0</volumeGame>
  <volumeMusic>0</volumeMusic>
  <volumeAmbient>0</volumeAmbient>
  <devMode>True</devMode>
  <runInBackground>True</runInBackground>
  <resetModsConfigOnCrash>False</resetModsConfigOnCrash>
</PrefsData>
""", encoding="utf-8")


# === evidence: verdicts from markers only ====================================

def derive_verdict(text: str, exit_code: int) -> Verdict:
    scenarios = tuple(Scenario(name=_clean(m.group("name"))[-160:],
                               status=m.group("status").upper())
                      for m in _SCENARIO_RE.finditer(text))
    harness_ran = bool(HARNESS_MARKERS.search(text))
    fv_markers = sum(1 for ln in text.splitlines() if "[FV][M-" in ln)
    regions = sorted({_clean(ln) for ln in text.splitlines()
                      if "scenario:BEGIN" in ln or "scenario:END" in ln})

    framework_errors, other_errors = [], []
    for ln in text.splitlines():
        if "Exception" in ln or "exception" in ln:
            (framework_errors if "FlexVerse" in ln else other_errors) \
                .append(_clean(ln)[:300])

    failed = tuple(s for s in scenarios if s.status == "FAILED")
    passed = (failed == () and harness_ran and exit_code == 0
              and not framework_errors)
    return Verdict(
        passed=passed,
        exit_code=exit_code,
        harness_ran=harness_ran,
        scenarios=scenarios,
        failed_scenarios=failed,
        fv_verify_markers=fv_markers,
        scenario_regions=tuple(regions),
        framework_errors=tuple(framework_errors[:20]),
        other_errors_count=len(other_errors),
        other_errors_sample=tuple(other_errors[:10]),
    )


def write_report(run_dir: Path, plan: RunPlan, game_info: GameInfo | None,
                 exit_code: int) -> Verdict:
    """Write the single derived report: run identity + typed verdict."""
    log_path = run_dir / "Player.log"
    text = (log_path.read_text(encoding="utf-8", errors="replace")
            if log_path.exists() else "")
    verdict = derive_verdict(text, exit_code)
    report = {
        "schema": "flexverse.report/1",
        "run": {
            "timestamp": _dt.datetime.now().isoformat(timespec="seconds"),
            "run": plan.run_name,
            "game": game_info.as_stamp() if game_info else None,
            "mods": plan.mod_stamps(),
            "filter": plan.filter,
            "seed": plan.seed,
            "exit_code": exit_code,
        },
        "verdict": verdict.as_dict(),
    }
    (run_dir / "report.json").write_text(
        json.dumps(report, indent=2, ensure_ascii=False), encoding="utf-8")
    return verdict


def load_report(run_dir: Path) -> dict:
    return json.loads((Path(run_dir) / "report.json").read_text(
        encoding="utf-8"))


def summarize(verdict: Verdict) -> None:
    print("--- scenario progress ---")
    for s in verdict.scenarios[-10:]:
        print(f"  {s.status:7s} {s.name}")
    print(f"--- FV verify markers: {verdict.fv_verify_markers}")
    if verdict.framework_errors:
        print("--- framework-attributable errors ---")
        for e in verdict.framework_errors[:5]:
            print(f"  {e}")
    if not verdict.harness_ran:
        warn("Pickle harness did not run (no scenario markers); NOT a pass")


def diff_verdict(base_dir: Path, run_dir: Path, plan: RunPlan,
                 registry: dict[str, ModPin]) -> DiffResult:
    """'Compatible with mod X' = same scenario set + same outcomes + no new
    framework errors, after subtracting the mod's declared noise."""
    base = load_report(base_dir)
    run = load_report(run_dir)
    noise = plan.noise_terms(registry)

    def _norm(lines: list[str]) -> set[str]:
        out: set[str] = set()
        for ln in lines:
            ln = re.sub(r"^\[\d+\]", "", ln)          # frame numbers
            ln = re.sub(r"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}", "", ln)
            if not any(n in ln for n in noise):
                out.add(ln)
        return out

    base_v, mod_v = base["verdict"], run["verdict"]
    base_scen = {s["name"] for s in base_v["scenarios"]}
    mod_scen = {s["name"] for s in mod_v["scenarios"]}
    base_fail = {s["name"] for s in base_v["failed_scenarios"]}
    mod_fail = {s["name"] for s in mod_v["failed_scenarios"]}
    new_errors = tuple(sorted(_norm(mod_v["framework_errors"])
                              - _norm(base_v["framework_errors"])))

    result = DiffResult(
        base=str(base_dir),
        run=str(run_dir),
        scenario_sets_equal=base_scen == mod_scen,
        outcomes_equal=base_fail == mod_fail,
        new_framework_errors=new_errors,
        run_harness_ran=mod_v["harness_ran"],
        compatible=(base_scen == mod_scen and base_fail == mod_fail
                    and not new_errors and mod_v["harness_ran"]),
    )
    (Path(run_dir) / "diff.json").write_text(
        json.dumps(result.as_dict(), indent=2, ensure_ascii=False),
        encoding="utf-8")
    return result


def report_exit_code(verdict: Verdict, *, differential: bool,
                     diff: DiffResult | None) -> int:
    """The 0/1/2 contract (S-VERIFICATION.REQ-001), decided in one place."""
    if not verdict.harness_ran:
        raise ToolingError(
            "no scenario markers in Player.log — silence is never green")
    if differential:
        if diff is None:
            raise ToolingError("differential run without a diff result")
        return 0 if diff.compatible else 1
    return 0 if verdict.passed else max(verdict.exit_code, 1)


# === shared operation parts: the game pipeline stages and argv
# === helpers used by Scripts/{{test,play}}. Commands own their
# === composition; this section owns what each stage does.

import shutil
from pathlib import Path
from typing import Callable



# --- argv helpers (the single place syntax is interpreted) -------------------

def require_value(args: list[str], flag: str) -> str:
    if not args:
        die(f"{flag} requires a value")
    return args.pop(0)


def parse_profile(value: str) -> Profile:
    try:
        return Profile.parse(value)
    except ValueError as exc:
        die(str(exc))
        raise  # unreachable; keeps type checkers happy


# --- stages: what one pipeline step does -------------------------------------

def stage_game(cached: bool, spec: RunSpec) -> GameInfo:
    if cached and (info := last_info()) is not None:
        return info
    return SOURCE_ADAPTERS[spec.profile]()


def stage_mods(cached: bool, spec: RunSpec) -> RunPlan:
    composition = compose(spec.with_mods)
    console(f"staging run '{composition.name}'")
    staged_dir = CacheLayout().run_staging(composition.name) / "Mods"
    if cached and staged_dir.is_dir() and any(staged_dir.iterdir()):
        registry = load_registry()
        staged = tuple(registry[m] for m in composition.mods)
        # Cheap-output recompute (PRIN-002): rewrite the Unity config even on
        # a resume — the game saves ModsConfig.xml back into the mount and
        # re-enables every known DLC; a resumed stage must not let that file
        # survive into the next run.
        appdata(composition.name).mkdir(parents=True, exist_ok=True)
        _write_config(composition)
    else:
        staged = materialize(composition)
    return RunPlan(composition=composition, staged=staged,
                   filter=run_filter(composition, spec.feature),
                   seed=spec.seed)


def fresh_run_dir(plan: RunPlan) -> str:
    """Fresh evidence dir + latest pointer (idempotent reset).

    The run dir is always the composition name ('base') — vm sessions share
    it with ordinary play runs; only the container/session id differs."""
    cache = CacheLayout()
    run_dir = cache.run_dir(plan.run_name)
    if run_dir.exists():
        shutil.rmtree(run_dir)
    run_dir.mkdir(parents=True, exist_ok=True)
    if cache.latest.is_symlink() or cache.latest.exists():
        cache.latest.unlink()
    cache.latest.symlink_to(run_dir.name)
    return plan.run_name


def spec_fingerprint(spec: RunSpec) -> Callable[[], str]:
    """Fingerprint of the run-selection inputs (mods, subject, seed) plus the
    source digest: rebuilt assemblies must always re-materialize into staging,
    even when the mod composition itself is unchanged."""
    def fp() -> str:
        return doc_digest({"with": list(spec.with_mods),
                           "feature": spec.feature, "seed": spec.seed,
                           "source": source_fingerprint()})
    return fp


# --- the shared game pipeline (build -> image -> game -> mods -> run dir) ----

def game_pipeline(spec: RunSpec) -> Pipeline:
    """Stages shared by every game-touching command (test pickle/all, play).

    The container run itself is command-owned: `test` adds the verdict plane
    (run_container + evidence), `play` swaps the entrypoint (run_play) and
    writes no report."""
    p = Pipeline()
    p.run("build_product", make_product,
          fingerprint=source_fingerprint,
          description="dotnet build product + test mods (Debug)")
    p.run("build_image", lambda cached: build_image(),
          fingerprint=image_fingerprint,
          description="runtime-deps image (inline definition)")
    p.run("stage_game", lambda cached: stage_game(cached, spec),
          description="game source adapter -> shared install")
    p.run("stage_mods", lambda cached: stage_mods(cached, spec),
          fingerprint=spec_fingerprint(spec),
          description="compose + materialize mods for the run")
    p.run("fresh_run_dir", lambda cached: fresh_run_dir(p.out("stage_mods")),
          description="fresh evidence dir + runs/latest")
    return p


# --- verdict-plane stages (owned by Scripts/test) -----------------------------

def run_container(p: Pipeline, spec: RunSpec) -> int:
    plan: RunPlan = p.out("stage_mods")
    run_dir = CacheLayout().run_dir(plan.run_name)
    console(f"in-game Pickle run '{plan.run_name}' (filter: {plan.filter})")
    return run_test(plan, run_dir)


def evidence(p: Pipeline, spec: RunSpec) -> int:
    plan: RunPlan = p.out("stage_mods")
    game_info: GameInfo = p.out("stage_game")
    rc: int = p.out("run_container")
    run_dir = CacheLayout().run_dir(plan.run_name)
    console(f"game exited with code {rc}")
    if not (run_dir / "Player.log").exists():
        die(f"no Player.log produced ({run_dir})")

    verdict = write_report(run_dir, plan, game_info, rc)
    summarize(verdict)

    diff = None
    if plan.differential:
        base = CacheLayout().run_dir("base")
        if not (base / "report.json").exists():
            die(f"no base run at {base}; run Scripts/test pickle first")
        diff = diff_verdict(base, run_dir, plan, load_registry())
        console(f"compat diff vs {base}: "
                f"{'COMPATIBLE' if diff.compatible else 'INCOMPATIBLE'} "
                f"(details: {run_dir / 'diff.json'})")
    try:
        code = report_exit_code(verdict, differential=plan.differential,
                                     diff=diff)
    except ToolingError as exc:
        die(str(exc), ExitCode.DID_NOT_RUN)
    console(f"evidence: {run_dir} (Player.log, report.json)")
    return code
