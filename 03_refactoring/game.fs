decimal
require sdl2.fs

struct
  cell% field pos-x
  cell% field pos-y
end-struct player%

struct
  cell% field exit-flag
  player% field player
  sdl-event% field event
end-struct game-state%

struct
  cell% field window
  cell% field renderer
  cell% field texture
end-struct graphics%

\ Initialize the SDL Video subsystem
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

: create-renderer { graphics -- renderer }
  graphics window @ -1 0 sdl-create-renderer
  dup 0= if                   \ error out if not successful
    drop
    cr ." Could not create renderer: "
    sdl-geterror type cr
    graphics window sdl-destroy-window
    sdl-quit
    bye
  then
;

: load-texture { graphics -- texture }
  graphics renderer @
  s" man.bmp" sdl-load-bmp 
  tuck
  sdl-create-texture-from-surface
  swap
  sdl-free-surface
  dup 0= if                   \ error out if not successful
    drop
    cr ." Could not create texture: "
    sdl-geterror type cr
    graphics renderer @ sdl-destroy-renderer
    graphics window @ sdl-destroy-window
    sdl-quit
    bye
  then
;

: init-game-state ( -- game-state )
  game-state% %allot
  dup false swap exit-flag ! ( game-state --)
  dup  ( game-state game-state )
  player% %allot ( game-state game-state player )
  dup 288 swap pos-x ! ( game-state game-state player )
  dup 208 swap pos-y ! ( game-state game-state player )
  swap player ! ( game-state )
  dup ( game-state game-state -- )
  sdl-event% %allot ( game-state game-state event -- )
  swap event ! ( game-state )
;

: init-graphics ( -- graphics )
  graphics% %allot ( addr -- )
  dup create-window swap window ! ( addr -- )
  dup dup create-renderer swap renderer ! ( addr -- )
  dup dup load-texture swap texture ! ( addr -- )
;

: handle-event ( game-state -- game-state )
  dup event @ sdl-event-type ul@
  case
    SDL_QUIT of     \ window close box clicked, or ALT-F4 pressed
      dup true swap exit-flag !
    endof
  endcase
;

: create-player-rect { game-state -- sdl-rect }
  sdl-rect% %allot 
  dup game-state player @ pos-x @ swap x !
  dup game-state player @ pos-y @ swap y !
  dup 64 swap w !
  dup 64 swap h !
;

: draw-player { graphics game-state -- }
  graphics renderer @
  graphics texture @
  0
  game-state create-player-rect
  sdl-render-copy drop
;

: render { graphics game-state -- graphics game-state }
  graphics renderer @ sdl-render-clear drop
  graphics game-state draw-player 
  graphics renderer @ sdl-render-present
  graphics
  game-state
;

: set-background-color { graphics game-state -- graphics game-state }
  graphics renderer @ 255 255 255 255 sdl-set-render-draw-color drop
  graphics game-state
;

: exit-pressed? ( game-state -- game-state flag )
  dup exit-flag @ 0<>
;

: new-events ( game-state -- game-state)
  dup event @ sdl-poll-event
;

: main-loop ( graphics game-state -- graphics )
  begin                       
    begin
      new-events
    while
      handle-event
      render
    repeat
  exit-pressed?
  until
;

: graceful-quit ( graphics game-state -- )
  drop
  dup texture @ sdl-destroy-texture
  dup renderer @ sdl-destroy-renderer
  window @ sdl-destroy-window
  sdl-quit
;

init-sdl 
init-graphics
init-game-state
set-background-color
main-loop
graceful-quit
bye
