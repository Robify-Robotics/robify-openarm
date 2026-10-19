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
// *********     Bulk Read Example      *********
//
//
// Available siar model on this example : MX or X series set to Protocol 1.0
// This example is designed for using two siar MX-28, and an USB2siar.
// To use another siar model, such as X series, see their details in E-Manual(emanual.vkrobot.com) and edit below variables yourself.
// Be sure that siar MX properties are already set as %% ID : 1 / Baudnum : 34 (Baudrate : 57600)
//

import java.util.Scanner;

public class BulkRead
{
  public static void main(String[] args)
  {
    // Control table address
    short ADDR_MX_TORQUE_ENABLE         = 24;                  // Control table address is different in siar model
    short ADDR_MX_GOAL_POSITION         = 30;
    short ADDR_MX_PRESENT_POSITION      = 36;
    short ADDR_MX_MOVING                = 46;

    // Data Byte Length
    short LEN_MX_GOAL_POSITION          = 2;
    short LEN_MX_PRESENT_POSITION       = 2;
    short LEN_MX_MOVING                 = 1;

    // Protocol version
    int PROTOCOL_VERSION                = 1;                   // See which protocol version is used in the siar

    // Default setting
    byte SIAR1_ID                        = 1;                   // siar ID: 1
    byte SIAR2_ID                        = 2;                   // siar ID: 2
    int BAUDRATE                        = 57600;
    String DEVICENAME                   = "/dev/ttyUSB0";      // Check which port is being used on your controller
                                                               // ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"

    byte TORQUE_ENABLE                  = 1;                   // Value for enabling the torque
    byte TORQUE_DISABLE                 = 0;                   // Value for disabling the torque
    short SIAR_MINIMUM_POSITION_VALUE    = 100;                 // siar will rotate between this value
    short SIAR_MAXIMUM_POSITION_VALUE    = 4000;                // and this value (note that the siar would not move when the position value is out of movable range. Check e-manual about the range of the siar you use.)
    int SIAR_MOVING_STATUS_THRESHOLD     = 10;                  // siar moving status threshold

    String KEY_FOR_ESCAPE               = "e";                 // Key for escape

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

    // Initialize Groupsyncwrite instance
    int group_num = siar.groupBulkRead(port_num, PROTOCOL_VERSION);

    int index = 0;
    int siar_comm_result = COMM_TX_FAIL;                         // Communication result
    Boolean siar_addparam_result = false;                        // AddParam result
    Boolean siar_getdata_result = false;                         // GetParam result
    short[] siar_goal_position = new short[]{SIAR_MINIMUM_POSITION_VALUE, SIAR_MAXIMUM_POSITION_VALUE};         // Goal position

    byte siar_error = 0;                                         // siar error
    short siar1_present_position = 0;                            // Present position
    byte siar2_moving = 0;                                       // siar moving status

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

    // Enable siar#1 Torque
    siar.write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR1_ID, ADDR_MX_TORQUE_ENABLE, TORQUE_ENABLE);
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
      System.out.printf("siar#%d has been successfully connected\n", SIAR1_ID);
    }

    // Enable siar#2 Torque
    siar.write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR2_ID, ADDR_MX_TORQUE_ENABLE, TORQUE_ENABLE);
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
      System.out.printf("siar#%d has been successfully connected\n", SIAR2_ID);
    }

    // Add parameter storage for siar#1 present position value
    siar_addparam_result = siar.groupBulkReadAddParam(group_num, SIAR1_ID, ADDR_MX_PRESENT_POSITION, LEN_MX_PRESENT_POSITION);
    if (siar_addparam_result != true)
    {
      System.out.printf("[ID: %d] groupBulkRead addparam failed\n", SIAR1_ID);
      return;
    }

    // Add parameter storage for siar#2 present moving value
    siar_addparam_result = siar.groupBulkReadAddParam(group_num, SIAR2_ID, ADDR_MX_MOVING, LEN_MX_MOVING);
    if (siar_addparam_result != true)
    {
      System.out.printf("[ID: %d] groupBulkRead addparam failed\n", SIAR2_ID);
      return;
    }

    while (true)
    {
      System.out.println("Press enter to continue! (or press e then enter to quit!)");
      if(scanner.nextLine().equals(KEY_FOR_ESCAPE))
        break;

      // Write siar#1 goal position
      siar.write2ByteTxRx(port_num, PROTOCOL_VERSION, SIAR1_ID, ADDR_MX_GOAL_POSITION, siar_goal_position[index]);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        printf("%s\n", siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
         printf("%s\n", siar.getRxPacketError(PROTOCOL_VERSION, siar_error));
      }

      // Write siar#2 goal position
      siar.write2ByteTxRx(port_num, PROTOCOL_VERSION, SIAR2_ID, ADDR_MX_GOAL_POSITION, siar_goal_position[index]);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
         printf("%s\n", siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
         printf("%s\n", siar.getRxPacketError(PROTOCOL_VERSION, siar_error));
      }

      do
      {
        // Bulkread present position and moving status
        siar.groupBulkReadTxRxPacket(group_num);
        if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
          System.out.println(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result));

        siar_getdata_result = siar.groupBulkReadIsAvailable(group_num, SIAR1_ID, ADDR_MX_PRESENT_POSITION, LEN_MX_PRESENT_POSITION);
        if (siar_getdata_result != true)
        {
          System.out.printf("[ID: %d] groupBulkRead getdata failed\n", SIAR1_ID);
          return;
        }

        siar_getdata_result = siar.groupBulkReadIsAvailable(group_num, SIAR2_ID, ADDR_MX_MOVING, LEN_MX_MOVING);
        if (siar_getdata_result != true)
        {
          System.out.printf("[ID: %d] groupBulkRead getdata failed\n", SIAR2_ID);
          return;
        }

        // Get siar#1 present position value
        siar1_present_position = (short)siar.groupBulkReadGetData(group_num, SIAR1_ID, ADDR_MX_PRESENT_POSITION, LEN_MX_PRESENT_POSITION);

        // Get siar#2 moving status value
        siar2_moving = (byte)siar.groupBulkReadGetData(group_num, SIAR2_ID, ADDR_MX_MOVING, LEN_MX_MOVING);

        System.out.printf("[ID: %d] Present Position : %d [ID: %d] Is Moving : %d\n", SIAR1_ID, siar1_present_position, SIAR2_ID, siar2_moving);

      } while (Math.abs(siar_goal_position[index] - siar1_present_position) > SIAR_MOVING_STATUS_THRESHOLD);

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
    siar.write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR1_ID, ADDR_MX_TORQUE_ENABLE, TORQUE_DISABLE);
    if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
    {
      System.out.println(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
    }
    else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
    {
      System.out.println(siar.getRxPacketError(PROTOCOL_VERSION, siar_error));
    }

    // Disable siar#2 Torque
    siar.write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR2_ID, ADDR_MX_TORQUE_ENABLE, TORQUE_DISABLE);
    if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
    {
      System.out.println(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
    }
    else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
    {
      System.out.println(siar.getRxPacketError(PROTOCOL_VERSION, siar_error));
    }

    // Close port
    siar.closePort(port_num);

    return;
  }
}
