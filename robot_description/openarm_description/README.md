# OpenArm 1.1

URDF robot model with 22 links, 21 joints, and 18 STL mesh files.

## Files

```text
Openarm1.1.urdf
meshes/
```

## Usage

Download the URDF and `meshes/` directory together, keeping this directory structure.

Mesh paths are relative to the URDF directory. Configure your loader to resolve paths from this directory, or adapt them to your application's resource URI format. RViz requires resource URI configuration to locate the meshes.

The URDF is not tied to a specific ROS version. Mesh scale is set to `0.001 0.001 0.001` to convert STL dimensions from millimeters to meters.
