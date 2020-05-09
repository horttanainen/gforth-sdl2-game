struct
  float% field dt'
  float% field t'
  float% field acc'
end-struct time%

time% %allot constant time

: dt@ time dt' f@ ;
: dt! time dt' f! ;
: t@ time t' f@ ;
: t! time t' f! ;
: acc@ time acc' f@ ;
: acc! time acc' f! ;

: microseconds-to-seconds ( dmicro -- fsec)
  d>f ( -- dtime fframe-t )
  1e6 f/
;

: time-now ( -- seconds )
  utime microseconds-to-seconds
;

: init-time ( -- )
  0.01e dt!
  time-now t!
  0e acc!
;

: seconds-after-init-time-or-last-call
  time-now
  t@ f- 
  time-now t!
;

: calculate-frame-time ( -- finterval)
  seconds-after-init-time-or-last-call
  0.25e fmin
  acc@ f+ acc!
;
