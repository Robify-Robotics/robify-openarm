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
// *********     Indirect Address Example      *********
//
//
// Available siar model on this example : All models using Protocol 2.0
// This example is tested with a siar PRO 54-200, and an USB2siar
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

#include "siardk.h"                                          // Uses siar SDK library

// Control table address
// Control table address is different in siar model
#define ADDR_PRO_INDIRECTADDRESS_FOR_WRITE      49                  // EEPROM region
#define ADDR_PRO_INDIRECTADDRESS_FOR_READ       59                  // EEPROM region
#define ADDR_PRO_TORQUE_ENABLE                  512
#define ADDR_PRO_LED_RED                        563
#define ADDR_PRO_GOAL_POSITION                  532
#define ADDR_PRO_MOVING                         610
#define ADDR_PRO_PRESENT_POSITION               611
#define ADDR_PRO_INDIRECTDATA_FOR_WRITE         634
#define ADDR_PRO_INDIRECTDATA_FOR_READ          639

// Data Byte Length
#define LEN_PRO_LED_RED                         1
#define LEN_PRO_GOAL_POSITION                   4
#define LEN_PRO_MOVING                          1
#define LEN_PRO_PRESENT_POSITION                4
#define LEN_PRO_INDIRECTDATA_FOR_WRITE          5
#define LEN_PRO_INDIRECTDATA_FOR_READ           5

// Protocol version
#define PROTOCOL_VERSION                        2.0                 // See which protocol version is used in the siar

// Default setting
#define SIAR_ID                                  1                   // siar ID: 1
#define BAUDRATE                                57600
#define DEVICENAME                              "/dev/ttyUSB0"      // Check which port is being used on your controller
                                                                    // ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"

#define TORQUE_ENABLE                           1                   // Value for enabling the torque
#define TORQUE_DISABLE                          0                   // Value for disabling the torque
#define SIAR_MINIMUM_POSITION_VALUE              -150000             // siar will rotate between this value
#define SIAR_MAXIMUM_POSITION_VALUE              150000              // and this value (note that the siar would not move when the position value is out of movable range. Check e-manual about the range of the siar you use.)
#define SIAR_MINIMUM_LED_VALUE                   0                   // siar LED will light between this value
#define SIAR_MAXIMUM_LED_VALUE                   255                 // and this value
#define SIAR_MOVING_STATUS_THRESHOLD             20                  // siar moving status threshold

