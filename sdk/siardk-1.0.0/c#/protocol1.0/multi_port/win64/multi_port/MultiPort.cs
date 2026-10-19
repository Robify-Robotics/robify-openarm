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
// *********     Multi Port Example      *********
//
//
// Available siar model on this example : All models using Protocol 1.0
// This example is designed for using two siar MX-28, and two USB2siar.
// To use another siar model, such as X series, see their details in E-Manual(emanual.vkrobot.com) and edit below variables yourself.
// Be sure that siar MX properties are already set as %% ID : 1 / Baudnum : 34 (Baudrate : 57600)
//

using System;
using System.Runtime.InteropServices;
using siardk;

namespace read_write
{
  class ReadWrite
  {
    // Control table address
    public const int ADDR_MX_TORQUE_ENABLE           = 24;                  // Control table address is different in siar model
    public const int ADDR_MX_GOAL_POSITION           = 30;
    public const int ADDR_MX_PRESENT_POSITION        = 36;

    // Protocol version
    public const int PROTOCOL_VERSION                = 1;                   // See which protocol version is used in the siar

    // Default setting
    public const int SIAR1_ID                         = 1;                   // siar ID: 1
    public const int SIAR2_ID                         = 2;                   // siar ID: 2
    public const int BAUDRATE                        = 1000000;
    public const string DEVICENAME1                  = "COM1";              // Check which port is being used on your controller
    public const string DEVICENAME2                  = "COM2";              // ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"

    public const int TORQUE_ENABLE                   = 1;                   // Value for enabling the torque
    public const int TORQUE_DISABLE                  = 0;                   // Value for disabling the torque
    public const int SIAR_MINIMUM_POSITION_VALUE      = 100;                 // siar will rotate between this value
    public const int SIAR_MAXIMUM_POSITION_VALUE      = 4000;                // and this value (note that the siar would not move when the position value is out of movable range. Check e-manual about the range of the siar you use.)
    public const int SIAR_MOVING_STATUS_THRESHOLD     = 10;                  // siar moving status threshold

    public const byte ESC_ASCII_VALUE                = 0x1b;

    public const int COMM_SUCCESS                    = 0;                   // Communication Success result value
    public const int COMM_TX_FAIL                    = -1001;               // Communication Tx Failed

    static void Main(string[] args)
    {
      // Initialize PortHandler Structs
      // Set the port path
      // Get methods and members of PortHandlerLinux or PortHandlerWindows
      int port_num1 = siar.portHandler(DEVICENAME1);
      int port_num2 = siar.portHandler(DEVICENAME2);

      // Initialize PacketHandler Structs
      siar.packetHandler();

      int index = 0;
      int siar_comm_result = COMM_TX_FAIL;                                   // Communication result
      UInt16[] siar_goal_position = new UInt16[2]{ SIAR_MINIMUM_POSITION_VALUE, SIAR_MAXIMUM_POSITION_VALUE }; // Goal position

      byte siar_error = 0;                                                   // siar error
      UInt16 siar1_present_position = 0, siar2_present_position = 0;          // Present position

      // Open port1
      if (siar.openPort(port_num1))
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

      // Open port2
      if (siar.openPort(port_num2))
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

      // Set port1 baudrate
      if (siar.setBaudRate(port_num1, BAUDRATE))
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

      // Set port2 baudrate
      if (siar.setBaudRate(port_num2, BAUDRATE))
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
      siar.write1ByteTxRx(port_num1, PROTOCOL_VERSION, SIAR1_ID, ADDR_MX_TORQUE_ENABLE, TORQUE_ENABLE);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num1, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num1, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
      }
      else
      {
        Console.WriteLine("siar#{0} has been successfully connected ", SIAR1_ID);
      }

      // Enable siar#2 Torque
      siar.write1ByteTxRx(port_num2, PROTOCOL_VERSION, SIAR2_ID, ADDR_MX_TORQUE_ENABLE, TORQUE_ENABLE);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num2, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num2, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
      }
      else
      {
        Console.WriteLine("siar#{0} has been successfully connected ", SIAR2_ID);
      }

      while (true)
      {
        Console.WriteLine("Press any key to continue! (or press ESC to quit!)");
        if (Console.ReadKey().KeyChar == ESC_ASCII_VALUE)
          break;

        // Write siar#1 goal position
        siar.write2ByteTxRx(port_num1, PROTOCOL_VERSION, SIAR1_ID, ADDR_MX_GOAL_POSITION, siar_goal_position[index]);
        if ((siar_comm_result = siar.getLastTxRxResult(port_num1, PROTOCOL_VERSION)) != COMM_SUCCESS)
        {
          Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
        }
        else if ((siar_error = siar.getLastRxPacketError(port_num1, PROTOCOL_VERSION)) != 0)
        {
          Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
        }

        // Write siar#2 goal position
        siar.write2ByteTxRx(port_num2, PROTOCOL_VERSION, SIAR2_ID, ADDR_MX_GOAL_POSITION, siar_goal_position[index]);
        if ((siar_comm_result = siar.getLastTxRxResult(port_num2, PROTOCOL_VERSION)) != COMM_SUCCESS)
        {
          Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
        }
        else if ((siar_error = siar.getLastRxPacketError(port_num2, PROTOCOL_VERSION)) != 0)
        {
          Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
        }

        do
        {
          // Read siar#1 present position
          siar1_present_position = siar.read2ByteTxRx(port_num1, PROTOCOL_VERSION, SIAR1_ID, ADDR_MX_PRESENT_POSITION);
          if ((siar_comm_result = siar.getLastTxRxResult(port_num1, PROTOCOL_VERSION)) != COMM_SUCCESS)
          {
            Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
          }
          else if ((siar_error = siar.getLastRxPacketError(port_num1, PROTOCOL_VERSION)) != 0)
          {
            Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
          }

          // Read siar#2 present position
          siar2_present_position = siar.read2ByteTxRx(port_num2, PROTOCOL_VERSION, SIAR2_ID, ADDR_MX_PRESENT_POSITION);
          if ((siar_comm_result = siar.getLastTxRxResult(port_num2, PROTOCOL_VERSION)) != COMM_SUCCESS)
          {
            Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
          }
          else if ((siar_error = siar.getLastRxPacketError(port_num2, PROTOCOL_VERSION)) != 0)
          {
            Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
          }

          Console.WriteLine("[ID: {0}] GoalPos: {1}  PresPos: {2} [ID: {3}] GoalPos: {4}  PresPos: {5}", SIAR1_ID, siar_goal_position[index], siar1_present_position, SIAR2_ID, siar_goal_position[index], siar2_present_position);

        } while ((Math.Abs(siar_goal_position[index] - siar1_present_position) > SIAR_MOVING_STATUS_THRESHOLD) || (Math.Abs(siar_goal_position[index] - siar2_present_position) > SIAR_MOVING_STATUS_THRESHOLD));

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
      siar.write1ByteTxRx(port_num1, PROTOCOL_VERSION, SIAR1_ID, ADDR_MX_TORQUE_ENABLE, TORQUE_DISABLE);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num1, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num1, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
      }

      // Disable siar#2 Torque
      siar.write1ByteTxRx(port_num2, PROTOCOL_VERSION, SIAR2_ID, ADDR_MX_TORQUE_ENABLE, TORQUE_DISABLE);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num2, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num2, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
      }

      // Close port1
      siar.closePort(port_num1);

      // Close port2
      siar.closePort(port_num2);

      return;
    }
  }
}
