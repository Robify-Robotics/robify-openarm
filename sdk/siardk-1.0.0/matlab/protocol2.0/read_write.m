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
% *********     Read and Write Example      *********
%
%
% Available siar model on this example : All models using Protocol 2.0
% This example is designed for using a siar PRO 54-200, and an USB2siar.
% To use another siar model, such as X series, see their details in E-Manual(emanual.vkrobot.com) and edit below variables yourself.
% Be sure that siar PRO properties are already set as %% ID : 1 / Baudnum : 1 (Baudrate : 57600)
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

% Control table address
ADDR_PRO_TORQUE_ENABLE       = 512;         % Control table address is different in siar model
ADDR_PRO_GOAL_POSITION       = 532;
ADDR_PRO_PRESENT_POSITION    = 611;

% Protocol version
PROTOCOL_VERSION            = 2.0;          % See which protocol version is used in the siar

% Default setting
SIAR_ID                      = 1;            % siar ID: 1
BAUDRATE                    = 57600;
DEVICENAME                  = 'COM1';       % Check which port is being used on your controller
                                            % ex) Windows: 'COM1'   Linux: '/dev/ttyUSB0' Mac: '/dev/tty.usbserial-*'

TORQUE_ENABLE               = 1;            % Value for enabling the torque
TORQUE_DISABLE              = 0;            % Value for disabling the torque
SIAR_MINIMUM_POSITION_VALUE  = -150000;      % siar will rotate between this value
SIAR_MAXIMUM_POSITION_VALUE  = 150000;       % and this value (note that the siar would not move when the position value is out of movable range. Check e-manual about the range of the siar you use.)
SIAR_MOVING_STATUS_THRESHOLD = 20;           % siar moving status threshold

ESC_CHARACTER               = 'e';          % Key for escaping loop

COMM_SUCCESS                = 0;            % Communication Success result value
COMM_TX_FAIL                = -1001;        % Communication Tx Failed

% Initialize PortHandler Structs
% Set the port path
% Get methods and members of PortHandlerLinux or PortHandlerWindows
port_num = portHandler(DEVICENAME);

% Initialize PacketHandler Structs
packetHandler();

index = 1;
siar_comm_result = COMM_TX_FAIL;           % Communication result
siar_goal_position = [SIAR_MINIMUM_POSITION_VALUE SIAR_MAXIMUM_POSITION_VALUE];         % Goal position

siar_error = 0;                              % siar error
siar_present_position = 0;                   % Present position


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


% Enable siar Torque
write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_ENABLE);
siar_comm_result = getLastTxRxResult(port_num, PROTOCOL_VERSION);
siar_error = getLastRxPacketError(port_num, PROTOCOL_VERSION);
if siar_comm_result ~= COMM_SUCCESS
    fprintf('%s\n', getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
elseif siar_error ~= 0
    fprintf('%s\n', getRxPacketError(PROTOCOL_VERSION, siar_error));
else
    fprintf('siar has been successfully connected \n');
end


while 1
    if input('Press any key to continue! (or input e to quit!)\n', 's') == ESC_CHARACTER
        break;
    end

    % Write goal position
    write4ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRO_GOAL_POSITION, typecast(int32(siar_goal_position(index)), 'uint32'));
    siar_comm_result = getLastTxRxResult(port_num, PROTOCOL_VERSION);
    siar_error = getLastRxPacketError(port_num, PROTOCOL_VERSION);
    if siar_comm_result ~= COMM_SUCCESS
        fprintf('%s\n', getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
    elseif siar_error ~= 0
        fprintf('%s\n', getRxPacketError(PROTOCOL_VERSION, siar_error));
    end

    while 1
        % Read present position
        siar_present_position = read4ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRO_PRESENT_POSITION);
        siar_comm_result = getLastTxRxResult(port_num, PROTOCOL_VERSION);
        siar_error = getLastRxPacketError(port_num, PROTOCOL_VERSION);
        if siar_comm_result ~= COMM_SUCCESS
            fprintf('%s\n', getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
        elseif siar_error ~= 0
            fprintf('%s\n', getRxPacketError(PROTOCOL_VERSION, siar_error));
        end

        fprintf('[ID:%03d] GoalPos:%03d  PresPos:%03d\n', SIAR_ID, siar_goal_position(index), typecast(uint32(siar_present_position), 'int32'));

        if ~(abs(siar_goal_position(index) - typecast(uint32(siar_present_position), 'int32')) > SIAR_MOVING_STATUS_THRESHOLD)
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


% Disable siar Torque
write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRO_TORQUE_ENABLE, TORQUE_DISABLE);
siar_comm_result = getLastTxRxResult(port_num, PROTOCOL_VERSION);
siar_error = getLastRxPacketError(port_num, PROTOCOL_VERSION);
if siar_comm_result ~= COMM_SUCCESS
    fprintf('%s\n', getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
elseif siar_error ~= 0
    fprintf('%s\n', getRxPacketError(PROTOCOL_VERSION, siar_error));
end

% Close port
closePort(port_num);

% Unload Library
unloadlibrary(lib_name);

close all;
clear all;
