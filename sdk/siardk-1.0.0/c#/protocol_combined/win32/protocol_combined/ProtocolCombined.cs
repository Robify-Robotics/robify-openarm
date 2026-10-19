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
// *********     Protocol Combined Example      *********
//
//
// Available siar model on this example : All models using Protocol 1.0 and 2.0
// This example is tested with a siar MX-28, a siar PRO 54-200 and an USB2siar
// Be sure that properties of siar MX and PRO are already set as %% MX - ID : 1 / Baudnum : 34 (Baudrate : 57600) , PRO - ID : 1 / Baudnum : 1 (Baudrate : 57600)
//

// Be aware that:
// This example configures two different control tables (especially, if it uses siar and siar PRO). It may modify critical siar parameter on the control table, if siars have wrong ID.
//

using System;
using System.Runtime.InteropServices;
using siardk;

namespace protocol_combined
{
  class ProtocolCombined
  {
    // Control table address for siar MX
    public const int ADDR_MX_TORQUE_ENABLE          = 24;                  // Control table address is different in siar model
    public const int ADDR_MX_GOAL_POSITION          = 30;
    public const int ADDR_MX_PRESENT_POSITION       = 36;

    // Control table address for siar PRO
    public const int ADDR_PRO_TORQUE_ENABLE         = 512;
    public const int ADDR_PRO_GOAL_POSITION         = 532;
    public const int ADDR_PRO_PRESENT_POSITION      = 611;

    // Protocol version
    public const int PROTOCOL_VERSION1              = 1;                   // See which protocol version is used in the siar
    public const int PROTOCOL_VERSION2              = 2;

    // Default setting
    public const int SIAR1_ID                        = 1;                   // siar ID: 1
    public const int SIAR2_ID                        = 2;                   // siar ID: 2
    public const int BAUDRATE                       = 57600;
    public const string DEVICENAME                  = "COM1";              // Check which port is being used on your controller
                                                                           // ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"

    public const int TORQUE_ENABLE                  = 1;                   // Value for enabling the torque
    public const int TORQUE_DISABLE                 = 0;                   // Value for disabling the torque
    public const int SIAR1_MINIMUM_POSITION_VALUE    = 100;                 // siar will rotate between this value
    public const int SIAR1_MAXIMUM_POSITION_VALUE    = 4000;                // and this value (note that the siar would not move when the position value is out of movable range. Check e-manual about the range of the siar you use.)
    public const int SIAR2_MINIMUM_POSITION_VALUE    = -150000;
    public const int SIAR2_MAXIMUM_POSITION_VALUE    = 150000;
    public const int SIAR1_MOVING_STATUS_THRESHOLD   = 10;                  // siar MX moving status threshold
    public const int SIAR2_MOVING_STATUS_THRESHOLD   = 20;                  // siar PRO moving status threshold

    public const byte ESC_ASCII_VALUE = 0x1b;

    public const int COMM_SUCCESS = 0;                                     // Communication Success result value
    public const int COMM_TX_FAIL = -1001;                                 // Communication Tx Failed

    static void Main(string[] args)
    {
      // Initialize PortHandler Structs
      // Set the port path
      // Get methods and members of PortHandlerLinux or PortHandlerWindows
      int port_num = siar.portHandler(DEVICENAME);

      // Initialize PacketHandler Structs
      siar.packetHandler();

      int index = 0;
      int siar_comm_result = COMM_TX_FAIL;                                  // Communication result
      UInt16[] siar1_goal_position = new UInt16[2]{ SIAR1_MINIMUM_POSITION_VALUE, SIAR1_MAXIMUM_POSITION_VALUE };   // Goal position of siar MX
      int[] siar2_goal_position = new int[2]{ SIAR2_MINIMUM_POSITION_VALUE, SIAR2_MAXIMUM_POSITION_VALUE };         // Goal position of siar PRO

      byte siar_error = 0;                                                  // siar error
      UInt16 siar1_present_position = 0;                                    // Present position of siar MX
      Int32 siar2_present_position = 0;                                     // Present position of siar PRO


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

      // Enable siar#1 torque
      siar.write1ByteTxRx(port_num, PROTOCOL_VERSION1, SIAR1_ID, ADDR_MX_TORQUE_ENABLE, TORQUE_ENABLE);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION1)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION1, siar_comm_result)));
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION1)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION1, siar_error)));
      }
      else
      {
        Console.WriteLine("siar#{0} has been successfully connected ", SIAR1_ID);
      }

      // Enable siar#2 torque
      siar.write1ByteTxRx(port_num, PROTOCOL_VERSION2, SIAR2_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_ENABLE);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION2)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION2, siar_comm_result)));
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION2)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION2, siar_error)));
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
        siar.write2ByteTxRx(port_num, PROTOCOL_VERSION1, SIAR1_ID, ADDR_MX_GOAL_POSITION, siar1_goal_position[index]);
        if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION1)) != COMM_SUCCESS)
        {
          Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION1, siar_comm_result)));
        }
        else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION1)) != 0)
        {
          Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION1, siar_error)));
        }

        // Write siar#2 goal position
        siar.write4ByteTxRx(port_num, PROTOCOL_VERSION2, SIAR2_ID, ADDR_PRO_GOAL_POSITION, (UInt32)siar2_goal_position[index]);
        if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION2)) != COMM_SUCCESS)
        {
          Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION2, siar_comm_result)));
        }
        else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION2)) != 0)
        {
          Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION2, siar_error)));
        }

        do
        {
          // Read siar#1 present position
          siar1_present_position = siar.read2ByteTxRx(port_num, PROTOCOL_VERSION1, SIAR1_ID, ADDR_MX_PRESENT_POSITION);
          if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION1)) != COMM_SUCCESS)
          {
            Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION1, siar_comm_result)));
          }
          else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION1)) != 0)
          {
            Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION1, siar_error)));
          }

          // Read siar#2 present position
          siar2_present_position = (Int32)siar.read4ByteTxRx(port_num, PROTOCOL_VERSION2, SIAR2_ID, ADDR_PRO_PRESENT_POSITION);
          if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION2)) != COMM_SUCCESS)
          {
            Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION2, siar_comm_result)));
          }
          else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION2)) != 0)
          {
            Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION2, siar_error)));
          }

          Console.WriteLine("[ID: {0}] GoalPos: {1}  PresPos: {2} [ID: {3}] GoalPos: {4}  PresPos: {5}", SIAR1_ID, siar1_goal_position[index], siar1_present_position, SIAR2_ID, siar2_goal_position[index], siar2_present_position);

        } while ((Math.Abs(siar1_goal_position[index] - siar1_present_position) > SIAR1_MOVING_STATUS_THRESHOLD) || (Math.Abs(siar2_goal_position[index] - siar2_present_position) > SIAR2_MOVING_STATUS_THRESHOLD));

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
      siar.write1ByteTxRx(port_num, PROTOCOL_VERSION1, SIAR1_ID, ADDR_MX_TORQUE_ENABLE, TORQUE_DISABLE);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION1)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION1, siar_comm_result)));
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION1)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION1, siar_error)));
      }

      // Disable siar#2 Torque
      siar.write1ByteTxRx(port_num, PROTOCOL_VERSION2, SIAR2_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_DISABLE);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION2)) != COMM_SUCCESS)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION2, siar_comm_result)));
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION2)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION2, siar_error)));
      }

      // Close port
      siar.closePort(port_num);

      return;
    }
  }
}
