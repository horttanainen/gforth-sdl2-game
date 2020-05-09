#include <math.h>
#include "timespec-sub.h"

typedef struct timespec Timespec;

Timespec currentTime() {
  Timespec ts;
  clock_gettime(CLOCK_REALTIME, &ts); 
  return ts;
}

Timespec secondsToTimespec(double seconds) {
  time_t intPart = (time_t) seconds;
  long fractionalPart = (seconds - intPart) * 1e9;
  Timespec timespec = {
    .tv_sec = intPart,
    .tv_nsec = fractionalPart
  };
  return timespec;
}

double diffTime(Timespec e, Timespec s) {
  Timespec diff = timespec_sub(e, s);
  return fabs(diff.tv_sec + (diff.tv_nsec / 1.0e9));
}

Timespec timespecAdd(Timespec ts1, Timespec ts2) {
  time_t ts1Seconds = ts1.tv_sec;
  long addedNanoseconds = ts1.tv_nsec + ts2.tv_nsec;
  if (addedNanoseconds >= 1e9) {
    addedNanoseconds -= 1e9;
    ++ts1Seconds;
  }
  Timespec tsAdd = {
    .tv_sec = ts1Seconds + ts2.tv_sec,
    .tv_nsec = addedNanoseconds
  };
  return tsAdd;
}

