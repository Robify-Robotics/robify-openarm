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
# *********     Bulk Read and Bulk Write Example      *********
#
#
# Available siar model on this example : All models using Protocol 2.0
# This example is tested with two siar PRO 54-200, and an USB2siar
# Be sure that siar PRO properties are already set as %% ID : 1 and 2 / Baudnum : 1 (Baudrate : 57600)
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

# Control table address
ADDR_PRO_TORQUE_ENABLE      = 512              # Control table address is different in siar model
ADDR_PRO_LED_RED            = 65
ADDR_PRO_GOAL_POSITION      = 532
ADDR_PRO_PRESENT_POSITION   = 132

# Data Byte Length
LEN_PRO_LED_RED             = 1
LEN_PRO_GOAL_POSITION       = 4
LEN_PRO_PRESENT_POSITION    = 4

# Protocol version
PROTOCOL_VERSION            = 2.0               # See which protocol version is used in the siar

# Default setting
SIAR1_ID                     = 1                 # siar#1 ID : 1
SIAR2_ID                     = 2                 # siar#1 ID : 2
BAUDRATE                    = 57600             # siar default baudrate : 57600
DEVICENAME                  = '/dev/ttyUSB0'    # Check which port is being used on your controller
                                                # ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"

TORQUE_ENABLE               = 1                 # Value for enabling the torque
TORQUE_DISABLE              = 0                 # Value for disabling the torque
SIAR_MINIMUM_POSITION_VALUE  = 100           # siar will rotate between this value
SIAR_MAXIMUM_POSITION_VALUE  = 4000            # and this value (note that the siar would not move when the position value is out of movable range. Check e-manual about the range of the siar you use.)
SIAR_MOVING_STATUS_THRESHOLD = 20                # siar moving status threshold

index = 0
siar_goal_position = [SIAR_MINIMUM_POSITION_VALUE, SIAR_MAXIMUM_POSITION_VALUE]        # Goal position
siar_led_value = [0x00, 0x01]                                                        # siar LED value for write

# Initialize PortHandler instance
# Set the port path
# Get methods and members of PortHandlerLinux or PortHandlerWindows
portHandler = PortHandler(DEVICENAME)

# Initialize PacketHandler instance
# Set the protocol version
# Get methods and members of Protocol1PacketHandler or Protocol2PacketHandler
packetHandler = PacketHandler(PROTOCOL_VERSION)

# Initialize GroupBulkWrite instance
groupBulkWrite = GroupBulkWrite(portHandler, packetHandler)

# Initialize GroupBulkRead instace for Present Position
groupBulkRead = GroupBulkRead(portHandler, packetHandler)

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
siar_comm_result, siar_error = packetHandler.write1ByteTxRx(portHandler, SIAR1_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_ENABLE)
if siar_comm_result != COMM_SUCCESS:
    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
elif siar_error != 0:
    print("%s" % packetHandler.getRxPacketError(siar_error))
else:
    print("siar#%d has been successfully connected" % SIAR1_ID)

# Enable siar#2 Torque
siar_comm_result, siar_error = packetHandler.write1ByteTxRx(portHandler, SIAR2_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_ENABLE)
if siar_comm_result != COMM_SUCCESS:
    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
elif siar_error != 0:
    print("%s" % packetHandler.getRxPacketError(siar_error))
else:
    print("siar#%d has been successfully connected" % SIAR2_ID)

# Add parameter storage for siar#1 present position
siar_addparam_result = groupBulkRead.addParam(SIAR1_ID, ADDR_PRO_PRESENT_POSITION, LEN_PRO_PRESENT_POSITION)
if siar_addparam_result != True:
    print("[ID:%03d] groupBulkRead addparam failed" % SIAR1_ID)
    quit()

# Add parameter storage for siar#2 LED value
siar_addparam_result = groupBulkRead.addParam(SIAR2_ID, ADDR_PRO_LED_RED, LEN_PRO_LED_RED)
if siar_addparam_result != True:
    print("[ID:%03d] groupBulkRead addparam failed" % SIAR2_ID)
    quit()

