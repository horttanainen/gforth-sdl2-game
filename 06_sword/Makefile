# the compiler to use
CC = clang
  
# compiler flags:
CFLAGS  = -Wall -I/usr/local/include -D_THREAD_SAFE
  
#files to link:
LFLAGS = $(shell sdl2-config --libs)
  
TARGET = game
  
all: $(TARGET)
$(TARGET): $(TARGET).c ; $(CC) $(CFLAGS) -o $(TARGET) $(TARGET).c $(LFLAGS) mathc.c

TESTS_TARGET = tests

all: $(TESTS_TARGET)
$(TESTS_TARGET): $(TESTS_TARGET).c ; $(CC) -o $(TESTS_TARGET) $(TESTS_TARGET).c
