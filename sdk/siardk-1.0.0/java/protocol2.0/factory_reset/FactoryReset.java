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

import java.util.Scanner;

public class FactoryReset
{
  public static void main(String[] args)
  {
    // Control table address
    short ADDR_PRO_BAUDRATE             = 8;                   // Control table address is different in siar model

    // Protocol version
    int PROTOCOL_VERSION                = 2;                   // See which protocol version is used in the siar

    // Default setting
    byte SIAR_ID                         = 1;                   // siar ID: 1-
    int BAUDRATE                        = 57600;
    String DEVICENAME                   = "/dev/ttyUSB0";      // Check which port is being used on your controller
                                                               // ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"

    int FACTORYRST_DEFAULTBAUDRATE      = 57600;               // siar baudrate set by factoryreset
    byte NEW_BAUDNUM                    = 3;                   // New baudnum to recover siar baudrate as it was
    byte OPERATION_MODE                 = 0x01;                // 0xFF : reset all values
                                                               // 0x01 : reset all values except ID
                                                               // 0x02 : reset all values except ID and baudrate

    int COMM_SUCCESS                    = 0;                   // Communication Success result value
    int COMM_TX_FAIL                    = -1001;               // Communication Tx Failed

    // Instead of getch
    Scanner scanner = new Scanner(System.in);

    // Initialize siar class for java
    SIAR sair = new SIAR();

    // Initialize PortHandler Structs
    // Set the port path
    // Get methods and members of PortHandlerLinux or PortHandlerWindows
    int port_num = siar.portHandler(DEVICENAME);

    // Initialize PacketHandler Structs
    siar.packetHandler();

    int siar_comm_result = COMM_TX_FAIL;                      // Communication result

    byte siar_error = 0;                                      // siar error
    byte siar_baudnum_read;                                   // Read baudnum

    // Open port
    if (siar.openPort(port_num))
    {
      System.out.println("Succeeded to open the port!");
    }
    else
    {
      System.out.println("Failed to open the port!");
      System.out.println("Press any key to terminate...");
      scanner.nextLine();
      return;
    }

    // Set port baudrate
    if (siar.setBaudRate(port_num, BAUDRATE))
    {
      System.out.println("Succeeded to change the baudrate!");
    }
    else
    {
      System.out.println("Failed to change the baudrate!");
      System.out.println("Press any key to terminate...");
      scanner.nextLine();
      return;
    }

    // Read present baudrate of the controller
    System.out.printf("Now the controller baudrate is : %d\n", siar.getBaudRate(port_num));

    // Try factoryreset
    System.out.printf("[ID: %d] Try factoryreset : \n", SIAR_ID);
    siar.factoryReset(port_num, PROTOCOL_VERSION, SIAR_ID, OPERATION_MODE);
    if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
    {
      System.out.println("Aborted");
      System.out.println(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
      return;
    }
    else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
    {
      System.out.println(siar.getRxPacketError(PROTOCOL_VERSION, siar_error));
    }

    // Wait for reset
    System.out.printf("Wait for reset...\n");
    try
    {
      Thread.sleep(2000);
    }
    catch (InterruptedException e)
    {
      System.out.println(e.getMessage());
    }

    System.out.printf("[ID: %d] factoryReset Success!\n", SIAR_ID);

    // Set controller baudrate to siar default baudrate
    if (siar.setBaudRate(port_num, FACTORYRST_DEFAULTBAUDRATE))
    {
      System.out.printf("Succeed to change the controller baudrate to : %d\n", FACTORYRST_DEFAULTBAUDRATE);
    }
    else
    {
      System.out.println("Failed to change the controller baudrate");
      System.out.println("Press any key to terminate...");
      scanner.nextLine();
      return;
    }

    // Read siar baudnum
    siar_baudnum_read = siar.read1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRO_BAUDRATE);
    if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
    {
      System.out.println(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
    }
    else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
    {
      System.out.println(siar.getRxPacketError(PROTOCOL_VERSION, siar_error));
    }
    else
    {
      System.out.printf("[ID: %d] siar baudnum is now : %d\n", SIAR_ID, siar_baudnum_read);
    }

    // Write new baudnum
    siar.write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRO_BAUDRATE, NEW_BAUDNUM);
    if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
    {
      System.out.println(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
    }
    else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
    {
      System.out.println(siar.getRxPacketError(PROTOCOL_VERSION, siar_error));
    }
    else
    {
      System.out.printf("[ID: %d] Set siar baudnum to : %d\n", SIAR_ID, NEW_BAUDNUM);
    }

    // Set port baudrate to BAUDRATE
    if (siar.setBaudRate(port_num, BAUDRATE))
    {
      System.out.printf("Succeed to change the controller baudrate to : %d\n", BAUDRATE);
    }
    else
    {
      System.out.println("Failed to change the controller baudrate");
      System.out.println("Press any key to terminate...");
      scanner.nextLine();
      return;
    }

    try
    {
      Thread.sleep(200);
    }
    catch (InterruptedException e)
    {
      System.out.println(e.getMessage());
    }

    // Read siar baudnum
    siar_baudnum_read = siar.read1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRO_BAUDRATE);
    if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
    {
      System.out.println(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
    }
    else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
    {
      System.out.println(siar.getRxPacketError(PROTOCOL_VERSION, siar_error));
    }
    else
    {
      System.out.printf("[ID: %d] siar Baudnum is now : %d", SIAR_ID, siar_baudnum_read);
    }

    return;
  }
}
