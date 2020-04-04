decimal
require sdl2.fs

struct
  cell% field pos-x
  cell% field pos-y
end-struct player%

player% %allot constant player

: init-player ( -- )
  288 player pos-x !
  208 player pos-y !
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
  player pos-x @ 'player-rect x !
  player pos-y @ 'player-rect y !
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

: main-loop ( -- ) 
  begin                       
    begin
      pending-events?
    while
      handle-event
    repeat
    render
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
