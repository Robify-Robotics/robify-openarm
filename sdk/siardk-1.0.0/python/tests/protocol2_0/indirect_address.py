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
# *********     Indirect Address Example      *********
#
#
# Available siar model on this example : All models using Protocol 2.0
# This example is tested with a siar PRO 54-200, and an USB2siar
# Be sure that siar PRO properties are already set as %% ID : 1 / Baudnum : 1 (Baudrate : 57600)
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

from siardk import *                         # Uses SIARDK library

# Control table address
# Control table address is different in siar model
ADDR_PRO_INDIRECTADDRESS_FOR_WRITE      = 168#49        # EEPROM region
ADDR_PRO_INDIRECTADDRESS_FOR_READ       = 178#59        # EEPROM region
ADDR_PRO_TORQUE_ENABLE                  = 512#562
ADDR_PRO_LED_RED                        = 65#563
ADDR_PRO_GOAL_POSITION                  = 116#596
ADDR_PRO_MOVING                         = 122#610
ADDR_PRO_PRESENT_POSITION               = 132#611
ADDR_PRO_INDIRECTDATA_FOR_WRITE         = 224#634
ADDR_PRO_INDIRECTDATA_FOR_READ          = 229#639

# Data Byte Length
LEN_PRO_LED_RED                         = 1
LEN_PRO_GOAL_POSITION                   = 4
LEN_PRO_MOVING                          = 1
LEN_PRO_PRESENT_POSITION                = 4
LEN_PRO_INDIRECTDATA_FOR_WRITE          = 5
LEN_PRO_INDIRECTDATA_FOR_READ           = 5

# Protocol version
PROTOCOL_VERSION            = 2.0                   # See which protocol version is used in the siar

# Default setting
SIAR_ID                      = 1                     # siar ID : 1
BAUDRATE                    = 57600                 # siar default baudrate : 57600
DEVICENAME                  = '/dev/ttyUSB0'        # Check which port is being used on your controller
                                                    # ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"

TORQUE_ENABLE               = 1                     # Value for enabling the torque
TORQUE_DISABLE              = 0                     # Value for disabling the torque
SIAR_MINIMUM_POSITION_VALUE  = 100           # siar will rotate between this value
SIAR_MAXIMUM_POSITION_VALUE  = 4000            # and this value (note that the siar would not move when the position value is out of movable range. Check e-manual about the range of the siar you use.)
SIAR_MINIMUM_LED_VALUE       = 0                     #  siar LED will light between this value
SIAR_MAXIMUM_LED_VALUE       = 1                  #  and this value
SIAR_MOVING_STATUS_THRESHOLD = 20                # siar moving status threshold

index = 0
siar_goal_position = [SIAR_MINIMUM_POSITION_VALUE, SIAR_MAXIMUM_POSITION_VALUE]        # Goal position
siar_led_value = [SIAR_MINIMUM_LED_VALUE, SIAR_MAXIMUM_LED_VALUE]                      # siar LED value

# Initialize PortHandler instance
# Set the port path
# Get methods and members of PortHandlerLinux or PortHandlerWindows
portHandler = PortHandler(DEVICENAME)

# Initialize PacketHandler instance
# Set the protocol version
# Get methods and members of Protocol1PacketHandler or Protocol2PacketHandler
packetHandler = PacketHandler(PROTOCOL_VERSION)

# Initialize GroupSyncWrite instance
groupSyncWrite = GroupSyncWrite(portHandler, packetHandler, ADDR_PRO_INDIRECTDATA_FOR_WRITE, LEN_PRO_INDIRECTDATA_FOR_WRITE)

# Initialize GroupSyncRead instace for Present Position
groupSyncRead = GroupSyncRead(portHandler, packetHandler, ADDR_PRO_INDIRECTDATA_FOR_READ, LEN_PRO_INDIRECTDATA_FOR_READ)

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


