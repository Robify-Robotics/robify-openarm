%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
% Copyright 2017 VKROBOT CO., LTD.
%
% Licensed under the Apache License, Version 2.0 (the "License");
% you may not use this file except in compliance with the License.
% You may obtain a copy of the License at
%
%     http://www.apache.org/licenses/LICENSE-2.0
%
% Unless required by applicable law or agreed to in writing, software
% distributed under the License is distributed on an "AS IS" BASIS,
% WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
% See the License for the specific language governing permissions and
% limitations under the License.
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

% Author: Ryu Woon Jung (Leon)

%
% protocol_combined.c
%
%  Created on: 2016. 6. 7.
%      Author: Ryu Woon Jung (Leon)
%

%
% *********     Protocol Combined Example      *********
%
%
% Available siar model on this example : All models using Protocol 1.0 and 2.0
% This example is tested with a siar MX-28, a siar PRO 54-200 and an USB2siar
% Be sure that properties of siar MX and PRO are already set as %% MX - ID : 1 / Baudnum : 34 (Baudrate : 57600) , PRO - ID : 1 / Baudnum : 1 (Baudrate : 57600)
%

% Be aware that:
% This example configures two different control tables (especially, if it uses siar and siar PRO). It may modify critical siar parameter on the control table, if siars have wrong ID.
%

clc;
clear all;

lib_name = '';

if strcmp(computer, 'PCWIN')
  lib_name = 'siar_x86_c';
elseif strcmp(computer, 'PCWIN64')
  lib_name = 'siar_x64_c';
elseif strcmp(computer, 'GLNX86')
  lib_name = 'libsiar_x86_c';
elseif strcmp(computer, 'GLNXA64')
  lib_name = 'libsiar_x64_c';
elseif strcmp(computer, 'MACI64')
  lib_name = 'libsiar_mac_c';
end

% Load Libraries
if ~libisloaded(lib_name)
    [notfound, warnings] = loadlibrary(lib_name, 'siardk.h', 'addheader', 'port_handler.h', 'addheader', 'packet_handler.h');
end

% Control table address for siar MX
ADDR_MX_TORQUE_ENABLE           = 24;             % Control table address is different in siar model
ADDR_MX_GOAL_POSITION           = 30;
ADDR_MX_PRESENT_POSITION        = 36;

% Control table address for siar PRO
ADDR_PRO_TORQUE_ENABLE          = 512;
ADDR_PRO_GOAL_POSITION          = 532;
ADDR_PRO_PRESENT_POSITION       = 611;

% Protocol version
PROTOCOL_VERSION1               = 1.0;            % See which protocol version is used in the siar
PROTOCOL_VERSION2               = 2.0;

% Default setting
SIAR1_ID                         = 1;              % siar#1 ID: 1
SIAR2_ID                         = 2;              % siar#2 ID: 2
BAUDRATE                        = 57600;
DEVICENAME                      = 'COM1';         % Check which port is being used on your controller
                                                  % ex) Windows: 'COM1'   Linux: '/dev/ttyUSB0'

TORQUE_ENABLE                   = 1;              % Value for enabling the torque
TORQUE_DISABLE                  = 0;              % Value for disabling the torque
SIAR1_MINIMUM_POSITION_VALUE     = 100;            % siar will rotate between this value
SIAR1_MAXIMUM_POSITION_VALUE     = 4000;           % and this value (note that the siar would not move when the position value is out of movable range. Check e-manual about the range of the siar you use.)
SIAR2_MINIMUM_POSITION_VALUE     = -150000;
SIAR2_MAXIMUM_POSITION_VALUE     = 150000;
SIAR1_MOVING_STATUS_THRESHOLD    = 10;             % siar MX moving status threshold
SIAR2_MOVING_STATUS_THRESHOLD    = 20;             % siar PRO moving status threshold

ESC_CHARACTER                   = 'e';            % Key for escaping loop

COMM_SUCCESS                    = 0;              % Communication Success result value
COMM_TX_FAIL                    = -1001;          % Communication Tx Failed


