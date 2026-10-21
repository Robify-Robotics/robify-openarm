# OpenArm 1.1 description

This package replaces the previous upstream v1.0/v2.0 description with the user-supplied OpenArm 1.1 model.

## Corrections

- Replaced the missing `meshes/part.stl` visual reference with `meshes/jaw-b.STL`.
- Replaced relative mesh paths with portable `package://openarm_description/meshes/...` URIs.
- Added `scale="0.001 0.001 0.001"` to all 41 visual and collision mesh references to convert millimeter geometry to meters.
- Preserved all joint origins, limits, axes, link names, material values and inertial parameters from the supplied model.

The model contains 22 links, 21 joints and 18 STL files. The entry point is `urdf/Openarm1.1.urdf`.

## ROS 2 Humble and RViz

Install the ROS 2 Humble packages `ament-cmake`, `robot-state-publisher`, `joint-state-publisher-gui` and `rviz2`, and install `colcon` if needed. From the repository root:

```bash
source /opt/ros/humble/setup.bash
colcon build --packages-select openarm_description
source install/setup.bash
ros2 launch openarm_description display_openarm.launch.py
```

Keep the package name and directory structure intact. Build and source the workspace before opening the URDF so ROS can resolve its package URIs.

## Scope

The launch file displays the robot and provides joint sliders. It does not connect to motors. Inertial parameters are inherited from the supplied export and have not been validated for dynamics. There is no Gazebo integration or hardware control configuration in this package. ROS 1 launch support is not included.

## Model provenance

The model and meshes were supplied as `Openarm1.1.zip`. The model supplier did not include a license declaration in that archive. The previous Enactic package's Apache license is not asserted over this replacement model. See the repository's third-party notices.
