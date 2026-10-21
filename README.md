# Robify OpenArm

OpenArm URDF model, SIAR 485/TTL communication SDK, and English SDK reference manual.

## Downloads

| Resource | Description |
| --- | --- |
| [OpenArm 1.1 model](robot_description/openarm_description) | URDF and 18 STL mesh files |
| [SIAR SDK](sdk/siardk-1.0.0) | Source code, examples, and build files for C, C++, C#, Java, LabVIEW, MATLAB, Python, and ROS |
| [English SDK manual](docs/01-485_TTL_Communicate_SDK_Complete.pdf) | SDK download, C, C++, ROS, and Python API reference |

To download all resources, select **Code > Download ZIP** or run:

```bash
git clone https://github.com/Robify-Robotics/robify-openarm.git
```

## OpenArm 1.1

The URDF file is located at:

```text
robot_description/openarm_description/Openarm1.1.urdf
```

Download the URDF together with the adjacent `meshes/` directory. Mesh paths are relative to the URDF directory. Configure your application to resolve these paths, or adapt them to its resource URI format.

The model is not tied to a specific ROS version. See the [model README](robot_description/openarm_description/README.md) for details.

## SIAR SDK

Open the [SDK directory](sdk/siardk-1.0.0) for language-specific code and examples. See the [English manual](docs/01-485_TTL_Communicate_SDK_Complete.pdf) for communication and API usage.

Configure joint mapping, communication settings, and motor drivers for your hardware.

## Licenses

See [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) and the license notices included with each component.
