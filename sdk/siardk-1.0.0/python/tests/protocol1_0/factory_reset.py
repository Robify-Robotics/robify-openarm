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
# *********     Factory Reset Example      *********
#
#
# Available siar model on this example : All models using Protocol 1.0
# This example is tested with a siar MX-28, and an USB2siar
# Be sure that siar PRO properties are already set as %% ID : 1 / Baudnum : 34 (Baudrate : 57600)
#

# Be aware that:
# This example resets all properties of siar to default values, such as %% ID : 1 / Baudnum : 34 (Baudrate : 57600)
#


import os
from time import sleep

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

from siardk import *                 # Uses SIARDK library

# Control table address
ADDR_MX_BAUDRATE            = 8                 # Control table address is different in siar model

# Protocol version
PROTOCOL_VERSION            = 1.0               # See which protocol version is used in the siar

# Default setting
SIAR_ID                      = 1                 # siar ID : 1
BAUDRATE                    = 57600             # siar default baudrate : 57600
DEVICENAME                  = '/dev/ttyUSB0'    # Check which port is being used on your controller
                                                # ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"

FACTORYRST_DEFAULTBAUDRATE  = 57600             # siar baudrate set by factoryreset
NEW_BAUDNUM                 = 1                 # New baudnum to recover siar baudrate as it was
OPERATION_MODE              = 0x00              # Mode is unavailable in Protocol 1.0 Reset

# Initialize PortHandler instance
# Set the port path
# Get methods and members of PortHandlerLinux or PortHandlerWindows
portHandler = PortHandler(DEVICENAME)

# Initialize PacketHandler instance
# Set the protocol version
# Get methods and members of Protocol1PacketHandler or Protocol2PacketHandler
packetHandler = PacketHandler(PROTOCOL_VERSION)

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

# Read present baudrate of the controller
print("Now the controller baudrate is : %d" % portHandler.getBaudRate())

# Try factoryreset
print("[ID:%03d] Try factoryreset : " % SIAR_ID)

siar_comm_result, siar_error = packetHandler.factoryReset(portHandler, SIAR_ID, OPERATION_MODE)
if siar_comm_result != COMM_SUCCESS:
    print("Aborted")
    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
    quit()
elif siar_error != 0:
    print("%s" % packetHandler.getRxPacketError(siar_error))

# Wait for reset
print("Wait for reset...")
sleep(2.0)
print("[ID:%03d] factoryReset Success!" % SIAR_ID)

# Set controller baudrate to siar default baudrate
if portHandler.setBaudRate(FACTORYRST_DEFAULTBAUDRATE):
    print("Succeeded to change the controller baudrate to : %d" % FACTORYRST_DEFAULTBAUDRATE)
else:
    print("Failed to change the controller baudrate")
    print("Press any key to terminate...")
    quit()


# Read siar baudnum
siar_baudnum_read, siar_comm_result, siar_error = packetHandler.read1ByteTxRx(portHandler, SIAR_ID, ADDR_MX_BAUDRATE)
if siar_comm_result != COMM_SUCCESS:
    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
elif siar_error != 0:
    print("%s" % packetHandler.getRxPacketError(siar_error))
else:
    print("[ID:%03d] SIAR baudnum is now : %d" % (SIAR_ID, siar_baudnum_read))

# Write new baudnum
siar_comm_result, siar_error = packetHandler.write1ByteTxRx(portHandler, SIAR_ID, ADDR_MX_BAUDRATE, NEW_BAUDNUM)
if siar_comm_result != COMM_SUCCESS:
    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
elif siar_error != 0:
    print("%s" % packetHandler.getRxPacketError(siar_error))
else:
    print("[ID:%03d] Set siar baudnum to : %d" % (SIAR_ID, NEW_BAUDNUM))

# Set port baudrate to BAUDRATE
if portHandler.setBaudRate(BAUDRATE):
    print("Succeeded to change the controller baudrate to : %d" % BAUDRATE)
else:
    print("Failed to change the controller baudrate")
    print("Press any key to terminate...")
    quit()

sleep(0.2)

# Read siar baudnum
siar_baudnum_read, siar_comm_result, siar_error = packetHandler.read1ByteTxRx(portHandler, SIAR_ID, ADDR_MX_BAUDRATE)
if siar_comm_result != COMM_SUCCESS:
    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
elif siar_error != 0:
    print("%s" % packetHandler.getRxPacketError(siar_error))
else:
    print("[ID:%03d] siar Baudnum is now : %d" % (SIAR_ID, siar_baudnum_read))

# Close port
portHandler.closePort()