% Initialize PortHandler Structs
% Set the port path
% Get methods and members of PortHandlerLinux or PortHandlerWindows
port_num = portHandler(DEVICENAME);

% Initialize PacketHandler Structs
packetHandler();

index = 1;
siar_comm_result = COMM_TX_FAIL;                   % Communication result
siar1_goal_position = [SIAR1_MINIMUM_POSITION_VALUE SIAR1_MAXIMUM_POSITION_VALUE];         % Goal position of siar MX
siar2_goal_position = [SIAR2_MINIMUM_POSITION_VALUE SIAR2_MAXIMUM_POSITION_VALUE];         % Goal position of siar PRO

siar_error = 0;                                    % siar error
siar1_present_position = 0;                        % Present position of siar MX
siar2_present_position = 0;                        % Present position of siar PRO

% Open port
if (openPort(port_num))
    fprintf('Succeeded to open the port!\n');
else
    unloadlibrary(lib_name);
    fprintf('Failed to open the port!\n');
    input('Press any key to terminate...\n');
    return;
end


% Set port baudrate
if (setBaudRate(port_num, BAUDRATE))
    fprintf('Succeeded to change the baudrate!\n');
else
    unloadlibrary(lib_name);
    fprintf('Failed to change the baudrate!\n');
    input('Press any key to terminate...\n');
    return;
end


% Enable siar#1 torque
write1ByteTxRx(port_num, PROTOCOL_VERSION1, SIAR1_ID, ADDR_MX_TORQUE_ENABLE, TORQUE_ENABLE);
siar_comm_result = getLastTxRxResult(port_num, PROTOCOL_VERSION1);
siar_error = getLastRxPacketError(port_num, PROTOCOL_VERSION1);
if siar_comm_result ~= COMM_SUCCESS
    fprintf('%s\n', getTxRxResult(PROTOCOL_VERSION1, siar_comm_result));
elseif siar_error ~= 0
    fprintf('%s\n', getRxPacketError(PROTOCOL_VERSION1, siar_error));
else
    fprintf('siar #%d has been successfully connected \n', SIAR1_ID);
end

% Enable siar#2 torque
write1ByteTxRx(port_num, PROTOCOL_VERSION2, SIAR2_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_ENABLE);
siar_comm_result = getLastTxRxResult(port_num, PROTOCOL_VERSION2);
siar_error = getLastRxPacketError(port_num, PROTOCOL_VERSION2);
if siar_comm_result ~= COMM_SUCCESS
    fprintf('%s\n', getTxRxResult(PROTOCOL_VERSION2, siar_comm_result));
elseif siar_error ~= 0
    fprintf('%s\n', getRxPacketError(PROTOCOL_VERSION2, siar_error));
else
    fprintf('siar #%d has been successfully connected \n', SIAR2_ID);
end


