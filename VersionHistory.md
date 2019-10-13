# Version History

## Pending

Add changes here when they're committed to the `master` branch. Move them to "Released" once the version number
is updated in preparation for publishing an updated NuGet package.

Prefix the description of the change with `[major]`, `[minor]` or `[patch]` in accordance with [SemVer](http://semver.org).

* [patch] Wait for package extraction to complete.
* [patch] Update to Nuget.Protocol 5.3.
* [patch] Fix comparing package with same version as previous comparison.

## Released

### 0.2.1

* [patch] If 1.2.3 is suggested, 1.2.3-xyz should be accepted.

### 0.2.0

* [minor] Optionally compare packages to pre-release version, defaulting to false.
* [minor] Add `includeInternals` option to ApiDiffTool, defaulting to false. This is a change in the default behavior.
* [patch] Detect additions to interfaces as breaking changes.
* [patch] Fix error when `nuget.config` includes a local directory.

### 0.1.1

* Suggest appropriate version for starting version below 1.0.0 or with prerelease.

### 0.1.0

* Initial release.
