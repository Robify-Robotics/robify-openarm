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

/* Author: RWJ(L) */

//
// *********     MultiPort Example      *********
//
//
// Available siar model on this example : All models using Protocol 2.0
// This example is designed for using two siar PRO 54-200, and two USB2siars.
// To use another siar model, such as X series, see their details in E-Manual(emanual.robotis.com) and edit below "#define"d variables yourself.
// Be sure that siar PRO properties are already set as %% ID : 1 / Baudnum : 1 (Baudrate : 57600)
//

#if defined(__linux__) || defined(__APPLE__)
#include <fcntl.h>
#include <termios.h>
#define STDIN_FILENO 0
#elif defined(_WIN32) || defined(_WIN64)
#include <conio.h>
#endif

#include <stdlib.h>
#include <stdio.h>
#include "siardk.h"                                  // Uses siar SDK library

// Control table address
#define ADDR_PRO_TORQUE_ENABLE          512                 // Control table address is different in siar model
#define ADDR_PRO_GOAL_POSITION          532
#define ADDR_PRO_PRESENT_POSITION       552

// Protocol version
#define PROTOCOL_VERSION                2.0                 // See which protocol version is used in the siar

// Default setting
#define SIAR1_ID                         1                   // siar#1 ID: 1
#define SIAR2_ID                         2                   // siar#2 ID: 2
#define BAUDRATE                        1000000
#define DEVICENAME1                     "/dev/ttyUSB0"      // Check which port is being used on your controller
#define DEVICENAME2                     "/dev/ttyUSB1"      // ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"

#define TORQUE_ENABLE                   1                   // Value for enabling the torque
#define TORQUE_DISABLE                  0                   // Value for disabling the torque
#define SIAR_MINIMUM_POSITION_VALUE      -150000             // siar will rotate between this value
#define SIAR_MAXIMUM_POSITION_VALUE      150000              // and this value (note that the siar would not move when the position value is out of movable range. Check e-manual about the range of the siar you use.)
#define SIAR_MOVING_STATUS_THRESHOLD     20                  // siar moving status threshold

#define ESC_ASCII_VALUE                 0x1b

int getch()
{
#if defined(__linux__) || defined(__APPLE__)
  struct termios oldt, newt;
  int ch;
  tcgetattr(STDIN_FILENO, &oldt);
  newt = oldt;
  newt.c_lflag &= ~(ICANON | ECHO);
  tcsetattr(STDIN_FILENO, TCSANOW, &newt);
  ch = getchar();
  tcsetattr(STDIN_FILENO, TCSANOW, &oldt);
  return ch;
#elif defined(_WIN32) || defined(_WIN64)
  return _getch();
#endif
}

int kbhit(void)
{
#if defined(__linux__) || defined(__APPLE__)
  struct termios oldt, newt;
  int ch;
  int oldf;

  tcgetattr(STDIN_FILENO, &oldt);
  newt = oldt;
  newt.c_lflag &= ~(ICANON | ECHO);
  tcsetattr(STDIN_FILENO, TCSANOW, &newt);
  oldf = fcntl(STDIN_FILENO, F_GETFL, 0);
  fcntl(STDIN_FILENO, F_SETFL, oldf | O_NONBLOCK);

  ch = getchar();

  tcsetattr(STDIN_FILENO, TCSANOW, &oldt);
  fcntl(STDIN_FILENO, F_SETFL, oldf);

  if (ch != EOF)
  {
    ungetc(ch, stdin);
    return 1;
  }

  return 0;
#elif defined(_WIN32) || defined(_WIN64)
  return _kbhit();
#endif
}

