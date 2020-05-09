require sleep.fs
require time.fs

: microseconds-to-seconds-works-as-expected
  utime
  100000 usleep drop
  utime
  2swap d-
  microseconds-to-seconds
  fdup
  assert( 0.1e f> )
  assert( 0.12e f< )
;
  
: calculate-frame-time-sets-acc-as-seconds-passed-after-init-time
  init-time
  75000 usleep drop
  calculate-frame-time
  assert( acc@ 0.075e f> )
  assert( acc@ 0.08e f< )
;

: seconds-after-init-time-or-last-call-works-as-expected
  init-time
  75000 usleep drop
  seconds-after-init-time-or-last-call
  fdup
  assert( 0.075e f> )
  assert( 0.08e f< )

  50000 usleep drop
  seconds-after-init-time-or-last-call
  fdup
  assert( 0.05e f> )
  assert( 0.06e f< )
;


microseconds-to-seconds-works-as-expected

seconds-after-init-time-or-last-call-works-as-expected

calculate-frame-time-sets-acc-as-seconds-passed-after-init-time

bye
