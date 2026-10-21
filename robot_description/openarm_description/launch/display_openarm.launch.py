from pathlib import Path

from ament_index_python.packages import get_package_share_directory
from launch import LaunchDescription
from launch_ros.actions import Node


def generate_launch_description():
    share = Path(get_package_share_directory('openarm_description'))
    model = share / 'urdf' / 'Openarm1.1.urdf'
    return LaunchDescription([
        Node(package='robot_state_publisher', executable='robot_state_publisher',
             parameters=[{'robot_description': model.read_text()}], output='screen'),
        Node(package='joint_state_publisher_gui', executable='joint_state_publisher_gui',
             arguments=[str(model)], output='screen'),
        Node(package='rviz2', executable='rviz2',
             arguments=['-d', str(share / 'rviz' / 'openarm_v11.rviz')], output='screen'),
    ])