#define ESC_ASCII_VALUE                         0x1b

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

  // Initialize GroupSyncWrite instance
  siar::GroupSyncWrite groupSyncWrite(portHandler, packetHandler, ADDR_PRO_INDIRECTDATA_FOR_WRITE, LEN_PRO_INDIRECTDATA_FOR_WRITE);

  // Initialize Groupsyncread instance
  siar::GroupSyncRead groupSyncRead(portHandler, packetHandler, ADDR_PRO_INDIRECTDATA_FOR_READ, LEN_PRO_INDIRECTDATA_FOR_READ);

  int index = 0;
  int siar_comm_result = COMM_TX_FAIL;             // Communication result
  bool siar_addparam_result = false;               // addParam result
  bool siar_getdata_result = false;                // GetParam result
  int siar_goal_position[2] = {SIAR_MINIMUM_POSITION_VALUE, SIAR_MAXIMUM_POSITION_VALUE};  // Goal position

  uint8_t siar_error = 0;                          // siar error
  uint8_t siar_moving = 0;                         // siar moving status
  uint8_t param_indirect_data_for_write[LEN_PRO_INDIRECTDATA_FOR_WRITE];
  uint8_t siar_led_value[2] = {0x00, 0xFF};        // siar LED value
  int32_t siar_present_position = 0;               // Present position

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

  // Disable siar Torque :
  // Indirect address would not accessible when the torque is already enabled
  siar_comm_result = packetHandler->write1ByteTxRx(portHandler, SIAR_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_DISABLE, &siar_error);
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
    printf("SIAR has been successfully connected \n");
  }

  // INDIRECTDATA parameter storages replace LED, goal position, present position and moving status storages
  siar_comm_result = packetHandler->write2ByteTxRx(portHandler, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_WRITE + 0, ADDR_PRO_GOAL_POSITION + 0, &siar_error);
  if (siar_comm_result != COMM_SUCCESS)
  {
    printf("%s\n", packetHandler->getTxRxResult(siar_comm_result));
  }
  else if (siar_error != 0)
  {
    printf("%s\n", packetHandler->getRxPacketError(siar_error));
  }

  siar_comm_result = packetHandler->write2ByteTxRx(portHandler, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_WRITE + 2, ADDR_PRO_GOAL_POSITION + 1, &siar_error);
  if (siar_comm_result != COMM_SUCCESS)
  {
    printf("%s\n", packetHandler->getTxRxResult(siar_comm_result));
  }
  else if (siar_error != 0)
  {
    printf("%s\n", packetHandler->getRxPacketError(siar_error));
  }

  siar_comm_result = packetHandler->write2ByteTxRx(portHandler, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_WRITE + 4, ADDR_PRO_GOAL_POSITION + 2, &siar_error);
  if (siar_comm_result != COMM_SUCCESS)
  {
    printf("%s\n", packetHandler->getTxRxResult(siar_comm_result));
  }
  else if (siar_error != 0)
  {
    printf("%s\n", packetHandler->getRxPacketError(siar_error));
  }

  siar_comm_result = packetHandler->write2ByteTxRx(portHandler, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_WRITE + 6, ADDR_PRO_GOAL_POSITION + 3, &siar_error);
  if (siar_comm_result != COMM_SUCCESS)
  {
    printf("%s\n", packetHandler->getTxRxResult(siar_comm_result));
  }
  else if (siar_error != 0)
  {
    printf("%s\n", packetHandler->getRxPacketError(siar_error));
  }

  siar_comm_result = packetHandler->write2ByteTxRx(portHandler, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_WRITE + 8, ADDR_PRO_LED_RED, &siar_error);
  if (siar_comm_result != COMM_SUCCESS)
  {
    printf("%s\n", packetHandler->getTxRxResult(siar_comm_result));
  }
  else if (siar_error != 0)
  {
    printf("%s\n", packetHandler->getRxPacketError(siar_error));
  }

  siar_comm_result = packetHandler->write2ByteTxRx(portHandler, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_READ + 0, ADDR_PRO_PRESENT_POSITION + 0, &siar_error);
  if (siar_comm_result != COMM_SUCCESS)
  {
    printf("%s\n", packetHandler->getTxRxResult(siar_comm_result));
  }
  else if (siar_error != 0)
  {
    printf("%s\n", packetHandler->getRxPacketError(siar_error));
  }

  siar_comm_result = packetHandler->write2ByteTxRx(portHandler, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_READ + 2, ADDR_PRO_PRESENT_POSITION + 1, &siar_error);
  if (siar_comm_result != COMM_SUCCESS)
  {
    printf("%s\n", packetHandler->getTxRxResult(siar_comm_result));
  }
  else if (siar_error != 0)
  {
    printf("%s\n", packetHandler->getRxPacketError(siar_error));
  }

  siar_comm_result = packetHandler->write2ByteTxRx(portHandler, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_READ + 4, ADDR_PRO_PRESENT_POSITION + 2, &siar_error);
  if (siar_comm_result != COMM_SUCCESS)
  {
    printf("%s\n", packetHandler->getTxRxResult(siar_comm_result));
  }
  else if (siar_error != 0)
  {
    printf("%s\n", packetHandler->getRxPacketError(siar_error));
  }

  siar_comm_result = packetHandler->write2ByteTxRx(portHandler, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_READ + 6, ADDR_PRO_PRESENT_POSITION + 3, &siar_error);
  if (siar_comm_result != COMM_SUCCESS)
  {
    printf("%s\n", packetHandler->getTxRxResult(siar_comm_result));
  }
  else if (siar_error != 0)
  {
    printf("%s\n", packetHandler->getRxPacketError(siar_error));
  }

  siar_comm_result = packetHandler->write2ByteTxRx(portHandler, SIAR_ID, ADDR_PRO_INDIRECTADDRESS_FOR_READ + 8, ADDR_PRO_MOVING, &siar_error);
  if (siar_comm_result != COMM_SUCCESS)
  {
    printf("%s\n", packetHandler->getTxRxResult(siar_comm_result));
  }
  else if (siar_error != 0)
  {
    printf("%s\n", packetHandler->getRxPacketError(siar_error));
  }

  // Enable siar Torque
  siar_comm_result = packetHandler->write1ByteTxRx(portHandler, SIAR_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_ENABLE, &siar_error);
  if (siar_comm_result != COMM_SUCCESS)
  {
    printf("%s\n", packetHandler->getTxRxResult(siar_comm_result));
  }
  else if (siar_error != 0)
  {
    printf("%s\n", packetHandler->getRxPacketError(siar_error));
  }

  // Add parameter storage for the present position value
  siar_addparam_result = groupSyncRead.addParam(SIAR_ID);
  if (siar_addparam_result != true)
  {
    fprintf(stderr, "[ID:%03d] groupSyncRead addparam failed\n", SIAR_ID);
    return 0;
  }

  while(1)
  {
    printf("Press any key to continue! (or press ESC to quit!)\n");
    if (getch() == ESC_ASCII_VALUE)
      break;

    // Allocate LED and goal position value into byte array
    param_indirect_data_for_write[0] = SIAR_LOBYTE(SIAR_LOWORD(siar_goal_position[index]));
    param_indirect_data_for_write[1] = SIAR_HIBYTE(SIAR_LOWORD(siar_goal_position[index]));
    param_indirect_data_for_write[2] = SIAR_LOBYTE(SIAR_HIWORD(siar_goal_position[index]));
    param_indirect_data_for_write[3] = SIAR_HIBYTE(SIAR_HIWORD(siar_goal_position[index]));
    param_indirect_data_for_write[4] = siar_led_value[index];

    // Add values to the Syncwrite storage
    siar_addparam_result = groupSyncWrite.addParam(SIAR_ID, param_indirect_data_for_write);
    if (siar_addparam_result != true)
    {
      fprintf(stderr, "[ID:%03d] groupSyncWrite addparam failed\n", SIAR_ID);
      return 0;
    }

    // Syncwrite all
    siar_comm_result = groupSyncWrite.txPacket();
    if (siar_comm_result != COMM_SUCCESS) printf("%s\n", packetHandler->getTxRxResult(siar_comm_result));

    // Clear syncwrite parameter storage
    groupSyncWrite.clearParam();

    do
    {
      // Syncread present position from indirectdata2
      siar_comm_result = groupSyncRead.txRxPacket();
      if (siar_comm_result != COMM_SUCCESS) printf("%s\n", packetHandler->getTxRxResult(siar_comm_result));

      // Check if groupsyncread data of Dyanamixel is available
      siar_getdata_result = groupSyncRead.isAvailable(SIAR_ID, ADDR_PRO_INDIRECTDATA_FOR_READ, LEN_PRO_PRESENT_POSITION);
      if (siar_getdata_result != true)
      {
        fprintf(stderr, "[ID:%03d] groupSyncRead getdata failed", SIAR_ID);
        return 0;
      }

      // Check if groupsyncread data of Dyanamixel is available
      siar_getdata_result = groupSyncRead.isAvailable(SIAR_ID, ADDR_PRO_INDIRECTDATA_FOR_READ + LEN_PRO_PRESENT_POSITION, LEN_PRO_MOVING);
      if (siar_getdata_result != true)
      {
        fprintf(stderr, "[ID:%03d] groupSyncRead getdata failed", SIAR_ID);
        return 0;
      }

      // Get siar present position value
      siar_present_position = groupSyncRead.getData(SIAR_ID, ADDR_PRO_INDIRECTDATA_FOR_READ, LEN_PRO_PRESENT_POSITION);

      // Get siar moving status value
      siar_moving = groupSyncRead.getData(SIAR_ID, ADDR_PRO_INDIRECTDATA_FOR_READ + LEN_PRO_PRESENT_POSITION, LEN_PRO_MOVING);

      printf("[ID:%03d] GoalPos:%d  PresPos:%d  IsMoving:%d\n", SIAR_ID, siar_goal_position[index], siar_present_position, siar_moving);

    }while(abs(siar_goal_position[index] - siar_present_position) > SIAR_MOVING_STATUS_THRESHOLD);

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

  // Disable siar Torque
  siar_comm_result = packetHandler->write1ByteTxRx(portHandler, SIAR_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_DISABLE, &siar_error);
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