while 1
    if input('Press any key to continue! (or input e to quit!)\n', 's') == ESC_CHARACTER
        break;
    end

    % Write siar#1 goal position
    write2ByteTxRx(port_num, PROTOCOL_VERSION1, SIAR1_ID, ADDR_MX_GOAL_POSITION, siar1_goal_position(index));
    siar_comm_result = getLastTxRxResult(port_num, PROTOCOL_VERSION1);
    siar_error = getLastRxPacketError(port_num, PROTOCOL_VERSION1);
    if siar_comm_result ~= COMM_SUCCESS
        fprintf('%s\n', getTxRxResult(PROTOCOL_VERSION1, siar_comm_result));
    elseif siar_error ~= 0
        fprintf('%s\n', getRxPacketError(PROTOCOL_VERSION1, siar_error));
    end

    % Write siar#2 goal position
    write4ByteTxRx(port_num, PROTOCOL_VERSION2, SIAR2_ID, ADDR_PRO_GOAL_POSITION, typecast(int32(siar2_goal_position(index)), 'uint32'));
    siar_comm_result = getLastTxRxResult(port_num, PROTOCOL_VERSION2);
    siar_error = getLastRxPacketError(port_num, PROTOCOL_VERSION2);
    if siar_comm_result ~= COMM_SUCCESS
        fprintf('%s\n', getTxRxResult(PROTOCOL_VERSION2, siar_comm_result));
    elseif siar_error ~= 0
        fprintf('%s\n', getRxPacketError(PROTOCOL_VERSION2, siar_error));
    end

    while 1
      % Read siar#1 present position
    	siar1_present_position = read2ByteTxRx(port_num, PROTOCOL_VERSION1, SIAR1_ID, ADDR_MX_PRESENT_POSITION);
      siar_comm_result = getLastTxRxResult(port_num, PROTOCOL_VERSION1);
      siar_error = getLastRxPacketError(port_num, PROTOCOL_VERSION1);
      if siar_comm_result ~= COMM_SUCCESS
          fprintf('%s\n', getTxRxResult(PROTOCOL_VERSION1, siar_comm_result));
      elseif siar_error ~= 0
          fprintf('%s\n', getRxPacketError(PROTOCOL_VERSION1, siar_error));
      end

      % Read siar#2 present position
      siar2_present_position = read4ByteTxRx(port_num, PROTOCOL_VERSION2, SIAR2_ID, ADDR_PRO_PRESENT_POSITION);
      siar_comm_result = getLastTxRxResult(port_num, PROTOCOL_VERSION2);
      siar_error = getLastRxPacketError(port_num, PROTOCOL_VERSION2);
      if siar_comm_result ~= COMM_SUCCESS
          fprintf('%s\n', getTxRxResult(PROTOCOL_VERSION2, siar_comm_result));
      elseif siar_error ~= 0
          fprintf('%s\n', getRxPacketError(PROTOCOL_VERSION2, siar_error));
      end

      fprintf('[ID:%03d] GoalPos:%03d  PresPos:%03d [ID:%03d] GoalPos:%03d  PresPos:%03d\n', SIAR1_ID, siar1_goal_position(index), siar1_present_position, SIAR2_ID, siar2_goal_position(index), typecast(uint32(siar2_present_position), 'int32'));

      if ~((abs(siar1_goal_position(index) - siar1_present_position) > SIAR1_MOVING_STATUS_THRESHOLD) || (abs(siar2_goal_position(index) - typecast(uint32(siar2_present_position), 'int32')) > SIAR2_MOVING_STATUS_THRESHOLD));
          break;
      end
    end

    % Change goal position
    if index == 1
        index = 2;
    else
        index = 1;
    end
end

  % Disable siar#1 Torque
  write1ByteTxRx(port_num, PROTOCOL_VERSION1, SIAR1_ID, ADDR_MX_TORQUE_ENABLE, TORQUE_DISABLE);
  siar_comm_result = getLastTxRxResult(port_num, PROTOCOL_VERSION1);
  siar_error = getLastRxPacketError(port_num, PROTOCOL_VERSION1);
  if siar_comm_result ~= COMM_SUCCESS
      fprintf('%s\n', getTxRxResult(PROTOCOL_VERSION1, siar_comm_result));
  elseif siar_error ~= 0
      fprintf('%s\n', getRxPacketError(PROTOCOL_VERSION1, siar_error));
end

  % Disable siar#2 Torque
  write1ByteTxRx(port_num, PROTOCOL_VERSION2, SIAR2_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_DISABLE);
  siar_comm_result = getLastTxRxResult(port_num, PROTOCOL_VERSION2);
  siar_error = getLastRxPacketError(port_num, PROTOCOL_VERSION2);
  if siar_comm_result ~= COMM_SUCCESS
      fprintf('%s\n', getTxRxResult(PROTOCOL_VERSION2, siar_comm_result));
  elseif siar_error ~= 0
      fprintf('%s\n', getRxPacketError(PROTOCOL_VERSION2, siar_error));
end


% Close port
closePort(port_num);

% Unload Library
unloadlibrary(lib_name);

close all;
clear all;
