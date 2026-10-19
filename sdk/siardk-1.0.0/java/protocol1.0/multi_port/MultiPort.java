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

import java.util.Scanner;

public class MultiPort
{
  public static void main(String[] args)
  {
    // Control table address
    short ADDR_MX_TORQUE_ENABLE         = 24;                  // Control table address is different in siar model
    short ADDR_MX_GOAL_POSITION         = 30;
    short ADDR_MX_PRESENT_POSITION      = 36;

    // Protocol version
    int PROTOCOL_VERSION                = 1;                   // See which protocol version is used in the siar

    // Default setting
    byte SIAR1_ID                        = 1;                   // siar ID: 1
    byte SIAR2_ID                        = 2;                   // siar ID: 2
    int BAUDRATE                        = 57600;
    String DEVICENAME1                   = "/dev/ttyUSB0";     // Check which port is being used on your controller
    String DEVICENAME2                   = "/dev/ttyUSB1";     // ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"

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
    int port_num1 = siar.portHandler(DEVICENAME1);
    int port_num2 = siar.portHandler(DEVICENAME2);

    // Initialize PacketHandler Structs
    siar.packetHandler();

    int index = 0;
    int siar_comm_result = COMM_TX_FAIL;                         // Communication result
    short[] siar_goal_position = new short[]{SIAR_MINIMUM_POSITION_VALUE, SIAR_MAXIMUM_POSITION_VALUE};         // Goal position

    byte siar_error = 0;                                         // siar error
    short siar1_present_position = 0, siar2_present_position = 0; // Present position

    // Open port1
    if (siar.openPort(port_num1))
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

    // Open port2
    if (siar.openPort(port_num2))
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


    // Set port1 baudrate
    if (siar.setBaudRate(port_num1, BAUDRATE))
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

    // Set port2 baudrate
    if (siar.setBaudRate(port_num1, BAUDRATE))
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
    siar.write1ByteTxRx(port_num1, PROTOCOL_VERSION, SIAR1_ID, ADDR_MX_TORQUE_ENABLE, TORQUE_ENABLE);
    if ((siar_comm_result = siar.getLastTxRxResult(port_num1, PROTOCOL_VERSION)) != COMM_SUCCESS)
    {
      System.out.println(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
    }
    else if ((siar_error = siar.getLastRxPacketError(port_num1, PROTOCOL_VERSION)) != 0)
    {
      System.out.println(siar.getRxPacketError(PROTOCOL_VERSION, siar_error));
    }
    else
    {
      System.out.printf("siar#%d has been successfully connected\n", SIAR1_ID);
    }

    // Enable siar#2 Torque
    siar.write1ByteTxRx(port_num2, PROTOCOL_VERSION, SIAR2_ID, ADDR_MX_TORQUE_ENABLE, TORQUE_ENABLE);
    if ((siar_comm_result = siar.getLastTxRxResult(port_num2, PROTOCOL_VERSION)) != COMM_SUCCESS)
    {
      System.out.println(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
    }
    else if ((siar_error = siar.getLastRxPacketError(port_num2, PROTOCOL_VERSION)) != 0)
    {
      System.out.println(siar.getRxPacketError(PROTOCOL_VERSION, siar_error));
    }
    else
    {
      System.out.printf("siar#%d has been successfully connected\n", SIAR2_ID);
    }

    while (true)
    {
      System.out.println("Press enter to continue! (or press e then enter to quit!)");
      if(scanner.nextLine().equals(KEY_FOR_ESCAPE))
        break;

      // Write siar#1 goal position
      siar.write2ByteTxRx(port_num1, PROTOCOL_VERSION, SIAR1_ID, ADDR_MX_GOAL_POSITION, siar_goal_position[index]);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num1, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        System.out.println(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num1, PROTOCOL_VERSION)) != 0)
      {
        System.out.println(siar.getRxPacketError(PROTOCOL_VERSION, siar_error));
      }

      // Write siar#2 goal position
      siar.write2ByteTxRx(port_num2, PROTOCOL_VERSION, SIAR2_ID, ADDR_MX_GOAL_POSITION, siar_goal_position[index]);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num2, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        System.out.println(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num2, PROTOCOL_VERSION)) != 0)
      {
        System.out.println(siar.getRxPacketError(PROTOCOL_VERSION, siar_error));
      }

      do
      {
        // Read siar#1 present position
        siar1_present_position = siar.read2ByteTxRx(port_num1, PROTOCOL_VERSION, SIAR1_ID, ADDR_MX_PRESENT_POSITION);
        if ((siar_comm_result = siar.getLastTxRxResult(port_num1, PROTOCOL_VERSION)) != COMM_SUCCESS)
        {
          System.out.println(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
        }
        else if ((siar_error = siar.getLastRxPacketError(port_num1, PROTOCOL_VERSION)) != 0)
        {
          System.out.println(siar.getRxPacketError(PROTOCOL_VERSION, siar_error));
        }

        // Read siar#2 present position
        siar2_present_position = siar.read2ByteTxRx(port_num2, PROTOCOL_VERSION, SIAR2_ID, ADDR_MX_PRESENT_POSITION);
        if ((siar_comm_result = siar.getLastTxRxResult(port_num2, PROTOCOL_VERSION)) != COMM_SUCCESS)
        {
          System.out.println(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
        }
        else if ((siar_error = siar.getLastRxPacketError(port_num2, PROTOCOL_VERSION)) != 0)
        {
          System.out.println(siar.getRxPacketError(PROTOCOL_VERSION, siar_error));
        }

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
    siar.write1ByteTxRx(port_num1, PROTOCOL_VERSION, SIAR1_ID, ADDR_MX_TORQUE_ENABLE, TORQUE_DISABLE);
    if ((siar_comm_result = siar.getLastTxRxResult(port_num1, PROTOCOL_VERSION)) != COMM_SUCCESS)
    {
      System.out.println(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
    }
    else if ((siar_error = siar.getLastRxPacketError(port_num1, PROTOCOL_VERSION)) != 0)
    {
      System.out.println(siar.getRxPacketError(PROTOCOL_VERSION, siar_error));
    }

    // Disable siar#2 Torque
    siar.write1ByteTxRx(port_num2, PROTOCOL_VERSION, SIAR2_ID, ADDR_MX_TORQUE_ENABLE, TORQUE_DISABLE);
    if ((siar_comm_result = siar.getLastTxRxResult(port_num2, PROTOCOL_VERSION)) != COMM_SUCCESS)
    {
      System.out.println(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
    }
    else if ((siar_error = siar.getLastRxPacketError(port_num2, PROTOCOL_VERSION)) != 0)
    {
      System.out.println(siar.getRxPacketError(PROTOCOL_VERSION, siar_error));
    }

    // Close port1
    siar.closePort(port_num1);

    // Close port2
    siar.closePort(port_num2);

    return;
  }
}
