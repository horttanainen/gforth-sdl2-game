decimal
require sdl2.fs

: +f! { addr -- }
  addr f@
  f+
  addr f!
;

struct
  float% field dt'
  double% field t'
  float% field acc'
end-struct time%

time% %allot constant time

: dt@ time dt' f@ ;
: dt! time dt' f! ;
: t@ time t' 2@ ;
: t! time t' 2! ;
: acc@ time acc' f@ ;
: acc! time acc' f! ;

: init-time ( -- )
  0.01e dt!
  utime t!
  0e acc!
;

struct
  float% field pos-x
  float% field pos-y
  float% field ax
  float% field ay
  float% field vx
  float% field vy
  float% field f
end-struct player%

player% %allot constant player
player% %allot constant interpolated-player
player% %allot value previous-player

: init-player ( -- )
  288e player pos-x f!
  208e player pos-y f!
  0e player ax f!
  0e player ay f!
  0e player vx f!
  0e player vy f!
  10e player f f!
;

0 value window
0 value renderer
0 value texture

sdl-event% %allot constant event

false value quit-flag

: init-sdl ( -- )
  SDL_INIT_VIDEO sdl-init 0< if
    drop
    cr ." Video Initialization failed: "
    sdl-geterror type cr
    bye
  then
;

: create-window ( -- window )
  s" game" SDL_WINDOWPOS_UNDEFINED SDL_WINDOWPOS_UNDEFINED 640 480 0 sdl-create-window
  dup 0= if                   \ error out if not successful
    drop
    cr ." Could not create window: "
    sdl-geterror type cr
    sdl-quit
    bye
  then
;

: create-renderer ( -- renderer )
  window -1 0 sdl-create-renderer
  dup 0= if                   \ error out if not successful
    drop
    cr ." Could not create renderer: "
    sdl-geterror type cr
    window sdl-destroy-window
    sdl-quit
    bye
  then
;


: load-texture ( -- texture )
  renderer
  s" man.bmp" sdl-load-bmp 
  tuck
  sdl-create-texture-from-surface
  swap
  sdl-free-surface
  dup 0= if                   \ error out if not successful
    drop
    cr ." Could not create texture: "
    sdl-geterror type cr
    renderer sdl-destroy-renderer
    window sdl-destroy-window
    sdl-quit
    bye
  then
;

: pending-events? ( -- flag )
  event sdl-poll-event
;

: should-quit? ( -- flag )
  quit-flag 0<>
;

: key-pressed? ( keycode keyboard-state -- flag )
  + c@ 0<>
;

: 2-keys-pressed? ( keycode1 keycode2 keyboard-state -- )
  dup ( keycode1 keycode2 keyboard-state keyboard-state -- )
  -rot ( keycode1 keyboard-state keycode2 keyboard-state -- )
  key-pressed? ( keycode1 keyboard-state flag -- )
  -rot ( flag keycode1 keyboard-state )
  key-pressed?
  and
;

: handle-x-movement ( keyboard-state -- )
  dup SDL_SCANCODE_A SDL_SCANCODE_D rot 2-keys-pressed?
  if
    0e player ax f!
    drop
  else
    dup SDL_SCANCODE_A key-pressed?
    if
      player f f@ fnegate player ax f!
      drop
    else
      SDL_SCANCODE_D key-pressed?
      if 
        player f f@ player ax f!
      else
        0e player ax f!
      endif
    endif
  endif
;

: handle-y-movement ( keyboard-state -- )
  dup SDL_SCANCODE_W SDL_SCANCODE_S rot 2-keys-pressed?
  if
    0e player ay f!
    drop
  else
    dup SDL_SCANCODE_W key-pressed?
    if
      player f f@ fnegate player ay f!
      drop
    else
      SDL_SCANCODE_S key-pressed?
      if 
        player f f@ player ay f!
      else
        0e player ay f!
      endif
    endif
  endif
;

: handle-keyboard ( -- )
  0 sdl-get-keyboard-state
  dup handle-x-movement
  handle-y-movement
;


: handle-events ( -- )
  begin
    pending-events?
  while
    event sdl-event-type ul@
    case
      SDL_QUIT of     \ window close box clicked, or ALT-F4 pressed
        true to quit-flag
      endof
    endcase
  repeat
  handle-keyboard
;

sdl-rect% %allot constant 'player-rect
: player-rect ( -- 'player-rect ) 
  interpolated-player pos-x f@ fround f>s 'player-rect x !
  interpolated-player pos-y f@ fround f>s 'player-rect y !
  64 'player-rect w !
  64 'player-rect h !
  'player-rect
;

: render-player ( -- ) 
  renderer
  texture
  0
  player-rect
  sdl-render-copy drop
;

: render ( -- ) 
  renderer sdl-render-clear drop
  render-player 
  renderer sdl-render-present
;

: set-background-color ( -- ) 
  renderer 255 255 255 255 sdl-set-render-draw-color drop
;
: calculate-time ( -- )
  utime ( -- dtime )
  2dup ( -- dtime dtime)
  t@ ( -- dtime dtime t )
  d- ( -- dtime frame-t )
  dabs ( -- dtime frame-t )
  d>f ( -- dtime fframe-t )
  0.25e fmin ( -- dtime fframe-t )
  acc!
  t!
;

: integrate ( -- )
  calculate-time
  begin
    acc@ dt@ f>=
  while
    player to previous-player

    player ax f@ dt@ f* player vx +f!
    player vx f@ dt@ f* player pos-x +f!
    player ay f@ dt@ f* player vy +f!
    player vy f@ dt@ f* player pos-y +f!

    acc@ dt@ f- acc!
  repeat
;

: graceful-quit ( -- )
  texture sdl-destroy-texture
  renderer sdl-destroy-renderer
  window sdl-destroy-window
  sdl-quit
;

: interpolate-player-pos-x { F: alpha -- }
  1e alpha f- previous-player pos-x f@ f*
  alpha player pos-x f@ f*
  f+
;

: interpolate-player-pos-y { F: alpha -- }
  1e alpha f- previous-player pos-y f@ f*
  alpha player pos-y f@ f*
  f+
;

: interpolate-player
  acc@ dt@ f/ ( -- alpha )
  fdup ( -- alpha alpha )
  interpolate-player-pos-x interpolated-player pos-x f!
  interpolate-player-pos-y interpolated-player pos-y f!
;

: main-loop
  begin
    integrate
    interpolate-player
    render
    handle-events
    should-quit?
  until
;

init-sdl 
create-window to window
create-renderer to renderer
load-texture to texture
set-background-color
init-player
init-time
main-loop
graceful-quit
bye
