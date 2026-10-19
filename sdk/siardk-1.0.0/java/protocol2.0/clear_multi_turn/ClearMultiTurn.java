/*******************************************************************************
* Copyright 2018 VKROBOT CO., LTD.
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
// Available siar model on this example : MX with Protocol 2.0 (firmware v42 or above), siar X-series (firmware v42 or above)
// This example is designed for using a siar XM430-W350-R, and an U2D2.
// To use another siar model, such as MX series, see their details in E-Manual(emanual.vkrobot.com) and edit below "#define"d variables yourself.
// Be sure that siar properties are already set as %% ID : 1 / Baudnum : 1 (Baudrate : 57600)
//

import java.io.IOException;
import java.util.Scanner;

public class ClearMultiTurn
{	
  public static void main(String[] args) throws InterruptedException, IOException
  {  
    // Control table address
    short ADDR_OPERATING_MODE           = 11;				   // Control table address is different in siar model
    short ADDR_TORQUE_ENABLE            = 64;                
    short ADDR_GOAL_POSITION            = 116;
    short ADDR_PRESENT_POSITION         = 132;

    // Protocol version
    int PROTOCOL_VERSION                = 2;                   // See which protocol version is used in the siar

    // Default setting
    byte SIAR_ID                         = 1;                   // siar ID: 1
    int BAUDRATE                        = 57600;
    String DEVICENAME                   = "COM1";              // Check which port is being used on your controller
                                                               // ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"

    byte TORQUE_ENABLE                  = 1;                   // Value for enabling the torque
    byte TORQUE_DISABLE                 = 0;                   // Value for disabling the torque
    int MAX_POSITION_VALUE              = 1048575;             
    int SIAR_MOVING_STATUS_THRESHOLD     = 20;                  // siar moving status threshold
    byte EXT_POSITION_CONTROL_MODE      = 4;                   //  Value for extended position control mode (operating mode)
    

    String KEY_FOR_ESCAPE               = "e";                 // Key for escape
    String KEY_FOR_CONTINUE             = " ";				   // key for continue
    
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

    int siar_comm_result = COMM_TX_FAIL;                        // Communication result
    
    byte siar_error = 0;                                        // siar error
    int siar_present_position = 0;                              // Present position

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
    
    // Set operating mode to extended position control mode
    siar.write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_OPERATING_MODE, EXT_POSITION_CONTROL_MODE);
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
      System.out.println("Operating mode changed to extended position control mode. \n");
    }   
    
    // Enable siar Torque
    siar.write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_TORQUE_ENABLE, TORQUE_ENABLE);
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
      System.out.println("siar has been successfully connected");
    }

    while (true)
    {
      System.out.println("\nPress enter to continue! (or press e then enter to quit!)");
          
      String ch = scanner.nextLine();
      if (ch.equals("e"))
        break;
      
	  System.out.println("\nPress SPACE key to clear multi-turn information! (or press ESC to stop!)\n");

	  // Write goal position
      siar.write4ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_GOAL_POSITION, MAX_POSITION_VALUE);
      if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        System.out.println(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
      }
      else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
      {
        System.out.println(siar.getRxPacketError(PROTOCOL_VERSION, siar_error));
      }

      do
      {
        // Read present position
        siar_present_position = siar.read4ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRESENT_POSITION);
        if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
        {
          System.out.println(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
        }
        else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
        {
          System.out.println(siar.getRxPacketError(PROTOCOL_VERSION, siar_error));
        }
        
        System.out.printf("  [ID: %d] GoalPos:%d  PresPos:%d\n", SIAR_ID, MAX_POSITION_VALUE, siar_present_position);

        if (System.in.available() > 0)
        {
    	 
          String c = scanner.nextLine(); 	
          if(c.equals(KEY_FOR_CONTINUE))
          {
            System.out.printf("  Stop & Clear Multi-Turn Information! \n");   
            
            // Write the present position to the goal position to stop moving 
            siar.write4ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_GOAL_POSITION, siar_present_position);
            if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
            {
              System.out.println(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
            }
            else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
            {
              System.out.println(siar.getRxPacketError(PROTOCOL_VERSION, siar_error));
            }
            
            Thread.sleep(300);
            
            // Clear Multi-Turn Information
            siar.clearMultiTurn(port_num, PROTOCOL_VERSION, SIAR_ID);
            if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
            {
              System.out.println(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
            }
            else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
            {
              System.out.println(siar.getRxPacketError(PROTOCOL_VERSION, siar_error));
            }
            
            // Read present position
            siar_present_position = siar.read4ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRESENT_POSITION);
            if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
            {
              System.out.println(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
            }
            else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
            {
              System.out.println(siar.getRxPacketError(PROTOCOL_VERSION, siar_error));
            }
            
            System.out.printf("  Present Position has been reset. : %03d \n", siar_present_position); 
            
            break;
          }
          
          else if (c.equals(KEY_FOR_ESCAPE))
          {
        	System.out.printf("\n  Stopped!! ");
        	  
        	// Write the present position to the goal position to stop moving
        	siar.write4ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_GOAL_POSITION, siar_present_position);
            if ((siar_comm_result = siar.getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
            {
              System.out.println(siar.getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
            }
            else if ((siar_error = siar.getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
            {
              System.out.println(siar.getRxPacketError(PROTOCOL_VERSION, siar_error));
            }
              
            break;
          }
        }
        
      } while ((Math.abs(MAX_POSITION_VALUE - siar_present_position) > SIAR_MOVING_STATUS_THRESHOLD));
    }

    // Disable siar Torque
    siar.write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_TORQUE_ENABLE, TORQUE_DISABLE);
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