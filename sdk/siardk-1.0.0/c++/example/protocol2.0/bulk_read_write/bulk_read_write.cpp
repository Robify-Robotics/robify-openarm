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
// *********     Bulk Read and Bulk Write Example      *********
//
//
// Available siar model on this example : All models using Protocol 2.0
// This example is tested with two siar PRO 54-200, and an USB2siar
// Be sure that siar PRO properties are already set as %% ID : 1 and 2 / Baudnum : 1 (Baudrate : 57600)
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
#define ADDR_PRO_LED_RED                563
#define ADDR_PRO_GOAL_POSITION          532
#define ADDR_PRO_PRESENT_POSITION       611

// Data Byte Length
#define LEN_PRO_LED_RED                 1
#define LEN_PRO_GOAL_POSITION           4
#define LEN_PRO_PRESENT_POSITION        4

// Protocol version
#define PROTOCOL_VERSION                2.0                 // See which protocol version is used in the siar

// Default setting
#define SIAR1_ID                         1                   // siar#1 ID: 1
#define SIAR2_ID                         2                   // siar#2 ID: 2
#define BAUDRATE                        57600
#define DEVICENAME                      "/dev/ttyUSB0"      // Check which port is being used on your controller
                                                            // ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"

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
  // Initialize PortHandler instance
  // Set the port path
  // Get methods and members of PortHandlerLinux or PortHandlerWindows
  siar::PortHandler *portHandler = siar::PortHandler::getPortHandler(DEVICENAME);

  // Initialize PacketHandler instance
  // Set the protocol version
  // Get methods and members of Protocol1PacketHandler or Protocol2PacketHandler
  siar::PacketHandler *packetHandler = siar::PacketHandler::getPacketHandler(PROTOCOL_VERSION);

  // Initialize GroupBulkWrite instance
  siar::GroupBulkWrite groupBulkWrite(portHandler, packetHandler);

  // Initialize GroupBulkRead instance
  siar::GroupBulkRead groupBulkRead(portHandler, packetHandler);

  int index = 0;
  int siar_comm_result = COMM_TX_FAIL;             // Communication result
  bool siar_addparam_result = false;               // addParam result
  bool siar_getdata_result = false;                // GetParam result
  int siar_goal_position[2] = {SIAR_MINIMUM_POSITION_VALUE, SIAR_MAXIMUM_POSITION_VALUE};         // Goal position

  uint8_t siar_error = 0;                          // siar error
  uint8_t siar_led_value[2] = {0x00, 0xFF};        // siar LED value for write
  uint8_t param_goal_position[4];
  int32_t siar1_present_position = 0;              // Present position
  uint8_t siar2_led_value_read;                    // siar LED value for read

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

  // Enable siar#1 Torque
  siar_comm_result = packetHandler->write1ByteTxRx(portHandler, SIAR1_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_ENABLE, &siar_error);
  if (siar_comm_result != COMM_SUCCESS)
  {
    printf("%s\n", packetHandler->getTxRxResult(siar_comm_result));
  }
  else if (siar_error != 0)
  {
    printf("%s\n", packetHandler->getRxPacketError(siar_error));
  }
  else
  {
    printf("siar#%d has been successfully connected \n", SIAR1_ID);
  }

  // Enable siar#2 Torque
  siar_comm_result = packetHandler->write1ByteTxRx(portHandler, SIAR2_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_ENABLE, &siar_error);
  if (siar_comm_result != COMM_SUCCESS)
  {
    printf("%s\n", packetHandler->getTxRxResult(siar_comm_result));
  }
  else if (siar_error != 0)
  {
    printf("%s\n", packetHandler->getRxPacketError(siar_error));
  }
  else
  {
    printf("siar#%d has been successfully connected \n", SIAR2_ID);
  }

  // Add parameter storage for siar#1 present position
  siar_addparam_result = groupBulkRead.addParam(SIAR1_ID, ADDR_PRO_PRESENT_POSITION, LEN_PRO_PRESENT_POSITION);
  if (siar_addparam_result != true)
  {
    fprintf(stderr, "[ID:%03d] grouBulkRead addparam failed", SIAR1_ID);
    return 0;
  }

  // Add parameter storage for siar#2 LED value
  siar_addparam_result = groupBulkRead.addParam(SIAR2_ID, ADDR_PRO_LED_RED, LEN_PRO_LED_RED);
  if (siar_addparam_result != true)
  {
    fprintf(stderr, "[ID:%03d] grouBulkRead addparam failed", SIAR2_ID);
    return 0;
  }

  while(1)
  {
    printf("Press any key to continue! (or press ESC to quit!)\n");
    if (getch() == ESC_ASCII_VALUE)
      break;

    // Allocate goal position value into byte array
    param_goal_position[0] = SIAR_LOBYTE(SIAR_LOWORD(siar_goal_position[index]));
    param_goal_position[1] = SIAR_HIBYTE(SIAR_LOWORD(siar_goal_position[index]));
    param_goal_position[2] = SIAR_LOBYTE(SIAR_HIWORD(siar_goal_position[index]));
    param_goal_position[3] = SIAR_HIBYTE(SIAR_HIWORD(siar_goal_position[index]));

    // Add parameter storage for siar#1 goal position
    siar_addparam_result = groupBulkWrite.addParam(SIAR1_ID, ADDR_PRO_GOAL_POSITION, LEN_PRO_GOAL_POSITION, param_goal_position);
    if (siar_addparam_result != true)
    {
      fprintf(stderr, "[ID:%03d] groupBulkWrite addparam failed", SIAR1_ID);
      return 0;
    }

    // Add parameter storage for siar#2 LED value
    siar_addparam_result = groupBulkWrite.addParam(SIAR2_ID, ADDR_PRO_LED_RED, LEN_PRO_LED_RED, &siar_led_value[index]);
    if (siar_addparam_result != true)
    {
      fprintf(stderr, "[ID:%03d] groupBulkWrite addparam failed", SIAR2_ID);
      return 0;
    }

    // Bulkwrite goal position and LED value
    siar_comm_result = groupBulkWrite.txPacket();
    if (siar_comm_result != COMM_SUCCESS) printf("%s\n", packetHandler->getTxRxResult(siar_comm_result));

    // Clear bulkwrite parameter storage
    groupBulkWrite.clearParam();

    do
    {
      // Bulkread present position and LED status
      siar_comm_result = groupBulkRead.txRxPacket();
      if (siar_comm_result != COMM_SUCCESS)
      {
        printf("%s\n", packetHandler->getTxRxResult(siar_comm_result));
      }
      else if (groupBulkRead.getError(SIAR1_ID, &siar_error))
      {
        printf("[ID:%03d] %s\n", SIAR1_ID, packetHandler->getRxPacketError(siar_error));
      }
      else if (groupBulkRead.getError(SIAR2_ID, &siar_error))
      {
        printf("[ID:%03d] %s\n", SIAR2_ID, packetHandler->getRxPacketError(siar_error));
      }

      // Check if groupbulkread data of siar#1 is available
      siar_getdata_result = groupBulkRead.isAvailable(SIAR1_ID, ADDR_PRO_PRESENT_POSITION, LEN_PRO_PRESENT_POSITION);
      if (siar_getdata_result != true)
      {
        fprintf(stderr, "[ID:%03d] groupBulkRead getdata failed", SIAR1_ID);
        return 0;
      }

      // Check if groupbulkread data of siar#2 is available
      siar_getdata_result = groupBulkRead.isAvailable(SIAR2_ID, ADDR_PRO_LED_RED, LEN_PRO_LED_RED);
      if (siar_getdata_result != true)
      {
        fprintf(stderr, "[ID:%03d] groupBulkRead getdata failed", SIAR2_ID);
        return 0;
      }

      // Get present position value
      siar1_present_position = groupBulkRead.getData(SIAR1_ID, ADDR_PRO_PRESENT_POSITION, LEN_PRO_PRESENT_POSITION);

      // Get LED value
      siar2_led_value_read = groupBulkRead.getData(SIAR2_ID, ADDR_PRO_LED_RED, LEN_PRO_LED_RED);

      printf("[ID:%03d] Present Position : %d \t [ID:%03d] LED Value: %d\n", SIAR1_ID, siar1_present_position, SIAR2_ID, siar2_led_value_read);

    }while(abs(siar_goal_position[index] - siar1_present_position) > SIAR_MOVING_STATUS_THRESHOLD);

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
  siar_comm_result = packetHandler->write1ByteTxRx(portHandler, SIAR1_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_DISABLE, &siar_error);
  if (siar_comm_result != COMM_SUCCESS)
  {
    printf("%s\n", packetHandler->getTxRxResult(siar_comm_result));
  }
  else if (siar_error != 0)
  {
    printf("%s\n", packetHandler->getRxPacketError(siar_error));
  }

  // Disable siar#2 Torque
  siar_comm_result = packetHandler->write1ByteTxRx(portHandler, SIAR2_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_DISABLE, &siar_error);
  if (siar_comm_result != COMM_SUCCESS)
  {
    printf("%s\n", packetHandler->getTxRxResult(siar_comm_result));
  }
  else if (siar_error != 0)
  {
    printf("%s\n", packetHandler->getRxPacketError(siar_error));
  }

  // Close port
  portHandler->closePort();

  return 0;
}
