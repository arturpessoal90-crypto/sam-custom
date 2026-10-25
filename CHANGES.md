# Custom changes (modified Steam Achievement Manager 7.0.41, zlib license)

* SAM.Game/AchievementToast.cs (new): popup "<your Steam name> has gotten an achievement" (bottom-right, always on top, no focus steal).
  Name is read from Steam's config/loginusers.vdf, falls back to "Me".
* SAM.Game/Manager.cs: popup after every manual Store; new silent `--auto` mode (ticks all non-protected achievements, stores, shows popup, exits).
* SAM.Game/Program.cs: `SAM.Game.exe <appid> --auto`.
* SAM.Picker: new toolbar button "Unlock ALL Games" - runs `--auto` for every game currently shown in the list, one at a time. Click again to stop.

Build: open SAM.sln in Visual Studio 2022 (Release | x86) or run `dotnet build SAM.sln -c Release`. Output goes to ./upload/
