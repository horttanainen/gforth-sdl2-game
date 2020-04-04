decimal
require sdl2.fs

: +f! { addr -- }
  addr f@
  f+
  addr f!
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

: handle-event ( -- )
  event sdl-event-type ul@
  case
    SDL_QUIT of     \ window close box clicked, or ALT-F4 pressed
      true to quit-flag
    endof
  endcase
;

sdl-rect% %allot constant 'player-rect
: player-rect ( -- 'player-rect ) 
  player pos-x f@ fround f>s 'player-rect x !
  player pos-y f@ fround f>s 'player-rect y !
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

: should-quit? ( -- flag )
  quit-flag 0<>
;

: pending-events? ( -- flag )
  event sdl-poll-event
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

: integrate
  player ax f@ 0.01e f* player vx +f!
  player vx f@ 0.01e f* player pos-x +f!

  player ay f@ 0.01e f* player vy +f!
  player vy f@ 0.01e f* player pos-y +f!
;

: main-loop ( -- ) 
  begin                       
    begin
      pending-events?
    while
      handle-event
    repeat
    integrate
    render
    handle-keyboard
    should-quit?
  until
;

: graceful-quit ( -- )
  texture sdl-destroy-texture
  renderer sdl-destroy-renderer
  window sdl-destroy-window
  sdl-quit
;

init-sdl 
create-window to window
create-renderer to renderer
load-texture to texture
set-background-color
init-player
main-loop
graceful-quit
bye
