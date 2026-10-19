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
// *********     Bulk Read and Bulk Write Example      *********
//
//
// Available siar model on this example : All models using Protocol 2.0
// This example is designed for using two siar PRO 54-200, and an USB2siar.
// To use another siar model, such as X series, see their details in E-Manual(emanual.vkrobot.com) and edit below variables yourself.
// Be sure that siar PRO properties are already set as %% ID : 1 and 2 / Baudnum : 1 (Baudrate : 57600)
//

using System;
using System.Runtime.InteropServices;
using siardk;

namespace bulk_read_write
{
  class BulkReadWrite
  {
    // Control table address
    public const int ADDR_PRO_TORQUE_ENABLE           = 512;                // Control table address is different in siar model
    public const int ADDR_PRO_LED_RED                 = 563;
    public const int ADDR_PRO_GOAL_POSITION           = 532;
    public const int ADDR_PRO_PRESENT_POSITION        = 611;

    // Data Byte Length
    public const int LEN_PRO_LED_RED                  = 1;
    public const int LEN_PRO_GOAL_POSITION            = 4;
    public const int LEN_PRO_PRESENT_POSITION         = 4;

    // Protocol version
    public const int PROTOCOL_VERSION                = 2;                   // See which protocol version is used in the siar

    // Default setting
    public const int SIAR1_ID                         = 1;                   // siar ID: 1
    public const int SIAR2_ID                         = 2;                   // siar ID: 2
    public const int BAUDRATE                        = 1000000;
    public const string DEVICENAME                   = "COM1";              // Check which port is being used on your controller
                                                                            // ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"

    public const int TORQUE_ENABLE                   = 1;                   // Value for enabling the torque
    public const int TORQUE_DISABLE                  = 0;                   // Value for disabling the torque
    public const int SIAR_MINIMUM_POSITION_VALUE      = -150000;             // siar will rotate between this value
    public const int SIAR_MAXIMUM_POSITION_VALUE      = 150000;              // and this value (note that the siar would not move when the position value is out of movable range. Check e-manual about the range of the siar you use.)
    public const int SIAR_MOVING_STATUS_THRESHOLD     = 20;                  // siar moving status threshold

    public const byte ESC_ASCII_VALUE                = 0x1b;

    public const int COMM_SUCCESS                    = 0;                   // Communication Success result value
    public const int COMM_TX_FAIL                    = -1001;               // Communication Tx Failed

    static void Main(string[] args)
    {
      // Initialize PortHandler Structs
      // Set the port path
      // Get methods and members of PortHandlerLinux or PortHandlerWindows
      int port_num = siar.portHandler(DEVICENAME);

      // Initialize PacketHandler Structs
      siar.packetHandler();

      // Initialize GroupBulkWrite Struct
      int groupwrite_num = siar.groupBulkWrite(port_num, PROTOCOL_VERSION);

      // Initialize Groupbulkread Structs
      int groupread_num = siar.groupBulkRead(port_num, PROTOCOL_VERSION);

      int index = 0;
      int siar_comm_result = COMM_TX_FAIL;                                   // Communication result
      bool siar_addparam_result = false;                                     // AddParam result
      bool siar_getdata_result = false;                                      // GetParam result
      int[] siar_goal_position = new int[2]{ SIAR_MINIMUM_POSITION_VALUE, SIAR_MAXIMUM_POSITION_VALUE };         // Goal position

      byte siar_error = 0;                                                   // siar error
      byte[] siar_led_value = new byte[2]{ 0x00, 0xFF };                     // siar LED value for write
      Int32 siar1_present_position = 0;                                      // Present position
      byte siar2_led_value_read = 0;                                         // siar moving status

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

      // Enable siar#1 Torque
      siar.write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR1_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_ENABLE);
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
        Console.WriteLine("siar{0} has been successfully connected ", SIAR1_ID);
      }

