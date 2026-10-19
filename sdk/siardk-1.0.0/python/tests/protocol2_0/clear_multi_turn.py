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

# Author: Ki Jong Gil (Gilbert)

#
# *********     Clear Multi Turn Example      *********
#
#
# Available siar model on this example : All models using Protocol 2.0
# This example is designed for using a siar PRO 54-200, and an USB2siar.
# To use another siar model, such as X series, see their details in E-Manual(emanual.vkrobot.com) and edit below variables yourself.
# Be sure that siar PRO properties are already set as %% ID : 1 / Baudnum : 1 (Baudrate : 57600)
#

from __future__ import print_function
import os

if os.name == 'nt':
    import msvcrt
    def getch():
        return msvcrt.getch().decode()
else:
    import termios, fcntl, sys, os
    from select import select
    fd = sys.stdin.fileno()
    old_term = termios.tcgetattr(fd)
    new_term = termios.tcgetattr(fd)

    def getch():   
        new_term[3] = (new_term[3] & ~termios.ICANON & ~termios.ECHO)
        termios.tcsetattr(fd, termios.TCSANOW, new_term)
        try:
            ch = sys.stdin.read(1)
        finally:      
            termios.tcsetattr(fd, termios.TCSADRAIN, old_term)
        return ch
        
    def kbhit():
        new_term[3] = (new_term[3] & ~(termios.ICANON | termios.ECHO))
        termios.tcsetattr(fd, termios.TCSANOW, new_term)
        try:
            dr,dw,de = select([sys.stdin], [], [], 0)
            if dr != []:
                return 1
        finally: 
            termios.tcsetattr(fd, termios.TCSADRAIN, old_term)
            sys.stdout.flush()

        return 0

from siardk import *                    # Uses SIARDK library

# Control table address
ADDR_OPERATING_MODE         = 11               # Control table address is different in siar model
ADDR_TORQUE_ENABLE          = 64               
ADDR_GOAL_POSITION          = 116
ADDR_PRESENT_POSITION       = 132

# Protocol version
PROTOCOL_VERSION            = 2.0               # See which protocol version is used in the siar

# Default setting
SIAR_ID                      = 1                 # siar ID : 1
BAUDRATE                    = 57600             # siar default baudrate : 57600
DEVICENAME                  = '/dev/ttyUSB0'    # Check which port is being used on your controller
                                                # ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"

TORQUE_ENABLE               = 1                 # Value for enabling the torque
TORQUE_DISABLE              = 0                 # Value for disabling the torque
MAX_POSITION_VALUE          = 1048575
SIAR_MOVING_STATUS_THRESHOLD = 20                # siar will rotate between this value
EXT_POSITION_CONTROL_MODE   = 4                 # Value for extended position control mode (operating mode)

ESC_ASCII_VALUE             = 0x1b
SPACE_ASCII_VALUE           = 0x20

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

# Set operating mode to extended position control mode
siar_comm_result, siar_error = packetHandler.write1ByteTxRx(portHandler, SIAR_ID, ADDR_OPERATING_MODE, EXT_POSITION_CONTROL_MODE)
if siar_comm_result != COMM_SUCCESS:
    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
elif siar_error != 0:
    print("%s" % packetHandler.getRxPacketError(siar_error))
else:
    print("Operating mode changed to extended position control mode.")

# Enable siar Torque
siar_comm_result, siar_error = packetHandler.write1ByteTxRx(portHandler, SIAR_ID, ADDR_TORQUE_ENABLE, TORQUE_ENABLE)
if siar_comm_result != COMM_SUCCESS:
    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
elif siar_error != 0:
    print("%s" % packetHandler.getRxPacketError(siar_error))
else:
    print("siar has been successfully connected")

while 1:
    print("\nPress any key to continue! (or press ESC to quit!)")
    if getch() == chr(ESC_ASCII_VALUE):
        break
    print("  Press SPACE key to clear multi-turn information! (or press ESC to stop!)")

    # Write goal position    
    siar_comm_result, siar_error = packetHandler.write4ByteTxRx(portHandler, SIAR_ID, ADDR_GOAL_POSITION, MAX_POSITION_VALUE)
    if siar_comm_result != COMM_SUCCESS:
        print("%s" % packetHandler.getTxRxResult(siar_comm_result))
    elif siar_error != 0:
        print("%s" % packetHandler.getRxPacketError(siar_error))

    while 1:
        # Read present position
        siar_present_position, siar_comm_result, siar_error = packetHandler.read4ByteTxRx(portHandler, SIAR_ID, ADDR_PRESENT_POSITION)
        if siar_comm_result != COMM_SUCCESS:
            print("%s" % packetHandler.getTxRxResult(siar_comm_result))
        elif siar_error != 0:
            print("%s" % packetHandler.getRxPacketError(siar_error))

        print("   [ID:%03d] GoalPos:%03d  PresPos:%03d" %(SIAR_ID, MAX_POSITION_VALUE, siar_present_position), end = "\r") 
        if kbhit():
            c = getch()
            if c == chr(SPACE_ASCII_VALUE):
                print("\n  Stop & Clear Multi-Turn Information! ")   
                # Write the present position to the goal position to stop moving
                siar_comm_result, siar_error = packetHandler.write4ByteTxRx(portHandler, SIAR_ID, ADDR_GOAL_POSITION, siar_present_position)
                if siar_comm_result != COMM_SUCCESS:
                    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
                elif siar_error != 0:
                    print("%s" % packetHandler.getRxPacketError(siar_error))
                
                time.sleep(0.3)

                # Clear Multi-Turn Information
                siar_comm_result, siar_error = packetHandler.clearMultiTurn(portHandler, SIAR_ID)
                if siar_comm_result != COMM_SUCCESS:
                    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
                elif siar_error != 0:
                    print("%s" % packetHandler.getRxPacketError(siar_error))

                # Read present position
                siar_present_position, siar_comm_result, siar_error = packetHandler.read4ByteTxRx(portHandler, SIAR_ID, ADDR_PRESENT_POSITION)
                if siar_comm_result != COMM_SUCCESS:
                    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
                elif siar_error != 0:
                    print("%s" % packetHandler.getRxPacketError(siar_error))

                print("  Present Position has been reset. : %03d" % siar_present_position)

                break

            elif c == chr(ESC_ASCII_VALUE):
                print("\n  Stopped!!")
                # Write the present position to the goal position to stop moving
                siar_comm_result, siar_error = packetHandler.write4ByteTxRx(portHandler, SIAR_ID, ADDR_GOAL_POSITION, siar_present_position)
                if siar_comm_result != COMM_SUCCESS:
                    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
                elif siar_error != 0:
                    print("%s" % packetHandler.getRxPacketError(siar_error))
                break

        if not abs(MAX_POSITION_VALUE - siar_present_position) > SIAR_MOVING_STATUS_THRESHOLD:
            break

# Disable siar Torque
siar_comm_result, siar_error = packetHandler.write1ByteTxRx(portHandler, SIAR_ID, ADDR_TORQUE_ENABLE, TORQUE_DISABLE)
if siar_comm_result != COMM_SUCCESS:
    print("%s" % packetHandler.getTxRxResult(siar_comm_result))
elif siar_error != 0:
    print("%s" % packetHandler.getRxPacketError(siar_error))

# Close port
portHandler.closePort()
