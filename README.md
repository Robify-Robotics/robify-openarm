# Robify OpenArm

OpenArm robot descriptions, the SIAR 485/TTL communication SDK, and an English SDK reference manual in one repository.

## Contents

| Directory | Contents |
| --- | --- |
| [`sdk/siardk-1.0.0`](sdk/siardk-1.0.0) | SIAR SDK source, examples and supplied build files for C, C++, C#, Java, LabVIEW, MATLAB, Python and ROS |
| [`robot_description/openarm_description`](robot_description/openarm_description) | Corrected OpenArm 1.1 URDF, 18 STL meshes and a ROS 2 Humble RViz launch file |
| [`docs/01-485_TTL_Communicate_SDK_Complete.pdf`](docs/01-485_TTL_Communicate_SDK_Complete.pdf) | Consolidated English reference covering SDK download, C, C++, ROS and Python APIs |

To download everything, select **Code > Download ZIP**, or run:

```bash
git clone https://github.com/Robify-Robotics/robify-openarm.git
```

## OpenArm 1.1

The previous v1.0/v2.0 descriptions have been replaced by the corrected user-supplied model:

```text
robot_description/openarm_description/urdf/Openarm1.1.urdf
```

The package fixes the missing jaw mesh reference, uses portable ROS package URIs, and converts STL dimensions from millimeters to meters. See the [model README](robot_description/openarm_description/README.md) for details.

With ROS 2 Humble and the package dependencies installed, run from the repository root:

```bash
source /opt/ros/humble/setup.bash
colcon build --packages-select openarm_description
source install/setup.bash
ros2 launch openarm_description display_openarm.launch.py
```

The `sdk/COLCON_IGNORE` marker keeps SDK examples outside ROS 2 package discovery.

## SIAR SDK

Browse [`sdk/siardk-1.0.0`](sdk/siardk-1.0.0) for language-specific implementations, examples and build files. Visual Studio and VS Code cache directories (`.vs` and `.vscode`) were omitted from this copy. The retained upstream files are unchanged.

Collecting these resources does not establish hardware compatibility between OpenArm and SIAR motors. Joint mapping, communication configuration and hardware driver integration must be implemented for the target hardware. The replacement OpenArm 1.1 package provides visualization only; it includes no hardware control plugin.

## Validation

- The corrected URDF was checked for 22 links, 21 joints and 41 valid mesh references across 18 STL files.
- All visual and collision meshes use a 0.001 scale; all other supplied model parameters are preserved.
- The corrected model was tested with ROS 2 Humble and RViz on Ubuntu 22.04. Mesh-loading errors were eliminated.
- The English SDK PDF was checked for content completeness and rendered for layout review.
- Physical motor operation and Gazebo dynamics have not been tested.

## Licensing and upstream versions

See [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md). Each component retains its own license and copyright notices.
