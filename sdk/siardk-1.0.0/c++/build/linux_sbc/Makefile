##################################################
# PROJECT: SIARDK - VKROBOT CO., Ltd.
##################################################

#---------------------------------------------------------------------
# C++ COMPILER, COMPILER FLAGS, AND TARGET PROGRAM NAME
#---------------------------------------------------------------------
DIR_SIAR     = ../..
DIR_OBJS    = ./.objects

INSTALL_ROOT = /usr/local

MAJ_VERSION = 2
MIN_VERSION = 0
REV_VERSION = 0

TARGET      = libsiar_sbc_cpp.so
TARGET1     = $(TARGET).$(MAJ_VERSION)
TARGET2     = $(TARGET).$(MAJ_VERSION).$(MIN_VERSION)
TARGET3     = $(TARGET).$(MAJ_VERSION).$(MIN_VERSION).$(REV_VERSION)

CHK_DIR_EXISTS = test -d
PRINT       = echo
STRIP       = strip
AR          = ar
ARFLAGS     = cr
LD          = g++
LDFLAGS     = -shared -fPIC #-Wl,-soname,siar
LD_CONFIG   = ldconfig
CP          = cp
CP_ALL      = cp -r
RM          = rm
RM_ALL      = rm -rf
SYMLINK     = ln -s
MKDIR       = mkdir
CC          = gcc
CX          = g++
CCFLAGS     = -O2 -O3 -DLINUX -D_GNU_SOURCE -Wall -c $(INCLUDES) -fPIC -g
CXFLAGS     = -O2 -O3 -DLINUX -D_GNU_SOURCE -Wall -c $(INCLUDES) -fPIC -g
INCLUDES    += -I$(DIR_SIAR)/include/siardk

#---------------------------------------------------------------------
# Required external libraries
#---------------------------------------------------------------------
LIBRARIES  += -lrt

#---------------------------------------------------------------------
# SDK Files
#---------------------------------------------------------------------
SOURCES  = src/siardk/group_bulk_read.cpp \
           src/siardk/group_bulk_write.cpp \
           src/siardk/group_sync_read.cpp \
           src/siardk/group_sync_write.cpp \
           src/siardk/packet_handler.cpp \
           src/siardk/port_handler.cpp \
           src/siardk/protocol1_packet_handler.cpp \
           src/siardk/protocol2_packet_handler.cpp \
           src/siardk/port_handler_linux.cpp \


OBJECTS=$(addsuffix .o,$(addprefix $(DIR_OBJS)/,$(basename $(notdir $(SOURCES)))))


#---------------------------------------------------------------------
# COMPILING RULES
#---------------------------------------------------------------------
$(TARGET): makedirs $(OBJECTS)
	$(LD) $(LDFLAGS) -o ./$(TARGET) $(OBJECTS) $(LIBRARIES)

makedirs:
	mkdir -p $(DIR_OBJS)/

clean:
	rm -f $(OBJECTS) ./$(TARGET)

install: $(TARGET)
    # copy the binaries into the lib directory
	@$(CHK_DIR_EXISTS) $(INSTALL_ROOT)/lib || $(MKDIR) $(INSTALL_ROOT)/lib
	-$(CP) "./$(TARGET)" "$(INSTALL_ROOT)/lib/$(TARGET)"
	-$(SYMLINK) "$(INSTALL_ROOT)/lib/$(TARGET)" "$(INSTALL_ROOT)/lib/$(TARGET1)"
	-$(SYMLINK) "$(INSTALL_ROOT)/lib/$(TARGET)" "$(INSTALL_ROOT)/lib/$(TARGET2)"
	-$(SYMLINK) "$(INSTALL_ROOT)/lib/$(TARGET)" "$(INSTALL_ROOT)/lib/$(TARGET3)"

    # copy the headers into the include directory
	@$(CHK_DIR_EXISTS) $(INSTALL_ROOT)/include/siardk || $(MKDIR) $(INSTALL_ROOT)/include/siardk
	$(CP_ALL) $(DIR_SIAR)/include/siardk/* $(INSTALL_ROOT)/include/siardk/

	$(LD_CONFIG)

uninstall:
	$(RM) $(INSTALL_ROOT)/lib/$(TARGET)
	$(RM) $(INSTALL_ROOT)/lib/$(TARGET1)
	$(RM) $(INSTALL_ROOT)/lib/$(TARGET2)
	$(RM) $(INSTALL_ROOT)/lib/$(TARGET3)

	$(RM) $(INSTALL_ROOT)/include/siardk/siardk.h
	$(RM_ALL) $(INSTALL_ROOT)/include/siardk/*

reinstall: uninstall install


#---------------------------------------------------------------------
# Make rules for all .c and .cpp files in each directory
#---------------------------------------------------------------------

$(DIR_OBJS)/%.o: $(DIR_SIAR)/src/siardk/%.c
	$(CC) $(CCFLAGS) -c $? -o $@

$(DIR_OBJS)/%.o: $(DIR_SIAR)/src/siardk/%.cpp
	$(CX) $(CXFLAGS) -c $? -o $@

$(DIR_OBJS)/%.o: $(DIR_SIAR)/src/siardk/%.c
	$(CC) $(CCFLAGS) -c $? -o $@

$(DIR_OBJS)/%.o: $(DIR_SIAR)/src/siardk/%.cpp
	$(CX) $(CXFLAGS) -c $? -o $@

#---------------------------------------------------------------------
# END OF MAKEFILE
#---------------------------------------------------------------------
