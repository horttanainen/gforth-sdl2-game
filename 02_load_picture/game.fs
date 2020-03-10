\ Most of this stuff is based on Timothy Trussell's gforth sdl opengl lessons:
\ ftp://ftp.taygeta.com/pub/Forth/Archive/tutorials/gforth-sdl-opengl/

\ ---[ Set Number Base ]---------------------------------------------
\ Set the numeric system to base 10

decimal

cr cr .( Loading Game...) cr

\ ---[ opengl-exit-flag ]--------------------------------------------
\ Boolean flag set by HandleKeyPress if the ESC key is pressed.

FALSE value opengl-exit-flag

\ ---[ window ]------------------------------------------------------
\ Pointer for working SDL surface

0 value window
0 value renderer
0 value texture

\ ---[ Screen Dimensions ]-------------------------------------------
\ These specify the size/depth of the SDL display surface

640 constant screen-width
480 constant screen-height
32  constant screen-bpp

\ ---[ SF, ]---------------------------------------------------------
\ Allocate and store a short float - 4 bytes - to the dictionary.
\ Suggested by Anton Ertl 06/03/2010 - Thanks Anton!
\ --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- ---
\ <here>      returns the address of the next free dictionary byte
\ <1 sfloats> calculates the size of an sfloat variable - 4 bytes
\ <allot>     allocates space at the next free dictionary address
\ <SF!>       stores the floating point value at the address <here>,
\             which is already on the stack.
\ --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- ---

: SF, ( r -- ) here 1 sfloats allot SF! ;

\ ===[ Load Graphics Framework ]=====================================
\ This loads the sdl2.fs file, which contains the sdl2 libcc interface
\ --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- ---

require sdl2.fs


\ Create an event structure for accessing the SDL Event subsystems
create event here sdl-event% nip dup allot 0 fill

\ Initialize the SDL Video subsystem
: init-sdl ( -- )
  SDL_INIT_VIDEO sdl-init 0< if
    cr ." Video Initialization failed: "
    sdl-geterror type cr
    bye
  then
;

: create-sdl-window ( -- )
  s" empty-window" SDL_WINDOWPOS_UNDEFINED SDL_WINDOWPOS_UNDEFINED 640 480 0 sdl-create-window
  dup 0= if                   \ error out if not successful
    drop
    sdl-quit
    cr ." Could not create window: "
    sdl-geterror type cr
    bye
  then
  to window                     \ success! save new pointer
;

: create-sdl-renderer ( -- )
  window -1 0 sdl-create-renderer
  dup 0= if                   \ error out if not successful
    drop
    window sdl-destroy-window
    sdl-quit
    cr ." Could not create renderer: "
    sdl-geterror type cr
    bye
  then
  to renderer                     \ success! save new pointer
;

: load-textures ( -- )
  renderer 
  s" man.bmp" sdl-load-bmp 
  tuck
  sdl-create-texture-from-surface
  swap
  sdl-free-surface
  dup 0= if                   \ error out if not successful
    drop
    renderer sdl-destroy-renderer
    window sdl-destroy-window
    sdl-quit
    cr ." Could not create texture: "
    sdl-geterror type cr
    bye
  then
  to texture                     \ success! save new pointer
;

\ Display keyboard/mouse help information
: help-msg ( -- )
  page
  ." Click the window close button to exit the game"
;

sdl-rect% %allot constant dstrect


: game ( -- )
  help-msg
  init-sdl
  create-sdl-window
  create-sdl-renderer
  load-textures
  renderer 255 255 255 255 sdl-set-render-draw-color

  288 dstrect x !
  208 dstrect y !
  64 dstrect w !
  64 dstrect h !

  begin                                             \ wait for events 
    opengl-exit-flag 0=             \ repeat until this flag set TRUE
  while
    begin
      event sdl-poll-event             \ are there any pending events?
    while
      event sdl-event-type ul@               \ yes, process the events
      case
        SDL_QUIT of     \ window close box clicked, or ALT-F4 pressed
          TRUE to opengl-exit-flag
        endof
      endcase
    renderer sdl-render-clear
    renderer texture 0 dstrect sdl-render-copy
    renderer sdl-render-present
    repeat                    \ until no more events are in the queue
  repeat                      \ until opengl-exit-flag is set to TRUE

  texture sdl-destroy-texture
  renderer sdl-destroy-renderer
  window sdl-destroy-window
  sdl-quit
;

cr .( type "game" to execute) cr