      // Enable siar#2 Torque
      siar.write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR2_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_ENABLE);
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
        Console.WriteLine("siar{0} has been successfully connected ", SIAR1_ID);
      }

      // Add parameter storage for siar#1 present position value
      siar_addparam_result = siar.groupBulkReadAddParam(groupread_num, SIAR1_ID, ADDR_PRO_PRESENT_POSITION, LEN_PRO_PRESENT_POSITION);
      if (siar_addparam_result != true)
      {
        Console.WriteLine("[ID: {0}] groupBulkRead addparam failed", SIAR1_ID);
        return;
      }

      // Add parameter storage for siar#2 present moving value
      siar_addparam_result = siar.groupBulkReadAddParam(groupread_num, SIAR2_ID, ADDR_PRO_LED_RED, LEN_PRO_LED_RED);
      if (siar_addparam_result != true)
      {
        Console.WriteLine("[ID: {0}] groupBulkRead addparam failed", SIAR2_ID);
        return;
      }

      while (true)
      {
        Console.WriteLine("Press any key to continue! (or press ESC to quit!)");
        if (Console.ReadKey().KeyChar == ESC_ASCII_VALUE)
          break;

        // Add parameter storage for siar#1 goal position
        siar_addparam_result = siar.groupBulkWriteAddParam(groupwrite_num, SIAR1_ID, ADDR_PRO_GOAL_POSITION, LEN_PRO_GOAL_POSITION, (UInt32)siar_goal_position[index], LEN_PRO_GOAL_POSITION);
        if (siar_addparam_result != true)
        {
          Console.WriteLine("[ID: {0}] groupBulkWrite addparam failed", SIAR1_ID);
          return;
        }

        // Add parameter storage for siar#2 LED value
        siar_addparam_result = siar.groupBulkWriteAddParam(groupwrite_num, SIAR2_ID, ADDR_PRO_LED_RED, LEN_PRO_LED_RED, siar_led_value[index], LEN_PRO_LED_RED);
        if (siar_addparam_result != true)
        {
          Console.WriteLine("[ID: {0}] groupBulkWrite addparam failed", SIAR2_ID);
          return;
        }

        // Bulkwrite goal position and LED value
        siar.groupBulkWriteTxPacket(groupwrite_num);
        if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
          Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));

        // Clear bulkwrite parameter storage
        siar.groupBulkWriteClearParam(groupwrite_num);

        do
        {
          // Bulkread present position and moving status
          siar.groupBulkReadTxRxPacket(groupread_num);
          if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
            Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));

          siar_getdata_result = siar.groupBulkReadIsAvailable(groupread_num, SIAR1_ID, ADDR_PRO_PRESENT_POSITION, LEN_PRO_PRESENT_POSITION);
          if (siar_getdata_result != true)
          {
            Console.WriteLine("[ID: {0}] groupBulkRead getdata failed", SIAR1_ID);
            return;
          }

          siar_getdata_result = siar.groupBulkReadIsAvailable(groupread_num, SIAR2_ID, ADDR_PRO_LED_RED, LEN_PRO_LED_RED);
          if (siar_getdata_result != true)
          {
            Console.WriteLine("[ID: {0}] groupBulkRead getdata failed", SIAR2_ID);
            return;
          }

          // Get siar#1 present position value
          siar1_present_position = (Int32)siar.groupBulkReadGetData(groupread_num, SIAR1_ID, ADDR_PRO_PRESENT_POSITION, LEN_PRO_PRESENT_POSITION);

          // Get siar#2 moving status value
          siar2_led_value_read = (byte)siar.groupBulkReadGetData(groupread_num, SIAR2_ID, ADDR_PRO_LED_RED, LEN_PRO_LED_RED);

          Console.WriteLine("[ID: {0}] Present Position : {1} [ID: {2}] LED Value : {3}", SIAR1_ID, siar1_present_position, SIAR2_ID, siar2_led_value_read);

        } while (Math.Abs(siar_goal_position[index] - siar1_present_position) > SIAR_MOVING_STATUS_THRESHOLD);

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

      // Disable siar#1 Torque
      siar.write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR1_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_DISABLE);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
      }

      // Disable siar#2 Torque
      siar.write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR2_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_DISABLE);
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
