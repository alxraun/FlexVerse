# Third-Party Notices

Third-party components used by FlexVerse, with the licenses they require us
to reproduce. FlexVerse itself is released under the license in `LICENSE`.

## Vendored source code (compiled into our assemblies)

### Flexbox layout engine

- Source: https://github.com/ru-ace/Flexbox (vendored, adapted, under
  `Source/FlexVerse/Flexbox/`)
- A pure C# port of Facebook's Yoga flexbox layout engine; upstream lineage:
  Facebook Yoga → endlesstravel (LoveYoga) → ace (ru-ace).
- License: **BSD 3-Clause**

------ BEGIN LICENSE: Flexbox (BSD 3-Clause) ------
BSD License

For yoga software

Copyright (c) 2014-present, Facebook, Inc. All rights reserved.
Copyright (c) 2017-present, Krzysztof Kowalczyk. All rights reserved.
Copyright (c) 2019, endlesstravel (https://github.com/endlesstravel)
Copyright (c) 2019, ace (https://github.com/ru-ace)


Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

 * Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

 * Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

 * Neither the name Facebook nor the names of its contributors may be used to
   endorse or promote products derived from this software without specific
   prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
------ END LICENSE: Flexbox (BSD 3-Clause) ------

## Build-time NuGet packages (not distributed as part of our binaries)

| Package | Version | License | Role |
|---|---|---|---|
| Fody | 6.9.2 | MIT | IL weaving (property change notifications) |
| FodyHelpers | 6.9.2 | MIT | weaver API for our PropertyWeaver |
| Microsoft.CodeAnalysis.CSharp (Roslyn) | 4.8.0 | MIT | source generators |
| Krafs.Rimworld.Ref | 1.6.4871 | MIT | compile-time game API reference (reference-only, never shipped) |
| Lib.Harmony.Ref | latest | MIT | compile-time Harmony API reference |
| CSharpier.MSBuild | 1.3.0 | MIT | code formatting |
| xunit / xunit.runner.visualstudio / Microsoft.NET.Test.Sdk | — | Apache-2.0 / MIT | offline tests only |

## Runtime mod dependencies (installed by the user, not distributed by us)

| Component | Source | License |
|---|---|---|
| Harmony | https://github.com/pardeike/Harmony (workshop: brrainz.harmony) | MIT (c) Andreas Pardeike |
| Pickle | https://github.com/RimWorks/Rimworld-Pickle | MIT (c) Aaron Scherer |
| RimLogging | https://github.com/RimWorks/rimworld-logging-framework | MIT (c) CryptikLemur |

These are fetched at verification time by `Scripts/_lib.py` from the pins
in `Scripts/mods.json` and never committed to this repository. If a release
artifact ever bundles their assemblies, their LICENSE files must be included
alongside.

## Tooling infrastructure (development/CI environment only)

- RimWorld by Ludeon Studios: used, never redistributed; verification runs
  require the user to own the game.
- steamcmd (`cm2network/steamcmd` image), docker, `ubuntu:24.04` base image:
  used to provision and run the game in verification containers.
