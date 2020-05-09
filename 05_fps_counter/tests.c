#include <unistd.h>
#include <assert.h>
#include <stdio.h>
#include "time.h"

void diffTimeCalculatesTimeDifferenceCorrectly() {
  Timespec s = currentTime();
  usleep(75000);
  Timespec e = currentTime();

  double diff = diffTime(s, e);
  printf("diff: %f\n", diff);
  assert(diff > 0.075 && diff < 0.1 && "diff should be atleast 75ms and less than 100ms");

  Timespec s2 = currentTime();
  usleep(175000);
  Timespec e2 = currentTime();

  double diff2 = diffTime(e2, s2);
  printf("diff2: %f\n", diff2);
  assert(diff2 > 0.175 && diff2 < 0.2 && "diff should be atleast 175ms and less than 200ms");

  Timespec s3 = currentTime();
  usleep(1750000);
  Timespec e3 = currentTime();

  double diff3 = diffTime(e3, s3);
  printf("diff3: %f\n", diff3);
  assert(diff3 > 1.75 && diff3 < 1.8 && "diff should be atleast 1.75s and less than 1.8s");
}

void timespecAddWorksAsExpected() {
  Timespec ts1 = {
    .tv_sec = 0,
    .tv_nsec = 8e8
  };

  Timespec ts2 = {
    .tv_sec = 0,
    .tv_nsec = 8e8
  };

  Timespec added1 = timespecAdd(ts1, ts2);
  assert(added1.tv_sec == 1 && added1.tv_nsec == 6e8 && "tv_sec should be 1 tv_nsec 6e8");
}

int main(int argc, char ** argv)
{
  timespecAddWorksAsExpected();

  diffTimeCalculatesTimeDifferenceCorrectly();

  return 0;
}
