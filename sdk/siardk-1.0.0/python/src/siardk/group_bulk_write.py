#!/usr/bin/env python
# -*- coding: utf-8 -*-

################################################################################
# Copyright 2017 VKROBOT CO., LTD.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
################################################################################

# Author: RWJ (L)

from .vkrobot_def import *


class GroupBulkWrite:
    def __init__(self, port, ph):
        self.port = port
        self.ph = ph

        self.is_param_changed = False
        self.param = []
        self.data_list = {}

        self.clearParam()

    def makeParam(self):
        if self.ph.getProtocolVersion() == 1.0 or not self.data_list:
            return

        self.param = []

        for siar_id in self.data_list:
            if not self.data_list[siar_id]:
                return

            self.param.append(siar_id)
            self.param.append(SIAR_LOBYTE(self.data_list[siar_id][1]))
            self.param.append(SIAR_HIBYTE(self.data_list[siar_id][1]))
            self.param.append(SIAR_LOBYTE(self.data_list[siar_id][2]))
            self.param.append(SIAR_HIBYTE(self.data_list[siar_id][2]))

            self.param.extend(self.data_list[siar_id][0])

    def addParam(self, siar_id, start_address, data_length, data):
        if self.ph.getProtocolVersion() == 1.0:
            return False

        if siar_id in self.data_list:  # siar_id already exist
            return False

        if len(data) > data_length:  # input data is longer than set
            return False

        self.data_list[siar_id] = [data, start_address, data_length]

        self.is_param_changed = True
        return True

    def removeParam(self, siar_id):
        if self.ph.getProtocolVersion() == 1.0:
            return

        if siar_id not in self.data_list:  # NOT exist
            return

        del self.data_list[siar_id]

        self.is_param_changed = True

    def changeParam(self, siar_id, start_address, data_length, data):
        if self.ph.getProtocolVersion() == 1.0:
            return False

        if siar_id not in self.data_list:  # NOT exist
            return False

        if len(data) > data_length:  # input data is longer than set
            return False

        self.data_list[siar_id] = [data, start_address, data_length]

        self.is_param_changed = True
        return True

    def clearParam(self):
        if self.ph.getProtocolVersion() == 1.0:
            return

        self.data_list.clear()
        return

    def txPacket(self):
        if self.ph.getProtocolVersion() == 1.0 or len(self.data_list.keys()) == 0:
            return COMM_NOT_AVAILABLE

        if self.is_param_changed is True or len(self.param) == 0:
            self.makeParam()

        return self.ph.bulkWriteTxOnly(self.port, self.param, len(self.param))
