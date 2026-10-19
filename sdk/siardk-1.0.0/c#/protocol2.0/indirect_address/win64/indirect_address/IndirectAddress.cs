/*******************************************************************************
* Copyright 2017 VKROBOT CO., LTD.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*     http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*******************************************************************************/

/* Author: Ryu Woon Jung (Leon) */

//
// *********     Indirect Address Example      *********
//
//
// Available siar model on this example : All models using Protocol 2.0
// This example is designed for using a siar PRO 54-200, and an USB2siar.
// To use another siar model, such as X series, see their details in E-Manual(emanual.vkrobot.com) and edit below variables yourself.
// Be sure that siar PRO properties are already set as %% ID : 1 / Baudnum : 1 (Baudrate : 57600)
//

using System;
using System.Runtime.InteropServices;
using siardk;

namespace sync_write
{
  class SyncWrite
  {
    // Control table address                                                        // Control table address is different in siar model
    public const int ADDR_PRO_INDIRECTADDRESS_FOR_WRITE      = 49;                  // EEPROM region
    public const int ADDR_PRO_INDIRECTADDRESS_FOR_READ       = 59;                  // EEPROM region
    public const int ADDR_PRO_TORQUE_ENABLE                  = 512;
    public const int ADDR_PRO_LED_RED                        = 563;
    public const int ADDR_PRO_GOAL_POSITION                  = 532;
    public const int ADDR_PRO_MOVING                         = 610;
    public const int ADDR_PRO_PRESENT_POSITION               = 611;
    public const int ADDR_PRO_INDIRECTDATA_FOR_WRITE         = 634;
    public const int ADDR_PRO_INDIRECTDATA_FOR_READ          = 639;

    // Data Byte Length
    public const int LEN_PRO_LED_RED                         = 1;
    public const int LEN_PRO_GOAL_POSITION                   = 4;
    public const int LEN_PRO_MOVING                          = 1;
    public const int LEN_PRO_PRESENT_POSITION                = 4;
    public const int LEN_PRO_INDIRECTDATA_FOR_WRITE          = 5;
    public const int LEN_PRO_INDIRECTDATA_FOR_READ           = 5;

    // Protocol version
    public const int PROTOCOL_VERSION                        = 2;                   // See which protocol version is used in the siar

    // Default setting
    public const int SIAR_ID                                  = 1;                   // siar ID: 1
    public const int BAUDRATE                                = 1000000;
    public const string DEVICENAME                           = "COM1";              // Check which port is being used on your controller
                                                                                    // ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"

    public const int TORQUE_ENABLE                           = 1;                   // Value for enabling the torque
    public const int TORQUE_DISABLE                          = 0;                   // Value for disabling the torque
    public const int SIAR_MINIMUM_POSITION_VALUE              = -150000;             // siar will rotate between this value
    public const int SIAR_MAXIMUM_POSITION_VALUE              = 150000;              // and this value (note that the siar would not move when the position value is out of movable range. Check e-manual about the range of the siar you use.)
    public const int SIAR_MINIMUM_LED_VALUE                   = 0;                   // siar LED will light between this value
    public const int SIAR_MAXIMUM_LED_VALUE                   = 255;                 // and this value
    public const int SIAR_MOVING_STATUS_THRESHOLD             = 20;                  // siar moving status threshold

    public const byte ESC_ASCII_VALUE                        = 0x1b;

    public const int COMM_SUCCESS                            = 0;                   // Communication Success result value
    public const int COMM_TX_FAIL                            = -1001;               // Communication Tx Failed

    static void Main(string[] args)
    {
      // Initialize PortHandler Structs
      // Set the port path
      // Get methods and members of PortHandlerLinux or PortHandlerWindows
      int port_num = siar.portHandler(DEVICENAME);

      // Initialize PacketHandler Structs
      siar.packetHandler();

      // Initialize Groupsyncwrite instance
      int groupwrite_num = siar.groupSyncWrite(port_num, PROTOCOL_VERSION, ADDR_PRO_INDIRECTDATA_FOR_WRITE, LEN_PRO_INDIRECTDATA_FOR_WRITE);

      // Initialize Groupsyncread Structs for Present Position
      int groupread_num = siar.groupSyncRead(port_num, PROTOCOL_VERSION, ADDR_PRO_INDIRECTDATA_FOR_READ, LEN_PRO_INDIRECTDATA_FOR_READ);

      int index = 0;
      int siar_comm_result = COMM_TX_FAIL;                     // Communication result
      bool siar_addparam_result = false;                       // AddParam result
      bool siar_getdata_result = false;                        // GetParam result
      int[] siar_goal_position = new int[2]{ SIAR_MINIMUM_POSITION_VALUE, SIAR_MAXIMUM_POSITION_VALUE };         // Goal position

      byte siar_error = 0;                                     // siar error
      byte siar_moving = 0;                                    // siar moving status
      byte[] siar_led_value = new byte[2]{ 0x00, 0xFF };       // siar LED value
      Int32 siar_present_position = 0;                         // Present position

      // Open port
      if (siar.openPort(port_num))
      {
        Console.WriteLine("Succeeded to open the port!");
      }
      else
      {
        Console.WriteLine("Failed to open the port!");
        Console.WriteLine("Press any key to terminate...");
        Console.ReadKey();
        return;
      }

      // Set port baudrate
      if (siar.setBaudRate(port_num, BAUDRATE))
      {
        Console.WriteLine("Succeeded to change the baudrate!");
      }
      else
      {
        Console.WriteLine("Failed to change the baudrate!");
        Console.WriteLine("Press any key to terminate...");
        Console.ReadKey();
        return;
      }

      // Disable siar Torque :
      // Indirect address would not accessible when the torque is already enabled
      siar.write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_DISABLE);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
      }
      else
      {
        Console.WriteLine("siar has been successfully connected");
      }

