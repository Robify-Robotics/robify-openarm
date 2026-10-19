#!/usr/bin/env python
# -*- coding: utf-8 -*-

################################################################################
# Copyright 2017 VKROBOT CO., LTD.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
################################################################################

# Author: RWJ (L)

#
# *********     Protocol Combined Example      *********
#
#
# Available siar model on this example : All models using Protocol 1.0 and 2.0
# This example is tested with a siar MX-28, a siar PRO 54-200 and an USB2siar
# Be sure that properties of siar MX and PRO are already set as %% MX - ID : 1 / Baudnum : 34 (Baudrate : 57600) , PRO - ID : 1 / Baudnum : 1 (Baudrate : 57600)
#

# Be aware that:
# This example configures two different control tables (especially, if it uses siar and siar PRO). It may modify critical siar parameter on the control table, if siars have wrong ID.
#

import os

if os.name == 'nt':
    import msvcrt
    def getch():
        return msvcrt.getch().decode()
else:
    import sys, tty, termios
    fd = sys.stdin.fileno()
    old_settings = termios.tcgetattr(fd)
    def getch():
        try:
            tty.setraw(sys.stdin.fileno())
            ch = sys.stdin.read(1)
        finally:
            termios.tcsetattr(fd, termios.TCSADRAIN, old_settings)
        return ch

from siardk import *                    # Uses SIARDK library

# Control table address for siar MX
ADDR_MX_TORQUE_ENABLE       = 64               # Control table address is different in siar model
ADDR_MX_GOAL_POSITION       = 116
ADDR_MX_PRESENT_POSITION    = 132

# Control table address for siar PRO
ADDR_PRO_TORQUE_ENABLE      = 512
ADDR_PRO_GOAL_POSITION      = 532
ADDR_PRO_PRESENT_POSITION   = 132

# Protocol version
PROTOCOL_VERSION1           = 1.0               #  See which protocol version is used in the siar
PROTOCOL_VERSION2           = 2.0

# Default setting
SIAR1_ID                     = 1                 # siar#1 ID : 1
SIAR2_ID                     = 2                 # siar#2 ID : 2
BAUDRATE                    = 57600             # siar default baudrate : 57600
DEVICENAME                  = '/dev/ttyUSB0'    # Check which port is being used on your controller
                                                # ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"

TORQUE_ENABLE               = 1                 # Value for enabling the torque
TORQUE_DISABLE              = 0                 # Value for disabling the torque
SIAR1_MINIMUM_POSITION_VALUE = 100           # siar will rotate between this value
SIAR1_MAXIMUM_POSITION_VALUE = 4000            # and this value (note that the siar would not move when the position value is out of movable range. Check e-manual about the range of the siar you use.)
SIAR2_MINIMUM_POSITION_VALUE = 100 
SIAR2_MAXIMUM_POSITION_VALUE = 4000
SIAR1_MOVING_STATUS_THRESHOLD = 10                # siar MX moving status threshold
SIAR2_MOVING_STATUS_THRESHOLD = 20                # siar PRO moving status threshold

index = 0
siar1_goal_position = [SIAR1_MINIMUM_POSITION_VALUE, SIAR1_MAXIMUM_POSITION_VALUE]         # Goal position of siar MX
siar2_goal_position = [SIAR2_MINIMUM_POSITION_VALUE, SIAR2_MAXIMUM_POSITION_VALUE]         # Goal position of siar PRO

# Initialize PortHandler instance
# Set the port path
# Get methods and members of PortHandlerLinux or PortHandlerWindows
portHandler = PortHandler(DEVICENAME)

# Initialize PacketHandler instance
# Set the protocol version
# Get methods and members of Protocol1PacketHandler or Protocol2PacketHandler
packetHandler1 = PacketHandler(PROTOCOL_VERSION1)
packetHandler2 = PacketHandler(PROTOCOL_VERSION2)


# Open port
if portHandler.openPort():
    print("Succeeded to open the port")
else:
    print("Failed to open the port")
    print("Press any key to terminate...")
    getch()
    quit()


# Set port baudrate
if portHandler.setBaudRate(BAUDRATE):
    print("Succeeded to change the baudrate")
else:
    print("Failed to change the baudrate")
    print("Press any key to terminate...")
    getch()
    quit()