int main()
{
  // Initialize PortHandler Structs
  // Set the port path
  // Get methods and members of PortHandlerLinux or PortHandlerWindows
  int port_num1 = portHandler(DEVICENAME1);
  int port_num2 = portHandler(DEVICENAME2);

  // Initialize PacketHandler Structs
  packetHandler();

  int index = 0;
  int siar_comm_result = COMM_TX_FAIL;             // Communication result
  int siar_goal_position[2] = { SIAR_MINIMUM_POSITION_VALUE, SIAR_MAXIMUM_POSITION_VALUE };  // Goal position

  uint8_t siar_error = 0;                          // siar error
  int32_t siar1_present_position = 0, siar2_present_position = 0;                           // Present position

  // Open port1
  if (openPort(port_num1))
  {
    printf("Succeeded to open the port1!\n");
  }
  else
  {
    printf("Failed to open the port1!\n");
    printf("Press any key to terminate...\n");
    getch();
    return 0;
  }

  // Open port2
  if (openPort(port_num2))
  {
    printf("Succeeded to open the port2!\n");
  }
  else
  {
    printf("Failed to open the port2!\n");
    printf("Press any key to terminate...\n");
    getch();
    return 0;
  }

  // Set port1 baudrate
  if (setBaudRate(port_num1, BAUDRATE))
  {
    printf("Succeed to change the baudrate port1!\n");
  }
  else
  {
    printf("Failed to change the baudrate port1!\n");
    printf("Press any key to terminate...\n");
    getch();
    return 0;
  }

  // Set port2 baudrate
  if (setBaudRate(port_num2, BAUDRATE))
  {
    printf("Succeed to change the baudrate port2!\n");
  }
  else
  {
    printf("Failed to change the baudrate port2!\n");
    printf("Press any key to terminate...\n");
    getch();
    return 0;
  }

  // Enable siar#1 Torque
  write1ByteTxRx(port_num1, PROTOCOL_VERSION, SIAR1_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_ENABLE);
  if ((siar_comm_result = getLastTxRxResult(port_num1, PROTOCOL_VERSION)) != COMM_SUCCESS)
  {
    printf("%s\n", getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
  }
  else if ((siar_error = getLastRxPacketError(port_num1, PROTOCOL_VERSION)) != 0)
  {
    printf("%s\n", getRxPacketError(PROTOCOL_VERSION, siar_error));
  }
  else
  {
    printf("siar#%d has been successfully connected \n", SIAR1_ID);
  }

  // Enable siar#2 Torque
  write1ByteTxRx(port_num2, PROTOCOL_VERSION, SIAR2_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_ENABLE);
  if ((siar_comm_result = getLastTxRxResult(port_num2, PROTOCOL_VERSION)) != COMM_SUCCESS)
  {
    printf("%s\n", getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
  }
  else if ((siar_error = getLastRxPacketError(port_num2, PROTOCOL_VERSION)) != 0)
  {
    printf("%s\n", getRxPacketError(PROTOCOL_VERSION, siar_error));
  }
  else
  {
    printf("siar#%d has been successfully connected \n", SIAR2_ID);
  }

  while (1)
  {
    printf("Press any key to continue! (or press ESC to quit!)\n");
    if (getch() == ESC_ASCII_VALUE)
      break;

    // Write siar#1 goal position
    write4ByteTxRx(port_num1, PROTOCOL_VERSION, SIAR1_ID, ADDR_PRO_GOAL_POSITION, siar_goal_position[index]);
    if ((siar_comm_result = getLastTxRxResult(port_num1, PROTOCOL_VERSION)) != COMM_SUCCESS)
    {
      printf("%s\n", getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
    }
    else if ((siar_error = getLastRxPacketError(port_num1, PROTOCOL_VERSION)) != 0)
    {
      printf("%s\n", getRxPacketError(PROTOCOL_VERSION, siar_error));
    }

    // Write siar#2 goal position
    write4ByteTxRx(port_num2, PROTOCOL_VERSION, SIAR2_ID, ADDR_PRO_GOAL_POSITION, siar_goal_position[index]);
    if ((siar_comm_result = getLastTxRxResult(port_num2, PROTOCOL_VERSION)) != COMM_SUCCESS)
    {
      printf("%s\n", getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
    }
    else if ((siar_error = getLastRxPacketError(port_num2, PROTOCOL_VERSION)) != 0)
    {
      printf("%s\n", getRxPacketError(PROTOCOL_VERSION, siar_error));
    }

    do
    {
      // Read siar#1 present position
    	siar1_present_position = read4ByteTxRx(port_num1, PROTOCOL_VERSION, SIAR1_ID, ADDR_PRO_PRESENT_POSITION);
      if ((siar_comm_result = getLastTxRxResult(port_num1, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        printf("%s\n", getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
      }
      else if ((siar_error = getLastRxPacketError(port_num1, PROTOCOL_VERSION)) != 0)
      {
        printf("%s\n", getRxPacketError(PROTOCOL_VERSION, siar_error));
      }

      // Read siar#2 present position
      siar2_present_position = read4ByteTxRx(port_num2, PROTOCOL_VERSION, SIAR2_ID, ADDR_PRO_PRESENT_POSITION);
      if ((siar_comm_result = getLastTxRxResult(port_num2, PROTOCOL_VERSION)) != COMM_SUCCESS)
      {
        printf("%s\n", getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
      }
      else if ((siar_error = getLastRxPacketError(port_num2, PROTOCOL_VERSION)) != 0)
      {
        printf("%s\n", getRxPacketError(PROTOCOL_VERSION, siar_error));
      }

      printf("[ID:%03d] GoalPos:%03d  PresPos:%03d\t[ID:%03d] GoalPos:%03d  PresPos:%03d\n", SIAR1_ID, siar_goal_position[index], siar1_present_position, SIAR2_ID, siar_goal_position[index], siar2_present_position);

    } while ((abs(siar_goal_position[index] - siar1_present_position) > SIAR_MOVING_STATUS_THRESHOLD) || (abs(siar_goal_position[index] - siar2_present_position) > SIAR_MOVING_STATUS_THRESHOLD));

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
  write1ByteTxRx(port_num1, PROTOCOL_VERSION, SIAR1_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_DISABLE);
  if ((siar_comm_result = getLastTxRxResult(port_num1, PROTOCOL_VERSION)) != COMM_SUCCESS)
  {
    printf("%s\n", getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
  }
  else if ((siar_error = getLastRxPacketError(port_num1, PROTOCOL_VERSION)) != 0)
  {
    printf("%s\n", getRxPacketError(PROTOCOL_VERSION, siar_error));
  }

  // Disable siar#2 Torque
  write1ByteTxRx(port_num2, PROTOCOL_VERSION, SIAR2_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_DISABLE);
  if ((siar_comm_result = getLastTxRxResult(port_num2, PROTOCOL_VERSION)) != COMM_SUCCESS)
  {
    printf("%s\n", getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
  }
  else if ((siar_error = getLastRxPacketError(port_num2, PROTOCOL_VERSION)) != 0)
  {
    printf("%s\n", getRxPacketError(PROTOCOL_VERSION, siar_error));
  }

  // Close port1
  closePort(port_num1);

  // Close port2
  closePort(port_num2);

  return 0;
}
