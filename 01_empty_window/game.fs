\ Most of this stuff is based on Timothy Trussell's gforth sdl opengl lessons:
\ ftp://ftp.taygeta.com/pub/Forth/Archive/tutorials/gforth-sdl-opengl/

\ ---[ Marker ]------------------------------------------------------
\ Allows for easy removal of a previous loading of the program, for
\ re-compilation of changes.
\ If ---marker--- exists, execute it to restore the dictionary.

[IFDEF] ---marker---
  ---marker---
[ENDIF]

\ ---[ New Marker ]--------------------------------------------------
\ Set a marker point in the dictionary.
\ --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- ---
\ If/when the program is re-loaded, everything in the dictionary
\ after this point will be unlinked (removed). Essentially 'forget'.
\ Does NOT affect linked libcc code, however.
\ --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- ---
\ Some programmers prefer to exit/re-enter gforth to ensure that they
\ are starting with a clean slate each time.  Your choice.
\ --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- --- ---

marker ---marker---

\ ---[ Set Number Base ]---------------------------------------------
\ Set the numeric system to base 10

decimal

cr cr .( Loading Game...) cr

\ ---[ opengl-exit-flag ]--------------------------------------------
\ Boolean flag set by HandleKeyPress if the ESC key is pressed.
\ Will be used in a begin..while..repeat loop in the main function.

FALSE value opengl-exit-flag

\ ---[ screen ]------------------------------------------------------
\ Pointer for working SDL surface

0 value screen

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
  to screen                     \ success! save new pointer
;


\ Display keyboard/mouse help information
: help-msg ( -- )
  page
  ." Click the window close button to exit the game"
;

: game ( -- )
  help-msg
  init-sdl
  create-sdl-window

  0 
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
    repeat                    \ until no more events are in the queue
  repeat                      \ until opengl-exit-flag is set to TRUE

  sdl-quit
;

cr .( type "game" to execute) cr
