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

/* Author: RWJ (L) */

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

// Control table address for siar MX
#define ADDR_MX_TORQUE_ENABLE           24                  // Control table address is different in siar model
#define ADDR_MX_GOAL_POSITION           30
#define ADDR_MX_PRESENT_POSITION        36

// Control table address for siar PRO
#define ADDR_PRO_TORQUE_ENABLE          512
#define ADDR_PRO_GOAL_POSITION          532
#define ADDR_PRO_PRESENT_POSITION       552

// Protocol version
#define PROTOCOL_VERSION1               1.0                 // See which protocol version is used in the siar
#define PROTOCOL_VERSION2               2.0

// Default setting
#define SIAR1_ID                         1                   // siar#1 ID: 1
#define SIAR2_ID                         2                   // siar#2 ID: 2
#define BAUDRATE                        1000000
#define DEVICENAME                      "/COM5"      // Check which port is being used on your controller
                                                            // ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"

#define TORQUE_ENABLE                   1                   // Value for enabling the torque
#define TORQUE_DISABLE                  0                   // Value for disabling the torque
#define SIAR1_MINIMUM_POSITION_VALUE     100                 // siar will rotate between this value
#define SIAR1_MAXIMUM_POSITION_VALUE     4000                // and this value (note that the siar would not move when the position value is out of movable range. Check e-manual about the range of the siar you use.)
#define SIAR2_MINIMUM_POSITION_VALUE     0
#define SIAR2_MAXIMUM_POSITION_VALUE     16384
#define SIAR1_MOVING_STATUS_THRESHOLD    10                  // siar MX moving status threshold
#define SIAR2_MOVING_STATUS_THRESHOLD    20                  // siar PRO moving status threshold

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
  // Initialize PortHandler instance
  // Set the port path
  // Get methods and members of PortHandlerLinux or PortHandlerWindows
  siar::PortHandler *portHandler = siar::PortHandler::getPortHandler(DEVICENAME);

  // Initialize PacketHandler instance
  // Set the protocol version
  // Get methods and members of Protocol1PacketHandler or Protocol2PacketHandler
  siar::PacketHandler *packetHandler1 = siar::PacketHandler::getPacketHandler(PROTOCOL_VERSION1);
  siar::PacketHandler *packetHandler2 = siar::PacketHandler::getPacketHandler(PROTOCOL_VERSION2);

  int index = 0;
  int siar_comm_result = COMM_TX_FAIL;       // Communication result
  int siar1_goal_position[2] = {SIAR1_MINIMUM_POSITION_VALUE, SIAR1_MAXIMUM_POSITION_VALUE};     // Goal position of siar MX
  int siar2_goal_position[2] = {SIAR2_MINIMUM_POSITION_VALUE, SIAR2_MAXIMUM_POSITION_VALUE};     // Goal position of siar PRO

  uint8_t siar_error = 0;                    // siar error
  uint16_t siar1_present_position = 0;       // Present position of siar MX
  int32_t siar2_present_position = 0;        // Present position of siar PRO

  // Open port
  if (portHandler->openPort())
  {
    printf("Succeeded to open the port!\n");
  }
  else
  {
    printf("Failed to open the port!\n");
    printf("Press any key to terminate...\n");
    getch();
    return 0;
  }

  // Set port baudrate
  if (portHandler->setBaudRate(BAUDRATE))
  {
    printf("Succeeded to change the baudrate!\n");
  }
  else
  {
    printf("Failed to change the baudrate!\n");
    printf("Press any key to terminate...\n");
    getch();
    return 0;
  }

  // Enable siar#1 torque
  siar_comm_result = packetHandler1->write1ByteTxRx(portHandler, SIAR1_ID, ADDR_MX_TORQUE_ENABLE, TORQUE_ENABLE, &siar_error);
  if (siar_comm_result != COMM_SUCCESS)
  {
    printf("%s\n", packetHandler1->getTxRxResult(siar_comm_result));
  }
  else if (siar_error != 0)
  {
    printf("%s\n", packetHandler1->getRxPacketError(siar_error));
  }
  else
  {
    printf("siar#%d has been successfully connected \n", SIAR1_ID);
  }
  // Enable siar#2 torque
  siar_comm_result = packetHandler2->write1ByteTxRx(portHandler, SIAR2_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_ENABLE, &siar_error);
  if (siar_comm_result != COMM_SUCCESS)
  {
    printf("%s\n", packetHandler2->getTxRxResult(siar_comm_result));
  }
  else if (siar_error != 0)
  {
    printf("%s\n", packetHandler2->getRxPacketError(siar_error));
  }
  else
  {
    printf("siar#%d has been successfully connected \n", SIAR2_ID);
  }

  while(1)
  {
    printf("Press any key to continue! (or press ESC to quit!)\n");
    if (getch() == ESC_ASCII_VALUE)
      break;

    // Write siar#1 goal position
    siar_comm_result = packetHandler1->write2ByteTxRx(portHandler, SIAR1_ID, ADDR_MX_GOAL_POSITION, siar1_goal_position[index], &siar_error);
    if (siar_comm_result != COMM_SUCCESS)
    {
      printf("%s\n", packetHandler1->getTxRxResult(siar_comm_result));
    }
    else if (siar_error != 0)
    {
      printf("%s\n", packetHandler1->getRxPacketError(siar_error));
    }

    // Write siar#2 goal position
    siar_comm_result = packetHandler2->write4ByteTxRx(portHandler, SIAR2_ID, ADDR_PRO_GOAL_POSITION, siar2_goal_position[index], &siar_error);
    if (siar_comm_result != COMM_SUCCESS)
    {
      printf("%s\n", packetHandler2->getTxRxResult(siar_comm_result));
    }
    else if (siar_error != 0)
    {
      printf("%s\n", packetHandler2->getRxPacketError(siar_error));
    }

    do
    {
      // Read siar#1 present position
      siar_comm_result = packetHandler1->read2ByteTxRx(portHandler, SIAR1_ID, ADDR_MX_PRESENT_POSITION, &siar1_present_position, &siar_error);
      if (siar_comm_result != COMM_SUCCESS)
      {
        printf("%s\n", packetHandler1->getTxRxResult(siar_comm_result));
      }
      else if (siar_error != 0)
      {
        printf("%s\n", packetHandler1->getRxPacketError(siar_error));
      }

      // Read siar#2 present position
      siar_comm_result = packetHandler2->read4ByteTxRx(portHandler, SIAR2_ID, ADDR_PRO_PRESENT_POSITION, (uint32_t*)&siar2_present_position, &siar_error);
      if (siar_comm_result != COMM_SUCCESS)
      {
        printf("%s\n", packetHandler2->getTxRxResult(siar_comm_result));
      }
      else if (siar_error != 0)
      {
        printf("%s\n", packetHandler2->getRxPacketError(siar_error));
      }

      printf("[ID:%03d] GoalPos:%03d  PresPos:%03d [ID:%03d] GoalPos:%03d  PresPos:%03d\n", SIAR1_ID, siar1_goal_position[index], siar1_present_position, SIAR2_ID, siar2_goal_position[index], siar2_present_position);

    }while((abs(siar1_goal_position[index] - siar1_present_position) > SIAR1_MOVING_STATUS_THRESHOLD) || (abs(siar2_goal_position[index] - siar2_present_position) > SIAR2_MOVING_STATUS_THRESHOLD));

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
  siar_comm_result = packetHandler1->write1ByteTxRx(portHandler, SIAR1_ID, ADDR_MX_TORQUE_ENABLE, TORQUE_DISABLE, &siar_error);
  if (siar_comm_result != COMM_SUCCESS)
  {
    printf("%s\n", packetHandler1->getTxRxResult(siar_comm_result));
  }
  else if (siar_error != 0)
  {
    printf("%s\n", packetHandler1->getRxPacketError(siar_error));
  }

  // Disable siar#2 Torque
  siar_comm_result = packetHandler2->write1ByteTxRx(portHandler, SIAR2_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_DISABLE, &siar_error);
  if (siar_comm_result != COMM_SUCCESS)
  {
    printf("%s\n", packetHandler2->getTxRxResult(siar_comm_result));
  }
  else if (siar_error != 0)
  {
    printf("%s\n", packetHandler2->getRxPacketError(siar_error));
  }

  // Close port
  portHandler->closePort();

  return 0;
}