# Disable siar Torque :
# Indirect address would not accessible when the torque is already enabled
siar_comm_result, siar_error = packetHandler.write1ByteTxRx(portHandler, SIAR_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_DISABLE)
if siar_comm_result != COMM_SUCCESS:
    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
elif siar_error != 0:
    print("%s" % packetHandler.getRxPacketError(siar_error))
else:
    print("[ID:%03d] siar has been successfully connected" % SIAR_ID)

# INDIRECTDATA parameter storages replace LED, goal position, present position and moving status storages
siar_comm_result, siar_error = packetHandler.write2ByteTxRx(portHandler, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_WRITE + 0, ADDR_PRO_GOAL_POSITION + 0)
if siar_comm_result != COMM_SUCCESS:
    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
elif siar_error != 0:
    print("%s" % packetHandler.getRxPacketError(siar_error))
siar_comm_result, siar_error = packetHandler.write2ByteTxRx(portHandler, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_WRITE + 2, ADDR_PRO_GOAL_POSITION + 1)
if siar_comm_result != COMM_SUCCESS:
    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
elif siar_error != 0:
    print("%s" % packetHandler.getRxPacketError(siar_error))
siar_comm_result, siar_error = packetHandler.write2ByteTxRx(portHandler, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_WRITE + 4, ADDR_PRO_GOAL_POSITION + 2)
if siar_comm_result != COMM_SUCCESS:
    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
elif siar_error != 0:
    print("%s" % packetHandler.getRxPacketError(siar_error))
siar_comm_result, siar_error = packetHandler.write2ByteTxRx(portHandler, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_WRITE + 6, ADDR_PRO_GOAL_POSITION + 3)
if siar_comm_result != COMM_SUCCESS:
    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
elif siar_error != 0:
    print("%s" % packetHandler.getRxPacketError(siar_error))
siar_comm_result, siar_error = packetHandler.write2ByteTxRx(portHandler, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_WRITE + 8, ADDR_PRO_LED_RED)
if siar_comm_result != COMM_SUCCESS:
    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
elif siar_error != 0:
    print("%s" % packetHandler.getRxPacketError(siar_error))

siar_comm_result, siar_error = packetHandler.write2ByteTxRx(portHandler, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_READ + 0, ADDR_PRO_PRESENT_POSITION + 0)
if siar_comm_result != COMM_SUCCESS:
    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
elif siar_error != 0:
    print("%s" % packetHandler.getRxPacketError(siar_error))
siar_comm_result, siar_error = packetHandler.write2ByteTxRx(portHandler, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_READ + 2, ADDR_PRO_PRESENT_POSITION + 1)
if siar_comm_result != COMM_SUCCESS:
    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
elif siar_error != 0:
    print("%s" % packetHandler.getRxPacketError(siar_error))
siar_comm_result, siar_error = packetHandler.write2ByteTxRx(portHandler, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_READ + 4, ADDR_PRO_PRESENT_POSITION + 2)
if siar_comm_result != COMM_SUCCESS:
    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
elif siar_error != 0:
    print("%s" % packetHandler.getRxPacketError(siar_error))
siar_comm_result, siar_error = packetHandler.write2ByteTxRx(portHandler, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_READ + 6, ADDR_PRO_PRESENT_POSITION + 3)
if siar_comm_result != COMM_SUCCESS:
    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
elif siar_error != 0:
    print("%s" % packetHandler.getRxPacketError(siar_error))
siar_comm_result, siar_error = packetHandler.write2ByteTxRx(portHandler, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_READ + 8, ADDR_PRO_MOVING)
if siar_comm_result != COMM_SUCCESS:
    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
elif siar_error != 0:
    print("%s" % packetHandler.getRxPacketError(siar_error))

# Enable siar Torque
siar_comm_result, siar_error = packetHandler.write1ByteTxRx(portHandler, SIAR_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_ENABLE)
if siar_comm_result != COMM_SUCCESS:
    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
