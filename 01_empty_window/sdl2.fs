\ Most of this stuff is based on Timothy Trussell's gforth sdl opengl lessons:
\ ftp://ftp.taygeta.com/pub/Forth/Archive/tutorials/gforth-sdl-opengl/

c-library sdl2_lib
s" SDL2" add-lib

\c #include <SDL2/SDL.h>

c-function sdl-create-window    SDL_CreateWindow  s n n n n u -- a
c-function sdl-init             SDL_Init          u -- n
c-function sdl-quit		          SDL_Quit		      -- void
c-function sdl-geterror         SDL_GetError      -- s

c-function sdl-poll-event       SDL_PollEvent     a -- n
c-function sdl-wait-event       SDL_WaitEvent     a -- n

end-c-library

1 1 2constant byte%
4 4 2constant int%

256 constant SDL_QUIT

$00000020	constant SDL_INIT_VIDEO
$1FFF0000 constant SDL_WINDOWPOS_UNDEFINED


struct
  int% field sdl-event-type
  1 0   field sdl-event-payload  \ 1st field in each STRUCT maps here
end-struct sdl-event-type%

sdl-event-type%
  1 0 field sdl-quit-event-type
end-struct sdl-quit-event%

\ The SDL_Event structure is arranged as a UNION.
\ This means that each of the structures is accessed at the same
\ event address. Therefore, we only have to allocate memory for the
\ largest struct size we will need to access.
\
\ All of the data can now be referenced by using the address of the
\ event struct in gforth memory, followed by the field name of the
\ specific event being processed.
\
\ All of the -type fields in the structs are mapped to the address of
\ the sdl-event-type field in the sdl-event% struct.

sdl-event-type%
  1 31 field sdl-event-data      \ actually 8 bytes, but I padded it
end-struct sdl-event%
