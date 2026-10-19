# Robify OpenArm

OpenArm robot descriptions, the SIAR 485/TTL communication SDK, and an English SDK reference manual in one repository.

## Contents

| Directory | Contents |
| --- | --- |
| [`sdk/siardk-1.0.0`](sdk/siardk-1.0.0) | SIAR SDK source, examples and supplied build files for C, C++, C#, Java, LabVIEW, MATLAB, Python and ROS |
| [`robot_description/openarm_description`](robot_description/openarm_description) | Official OpenArm description package, including v1.0 and v2.0 models, meshes, configuration and launch resources |
| [`docs/01-485_TTL_Communicate_SDK_Complete.pdf`](docs/01-485_TTL_Communicate_SDK_Complete.pdf) | Consolidated English reference covering SDK download, C, C++, ROS and Python APIs |

To download everything, select **Code > Download ZIP**, or run:

```bash
git clone https://github.com/Robify-Robotics/robify-openarm.git
```

## OpenArm v1.0

The Xacro entry point is:

```text
robot_description/openarm_description/assets/robot/openarm_v1.0/urdf/openarm_v10.urdf.xacro
```

The expanded upstream example is:

```text
robot_description/openarm_description/assets/robot/openarm_v1.0/urdf/example/v1.urdf
```

Keep the ROS package name `openarm_description` and its internal directory structure. The model uses package-relative mesh and configuration paths. The v1 example contains two seven-joint arms and grippers.

In a ROS 2 workspace with the package dependencies installed:

```bash
colcon build --packages-select openarm_description
source install/setup.bash
ros2 launch openarm_description display_openarm.launch.py arm_type:=v10
```

The launch file defaults to a v2 model, so specify `arm_type:=v10` for the v1 model. The `sdk/COLCON_IGNORE` marker keeps the SDK's ROS examples outside ROS 2 package discovery.

## SIAR SDK

Browse [`sdk/siardk-1.0.0`](sdk/siardk-1.0.0) for language-specific implementations, examples and build files. Visual Studio and VS Code cache directories (`.vs` and `.vscode`) were omitted from this copy. The retained upstream files are unchanged.

Collecting these resources does not establish hardware compatibility between OpenArm and SIAR motors. Joint mapping, communication configuration and hardware driver integration must be implemented for the target hardware. OpenArm's optional control definitions refer to `openarm_hardware/OpenArmHW`.

## Validation

- Downloaded files were checked against the upstream Git objects.
- OpenArm XML/Xacro files were parsed, and the v1 example's link/joint structure and all 22 unique mesh references were checked.
- The English PDF was checked for content completeness and rendered for layout review.
- ROS 2/RViz execution and physical motor operation have not been tested.

## Licensing and upstream versions

See [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md). Each component retains its own license and copyright notices.
