# OpenArm 1.1

This directory contains the corrected robot model and its 18 STL meshes:

```text
Openarm1.1.urdf
meshes/
```

Keep the URDF and `meshes` directory together. Mesh paths are relative to the URDF directory. Configure your loader to resolve those paths from this directory, or adapt the paths to your application. Some tools, including RViz, require resource URI configuration rather than resolving bare relative mesh paths automatically.

The model does not require a specific ROS version. No launch files, ROS package manifests, build files or RViz configuration are included.

The missing `part.stl` reference is corrected to `jaw-b.STL`. All 41 visual and collision mesh references use `scale="0.001 0.001 0.001"` to convert millimeter geometry to meters. Joint origins, limits, axes and inertial parameters are unchanged.

The model contains 22 links and 21 joints. Inertial parameters have not been validated for dynamics. The supplier did not include a license declaration in the original archive; see the repository's third-party notices.

Validated with the ROS-independent yourdfpy loader: all 21 visual and 20 collision mesh instances loaded, meter-scale bounds were checked, and all 18 movable joints were exercised at two configurations within the supplied limits. This checks loading and transforms, not physical collision clearance or dynamic accuracy.
