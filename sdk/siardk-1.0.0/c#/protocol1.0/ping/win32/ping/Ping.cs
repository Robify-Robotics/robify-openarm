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
// *********     Ping Example      *********
//
//
// Available siar model on this example : All models using Protocol 1.0
// This example is designed for using a siar MX-28, and an USB2siar.
// To use another siar model, such as X series, see their details in E-Manual(emanual.vkrobot.com) and edit below variables yourself.
// Be sure that siar MX properties are already set as %% ID : 1 / Baudnum : 34 (Baudrate : 57600)
//

using System;
using System.Runtime.InteropServices;
using siardk;

namespace ping
{
  class Ping
  {
    // Protocol version
    public const int PROTOCOL_VERSION                = 1;                   // See which protocol version is used in the siar

    // Default setting
    public const int SIAR_ID                          = 1;                   // siar ID: 1
    public const int BAUDRATE                        = 1000000;
    public const string DEVICENAME                   = "COM1";              // Check which port is being used on your controller
                                                                            // ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"

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

      int siar_comm_result = COMM_TX_FAIL;                                   // Communication result

      byte siar_error = 0;                                                   // siar error
      UInt16 siar_model_number;                                              // siar model number

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

      // Try to ping the siar
      // Get siar model number
      siar_model_number = siar.pingGetModelNum(port_num, PROTOCOL_VERSION, SIAR_ID);
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
        Console.WriteLine("[ID: {0}] ping Succeeded. siar model number : {1}", SIAR_ID, siar_model_number);
      }
      
      // Close port
      siar.closePort(port_num);

      return;
    }
  }
}
