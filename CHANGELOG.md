# Changelog

## [v1.4.5](https://github.com/coldphysics/experiment-control/tree/v1.4.5) (2023-08-14)

[Full Changelog](https://github.com/coldphysics/experiment-control/compare/v1.4.5...HEAD)

**Implemented enhancements:**

- Various small UI improvements [\#54](https://github.com/coldphysics/experiment-control/issues/54)

**Fixed bugs:**

- Missing global counters [\#44](https://github.com/coldphysics/experiment-control/issues/44)
- Iterator variable not iterated correctly: Keeps the Start value and claims Number of Iterations = 1 [\#42](https://github.com/coldphysics/experiment-control/issues/42)
- Iterators that are restored from static variables do not work without additional clicking around [\#38](https://github.com/coldphysics/experiment-control/issues/38)
- Analog card window: Clicking Settings button spawns new Settings button [\#47](https://github.com/coldphysics/experiment-control/issues/47)
- Message dialog is taller than the screen [\#50](https://github.com/coldphysics/experiment-control/issues/50)

## [v1.4.4](https://github.com/coldphysics/experiment-control/tree/v1.4.4) (2020-11-17)

[Full Changelog](https://github.com/coldphysics/experiment-control/compare/v1.4.3-preview...v1.4.4)

**Fixed bugs:**

- Changing variable from static to iterator fails [\#38](https://github.com/coldphysics/experiment-control/issues/38)

**Merged pull requests:**

- Fix/issue 038 - Visual Appearance of Variables not Updating after Changing Their Type [\#40](https://github.com/coldphysics/experiment-control/pull/40) ([ghareeb-falazi](https://github.com/ghareeb-falazi))

## [v1.4.3-preview](https://github.com/coldphysics/experiment-control/tree/v1.4.3-preview) (2020-09-30)

[Full Changelog](https://github.com/coldphysics/experiment-control/compare/v1.4.2...v1.4.3-preview)

**Implemented enhancements:**

- Upgrade NuGet Packages [\#34](https://github.com/coldphysics/experiment-control/issues/34)
- Make Errors Project Follow MVVM [\#31](https://github.com/coldphysics/experiment-control/issues/31)
- Use Strict MVVM [\#29](https://github.com/coldphysics/experiment-control/issues/29)

**Fixed bugs:**

- Application validation did not succeed. Version 1.4.2  [\#32](https://github.com/coldphysics/experiment-control/issues/32)

**Merged pull requests:**

- Upgrade NuGet packages [\#35](https://github.com/coldphysics/experiment-control/pull/35) ([ghareeb-falazi](https://github.com/ghareeb-falazi))
- Make Error Window Follow the MVVM Pattern [\#33](https://github.com/coldphysics/experiment-control/pull/33) ([ghareeb-falazi](https://github.com/ghareeb-falazi))
- Apply MVVM to the View and Controller Projects [\#30](https://github.com/coldphysics/experiment-control/pull/30) ([ghareeb-falazi](https://github.com/ghareeb-falazi))

## [v1.4.2](https://github.com/coldphysics/experiment-control/tree/v1.4.2) (2020-09-22)

[Full Changelog](https://github.com/coldphysics/experiment-control/compare/v1.4.1...v1.4.2)

**Implemented enhancements:**

- Load advanced mode python scripts from file [\#22](https://github.com/coldphysics/experiment-control/issues/22)
- Export Model Output as CSV [\#27](https://github.com/coldphysics/experiment-control/pull/27) ([ghareeb-falazi](https://github.com/ghareeb-falazi))

**Fixed bugs:**

- New Measurement Routines are not Saving Properly [\#23](https://github.com/coldphysics/experiment-control/issues/23)

**Merged pull requests:**

- Feature/issue-022 [\#25](https://github.com/coldphysics/experiment-control/pull/25) ([ghareeb-falazi](https://github.com/ghareeb-falazi))
- Fix/issue 023 [\#24](https://github.com/coldphysics/experiment-control/pull/24) ([ghareeb-falazi](https://github.com/ghareeb-falazi))

## [v1.4.1](https://github.com/coldphysics/experiment-control/tree/v1.4.1) (2020-07-09)

[Full Changelog](https://github.com/coldphysics/experiment-control/compare/v1.4.0...v1.4.1)

**Implemented enhancements:**

- Add a No-Output Hardware Option [\#15](https://github.com/coldphysics/experiment-control/issues/15)

**Fixed bugs:**

- All windows open when changing sequence [\#20](https://github.com/coldphysics/experiment-control/issues/20)
- Output Replication not Working [\#17](https://github.com/coldphysics/experiment-control/issues/17)
- Error window appearance [\#12](https://github.com/coldphysics/experiment-control/issues/12)
- Linear Ramp at the beginning of a new tab [\#3](https://github.com/coldphysics/experiment-control/issues/3)

**Merged pull requests:**

- Fix Output Replication Bug [\#18](https://github.com/coldphysics/experiment-control/pull/18) ([ghareeb-falazi](https://github.com/ghareeb-falazi))
- Add NoOutput Hardware and Enhance Output Visualizer [\#16](https://github.com/coldphysics/experiment-control/pull/16) ([ghareeb-falazi](https://github.com/ghareeb-falazi))
- Force Errors window to be on top when starting the experiment [\#14](https://github.com/coldphysics/experiment-control/pull/14) ([ghareeb-falazi](https://github.com/ghareeb-falazi))

## [v1.4.0](https://github.com/coldphysics/experiment-control/tree/v1.4.0) (2020-05-03)

[Full Changelog](https://github.com/coldphysics/experiment-control/compare/657bb41db478977bbf88f37607bcba1973362452...v1.4.0)

**Implemented enhancements:**

- Less clicking to move entries in analog and digital boxes and variable view [\#5](https://github.com/coldphysics/experiment-control/issues/5)

**Fixed bugs:**

- Linear ramp with deactived tab inbetween [\#4](https://github.com/coldphysics/experiment-control/issues/4)
- Global counter mismatch  [\#2](https://github.com/coldphysics/experiment-control/issues/2)
- Stop after variable moved from iterator non-zero stepsize [\#1](https://github.com/coldphysics/experiment-control/issues/1)

**Merged pull requests:**

- Use arrows to quickly move variables and steps [\#11](https://github.com/coldphysics/experiment-control/pull/11) ([ghareeb-falazi](https://github.com/ghareeb-falazi))
- Fix/issue 002 gc mismatch [\#10](https://github.com/coldphysics/experiment-control/pull/10) ([ghareeb-falazi](https://github.com/ghareeb-falazi))
- Disregard disabled sequences when looking for the previous step [\#9](https://github.com/coldphysics/experiment-control/pull/9) ([ghareeb-falazi](https://github.com/ghareeb-falazi))
- Featue/allow changing visualized samples count [\#8](https://github.com/coldphysics/experiment-control/pull/8) ([ghareeb-falazi](https://github.com/ghareeb-falazi))
- Fix issue with changing iterator type [\#7](https://github.com/coldphysics/experiment-control/pull/7) ([ghareeb-falazi](https://github.com/ghareeb-falazi))
- Use NuGets-based MySQL Connector dll [\#6](https://github.com/coldphysics/experiment-control/pull/6) ([ghareeb-falazi](https://github.com/ghareeb-falazi))



\* *This Changelog was automatically generated by [github_changelog_generator](https://github.com/github-changelog-generator/github-changelog-generator)*
