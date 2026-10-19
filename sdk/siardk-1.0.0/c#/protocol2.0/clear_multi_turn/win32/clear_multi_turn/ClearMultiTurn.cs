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

/* Author: Ki Jong Gil (Gilbert) */

//
// *********     Clear Multi-Turn Example      *********
//
//
// Available siar model on this example : siar X-series (firmware v42 or above)
// This example is designed for using a siar XM430-W350-R, and an U2D2.
// To use another siar model, such as MX series, see their details in E-Manual(emanual.vkrobot.com) and edit below "#define"d variables yourself.
// Be sure that siar properties are already set as %% ID : 1 / Baudnum : 1 (Baudrate : 57600)
//

using System;
using System.Runtime.InteropServices;
using siardk;

namespace clear_multi_turn
{
  class ClearMultiTurn
  {
    // Control table address
    public const int ADDR_OPERATING_MODE         = 11;                 // Control table address is different in siar model
    public const int ADDR_TORQUE_ENABLE          = 64;                
    public const int ADDR_GOAL_POSITION          = 116;
    public const int ADDR_PRESENT_POSITION       = 132;

    // Protocol version
    public const int PROTOCOL_VERSION            = 2;                  // See which protocol version is used in the siar

    // Default setting
    public const int SIAR_ID                      = 1;                  // siar ID: 1
    public const int BAUDRATE                    = 1000000;
    public const string DEVICENAME               = "COM1";             // Check which port is being used on your controller
                                                                        // ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"

    public const int TORQUE_ENABLE               = 1;                  // Value for enabling the torque
    public const int TORQUE_DISABLE              = 0;                  // Value for disabling the torque
    public const int MAX_POSITION_VALUE          = 1048575;            // 
    public const int SIAR_MOVING_STATUS_THRESHOLD = 20;                 // siar moving status threshold
    public const int EXT_POSITION_CONTROL_MODE   = 4;                  // Value for extended position control mode (operating mode)

    public const byte ESC_ASCII_VALUE            = 0x1b;
    public const byte SPACE_ASCII_VALUE          = 0x20;

    public const int COMM_SUCCESS = 0;                                 // Communication Success result value
    public const int COMM_TX_FAIL = -1001;                             // Communication Tx Failed

    static void Main(string[] args)
    {
      int port_num = siar.portHandler(DEVICENAME);

      // Initialize PacketHandler Structs
      siar.packetHandler();

      int index = 0;
      int siar_comm_result = COMM_TX_FAIL;                            // Communication result

      byte siar_error = 0;                                            // siar error
      Int32 siar_present_position = 0;                                // Present position

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

      siar.write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_OPERATING_MODE, EXT_POSITION_CONTROL_MODE);
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
        Console.WriteLine("Operating mode changed to extended position control mode.");
      }

      // Enable siar Torque
      siar.write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_TORQUE_ENABLE, TORQUE_ENABLE);
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

      while (true)
      {
        Console.WriteLine("Press any key to continue! (or press ESC to quit!)");
        if (Console.ReadKey(true).KeyChar == ESC_ASCII_VALUE)
            break;

        Console.WriteLine("  Press SPACE key to clear multi-turn information! (or press ESC to stop!)");

        // Write goal position
        siar.write4ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_GOAL_POSITION, MAX_POSITION_VALUE);
        if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
        {
            Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
        }
        else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
        {
            Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
        }

        do
        {
          // Read present position
          siar_present_position = (Int32)siar.read4ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRESENT_POSITION);
          if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
          {
            Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
          }
          else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
          {
            Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
          }

          Console.Write(string.Format("  [ID: {0}] GoalPos: {1}  PresPos: {2}", SIAR_ID, MAX_POSITION_VALUE, siar_present_position, Environment.NewLine));
          Console.Write("\r".PadLeft(Console.WindowWidth - Console.CursorLeft - 1));
          if (Console.KeyAvailable)
          {
            char c = Console.ReadKey().KeyChar;
            if (c == SPACE_ASCII_VALUE)
            {
              Console.WriteLine("\n  Stop & Clear Multi-Turn Information!");

             // Write the present position to the goal position to stop moving
              siar.write4ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_GOAL_POSITION, (UInt32)siar_present_position);
              if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
              {
                Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
              }
              else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
              {
                Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
              }

              System.Threading.Thread.Sleep(300);

              // Clear Multi-Turn Information
              siar.clearMultiTurn(port_num, PROTOCOL_VERSION, SIAR_ID);
              if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
              {
                Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
              }
              else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
              {
                Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
              }

              // Read present position
              siar_present_position = (Int32)siar.read4ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRESENT_POSITION);
              if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
              {
                Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
              }
              else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
              {
                Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
              }

              Console.WriteLine("  Present Position has been reset. : {0} \n", siar_present_position);

              break;
          }
          else if ( c == ESC_ASCII_VALUE)
          {
            Console.WriteLine("\n  Stopped!! \n");

            // Write the present position to the goal position to stop moving
            siar.write4ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_GOAL_POSITION, (UInt32)siar_present_position);
            if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
            {
                Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
            }
            else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
            {
                Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
            }

              break;
          }
        }
      } while ((Math.Abs(MAX_POSITION_VALUE - siar_present_position) > SIAR_MOVING_STATUS_THRESHOLD));
    }

    // Disable siar Torque
    siar.write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_TORQUE_ENABLE, TORQUE_DISABLE);
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