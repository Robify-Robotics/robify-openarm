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
// *********     Factory Reset Example      *********
//
//
// Available siar model on this example : All models using Protocol 2.0
// This example is designed for using a siar PRO 54-200, and an USB2siar.
// To use another siar model, such as X series, see their details in E-Manual(emanual.vkrobot.com) and edit below variables yourself.
// Be sure that siar PRO properties are already set as %% ID : 1 / Baudnum : 1 (Baudrate : 57600)
//

// Be aware that:
// This example resets all properties of siar to default values, such as %% ID : 1 / Baudnum : 34 (Baudrate : 57600)
//

using System;
using System.Runtime.InteropServices;
using System.Threading;
using siardk;

namespace factory_reset
{
  class FactoryReset
  {
    // Control table address
    public const int ADDR_PRO_BAUDRATE  = 8;                // Control table address is different in siar model

    // Protocol version
    public const int PROTOCOL_VERSION   = 2;                // See which protocol version is used in the siar

    // Default setting
    public const int SIAR_ID             = 1;                // siar ID: 1
    public const int BAUDRATE           = 1000000;
    public const string DEVICENAME      = "COM1";           // Check which port is being used on your controller
                                                            // ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"

    public const int FACTORYRST_DEFAULTBAUDRATE = 57600;    // siar baudrate set by factoryreset
    public const int NEW_BAUDNUM        = 3;                // New baudnum to recover siar baudrate as it was
    public const byte OPERATION_MODE    = 0x01;             // 0xFF : reset all values
                                                            // 0x01 : reset all values except ID
                                                            // 0x02 : reset all values except ID and baudrate

    public const int COMM_SUCCESS       = 0;                // Communication Success result value
    public const int COMM_TX_FAIL       = -1001;            // Communication Tx Failed

    static void Main(string[] args)
    {
      // Initialize PortHandler Structs
      // Set the port path
      // Get methods and members of PortHandlerLinux or PortHandlerWindows
      int port_num = siar.portHandler(DEVICENAME);

      // Initialize PacketHandler Structs
      siar.packetHandler();

      int siar_comm_result = COMM_TX_FAIL;                  // Communication result

      byte siar_error = 0;                                  // siar error
      byte siar_baudnum_read;                               // Read baudnum

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


      // Read present baudrate of the controller
      Console.WriteLine("Now the controller baudrate is : {0}", siar.getBaudRate(port_num));

      // Try factoryreset
      Console.WriteLine("[ID: {0}] Try factoryreset : ", SIAR_ID);
      siar.factoryReset(port_num, PROTOCOL_VERSION, SIAR_ID, OPERATION_MODE);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        Console.WriteLine("Aborted");
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result)));
        return;
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        Console.WriteLine(Marshal.PtrToStringAnsi(siar.getRxPacketError(PROTOCOL_VERSION, siar_error)));
      }

      // Wait for reset
      Console.WriteLine("Wait for reset...");
      Thread.Sleep(2000);

      Console.WriteLine("[ID: {0}] factoryReset Success!", SIAR_ID);

      // Set controller baudrate to siar default baudrate
      if (siar.setBaudRate(port_num, FACTORYRST_DEFAULTBAUDRATE))
      {
        Console.WriteLine("Succeed to change the controller baudrate to : {0}", FACTORYRST_DEFAULTBAUDRATE);
      }
      else
      {
        Console.WriteLine("Failed to change the controller baudrate");
        Console.WriteLine("Press any key to terminate...");
        Console.ReadKey();
        return;
      }

      // Read siar baudnum
      siar_baudnum_read = siar.read1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRO_BAUDRATE);
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
        Console.WriteLine("[ID: {0}] siar baudnum is now : {1}", SIAR_ID, siar_baudnum_read);
      }

      // Write new baudnum
      siar.write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRO_BAUDRATE, NEW_BAUDNUM);
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
        Console.WriteLine("[ID: {0}] Set siar baudnum to : {1}", SIAR_ID, NEW_BAUDNUM);
      }

      // Set port baudrate to BAUDRATE
      if (siar.setBaudRate(port_num, BAUDRATE))
      {
        Console.WriteLine("Succeed to change the controller baudrate to : {0}", BAUDRATE);
      }
      else
      {
        Console.WriteLine("Failed to change the controller baudrate");
        Console.WriteLine("Press any key to terminate...");
        Console.ReadKey();
        return;
      }

      Thread.Sleep(200);

      // Read siar baudnum
      siar_baudnum_read = siar.read1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRO_BAUDRATE);
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
        Console.WriteLine("[ID: {0}] siar Baudnum is now : {1}", SIAR_ID, siar_baudnum_read);
      }

      // Close port
      siar.closePort(port_num);

      return;
    }
  }
}
