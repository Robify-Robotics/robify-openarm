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

//
// *********     Clear Multi-Turn Example      *********
//
//
// Available siar model on this example : MX with Protocol 2.0 (firmware v42 or above), siar X-series (firmware v42 or above)
// This example is tested with a siar XM430-W350-R, and an U2D2
// Be sure that siar properties are already set as %% ID : 1 / Baudnum : 1 (Baudrate : 57600)
//

#if defined(__linux__) || defined(__APPLE__)
#include <fcntl.h>
#include <termios.h>
#include <unistd.h>
#define STDIN_FILENO 0
#elif defined(_WIN32) || defined(_WIN64)
#include <conio.h>
#endif

#include <stdlib.h>
#include <stdio.h>

#include "siardk.h"                                  // Uses siar SDK library

// Control table address
#define ADDR_OPERATING_MODE             11                  // Control table address is different in siar model
#define ADDR_TORQUE_ENABLE              64
#define ADDR_GOAL_POSITION              116
#define ADDR_PRESENT_POSITION           132

// Protocol version
#define PROTOCOL_VERSION                2.0                 // See which protocol version is used in the siar

// Default setting
#define SIAR_ID                          1                   // siar ID: 1
#define BAUDRATE                        57600
#define DEVICENAME                      "/dev/ttyUSB0"      // Check which port is being used on your controller
                                                            // ex) Windows: "COM1"   Linux: "/dev/ttyUSB0" Mac: "/dev/tty.usbserial-*"

#define TORQUE_ENABLE                   1                   // Value for enabling the torque
#define TORQUE_DISABLE                  0                   // Value for disabling the torque
#define MAX_POSITION_VALUE              1048575
#define SIAR_MOVING_STATUS_THRESHOLD     20                  // siar moving status threshold
#define EXT_POSITION_CONTROL_MODE       4                   // Value for extended position control mode (operating mode)

#define ESC_ASCII_VALUE                 0x1b
#define SPACE_ASCII_VALUE               0x20

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

void msecSleep(int waitTime)
{
#if defined(__linux__) || defined(__APPLE__)
  usleep(waitTime * 1000);
#elif defined(_WIN32) || defined(_WIN64)
  _sleep(waitTime);
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

  int siar_comm_result = COMM_TX_FAIL;             // Communication result

  uint8_t siar_error = 0;                          // siar error
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

  // Set operating mode to extended position control mode
  siar_comm_result = packetHandler->write1ByteTxRx(portHandler, SIAR_ID, ADDR_OPERATING_MODE, EXT_POSITION_CONTROL_MODE, &siar_error);
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
    printf("Operating mode changed to extended position control mode. \n");
  }


  // Enable siar Torque
  siar_comm_result = packetHandler->write1ByteTxRx(portHandler, SIAR_ID, ADDR_TORQUE_ENABLE, TORQUE_ENABLE, &siar_error);
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
    printf("siar has been successfully connected \n");
  }

  while(1)
  {
    printf("\nPress any key to continue! (or press ESC to quit!)\n");
    if (getch() == ESC_ASCII_VALUE)
      break;

    printf("  Press SPACE key to clear multi-turn information! (or press ESC to stop!)\n");

    // Write goal position
    siar_comm_result = packetHandler->write4ByteTxRx(portHandler, SIAR_ID, ADDR_GOAL_POSITION, MAX_POSITION_VALUE, &siar_error);
    if (siar_comm_result != COMM_SUCCESS)
    {
      printf("%s\n", packetHandler->getTxRxResult(siar_comm_result));
    }
    else if (siar_error != 0)
    {
      printf("%s\n", packetHandler->getRxPacketError(siar_error));
    }

    do
    {
      // Read present position
      siar_comm_result = packetHandler->read4ByteTxRx(portHandler, SIAR_ID, ADDR_PRESENT_POSITION, (uint32_t*)&siar_present_position, &siar_error);
      if (siar_comm_result != COMM_SUCCESS)
      {
        printf("%s\n", packetHandler->getTxRxResult(siar_comm_result));
      }
      else if (siar_error != 0)
      {
        printf("%s\n", packetHandler->getRxPacketError(siar_error));
      }

      printf("  [ID:%03d] GoalPos:%03d  PresPos:%03d\r", SIAR_ID, MAX_POSITION_VALUE, siar_present_position);

      if (kbhit())
      {
        char c = getch();
        if (c == SPACE_ASCII_VALUE)
        {
          printf("\n  Stop & Clear Multi-Turn Information! \n");

          // Write the present position to the goal position to stop moving
          siar_comm_result = packetHandler->write4ByteTxRx(portHandler, SIAR_ID, ADDR_GOAL_POSITION, siar_present_position, &siar_error);
          if (siar_comm_result != COMM_SUCCESS)
          {
            printf("%s\n", packetHandler->getTxRxResult(siar_comm_result));
          }
          else if (siar_error != 0)
          {
            printf("%s\n", packetHandler->getRxPacketError(siar_error));
          }

          msecSleep(300);

          // Clear Multi-Turn Information
          siar_comm_result = packetHandler->clearMultiTurn(portHandler, SIAR_ID, &siar_error);
          if (siar_comm_result != COMM_SUCCESS)
          {
            printf("%s\n", packetHandler->getTxRxResult(siar_comm_result));
          }
          else if (siar_error != 0)
          {
            printf("%s\n", packetHandler->getRxPacketError(siar_error));
          }

          // Read present position
          siar_comm_result = packetHandler->read4ByteTxRx(portHandler, SIAR_ID, ADDR_PRESENT_POSITION, (uint32_t*)&siar_present_position, &siar_error);
          if (siar_comm_result != COMM_SUCCESS)
          {
            printf("%s\n", packetHandler->getTxRxResult(siar_comm_result));
          }
          else if (siar_error != 0)
          {
            printf("%s\n", packetHandler->getRxPacketError(siar_error));
          }

          printf("  Present Position has been reset. : %03d \n", siar_present_position);

          break;
        }
        else if (c == ESC_ASCII_VALUE)
        {
          printf("\n  Stopped!! \n");

          // Write the present position to the goal position to stop moving
          siar_comm_result = packetHandler->write4ByteTxRx(portHandler, SIAR_ID, ADDR_GOAL_POSITION, siar_present_position, &siar_error);
          if (siar_comm_result != COMM_SUCCESS)
          {
            printf("%s\n", packetHandler->getTxRxResult(siar_comm_result));
          }
          else if (siar_error != 0)
          {
            printf("%s\n", packetHandler->getRxPacketError(siar_error));
          }

          break;
        }
      }

    }while((abs(MAX_POSITION_VALUE - siar_present_position) > SIAR_MOVING_STATUS_THRESHOLD));

    printf("\n");
  }

  // Disable siar Torque
  siar_comm_result = packetHandler->write1ByteTxRx(portHandler, SIAR_ID, ADDR_TORQUE_ENABLE, TORQUE_DISABLE, &siar_error);
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