elif siar_error != 0:
    print("%s" % packetHandler.getRxPacketError(siar_error))


# Add parameter storage for multiple values
siar_addparam_result = groupSyncRead.addParam(SIAR_ID)
if siar_addparam_result != True:
    print("[ID:%03d] groupSyncRead addparam failed" % SIAR_ID)
    quit()

while 1:
    print("Press any key to continue! (or press ESC to quit!)")
    if getch() == chr(0x1b):
        break

    # Allocate goal position value into byte array
    param_indirect_data_for_write = [SIAR_LOBYTE(SIAR_LOWORD(siar_goal_position[index])), SIAR_HIBYTE(SIAR_LOWORD(siar_goal_position[index])), SIAR_LOBYTE(SIAR_HIWORD(siar_goal_position[index])), SIAR_HIBYTE(SIAR_HIWORD(siar_goal_position[index]))]
    param_indirect_data_for_write.append(siar_led_value[index])

    # Add values to the Syncwrite parameter storage
    siar_addparam_result = groupSyncWrite.addParam(SIAR_ID, param_indirect_data_for_write)
    if siar_addparam_result != True:
        print("[ID:%03d]groupSyncWrite addparam failed" % SIAR_ID)
        quit()

    # Syncwrite all
    siar_comm_result = groupSyncWrite.txPacket()
    if siar_comm_result != COMM_SUCCESS:
        print("%s" % packetHandler.getTxRxResult(siar_comm_result))

    # Clear syncwrite parameter storage
    groupSyncWrite.clearParam()

    while 1:
        # Syncread present position from indirectdata2
        siar_comm_result = groupSyncRead.txRxPacket()
        if siar_comm_result != COMM_SUCCESS:
            print("%s" % packetHandler.getTxRxResult(siar_comm_result))

        # Check if groupsyncread data of siar present position value is available
        siar_getdata_result = groupSyncRead.isAvailable(SIAR_ID, ADDR_PRO_INDIRECTDATA_FOR_READ, LEN_PRO_PRESENT_POSITION)
        if siar_getdata_result != True:
            print("[ID:%03d] groupSyncRead getdata failed" % SIAR_ID)
            quit()

        # Check if groupsyncread data of siar moving status is available
        siar_getdata_result = groupSyncRead.isAvailable(SIAR_ID, ADDR_PRO_INDIRECTDATA_FOR_READ + LEN_PRO_PRESENT_POSITION, LEN_PRO_MOVING)
        if siar_getdata_result != True:
            print("[ID:%03d] groupSyncRead getdata failed" % SIAR_ID)
            quit()

        # Get siar present position value
        siar_present_position = groupSyncRead.getData(SIAR_ID, ADDR_PRO_INDIRECTDATA_FOR_READ, LEN_PRO_PRESENT_POSITION)

        # Get siar moving status value
        siar_moving = groupSyncRead.getData(SIAR_ID, ADDR_PRO_INDIRECTDATA_FOR_READ + LEN_PRO_PRESENT_POSITION, LEN_PRO_MOVING)

        print("[ID:%03d] GoalPos:%d  PresPos:%d  IsMoving:%d" % (SIAR_ID, siar_goal_position[index], siar_present_position, siar_moving))

        if not (abs(siar_goal_position[index] - siar_present_position) > SIAR_MOVING_STATUS_THRESHOLD):
            break

    # Change goal position
    if index == 0:
        index = 1
    else:
        index = 0

# Clear syncread parameter storage
groupSyncRead.clearParam()

# Disable siar Torque
siar_comm_result, siar_error = packetHandler.write1ByteTxRx(portHandler, SIAR_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_DISABLE)
if siar_comm_result != COMM_SUCCESS:
    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
elif siar_error != 0:
    print("%s" % packetHandler.getRxPacketError(siar_error))

# Close port
portHandler.closePort()