      // INDIRECTDATA parameter storages replace LED, goal position, present position and moving status storages
      siar.write2ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_WRITE + 0, ADDR_PRO_GOAL_POSITION + 0);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
      }

      siar.write2ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_WRITE + 2, ADDR_PRO_GOAL_POSITION + 1);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
      }

      siar.write2ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_WRITE + 4, ADDR_PRO_GOAL_POSITION + 2);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
      }

      siar.write2ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_WRITE + 6, ADDR_PRO_GOAL_POSITION + 3);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
      }

      siar.write2ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_WRITE + 8, ADDR_PRO_LED_RED);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
      }

      siar.write2ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_READ + 0, ADDR_PRO_PRESENT_POSITION + 0);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
      }

      siar.write2ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_READ + 2, ADDR_PRO_PRESENT_POSITION + 1);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
      }

      siar.write2ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_READ + 4, ADDR_PRO_PRESENT_POSITION + 2);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
      }

      siar.write2ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_READ + 6, ADDR_PRO_PRESENT_POSITION + 3);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
      }

      siar.write2ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_READ + 8, ADDR_PRO_MOVING);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
      }

      // Enable siar Torque
      siar.write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_ENABLE);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
      }

      // Add parameter storage for siar present position value
      siar_addparam_result = siar.groupSyncReadAddParam(groupread_num, SIAR_ID);
      if (siar_addparam_result != true)
      {
        Console.WriteLine("[ID: {0}] groupSyncRead addparam failed", SIAR_ID);
        return;
      }

      while (true)
      {
        Console.WriteLine("Press any key to continue! (or press ESC to quit!)");
        if (Console.ReadKey().KeyChar == ESC_ASCII_VALUE)
          break;

        // Add values to the Syncwrite storage
        siar_addparam_result = siar.groupSyncWriteAddParam(groupwrite_num, SIAR_ID, (UInt32)siar_goal_position[index], LEN_PRO_GOAL_POSITION);
        if (siar_addparam_result != true)
        {
          Console.WriteLine("[ID: {0}] groupSyncWrite addparam failed", SIAR_ID);
          return;
        }
        siar_addparam_result = siar.groupSyncWriteAddParam(groupwrite_num, SIAR_ID, siar_led_value[index], LEN_PRO_LED_RED);
        if (siar_addparam_result != true)
        {
          Console.WriteLine("[ID: {0}] groupSyncWrite addparam failed", SIAR_ID);
          return;
        }

        // Syncwrite goal position
        siar.groupSyncWriteTxPacket(groupwrite_num);
        if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
          Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));

        // Clear syncwrite parameter storage
        siar.groupSyncWriteClearParam(groupwrite_num);

        do
        {
          // Syncread present position from indirectdata2
          siar.groupSyncReadTxRxPacket(groupread_num);
          if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
            Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));

          // Check if groupsyncread data of Dyanamixel is available
          siar_getdata_result = siar.groupSyncReadIsAvailable(groupread_num, SIAR_ID, ADDR_PRO_INDIRECTDATA_FOR_READ, LEN_PRO_PRESENT_POSITION);
          if (siar_getdata_result != true)
          {
            Console.WriteLine("[ID: {0}] groupSyncRead getdata failed", SIAR_ID);
            return;
          }

          // Check if groupsyncread data of Dyanamixel is available
          siar_getdata_result = siar.groupSyncReadIsAvailable(groupread_num, SIAR_ID, ADDR_PRO_INDIRECTDATA_FOR_READ + LEN_PRO_PRESENT_POSITION, LEN_PRO_MOVING);
          if (siar_getdata_result != true)
          {
            Console.WriteLine("[ID: {0}] groupSyncRead getdata failed", SIAR_ID);
            return;
          }

          // Get siar present position value
          siar_present_position = (Int32)siar.groupSyncReadGetData(groupread_num, SIAR_ID, ADDR_PRO_INDIRECTDATA_FOR_READ, LEN_PRO_PRESENT_POSITION);

          // Get siar moving status value
          siar_moving = (byte)siar.groupSyncReadGetData(groupread_num, SIAR_ID, ADDR_PRO_INDIRECTDATA_FOR_READ + LEN_PRO_PRESENT_POSITION, LEN_PRO_MOVING);

          Console.WriteLine("[ID: {0}] GoalPos: {1}  PresPos: {2}  IsMoving: {3}", SIAR_ID, siar_goal_position[index], siar_present_position, siar_moving);

        } while (Math.Abs(siar_goal_position[index] - siar_present_position) > SIAR_MOVING_STATUS_THRESHOLD);

        // Change goal position
        if (index == 0)
        {
          index = 1;
        }
        else
        {
          index = 0;
        }
      }

      // Disable siar Torque
      siar.write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_DISABLE);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
      }

      // Close port
      siar.closePort(port_num);

      return;
    }
  }
}
