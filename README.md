# Robify OpenArm

OpenArm robot descriptions, the SIAR 485/TTL communication SDK, and an English SDK reference manual in one repository.

## Contents

| Directory | Contents |
| --- | --- |
| [`sdk/siardk-1.0.0`](sdk/siardk-1.0.0) | SIAR SDK source, examples and supplied build files for C, C++, C#, Java, LabVIEW, MATLAB, Python and ROS |
| [`robot_description/openarm_description`](robot_description/openarm_description) | Corrected OpenArm 1.1 URDF and 18 STL meshes |
| [`docs/01-485_TTL_Communicate_SDK_Complete.pdf`](docs/01-485_TTL_Communicate_SDK_Complete.pdf) | Consolidated English reference covering SDK download, C, C++, ROS and Python APIs |

To download everything, select **Code > Download ZIP**, or run:

```bash
git clone https://github.com/Robify-Robotics/robify-openarm.git
```

## OpenArm 1.1

The corrected model is available at:

```text
robot_description/openarm_description/Openarm1.1.urdf
```

Download the URDF together with its adjacent `meshes/` directory. Mesh paths are relative to the URDF directory; configure your loader accordingly. Some loaders require adapting these paths to their resource URI conventions.

The files do not require a specific ROS version. The missing jaw reference and millimeter-to-meter mesh scale have been corrected. See the [model README](robot_description/openarm_description/README.md) for details.

## SIAR SDK

Browse [`sdk/siardk-1.0.0`](sdk/siardk-1.0.0) for language-specific implementations, examples and build files. Visual Studio and VS Code cache directories (`.vs` and `.vscode`) were omitted from this copy. The retained upstream files are unchanged.

Collecting these resources does not establish hardware compatibility between OpenArm and SIAR motors. Joint mapping, communication configuration and hardware driver integration must be implemented for the target hardware. The OpenArm 1.1 files contain the robot description and geometry only.

## Validation

- The corrected URDF was checked for 22 links, 21 joints and 41 valid mesh references across 18 STL files.
- All visual and collision meshes use a 0.001 scale; all other supplied model parameters are preserved.
- The final minimal URDF was loaded with the ROS-independent yourdfpy loader: all 21 visual and 20 collision mesh instances loaded, meter-scale bounds were checked, and all 18 movable joints were exercised at two configurations. Other loaders may require adapting resource paths.
- The English SDK PDF was checked for content completeness and rendered for layout review.
- Physical motor operation and Gazebo dynamics have not been tested.

## Licensing and upstream versions

See [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md). Each component retains its own license and copyright notices.
