# Third-party components

## OpenArm 1.1 model

- Source: user-supplied `Openarm1.1.zip`.
- Original archive SHA-256: `e2c46df9b86d3ac14b98ab671029da9f7ce1d7c9e2eb0992d2ef0f1a0822d3d6`.
- No license file or license declaration was included by the model supplier.
- Changes: missing `part.stl` reference corrected to `jaw-b.STL`; mesh paths converted to ROS package URIs; all 41 mesh references scaled by 0.001; ROS 2 package, RViz launch and documentation added.
- The STL files are unchanged from the supplied archive. Joint and inertial parameters are unchanged.

This replaces the previously included Enactic OpenArm description package (upstream commit `206eb8a4cf2e5e564536b4f5366ef369843a4aa7`). That older package remains available in Git history with its original Apache-2.0 notices; its license is not asserted over the replacement model.

## SIAR SDK 1.0.0

- Upstream: https://gitee.com/WEIKONGROBOT/siar
- Upstream directory: `6.SDK/siardk-1.0.0`
- Commit: `387fe7749d83794437f5f523f70b0665fc5bf041`
- License notices: retain and review the notices in the individual files and [`sdk/siardk-1.0.0/python/LICENSE.txt`](sdk/siardk-1.0.0/python/LICENSE.txt).
- Changes: `.vs` and `.vscode` development cache/settings directories were omitted. Retained upstream files are unchanged.

No single repository-wide license is asserted over the combined third-party materials. This collection does not change their respective licensing terms or imply endorsement by their upstream authors.

## Consolidated SDK reference

The English PDF compiles the four SIAR 485/TTL SDK documentation chapters. Chinese passages were translated to English, file-list illustrations were rebuilt as English tables, and formatting was unified. API identifiers and technical examples are retained. The SDK download link points to this repository.

Collection date: 19 October 2026.