while 1:
    print("Press any key to continue! (or press ESC to quit!)")
    if getch() == chr(0x1b):
        break

    # Allocate goal position value into byte array
    param_goal_position = [SIAR_LOBYTE(SIAR_LOWORD(siar_goal_position[index])), SIAR_HIBYTE(SIAR_LOWORD(siar_goal_position[index])), SIAR_LOBYTE(SIAR_HIWORD(siar_goal_position[index])), SIAR_HIBYTE(SIAR_HIWORD(siar_goal_position[index]))]

    # Add siar#1 goal position value to the Bulkwrite parameter storage
    siar_addparam_result = groupBulkWrite.addParam(SIAR1_ID, ADDR_PRO_GOAL_POSITION, LEN_PRO_GOAL_POSITION, param_goal_position)
    if siar_addparam_result != True:
        print("[ID:%03d] groupBulkWrite addparam failed" % SIAR1_ID)
        quit()

    # Add siar#2 LED value to the Bulkwrite parameter storage
    siar_addparam_result = groupBulkWrite.addParam(SIAR2_ID, ADDR_PRO_LED_RED, LEN_PRO_LED_RED, [siar_led_value[index]])
    if siar_addparam_result != True:
        print("[ID:%03d] groupBulkWrite addparam failed" % SIAR2_ID)
        quit()

    # Bulkwrite goal position and LED value
    siar_comm_result = groupBulkWrite.txPacket()
    if siar_comm_result != COMM_SUCCESS:
        print("%s" % packetHandler.getTxRxResult(siar_comm_result))

    # Clear bulkwrite parameter storage
    groupBulkWrite.clearParam()

    while 1:
        # Bulkread present position and LED status
        siar_comm_result = groupBulkRead.txRxPacket()
        if siar_comm_result != COMM_SUCCESS:
            print("%s" % packetHandler.getTxRxResult(siar_comm_result))

        # Check if groupbulkread data of siar#1 is available
        siar_getdata_result = groupBulkRead.isAvailable(SIAR1_ID, ADDR_PRO_PRESENT_POSITION, LEN_PRO_PRESENT_POSITION)
        if siar_getdata_result != True:
            print("[ID:%03d] groupBulkRead getdata failed" % SIAR1_ID)
            quit()

        # Check if groupbulkread data of siar#2 is available
        siar_getdata_result = groupBulkRead.isAvailable(SIAR2_ID, ADDR_PRO_LED_RED, LEN_PRO_LED_RED)
        if siar_getdata_result != True:
            print("[ID:%03d] groupBulkRead getdata failed" % SIAR2_ID)
            quit()

        # Get present position value
        siar1_present_position = groupBulkRead.getData(SIAR1_ID, ADDR_PRO_PRESENT_POSITION, LEN_PRO_PRESENT_POSITION)

        # Get LED value
        siar2_led_value_read = groupBulkRead.getData(SIAR2_ID, ADDR_PRO_LED_RED, LEN_PRO_LED_RED)

        print("[ID:%03d] Present Position : %d \t [ID:%03d] LED Value: %d" % (SIAR1_ID, siar1_present_position, SIAR2_ID, siar2_led_value_read))

        if not (abs(siar_goal_position[index] - siar1_present_position) > SIAR_MOVING_STATUS_THRESHOLD):
            break

    # Change goal position
    if index == 0:
        index = 1
    else:
        index = 0

# Clear bulkread parameter storage
groupBulkRead.clearParam()

# Disable siar#1 Torque
siar_comm_result, siar_error = packetHandler.write1ByteTxRx(portHandler, SIAR1_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_DISABLE)
if siar_comm_result != COMM_SUCCESS:
    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
elif siar_error != 0:
    print("%s" % packetHandler.getRxPacketError(siar_error))

# Disable siar#2 Torque
siar_comm_result, siar_error = packetHandler.write1ByteTxRx(portHandler, SIAR2_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_DISABLE)
if siar_comm_result != COMM_SUCCESS:
    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
elif siar_error != 0:
    print("%s" % packetHandler.getRxPacketError(siar_error))

# Close port
portHandler.closePort()
