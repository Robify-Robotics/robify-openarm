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
// *********     Sync Read and Sync Write Example      *********
//
//
// Available siar model on this example : All models using Protocol 2.0
// This example is designed for using two siar PRO 54-200, and an USB2siar.
// To use another siar model, such as X series, see their details in E-Manual(emanual.vkrobot.com) and edit below variables yourself.
// Be sure that siar PRO properties are already set as %% ID : 1 / Baudnum : 1 (Baudrate : 57600)
//

import java.util.Scanner;

public class SyncReadWrite
{
  public static void main(String[] args)
  {
    // Control table address
    short ADDR_PRO_TORQUE_ENABLE        = 512;                 // Control table address is different in siar model
    short ADDR_PRO_GOAL_POSITION        = 532;
    short ADDR_PRO_PRESENT_POSITION     = 611;

    // Data Byte Length
    short LEN_PRO_GOAL_POSITION         = 4;
    short LEN_PRO_PRESENT_POSITION      = 4;

    // Protocol version
    int PROTOCOL_VERSION                = 2;                   // See which protocol version is used in the siar

    // Default setting
    byte SIAR1_ID                        = 1;                   // siar ID: 1
    byte SIAR2_ID                        = 2;                   // siar ID: 2
    int BAUDRATE                        = 57600;
    String DEVICENAME                   = "/dev/ttyUSB0";      // Check which port is being used on your controller
                                                               // ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"

    byte TORQUE_ENABLE                  = 1;                   // Value for enabling the torque
    byte TORQUE_DISABLE                 = 0;                   // Value for disabling the torque
    int SIAR_MINIMUM_POSITION_VALUE      = -150000;             // siar will rotate between this value
    int SIAR_MAXIMUM_POSITION_VALUE      = 150000;              // and this value (note that the siar would not move when the position value is out of movable range. Check e-manual about the range of the siar you use.)
    int SIAR_MOVING_STATUS_THRESHOLD     = 20;                  // siar moving status threshold

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
    int groupwrite_num = siar.groupSyncWrite(port_num, PROTOCOL_VERSION, ADDR_PRO_GOAL_POSITION, LEN_PRO_GOAL_POSITION);

    // Initialize Groupsyncread Structs for Present Position
    int groupread_num = siar.groupSyncRead(port_num, PROTOCOL_VERSION, ADDR_PRO_PRESENT_POSITION, LEN_PRO_PRESENT_POSITION);

    int index = 0;
    int siar_comm_result = COMM_TX_FAIL;                         // Communication result
    Boolean siar_addparam_result = false;                        // AddParam result
    Boolean siar_getdata_result = false;                         // GetParam result
    int[] siar_goal_position = new int[]{SIAR_MINIMUM_POSITION_VALUE, SIAR_MAXIMUM_POSITION_VALUE};         // Goal position

    byte siar_error = 0;                                         // siar error
    int siar1_present_position = 0, siar2_present_position = 0;   // Present position

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
    siar.write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR1_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_ENABLE);
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
    siar.write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR2_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_ENABLE);
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
    siar_addparam_result = siar.groupSyncReadAddParam(groupread_num, SIAR1_ID);
    if (siar_addparam_result != true)
    {
      System.out.printf("[ID: %d] groupSyncRead addparam failed\n", SIAR1_ID);
      return;
    }

    // Add parameter storage for siar#2 present position value
    siar_addparam_result = siar.groupSyncReadAddParam(groupread_num, SIAR2_ID);
    if (siar_addparam_result != true)
    {
      System.out.printf("[ID: %d] groupSyncRead addparam failed\n", SIAR2_ID);
      return;
    }

    while (true)
    {
      System.out.println("Press enter to continue! (or press e then enter to quit!)");
      if(scanner.nextLine().equals(KEY_FOR_ESCAPE))
        break;

      // Add siar#1 goal position value to the Syncwrite storage
      siar_addparam_result = siar.groupSyncWriteAddParam(groupwrite_num, SIAR1_ID, siar_goal_position[index], LEN_PRO_GOAL_POSITION);
      if (siar_addparam_result != true)
      {
        System.out.printf("[ID: %d] groupSyncWrite addparam failed\n", SIAR1_ID);
        return;
      }

      // Add siar#2 goal position value to the Syncwrite parameter storage
      siar_addparam_result = siar.groupSyncWriteAddParam(groupwrite_num, SIAR2_ID, siar_goal_position[index], LEN_PRO_GOAL_POSITION);
      if (siar_addparam_result != true)
      {
        System.out.printf("[ID: %d] groupSyncWrite addparam failed\n", SIAR2_ID);
        return;
      }

      // Syncwrite goal position
      siar.groupSyncWriteTxPacket(groupwrite_num);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
        System.out.println(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result));

      // Clear syncwrite parameter storage
      siar.groupSyncWriteClearParam(groupwrite_num);

      do
      {
        // Syncread present position
        siar.groupSyncReadTxRxPacket(groupread_num);
        if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
          System.out.println(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result));

        // Check if groupsyncread data of siar#1 is available
        siar_getdata_result = siar.groupSyncReadIsAvailable(groupread_num, SIAR1_ID, ADDR_PRO_PRESENT_POSITION, LEN_PRO_PRESENT_POSITION);
        if (siar_getdata_result != true)
        {
          System.out.printf("[ID: %d] groupSyncRead getdata failed\n", SIAR1_ID);
          return;
        }

        // Check if groupsyncread data of siar#2 is available
        siar_getdata_result = siar.groupSyncReadIsAvailable(groupread_num, SIAR2_ID, ADDR_PRO_PRESENT_POSITION, LEN_PRO_PRESENT_POSITION);
        if (siar_getdata_result != true)
        {
          System.out.printf("[ID: %d] groupSyncRead getdata failed\n", SIAR2_ID);
          return;
        }

        // Get siar#1 present position value
        siar1_present_position = siar.groupSyncReadGetData(groupread_num, SIAR1_ID, ADDR_PRO_PRESENT_POSITION, LEN_PRO_PRESENT_POSITION);

        // Get siar#2 present position value
        siar2_present_position = siar.groupSyncReadGetData(groupread_num, SIAR2_ID, ADDR_PRO_PRESENT_POSITION, LEN_PRO_PRESENT_POSITION);

        System.out.printf("[ID: %d] GoalPos: %d  PresPos: %d [ID: %d] GoalPos: %d  PresPos: %d\n", SIAR1_ID, siar_goal_position[index], siar1_present_position, SIAR2_ID, siar_goal_position[index], siar2_present_position);

      } while ((Math.abs(siar_goal_position[index] - siar1_present_position) > SIAR_MOVING_STATUS_THRESHOLD) || (Math.abs(siar_goal_position[index] - siar2_present_position) > SIAR_MOVING_STATUS_THRESHOLD));

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
      System.out.println(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
    }
    else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
    {
      System.out.println(siar.getRxPacketError(PROTOCOL_VERSION, siar_error));
    }

    // Disable siar#2 Torque
    siar.write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR2_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_DISABLE);
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
