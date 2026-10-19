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
% *********     Factory Reset Example      *********
%
%
% Available siar model on this example : All models using Protocol 2.0
% This example is designed for using a siar PRO 54-200, and an USB2siar.
% To use another siar model, such as X series, see their details in E-Manual(emanual.vkrobot.com) and edit below variables yourself.
% Be sure that siar PRO properties are already set as %% ID : 1 / Baudnum : 1 (Baudrate : 57600)
%

% Be aware that:
% This example resets all properties of siar to default values, such as %% ID : 1 / Baudnum : 34 (Baudrate : 57600)
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
ADDR_PRO_BAUDRATE               = 8;            % Control table address is different in siar model

% Protocol version
PROTOCOL_VERSION                = 2.0;          % See which protocol version is used in the siar

% Default setting
SIAR_ID                          = 1;            % siar ID: 1
BAUDRATE                        = 57600;
DEVICENAME                      = 'COM1';       % Check which port is being used on your controller
                                                % ex) Windows: 'COM1'   Linux: '/dev/ttyUSB0' Mac: '/dev/tty.usbserial-*'

FACTORYRST_DEFAULTBAUDRATE      = 57600;        % siar baudrate set by factoryreset
NEW_BAUDNUM                     = 3;            % New baudnum to recover siar baudrate as it was
OPERATION_MODE                  = 1;            % 0xFF : reset all values
                                                % 0x01 : reset all values except ID
                                                % 0x02 : reset all values except ID and baudrate

TORQUE_ENABLE                   = 1;            % Value for enabling the torque
TORQUE_DISABLE                  = 0;            % Value for disabling the torque

COMM_SUCCESS                    = 0;            % Communication Success result value
COMM_TX_FAIL                    = -1001;        % Communication Tx Failed

% Initialize PortHandler Structs
% Set the port path
% Get methods and members of PortHandlerLinux or PortHandlerWindows
port_num = portHandler(DEVICENAME);

% Initialize PacketHandler Structs
packetHandler();

siar_comm_result = COMM_TX_FAIL;                 % Communication result

siar_error = 0;                                  % siar error
siar_baudnum_read = 0;                           % Read baudnum

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


% Read present baudrate of the controller
fprintf('Now the controller baudrate is : %d\n', getBaudRate(port_num));

% Try factoryreset
fprintf('[ID:%03d] Try factoryreset : ', SIAR_ID);
factoryReset(port_num, PROTOCOL_VERSION, SIAR_ID, OPERATION_MODE);
siar_comm_result = getLastTxRxResult(port_num, PROTOCOL_VERSION);
siar_error = getLastRxPacketError(port_num, PROTOCOL_VERSION);
if siar_comm_result ~= COMM_SUCCESS
    fprintf('Aborted\n');
    fprintf('%s\n', getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
elseif siar_error ~= 0
    fprintf('%s\n', getRxPacketError(PROTOCOL_VERSION, siar_error));
end

% Wait for reset
fprintf('Wait for reset...\n');
pause(2);

fprintf('[ID:%03d] factoryReset Success!\n', SIAR_ID);

% Set controller baudrate to siar default baudrate
if (setBaudRate(port_num, FACTORYRST_DEFAULTBAUDRATE))
  fprintf('Succeed to change the controller baudrate to : %d\n', FACTORYRST_DEFAULTBAUDRATE);
else
  fprintf('Failed to change the controller baudrate\n');
  input('Press any key to terminate...\n');
  return;
end

% Read siar baudnum
siar_baudnum_read = read1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRO_BAUDRATE);
siar_comm_result = getLastTxRxResult(port_num, PROTOCOL_VERSION);
siar_error = getLastRxPacketError(port_num, PROTOCOL_VERSION);
if siar_comm_result ~= COMM_SUCCESS
    fprintf('%s\n', getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
elseif siar_error ~= 0
    fprintf('%s\n', getRxPacketError(PROTOCOL_VERSION, siar_error));
else
  fprintf('[ID:%03d] siar baudnum is now : %d\n', SIAR_ID, siar_baudnum_read);
end

% Write new baudnum
write1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRO_BAUDRATE, NEW_BAUDNUM);
siar_comm_result = getLastTxRxResult(port_num, PROTOCOL_VERSION);
siar_error = getLastRxPacketError(port_num, PROTOCOL_VERSION);
if siar_comm_result ~= COMM_SUCCESS
    fprintf('%s\n', getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
elseif siar_error ~= 0
    fprintf('%s\n', getRxPacketError(PROTOCOL_VERSION, siar_error));
else
  fprintf('[ID:%03d] Set siar baudnum to : %d\n', SIAR_ID, NEW_BAUDNUM);
end

% Set port baudrate to BAUDRATE
if (setBaudRate(port_num, BAUDRATE))
  fprintf('Succeed to change the controller baudrate to : %d\n', BAUDRATE);
else
  fprintf('Failed to change the controller baudrate\n');
  input('Press any key to terminate...\n');
  return;
end

pause(0.2);

% Read siar baudnum
siar_baudnum_read = read1ByteTxRx(port_num, PROTOCOL_VERSION, SIAR_ID, ADDR_PRO_BAUDRATE);
siar_comm_result = getLastTxRxResult(port_num, PROTOCOL_VERSION);
siar_error = getLastRxPacketError(port_num, PROTOCOL_VERSION);
if siar_comm_result ~= COMM_SUCCESS
    fprintf('%s\n', getTxRxResult(PROTOCOL_VERSION, siar_comm_result));
elseif siar_error ~= 0
    fprintf('%s\n', getRxPacketError(PROTOCOL_VERSION, siar_error));
else
  fprintf('[ID:%03d] siar baudnum is now : %d\n', SIAR_ID, siar_baudnum_read);
end


% Close port
closePort(port_num);

% Unload Library
unloadlibrary(lib_name);

close all;
clear all;
