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
// *********     Bulk Read Example      *********
//
//
// Available siar model on this example : MX or X series set to Protocol 1.0
// This example is designed for using two siar MX-28, and an USB2siar.
// To use another siar model, such as X series, see their details in E-Manual(emanual.robotis.com) and edit below "#define"d variables yourself.
// Be sure that siar MX properties are already set as %% ID : 1 / Baudnum : 34 (Baudrate : 57600)
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
#define ADDR_MX_TORQUE_ENABLE           24                  // Control table address is different in siar model
#define ADDR_MX_GOAL_POSITION           30
#define ADDR_MX_PRESENT_POSITION        36
#define ADDR_MX_MOVING                  46

// Data Byte Length
#define LEN_MX_GOAL_POSITION            2
#define LEN_MX_PRESENT_POSITION         2
#define LEN_MX_MOVING                   1

// Protocol version
#define PROTOCOL_VERSION                1.0                 // See which protocol version is used in the siar

// Default setting
#define SIAR1_ID                         1                   // siar#1 ID: 1
#define SIAR2_ID                         2                   // siar#2 ID: 2
#define BAUDRATE                        1000000
#define DEVICENAME                      "/dev/ttyUSB0"      // Check which port is being used on your controller
                                                            // ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"

#define TORQUE_ENABLE                   1                   // Value for enabling the torque
#define TORQUE_DISABLE                  0                   // Value for disabling the torque
#define SIAR_MINIMUM_POSITION_VALUE      100                 // siar will rotate between this value
#define SIAR_MAXIMUM_POSITION_VALUE      4000                // and this value (note that the siar would not move when the position value is out of movable range. Check e-manual about the range of the siar you use.)
#define SIAR_MOVING_STATUS_THRESHOLD     10                  // siar moving status threshold

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
  int port_num = portHandler(DEVICENAME);

  // Initialize PacketHandler Structs
  packetHandler();

  // Initialize Groupbulkread Structs
  int group_num = groupBulkRead(port_num, PROTOCOL_VERSION);

  int index = 0;
  int siar_comm_result = COMM_TX_FAIL;             // Communication result
  uint8_t siar_addparam_result = False;               // AddParam result
  uint8_t siar_getdata_result = False;                // GetParam result
  int siar_goal_position[2] = { SIAR_MINIMUM_POSITION_VALUE, SIAR_MAXIMUM_POSITION_VALUE };         // Goal position

  uint8_t siar_error = 0;                          // siar error
  uint16_t siar1_present_position = 0;             // Present position
  uint8_t siar2_moving = 0;                        // siar moving status

  // Open port
  if (openPort(port_num))
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
  if (setBaudRate(port_num, BAUDRATE))
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

  // Enable siar#1 Torque
  write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR1_ID, ADDR_MX_TORQUE_ENABLE, TORQUE_ENABLE);
  if ((siar_comm_result = getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
  {
    printf("%s\n", getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
  }
  else if ((siar_error = getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
  {
    printf("%s\n", getRxPacketError(PROTOCOL_VERSION, siar_error));
  }
  else
  {
    printf("siar#%d has been successfully connected \n", SIAR1_ID);
  }

  // Enable siar#2 Torque
  write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR2_ID, ADDR_MX_TORQUE_ENABLE, TORQUE_ENABLE);
  if ((siar_comm_result = getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
  {
    printf("%s\n", getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
  }
  else if ((siar_error = getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
  {
    printf("%s\n", getRxPacketError(PROTOCOL_VERSION, siar_error));
  }
  else
  {
    printf("siar#%d has been successfully connected \n", SIAR2_ID);
  }

  // Add parameter storage for siar#1 present position value
  siar_addparam_result = groupBulkReadAddParam(group_num, SIAR1_ID, ADDR_MX_PRESENT_POSITION, LEN_MX_PRESENT_POSITION);
  if (siar_addparam_result != True)
  {
    fprintf(stderr, "[ID:%03d] groupBulkRead addparam failed", SIAR1_ID);
    return 0;
  }

  // Add parameter storage for siar#2 present moving value
  siar_addparam_result = groupBulkReadAddParam(group_num, SIAR2_ID, ADDR_MX_MOVING, LEN_MX_MOVING);
  if (siar_addparam_result != True)
  {
    fprintf(stderr, "[ID:%03d] groupBulkRead addparam failed", SIAR2_ID);
    return 0;
  }

  while (1)
  {
    printf("Press any key to continue! (or press ESC to quit!)\n");
    if (getch() == ESC_ASCII_VALUE)
      break;

    // Write siar#1 goal position
    write2ByteTxRx(port_num, PROTOCOL_VERSION, SIAR1_ID, ADDR_MX_GOAL_POSITION, siar_goal_position[index]);
    if ((siar_comm_result = getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
    {
      printf("%s\n", getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
    }
    else if ((siar_error = getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
    {
      printf("%s\n", getRxPacketError(PROTOCOL_VERSION, siar_error));
    }

    // Write siar#2 goal position
    write2ByteTxRx(port_num, PROTOCOL_VERSION, SIAR2_ID, ADDR_MX_GOAL_POSITION, siar_goal_position[index]);
    if ((siar_comm_result = getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
    {
      printf("%s\n", getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
    }
    else if ((siar_error = getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
    {
      printf("%s\n", getRxPacketError(PROTOCOL_VERSION, siar_error));
    }

    do
    {
      // Bulkread present position and moving status
      groupBulkReadTxRxPacket(group_num);
      if ((siar_comm_result = getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
        printf("%s\n", getTxRxResult(PROTOCOL_VERSION, siar_comm_result));

      siar_getdata_result = groupBulkReadIsAvailable(group_num, SIAR1_ID, ADDR_MX_PRESENT_POSITION, LEN_MX_PRESENT_POSITION);
      if (siar_getdata_result != True)
      {
        fprintf(stderr, "[ID:%03d] groupBulkRead getdata failed", SIAR1_ID);
        return 0;
      }

      siar_getdata_result = groupBulkReadIsAvailable(group_num, SIAR2_ID, ADDR_MX_MOVING, LEN_MX_MOVING);
      if (siar_getdata_result != True)
      {
        fprintf(stderr, "[ID:%03d] groupBulkRead getdata failed", SIAR2_ID);
        return 0;
      }

      // Get siar#1 present position value
      siar1_present_position = groupBulkReadGetData(group_num, SIAR1_ID, ADDR_MX_PRESENT_POSITION, LEN_MX_PRESENT_POSITION);

      // Get siar#2 moving status value
      siar2_moving = groupBulkReadGetData(group_num, SIAR2_ID, ADDR_MX_MOVING, LEN_MX_MOVING);

      printf("[ID:%03d] Present Position : %d \t [ID:%03d] Is Moving : %d\n", SIAR1_ID, siar1_present_position, SIAR2_ID, siar2_moving);

    } while (abs(siar_goal_position[index] - siar1_present_position) > SIAR_MOVING_STATUS_THRESHOLD);

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
  write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR1_ID, ADDR_MX_TORQUE_ENABLE, TORQUE_DISABLE);
  if ((siar_comm_result = getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
  {
    printf("%s\n", getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
  }
  else if ((siar_error = getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
  {
    printf("%s\n", getRxPacketError(PROTOCOL_VERSION, siar_error));
  }

  // Disable siar#2 Torque
  write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR2_ID, ADDR_MX_TORQUE_ENABLE, TORQUE_DISABLE);
  if ((siar_comm_result = getLastTxRxResult(port_num, PROTOCOL_VERSION)) != COMM_SUCCESS)
  {
    printf("%s\n", getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
  }
  else if ((siar_error = getLastRxPacketError(port_num, PROTOCOL_VERSION)) != 0)
  {
    printf("%s\n", getRxPacketError(PROTOCOL_VERSION, siar_error));
  }

  // Close port
  closePort(port_num);

  return 0;
}
