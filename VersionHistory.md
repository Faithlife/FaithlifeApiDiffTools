# Version History

## Pending

Add changes here when they're committed to the `master` branch. Move them to "Released" once the version number
is updated in preparation for publishing an updated NuGet package.

Prefix the description of the change with `[major]`, `[minor]` or `[patch]` in accordance with [SemVer](http://semver.org).

* [minor] Optionally compare packages to pre-release version, defaulting to false.
* [minor] Add `includeInternals` option to ApiDiffTool, defaulting to false. This is a change in the default behavior.
* [patch] Detect additions to interfaces as breaking changes.
* [patch] Fix error when `nuget.config` includes a local directory.

## Released

### 0.1.1

* Suggest appropriate version for starting version below 1.0.0 or with prerelease.

### 0.1.0

* Initial release.