# Enable siar#1 Torque
siar_comm_result, siar_error = packetHandler1.write1ByteTxRx(portHandler, SIAR1_ID, ADDR_MX_TORQUE_ENABLE, TORQUE_ENABLE)
if siar_comm_result != COMM_SUCCESS:
    print("%s" % packetHandler1.getTxRxResult(siar_comm_result))
elif siar_error != 0:
    print("%s" % packetHandler1.getRxPacketError(siar_error))
else:
    print("siar#%d has been successfully connected" % SIAR1_ID)

# Enable siar#2 Torque
siar_comm_result, siar_error = packetHandler2.write1ByteTxRx(portHandler, SIAR2_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_ENABLE)
if siar_comm_result != COMM_SUCCESS:
    print("%s" % packetHandler2.getTxRxResult(siar_comm_result))
elif siar_error != 0:
    print("%s" % packetHandler2.getRxPacketError(siar_error))
else:
    print("siar#%d has been successfully connected" % SIAR2_ID)

while 1:
    print("Press any key to continue! (or press ESC to quit!)")
    if getch() == chr(0x1b):
        break

    # Write siar#1 goal position
    siar_comm_result, siar_error = packetHandler1.write4ByteTxRx(portHandler, SIAR1_ID, ADDR_MX_GOAL_POSITION, siar1_goal_position[index])
    if siar_comm_result != COMM_SUCCESS:
        print("%s" % packetHandler1.getTxRxResult(siar_comm_result))
    elif siar_error != 0:
        print("%s" % packetHandler1.getRxPacketError(siar_error))

    # Write siar#2 goal position
    siar_comm_result, siar_error = packetHandler2.write4ByteTxRx(portHandler, SIAR2_ID, ADDR_PRO_GOAL_POSITION, siar2_goal_position[index])
    if siar_comm_result != COMM_SUCCESS:
        print("%s" % packetHandler2.getTxRxResult(siar_comm_result))
    elif siar_error != 0:
        print("%s" % packetHandler2.getRxPacketError(siar_error))


    while 1:
        # Read siar#1 present position
        siar1_present_position, siar_comm_result, siar_error = packetHandler1.read4ByteTxRx(portHandler, SIAR1_ID, ADDR_MX_PRESENT_POSITION)
        if siar_comm_result != COMM_SUCCESS:
            print("%s" % packetHandler1.getTxRxResult(siar_comm_result))
        elif siar_error != 0:
            print("%s" % packetHandler1.getRxPacketError(siar_error))

        # Read siar#2 present position
        siar2_present_position, siar_comm_result, siar_error = packetHandler2.read4ByteTxRx(portHandler, SIAR2_ID, ADDR_PRO_PRESENT_POSITION)
        if siar_comm_result != COMM_SUCCESS:
            print("%s" % packetHandler2.getTxRxResult(siar_comm_result))
        elif siar_error != 0:
            print("%s" % packetHandler2.getRxPacketError(siar_error))

        print("[ID:%03d] GoalPos:%03d  PresPos:%03d\t[ID:%03d] GoalPos:%03d  PresPos:%03d" % (SIAR1_ID, siar1_goal_position[index], siar1_present_position, SIAR2_ID, siar2_goal_position[index], siar2_present_position))

        if not ((abs(siar1_goal_position[index] - siar1_present_position) > SIAR1_MOVING_STATUS_THRESHOLD) and (abs(siar2_goal_position[index] - siar2_present_position) > SIAR2_MOVING_STATUS_THRESHOLD)):
            break

    # Change goal position
    if index == 0:
        index = 1
    else:
        index = 0    


# Disable siar#1 Torque
siar_comm_result, siar_error = packetHandler1.write1ByteTxRx(portHandler, SIAR1_ID, ADDR_MX_TORQUE_ENABLE, TORQUE_DISABLE)
if siar_comm_result != COMM_SUCCESS:
    print("%s" % packetHandler1.getTxRxResult(siar_comm_result))
elif siar_error != 0:
    print("%s" % packetHandler1.getRxPacketError(siar_error))

# Disable siar#2 Torque
siar_comm_result, siar_error = packetHandler2.write1ByteTxRx(portHandler, SIAR2_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_DISABLE)
if siar_comm_result != COMM_SUCCESS:
    print("%s" % packetHandler2.getTxRxResult(siar_comm_result))
elif siar_error != 0:
    print("%s" % packetHandler2.getRxPacketError(siar_error))

# Close port
portHandler.closePort()
